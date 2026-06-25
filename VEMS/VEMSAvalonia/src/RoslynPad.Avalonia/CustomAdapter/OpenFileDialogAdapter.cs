using System.Composition;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using RoslynPad.UI;

namespace RoslynPad.CustomAdapter;

/// <summary>
/// 打开文件对话框适配器（Avalonia 版）：将 <see cref="IOpenFileDialog"/> 桥接到 Avalonia 存储 API。
/// </summary>
/// <remarks>
/// <para>
/// 此类实现 <see cref="IOpenFileDialog"/> 接口，使用 Avalonia 的
/// <see cref="IStorageProvider.OpenFilePickerAsync(FilePickerOpenOptions)"/> 显示系统文件选择对话框。
/// </para>
/// <para>
/// 行为要点：
/// <list type="bullet">
///   <item><description>支持多选（<see cref="AllowMultiple"/> 属性）</description></item>
///   <item><description>优先使用 <see cref="Filters"/>；为空时回退到单个 <see cref="Filter"/></description></item>
///   <item><description>支持初始目录 <see cref="InitialDirectory"/> 和对话框标题 <see cref="Title"/></description></item>
/// </list>
/// </para>
/// <para>
/// 线程模型：应在 UI 线程发起调用（需要活动窗口）。
/// </para>
/// <para>
/// 平台兼容：依赖 Avalonia 跨平台存储抽象，Windows / Linux / macOS 通用。
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var dialog = serviceProvider.GetRequiredService&lt;IOpenFileDialog&gt;();
/// dialog.Title = "打开脚本";
/// dialog.AllowMultiple = false;
/// dialog.Filter = new FileDialogFilter("C# 脚本", "cs", "csx");
/// 
/// var files = await dialog.ShowAsync();
/// if (files?.Length > 0)
/// {
///     var content = await File.ReadAllTextAsync(files[0]);
/// }
/// </code>
/// </example>
[Export(typeof(IOpenFileDialog))]
internal class OpenFileDialogAdapter : IOpenFileDialog
{
    /// <summary>
    /// 获取或设置是否允许多选文件。
    /// </summary>
    /// <value>
    /// <c>true</c> 允许选择多个文件；<c>false</c> 仅允许单选（默认）。
    /// </value>
    public bool AllowMultiple { get; set; }

    /// <summary>
    /// 获取或设置单个文件类型过滤器（回退选项）。
    /// </summary>
    /// <value>
    /// 当 <see cref="Filters"/> 为空或未设置时使用此过滤器。
    /// </value>
    public FileDialogFilter? Filter { get; set; }

    /// <summary>
    /// 获取或设置多个文件类型过滤器列表（优先使用）。
    /// </summary>
    /// <value>
    /// 若非空，将覆盖 <see cref="Filter"/> 属性。
    /// </value>
    public IList<FileDialogFilter>? Filters { get; set; }

    /// <summary>
    /// 获取或设置初始目录的本地绝对路径。
    /// </summary>
    /// <value>
    /// 如 <c>C:\Work</c> 或 <c>/home/user</c>。无效目录将被忽略，显示默认位置。
    /// </value>
    public string? InitialDirectory { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置建议的文件名（在某些平台上作为 UI 提示）。
    /// </summary>
    /// <value>
    /// 此属性为接口兼容性保留，Avalonia 的打开对话框可能不直接使用此值。
    /// </value>
    public string? FileName { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置对话框标题。
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 异步显示打开文件对话框。
    /// </summary>
    /// <returns>
    /// 选中的文件路径数组；用户取消或无活动窗口时返回 <c>null</c>。
    /// </returns>
    /// <remarks>
    /// <para>
    /// 实现流程：
    /// <list type="number">
    ///   <item><description>定位当前活动窗口；无则返回 <c>null</c></description></item>
    ///   <item><description>若 <see cref="InitialDirectory"/> 非空，尝试定位起始目录</description></item>
    ///   <item><description>配置 <see cref="FilePickerOpenOptions"/>（多选、起始目录、标题）</description></item>
    ///   <item><description>应用 <see cref="Filters"/> 或回退到 <see cref="Filter"/>，扩展名规范为 <c>*.ext</c> 格式</description></item>
    ///   <item><description>调用 <see cref="IStorageProvider.OpenFilePickerAsync(FilePickerOpenOptions)"/> 并返回结果</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public async Task<string[]?> ShowAsync()
    {
        // 1) 查找当前活动窗口
        var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
            ?.Windows.FirstOrDefault(w => w.IsActive);

        if (window == null)
        {
            return null;
        }

        // 2) 解析初始目录
        var suggestedStartLocation = !string.IsNullOrWhiteSpace(InitialDirectory)
            ? await window.StorageProvider.TryGetFolderFromPathAsync(InitialDirectory).ConfigureAwait(false)
            : null;

        // 3) 构建打开对话框选项
        var options = new FilePickerOpenOptions
        {
            AllowMultiple = AllowMultiple,
            SuggestedStartLocation = suggestedStartLocation,
            Title = Title
        };

        // 4) 配置文件类型过滤器
        if (Filters != null && Filters.Count > 0)
        {
            options.FileTypeFilter = Filters
                .Select(f => new FilePickerFileType(f.Header)
                {
                    Patterns = f.Extensions
                        .Select(ext => ext.StartsWith('.') ? $"*{ext}" : $"*.{ext}")
                        .ToList()
                })
                .ToList();
        }
        else if (Filter != null)
        {
            options.FileTypeFilter =
            [
                new FilePickerFileType(Filter.Header)
                {
                    Patterns = Filter.Extensions
                        .Select(ext => ext.StartsWith('.') ? $"*{ext}" : $"*.{ext}")
                        .ToList()
                }
            ];
        }

        // 5) 调用系统文件选择器
        var files = await window.StorageProvider
            .OpenFilePickerAsync(options)
            .ConfigureAwait(false);

        // 6) 转换为字符串路径数组
        return files.Select(file => file.Path.ToString()).ToArray();
    }
}
