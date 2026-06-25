// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace RoslynPad.Roslyn.BraceMatching;

/// <summary>
/// C#字符串字面量的括号匹配器，负责匹配普通字符串、逐字字符串、插值字符串的起止符号
/// </summary>
[ExportBraceMatcher(LanguageNames.CSharp)]
internal class StringLiteralBraceMatcher : IBraceMatcher
{
    /// <summary>
    /// 异步查找指定位置对应的字符串字面量起止括号位置
    /// </summary>
    /// <param name="document">当前文档实例</param>
    /// <param name="position">要匹配的字符位置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>括号匹配结果，若不匹配则返回null</returns>
    public async Task<BraceMatchingResult?> FindBracesAsync(Document document, int position, CancellationToken cancellationToken)
    {
        // 获取语法根节点
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        // 查找指定位置的语法令牌
        var token = root!.FindToken(position);

        // 令牌无诊断错误时执行匹配逻辑
        if (!token.ContainsDiagnostics)
        {
            // 处理普通字符串/逐字字符串字面量
            if (token.IsKind(SyntaxKind.StringLiteralToken))
            {
                // 逐字字符串（以@开头）的起止匹配（@" 开头，" 结尾）
                if (token.IsVerbatimStringLiteral())
                {
                    return new BraceMatchingResult(
                        new TextSpan(token.SpanStart, 2),
                        new TextSpan(token.Span.End - 1, 1));
                }

                // 普通字符串的起止匹配（" 开头，" 结尾）
                return new BraceMatchingResult(
                    new TextSpan(token.SpanStart, 1),
                    new TextSpan(token.Span.End - 1, 1));
            }

            // 处理插值字符串起始令牌（$" 或 @$"）
            if (token.IsKind(SyntaxKind.InterpolatedStringStartToken) || token.IsKind(SyntaxKind.InterpolatedVerbatimStringStartToken))
            {
                if (token.Parent is InterpolatedStringExpressionSyntax interpolatedString)
                {
                    return new BraceMatchingResult(token.Span, interpolatedString.StringEndToken.Span);
                }
            }
            // 处理插值字符串结束令牌
            else if (token.IsKind(SyntaxKind.InterpolatedStringEndToken))
            {
                if (token.Parent is InterpolatedStringExpressionSyntax interpolatedString)
                {
                    return new BraceMatchingResult(interpolatedString.StringStartToken.Span, token.Span);
                }
            }
        }

        // 无匹配结果时返回null
        return null;
    }
}
