namespace RoslynPad.IconsFront.Models
{
    /// <summary>
    /// 表示图标的数据模型，包含视口和路径信息。
    /// </summary>
    /// <param name="ViewBox">图标的 SVG 视口定义。</param>
    /// <param name="Path">图标的 SVG 路径数据。</param>
    /// <remarks>
    /// 此记录类型封装了渲染 SVG 图标所需的基本信息，
    /// 可用于在不同 UI 框架中统一处理图标数据。
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code language="csharp">
    /// var viewBox = new ViewBoxModel(0, 0, 512, 512);
    /// var path = new PathModel("M256 0.. .");
    /// var icon = new IconModel(viewBox, path);
    /// 
    /// // 渲染图标
    /// var geometry = StreamGeometry.Parse(icon.Path);
    /// </code>
    /// </example>
    /// <seealso cref="ViewBoxModel"/>
    /// <seealso cref="PathModel"/>
    public record IconModel(ViewBoxModel ViewBox, PathModel Path);
}
