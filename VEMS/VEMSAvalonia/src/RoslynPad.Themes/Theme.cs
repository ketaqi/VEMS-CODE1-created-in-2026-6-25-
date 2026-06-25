using System.Text.Json.Serialization;

namespace RoslynPad.Themes;

/// <summary>
/// 表示一个编辑器主题，包含颜色定义和语法高亮标记颜色。
/// </summary>
public class Theme
{
    private readonly IColorRegistry? _colorRegistry;

    /// <summary>
    /// 初始化 <see cref="Theme"/> 类的新实例。
    /// </summary>
    public Theme()
    {
    }

    /// <summary>
    /// 使用指定的颜色注册表初始化 <see cref="Theme"/> 类的新实例。
    /// </summary>
    /// <param name="colorRegistry">用于解析默认颜色的颜色注册表。</param>
    public Theme(IColorRegistry colorRegistry)
    {
        _colorRegistry = colorRegistry;
    }

    /// <summary>
    /// 获取或设置主题名称。
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 获取或设置语法高亮标记颜色列表。
    /// </summary>
    public List<TokenColor>? TokenColors { get; set; }

    /// <summary>
    /// 获取或设置颜色键值对字典（如 "editor.background" -> "#1E1E1E"）。
    /// </summary>
    public Dictionary<string, string>? Colors { get; set; }

    /// <summary>
    /// 获取或设置主题类型（亮色/暗色）。
    /// </summary>
    [JsonIgnore]
    public ThemeType Type { get; set; }

    /// <summary>
    /// 获取或设置要包含的基础主题文件路径。
    /// </summary>
    [JsonInclude]
    internal string? Include { get; set; }

    /// <summary>
    /// 获取作用域设置的前缀树，用于快速匹配语法作用域。
    /// </summary>
    internal Trie<TokenColorSettings> ScopeSettings { get; } = new();

    /// <summary>
    /// 尝试获取指定作用域的样式设置。
    /// </summary>
    /// <param name="scope">语法作用域名称。</param>
    /// <returns>匹配的作用域设置，如果未找到则返回 <c>null</c>。</returns>
    public KeyValuePair<string, TokenColorSettings>? TryGetScopeSettings(string scope) => ScopeSettings.FindLongestPrefix(scope);

    /// <summary>
    /// 尝试获取指定标识符对应的颜色值。
    /// </summary>
    /// <param name="id">颜色标识符。</param>
    /// <returns>颜色的十六进制字符串表示，如果未找到则返回 <c>null</c>。</returns>
    public string? TryGetColor(string id)
    {
        if (Colors?.TryGetValue(id, out var themeColor) == true)
        {
            return themeColor;
        }

        var color = _colorRegistry.NotNull().ResolveDefaultColor(id, this);
        if (color is not null)
        {
            Colors ??= [];
            Colors.Add(id, color);
            return color;
        }

        return null;
    }
}
