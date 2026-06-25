namespace RoslynPad.UI.Dialogs;

/// <summary>
/// 保存文件对话框接口：定义跨平台保存文件对话框的统一抽象。
/// </summary>
/// <remarks>
/// <para>
/// 此接口为应用程序提供与平台无关的保存文件对话框抽象，
/// 具体实现由平台适配器（如 <c>SaveFileDialogAdapter</c>）提供。
/// </para>
/// <para>
/// 主要功能：
/// <list type="bullet">
///   <item><description>设置文件类型过滤器（单个或多个）</description></item>
///   <item><description>指定默认文件名和目录</description></item>
///   <item><description>获取用户选择的保存路径</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var dialog = serviceProvider.GetRequiredService&lt;ISaveFileDialog&gt;();
/// dialog.Title = "保存脚本";
/// dialog.InitialFileName = "script.csx";
/// dialog.Filters = new List&lt;FileDialogFilter&gt;
/// {
///     new("C# 脚本", "csx"),
///     new("所有文件", "*")
/// };
/// 
/// var path = await dialog.ShowAsync();
/// if (path != null)
/// {
///     await File.WriteAllTextAsync(path, content);
/// }
/// </code>
/// </example>
public interface ISaveFileDialog
{
    /// <summary>
    /// 获取或设置单个文件类型过滤器。
    /// </summary>
    /// <value>
    /// 文件类型过滤器，格式如 "C# Files|*.cs;*.csx"。
    /// 当 <see cref="Filters"/> 为空时使用此属性。
    /// </value>
    /// <example>
    /// <code>
    /// dialog.Filter = new FileDialogFilter("C# 文件", "cs", "csx");
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
    ///     new("C# 脚本", "csx"),
    ///     new("JSON 文件", "json"),
    ///     new("所有文件", "*")
    /// };
    /// </code>
    /// </example>
    IList<FileDialogFilter>? Filters { get; set; }

    /// <summary>
    /// 获取或设置默认文件名。
    /// </summary>
    /// <value>
    /// 对话框打开时显示的初始文件名（不含路径），如 "Program.cs"。
    /// </value>
    string? InitialFileName { get; set; }

    /// <summary>
    /// 获取或设置默认打开目录。
    /// </summary>
    /// <value>
    /// 对话框打开时的初始目录路径，如 "C:\Users\xxx\Documents"。
    /// </value>
    string? InitialDirectory { get; set; }

    /// <summary>
    /// 获取或设置默认扩展名。
    /// </summary>
    /// <value>
    /// 当用户未输入扩展名时自动追加的扩展名（不含点号），如 "json"。
    /// </value>
    string? DefaultExtension { get; set; }

    /// <summary>
    /// 获取用户选择的文件路径。
    /// </summary>
    /// <value>
    /// 用户选择的完整文件路径；如果用户取消操作，则为 <c>null</c>。
    /// </value>
    string? FilePath { get; }

    /// <summary>
    /// 获取或设置对话框标题。
    /// </summary>
    /// <value>
    /// 显示在对话框标题栏的文本。
    /// </value>
    string? Title { get; set; }

    /// <summary>
    /// 异步显示保存文件对话框。
    /// </summary>
    /// <returns>
    /// 用户选择的文件保存路径；如果用户取消操作，则返回 <c>null</c>。
    /// </returns>
    /// <remarks>
    /// 此方法应在 UI 线程调用，因为需要访问活动窗口。
    /// </remarks>
    Task<string?> ShowAsync();
}
