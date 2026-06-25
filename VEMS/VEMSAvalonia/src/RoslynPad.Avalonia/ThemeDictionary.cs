using RoslynPad.Themes;

namespace RoslynPad;

/// <summary>
/// 具体主题资源字典：
/// 基于 <see cref="ThemeDictionaryBase"/>，将 <see cref="Theme"/> 中声明的颜色项（由本类的常量字段命名）
/// 批量注册到 <see cref="Avalonia.Controls.ResourceDictionary"/>：
/// - 注册 Brush：键为常量名本身（如 <c>"editor.background"</c>）；
/// - 注册 Color：键为常量名 + <c>"Color"</c>（如 <c>"editor.backgroundColor"</c>）。
/// </summary>
/// <remarks>
/// 约定：
/// 1) 仅本类中定义为 <c>public const string</c> 的字段会被识别为“需要注入的主题颜色键”；
/// 2) 实例化时会自动调用 <see cref="Initialize(Theme)"/>，从 <see cref="Theme"/> 中取色并注入到字典；
/// 3) 若某个键在 <see cref="Theme"/>.Colors 中不存在，将被忽略（不会抛异常）。<br/>
/// 用法：
/// <code>
/// var theme = Theme.LoadFromJson(...); // 含 Colors 映射
/// var dict  = new ThemeDictionary(theme); // 自动把本类常量对应的颜色注入为 Brush/Color
/// </code>
/// </remarks>
public class ThemeDictionary : ThemeDictionaryBase
{
    /// <summary>
    /// 反射收集“需要注册的颜色键”的集合：扫描本类中的所有 <c>public const string</c> 字段。
    /// </summary>
    /// <remarks>
    /// 通过仅保留 <see cref="System.Reflection.FieldAttributes.Literal"/>（常量）且静态的字段，
    /// 将它们的值（字符串）汇总为键集合，供 <see cref="Initialize(Theme)"/> 遍历。
    /// </remarks>
    private static readonly IReadOnlySet<string> s_colors = typeof(ThemeDictionary).GetFields()
        .Where(t => t.IsStatic && t.Attributes.HasFlag(System.Reflection.FieldAttributes.Literal))
        .Select(t => (string)t.GetValue(null)!) // 常量字段必有值，这里使用 ! 抑制空性警告
        .ToHashSet();

    /// <summary>
    /// 使用指定主题构造资源字典，并立即按约定把颜色键注入为 Brush/Color。
    /// </summary>
    /// <param name="theme">主题对象，包含颜色表（<see cref="Theme.Colors"/>）。</param>
    public ThemeDictionary(Theme theme) : base(theme)
    {
        Initialize(theme);
    }

    // —— 以下常量均为“主题颜色键名”，将用于从 Theme 中取色并注入到资源字典 —— //
    /// <summary>顶部文字颜色。</summary>
    public const string topforeground = "top.foreground";
    /// <summary>左侧文字颜色。</summary>
    public const string leftforeground = "left.foreground";
    /// <summary>菜单前景色。</summary>
    public const string menuforeground = "Menu.foreground";
    /// <summary>自身/独立区域前景色。</summary>
    public const string selfforeground = "self.foreground";
    /// <summary>底部状态栏背景色。</summary>
    public const string bottomstatusbar = "bottom.statusbar";
    /// <summary>编辑区块背景色。</summary>
    public const string editingblock = "editing.block";
    /// <summary>输出区块背景色。</summary>
    public const string outputblock = "output.block";
    /// <summary>文档树背景色。</summary>
    public const string documenttree = "document.tree";
    /// <summary>左侧工具栏背景色。</summary>
    public const string lefttoolbar = "left.toolbar";
    /// <summary>顶部工具栏背景色。</summary>
    public const string toptoolbar = "top.toolbar";
    /// <summary>编辑器页签栏背景色。</summary>
    public const string TabBarBackground = "editorGroupHeader.tabsBackground";
    /// <summary>编辑器页签栏边框色。</summary>
    public const string TabBarBorder = "editorGroupHeader.tabsBorder";
    /// <summary>滚动条滑块背景色。</summary>
    public const string ScrollBarSliderBackground = "scrollbarSlider.background";
    /// <summary>滚动条滑块激活背景色（按下/拖动）。</summary>
    public const string ScrollBarSliderActiveBackground = "scrollbarSlider.activeBackground";
    /// <summary>滚动条滑块悬停背景色。</summary>
    public const string ScrollBarSliderHoverBackground = "scrollbarSlider.hoverBackground";
    /// <summary>滚动条阴影色。</summary>
    public const string ScrollBarShadow = "scrollbar.shadow";
    /// <summary>编辑器背景色。</summary>
    public const string EditorBackground = "editor.background";
    /// <summary>编辑器前景色（文本颜色）。</summary>
    public const string EditorForeground = "editor.foreground";
    /// <summary>通用前景色。</summary>
    public const string Foreground = "foreground";
    /// <summary>面板背景色。</summary>
    public const string PanelBackground = "panel.background";
    /// <summary>状态栏背景色。</summary>
    public const string StatusBarBackground = "statusBar.background";
    /// <summary>状态栏前景色。</summary>
    public const string StatusBarForeground = "statusBar.foreground";
    /// <summary>状态栏“错误”项背景色。</summary>
    public const string StatusBarItemErrorBackground = "statusBarItem.errorBackground";
    /// <summary>状态栏“错误”项前景色。</summary>
    public const string StatusBarItemErrorForeground = "statusBarItem.errorForeground";
    /// <summary>焦点边框色。</summary>
    public const string FocusBorder = "focusBorder";
    /// <summary>列表选中（激活）背景色。</summary>
    public const string ListActiveSelectionBackground = "list.activeSelectionBackground";
    /// <summary>列表选中（激活）前景色。</summary>
    public const string ListActiveSelectionForeground = "list.activeSelectionForeground";
    /// <summary>列表选中（非激活）背景色。</summary>
    public const string ListInactiveSelectionBackground = "list.inactiveSelectionBackground";
    /// <summary>页签激活背景色。</summary>
    public const string TabActiveBackground = "tab.activeBackground";
    /// <summary>页签未激活背景色。</summary>
    public const string TabInactiveBackground = "tab.inactiveBackground";
    /// <summary>页签激活前景色。</summary>
    public const string TabActiveForeground = "tab.activeForeground";
    /// <summary>页签未激活前景色。</summary>
    public const string TabInactiveForeground = "tab.inactiveForeground";
    /// <summary>页签悬停背景色。</summary>
    public const string TabHoverBackground = "tab.hoverBackground";
    /// <summary>页签悬停前景色。</summary>
    public const string TabHoverForeground = "tab.hoverForeground";
    /// <summary>页签激活边框色。</summary>
    public const string TabActiveBorder = "tab.activeBorder";
    /// <summary>页签激活上边框色。</summary>
    public const string TabActiveBorderTop = "tab.activeBorderTop";
    /// <summary>页签悬停边框色。</summary>
    public const string TabHoverBorder = "tab.hoverBorder";
    /// <summary>页签边框色。</summary>
    public const string TabBorder = "tab.border";
    /// <summary>输入框边框色。</summary>
    public const string InputBorder = "input.border";
    /// <summary>标题栏（激活窗口）背景色。</summary>
    public const string TitleBarActiveBackground = "titleBar.activeBackground";
    /// <summary>菜单背景色。</summary>
    public const string Menubackground = "menu.background";
    /// <summary>侧边栏背景色。</summary>
    public const string SideBarbackground = "sideBar.background";
    /// <summary>侧边栏分区标题背景色。</summary>
    public const string SideBarSectionHeaderbackground = "sideBarSectionHeader.background";

    /// <summary>
    /// 根据 <paramref name="theme"/> 的颜色表，把本类所有常量键对应的颜色注册到资源字典：
    /// 对每个键，注册 Brush（键为原名）与 Color（键为原名 + "Color"）。
    /// </summary>
    /// <param name="theme">主题对象，提供 <see cref="Theme.TryGetColor(string)"/> 能力。</param>
    private void Initialize(Theme theme)
    {
        // 若主题不含颜色表，直接返回；这样可兼容最小化主题定义
        if (theme.Colors is null)
        {
            return;
        }

        // 遍历收集的键名集合，逐项尝试从主题取色并注入
        foreach (var id in s_colors)
        {
            // TryGetColor 成功才注入，避免注入空键；失败则静默跳过
            if (theme.TryGetColor(id) is { } color)
            {
                SetThemeColor(id, color); // 基类：同时注入 Brush 与 Color
            }
        }
    }
}
