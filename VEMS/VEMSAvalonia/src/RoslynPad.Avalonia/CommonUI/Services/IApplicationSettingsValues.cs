using System.ComponentModel;
using RoslynPad.Themes;

namespace RoslynPad.UI;

/// <summary>
/// 应用程序设置值接口：定义所有可配置的应用程序选项。
/// </summary>
/// <remarks>
/// <para>
/// 此接口继承自 <see cref="INotifyPropertyChanged"/>，
/// 支持 UI 绑定自动更新。设置值的更改会通知所有绑定的控件。
/// </para>
/// <para>
/// 设置分类：
/// <list type="bullet">
///   <item><description>遥测：<see cref="SendErrors"/></description></item>
///   <item><description>编辑器外观：<see cref="EditorFontSize"/>、<see cref="EditorFontFamily"/> 等</description></item>
///   <item><description>窗口布局：<see cref="WindowBounds"/>、<see cref="DockLayout"/> 等</description></item>
///   <item><description>搜索选项：<see cref="SearchFileContents"/>、<see cref="SearchUsingRegex"/> 等</description></item>
///   <item><description>主题：<see cref="BuiltInTheme"/>、<see cref="CustomThemePath"/> 等</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 修改编辑器字号
/// settings.Values.EditorFontSize = 14;
/// 
/// // 启用正则表达式搜索
/// settings.Values.SearchUsingRegex = true;
/// 
/// // 切换主题
/// settings.Values.BuiltInTheme = BuiltInTheme.Dark;
/// </code>
/// </example>
public interface IApplicationSettingsValues : INotifyPropertyChanged
{
    /// <summary>
    /// 获取或设置是否发送错误报告。
    /// </summary>
    /// <value>
    /// <c>true</c> 发送匿名错误报告以帮助改进应用；<c>false</c> 禁用遥测。
    /// </value>
    bool SendErrors { get; set; }

    /// <summary>
    /// 获取或设置是否启用括号自动补全。
    /// </summary>
    /// <value>
    /// <c>true</c> 输入左括号时自动补全右括号；<c>false</c> 禁用此功能。
    /// </value>
    bool EnableBraceCompletion { get; set; }

    /// <summary>
    /// 获取或设置已知的最新版本号（用于更新检查）。
    /// </summary>
    /// <value>
    /// 缓存的最新版本字符串，如 "1.2.3"；<c>null</c> 表示未检查过更新。
    /// </value>
    string? LatestVersion { get; set; }

    /// <summary>
    /// 获取或设置主窗口的边界位置（序列化格式）。
    /// </summary>
    /// <value>
    /// 窗口位置和大小的序列化字符串，用于恢复窗口布局。
    /// </value>
    string? WindowBounds { get; set; }

    /// <summary>
    /// 获取或设置 Dock 面板的布局配置（序列化格式）。
    /// </summary>
    /// <value>
    /// Dock 控件布局的序列化字符串，用于恢复面板布局。
    /// </value>
    string? DockLayout { get; set; }

    /// <summary>
    /// 获取或设置主窗口的状态（最大化/最小化/正常）。
    /// </summary>
    /// <value>
    /// 窗口状态的字符串表示。
    /// </value>
    string? WindowState { get; set; }

    /// <summary>
    /// 获取或设置代码编辑器的字号。
    /// </summary>
    /// <value>
    /// 编辑器字体大小（以磅为单位），通常范围为 8-72。
    /// </value>
    int EditorFontSize { get; set; }

    /// <summary>
    /// 获取或设置代码编辑器的字体名称。
    /// </summary>
    /// <value>
    /// 等宽字体名称，如 "Consolas"、"Fira Code"。
    /// </value>
    string EditorFontFamily { get; set; }

    /// <summary>
    /// 获取或设置输出区域的字号。
    /// </summary>
    /// <value>
    /// 输出面板字体大小（以磅为单位）。
    /// </value>
    int OutputFontSize { get; set; }

    /// <summary>
    /// 获取或设置输出区域的字体名称。
    /// </summary>
    /// <value>
    /// 输出面板使用的字体名称。
    /// </value>
    string EditorFontFamily1 { get; set; }

    /// <summary>
    /// 获取或设置用户文档的存储路径。
    /// </summary>
    /// <value>
    /// 用户自定义的文档目录；<c>null</c> 表示使用默认路径。
    /// </value>
    string? DocumentPath { get; set; }

    /// <summary>
    /// 获取或设置搜索时是否包含文件内容。
    /// </summary>
    /// <value>
    /// <c>true</c> 搜索文件名和文件内容；<c>false</c> 仅搜索文件名。
    /// </value>
    bool SearchFileContents { get; set; }

    /// <summary>
    /// 获取或设置是否使用正则表达式搜索。
    /// </summary>
    /// <value>
    /// <c>true</c> 将搜索文本解释为正则表达式；<c>false</c> 普通文本搜索。
    /// </value>
    bool SearchUsingRegex { get; set; }

    /// <summary>
    /// 获取或设置是否启用编译优化。
    /// </summary>
    /// <value>
    /// <c>true</c> 使用 Release 模式编译；<c>false</c> 使用 Debug 模式。
    /// </value>
    bool OptimizeCompilation { get; set; }

    /// <summary>
    /// 获取或设置实时模式的延迟时间（毫秒）。
    /// </summary>
    /// <value>
    /// 用户停止输入后触发实时编译的等待时间。
    /// </value>
    int LiveModeDelayMs { get; set; }

    /// <summary>
    /// 获取或设置是否在输入时实时搜索。
    /// </summary>
    /// <value>
    /// <c>true</c> 边输入边搜索；<c>false</c> 需按回车确认搜索。
    /// </value>
    bool SearchWhileTyping { get; set; }

    /// <summary>
    /// 获取或设置默认的执行平台名称。
    /// </summary>
    /// <value>
    /// 运行时平台标识，如 ".NET 8.0"。
    /// </value>
    string DefaultPlatformName { get; set; }

    /// <summary>
    /// 获取或设置窗口的默认字号。
    /// </summary>
    /// <value>
    /// UI 元素的基准字体大小；<c>null</c> 使用系统默认。
    /// </value>
    double? WindowFontSize { get; set; }

    /// <summary>
    /// 获取或设置注释时是否自动格式化文档。
    /// </summary>
    /// <value>
    /// <c>true</c> 添加注释后自动格式化代码；<c>false</c> 不自动格式化。
    /// </value>
    bool FormatDocumentOnComment { get; set; }

    /// <summary>
    /// 获取有效的文档存储路径。
    /// </summary>
    /// <value>
    /// 优先返回 <see cref="DocumentPath"/>，若为空则返回默认路径。
    /// </value>
    /// <remarks>
    /// 此属性为只读，提供获取实际使用的文档路径的便捷方式。
    /// </remarks>
    string EffectiveDocumentPath { get; }

    /// <summary>
    /// 获取或设置自定义主题文件的路径。
    /// </summary>
    /// <value>
    /// VS Code 主题 JSON 文件的路径；<c>null</c> 表示使用内置主题。
    /// </value>
    string? CustomThemePath { get; set; }

    /// <summary>
    /// 获取或设置自定义主题的类型（亮色/暗色）。
    /// </summary>
    /// <value>
    /// 主题类型；<c>null</c> 表示未设置自定义主题。
    /// </value>
    ThemeType? CustomThemeType { get; set; }

    /// <summary>
    /// 获取或设置当前使用的内置主题。
    /// </summary>
    /// <value>
    /// 内置主题枚举值，如 <see cref="BuiltInTheme.Light"/>、<see cref="BuiltInTheme.Dark"/>。
    /// </value>
    BuiltInTheme BuiltInTheme { get; set; }
}
