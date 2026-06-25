using System.Composition;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using RoslynPad.UI;

namespace RoslynPad.CustomAdapter;

/// <summary>
/// 文件夹浏览对话框适配器（Avalonia 版）：将 <see cref="IFolderBrowserDialog"/> 桥接到 Avalonia 存储 API。
/// </summary>
/// <remarks>
/// <para>
/// 此类实现 <see cref="IFolderBrowserDialog"/> 接口，使用 Avalonia 的
/// <see cref="IStorageProvider.OpenFolderPickerAsync(FolderPickerOpenOptions)"/> 显示系统文件夹选择对话框。
/// </para>
/// <para>
/// 行为要点：
/// <list type="bullet">
///   <item><description><see cref="SelectedPath"/> 作为起始目录的建议值（若能解析为有效目录）</description></item>
///   <item><description>用户选择成功则更新 <see cref="SelectedPath"/> 并返回 <c>true</c></description></item>
///   <item><description><see cref="ShowEditBox"/> 在 Avalonia 下无对应能力，作为兼容属性保留但不生效</description></item>
/// </list>
/// </para>
/// <para>
/// 线程模型：当前实现使用同步封装（<c>Task.Run(...).Result</c>）以满足接口的同步 <see cref="Show"/> 签名。
/// </para>
/// <para>
/// 平台兼容：依赖 Avalonia 跨平台存储抽象，Windows / Linux / macOS 通用。
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var dialog = serviceProvider.GetRequiredService&lt;IFolderBrowserDialog&gt;();
/// dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
/// 
/// if (dialog.Show() == true)
/// {
///     string selectedFolder = dialog.SelectedPath;
///     Console.WriteLine($"选中的文件夹: {selectedFolder}");
/// }
/// </code>
/// </example>
[Export(typeof(IFolderBrowserDialog))]
internal class FolderBrowserDialogAdapter : IFolderBrowserDialog
{
    /// <summary>
    /// 当前选中或建议的文件夹路径。
    /// </summary>
    private string _selectedPath = string.Empty;

    /// <summary>
    /// 获取或设置是否显示可编辑的路径输入框。
    /// </summary>
    /// <value>
    /// 此属性为 WinForms/WPF 兼容保留；Avalonia 的文件夹选择器不支持此功能，设置无效。
    /// </value>
    public bool ShowEditBox { get; set; }

    /// <summary>
    /// 获取或设置当前选中的文件夹路径。
    /// </summary>
    /// <value>
    /// 作为输入：在打开对话框前用于尝试定位起始目录。
    /// 作为输出：当用户选择了文件夹后会被更新为该文件夹的本地路径。
    /// </value>
    public string SelectedPath
    {
        get => _selectedPath;
        set => _selectedPath = value;
    }

    /// <summary>
    /// 显示文件夹选择对话框（同步）。
    /// </summary>
    /// <returns>
    /// <c>true</c> - 用户选择了文件夹，<see cref="SelectedPath"/> 已更新；
    /// <c>false</c> - 无活动窗口、用户取消或操作失败。
    /// </returns>
    /// <remarks>
    /// <para>
    /// 实现步骤：
    /// <list type="number">
    ///   <item><description>通过 <see cref="IClassicDesktopStyleApplicationLifetime"/> 获取活动主窗口</description></item>
    ///   <item><description>构造 <see cref="FolderPickerOpenOptions"/>，尝试设置起始目录</description></item>
    ///   <item><description>调用 <see cref="IStorageProvider.OpenFolderPickerAsync(FolderPickerOpenOptions)"/></description></item>
    ///   <item><description>若用户选择了文件夹，取第一个结果并写回 <see cref="SelectedPath"/></description></item>
    /// </list>
    /// </para>
    /// <para>
    /// 注意：此方法为同步接口，内部使用 <c>Task.Run(...).Result</c> 以兼容既有调用点。
    /// </para>
    /// </remarks>
    public bool? Show()
    {
        // 1) 获取主窗口（文件选择器宿主）
        var mainWindow = Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if (mainWindow?.StorageProvider == null)
        {
            return false;
        }

        // 2) 同步包装异步流程
        var result = Task.Run(async () =>
        {
            var options = new FolderPickerOpenOptions
            {
                Title = "选择文件夹"
            };

            // 3) 起始目录：尝试将 SelectedPath 转换为 IStorageFolder
            if (!string.IsNullOrEmpty(_selectedPath))
            {
                var folder = await mainWindow.StorageProvider
                    .TryGetFolderFromPathAsync(_selectedPath)
                    .ConfigureAwait(true);

                if (folder != null)
                {
                    options.SuggestedStartLocation = folder;
                }
            }

            // 4) 打开系统文件夹选择器
            var folders = await mainWindow.StorageProvider
                .OpenFolderPickerAsync(options)
                .ConfigureAwait(true);

            return folders?.Count > 0 ? folders[0] : null;
        }).Result;

        // 5) 写回选择结果
        if (result != null)
        {
            _selectedPath = result.Path.LocalPath;
            return true;
        }

        return false;
    }
}
