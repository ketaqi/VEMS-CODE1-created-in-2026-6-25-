namespace RoslynPad.FontAwesome.Models
{
    /// <summary>
    /// 定义 Font Awesome 图标的样式类型。
    /// </summary>
    /// <remarks>
    /// Font Awesome 提供三种主要的图标样式：
    /// <list type="bullet">
    ///   <item><description><see cref="Solid"/> - 实心图标，最常用的样式</description></item>
    ///   <item><description><see cref="Regular"/> - 线框图标，较细的线条</description></item>
    ///   <item><description><see cref="Brands"/> - 品牌图标，如公司和产品 Logo</description></item>
    /// </list>
    /// </remarks>
    internal enum Style
    {
        /// <summary>
        /// 实心样式（Solid）。
        /// </summary>
        /// <remarks>
        /// 对应 CSS 类前缀 <c>fa-solid</c> 或 <c>fas</c>。
        /// 这是最常用的图标样式，图标填充为实心。
        /// </remarks>
        Solid,

        /// <summary>
        /// 常规样式（Regular）。
        /// </summary>
        /// <remarks>
        /// 对应 CSS 类前缀 <c>fa-regular</c> 或 <c>far</c>。
        /// 图标使用线框描边，内部为空心。
        /// </remarks>
        Regular,

        /// <summary>
        /// 品牌样式（Brands）。
        /// </summary>
        /// <remarks>
        /// 对应 CSS 类前缀 <c>fa-brands</c> 或 <c>fab</c>。
        /// 包含各种品牌和公司的 Logo 图标。
        /// </remarks>
        Brands,
    }
}
