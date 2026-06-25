// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Composition;

namespace RoslynPad.Roslyn.BraceMatching;

/// <summary>
/// 导出括号匹配器的元数据特性，用于标记特定语言的括号匹配器实现类
/// </summary>
/// <remarks>
/// 该特性继承自<see cref="ExportAttribute"/>，用于MEF组合，指定导出的类型为<see cref="IBraceMatcher"/>
/// 并附加语言名称元数据，确保匹配器与特定编程语言关联
/// </remarks>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class)]
internal class ExportBraceMatcherAttribute(string language) : ExportAttribute(typeof(IBraceMatcher))
{
    /// <summary>
    /// 获取当前括号匹配器对应的语言名称（如C#、VB等）
    /// </summary>
    /// <exception cref="ArgumentNullException">当传入的language为null时抛出</exception>
    public string Language { get; } = language ?? throw new ArgumentNullException(nameof(language));
}
