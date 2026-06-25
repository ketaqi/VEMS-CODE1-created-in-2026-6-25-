// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RoslynPad.Roslyn.BraceMatching;

/// <summary>
/// 括号匹配服务的扩展方法类，提供更便捷的匹配结果处理逻辑
/// </summary>
public static class BraceMatchingServiceExtensions
{
    /// <summary>
    /// 异步查找指定位置匹配括号的文本范围（自动处理光标在括号左侧/右侧的场景）
    /// </summary>
    /// <param name="service">括号匹配服务实例</param>
    /// <param name="document">待分析的代码文档</param>
    /// <param name="position">光标位置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>匹配括号的文本范围（无匹配则返回null）</returns>
    public static async Task<TextSpan?> FindMatchingSpanAsync(
        this IBraceMatchingService service,
        Document document,
        int position,
        CancellationToken cancellationToken)
    {
        // 检查光标右侧令牌的匹配括号
        var braces1 = await service.GetMatchingBracesAsync(document, position, cancellationToken).ConfigureAwait(false);

        // 检查光标左侧令牌的匹配括号
        BraceMatchingResult? braces2 = null;

        // 确保光标位置有效（大于0）
        if (position > 0)
        {
            braces2 = await service.GetMatchingBracesAsync(document, position - 1, cancellationToken).ConfigureAwait(false);
        }

        // 优先匹配光标在括号外侧边界的场景，例如 {^()} 优先返回()而非{}
        if (braces1.HasValue && position >= braces1.Value.LeftSpan.Start && position < braces1.Value.LeftSpan.End)
        {
            // ^{ } -- 返回右侧括号范围
            return braces1.Value.RightSpan;
        }

        if (braces2.HasValue && position > braces2.Value.RightSpan.Start && position <= braces2.Value.RightSpan.End)
        {
            // { }^ -- 返回左侧括号范围
            return braces2.Value.LeftSpan;
        }

        if (braces2.HasValue && position > braces2.Value.LeftSpan.Start && position <= braces2.Value.LeftSpan.End)
        {
            // {^ } -- 返回右侧括号范围
            return braces2.Value.RightSpan;
        }

        if (braces1.HasValue && position >= braces1.Value.RightSpan.Start && position < braces1.Value.RightSpan.End)
        {
            // { ^} - 返回左侧括号范围
            return braces1.Value.LeftSpan;
        }

        return null;
    }

    /// <summary>
    /// 异步获取光标位置左侧和右侧的所有匹配括号对
    /// </summary>
    /// <remarks>
    /// 规则：仅当光标位于起始括号左侧、结束括号右侧时，才返回匹配对；
    /// 支持多字符括号（如&lt;@ @&gt;），光标在起始括号内部（非末尾）也视为有效匹配
    /// </remarks>
    /// <param name="service">括号匹配服务实例</param>
    /// <param name="document">待分析的代码文档</param>
    /// <param name="position">光标位置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>左侧匹配结果和右侧匹配结果的元组</returns>
    public static async Task<(BraceMatchingResult? leftOfPosition, BraceMatchingResult? rightOfPosition)> GetAllMatchingBracesAsync(
        this IBraceMatchingService service,
        Document document,
        int position,
        CancellationToken cancellationToken)
    {
        // 光标右侧令牌的匹配括号
        var rightOfPosition = await service.GetMatchingBracesAsync(document, position, cancellationToken).ConfigureAwait(false);

        // 仅当光标位于起始括号范围内时，右侧匹配结果才有效
        // 单字符括号：^{ } 有效，{^ } 无效；多字符括号：^<@ @>、<^@ @> 有效，<@^ @> 无效
        if (rightOfPosition.HasValue &&
            !rightOfPosition.Value.LeftSpan.Contains(position))
        {
            rightOfPosition = null;
        }

        if (position == 0)
        {
            // 光标在文档起始位置，无左侧匹配括号
            return (leftOfPosition: null, rightOfPosition);
        }

        // 检查是否触达某个结构的结束位置（如 { }^、<@ @>^、<@ @^>）
        // 排除 { ^}、<@ ^@> 这类场景
        var leftOfPosition = await service.GetMatchingBracesAsync(document, position - 1, cancellationToken).ConfigureAwait(false);

        if (leftOfPosition.HasValue &&
            position <= leftOfPosition.Value.RightSpan.End &&
            position > leftOfPosition.Value.RightSpan.Start)
        {
            // 找到左侧有效匹配对
            return (leftOfPosition, rightOfPosition);
        }

        // 左侧无有效匹配对
        return (leftOfPosition: null, rightOfPosition);
    }
}
