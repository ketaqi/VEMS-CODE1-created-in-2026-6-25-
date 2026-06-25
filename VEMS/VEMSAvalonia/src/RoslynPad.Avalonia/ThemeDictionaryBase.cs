using Avalonia.Controls;
using Avalonia.Media;
using RoslynPad.Themes;

namespace RoslynPad;

/// <summary>
/// 主题资源字典基类：
/// 提供“以主题颜色为中心”的资源注册辅助方法，统一把某一主题色同时注册成
/// <list type="bullet">
/// <item><description><c>{name}</c>：对应的 <see cref="SolidColorBrush"/>（供控件绑定刷子使用）；</description></item>
/// <item><description><c>{name}Color</c>：对应的 <see cref="Color"/> 值（供需要 <see cref="Color"/> 的场景，如动画、绘制等）。</description></item>
/// </list>
/// 另外提供把同一颜色桥接到“系统资源键”的方法，便于跟 Avalonia 的系统主题键联动。
/// </summary>
/// <remarks>
/// 约定：若注册键为 <c>"Primary"</c>，则会同时产生 <c>"Primary"</c>（Brush）与 <c>"PrimaryColor"</c>（Color）两个项。<br/>
/// 用法：派生类在构造函数中根据 <see cref="Theme"/> 的颜色定义调用 <see cref="SetThemeColor(string, string)"/> 或
/// <see cref="SetThemeColorForSystemKeys(string, object, object)"/> 进行批量注册。<br/>
/// 线程模型：仅应在 UI 线程初始化与访问，以避免跨线程写入 <see cref="ResourceDictionary"/>。
/// </remarks>
public abstract class ThemeDictionaryBase : ResourceDictionary
{
    /// <summary>
    /// 使用指定主题实例构造资源字典。
    /// </summary>
    /// <param name="theme">主题对象，提供颜色查找能力。</param>
    /// <remarks>
    /// 基类不直接初始化任何键值；实际的键值填充由派生类完成。
    /// </remarks>
    protected ThemeDictionaryBase(Theme theme)
    {
        // 这里故意保持为空：派生类在其构造函数内根据 theme 填充资源。
    }

    /// <summary>
    /// 将一个主题颜色注册为 Brush 与 Color 两个条目：
    /// <c>{name}</c>（<see cref="SolidColorBrush"/>）与 <c>{name}Color</c>（<see cref="Color"/>）。
    /// </summary>
    /// <param name="name">资源名（不含 <c>Color</c> 后缀）。例如：<c>"Primary"</c>。</param>
    /// <param name="colorString">颜色字符串，接受 Avalonia 的解析格式（如 <c>#FF3366</c>、<c>#AARRGGBB</c>）。</param>
    /// <remarks>
    /// - 典型绑定：<c>{DynamicResource Primary}</c> 用作 Brush；<c>{DynamicResource PrimaryColor}</c> 用作 Color。<br/>
    /// - 颜色字符串解析由 <see cref="ParseColor(string)"/> 完成；非法格式将抛出 <see cref="FormatException"/>。
    /// </remarks>
    protected void SetThemeColor(string name, string colorString)
    {
        var brush = CreateBrush(ParseColor(colorString)); // 将字符串解析为 Color，再创建可冻结的 SolidColorBrush
        this[name] = brush;                               // 注册刷子键：name
        this[GetColorKey(name)] = brush.Color;            // 注册颜色键：name + "Color"
    }

    /// <summary>
    /// 将已注册的主题颜色（通过 <paramref name="name"/>）桥接到指定的“系统资源键”：
    /// 会把 <c>{name}</c> 的 Brush 映射到 <paramref name="brushKey"/>，
    /// 把 <c>{name}Color</c> 的 Color 映射到 <paramref name="colorKey"/>。
    /// </summary>
    /// <param name="name">已通过 <see cref="SetThemeColor(string, string)"/> 注册的颜色名。</param>
    /// <param name="brushKey">目标系统 Brush 资源键（通常为 <see cref="object"/> 类型的静态键）。</param>
    /// <param name="colorKey">目标系统 Color 资源键（通常为 <see cref="object"/> 类型的静态键）。</param>
    /// <remarks>
    /// 典型场景：把自定义的 <c>"Primary"</c> 同步到 Avalonia 的系统键（如按钮前景/背景的主题位），
    /// 以获得整套控件的一致外观。
    /// </remarks>
    protected void SetThemeColorForSystemKeys(string name, object brushKey, object colorKey)
    {
        // 将 name 对应的 Brush/Color 重定向到系统键，避免重复创建对象
        this[brushKey] = this[name];
        this[colorKey] = this[GetColorKey(name)];
    }

    /// <summary>
    /// 由基础名生成对应的 Color 键名：<c>{key}Color</c>。
    /// </summary>
    private static string GetColorKey(string key) => key + "Color";

    /// <summary>
    /// 尝试从 <paramref name="theme"/> 读取颜色 id，并在成功时创建对应的 <see cref="SolidColorBrush"/>。
    /// </summary>
    /// <param name="theme">主题对象。</param>
    /// <param name="id">主题颜色标识（例如 <c>"Primary"</c>、<c>"Accent1"</c>）。</param>
    /// <returns>
    /// 若该 <paramref name="id"/> 在主题中存在颜色定义，则返回对应的 <see cref="SolidColorBrush"/>；
    /// 否则返回 <see langword="null"/>。
    /// </returns>
    /// <remarks>
    /// 这是一个便捷方法，供派生类在不直接持有颜色字符串时使用。
    /// </remarks>
    protected static SolidColorBrush? CreateBrush(Theme theme, string id)
    {
        // Theme.TryGetColor 返回颜色字符串；存在即解析为 Color 并创建 Brush
        return theme.TryGetColor(id) is { } color ? CreateBrush(ParseColor(color)) : null;
    }

    /// <summary>
    /// 由 <see cref="Color"/> 创建对应的 <see cref="SolidColorBrush"/>。
    /// </summary>
    private static SolidColorBrush CreateBrush(Color color) => new(color);

    /// <summary>
    /// 解析 Avalonia 风格的颜色字符串为 <see cref="Color"/>。
    /// </summary>
    /// <param name="color">颜色字符串（支持 <c>#RGB</c>、<c>#ARGB</c>、<c>#RRGGBB</c>、<c>#AARRGGBB</c> 等）。</param>
    private static Color ParseColor(string color) => Color.Parse(color);
}
