using System.Globalization;

namespace RoslynPad.IconsFront.Models
{
    /// <summary>
    /// 表示 SVG 视口（ViewBox）的数据模型。
    /// </summary>
    /// <param name="X">视口左上角的 X 坐标。</param>
    /// <param name="Y">视口左上角的 Y 坐标。</param>
    /// <param name="Width">视口的宽度。</param>
    /// <param name="Height">视口的高度。</param>
    /// <remarks>
    /// <para>
    /// SVG 视口定义了图标内容的坐标系统和可视区域。
    /// 标准的 Font Awesome 图标通常使用 <c>0 0 512 512</c> 或 <c>0 0 640 512</c> 等视口。
    /// </para>
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code language="csharp">
    /// // 从参数创建
    /// var viewBox = new ViewBoxModel(0, 0, 512, 512);
    /// 
    /// // 从字符串解析
    /// var parsed = ViewBoxModel.Parse("0 0 640 512");
    /// 
    /// // 访问属性
    /// Console.WriteLine($"尺寸:  {viewBox.Width}x{viewBox.Height}");
    /// </code>
    /// </example>
    public record ViewBoxModel(int X, int Y, int Width, int Height)
    {
        /// <summary>
        /// 从空格分隔的字符串解析视口模型。
        /// </summary>
        /// <param name="viewBox">视口字符串，格式为 "X Y Width Height"。</param>
        /// <returns>解析后的 <see cref="ViewBoxModel"/> 实例。</returns>
        /// <exception cref="System.FormatException">当字符串格式不正确时抛出。</exception>
        /// <exception cref="System.IndexOutOfRangeException">当字符串部分不足四个时抛出。</exception>
        /// <example>
        /// <code language="csharp">
        /// var viewBox = ViewBoxModel.Parse("0 0 512 512");
        /// // viewBox. X = 0, viewBox.Y = 0, viewBox.Width = 512, viewBox.Height = 512
        /// </code>
        /// </example>
        public static ViewBoxModel Parse(string viewBox)
        {
            var parts = viewBox.Split(' ');
            return new ViewBoxModel(
                int.Parse(parts[0], CultureInfo.InvariantCulture),
                int.Parse(parts[1], CultureInfo.InvariantCulture),
                int.Parse(parts[2], CultureInfo.InvariantCulture),
                int.Parse(parts[3], CultureInfo.InvariantCulture));
        }
    }
}
