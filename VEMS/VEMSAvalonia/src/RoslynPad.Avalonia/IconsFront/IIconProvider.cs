namespace RoslynPad.IconsFront
{
    /// <summary>
    /// 定义图提供程序的接口。
    /// </summary>
    /// <remarks>
    /// 实现此接口以提供特定图标库（如 Font Awesome、Material Icons 等）的图标数据。
    /// </remarks>
    /// <seealso cref="IIconReader"/>
    /// <seealso cref="IconProvider"/>
    public interface IIconProvider : IIconReader
    {
        /// <summary>
        /// 获取此图标提供程序的前缀标识符。
        /// </summary>
        /// <value>
        /// 用于识别此提供程序的前缀字符串。
        /// 例如，Font Awesome 提供程序使用 "fa" 前缀。
        /// </value>
        /// <remarks>
        /// <see cref="IconProvider"/> 使用此前缀将图标请求路由到正确的提供程序。
        /// </remarks>
        string Prefix { get; }
    }
}
