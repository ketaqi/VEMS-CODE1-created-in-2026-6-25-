using System.IO;
using System.Reflection;

namespace RoslynPad.FontAwesome
{
    /// <summary>
    /// 提供 Font Awesome Free 图标 JSON 数据流的提供程序。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此类从当前程序集的嵌入式资源中读取 Font Awesome 图标定义 JSON 文件。
    /// </para>
    /// <para>
    /// 资源文件应位于：<c>{AssemblyName}.FontAwesome.Assets.icons.json</c>。
    /// </para>
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code language="csharp">
    /// var streamProvider = new FontAwesomeFreeUtf8JsonStreamProvider();
    /// using var stream = streamProvider.GetUtf8JsonStream();
    /// // 反序列化 JSON 数据... 
    /// </code>
    /// </example>
    /// <seealso cref="IFontAwesomeUtf8JsonStreamProvider"/>
    /// <seealso cref="FontAwesomeIconProvider"/>
    public sealed class FontAwesomeFreeUtf8JsonStreamProvider : IFontAwesomeUtf8JsonStreamProvider
    {
        /// <summary>
        /// 获取包含 Font Awesome 图标定义的 UTF-8 编码 JSON 数据流。
        /// </summary>
        /// <returns>包含图标 JSON 数据的 <see cref="Stream"/>。调用方负责释放此流。</returns>
        /// <exception cref="FileNotFoundException">
        /// 当在程序集中找不到嵌入式资源文件时抛出。
        /// </exception>
        /// <remarks>
        /// 返回的流是从程序集嵌入式资源中打开的，
        /// 调用方应在使用完毕后调用 <see cref="Stream. Dispose"/> 释放资源。
        /// </remarks>
        public Stream GetUtf8JsonStream()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{assembly.GetName().Name}.FontAwesome.Assets.icons.json";
            var stream = assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                throw new FileNotFoundException(
                    $"在程序集 '{assembly.FullName}' 中找不到资源 '{resourceName}'。");
            }

            return stream;
        }
    }
}
