using RoslynPad.Themes;

namespace RoslynPad.UI;

public class ThemeDictionary : ThemeDictionaryBase
{
    private static readonly IReadOnlySet<string> s_colors = typeof(ThemeDictionary).GetFields()
        .Where(t => t.IsStatic && t.Attributes.HasFlag(System.Reflection.FieldAttributes.Literal))
        .Select(t => (string)t.GetValue(null)!).ToHashSet();

    public ThemeDictionary(Theme theme) : base(theme)
    {
        Initialize(theme);
    }
    public const string topforeground = "top.foreground";
    public const string leftforeground = "left.foreground";
    public const string menuforeground = "Menu.foreground";
    public const string selfforeground = "self.foreground";
    public const string bottomstatusbar = "bottom.statusbar";
    public const string editingblock = "editing.block";
    public const string outputblock = "output.block";
    public const string documenttree = "document.tree";
    public const string lefttoolbar = "left.toolbar";
    public const string toptoolbar = "top.toolbar";
    public const string TabBarBackground = "editorGroupHeader.tabsBackground";
    public const string TabBarBorder = "editorGroupHeader.tabsBorder";
    public const string ScrollBarSliderBackground = "scrollbarSlider.background";
    public const string ScrollBarSliderActiveBackground = "scrollbarSlider.activeBackground";
    public const string ScrollBarSliderHoverBackground = "scrollbarSlider.hoverBackground";
    public const string ScrollBarShadow = "scrollbar.shadow";
    public const string EditorBackground = "editor.background";
    public const string EditorForeground = "editor.foreground";
    public const string Foreground = "foreground";
    public const string PanelBackground = "panel.background";
    public const string StatusBarBackground = "statusBar.background";
    public const string StatusBarForeground = "statusBar.foreground";
    public const string StatusBarItemErrorBackground = "statusBarItem.errorBackground";
    public const string StatusBarItemErrorForeground = "statusBarItem.errorForeground";
    public const string FocusBorder = "focusBorder";
    public const string ListActiveSelectionBackground = "list.activeSelectionBackground";
    public const string ListActiveSelectionForeground = "list.activeSelectionForeground";
    public const string ListInactiveSelectionBackground = "list.inactiveSelectionBackground";
    public const string TabActiveBackground = "tab.activeBackground";
    public const string TabInactiveBackground = "tab.inactiveBackground";
    public const string TabActiveForeground = "tab.activeForeground";
    public const string TabInactiveForeground = "tab.inactiveForeground";
    public const string TabHoverBackground = "tab.hoverBackground";
    public const string TabHoverForeground = "tab.hoverForeground";
    public const string TabActiveBorder = "tab.activeBorder";
    public const string TabActiveBorderTop = "tab.activeBorderTop";
    public const string TabHoverBorder = "tab.hoverBorder";
    public const string TabBorder = "tab.border";
    public const string InputBorder = "input.border";
    public const string TitleBarActiveBackground = "titleBar.activeBackground";
    public const string Menubackground = "menu.background";
    public const string SideBarbackground = "sideBar.background";
    public const string SideBarSectionHeaderbackground = "sideBarSectionHeader.background";
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
