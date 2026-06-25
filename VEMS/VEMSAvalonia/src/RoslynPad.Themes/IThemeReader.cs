namespace RoslynPad.Themes;

/// <summary>
/// 定义主题读取器接口，用于从文件加载主题配置。
/// </summary>
public interface IThemeReader
{
    /// <summary>
    /// 异步读取主题文件并返回主题对象。
    /// </summary>
    /// <param name="file">主题文件的路径。</param>
    /// <param name="type">主题类型（亮色或暗色）。</param>
    /// <returns>解析后的 <see cref="Theme"/> 对象。</returns>
    Task<Theme> ReadThemeAsync(string file, ThemeType type);
}
