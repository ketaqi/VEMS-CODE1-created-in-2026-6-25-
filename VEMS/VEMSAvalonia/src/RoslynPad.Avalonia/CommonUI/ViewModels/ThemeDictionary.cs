using RoslynPad.Themes;

namespace RoslynPad.UI;

/// <summary>
/// 主题资源字典：定义应用程序使用的所有主题颜色资源键。
/// </summary>
/// <remarks>
/// <para>
/// 此类继承自 <see cref="ThemeDictionaryBase"/>，定义了应用程序中使用的所有颜色资源键，
/// 并在初始化时从 <see cref="Theme"/> 对象加载对应的颜色值。
/// </para>
/// <para>
/// 颜色键分类：
/// <list type="bullet">
///   <item><description>编辑器：<see cref="EditorBackground"/>、<see cref="EditorForeground"/></description></item>
///   <item><description>标签页：<see cref="TabActiveBackground"/>、<see cref="TabInactiveBackground"/> 等</description></item>
///   <item><description>状态栏：<see cref="StatusBarBackground"/>、<see cref="StatusBarForeground"/></description></item>
///   <item><description>滚动条：<see cref="ScrollBarSliderBackground"/> 等</description></item>
///   <item><description>自定义区域：<see cref="editingblock"/>、<see cref="outputblock"/> 等</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 创建主题资源字典
/// var theme = await themeReader.ReadThemeAsync("dark_modern.json", ThemeType.Dark);
/// var themeDictionary = new ThemeDictionary(theme);
/// 
/// // 添加到应用程序资源
/// Application.Current.Resources.MergedDictionaries.Add(themeDictionary);
/// 
/// // 在 XAML 中使用
/// // &lt;Border Background="{DynamicResource editor.background}" /&gt;
/// // &lt;TextBlock Foreground="{DynamicResource editor.foreground}" /&gt;
/// </code>
/// </example>
public class ThemeDictionary : ThemeDictionaryBase
{
    /// <summary>
    /// 所有已定义的颜色资源键集合（从静态常量字段自动提取）。
    /// </summary>
    private static readonly IReadOnlySet<string> s_colors = typeof(ThemeDictionary).GetFields()
        .Where(t => t.IsStatic && t.Attributes.HasFlag(System.Reflection.FieldAttributes.Literal))
        .Select(t => (string)t.GetValue(null)!).ToHashSet();

    /// <summary>
    /// 使用指定主题初始化 <see cref="ThemeDictionary"/> 类的新实例。
    /// </summary>
    /// <param name="theme">要应用的主题对象。</param>
    public ThemeDictionary(Theme theme) : base(theme)
    {
        Initialize(theme);
    }

    #region 自定义区域颜色键

    /// <summary>顶部工具栏前景色。</summary>
    public const string topforeground = "top.foreground";

    /// <summary>左侧活动栏前景色。</summary>
    public const string leftforeground = "left.foreground";

    /// <summary>菜单前景色。</summary>
    public const string menuforeground = "Menu.foreground";

    /// <summary>自定义前景色。</summary>
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

    #endregion

    #region 标签栏颜色键

    /// <summary>标签栏背景色。</summary>
    public const string TabBarBackground = "editorGroupHeader.tabsBackground";

    /// <summary>标签栏边框色。</summary>
    public const string TabBarBorder = "editorGroupHeader.tabsBorder";

    #endregion

    #region 滚动条颜色键

    /// <summary>滚动条滑块背景色。</summary>
    public const string ScrollBarSliderBackground = "scrollbarSlider.background";

    /// <summary>滚动条滑块激活状态背景色。</summary>
    public const string ScrollBarSliderActiveBackground = "scrollbarSlider.activeBackground";

    /// <summary>滚动条滑块悬停状态背景色。</summary>
    public const string ScrollBarSliderHoverBackground = "scrollbarSlider.hoverBackground";

    /// <summary>滚动条阴影色。</summary>
    public const string ScrollBarShadow = "scrollbar.shadow";

    #endregion

    #region 编辑器颜色键

    /// <summary>编辑器背景色。</summary>
    public const string EditorBackground = "editor.background";

    /// <summary>编辑器前景色（文本颜色）。</summary>
    public const string EditorForeground = "editor.foreground";

    /// <summary>通用前景色。</summary>
    public const string Foreground = "foreground";

    #endregion

    #region 面板颜色键

    /// <summary>面板背景色。</summary>
    public const string PanelBackground = "panel.background";

    #endregion

    #region 状态栏颜色键

    /// <summary>状态栏背景色。</summary>
    public const string StatusBarBackground = "statusBar.background";

    /// <summary>状态栏前景色。</summary>
    public const string StatusBarForeground = "statusBar.foreground";

    /// <summary>状态栏错误项背景色。</summary>
    public const string StatusBarItemErrorBackground = "statusBarItem.errorBackground";

    /// <summary>状态栏错误项前景色。</summary>
    public const string StatusBarItemErrorForeground = "statusBarItem.errorForeground";

    #endregion

    #region 焦点与列表颜色键

    /// <summary>焦点边框色。</summary>
    public const string FocusBorder = "focusBorder";

    /// <summary>列表激活选中项背景色。</summary>
    public const string ListActiveSelectionBackground = "list.activeSelectionBackground";

    /// <summary>列表激活选中项前景色。</summary>
    public const string ListActiveSelectionForeground = "list.activeSelectionForeground";

    /// <summary>列表非激活选中项背景色。</summary>
    public const string ListInactiveSelectionBackground = "list.inactiveSelectionBackground";

    #endregion

    #region 标签页颜色键

    /// <summary>激活标签页背景色。</summary>
    public const string TabActiveBackground = "tab.activeBackground";

    /// <summary>非激活标签页背景色。</summary>
    public const string TabInactiveBackground = "tab.inactiveBackground";

    /// <summary>激活标签页前景色。</summary>
    public const string TabActiveForeground = "tab.activeForeground";

    /// <summary>非激活标签页前景色。</summary>
    public const string TabInactiveForeground = "tab.inactiveForeground";

    /// <summary>标签页悬停背景色。</summary>
    public const string TabHoverBackground = "tab.hoverBackground";

    /// <summary>标签页悬停前景色。</summary>
    public const string TabHoverForeground = "tab.hoverForeground";

    /// <summary>激活标签页底部边框色。</summary>
    public const string TabActiveBorder = "tab.activeBorder";

    /// <summary>激活标签页顶部边框色。</summary>
    public const string TabActiveBorderTop = "tab.activeBorderTop";

    /// <summary>标签页悬停边框色。</summary>
    public const string TabHoverBorder = "tab.hoverBorder";

    /// <summary>标签页边框色。</summary>
    public const string TabBorder = "tab.border";

    #endregion

    #region 输入框与标题栏颜色键

    /// <summary>输入框边框色。</summary>
    public const string InputBorder = "input.border";

    /// <summary>标题栏激活状态背景色。</summary>
    public const string TitleBarActiveBackground = "titleBar.activeBackground";

    /// <summary>菜单背景色。</summary>
    public const string Menubackground = "menu.background";

    /// <summary>侧边栏背景色。</summary>
    public const string SideBarbackground = "sideBar.background";

    /// <summary>侧边栏分区标题背景色。</summary>
    public const string SideBarSectionHeaderbackground = "sideBarSectionHeader.background";

    #endregion

    /// <summary>
    /// 从主题对象初始化所有颜色资源。
    /// </summary>
    /// <param name="theme">主题对象。</param>
    /// <remarks>
    /// 此方法遍历所有已定义的颜色键常量，从主题中获取对应的颜色值并注册到资源字典。
    /// </remarks>
    private void Initialize(Theme theme)
    {
        if (theme.Colors is null)
        {
            return;
        }

        foreach (var id in s_colors)
        {
            if (theme.TryGetColor(id) is { } color)
            {
                SetThemeColor(id, color);
            }
        }
    }
}
