namespace RoslynPad.Editor;

/// <summary>
/// AvaloniaEdit 控件扩展方法类
/// 提供补全窗口状态判断的扩展方法
/// </summary>
public static class AvaloniaEditExtensions
{
    /// <summary>
    /// 判断补全窗口是否处于打开状态
    /// </summary>
    /// <param name="window">补全窗口基类实例</param>
    /// <returns>窗口有效可见则返回 true，否则返回 false</returns>
    public static bool IsOpen(this CompletionWindowBase window) => window?.IsEffectivelyVisible == true;
}
