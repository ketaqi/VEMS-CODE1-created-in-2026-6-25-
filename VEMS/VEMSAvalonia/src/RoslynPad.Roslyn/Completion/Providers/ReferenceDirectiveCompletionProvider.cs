using System.Composition;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;

namespace RoslynPad.Roslyn.Completion.Providers;

/// <summary>
/// 为C#引用指令（#r）提供自动补全的提供程序，支持NuGet包补全
/// </summary>
[ExportCompletionProvider("ReferenceDirectiveCompletionProvider", LanguageNames.CSharp)]
[method: ImportingConstructor]
internal class ReferenceDirectiveCompletionProvider([Import(AllowDefault = true)] INuGetCompletionProvider nuGetCompletionProvider)
    : AbstractReferenceDirectiveCompletionProvider
{
    /// <summary>
    /// 补全项规则：禁用过滤字符、提交字符，禁用回车提交，软选择行为
    /// </summary>
    private static readonly CompletionItemRules s_rules = CompletionItemRules.Create(
        filterCharacterRules: [],
        commitCharacterRules: [],
        enterKeyRule: EnterKeyRule.Never,
        selectionBehavior: CompletionItemSelectionBehavior.SoftSelection);

    /// <summary>
    /// NuGet包补全提供程序实例
    /// </summary>
    private readonly INuGetCompletionProvider _nuGetCompletionProvider = nuGetCompletionProvider;

    /// <summary>
    /// 创建NuGet根补全项
    /// </summary>
    /// <returns>NuGet根补全项</returns>
    private CompletionItem CreateNuGetRoot()
        => CommonCompletionItem.Create(
            displayText: ReferenceDirectiveHelper.NuGetPrefix,
            displayTextSuffix: "",
            rules: s_rules,
            glyph: Microsoft.CodeAnalysis.Glyph.NuGet,
            sortText: "");

    /// <summary>
    /// 提供引用指令的补全项
    /// </summary>
    /// <param name="context">补全上下文</param>
    /// <param name="pathThroughLastSlash">最后一个斜杠前的路径字符串</param>
    /// <returns>异步任务</returns>
    protected override Task ProvideCompletionsAsync(CompletionContext context, string pathThroughLastSlash)
    {
        // 处理NuGet包补全逻辑
        if (_nuGetCompletionProvider != null &&
            pathThroughLastSlash.StartsWith(ReferenceDirectiveHelper.NuGetPrefix, StringComparison.InvariantCultureIgnoreCase))
        {
            return ProvideNuGetCompletionsAsync(context, pathThroughLastSlash);
        }

        // 空路径时添加NuGet根补全项
        if (string.IsNullOrEmpty(pathThroughLastSlash))
        {
            context.AddItem(CreateNuGetRoot());
        }

        return base.ProvideCompletionsAsync(context, pathThroughLastSlash);
    }

    /// <summary>
    /// 提供NuGet包相关的补全项
    /// </summary>
    /// <param name="context">补全上下文</param>
    /// <param name="packageIdAndVersion">包含包ID和版本的字符串</param>
    /// <returns>异步任务</returns>
    private async Task ProvideNuGetCompletionsAsync(CompletionContext context, string packageIdAndVersion)
    {
        // 解析NuGet引用的包ID和版本
        var (id, version) = ReferenceDirectiveHelper.ParseNuGetReference(packageIdAndVersion);
        // 搜索NuGet包（异步执行）
        var packages = await Task.Run(
            () => _nuGetCompletionProvider.SearchPackagesAsync(id, exactMatch: version != null, context.CancellationToken),
            context.CancellationToken).ConfigureAwait(false);

        // 版本不为空时补全版本列表
        if (version != null)
        {
            if (packages.Count > 0)
            {
                var package = packages[0];
                var versions = package.Versions;
                // 过滤匹配前缀的版本
                if (!string.IsNullOrWhiteSpace(version))
                {
                    versions = versions.Where(v => v.StartsWith(version, StringComparison.InvariantCultureIgnoreCase));
                }

                // 添加版本补全项
                context.AddItems(versions.Select((v, i) =>
                    CommonCompletionItem.Create(
                        v,
                        "",
                        s_rules,
                        Microsoft.CodeAnalysis.Glyph.NuGet,
                        sortText: i.ToString("0000", CultureInfo.InvariantCulture))));
            }
        }
        else
        {
            // 版本为空时补全包ID项
            context.AddItems(packages.Select((p, i) =>
                CommonCompletionItem.Create(
                    $"{ReferenceDirectiveHelper.NuGetPrefix} {p.Id}, ",
                     "",
                    s_rules,
                    Microsoft.CodeAnalysis.Glyph.NuGet,
                    sortText: i.ToString("0000", CultureInfo.InvariantCulture))));
        }
    }

    /// <summary>
    /// 尝试获取引用指令中的字符串字面量令牌
    /// </summary>
    /// <param name="tree">语法树</param>
    /// <param name="position">位置偏移量</param>
    /// <param name="stringLiteral">输出的字符串字面量令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功获取令牌</returns>
    protected override bool TryGetStringLiteralToken(SyntaxTree tree, int position, out SyntaxToken stringLiteral, CancellationToken cancellationToken) =>
        tree.TryGetStringLiteralToken(position, SyntaxKind.ReferenceDirectiveTrivia, out stringLiteral, cancellationToken);
}

/// <summary>
/// NuGet包补全提供程序接口
/// </summary>
public interface INuGetCompletionProvider
{
    /// <summary>
    /// 搜索NuGet包
    /// </summary>
    /// <param name="searchString">搜索字符串</param>
    /// <param name="exactMatch">是否精确匹配</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>NuGet包列表</returns>
    Task<IReadOnlyList<INuGetPackage>> SearchPackagesAsync(string searchString, bool exactMatch, CancellationToken cancellationToken);
}

/// <summary>
/// NuGet包信息接口
/// </summary>
public interface INuGetPackage
{
    /// <summary>
    /// 包ID
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 包版本列表
    /// </summary>
    IEnumerable<string> Versions { get; }
}
