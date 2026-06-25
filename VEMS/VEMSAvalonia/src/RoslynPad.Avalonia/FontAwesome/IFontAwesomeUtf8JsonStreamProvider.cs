using System.IO;

namespace RoslynPad.FontAwesome
{
    /// <summary>
    /// 定义提供 Font Awesome 图标 JSON 数据流的接口。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 实现此接口以提供自定义的图标数据源。
    /// 默认实现 <see cref="FontAwesomeFreeUtf8JsonStreamProvider"/> 从程序集嵌入式资源加载数据。
    /// </para>
    /// <para>
    /// 可以通过实现此接口来支持：
    /// <list type="bullet">
    ///   <item><description>从文件系统加载图标数据</description></item>
    ///   <item><description>从网络下载图标数据</description></item>
    ///   <item><description>使用 Font Awesome Pro 图标集</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// 自定义实现示例：
    /// <code language="csharp">
    /// public class FileBasedStreamProvider : IFontAwesomeUtf8JsonStreamProvider
    /// {
    ///     private readonly string _filePath;
    ///     
    ///     public FileBasedStreamProvider(string filePath)
    ///     {
    ///         _filePath = filePath;
    ///     }
    ///     
    ///     public Stream GetUtf8JsonStream()
    ///     {
    ///         return File.OpenRead(_filePath);
    ///     }
    /// }
    /// 
    /// // 使用自定义提供程序
    /// var provider = new FontAwesomeIconProvider(new FileBasedStreamProvider("icons. json"));
    /// </code>
    /// </example>
    /// <seealso cref="FontAwesomeFreeUtf8JsonStreamProvider"/>
    /// <seealso cref="FontAwesomeIconProvider"/>
    public interface IFontAwesomeUtf8JsonStreamProvider
    {
        /// <summary>
        /// 获取包含 Font Awesome 图标定义的 UTF-8 编码 JSON 数据流。
        /// </summary>
        /// <returns>
        /// 包含图标 JSON 数据的 <see cref="Stream"/>。
        /// 调用方负责在使用完毕后释放此流。
        /// </returns>
        /// <remarks>
        /// JSON 数据格式应符合 Font Awesome 图标定义结构：
        /// <code language="json">
        /// {
        ///   "icon-name": {
        ///     "svg": {
        ///       "solid": {
        ///         "path": "M.. .",
        ///         "viewBox": [0, 0, 512, 512]
        ///       }
        ///     }
        ///   }
        /// }
        /// </code>
        /// </remarks>
        Stream GetUtf8JsonStream();
    }
}
