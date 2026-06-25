// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RoslynPad.Roslyn.BraceMatching;

/// <summary>
/// C#尖括号（<>）的括号匹配器（主要用于泛型语法）
/// </summary>
[ExportBraceMatcher(LanguageNames.CSharp)]
internal class LessThanGreaterThanBraceMatcher : AbstractCSharpBraceMatcher
{
    /// <summary>
    /// 初始化尖括号匹配器，指定匹配的起止令牌类型
    /// </summary>
    public LessThanGreaterThanBraceMatcher()
        : base(SyntaxKind.LessThanToken, SyntaxKind.GreaterThanToken)
    {
    }
}
