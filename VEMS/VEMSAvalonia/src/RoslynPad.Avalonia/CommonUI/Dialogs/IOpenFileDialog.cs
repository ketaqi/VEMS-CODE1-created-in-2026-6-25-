namespace RoslynPad.UI;

/// <summary>
/// 打开文件对话框接口：定义跨平台打开文件对话框的统一抽象。
/// </summary>
/// <remarks>
/// <para>
/// 此接口为应用程序提供与平台无关的打开文件对话框抽象，
/// 具体实现由平台适配器（如 <c>OpenFileDialogAdapter</c>）提供。
/// </para>
/// <para>
/// 主要功能：
/// <list type="bullet">
///   <item><description>支持单选或多选文件</description></item>
///   <item><description>设置文件类型过滤器（单个或多个）</description></item>
///   <item><description>指定默认目录和文件名</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var dialog = serviceProvider.GetRequiredService&lt;IOpenFileDialog&gt;();
/// dialog.Title = "打开脚本";
/// dialog.AllowMultiple = false;
/// dialog.Filter = new FileDialogFilter("C# 文件", "cs", "csx");
/// 
/// var files = await dialog.ShowAsync();
/// if (files?.Length > 0)
/// {
///     var content = await File.ReadAllTextAsync(files[0]);
/// }
/// </code>
/// </example>
public interface IOpenFileDialog
{
    /// <summary>
    /// 获取或设置是否允许选择多个文件。
    /// </summary>
    /// <value>
    /// <c>true</c> 允许多选；<c>false</c> 仅允许单选（默认）。
    /// </value>
    bool AllowMultiple { get; set; }

    /// <summary>
    /// 获取或设置单个文件类型过滤器。
    /// </summary>
    /// <value>
    /// 文件类型过滤器。当 <see cref="Filters"/> 为空时使用此属性。
    /// </value>
    /// <example>
    /// <code>
    /// dialog.Filter = new FileDialogFilter("C# 源文件", "cs");
    /// </code>
    /// </example>
    FileDialogFilter? Filter { get; set; }

    /// <summary>
    /// 获取或设置多个文件类型过滤器列表。
    /// </summary>
    /// <value>
    /// 文件类型过滤器集合；若非空，将覆盖 <see cref="Filter"/> 属性。
    /// </value>
    /// <example>
    /// <code>
    /// dialog.Filters = new List&lt;FileDialogFilter&gt;
    /// {
    ///     new("C# 文件", "cs", "csx"),
    ///     new("所有文件", "*")
    /// };
    /// </code>
    /// </example>
    IList<FileDialogFilter>? Filters { get; set; }

    /// <summary>
    /// 获取或设置默认打开目录。
    /// </summary>
    /// <value>
    /// 对话框打开时的初始目录路径。
    /// </value>
    string? InitialDirectory { get; set; }

    /// <summary>
    /// 获取或设置默认文件名。
    /// </summary>
    /// <value>
    /// 对话框打开时在文件名输入框中显示的默认值。
    /// </value>
    string? FileName { get; set; }

    /// <summary>
    /// 获取或设置对话框标题。
    /// </summary>
    /// <value>
    /// 显示在对话框标题栏的文本。
    /// </value>
    string? Title { get; set; }

    /// <summary>
    /// 异步显示打开文件对话框。
    /// </summary>
    /// <returns>
    /// 用户选择的文件路径数组；如果用户取消操作，则返回 <c>null</c>。
    /// 当 <see cref="AllowMultiple"/> 为 <c>false</c> 时，数组最多包含一个元素。
    /// </returns>
    /// <remarks>
    /// 此方法应在 UI 线程调用，因为需要访问活动窗口。
    /// </remarks>
    Task<string[]?> ShowAsync();
}
