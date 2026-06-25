// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace RoslynPad.Roslyn.BraceMatching;

/// <summary>
/// C# 指令注释（如#if、#endif）的括号匹配器实现
/// </summary>
/// <remarks>
/// 继承自抽象的指令注释匹配器，专门处理C#语法中的条件编译指令匹配
/// </remarks>
[ExportBraceMatcher(LanguageNames.CSharp)]
internal class CSharpDirectiveTriviaBraceMatcher : AbstractDirectiveTriviaBraceMatcher<DirectiveTriviaSyntax, IfDirectiveTriviaSyntax, ElifDirectiveTriviaSyntax, ElseDirectiveTriviaSyntax, EndIfDirectiveTriviaSyntax>
{
    /// <summary>
    /// 获取与指定指令匹配的条件编译指令列表
    /// </summary>
    /// <param name="directive">待匹配的指令语法节点</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>匹配的指令语法节点列表</returns>
    internal override List<DirectiveTriviaSyntax> GetMatchingConditionalDirectives(DirectiveTriviaSyntax directive, CancellationToken cancellationToken)
            => [.. directive.GetMatchingConditionalDirectives(cancellationToken)];

    /// <summary>
    /// 获取与指定指令匹配的单个指令节点
    /// </summary>
    /// <param name="directive">待匹配的指令语法节点</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>匹配的指令语法节点（无匹配则返回null）</returns>
    internal override DirectiveTriviaSyntax? GetMatchingDirective(DirectiveTriviaSyntax directive, CancellationToken cancellationToken)
            => directive.GetMatchingDirective(cancellationToken);

    /// <summary>
    /// 获取指令节点用于标记的文本范围（包含#号和指令名称）
    /// </summary>
    /// <param name="directive">指令语法节点</param>
    /// <returns>指令的文本范围</returns>
    internal override TextSpan GetSpanForTagging(DirectiveTriviaSyntax directive)
            => TextSpan.FromBounds(directive.HashToken.SpanStart, directive.DirectiveNameToken.Span.End);
}
