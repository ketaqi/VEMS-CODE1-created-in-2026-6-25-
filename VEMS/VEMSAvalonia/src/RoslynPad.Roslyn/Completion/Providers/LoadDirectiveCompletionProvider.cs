// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;

namespace RoslynPad.Roslyn.Completion.Providers;

/// <summary>
/// 为C#加载指令（#load）提供自动补全的提供程序
/// </summary>
[ExportCompletionProvider("LoadDirectiveCompletionProvider", LanguageNames.CSharp)]
internal sealed class LoadDirectiveCompletionProvider : AbstractLoadDirectiveCompletionProvider
{
    /// <summary>
    /// 尝试获取加载指令中的字符串字面量令牌
    /// </summary>
    /// <param name="tree">语法树</param>
    /// <param name="position">位置偏移量</param>
    /// <param name="stringLiteral">输出的字符串字面量令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功获取令牌</returns>
    protected override bool TryGetStringLiteralToken(SyntaxTree tree, int position, out SyntaxToken stringLiteral, CancellationToken cancellationToken)
        => tree.TryGetStringLiteralToken(position, SyntaxKind.LoadDirectiveTrivia, out stringLiteral, cancellationToken);
}
