using System;
using System.IO;
using System.Text;
using Avalonia.Platform;
using PropertyModels.Localization;

namespace RoslynPad.Models
{
    /// <summary>
    /// 基于 Avalonia 资源的本地化数据加载器。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此类继承自 <see cref="AbstractCultureData"/>，从 Avalonia 资源（avares://）中加载本地化 JSON 文件。
    /// </para>
    /// <para>
    /// 资源文件应为 UTF-8 编码的 JSON 格式，包含键值对形式的本地化字符串。
    /// </para>
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code language="csharp">
    /// // 创建并自动加载
    /// var cultureData = new AssetCultureData(
    ///     new Uri("avares://RoslynPad/Assets/Localization. zh-CN.json"),
    ///     autoLoad: true);
    /// 
    /// // 获取本地化文本
    /// var text = cultureData.LocalTexts["Welcome"];
    /// </code>
    /// </example>
    /// <seealso cref="AbstractCultureData"/>
    internal class AssetCultureData : AbstractCultureData
    {
        /// <summary>
        /// 初始化 <see cref="AssetCultureData"/> 类的新实例。
        /// </summary>
        /// <param name="uri">本地化资源的 URI（如 "avares://Assembly/Assets/file.json"）。</param>
        /// <param name="autoLoad">
        /// 若为 <see langword="true"/>，则在构造时自动加载资源；
        /// 若为 <see langword="false"/>，则需手动调用 <see cref="Reload"/>。
        /// 默认为 <see langword="false"/>。
        /// </param>
        public AssetCultureData(Uri uri, bool autoLoad = false)
            : base(uri)
        {
            if (autoLoad)
            {
                _ = Reload();
            }
        }

        /// <summary>
        /// 重新加载本地化资源数据。
        /// </summary>
        /// <returns>若加载成功返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        /// <remarks>
        /// <para>
        /// 此方法从 <see cref="AbstractCultureData. Path"/> 指定的资源 URI 加载 JSON 数据，
        /// 并解析为字符串字典存储在 <see cref="AbstractCultureData.LocalTexts"/> 中。
        /// </para>
        /// <para>
        /// 若加载过程中发生任何异常（如文件不存在、格式错误等），方法将返回 <see langword="false"/>
        /// 而不抛出异常。
        /// </para>
        /// </remarks>
        public sealed override bool Reload()
        {
            try
            {
                using var stream = AssetLoader.Open(Path);
                using var streamReader = new StreamReader(stream, Encoding.UTF8);

                var jsonContent = streamReader.ReadToEnd();
                var tempDict = ReadJsonStringDictionary(jsonContent);

                LocalTexts = tempDict;

                return LocalTexts != null;
            }
            catch
            {
                // 加载失败时返回 false，不抛出异常
                return false;
            }
        }
    }
}
