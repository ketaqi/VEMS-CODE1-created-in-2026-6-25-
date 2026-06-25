using Avalonia.Controls;
using Avalonia.Media;
using RoslynPad.Themes;

namespace RoslynPad.UI;

/// <summary>
/// 主题资源字典基类：将 VS Code 风格主题转换为 Avalonia 资源字典。
/// </summary>
/// <remarks>
/// <para>
/// 此抽象类继承自 <see cref="ResourceDictionary"/>，提供将 <see cref="Theme"/> 对象中的颜色
/// 转换为 Avalonia 可用的画刷和颜色资源的基础设施。
/// </para>
/// <para>
/// 主要功能：
/// <list type="bullet">
///   <item><description>解析十六进制颜色字符串为 <see cref="Color"/></description></item>
///   <item><description>创建 <see cref="SolidColorBrush"/> 资源</description></item>
///   <item><description>同时注册画刷和颜色资源（使用 "Color" 后缀区分）</description></item>
/// </list>
/// </para>
/// <para>
/// 派生类（如 <see cref="ThemeDictionary"/>）负责定义具体的颜色键常量并调用初始化逻辑。
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 在派生类中设置主题颜色
/// protected void Initialize(Theme theme)
/// {
///     if (theme.TryGetColor("editor.background") is { } color)
///     {
///         SetThemeColor("editor.background", color);
///     }
/// }
/// 
/// // 在 XAML 中使用
/// // Background="{DynamicResource editor.background}"
/// // 或直接使用颜色
/// // Color="{DynamicResource editor.backgroundColor}"
/// </code>
/// </example>
public abstract class ThemeDictionaryBase : ResourceDictionary
{
    /// <summary>
    /// 初始化 <see cref="ThemeDictionaryBase"/> 类的新实例。
    /// </summary>
    /// <param name="theme">要应用的主题对象。</param>
    /// <remarks>
    /// 派生类应在构造函数中调用自己的初始化方法来填充资源字典。
    /// </remarks>
    protected ThemeDictionaryBase(Theme theme)
    {
    }

    /// <summary>
    /// 设置指定名称的主题颜色资源。
    /// </summary>
    /// <param name="name">资源键名称。</param>
    /// <param name="colorString">十六进制颜色字符串（如 "#FF0000" 或 "#80FF0000"）。</param>
    /// <remarks>
    /// <para>
    /// 此方法会同时注册两个资源：
    /// <list type="bullet">
    ///   <item><description><paramref name="name"/> - <see cref="SolidColorBrush"/> 类型</description></item>
    ///   <item><description><paramref name="name"/> + "Color" - <see cref="Color"/> 类型</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// SetThemeColor("editor.background", "#1E1E1E");
    /// // 注册资源：
    /// // "editor.background" -> SolidColorBrush
    /// // "editor.backgroundColor" -> Color
    /// </code>
    /// </example>
    protected void SetThemeColor(string name, string colorString)
    {
        var brush = CreateBrush(ParseColor(colorString));
        this[name] = brush;
        this[GetColorKey(name)] = brush.Color;
    }

    /// <summary>
    /// 将主题颜色映射到系统资源键。
    /// </summary>
    /// <param name="name">主题颜色的资源键名称。</param>
    /// <param name="brushKey">系统画刷资源键。</param>
    /// <param name="colorKey">系统颜色资源键。</param>
    /// <remarks>
    /// 此方法用于将自定义主题颜色映射到 Avalonia 的系统主题资源键，
    /// 使得系统控件也能使用自定义主题颜色。
    /// </remarks>
    protected void SetThemeColorForSystemKeys(string name, object brushKey, object colorKey)
    {
        this[brushKey] = this[name];
        this[colorKey] = this[GetColorKey(name)];
    }

    /// <summary>
    /// 获取颜色资源的键名（在原键名后添加 "Color" 后缀）。
    /// </summary>
    /// <param name="key">原始资源键。</param>
    /// <returns>颜色资源键。</returns>
    private static string GetColorKey(string key) => key + "Color";

    /// <summary>
    /// 从主题对象创建画刷。
    /// </summary>
    /// <param name="theme">主题对象。</param>
    /// <param name="id">颜色 ID。</param>
    /// <returns>
    /// 如果主题中存在指定颜色，返回对应的 <see cref="SolidColorBrush"/>；
    /// 否则返回 <c>null</c>。
    /// </returns>
    protected static SolidColorBrush? CreateBrush(Theme theme, string id)
    {
        return theme.TryGetColor(id) is { } color ? CreateBrush(ParseColor(color)) : null;
    }

    /// <summary>
    /// 从颜色值创建实心画刷。
    /// </summary>
    /// <param name="color">颜色值。</param>
    /// <returns>使用指定颜色的 <see cref="SolidColorBrush"/>。</returns>
    private static SolidColorBrush CreateBrush(Color color) => new(color);

    /// <summary>
    /// 解析十六进制颜色字符串。
    /// </summary>
    /// <param name="color">颜色字符串（如 "#FF0000"、"#80FF0000"）。</param>
    /// <returns>解析后的 <see cref="Color"/> 值。</returns>
    /// <exception cref="FormatException">当颜色字符串格式无效时抛出。</exception>
    private static Color ParseColor(string color) => Color.Parse(color);
}
