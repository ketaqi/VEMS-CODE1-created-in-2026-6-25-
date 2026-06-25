namespace RoslynPad.UI;

/// <summary>
/// 文件夹浏览对话框接口：定义跨平台文件夹选择对话框的统一抽象。
/// </summary>
/// <remarks>
/// <para>
/// 此接口为应用程序提供与平台无关的文件夹选择对话框抽象，
/// 具体实现由平台适配器（如 <c>FolderBrowserDialogAdapter</c>）提供。
/// </para>
/// <para>
/// 典型使用场景：
/// <list type="bullet">
///   <item><description>选择工作目录</description></item>
///   <item><description>选择文档保存位置</description></item>
///   <item><description>设置默认项目路径</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var dialog = serviceProvider.GetRequiredService&lt;IFolderBrowserDialog&gt;();
/// dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
/// dialog.ShowEditBox = true;
/// 
/// if (dialog.Show() == true)
/// {
///     string selectedFolder = dialog.SelectedPath;
///     // 使用选中的文件夹路径
/// }
/// </code>
/// </example>
public interface IFolderBrowserDialog
{
    /// <summary>
    /// 获取或设置是否显示路径编辑框。
    /// </summary>
    /// <value>
    /// <c>true</c> 显示可编辑的路径输入框；<c>false</c> 仅显示文件夹树。
    /// </value>
    /// <remarks>
    /// 当设置为 <c>true</c> 时，用户可以直接输入或粘贴路径，
    /// 而不仅仅通过树形结构选择。
    /// </remarks>
    bool ShowEditBox { get; set; }

    /// <summary>
    /// 获取或设置选中的文件夹路径。
    /// </summary>
    /// <value>
    /// 在显示对话框前设置此属性可指定初始路径；
    /// 对话框关闭后此属性包含用户选择的路径。
    /// </value>
    /// <example>
    /// <code>
    /// // 设置初始路径
    /// dialog.SelectedPath = @"C:\Users\Documents";
    /// 
    /// if (dialog.Show() == true)
    /// {
    ///     // 获取用户选择的路径
    ///     string path = dialog.SelectedPath;
    /// }
    /// </code>
    /// </example>
    string SelectedPath { get; set; }

    /// <summary>
    /// 显示文件夹浏览对话框。
    /// </summary>
    /// <returns>
    /// <c>true</c> 如果用户选择了文件夹并点击确定；
    /// <c>false</c> 如果用户取消操作；
    /// <c>null</c> 如果发生错误或无法显示对话框。
    /// </returns>
    /// <remarks>
    /// <para>
    /// 调用此方法会阻塞直到用户关闭对话框。
    /// </para>
    /// <para>
    /// 返回 <c>true</c> 后，可通过 <see cref="SelectedPath"/> 获取选中的路径。
    /// </para>
    /// </remarks>
    bool? Show();
}
