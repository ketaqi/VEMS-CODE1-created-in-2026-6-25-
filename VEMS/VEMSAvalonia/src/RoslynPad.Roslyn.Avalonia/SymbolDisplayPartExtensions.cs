using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace RoslynPad.Roslyn;

/// <summary>
/// 为SymbolDisplayPart相关的TaggedText提供扩展方法，用于转换为可视化字符串或UI元素
/// </summary>
public static class SymbolDisplayPartExtensions
{
    /// <summary>
    /// 从左到右标记前缀字符（Unicode左到右标记）
    /// </summary>
    private const string LeftToRightMarkerPrefix = "\u200e";

    /// <summary>
    /// 将TaggedText转换为可视化显示字符串，可选择是否包含左到右标记前缀
    /// </summary>
    /// <param name="part">待转换的TaggedText实例</param>
    /// <param name="includeLeftToRightMarker">是否包含左到右标记前缀</param>
    /// <returns>处理后的可视化显示字符串</returns>
    public static string ToVisibleDisplayString(this TaggedText part, bool includeLeftToRightMarker)
    {
        var text = part.ToString();

        if (includeLeftToRightMarker)
        {
            if (part.Tag == TextTags.Punctuation ||
                part.Tag == TextTags.Space ||
                part.Tag == TextTags.LineBreak)
            {
                text = LeftToRightMarkerPrefix + text;
            }
        }

        return text;
    }

    /// <summary>
    /// 将TaggedText转换为Avalonia的TextBlock控件，可指定是否加粗
    /// </summary>
    /// <param name="text">待转换的TaggedText实例</param>
    /// <param name="isBold">是否将TextBlock文本设置为粗体</param>
    /// <returns>包含TaggedText内容的TextBlock控件</returns>
    public static TextBlock ToRun(this TaggedText text, bool isBold = false)
    {
        var s = text.ToVisibleDisplayString(includeLeftToRightMarker: false);

        var run = new TextBlock { Text = s };

        if (isBold)
        {
            run.FontWeight = FontWeight.Bold;
        }

        switch (text.Tag)
        {
            case TextTags.Keyword:
                run.Foreground = Brushes.Blue;
                break;
            case TextTags.Struct:
            case TextTags.Enum:
            case TextTags.TypeParameter:
            case TextTags.Class:
            case TextTags.Delegate:
            case TextTags.Interface:
                run.Foreground = Brushes.Teal;
                break;
        }

        return run;
    }

    /// <summary>
    /// 将一组TaggedText转换为包含多个TextBlock的WrapPanel面板
    /// </summary>
    /// <param name="text">待转换的TaggedText集合</param>
    /// <param name="isBold">是否将所有TextBlock文本设置为粗体</param>
    /// <returns>包含所有TextBlock的WrapPanel面板（水平布局）</returns>
    public static Panel ToTextBlock(this IEnumerable<TaggedText> text, bool isBold = false)
    {
        var panel = new WrapPanel { Orientation = Orientation.Horizontal };

        foreach (var part in text)
        {
            panel.Children.Add(part.ToRun(isBold));
        }

        return panel;
    }
}
