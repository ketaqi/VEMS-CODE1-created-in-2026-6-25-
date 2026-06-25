// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Completion;
using Roslyn.Utilities;
using Microsoft.CodeAnalysis.PooledObjects;

namespace RoslynPad.Roslyn.Completion.Providers;

/// <summary>
/// 加载指令（#load）路径补全提供程序的抽象基类，处理文件相关的路径补全
/// </summary>
internal abstract class AbstractLoadDirectiveCompletionProvider : AbstractDirectivePathCompletionProvider
{
    /// <summary>
    /// 加载指令补全项的规则配置
    /// </summary>
    private static readonly CompletionItemRules s_rules = CompletionItemRules.Create(
         filterCharacterRules: [],
         commitCharacterRules: [CharacterSetModificationRule.Create(CharacterSetModificationKind.Replace, GetCommitCharacters())],
         enterKeyRule: EnterKeyRule.Never,
         selectionBehavior: CompletionItemSelectionBehavior.HardSelection);

    /// <summary>
    /// 获取加载指令补全的提交字符集合
    /// </summary>
    /// <returns>不可变的提交字符数组</returns>
    private static ImmutableArray<char> GetCommitCharacters()
    {
        var builder = ArrayBuilder<char>.GetInstance();
        builder.Add('"'); // 引号作为提交字符

        if (PathUtilities.IsUnixLikePlatform)
        {
            builder.Add('/'); // Unix/Linux 下的路径分隔符
        }
        else
        {
            builder.Add('/'); // Windows 下的正斜杠
            builder.Add('\\'); // Windows 下的反斜杠
        }

        return builder.ToImmutableAndFree();
    }

    /// <summary>
    /// 提供加载指令的补全项（匹配当前文档扩展名的文件）
    /// </summary>
    /// <param name="context">补全上下文</param>
    /// <param name="pathThroughLastSlash">最后一个路径分隔符（含）之前的路径</param>
    /// <returns>异步任务</returns>
    protected override async Task ProvideCompletionsAsync(CompletionContext context, string pathThroughLastSlash)
    {
        var extension = Path.GetExtension(context.Document.FilePath);
        if (extension == null)
        {
            return;
        }

        // 获取文件系统补全辅助类，筛选与当前文档同扩展名的文件
        var helper = GetFileSystemCompletionHelper(context.Document, Microsoft.CodeAnalysis.Glyph.CSharpFile, [extension], s_rules);
        context.AddItems(await helper.GetItemsAsync(pathThroughLastSlash, context.CancellationToken).ConfigureAwait(false));
    }
}
