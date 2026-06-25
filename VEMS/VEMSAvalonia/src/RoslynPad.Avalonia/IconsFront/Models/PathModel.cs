namespace RoslynPad.IconsFront.Models
{
    /// <summary>
    /// 表示 SVG 路径数据的值类型。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此结构体封装了 SVG <c>&lt;path&gt;</c> 元素的 <c>d</c> 属性值，
    /// 定义了图标的形状和轮廓。
    /// </para>
    /// <para>
    /// 提供到 <see cref="string"/> 的隐式转换，便于直接在需要字符串的 API 中使用。
    /// </para>
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code language="csharp">
    /// var path = new PathModel("M256 0 L512 256 L256 512 L0 256 Z");
    /// 
    /// // 隐式转换为字符串
    /// string pathData = path;
    /// 
    /// // 用于 Avalonia 几何体解析
    /// var geometry = StreamGeometry.Parse(path);
    /// </code>
    /// </example>
    public readonly record struct PathModel
    {
        /// <summary>
        /// SVG 路径数据字符串。
        /// </summary>
        private readonly string _path;

        /// <summary>
        /// 初始化 <see cref="PathModel"/> 结构的新实例。
        /// </summary>
        /// <param name="path">SVG 路径数据字符串。</param>
        public PathModel(string path)
        {
            _path = path;
        }

        /// <summary>
        /// 将 <see cref="PathModel"/> 隐式转换为 <see cref="string"/>。
        /// </summary>
        /// <param name="path">要转换的路径模型。</param>
        /// <returns>路径数据字符串。</returns>
        public static implicit operator string(PathModel path) => path._path;

        /// <summary>
        /// 返回路径数据的字符串表示。
        /// </summary>
        /// <returns>SVG 路径数据字符串。</returns>
        public override string ToString() => _path;
    }
}
