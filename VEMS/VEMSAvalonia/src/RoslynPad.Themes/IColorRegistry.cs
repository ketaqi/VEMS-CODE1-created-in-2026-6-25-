namespace RoslynPad.Themes;

/// <summary>
/// 定义颜色注册表接口，用于解析主题中的默认颜色值。
/// </summary>
public interface IColorRegistry
{
    /// <summary>
    /// 根据颜色标识符解析默认颜色值。
    /// </summary>
    /// <param name="id">颜色标识符（如 "editor.background"）。</param>
    /// <param name="theme">当前主题对象。</param>
    /// <returns>颜色的十六进制字符串表示，如果无法解析则返回 <c>null</c>。</returns>
    string? ResolveDefaultColor(string id, Theme theme);
}
