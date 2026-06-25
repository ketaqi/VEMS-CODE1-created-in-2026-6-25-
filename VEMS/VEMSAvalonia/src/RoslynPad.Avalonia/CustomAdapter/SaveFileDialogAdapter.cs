using System.Composition;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using RoslynPad.UI;
using RoslynPad.UI.Dialogs;

namespace RoslynPad.CustomAdapter;

/// <summary>
/// 保存文件对话框适配器（Avalonia 版）：将 <see cref="ISaveFileDialog"/> 桥接到 Avalonia 存储 API。
/// </summary>
/// <remarks>
/// <para>
/// 此类实现 <see cref="ISaveFileDialog"/> 接口，使用 Avalonia 的
/// <see cref="IStorageProvider.SaveFilePickerAsync(FilePickerSaveOptions)"/> 显示系统保存文件对话框。
/// </para>
/// <para>
/// 行为要点：
/// <list type="bullet">
///   <item><description>优先使用 <see cref="Filters"/> 多过滤器；为空时回退到单个 <see cref="Filter"/></description></item>
///   <item><description>支持 <see cref="InitialDirectory"/>、<see cref="InitialFileName"/> 和 <see cref="Title"/></description></item>
///   <item><description>若设置了 <see cref="DefaultExtension"/>，在用户未填写扩展名时自动补全</description></item>
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
/// var dialog = serviceProvider.GetRequiredService&lt;ISaveFileDialog&gt;();
/// dialog.Title = "保存脚本";
/// dialog.InitialFileName = "script.csx";
/// dialog.DefaultExtension = "csx";
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
[Export(typeof(ISaveFileDialog))]
internal class SaveFileDialogAdapter : ISaveFileDialog
{
    /// <summary>
    /// 获取或设置单个文件类型过滤器（回退选项）。
    /// </summary>
    /// <value>
    /// 当 <see cref="Filters"/> 未设置时使用此过滤器。
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
    /// 获取或设置建议的默认文件名（不含路径）。
    /// </summary>
    public string? InitialFileName { get; set; }

    /// <summary>
    /// 获取或设置初始目录的本地绝对路径。
    /// </summary>
    /// <value>
    /// 如 <c>C:\Work</c> 或 <c>/home/user</c>。
    /// </value>
    public string? InitialDirectory { get; set; }

    /// <summary>
    /// 获取或设置未提供扩展名时自动追加的默认扩展名。
    /// </summary>
    /// <value>
    /// 不含点号的扩展名，如 "csx"、"json"。
    /// </value>
    /// <remarks>
    /// 当用户未在保存对话框里输入扩展名且此属性非空时，
    /// 会在返回路径后自动添加 <c>.扩展名</c>。
    /// </remarks>
    public string? DefaultExtension { get; set; }

    /// <summary>
    /// 获取或设置对话框标题。
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 获取最近一次调用 <see cref="ShowAsync"/> 的返回文件路径。
    /// </summary>
    /// <value>
    /// 用户选择的本地文件路径；用户取消或无活动窗口时为 <c>null</c>。
    /// </value>
    public string? FilePath { get; private set; }

    /// <summary>
    /// 异步显示保存文件对话框。
    /// </summary>
    /// <returns>
    /// 用户选择的本地文件路径；用户取消或无活动窗口时返回 <c>null</c>。
    /// </returns>
    /// <remarks>
    /// <para>
    /// 实现流程：
    /// <list type="number">
    ///   <item><description>查找当前活动窗口；无则返回 <c>null</c></description></item>
    ///   <item><description>组装 <see cref="FilePickerSaveOptions"/>（文件名、初始目录、标题）</description></item>
    ///   <item><description>配置文件类型过滤器（优先 <see cref="Filters"/>，回退 <see cref="Filter"/>）</description></item>
    ///   <item><description>调用 <see cref="IStorageProvider.SaveFilePickerAsync(FilePickerSaveOptions)"/></description></item>
    ///   <item><description>若设置了 <see cref="DefaultExtension"/> 且路径缺少扩展名，自动补全</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public async Task<string?> ShowAsync()
    {
        // 1) 查找当前活动窗口
        var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
            ?.Windows.FirstOrDefault(w => w.IsActive);

        if (window == null)
        {
            return null;
        }

        // 2) 组装保存对话框参数
        var options = new FilePickerSaveOptions
        {
            SuggestedFileName = InitialFileName,
            SuggestedStartLocation = null,
            Title = Title
        };

        // 3) 设置初始目录
        if (!string.IsNullOrWhiteSpace(InitialDirectory))
        {
            var folder = await window.StorageProvider
                .TryGetFolderFromPathAsync(InitialDirectory)
                .ConfigureAwait(false);

            if (folder != null)
            {
                options.SuggestedStartLocation = folder;
                Console.WriteLine($"SaveFileDialogAdapter: 设置对话框初始目录为 {InitialDirectory}");
            }
            else
            {
                Console.WriteLine($"SaveFileDialogAdapter: 未找到初始目录 {InitialDirectory}");
            }
        }

        // 4) 配置文件类型过滤器
        if (Filters != null && Filters.Count > 0)
        {
            options.FileTypeChoices = Filters
                .Select(f => new FilePickerFileType(f.Header)
                {
                    Patterns = f.Extensions.ToList()
                })
                .ToList();
        }
        else if (Filter != null)
        {
            options.FileTypeChoices =
            [
                new FilePickerFileType(Filter.Header)
                {
                    Patterns = Filter.Extensions.ToList()
                }
            ];
        }

        // 5) 调用系统保存对话框
        var result = await window.StorageProvider
            .SaveFilePickerAsync(options)
            .ConfigureAwait(false);

        // 6) 取回本地路径
        FilePath = result?.TryGetLocalPath();

        // 7) 自动补全扩展名
        if (!string.IsNullOrWhiteSpace(FilePath) && !string.IsNullOrWhiteSpace(DefaultExtension))
        {
            var ext = "." + DefaultExtension.TrimStart('.');
            if (!FilePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            {
                FilePath += ext;
            }
        }

        return FilePath;
    }
}
