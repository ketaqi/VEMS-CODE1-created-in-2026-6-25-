namespace RoslynPad.UI;

/// <summary>
/// 对话框基础接口：定义所有模态对话框的通用契约。
/// </summary>
/// <remarks>
/// <para>
/// 此接口是所有自定义对话框接口的基类，提供显示和关闭对话框的基本能力。
/// </para>
/// <para>
/// 派生接口示例：
/// <list type="bullet">
///   <item><description><see cref="ISaveDocumentDialog"/> - 保存文档对话框</description></item>
///   <item><description><see cref="IRenameSymbolDialog"/> - 重命名符号对话框</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyDialog : Window, IDialog
/// {
///     public async Task ShowAsync()
///     {
///         await ShowDialog(parentWindow);
///     }
///     
///     public void Close()
///     {
///         base.Close();
///     }
/// }
/// </code>
/// </example>
public interface IDialog
{
    /// <summary>
    /// 异步显示对话框并等待用户操作。
    /// </summary>
    /// <returns>表示异步操作的任务。任务完成时对话框已关闭。</returns>
    /// <remarks>
    /// 此方法应以模态方式显示对话框，阻止用户与父窗口交互，
    /// 直到对话框关闭。
    /// </remarks>
    Task ShowAsync();

    /// <summary>
    /// 关闭对话框。
    /// </summary>
    /// <remarks>
    /// 调用此方法将立即关闭对话框窗口。
    /// 通常由对话框内的"取消"按钮或关闭按钮触发。
    /// </remarks>
    void Close();
}
