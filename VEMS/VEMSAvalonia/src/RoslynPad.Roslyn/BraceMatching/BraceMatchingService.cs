// 括号匹配服务入口，负责根据文档语言选择对应的匹配器并执行匹配

using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host.Mef;

namespace RoslynPad.Roslyn.BraceMatching;

[Export(typeof(IBraceMatchingService))]
[method: ImportingConstructor]
internal class BraceMatchingService(
    [ImportMany] IEnumerable<Lazy<IBraceMatcher, LanguageMetadata>> braceMatchers) : IBraceMatchingService
{
    // 所有可用括号匹配器集合
    private readonly ImmutableArray<Lazy<IBraceMatcher, LanguageMetadata>> _braceMatchers = braceMatchers.ToImmutableArray();

    /// <summary>
    /// 根据文档语言调用相应匹配器，返回匹配结果
    /// </summary>
    public async Task<BraceMatchingResult?> GetMatchingBracesAsync(Document document, int position, CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        // 位置越界则抛异常
        if (position < 0 || position > text.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        // 只选择匹配当前语言的匹配器
        var matchers = _braceMatchers.Where(b => b.Metadata.Language == document.Project.Language);

        foreach (var matcher in matchers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var braces = await matcher.Value.FindBracesAsync(document, position, cancellationToken).ConfigureAwait(false);
            if (braces.HasValue)
            {
                return braces;
            }
        }

        return null;
    }
}
