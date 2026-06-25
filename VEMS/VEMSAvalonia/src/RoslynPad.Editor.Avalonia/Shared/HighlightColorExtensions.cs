namespace RoslynPad.Editor;

/// <summary>
/// 为 <see cref="HighlightingColor"/> 类型提供的扩展方法类
/// </summary>
internal static class HighlightColorExtensions
{
    /// <summary>
    /// 将 <see cref="HighlightingColor"/> 实例冻结（若未冻结）并返回该实例
    /// </summary>
    /// <param name="color">待处理的 <see cref="HighlightingColor"/> 实例</param>
    /// <returns>冻结后的 <see cref="HighlightingColor"/> 实例</returns>
    public static HighlightingColor AsFrozen(this HighlightingColor color)
    {
        if (!color.IsFrozen)
        {
            color.Freeze();
        }

        return color;
    }
}
