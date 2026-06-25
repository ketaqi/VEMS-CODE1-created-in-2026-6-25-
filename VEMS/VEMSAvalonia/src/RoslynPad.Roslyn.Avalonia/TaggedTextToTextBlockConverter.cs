using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Microsoft.CodeAnalysis;

namespace RoslynPad.Roslyn;

/// <summary>
/// Avalonia值转换器，用于将TaggedText集合转换为WrapPanel面板（包含对应的TextBlock控件）
/// </summary>
public sealed class TaggedTextToTextBlockConverter : IValueConverter
{
    /// <summary>
    /// 将TaggedText集合转换为WrapPanel面板
    /// </summary>
    /// <param name="value">待转换的值（应为IEnumerable&lt;TaggedText&gt;类型）</param>
    /// <param name="targetType">目标类型（未使用）</param>
    /// <param name="parameter">转换参数（未使用）</param>
    /// <param name="culture">文化信息（未使用）</param>
    /// <returns>转换后的WrapPanel面板，若value类型不匹配则返回null</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (value as IEnumerable<TaggedText>)?.ToTextBlock();
    }

    /// <summary>
    /// 反向转换（不支持）
    /// </summary>
    /// <param name="value">待反向转换的值（未使用）</param>
    /// <param name="targetType">目标类型（未使用）</param>
    /// <param name="parameter">转换参数（未使用）</param>
    /// <param name="culture">文化信息（未使用）</param>
    /// <exception cref="NotSupportedException">始终抛出该异常，表示不支持反向转换</exception>
    /// <returns>无返回值</returns>
    object IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
