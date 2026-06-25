// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RoslynPad.Roslyn.BraceMatching;

/// <summary>
/// 括号匹配服务核心接口，定义获取匹配括号的核心方法
/// </summary>
public interface IBraceMatchingService
{
    /// <summary>
    /// 异步获取指定文档中指定位置的匹配括号结果
    /// </summary>
    /// <param name="document">待分析的代码文档</param>
    /// <param name="position">查找位置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>括号匹配结果（无匹配则返回null）</returns>
    Task<BraceMatchingResult?> GetMatchingBracesAsync(Document document, int position, CancellationToken cancellationToken = default);
}

/// <summary>
/// 括号匹配结果结构体，包含左括号和右括号的文本范围
/// </summary>
/// <remarks>
/// 实现<see cref="IEquatable{T}"/>接口，支持值相等性比较
/// </remarks>
public readonly struct BraceMatchingResult : IEquatable<BraceMatchingResult>
{
    /// <summary>
    /// 获取左括号的文本范围
    /// </summary>
    public TextSpan LeftSpan { get; }

    /// <summary>
    /// 获取右括号的文本范围
    /// </summary>
    public TextSpan RightSpan { get; }

    /// <summary>
    /// 初始化括号匹配结果实例
    /// </summary>
    /// <param name="leftSpan">左括号文本范围</param>
    /// <param name="rightSpan">右括号文本范围</param>
    public BraceMatchingResult(TextSpan leftSpan, TextSpan rightSpan)
        : this()
    {
        LeftSpan = leftSpan;
        RightSpan = rightSpan;
    }

    /// <summary>
    /// 比较当前实例与另一<BraceMatchingResult>实例是否相等
    /// </summary>
    /// <param name="other">待比较的实例</param>
    /// <returns>相等返回true，否则返回false</returns>
    public bool Equals(BraceMatchingResult other)
    {
        return LeftSpan.Equals(other.LeftSpan) && RightSpan.Equals(other.RightSpan);
    }

    /// <summary>
    /// 比较当前实例与指定对象是否相等
    /// </summary>
    /// <param name="obj">待比较的对象</param>
    /// <returns>相等返回true，否则返回false</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        return obj is BraceMatchingResult result && Equals(result);
    }

    /// <summary>
    /// 获取当前实例的哈希码
    /// </summary>
    /// <returns>哈希码值</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            return (LeftSpan.GetHashCode() * 397) ^ RightSpan.GetHashCode();
        }
    }

    /// <summary>
    /// 重载相等运算符，比较两个<BraceMatchingResult>实例是否相等
    /// </summary>
    /// <param name="left">左侧实例</param>
    /// <param name="right">右侧实例</param>
    /// <returns>相等返回true，否则返回false</returns>
    public static bool operator ==(BraceMatchingResult left, BraceMatchingResult right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 重载不等运算符，比较两个<BraceMatchingResult>实例是否不相等
    /// </summary>
    /// <param name="left">左侧实例</param>
    /// <param name="right">右侧实例</param>
    /// <returns>不相等返回true，否则返回false</returns>
    public static bool operator !=(BraceMatchingResult left, BraceMatchingResult right)
    {
        return !left.Equals(right);
    }
}
