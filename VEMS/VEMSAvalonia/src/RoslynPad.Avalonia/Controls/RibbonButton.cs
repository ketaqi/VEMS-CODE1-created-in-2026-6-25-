using Avalonia;
using Avalonia.Controls;

namespace RoslynPad.Controls;

/// <summary>
/// Ribbon 按钮控件：带有图标支持的 Office 风格按钮。
/// </summary>
/// <remarks>
/// <para>
/// 此控件继承自 <see cref="Button"/>，新增 <see cref="Icon"/> 依赖属性用于显示图标。
/// 图标的具体呈现方式取决于控件模板的定义。
/// </para>
/// <para>
/// 典型用法：在 <see cref="RibbonGroup"/> 内放置多个 <see cref="RibbonButton"/>，
/// 每个按钮包含图标和文字标签。
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;controls:RibbonButton Content="保存" Command="{Binding SaveCommand}"&gt;
///     &lt;controls:RibbonButton.Icon&gt;
///         &lt;Image Source="/Resources/Save.png" /&gt;
///     &lt;/controls:RibbonButton.Icon&gt;
/// &lt;/controls:RibbonButton&gt;
/// 
/// &lt;!-- 使用 FontAwesome 图标 --&gt;
/// &lt;controls:RibbonButton Content="新建"&gt;
///     &lt;controls:RibbonButton.Icon&gt;
///         &lt;icon:Icon Value="fa-solid fa-plus" FontSize="32" /&gt;
///     &lt;/controls:RibbonButton.Icon&gt;
/// &lt;/controls:RibbonButton&gt;
/// </code>
/// </example>
public class RibbonButton : Button
{
    /// <summary>
    /// 标识 <see cref="Icon"/> 依赖属性。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此属性支持多种类型的图标内容：
    /// <list type="bullet">
    ///   <item><description><see cref="Avalonia.Media.Imaging.Bitmap"/> - 位图图像</description></item>
    ///   <item><description><see cref="Avalonia.Media.Drawing"/> - 矢量图形</description></item>
    ///   <item><description><see cref="Avalonia.Controls.Control"/> - 任意控件（如 PathIcon、FontIcon）</description></item>
    ///   <item><description><see cref="string"/> - 资源 URI 路径</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<RibbonButton, object?>(nameof(Icon));

    /// <summary>
    /// 获取或设置按钮的图标内容。
    /// </summary>
    /// <value>
    /// 图标内容对象，将由控件模板中的 <c>ContentPresenter</c> 或自定义元素呈现。
    /// </value>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// 指定样式查找键为 <see cref="RibbonButton"/>。
    /// </summary>
    /// <remarks>
    /// 这允许在资源字典中为 <see cref="RibbonButton"/> 定义专属样式，
    /// 而不会与普通 <see cref="Button"/> 样式冲突。
    /// </remarks>
    protected override Type StyleKeyOverride => typeof(RibbonButton);
}
