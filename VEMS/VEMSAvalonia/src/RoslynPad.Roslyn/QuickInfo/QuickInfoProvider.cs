using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.DocumentationComments;
using Microsoft.CodeAnalysis.ExternalAccess.Pythia.Api;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Roslyn.Utilities;

namespace RoslynPad.Roslyn.QuickInfo;

/// <summary>
/// 快速信息提供器实现类，负责解析文档中指定位置的语法/语义信息并构建快速信息内容
/// </summary>
[Export(typeof(IQuickInfoProvider)), Shared]
[method: ImportingConstructor]
internal sealed class QuickInfoProvider(IDeferredQuickInfoContentProvider contentProvider) : IQuickInfoProvider
{
    /// <summary>
    /// 延迟加载的快速信息内容提供器
    /// </summary>
    private readonly IDeferredQuickInfoContentProvider _contentProvider = contentProvider;

    /// <inheritdoc/>
    public async Task<QuickInfoItem?> GetItemAsync(
        Document document,
        int position,
        CancellationToken cancellationToken)
    {
        var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
        if (tree == null)
        {
            return null;
        }

        // 获取包含指定位置的语法令牌（包含琐事）
        var token = await tree.GetTouchingTokenAsync(position, cancellationToken, findInsideTrivia: true).ConfigureAwait(false);

        // 尝试从当前令牌获取快速信息
        var state = await GetQuickInfoItemAsync(document, token, position, cancellationToken).ConfigureAwait(false);
        if (state != null)
        {
            return state;
        }

        // 当前令牌无有效信息时，尝试前一个令牌
        if (ShouldCheckPreviousToken(token))
        {
            var previousToken = token.GetPreviousToken();
            if ((state = await GetQuickInfoItemAsync(document, previousToken, position, cancellationToken).ConfigureAwait(false)) != null)
            {
                return state;
            }
        }

        return null;
    }

    /// <summary>
    /// 判断是否需要检查前一个语法令牌
    /// </summary>
    /// <param name="token">当前语法令牌</param>
    /// <returns>true表示需要检查，false则否</returns>
    private static bool ShouldCheckPreviousToken(SyntaxToken token)
    {
        // 排除XML Cref属性场景
        return !token.Parent.IsKind(SyntaxKind.XmlCrefAttribute);
    }

    /// <summary>
    /// 从指定语法令牌异步获取快速信息项
    /// </summary>
    /// <param name="document">目标文档</param>
    /// <param name="token">语法令牌</param>
    /// <param name="position">目标位置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>快速信息项，无有效信息则返回null</returns>
    private async Task<QuickInfoItem?> GetQuickInfoItemAsync(
        Document document,
        SyntaxToken token,
        int position,
        CancellationToken cancellationToken)
    {
        if (token != default && token.Span.IntersectsWith(position))
        {
            // 构建快速信息内容
            var deferredContent = await BuildContentAsync(document, token, cancellationToken).ConfigureAwait(false);
            if (deferredContent != null)
            {
                // 创建快速信息项
                return new QuickInfoItem(token.Span, deferredContent.Create);
            }
        }

        return null;
    }

    /// <summary>
    /// 异步构建快速信息延迟加载内容
    /// </summary>
    /// <param name="document">目标文档</param>
    /// <param name="token">语法令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>延迟加载的快速信息内容，无则返回null</returns>
    private async Task<IDeferredQuickInfoContent?> BuildContentAsync(
        Document document,
        SyntaxToken token,
        CancellationToken cancellationToken)
    {
        var linkedDocumentIds = document.GetLinkedDocumentIds();
        var modelAndSymbols = await BindTokenAsync(document, token, cancellationToken).ConfigureAwait(false);

        // 无符号信息且无链接文档时直接返回null
        if ((modelAndSymbols.Item2 == null || modelAndSymbols.Item2.Count == 0) && !linkedDocumentIds.Any())
        {
            return null;
        }

        // 无链接文档时直接创建内容
        if (!linkedDocumentIds.Any())
        {
            return await CreateContentAsync(document.Project.Solution.Workspace,
                token,
                modelAndSymbols.Item1,
                modelAndSymbols.Item2!,
                supportedPlatforms: null,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        // 处理链接文件/共享项目场景：找到最佳绑定结果（无错误的绑定）
        var candidateProjects = ImmutableArray.CreateBuilder<ProjectId>();
        candidateProjects.Add(document.Project.Id);

        var invalidProjects = ImmutableArray.CreateBuilder<ProjectId>();

        var candidateResults = new List<Tuple<DocumentId, SemanticModel, IList<ISymbol>>>
        {
            Tuple.Create(document.Id, modelAndSymbols.Item1, modelAndSymbols.Item2!)
        };

        // 遍历所有链接文档，收集候选绑定结果
        foreach (var link in linkedDocumentIds)
        {
            var linkedDocument = document.Project.Solution.GetDocument(link);
            var linkedToken = await FindTokenInLinkedDocument(token, linkedDocument!, cancellationToken).ConfigureAwait(false);

            if (linkedToken != default)
            {
                candidateProjects.Add(link.ProjectId);
                var linkedModelAndSymbols = await BindTokenAsync(linkedDocument!, linkedToken, cancellationToken).ConfigureAwait(false);
                candidateResults.Add(Tuple.Create(link, linkedModelAndSymbols.Item1, linkedModelAndSymbols.Item2));
            }
        }

        // 优先选择无错误的第一个绑定结果
        var bestBinding = candidateResults.FirstOrDefault(c => c.Item3.Count > 0 && !ErrorVisitor.ContainsError(c.Item3.First()));
        // 无有效结果时选择当前文件的第一个候选
        bestBinding ??= candidateResults.First();

        if (bestBinding.Item3 == null || !bestBinding.Item3.Any())
        {
            return null;
        }

        // 筛选无效项目（无等效符号的项目）
        candidateResults.Remove(bestBinding);
        foreach (var candidate in candidateResults)
        {
            if (!candidate.Item3.Intersect(bestBinding.Item3, SymbolEqualityComparer.Default).Any())
            {
                invalidProjects.Add(candidate.Item1.ProjectId);
            }
        }

        // 构建平台支持数据并创建最终内容
        var supportedPlatforms = new SupportedPlatformData(document.Project.Solution, invalidProjects.ToImmutable(), candidateProjects.ToImmutable());
        return await CreateContentAsync(document.Project.Solution.Workspace, token, bestBinding.Item2, bestBinding.Item3, supportedPlatforms, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 在链接文档中查找与指定令牌匹配的语法令牌
    /// </summary>
    /// <param name="token">源语法令牌</param>
    /// <param name="linkedDocument">链接文档</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>匹配的语法令牌，无则返回默认值</returns>
    private static async Task<SyntaxToken> FindTokenInLinkedDocument(SyntaxToken token, Document linkedDocument, CancellationToken cancellationToken)
    {
        if (!linkedDocument.SupportsSyntaxTree)
        {
            return default;
        }

        var root = await linkedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        // 不搜索琐事（忽略非活动区域）
        var linkedToken = root!.FindToken(token.SpanStart);

        // 验证令牌范围是否匹配
        if (token.Span == linkedToken.Span)
        {
            return linkedToken;
        }

        return default;
    }

    /// <summary>
    /// 异步创建快速信息延迟加载内容
    /// </summary>
    /// <param name="workspace">工作区</param>
    /// <param name="token">语法令牌</param>
    /// <param name="semanticModel">语义模型</param>
    /// <param name="symbols">符号列表</param>
    /// <param name="supportedPlatforms">平台支持数据</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>延迟加载的快速信息内容</returns>
    private async Task<IDeferredQuickInfoContent> CreateContentAsync(
        Workspace workspace,
        SyntaxToken token,
        SemanticModel semanticModel,
        IEnumerable<ISymbol> symbols,
        SupportedPlatformData? supportedPlatforms,
        CancellationToken cancellationToken)
    {
        // 获取符号显示服务
        var descriptionService = workspace.Services.GetLanguageServices(token.Language).GetRequiredService<ISymbolDisplayService>();
        // 将符号转换为描述分组
        var sections = await descriptionService.ToDescriptionGroupsAsync(semanticModel, token.SpanStart, symbols.AsImmutable(), SymbolDescriptionOptions.Default, cancellationToken).ConfigureAwait(false);

        // 构建主描述文本
        var mainDescriptionBuilder = new List<TaggedText>();
        if (sections.TryGetValue(SymbolDescriptionGroups.MainDescription, out var value))
        {
            mainDescriptionBuilder.AddRange(value);
        }

        // 构建类型参数映射文本
        var typeParameterMapBuilder = new List<TaggedText>();
        if (sections.TryGetValue(SymbolDescriptionGroups.TypeParameterMap, out var parts))
        {
            if (!parts.IsDefaultOrEmpty)
            {
                typeParameterMapBuilder.AddLineBreak();
                typeParameterMapBuilder.AddRange(parts);
            }
        }

        // 构建结构类型文本
        var structuralTypesBuilder = new List<TaggedText>();
        if (sections.TryGetValue(SymbolDescriptionGroups.StructuralTypes, out parts))
        {
            if (!parts.IsDefaultOrEmpty)
            {
                structuralTypesBuilder.AddLineBreak();
                structuralTypesBuilder.AddRange(parts);
            }
        }

        // 构建使用说明文本（包含平台支持信息）
        var usageTextBuilder = new List<TaggedText>();
        if (sections.TryGetValue(SymbolDescriptionGroups.AwaitableUsageText, out parts))
        {
            if (!parts.IsDefaultOrEmpty)
            {
                usageTextBuilder.AddRange(parts);
            }
        }
        if (supportedPlatforms != null)
        {
            usageTextBuilder.AddRange(supportedPlatforms.ToDisplayParts().ToTaggedText());
        }

        // 构建异常说明文本
        var exceptionsTextBuilder = new List<TaggedText>();
        if (sections.TryGetValue(SymbolDescriptionGroups.Exceptions, out parts))
        {
            if (!parts.IsDefaultOrEmpty)
            {
                exceptionsTextBuilder.AddRange(parts);
            }
        }

        // 获取文档注释格式化服务和语法事实服务
        var formatter = workspace.Services.GetLanguageServices(semanticModel.Language).GetRequiredService<IDocumentationCommentFormattingService>();
        var syntaxFactsService = workspace.Services.GetLanguageServices(semanticModel.Language).GetRequiredService<ISyntaxFactsService>();

        // 获取文档注释内容
        var documentationContent = GetDocumentationContent(symbols, sections, semanticModel, token, formatter, syntaxFactsService, cancellationToken);

        // 确定是否显示警告图标（存在有效/无效项目时）
        var showWarningGlyph = supportedPlatforms != null && supportedPlatforms.HasValidAndInvalidProjects();
        var showSymbolGlyph = true;

        // 特殊处理await关键字绑定到void类型的场景
        if (syntaxFactsService.IsAwaitKeyword(token) &&
            symbols.First() is INamedTypeSymbol { SpecialType: SpecialType.System_Void })
        {
            documentationContent = _contentProvider.CreateDocumentationCommentDeferredContent(null);
            showSymbolGlyph = false;
        }

        // 创建最终的快速信息显示内容
        return _contentProvider.CreateQuickInfoDisplayDeferredContent(
            symbol: symbols.First(),
            showWarningGlyph: showWarningGlyph,
            showSymbolGlyph: showSymbolGlyph,
            mainDescription: mainDescriptionBuilder,
            documentation: documentationContent,
            typeParameterMap: typeParameterMapBuilder,
            anonymousTypes: structuralTypesBuilder,
            usageText: usageTextBuilder,
            exceptionText: exceptionsTextBuilder);
    }

    /// <summary>
    /// 获取符号的文档注释内容
    /// </summary>
    /// <param name="symbols">符号列表</param>
    /// <param name="sections">符号描述分组</param>
    /// <param name="semanticModel">语义模型</param>
    /// <param name="token">语法令牌</param>
    /// <param name="formatter">文档注释格式化服务</param>
    /// <param name="syntaxFactsService">语法事实服务</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>延迟加载的文档注释内容</returns>
    private IDeferredQuickInfoContent GetDocumentationContent(
        IEnumerable<ISymbol> symbols,
        IDictionary<SymbolDescriptionGroups, ImmutableArray<TaggedText>> sections,
        SemanticModel semanticModel,
        SyntaxToken token,
        IDocumentationCommentFormattingService formatter,
        ISyntaxFactsService syntaxFactsService,
        CancellationToken cancellationToken)
    {
        // 优先使用预解析的文档注释分组
        if (sections.TryGetValue(SymbolDescriptionGroups.Documentation, out var value))
        {
            var documentationBuilder = new List<TaggedText>();
            documentationBuilder.AddRange(value);
            return _contentProvider.CreateClassifiableDeferredContent(documentationBuilder);
        }

        // 从符号原始定义获取文档注释
        if (symbols.Any())
        {
            var symbol = symbols.First().OriginalDefinition;

            // 特殊处理特性：绑定到特性类而非构造函数
            if (token.Parent != null &&
                syntaxFactsService.IsNameOfAttribute(token.Parent) &&
                symbol.ContainingType?.IsAttribute() == true)
            {
                symbol = symbol.ContainingType;
            }

            // 获取格式化后的文档注释部分
            var documentation = symbol.GetDocumentationParts(semanticModel, token.SpanStart, formatter, cancellationToken);
            if (documentation != null)
            {
                return _contentProvider.CreateClassifiableDeferredContent([.. documentation]);
            }
        }

        // 无文档注释时创建空内容
        return _contentProvider.CreateDocumentationCommentDeferredContent(null);
    }

    /// <summary>
    /// 异步绑定语法令牌到语义模型和符号列表
    /// </summary>
    /// <param name="document">目标文档</param>
    /// <param name="token">语法令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>语义模型和符号列表的元组</returns>
    private async Task<ValueTuple<SemanticModel, IList<ISymbol>>> BindTokenAsync(
        Document document,
        SyntaxToken token,
        CancellationToken cancellationToken)
    {
        // 获取令牌父节点的语义模型
        var semanticModel = await document.GetSemanticModelForNodeAsync(token.Parent, cancellationToken).ConfigureAwait(false);
        var enclosingType = semanticModel.GetEnclosingNamedType(token.SpanStart, cancellationToken);

        // 获取令牌的语义符号
        var symbols = semanticModel.GetSemanticInfo(token, document.Project.Solution.Services, cancellationToken).GetSymbols(includeType: true);

        // 尝试获取可绑定的父节点
        var bindableParent = document.GetLanguageService<ISyntaxFactsService>().TryGetBindableParent(token);
        if (bindableParent != null)
        {
            // 获取成员组并筛选有效符号
            var overloads = semanticModel.GetMemberGroup(bindableParent, cancellationToken);
            symbols = symbols.Where(IsOk)
                .Where(s => IsAccessible(s, enclosingType!))
                .Concat(overloads)
                .Distinct(SymbolEqualityComparer.Default)
                .ToImmutableArray();

            if (symbols.Any())
            {
                // 排除Cref类型参数符号
                return new ValueTuple<SemanticModel, IList<ISymbol>>(
                    semanticModel,
                    symbols.First() is ITypeParameterSymbol typeParameter && typeParameter.TypeParameterKind == TypeParameterKind.Cref
                        ? SpecializedCollections.EmptyList<ISymbol>()
                        : [.. symbols]);
            }

            // 处理运算符绑定：尝试绑定到类型
            var syntaxFacts = document.Project.Services.GetRequiredService<ISyntaxFactsService>();
            if (syntaxFacts.IsOperator(token) && token.Parent != null)
            {
                var typeInfo = semanticModel.GetTypeInfo(token.Parent, cancellationToken);
                if (IsOk(typeInfo.Type!))
                {
                    return new ValueTuple<SemanticModel, IList<ISymbol>>(semanticModel, [typeInfo.Type!]);
                }
            }
        }

        // 无有效符号时返回空列表
        return ValueTuple.Create(semanticModel, SpecializedCollections.EmptyList<ISymbol>());
    }

    /// <summary>
    /// 检查符号是否为有效符号（非错误类型、非匿名函数）
    /// </summary>
    /// <param name="symbol">待检查符号</param>
    /// <returns>true表示有效，false则否</returns>
    private static bool IsOk(ISymbol symbol)
    {
        return symbol != null && !symbol.IsErrorType() && !symbol.IsAnonymousFunction();
    }

    /// <summary>
    /// 检查符号在指定类型内是否可访问
    /// </summary>
    /// <param name="symbol">待检查符号</param>
    /// <param name="within">包含类型</param>
    /// <returns>true表示可访问，false则否</returns>
    private static bool IsAccessible(ISymbol symbol, INamedTypeSymbol within)
    {
        return within == null || symbol.IsAccessibleWithin(within);
    }

    /// <summary>
    /// 符号错误访问者，用于检查符号是否包含错误类型
    /// </summary>
    private class ErrorVisitor : SymbolVisitor<bool>
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        private static readonly ErrorVisitor s_instance = new();

        /// <summary>
        /// 检查符号是否包含错误类型
        /// </summary>
        /// <param name="symbol">目标符号</param>
        /// <returns>true表示包含错误，false则否</returns>
        public static bool ContainsError(ISymbol symbol)
        {
            return s_instance.Visit(symbol);
        }

        /// <summary>
        /// 默认访问逻辑：标记为包含错误
        /// </summary>
        /// <param name="symbol">目标符号</param>
        /// <returns>始终返回true</returns>
        public override bool DefaultVisit(ISymbol symbol)
        {
            return true;
        }

        /// <summary>
        /// 访问别名符号：无错误
        /// </summary>
        /// <param name="symbol">别名符号</param>
        /// <returns>始终返回false</returns>
        public override bool VisitAlias(IAliasSymbol symbol)
        {
            return false;
        }

        /// <summary>
        /// 访问数组类型符号：检查元素类型
        /// </summary>
        /// <param name="symbol">数组类型符号</param>
        /// <returns>元素类型是否包含错误</returns>
        public override bool VisitArrayType(IArrayTypeSymbol symbol)
        {
            return Visit(symbol.ElementType);
        }

        /// <summary>
        /// 访问事件符号：检查事件类型
        /// </summary>
        /// <param name="symbol">事件符号</param>
        /// <returns>事件类型是否包含错误</returns>
        public override bool VisitEvent(IEventSymbol symbol)
        {
            return Visit(symbol.Type);
        }

        /// <summary>
        /// 访问字段符号：检查字段类型
        /// </summary>
        /// <param name="symbol">字段符号</param>
        /// <returns>字段类型是否包含错误</returns>
        public override bool VisitField(IFieldSymbol symbol)
        {
            return Visit(symbol.Type);
        }

        /// <summary>
        /// 访问局部变量符号：检查局部变量类型
        /// </summary>
        /// <param name="symbol">局部变量符号</param>
        /// <returns>局部变量类型是否包含错误</returns>
        public override bool VisitLocal(ILocalSymbol symbol)
        {
            return Visit(symbol.Type);
        }

        /// <summary>
        /// 访问方法符号：检查参数和类型参数
        /// </summary>
        /// <param name="symbol">方法符号</param>
        /// <returns>参数/类型参数是否包含错误</returns>
        public override bool VisitMethod(IMethodSymbol symbol)
        {
            foreach (var parameter in symbol.Parameters)
            {
                if (!Visit(parameter))
                {
                    return true;
                }
            }

            foreach (var typeParameter in symbol.TypeParameters)
            {
                if (!Visit(typeParameter))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 访问命名类型符号：检查类型参数/实参及是否为错误类型
        /// </summary>
        /// <param name="symbol">命名类型符号</param>
        /// <returns>是否包含错误类型</returns>
        public override bool VisitNamedType(INamedTypeSymbol symbol)
        {
            foreach (var typeParameter in symbol.TypeArguments.Concat(symbol.TypeParameters))
            {
                if (Visit(typeParameter))
                {
                    return true;
                }
            }

            return symbol.IsErrorType();
        }

        /// <summary>
        /// 访问参数符号：检查参数类型
        /// </summary>
        /// <param name="symbol">参数符号</param>
        /// <returns>参数类型是否包含错误</returns>
        public override bool VisitParameter(IParameterSymbol symbol)
        {
            return Visit(symbol.Type);
        }

        /// <summary>
        /// 访问属性符号：检查属性类型
        /// </summary>
        /// <param name="symbol">属性符号</param>
        /// <returns>属性类型是否包含错误</returns>
        public override bool VisitProperty(IPropertySymbol symbol)
        {
            return Visit(symbol.Type);
        }

        /// <summary>
        /// 访问指针类型符号：检查指向的类型
        /// </summary>
        /// <param name="symbol">指针类型符号</param>
        /// <returns>指向类型是否包含错误</returns>
        public override bool VisitPointerType(IPointerTypeSymbol symbol)
        {
            return Visit(symbol.PointedAtType);
        }
    }
}

/// <summary>
/// 延迟加载的快速信息内容提供器接口，定义创建不同类型快速信息内容的契约
/// </summary>
internal interface IDeferredQuickInfoContentProvider
{
    /// <summary>
    /// 创建快速信息显示的延迟加载内容
    /// </summary>
    /// <param name="symbol">核心符号</param>
    /// <param name="showWarningGlyph">是否显示警告图标</param>
    /// <param name="showSymbolGlyph">是否显示符号图标</param>
    /// <param name="mainDescription">主描述文本</param>
    /// <param name="documentation">文档注释内容</param>
    /// <param name="typeParameterMap">类型参数映射文本</param>
    /// <param name="anonymousTypes">匿名类型文本</param>
    /// <param name="usageText">使用说明文本</param>
    /// <param name="exceptionText">异常说明文本</param>
    /// <returns>延迟加载的快速信息显示内容</returns>
    IDeferredQuickInfoContent CreateQuickInfoDisplayDeferredContent(
        ISymbol symbol,
        bool showWarningGlyph,
        bool showSymbolGlyph,
        IList<TaggedText> mainDescription,
        IDeferredQuickInfoContent documentation,
        IList<TaggedText> typeParameterMap,
        IList<TaggedText> anonymousTypes,
        IList<TaggedText> usageText,
        IList<TaggedText> exceptionText);

    /// <summary>
    /// 创建文档注释的延迟加载内容
    /// </summary>
    /// <param name="documentationComment">文档注释文本</param>
    /// <returns>延迟加载的文档注释内容</returns>
    IDeferredQuickInfoContent CreateDocumentationCommentDeferredContent(string? documentationComment);

    /// <summary>
    /// 创建可分类的延迟加载内容
    /// </summary>
    /// <param name="content">分类文本内容</param>
    /// <returns>延迟加载的可分类内容</returns>
    IDeferredQuickInfoContent CreateClassifiableDeferredContent(IList<TaggedText> content);
}

/// <summary>
/// 延迟加载的快速信息内容接口，定义内容创建方法
/// </summary>
internal interface IDeferredQuickInfoContent
{
    /// <summary>
    /// 创建快速信息内容对象
    /// </summary>
    /// <returns>内容对象</returns>
    object Create();
}
