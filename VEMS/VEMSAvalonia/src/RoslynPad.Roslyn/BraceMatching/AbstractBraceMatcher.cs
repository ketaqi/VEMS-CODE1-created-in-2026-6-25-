// Copyright (c) ...
// 抽象括号匹配器基础类，提供通用括号匹配逻辑。
// 负责根据语法 Token 查找对应的匹配括号。
// 派生类只需要定义具体符号种类与允许规则。

using Microsoft.CodeAnalysis;

namespace RoslynPad.Roslyn.BraceMatching;

internal abstract class AbstractBraceMatcher : IBraceMatcher
{
    // 左括号定义（包含字符与语法 Kind）
    private readonly BraceCharacterAndKind _openBrace;

    // 右括号定义（包含字符与语法 Kind）
    private readonly BraceCharacterAndKind _closeBrace;

    /// <summary>
    /// 构造函数，初始化括号匹配信息
    /// </summary>
    protected AbstractBraceMatcher(
        BraceCharacterAndKind openBrace,
        BraceCharacterAndKind closeBrace)
    {
        _openBrace = openBrace;
        _closeBrace = closeBrace;
    }

    /// <summary>
    /// 尝试根据当前 Token 在父节点中查找与其对应的匹配 Token
    /// </summary>
    private bool TryFindMatchingToken(SyntaxToken token, out SyntaxToken match)
    {
        var parent = token.Parent;
        if (parent == null)
        {
            match = default;
            return false;
        }

        // 查找父节点下的两个括号 Token
        var braceTokens = (from child in parent.ChildNodesAndTokens()
                           where child.IsToken
                           let tok = child.AsToken()
                           where tok.RawKind == _openBrace.Kind || tok.RawKind == _closeBrace.Kind
                           where tok.Span.Length > 0
                           select tok).ToList();

        // 必须刚好两个，且顺序为 open → close
        if (braceTokens.Count == 2 &&
            braceTokens[0].RawKind == _openBrace.Kind &&
            braceTokens[1].RawKind == _closeBrace.Kind)
        {
            if (braceTokens[0] == token)
            {
                match = braceTokens[1];
                return true;
            }
            else if (braceTokens[1] == token)
            {
                match = braceTokens[0];
                return true;
            }
        }

        match = default;
        return false;
    }

    /// <summary>
    /// 查找匹配括号（主入口 API）
    /// </summary>
    public async Task<BraceMatchingResult?> FindBracesAsync(
        Document document,
        int position,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var token = root!.FindToken(position);

        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        // 当前光标字符必须本身是括号
        if (position < text.Length && IsBrace(text[position]))
        {
            // 处理左括号
            if (token.RawKind == _openBrace.Kind && AllowedForToken(token))
            {
                var leftToken = token;
                if (TryFindMatchingToken(leftToken, out var rightToken))
                {
                    return new BraceMatchingResult(leftToken.Span, rightToken.Span);
                }
            }
            // 处理右括号
            else if (token.RawKind == _closeBrace.Kind && AllowedForToken(token))
            {
                var rightToken = token;
                if (TryFindMatchingToken(rightToken, out var leftToken))
                {
                    return new BraceMatchingResult(leftToken.Span, rightToken.Span);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 是否允许该 token 参与匹配（子类可重写进行限制）
    /// </summary>
    protected virtual bool AllowedForToken(SyntaxToken token)
    {
        return true;
    }

    /// <summary>
    /// 判断字符是否为目标括号
    /// </summary>
    private bool IsBrace(char c)
    {
        return _openBrace.Character == c || _closeBrace.Character == c;
    }
}
