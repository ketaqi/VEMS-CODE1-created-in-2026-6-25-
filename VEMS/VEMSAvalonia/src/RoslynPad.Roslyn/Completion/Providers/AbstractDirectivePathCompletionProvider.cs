// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.ErrorReporting;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace RoslynPad.Roslyn.Completion.Providers;

/// <summary>
/// 指令路径补全提供程序的抽象基类，封装路径补全的通用逻辑
/// </summary>
internal abstract class AbstractDirectivePathCompletionProvider : CompletionProvider
{
    /// <summary>
    /// 路径分隔符集合，根据操作系统类型区分（Unix/Linux 仅包含 /、,；Windows 包含 /、,、\）
    /// </summary>
    private static readonly char[] s_separators = PathUtilities.IsUnixLikePlatform
        ? ['/', ',']
        : ['/', ',', '\\'];

    /// <summary>
    /// 尝试从语法树指定位置获取字符串字面量令牌
    /// </summary>
    /// <param name="tree">语法树实例</param>
    /// <param name="position">代码中的位置偏移量</param>
    /// <param name="stringLiteral">输出参数，匹配到的字符串字面量令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功获取字符串字面量令牌</returns>
    protected abstract bool TryGetStringLiteralToken(SyntaxTree tree, int position, out SyntaxToken stringLiteral, CancellationToken cancellationToken);

    /// <summary>
    /// 提供补全项的核心方法，封装通用的补全前置逻辑
    /// </summary>
    /// <param name="context">补全上下文，包含补全所需的文档、位置、取消令牌等信息</param>
    /// <returns>异步任务</returns>
    public sealed override async Task ProvideCompletionsAsync(CompletionContext context)
    {
        try
        {
            var document = context.Document;
            var position = context.Position;
            var cancellationToken = context.CancellationToken;

            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

            if (!TryGetStringLiteralToken(tree!, position, out var stringLiteral, cancellationToken))
            {
                return;
            }

            var literalValue = stringLiteral.ToString();

            // 设置补全列表的文本跨度
            context.CompletionListSpan = GetTextChangeSpan(
                quotedPath: literalValue,
                quotedPathStart: stringLiteral.SpanStart,
                position: position);

            // 获取最后一个路径分隔符之前的路径部分
            var pathThroughLastSlash = GetPathThroughLastSlash(
                quotedPath: literalValue,
                quotedPathStart: stringLiteral.SpanStart,
                position: position);

            // 由子类实现具体的补全项提供逻辑
            await ProvideCompletionsAsync(context, pathThroughLastSlash).ConfigureAwait(false);
        }
        catch (Exception e) when (FatalError.ReportAndPropagateUnlessCanceled(e))
        {
            // 捕获并报告非取消类致命错误，无业务处理逻辑
        }
    }

    /// <summary>
    /// 判断是否应触发补全
    /// </summary>
    /// <param name="text">源代码文本</param>
    /// <param name="caretPosition">光标位置</param>
    /// <param name="trigger">补全触发器</param>
    /// <param name="options">选项集</param>
    /// <returns>始终返回true，表示触发补全</returns>
    public override bool ShouldTriggerCompletion(SourceText text, int caretPosition, CompletionTrigger trigger, OptionSet options)
    {
        return true;
    }

    /// <summary>
    /// 获取最后一个路径分隔符（含）之前的路径部分
    /// </summary>
    /// <param name="quotedPath">带引号的路径字符串</param>
    /// <param name="quotedPathStart">带引号路径在语法树中的起始位置</param>
    /// <param name="position">当前光标位置</param>
    /// <returns>最后一个路径分隔符（含）之前的路径</returns>
    /// <exception cref="ContractException">当路径首字符非引号时抛出</exception>
    private static string GetPathThroughLastSlash(string quotedPath, int quotedPathStart, int position)
    {
        Contract.ThrowIfTrue(quotedPath[0] != '"');

        const int QuoteLength = 1;

        var positionInQuotedPath = position - quotedPathStart;
        var path = quotedPath.Substring(QuoteLength, positionInQuotedPath - QuoteLength).Trim();
        var afterLastSlashIndex = AfterLastSlashIndex(path, path.Length);

        // 若存在最后一个分隔符，返回分隔符（含）之前的部分；否则返回完整路径
        return afterLastSlashIndex >= 0 ? path.Substring(0, afterLastSlashIndex) : path;
    }

    /// <summary>
    /// 获取补全时的文本变更跨度（即需要替换的文本范围）
    /// </summary>
    /// <param name="quotedPath">带引号的路径字符串</param>
    /// <param name="quotedPathStart">带引号路径在语法树中的起始位置</param>
    /// <param name="position">当前光标位置</param>
    /// <returns>文本变更的跨度范围</returns>
    private static TextSpan GetTextChangeSpan(string quotedPath, int quotedPathStart, int position)
    {
        // 文本变更范围：最后一个分隔符之后 至 带引号路径结束（排除末尾引号）
        var positionInQuotedPath = position - quotedPathStart;

        // 起始位置：最后一个分隔符之后 或 引号之后（无分隔符时）
        var afterLastSlashIndex = AfterLastSlashIndex(quotedPath, positionInQuotedPath);
        var afterFirstQuote = 1;

        var startIndex = Math.Max(afterLastSlashIndex, afterFirstQuote);
        var endIndex = quotedPath.Length;

        // 若路径以引号结尾，排除末尾引号
        if (EndsWithQuote(quotedPath))
        {
            endIndex--;
        }

        // 转换为语法树中的绝对位置跨度
        return TextSpan.FromBounds(startIndex + quotedPathStart, endIndex + quotedPathStart);
    }

    /// <summary>
    /// 判断带引号路径是否以引号结尾
    /// </summary>
    /// <param name="quotedPath">带引号的路径字符串</param>
    /// <returns>路径长度≥2且最后一个字符是引号时返回true，否则false</returns>
    private static bool EndsWithQuote(string quotedPath)
    {
        return quotedPath.Length >= 2 && quotedPath[quotedPath.Length - 1] == '"';
    }

    /// <summary>
    /// 获取指定位置之前最后一个路径分隔符的下一个索引
    /// </summary>
    /// <param name="text">待检索的文本</param>
    /// <param name="position">检索的结束位置</param>
    /// <returns>最后一个分隔符的下一个索引；无分隔符时返回-1</returns>
    private static int AfterLastSlashIndex(string text, int position)
    {
        // 确保位置在文本范围内（处理未终止的字符串）
        position = Math.Min(position, text.Length - 1);

        int index;
        if ((index = text.LastIndexOfAny(s_separators, position)) >= 0)
        {
            return index + 1;
        }

        return -1;
    }

    /// <summary>
    /// 由子类实现的、具体的补全项提供逻辑
    /// </summary>
    /// <param name="context">补全上下文</param>
    /// <param name="pathThroughLastSlash">最后一个路径分隔符（含）之前的路径</param>
    /// <returns>异步任务</returns>
    protected abstract Task ProvideCompletionsAsync(CompletionContext context, string pathThroughLastSlash);

    /// <summary>
    /// 获取文件系统补全辅助类实例
    /// </summary>
    /// <param name="document">当前文档实例</param>
    /// <param name="itemGlyph">补全项的图标类型</param>
    /// <param name="extensions">需要筛选的文件扩展名集合</param>
    /// <param name="completionRules">补全项规则</param>
    /// <returns>文件系统补全辅助类实例</returns>
    protected static FileSystemCompletionHelper GetFileSystemCompletionHelper(
        Document document,
        Microsoft.CodeAnalysis.Glyph itemGlyph,
        ImmutableArray<string> extensions,
        CompletionItemRules completionRules)
    {
        ImmutableArray<string> referenceSearchPaths;
        string? baseDirectory;

        // 从编译选项中获取元数据引用解析器的路径配置
        if (document.Project.CompilationOptions?.MetadataReferenceResolver is RuntimeMetadataReferenceResolver resolver)
        {
            referenceSearchPaths = resolver.PathResolver.SearchPaths;
            baseDirectory = resolver.PathResolver.BaseDirectory;
        }
        else
        {
            referenceSearchPaths = [];
            baseDirectory = null;
        }

        return new FileSystemCompletionHelper(
            Microsoft.CodeAnalysis.Glyph.OpenFolder,
            itemGlyph,
            referenceSearchPaths,
            GetBaseDirectory(document, baseDirectory),
            extensions,
            completionRules);
    }

    /// <summary>
    /// 获取文档的基础目录（优先使用文档路径的目录，相对路径时使用解析器的基础目录）
    /// </summary>
    /// <param name="document">当前文档实例</param>
    /// <param name="baseDirectory">元数据引用解析器的基础目录</param>
    /// <returns>文档的绝对基础目录；无有效目录时返回空字符串</returns>
    private static string GetBaseDirectory(Document document, string? baseDirectory)
    {
        var result = PathUtilities.GetDirectoryName(document.FilePath);
        if (!PathUtilities.IsAbsolute(result))
        {
            result = baseDirectory;
            Debug.Assert(result == null || PathUtilities.IsAbsolute(result));
        }

        return result ?? string.Empty;
    }
}
