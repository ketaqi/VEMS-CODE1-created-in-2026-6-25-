using System.Collections.Generic;
using RoslynPad.IconsFront.Models;

namespace RoslynPad.FontAwesome.Models
{
    /// <summary>
    /// 表示 Font Awesome 图标的 SVG 数据。
    /// </summary>
    /// <remarks>
    /// 此类包含渲染 SVG 图标所需的路径数据和视口信息。
    /// </remarks>
    internal class Svg
    {
        /// <summary>
        /// 获取或设置 SVG 路径数据。
        /// </summary>
        /// <value>
        /// SVG <c>&lt;path&gt;</c> 元素的 <c>d</c> 属性值，
        /// 定义图标的形状和轮廓。
        /// </value>
        public required string Path { get; set; }

        /// <summary>
        /// 获取或设置 SVG 视口（ViewBox）数据。
        /// </summary>
        /// <value>
        /// 包含四个整数值的列表，依次为：
        /// <list type="number">
        ///   <item><description>min-x：视口左上角 X 坐标</description></item>
        ///   <item><description>min-y：视口左上角 Y 坐标</description></item>
        ///   <item><description>width：视口宽度</description></item>
        ///   <item><description>height：视口高度</description></item>
        /// </list>
        /// </value>
        public required IReadOnlyList<int> ViewBox { get; set; }

        /// <summary>
        /// 将此 SVG 数据转换为通用的图标模型。
        /// </summary>
        /// <returns>包含视口和路径信息的 <see cref="IconModel"/> 实例。</returns>
        /// <remarks>
        /// 此方法将 Font Awesome 特定的 SVG 数据格式转换为应用程序通用的图标模型，
        /// 以便统一处理不同来源的图标。
        /// </remarks>
        public IconModel ToIconModel()
        {
            var viewBox = new ViewBoxModel(ViewBox[0], ViewBox[1], ViewBox[2], ViewBox[3]);
            var path = new PathModel(Path);
            return new IconModel(viewBox, path);
        }
    }
}
