namespace RoslynPad.Editor;

/// <summary>
/// 通用解析器工具类
/// </summary>
public static class Parsers
{
    /// <summary>
    /// 将字符串解析为Color对象
    /// </summary>
    /// <param name="color">颜色字符串（如#RRGGBB、RGB(255,255,255)等）</param>
    /// <returns>解析后的Color实例</returns>
    public static Color ParseColor(string color) => Color.Parse(color);
}
