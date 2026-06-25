using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using RoslynPad.IconsFront;
using RoslynPad.IconsFront.Models;

namespace RoslynPad.MaterialDesign
{
    /// <summary>
    /// Material Design 图标提供程序，实现 <see cref="IIconProvider"/> 接口。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此类从程序集嵌入式 SVG 资源中加载 Material Design 图标。
    /// 图标数据会被缓存以提高后续访问性能。
    /// </para>
    /// <para>
    /// 支持的图标键格式：<c>"mdi-icon-name"</c>，
    /// 其中 <c>icon-name</c> 对应资源文件名（不含 <c>.svg</c> 扩展名）。
    /// </para>
    /// <para>
    /// 资源文件应位于：<c>{AssemblyName}.Assets.{icon-name}.svg</c>。
    /// </para>
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code language="csharp">
    /// // 注册提供程序
    /// IconProvider.Current.Register&lt;MaterialDesignIconProvider&gt;();
    /// 
    /// // 获取图标
    /// var icon = IconProvider.Current.GetIcon("mdi-account");
    /// var homeIcon = IconProvider. Current.GetIcon("mdi-home");
    /// </code>
    /// </example>
    /// <seealso cref="IIconProvider"/>
    /// <seealso cref="IconProvider"/>
    public class MaterialDesignIconProvider : IIconProvider
    {
        /// <summary>
        /// Material Design 图标提供程序的前缀标识符。
        /// </summary>
        private const string _mdiProviderPrefix = "mdi";

        /// <summary>
        /// 当前程序集引用。
        /// </summary>
        private static readonly Assembly _assembly = typeof(MaterialDesignIconProvider).Assembly;

        /// <summary>
        /// 资源名称模板。
        /// </summary>
        private static readonly string _resourceNameTemplate =
            $"{_assembly.GetName().Name}.Assets.{{0}}.svg";

        /// <summary>
        /// 用于从 SVG 中提取 viewBox 属性的正则表达式。
        /// </summary>
        private static readonly Regex _viewBoxRegex = new("viewBox=\"([0-9 -]+)\"");

        /// <summary>
        /// 用于从 SVG 中提取 path 数据的正则表达式。
        /// </summary>
        private static readonly Regex _pathRegex = new("<path d=\"(. +)\"");

        /// <summary>
        /// 图标缓存字典。
        /// </summary>
        private readonly Dictionary<string, IconModel> _icons = new();

        /// <inheritdoc/>
        /// <value>返回 <c>"mdi"</c>，表示 Material Design 图标前缀。</value>
        public string Prefix => _mdiProviderPrefix;

        /// <inheritdoc/>
        /// <exception cref="KeyNotFoundException">
        /// 当找不到指定的图标资源时抛出。
        /// </exception>
        public IconModel GetIcon(string value)
        {
            // 检查缓存
            if (_icons.TryGetValue(value, out var icon))
            {
                return icon;
            }

            // 从资源加载并缓存
            icon = GetIconFromResource(value);
            return _icons[value] = icon;
        }

        /// <summary>
        /// 从嵌入式资源加载图标数据。
        /// </summary>
        /// <param name="value">图标标识字符串（如 "mdi-account"）。</param>
        /// <returns>包含视口和路径信息的 <see cref="IconModel"/>。</returns>
        private static IconModel GetIconFromResource(string value)
        {
            using Stream stream = GetIconResourceStream(value);
            using TextReader textReader = new StreamReader(stream);

            var svg = textReader.ReadToEnd();

            // 解析 viewBox 属性
            var viewBoxMatch = _viewBoxRegex.Match(svg);
            var viewBox = viewBoxMatch.Groups[1].Value;

            // 解析 path 数据
            var pathMatch = _pathRegex.Match(svg);
            var path = pathMatch.Groups[1].Value;

            return new IconModel(
                ViewBoxModel.Parse(viewBox),
                new PathModel(path));
        }

        /// <summary>
        /// 获取图标资源流。
        /// </summary>
        /// <param name="value">图标标识字符串。</param>
        /// <returns>资源流。</returns>
        /// <exception cref="KeyNotFoundException">当找不到资源时抛出。</exception>
        private static Stream GetIconResourceStream(string value)
        {
            return TryGetIconResourceStream(value, out var stream) && stream != null
                ? stream
                : throw new KeyNotFoundException($"未找到 Material Design 图标 \"{value}\"！");
        }

        /// <summary>
        /// 尝试获取图标资源流。
        /// </summary>
        /// <param name="value">图标标识字符串。</param>
        /// <param name="stream">成功时返回资源流；失败时返回 <see langword="null"/>。</param>
        /// <returns>若成功获取资源流返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        private static bool TryGetIconResourceStream(string value, out Stream? stream)
        {
            stream = default;

            // 验证值长度（需要包含前缀和至少一个字符的图标名）
            if (value.Length <= _mdiProviderPrefix.Length + 1)
            {
                return false;
            }

            // 移除前缀（"mdi-"）
            var withoutPrefix = value.Substring(_mdiProviderPrefix.Length + 1);

            // 构建资源名称
            const string resourceNameFormat = "{0}.Assets.{1}.svg";
            var resourceName = string.Format(
                CultureInfo.InvariantCulture,
                resourceNameFormat,
                _assembly.GetName().Name,
                withoutPrefix);

            stream = _assembly.GetManifestResourceStream(resourceName);

            return stream != null;
        }
    }
}
