// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace RoslynPad.Roslyn.BraceMatching;

/// <summary>
/// 括号匹配器核心接口，定义查找匹配括号的异步方法
/// </summary>
/// <remarks>
/// 不同语言/语法结构的括号匹配器需实现此接口，通过MEF导出（配合<see cref="ExportBraceMatcherAttribute"/>）
/// </remarks>
internal interface IBraceMatcher
{
    /// <summary>
    /// 异步查找指定文档中指定位置的匹配括号
    /// </summary>
    /// <param name="document">待分析的代码文档</param>
    /// <param name="position">查找匹配括号的起始位置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>括号匹配结果（无匹配则返回null）</returns>
    Task<BraceMatchingResult?> FindBracesAsync(Document document, int position, CancellationToken cancellationToken = default);
}
