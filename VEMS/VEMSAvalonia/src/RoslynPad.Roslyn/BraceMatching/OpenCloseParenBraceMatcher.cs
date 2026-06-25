// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RoslynPad.Roslyn.BraceMatching;

/// <summary>
/// C#圆括号（()）的括号匹配器
/// </summary>
[ExportBraceMatcher(LanguageNames.CSharp)]
internal class OpenCloseParenBraceMatcher : AbstractCSharpBraceMatcher
{
    /// <summary>
    /// 初始化圆括号匹配器，指定匹配的起止令牌类型
    /// </summary>
    public OpenCloseParenBraceMatcher()
        : base(SyntaxKind.OpenParenToken, SyntaxKind.CloseParenToken)
    {
    }
}
