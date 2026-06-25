namespace RoslynPad.UI;

/// <summary>
/// 内置主题枚举：定义应用程序可用的预设主题样式。
/// </summary>
/// <remarks>
/// <para>
/// 此枚举列出了应用程序内置的所有视觉主题。每个主题对应一组预定义的颜色配置，
/// 包括编辑器语法高亮、UI 控件颜色等。
/// </para>
/// <para>
/// 主题分类：
/// <list type="bullet">
///   <item><description>亮色系：<see cref="Light"/>、<see cref="VEMS"/>、<see cref="Quiet"/>、<see cref="Solarized"/></description></item>
///   <item><description>暗色系：<see cref="Dark"/>、<see cref="Red"/>、<see cref="Abyss"/>、<see cref="KimbieDark"/></description></item>
///   <item><description>系统自适应：<see cref="System"/></description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 切换到暗色主题
/// settings.Values.BuiltInTheme = BuiltInTheme.Dark;
/// 
/// // 检查当前是否为亮色主题
/// bool isLight = settings.Values.BuiltInTheme is 
///     BuiltInTheme.Light or BuiltInTheme.VEMS or BuiltInTheme.Quiet;
/// </code>
/// </example>
public enum BuiltInTheme
{
    /// <summary>
    /// 跟随系统主题：根据操作系统的深色/浅色模式设置自动切换。
    /// </summary>
    /// <remarks>
    /// 在 Windows 11/10、macOS 和支持主题切换的 Linux 桌面环境中有效。
    /// </remarks>
    System,

    /// <summary>
    /// 浅色主题：适合日间使用的明亮配色方案。
    /// </summary>
    /// <remarks>
    /// 基于 VS Code 的 Light Modern 主题。
    /// </remarks>
    Light,

    /// <summary>
    /// 深色主题：适合夜间使用的暗色配色方案。
    /// </summary>
    /// <remarks>
    /// 基于 VS Code 的 Dark Modern 主题。
    /// </remarks>
    Dark,

    /// <summary>
    /// VEMS 主题：应用程序的品牌定制浅色主题。
    /// </summary>
    /// <remarks>
    /// 专为 Virtual ElectroMagnetic Solutions 应用定制的配色方案。
    /// </remarks>
    VEMS,

    /// <summary>
    /// Quiet Light 主题：柔和的浅色主题。
    /// </summary>
    /// <remarks>
    /// 低对比度的浅色方案，减少视觉疲劳。
    /// </remarks>
    Quiet,

    /// <summary>
    /// Solarized 主题：经典的 Solarized 配色方案。
    /// </summary>
    /// <remarks>
    /// 基于 Ethan Schoonover 设计的 Solarized 色彩系统。
    /// </remarks>
    Solarized,

    /// <summary>
    /// Red 主题：红色调的深色主题。
    /// </summary>
    /// <remarks>
    /// 以红色为主色调的暗色方案。
    /// </remarks>
    Red,

    /// <summary>
    /// Abyss 主题：深邃的深蓝色暗色主题。
    /// </summary>
    /// <remarks>
    /// 极低亮度的深色方案，适合完全黑暗的环境。
    /// </remarks>
    Abyss,

    /// <summary>
    /// Kimbie Dark 主题：温暖色调的深色主题。
    /// </summary>
    /// <remarks>
    /// 以棕色和橙色为主的暖色调暗色方案。
    /// </remarks>
    KimbieDark,
}
