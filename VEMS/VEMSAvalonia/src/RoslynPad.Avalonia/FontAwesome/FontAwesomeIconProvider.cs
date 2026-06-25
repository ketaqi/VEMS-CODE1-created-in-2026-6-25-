using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RoslynPad.FontAwesome.Models;
using RoslynPad.IconsFront;
using RoslynPad.IconsFront.Models;

namespace RoslynPad.FontAwesome
{
    /// <summary>
    /// Font Awesome 图标提供程序，实现 <see cref="IIconProvider"/> 接口。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此类负责加载和提供 Font Awesome 图标数据。
    /// 图标数据从 JSON 资源文件中延迟加载，首次访问时进行解析。
    /// </para>
    /// <para>
    /// 支持的图标键格式：
    /// <list type="bullet">
    ///   <item><description><c>"fa-icon-name"</c> - 使用图标的第一个可用样式</description></item>
    ///   <item><description><c>"fa-solid fa-icon-name"</c> - 使用指定的 Solid 样式</description></item>
    ///   <item><description><c>"fa-regular fa-icon-name"</c> - 使用指定的 Regular 样式</description></item>
    ///   <item><description><c>"fa-brands fa-icon-name"</c> - 使用指定的 Brands 样式</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code language="csharp">
    /// var provider = new FontAwesomeIconProvider();
    /// 
    /// // 获取图标
    /// var userIcon = provider.GetIcon("fa-solid fa-user");
    /// var homeIcon = provider.GetIcon("fa-home");
    /// 
    /// // 使用图标模型渲染 SVG
    /// var viewBox = userIcon.ViewBox;
    /// var pathData = userIcon. Path. Data;
    /// </code>
    /// </example>
    /// <seealso cref="IIconProvider"/>
    /// <seealso cref="FontAwesomeIconKey"/>
    public class FontAwesomeIconProvider : IIconProvider
    {
        /// <summary>
        /// Font Awesome 图标提供程序的前缀标识符。
        /// </summary>
        private const string _faProviderPrefix = "fa";

        /// <summary>
        /// JSON 数据流提供程序。
        /// </summary>
        private readonly IFontAwesomeUtf8JsonStreamProvider _streamProvider;

        /// <summary>
        /// 延迟加载的图标字典。
        /// </summary>
        private readonly Lazy<Dictionary<string, FontAwesomeIcon>> _lazyIcons;

        /// <summary>
        /// 使用默认的 <see cref="FontAwesomeFreeUtf8JsonStreamProvider"/> 初始化 <see cref="FontAwesomeIconProvider"/> 类的新实例。
        /// </summary>
        public FontAwesomeIconProvider()
            : this(new FontAwesomeFreeUtf8JsonStreamProvider())
        {
        }

        /// <summary>
        /// 使用指定的 JSON 数据流提供程序初始化 <see cref="FontAwesomeIconProvider"/> 类的新实例。
        /// </summary>
        /// <param name="streamProvider">提供 UTF-8 编码 JSON 数据流的提供程序。</param>
        public FontAwesomeIconProvider(IFontAwesomeUtf8JsonStreamProvider streamProvider)
        {
            _streamProvider = streamProvider;
            _lazyIcons = new Lazy<Dictionary<string, FontAwesomeIcon>>(Parse);
        }

        /// <inheritdoc/>
        /// <value>返回 <c>"fa"</c>，表示 Font Awesome 图标前缀。</value>
        public string Prefix => _faProviderPrefix;

        /// <summary>
        /// 获取已解析的图标字典。
        /// </summary>
        private Dictionary<string, FontAwesomeIcon> Icons => _lazyIcons.Value;

        /// <inheritdoc/>
        /// <exception cref="KeyNotFoundException">
        /// 当图标键无法解析、图标不存在或请求的样式不可用时抛出。
        /// </exception>
        public IconModel GetIcon(string value)
        {
            // 解析图标键
            if (!FontAwesomeIconKey.TryParse(value, out FontAwesomeIconKey? key) || key is null)
            {
                throw new KeyNotFoundException($"无法解析 Font Awesome 图标键 \"{value}\"！");
            }

            // 查找图标
            if (!Icons.TryGetValue(key.Value, out FontAwesomeIcon? icon) || icon is null)
            {
                throw new KeyNotFoundException($"未找到 Font Awesome 图标 \"{key.Value}\"！");
            }

            // 未指定样式时，使用第一个可用样式
            if (!key.Style.HasValue)
            {
                return icon.Svg.Values.First().ToIconModel();
            }

            // 使用指定样式
            if (icon.Svg.TryGetValue(key.Style.Value, out Svg? svg) && svg is not null)
            {
                return svg.ToIconModel();
            }

            throw new KeyNotFoundException(
                $"图标 \"{key.Value}\" 不支持样式 \"{key.Style}\"。可能您正在尝试使用不支持的 Pro 图标。");
        }

        /// <summary>
        /// 解析 JSON 数据流并返回图标字典。
        /// </summary>
        /// <returns>图标名称到图标数据的映射字典。</returns>
        private Dictionary<string, FontAwesomeIcon> Parse()
        {
            using var stream = _streamProvider.GetUtf8JsonStream();

            var result = JsonSerializer.Deserialize(
                stream,
                FontAwesomeIconsJsonContext.Default.DictionaryStringFontAwesomeIcon);

            return result ?? new Dictionary<string, FontAwesomeIcon>();
        }
    }
}
