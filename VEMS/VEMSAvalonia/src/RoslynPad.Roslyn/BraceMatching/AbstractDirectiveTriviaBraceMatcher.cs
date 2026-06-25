// 处理 #if/#else/#endif 等预处理指令匹配的抽象基类

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RoslynPad.Roslyn.BraceMatching;

internal abstract class AbstractDirectiveTriviaBraceMatcher<
    TDirectiveTriviaSyntax,
    TIfDirectiveTriviaSyntax,
    TElseIfDirectiveTriviaSyntax,
    TElseDirectiveTriviaSyntax,
    TEndIfDirectiveTriviaSyntax> : IBraceMatcher
        where TDirectiveTriviaSyntax : SyntaxNode
        where TIfDirectiveTriviaSyntax : TDirectiveTriviaSyntax
        where TElseIfDirectiveTriviaSyntax : TDirectiveTriviaSyntax
        where TElseDirectiveTriviaSyntax : TDirectiveTriviaSyntax
        where TEndIfDirectiveTriviaSyntax : TDirectiveTriviaSyntax
{
    internal abstract List<TDirectiveTriviaSyntax> GetMatchingConditionalDirectives(TDirectiveTriviaSyntax directive, CancellationToken cancellationToken);
    internal abstract TDirectiveTriviaSyntax? GetMatchingDirective(TDirectiveTriviaSyntax directive, CancellationToken cancellationToken);
    internal abstract TextSpan GetSpanForTagging(TDirectiveTriviaSyntax directive);

    /// <summary>
    /// 查找预处理指令的匹配项
    /// </summary>
    public async Task<BraceMatchingResult?> FindBracesAsync(Document document, int position, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var token = root!.FindToken(position, findInsideTrivia: true);

        // 不是预处理指令则返回
        if (token.Parent is not TDirectiveTriviaSyntax directive)
        {
            return null;
        }

        TDirectiveTriviaSyntax? matchingDirective = null;

        // 处理条件编译指令组（if/elif/else/endif）
        if (IsConditionalDirective(directive))
        {
            var matchingDirectives = GetMatchingConditionalDirectives(directive, cancellationToken);
            if (matchingDirectives?.Count > 0)
            {
                matchingDirective = matchingDirectives[(matchingDirectives.IndexOf(directive) + 1) % matchingDirectives.Count];
            }
        }
        else
        {
            // 普通 #region/#endregion
            matchingDirective = GetMatchingDirective(directive, cancellationToken);
        }

        if (matchingDirective == null)
        {
            return null;
        }

        return new BraceMatchingResult(
            leftSpan: GetSpanForTagging(directive),
            rightSpan: GetSpanForTagging(matchingDirective));
    }

    /// <summary>
    /// 判断是否属于条件指令
    /// </summary>
    private bool IsConditionalDirective(TDirectiveTriviaSyntax directive)
    {
        return directive is TIfDirectiveTriviaSyntax ||
               directive is TElseIfDirectiveTriviaSyntax ||
               directive is TElseDirectiveTriviaSyntax ||
               directive is TEndIfDirectiveTriviaSyntax;
    }
}
