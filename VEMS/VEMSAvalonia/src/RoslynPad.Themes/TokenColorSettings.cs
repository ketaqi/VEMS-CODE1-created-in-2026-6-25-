namespace RoslynPad.Themes;

/// <summary>
/// 表示语法标记的颜色和样式设置。
/// </summary>
/// <param name="Foreground">前景色（十六进制格式）。</param>
/// <param name="FontStyle">字体样式（如 "bold"、"italic"、"underline"）。</param>
public record TokenColorSettings(
    string? Foreground,
    string? FontStyle
);
