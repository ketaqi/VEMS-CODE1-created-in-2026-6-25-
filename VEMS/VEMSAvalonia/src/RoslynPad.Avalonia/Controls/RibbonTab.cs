using Avalonia.Controls;

namespace RoslynPad.Controls;

/// <summary>
/// Ribbon 标签页控件：表示 <see cref="Ribbon"/> 控件中的一个功能标签。
/// </summary>
/// <remarks>
/// <para>
/// 此控件继承自 <see cref="TabItem"/>，用于在 <see cref="Ribbon"/> 控件中定义功能标签页。
/// 每个标签页的内容区域通常包含一个或多个 <see cref="RibbonGroup"/>。
/// </para>
/// <para>
/// 与普通 <see cref="TabItem"/> 的区别：
/// <list type="bullet">
///   <item><description>使用专属的 <see cref="StyleKeyOverride"/> 以应用 Ribbon 风格样式</description></item>
///   <item><description>配合 <see cref="Ribbon"/> 控件的折叠/展开行为</description></item>
/// </list>
/// </para>
/// <para>
/// 继承的 <c>Header</c> 属性用于设置标签文字，<c>Content</c> 属性用于放置标签内容。
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;controls:Ribbon&gt;
///     &lt;!-- 首页标签 --&gt;
///     &lt;controls:RibbonTab Header="首页"&gt;
///         &lt;StackPanel Orientation="Horizontal"&gt;
///             &lt;controls:RibbonGroup Header="剪贴板"&gt;
///                 &lt;controls:RibbonButton Content="粘贴" Icon="{StaticResource PasteIcon}" /&gt;
///                 &lt;controls:RibbonButton Content="剪切" Icon="{StaticResource CutIcon}" /&gt;
///                 &lt;controls:RibbonButton Content="复制" Icon="{StaticResource CopyIcon}" /&gt;
///             &lt;/controls:RibbonGroup&gt;
///             
///             &lt;controls:RibbonGroup Header="字体"&gt;
///                 &lt;controls:RibbonButton Content="加粗" Icon="{StaticResource BoldIcon}" /&gt;
///                 &lt;controls:RibbonButton Content="斜体" Icon="{StaticResource ItalicIcon}" /&gt;
///             &lt;/controls:RibbonGroup&gt;
///         &lt;/StackPanel&gt;
///     &lt;/controls:RibbonTab&gt;
///     
///     &lt;!-- 插入标签 --&gt;
///     &lt;controls:RibbonTab Header="插入"&gt;
///         &lt;!-- 插入功能内容 --&gt;
///     &lt;/controls:RibbonTab&gt;
///     
///     &lt;!-- 视图标签 --&gt;
///     &lt;controls:RibbonTab Header="视图"&gt;
///         &lt;!-- 视图功能内容 --&gt;
///     &lt;/controls:RibbonTab&gt;
/// &lt;/controls:Ribbon&gt;
/// </code>
/// </example>
public class RibbonTab : TabItem
{
    /// <summary>
    /// 静态构造函数：可用于注册类级别的属性默认值或事件处理器。
    /// </summary>
    /// <remarks>
    /// 当前为空实现，保留以供将来扩展（如设置默认属性值、注册附加行为等）。
    /// </remarks>
    static RibbonTab()
    {
    }

    /// <summary>
    /// 指定样式查找键为 <see cref="RibbonTab"/>。
    /// </summary>
    /// <value>
    /// 返回 <see cref="RibbonTab"/> 类型，使得样式系统能够区分此控件与普通 <see cref="TabItem"/>。
    /// </value>
    /// <remarks>
    /// <para>
    /// 通过覆盖此属性，可以在资源字典中为 <see cref="RibbonTab"/> 定义专属样式：
    /// <code>
    /// &lt;Style Selector="controls|RibbonTab"&gt;
    ///     &lt;Setter Property="Background" Value="{DynamicResource RibbonTabBackground}" /&gt;
    ///     &lt;!-- 其他样式设置 --&gt;
    /// &lt;/Style&gt;
    /// </code>
    /// </para>
    /// </remarks>
    protected override Type StyleKeyOverride => typeof(RibbonTab);
}
