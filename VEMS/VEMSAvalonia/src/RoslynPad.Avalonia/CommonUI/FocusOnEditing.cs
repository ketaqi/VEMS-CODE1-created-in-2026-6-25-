using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace RoslynPad.UI;

/// <summary>
/// 编辑聚焦行为：当进入编辑模式时自动聚焦并智能选择文本。
/// </summary>
/// <remarks>
/// <para>
/// 此附加属性提供了一种声明式的方式来处理内联编辑场景（如文件重命名）。
/// 当 <see cref="EnableProperty"/> 设置为 <c>true</c> 时，自动执行以下操作：
/// <list type="bullet">
///   <item><description>聚焦到 <see cref="TextBox"/> 控件</description></item>
///   <item><description>文件：仅选中不含扩展名的文件名部分</description></item>
///   <item><description>文件夹：选中全部文本</description></item>
/// </list>
/// </para>
/// <para>
/// 这符合大多数文件管理器的用户体验：重命名文件时保留扩展名不被误改。
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;!-- XAML 用法 --&gt;
/// &lt;TextBox Text="{Binding Name}"
///          behaviors:FocusOnEditing.Enable="{Binding IsEditing}" /&gt;
/// </code>
/// </example>
public static class FocusOnEditing
{
    /// <summary>
    /// 标识 <see cref="EnableProperty"/> 附加属性。
    /// </summary>
    /// <remarks>
    /// 当此属性从 <c>false</c> 变为 <c>true</c> 时，将自动聚焦并选择文本。
    /// </remarks>
    public static readonly AttachedProperty<bool> EnableProperty =
        AvaloniaProperty.RegisterAttached<TextBox, bool>(
            "Enable",
            typeof(FocusOnEditing),
            defaultValue: false);

    /// <summary>
    /// 设置指定 <see cref="TextBox"/> 的编辑聚焦启用状态。
    /// </summary>
    /// <param name="element">目标 <see cref="TextBox"/> 控件。</param>
    /// <param name="value">是否启用编辑聚焦。</param>
    public static void SetEnable(TextBox element, bool value) => element.SetValue(EnableProperty, value);

    /// <summary>
    /// 获取指定 <see cref="TextBox"/> 的编辑聚焦启用状态。
    /// </summary>
    /// <param name="element">目标 <see cref="TextBox"/> 控件。</param>
    /// <returns>是否启用编辑聚焦。</returns>
    public static bool GetEnable(TextBox element) => element.GetValue(EnableProperty);

    /// <summary>
    /// 静态构造函数：注册属性变更处理器。
    /// </summary>
    static FocusOnEditing()
    {
        EnableProperty.Changed.AddClassHandler<TextBox>(OnEnableChanged);
    }

    /// <summary>
    /// 处理 <see cref="EnableProperty"/> 值变更。
    /// </summary>
    /// <param name="tb">目标 <see cref="TextBox"/>。</param>
    /// <param name="e">属性变更事件参数。</param>
    private static void OnEnableChanged(TextBox tb, AvaloniaPropertyChangedEventArgs e)
    {
        // 只在从 false -> true 时触发
        if (e.NewValue is bool enabled && enabled)
        {
            // 若还未挂到可视树，等挂上再尝试
            tb.AttachedToVisualTree -= OnAttached;
            tb.AttachedToVisualTree += OnAttached;

            RequestFocus(tb);
        }
    }

    /// <summary>
    /// 处理控件附加到可视树事件。
    /// </summary>
    private static void OnAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is TextBox tb)
        {
            tb.AttachedToVisualTree -= OnAttached;
            RequestFocus(tb);
        }
    }

    /// <summary>
    /// 请求聚焦并智能选择文本。
    /// </summary>
    /// <param name="tb">目标 <see cref="TextBox"/>。</param>
    /// <remarks>
    /// <para>
    /// 使用 <see cref="DispatcherPriority.Render"/> 确保控件模板和布局已准备就绪。
    /// </para>
    /// <para>
    /// 文本选择逻辑：
    /// <list type="bullet">
    ///   <item><description>文件：选中主文件名（不含扩展名），如 "readme.txt" 只选中 "readme"</description></item>
    ///   <item><description>文件夹：选中全部文本</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    private static void RequestFocus(TextBox tb)
    {
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                tb.BringIntoView(); // 虚拟化场景下有助于生成容器
                tb.Focus();

                var text = tb.Text ?? string.Empty;

                if (tb.DataContext is DocumentViewModel vm && !vm.IsFolder)
                {
                    // 文件：只选中主文件名（不含扩展名）
                    var baseNameLen = Path.GetFileNameWithoutExtension(text)?.Length ?? 0;
                    tb.SelectionStart = 0;
                    tb.SelectionEnd = Math.Max(0, baseNameLen);
                }
                else
                {
                    // 文件夹：全选
                    tb.SelectAll();
                }
            }
            catch
            {
                // 忽略极少数控件生命周期时序问题
            }
        }, DispatcherPriority.Render);
    }
}
