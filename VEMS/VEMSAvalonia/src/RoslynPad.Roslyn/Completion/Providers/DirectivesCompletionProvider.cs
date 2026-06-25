using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Shared.Extensions.ContextQuery;

namespace RoslynPad.Roslyn.Completion.Providers;

/// <summary>
/// C# 预处理指令补全提供程序，负责提供如 #r 等指令的补全支持
/// </summary>
[ExportCompletionProvider("DirectivesCompletionProvider", LanguageNames.CSharp)]
internal class DirectivesCompletionProvider : CompletionProvider
{
    /// <summary>
    /// 支持补全的指令名称集合
    /// </summary>
    private static readonly ImmutableArray<string> s_directivesName = ["r"];

    /// <summary>
    /// 提供预处理指令的补全项
    /// </summary>
    /// <param name="context">补全上下文</param>
    /// <returns>异步任务</returns>
    public override async Task ProvideCompletionsAsync(CompletionContext context)
    {
        var originatingDocument = context.Document;

        // 仅处理常规源代码类型的文档
        if (originatingDocument.SourceCodeKind != SourceCodeKind.Regular)
        {
            return;
        }

        var cancellationToken = context.CancellationToken;
        var position = context.Position;

        // 复用现有推测语义模型
        var semanticModel = await originatingDocument.ReuseExistingSpeculativeModelAsync(position, cancellationToken).ConfigureAwait(false);
        var service = originatingDocument.GetRequiredLanguageService<ISyntaxContextService>();
        var syntaxContext = service.CreateContext(originatingDocument, semanticModel, position, cancellationToken);

        // 仅在预处理表达式上下文中提供补全
        if (!syntaxContext.IsPreProcessorExpressionContext)
        {
            return;
        }

        // 为每个支持的指令添加补全项
        foreach (var name in s_directivesName)
        {
            context.AddItem(CommonCompletionItem.Create(
            name,
            displayTextSuffix: "",
            CompletionItemRules.Default,
            glyph: Microsoft.CodeAnalysis.Glyph.Keyword,
            sortText: "_0_" + name)); // 排序文本确保指令优先显示
        }
    }
}
