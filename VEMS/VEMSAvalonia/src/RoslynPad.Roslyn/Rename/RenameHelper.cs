using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Extensions.ContextQuery;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace RoslynPad.Roslyn.Rename;

/// <summary>
/// 重命名辅助工具类，提供获取可重命名符号的核心方法
/// 用于在RoslynPad中判断指定位置/语法令牌对应的符号是否支持重命名操作
/// </summary>
public static class RenameHelper
{
    /// <summary>
    /// 根据文档和位置获取可重命名的符号
    /// </summary>
    /// <param name="document">当前文档实例</param>
    /// <param name="position">源代码中的字符位置</param>
    /// <param name="cancellationToken">取消令牌，用于取消异步操作</param>
    /// <returns>可重命名的符号；若不支持重命名则返回null</returns>
    public static async Task<ISymbol?> GetRenameSymbol(
        Document document, int position, CancellationToken cancellationToken = default)
    {
        // 获取指定位置触达的语法单词令牌
        var token = await document.GetTouchingWordAsync(position, cancellationToken).ConfigureAwait(false);
        return token != default
                ? await GetRenameSymbol(document, token, cancellationToken).ConfigureAwait(false)
                : null;
    }

    /// <summary>
    /// 根据文档和语法令牌获取可重命名的符号
    /// 核心逻辑：校验令牌合法性 → 解析语义模型 → 过滤不支持重命名的符号类型 → 校验符号位置合法性
    /// </summary>
    /// <param name="document">当前文档实例</param>
    /// <param name="triggerToken">触发重命名的语法令牌</param>
    /// <param name="cancellationToken">取消令牌，用于取消异步操作</param>
    /// <returns>可重命名的符号；若不支持重命名则返回null</returns>
    public static async Task<ISymbol?> GetRenameSymbol(
        Document document, SyntaxToken triggerToken, CancellationToken cancellationToken)
    {
        // 获取语法事实服务，用于校验关键字类型
        var syntaxFactsService = document.Project.Services.GetRequiredService<ISyntaxFactsService>();
        if (syntaxFactsService.IsReservedOrContextualKeyword(triggerToken))
        {
            // 保留关键字/上下文关键字不支持重命名
            return null;
        }

        // 获取文档的语义模型（语义分析核心对象）
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
        {
            // 语义模型为空时无法解析符号，直接返回null
            return null;
        }

        // 获取语义事实服务，用于符号重命名信息分析
        var semanticFacts = document.GetLanguageService<ISemanticFactsService>();

        // 获取令牌对应的重命名信息（包含候选符号、是否为成员组等）
        var tokenRenameInfo = RenameUtilities.GetTokenRenameInfo(semanticFacts, semanticModel, triggerToken, cancellationToken);

        // 处理nameof表达式中的成员组引用：取第一个候选符号，强制启用重载重命名（此处仅读取符号，逻辑在外部处理）
        var triggerSymbol = tokenRenameInfo.HasSymbols ? tokenRenameInfo.Symbols.First() : null;
        if (triggerSymbol == null)
        {
            // 无候选符号时返回null
            return null;
        }

        // 临时禁用元组字段重命名（参考Roslyn官方问题：https://github.com/dotnet/roslyn/issues/10898）
        // 原因：1) 编译器返回的元组字段位置信息不正确 2) 元组字段重命名需复杂设计
        if (triggerSymbol.ContainingType?.IsTupleType == true)
        {
            return null;
        }

        // 尝试获取可重命名的符号（Roslyn内置工具方法）
        var symbol = await RenameUtilities.TryGetRenamableSymbolAsync(document, triggerToken.SpanStart, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (symbol == null)
        {
            return null;
        }

        // 过滤不支持的符号类型：外部别名、dynamic类型
        if (symbol.Kind == SymbolKind.Alias && symbol.IsExtern ||
            triggerToken.IsTypeNamedDynamic() && symbol.Kind == SymbolKind.DynamicType)
        {
            return null;
        }

        // 过滤隐式声明的符号（仅允许事件处理程序的隐式局部变量/参数）
        // 事件处理程序判定条件：参数所属方法为委托类型，且关联到事件符号
        if (symbol.IsImplicitlyDeclared &&
            symbol.Kind != SymbolKind.Local &&
            !(symbol.Kind == SymbolKind.Parameter &&
              symbol.ContainingSymbol.Kind == SymbolKind.Method &&
              symbol.ContainingType != null &&
              symbol.ContainingType.IsDelegateType() &&
              symbol.ContainingType.AssociatedSymbol != null))
        {
            // 注：若事件基于委托类型声明，无法关联委托类型与事件，因此该场景下重命名被阻止
            return null;
        }

        // 匿名类型的属性不支持重命名
        if (symbol.Kind == SymbolKind.Property && symbol.ContainingType.IsAnonymousType)
        {
            return null;
        }

        // 错误类型符号不支持重命名
        if (symbol.IsErrorType())
        {
            return null;
        }

        // 用户定义的运算符方法不支持重命名
        if (symbol.Kind == SymbolKind.Method && ((IMethodSymbol)symbol).MethodKind == MethodKind.UserDefinedOperator)
        {
            return null;
        }

        var symbolLocations = symbol.Locations;

        // 校验符号所在位置是否可修改：遍历所有符号位置，过滤元数据/不可修改的源码位置
        foreach (var location in symbolLocations)
        {
            // 元数据中的符号（如程序集引用）不可重命名
            if (location.IsInMetadata)
            {
                return null;
            }

            if (location.IsInSource)
            {
                // 处理提交型项目（Submission）：若符号所在项目被其他提交项目引用，则不可重命名
                if (document.Project.IsSubmission)
                {
                    var solution = document.Project.Solution;
                    var projectIdOfLocation = solution.GetDocument(location.SourceTree)?.Project.Id;

                    if (solution.Projects.Any(p => p.IsSubmission && p.ProjectReferences.Any(r => r.ProjectId == projectIdOfLocation)))
                    {
                        return null;
                    }
                }
            }
            else
            {
                // 非源码/元数据的未知位置，直接返回null
                return null;
            }
        }

        // 所有校验通过，返回可重命名的符号
        return symbol;
    }
}
