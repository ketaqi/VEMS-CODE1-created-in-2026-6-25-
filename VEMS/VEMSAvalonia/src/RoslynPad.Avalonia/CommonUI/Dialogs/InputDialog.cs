using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace RoslynPad.UI.Dialogs;

/// <summary>
/// 输入对话框：用于获取用户输入的简单模态窗口。
/// </summary>
/// <remarks>
/// <para>
/// 该对话框提供一个文本输入框、确定和取消按钮，适用于重命名文件、
/// 输入新文件名等需要用户提供单行文本的场景。
/// </para>
/// <para>
/// 使用方式：
/// <code>
/// var dialog = new InputDialog("重命名", "请输入新名称：", "默认值");
/// var result = await dialog.ShowDialogAsync(ownerWindow);
/// if (result != null)
/// {
///     // 用户输入的内容在 result 中
/// }
/// </code>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var dialog = new InputDialog("新建文件", "请输入文件名：", "NewFile.cs");
/// string? fileName = await dialog.ShowDialogAsync(this);
/// </code>
/// </example>
public class InputDialog : Window
{
    /// <summary>
    /// 文本输入框控件。
    /// </summary>
    private readonly TextBox _inputBox;

    /// <summary>
    /// 获取用户输入的文本内容。
    /// </summary>
    /// <value>
    /// 用户在输入框中输入的文本；如果输入框为空或未初始化，则返回空字符串。
    /// </value>
    public string InputText => _inputBox?.Text ?? string.Empty;

    /// <summary>
    /// 初始化 <see cref="InputDialog"/> 类的新实例。
    /// </summary>
    /// <param name="title">对话框窗口的标题。</param>
    /// <param name="message">显示在输入框上方的提示消息。</param>
    /// <param name="defaultValue">输入框的默认值（可选）。</param>
    /// <remarks>
    /// 窗口默认尺寸为 400x160 像素，居中显示在父窗口上方。
    /// </remarks>
    public InputDialog(string title, string message, string defaultValue = "")
    {
        Title = title;
        Width = 400;
        Height = 160;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        // 创建确定按钮
        var okButton = new Button
        {
            Content = "确定",
            IsDefault = true,
            Width = 80,
            Margin = new Thickness(8)
        };

        // 创建取消按钮
        var cancelButton = new Button
        {
            Content = "取消",
            IsCancel = true,
            Width = 80,
            Margin = new Thickness(8)
        };

        // 创建输入框
        _inputBox = new TextBox
        {
            Text = defaultValue,
            Margin = new Thickness(8),
            Width = 360
        };

        // 绑定按钮事件
        okButton.Click += (_, _) => Close(true);
        cancelButton.Click += (_, _) => Close(false);

        // 构建界面布局
        Content = new StackPanel
        {
            Children =
            {
                new TextBlock { Text = message, Margin = new Thickness(8) },
                _inputBox,
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Children = { okButton, cancelButton }
                }
            }
        };
    }

    /// <summary>
    /// 以模态方式显示对话框，并等待用户操作。
    /// </summary>
    /// <param name="owner">父窗口，对话框将居中显示在此窗口上。</param>
    /// <returns>
    /// 如果用户点击"确定"，返回输入框中的文本；
    /// 如果用户点击"取消"或关闭窗口，返回 <c>null</c>。
    /// </returns>
    /// <example>
    /// <code>
    /// var dialog = new InputDialog("标题", "请输入内容：");
    /// var userInput = await dialog.ShowDialogAsync(mainWindow);
    /// if (userInput != null)
    /// {
    ///     Console.WriteLine($"用户输入：{userInput}");
    /// }
    /// </code>
    /// </example>
    public async Task<string?> ShowDialogAsync(Window owner)
    {
        var result = await ShowDialog<bool>(owner).ConfigureAwait(true);
        return result ? InputText : null;
    }
}
