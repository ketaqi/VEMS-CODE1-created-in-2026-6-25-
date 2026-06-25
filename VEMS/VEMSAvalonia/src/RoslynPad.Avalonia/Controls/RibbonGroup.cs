using Avalonia;
using Avalonia.Controls;

namespace RoslynPad.Controls;

/// <summary>
/// Ribbon 分组控件：用于在 Ribbon 标签页内组织相关功能按钮。
/// </summary>
/// <remarks>
/// <para>
/// 此控件继承自 <see cref="ItemsControl"/>，用于将一组相关的 <see cref="RibbonButton"/> 
/// 组织在一起，并显示一个分组标题。
/// </para>
/// <para>
/// 典型布局：
/// <list type="bullet">
///   <item><description>顶部：分组内的按钮（水平排列）</description></item>
///   <item><description>底部：分组标题（<see cref="Header"/>）</description></item>
///   <item><description>右侧：可选的分隔线</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;controls:RibbonTab Header="文件"&gt;
///     &lt;StackPanel Orientation="Horizontal"&gt;
///         &lt;controls:RibbonGroup Header="新建"&gt;
///             &lt;controls:RibbonButton Content="新建文件" Icon="{StaticResource NewFileIcon}" /&gt;
///             &lt;controls:RibbonButton Content="打开文件" Icon="{StaticResource OpenIcon}" /&gt;
///         &lt;/controls:RibbonGroup&gt;
///         
///         &lt;controls:RibbonGroup Header="保存"&gt;
///             &lt;controls:RibbonButton Content="保存" Icon="{StaticResource SaveIcon}" /&gt;
///             &lt;controls:RibbonButton Content="另存为" Icon="{StaticResource SaveAsIcon}" /&gt;
///         &lt;/controls:RibbonGroup&gt;
///     &lt;/StackPanel&gt;
/// &lt;/controls:RibbonTab&gt;
/// </code>
/// </example>
public class RibbonGroup : ItemsControl
{
    /// <summary>
    /// 标识 <see cref="Header"/> 依赖属性。
    /// </summary>
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<RibbonGroup, string?>(nameof(Header));

    /// <summary>
    /// 获取或设置分组的标题文本。
    /// </summary>
    /// <value>
    /// 显示在分组底部的标题文字；如果为 <c>null</c> 或空字符串，可能不显示标题区域（取决于样式）。
    /// </value>
    /// <example>
    /// <code>
    /// &lt;controls:RibbonGroup Header="编辑操作"&gt;
    ///     &lt;!-- 按钮内容 --&gt;
    /// &lt;/controls:RibbonGroup&gt;
    /// </code>
    /// </example>
    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// 指定样式查找键为 <see cref="RibbonGroup"/>。
    /// </summary>
    /// <remarks>
    /// 这允许在资源字典中为 <see cref="RibbonGroup"/> 定义专属样式。
    /// </remarks>
    protected override Type StyleKeyOverride => typeof(RibbonGroup);
}
