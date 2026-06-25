namespace RoslynPad.Editor;

/// <summary>
/// 提供 <see cref="TextView"/> 的扩展方法。
/// </summary>
internal static class TextViewExtensions
{
    /// <summary>
    /// 获取指定行列位置在视图中的屏幕坐标。
    /// </summary>
    /// <param name="textView">文本视图。</param>
    /// <param name="line">行号（1-based）。</param>
    /// <param name="column">列号（1-based）。</param>
    /// <returns>相对于视图的屏幕坐标点。</returns>
    public static Point GetPosition(this TextView textView, int line, int column)
    {
        var visualPosition = textView.GetVisualPosition(
            new TextViewPosition(line, column), VisualYPosition.LineBottom) - textView.ScrollOffset;
        return visualPosition;
    }
}
