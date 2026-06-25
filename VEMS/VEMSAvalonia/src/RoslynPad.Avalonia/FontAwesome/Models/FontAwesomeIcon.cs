using System.Collections.Generic;

namespace RoslynPad.FontAwesome.Models
{
    /// <summary>
    /// 表示 Font Awesome 图标的数据模型。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 每个 Font Awesome 图标可能包含多种样式（如 Solid、Regular、Brands），
    /// 每种样式对应不同的 SVG 数据。
    /// </para>
    /// <para>
    /// 此类用于反序列化 Font Awesome 图标 JSON 数据文件中的图标定义。
    /// </para>
    /// </remarks>
    internal class FontAwesomeIcon
    {
        /// <summary>
        /// 获取或设置图标的 SVG 数据字典。
        /// </summary>
        /// <value>
        /// 键为图标样式（<see cref="Style"/>），值为对应的 SVG 数据（<see cref="Svg"/>）。
        /// 一个图标可能有多种样式的 SVG 表示。
        /// </value>
        public required Dictionary<Style, Svg> Svg { get; set; }
    }
}
