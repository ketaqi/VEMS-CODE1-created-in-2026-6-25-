#if !NET6_0_OR_GREATER
using System;
using System.Xml;

namespace RoslynPad.Runtime;

/// <summary>
/// 提供 <see cref="XmlDictionaryWriter"/> 的扩展方法，用于简化 JSON 序列化。
/// </summary>
/// <remarks>
/// 此类仅用于 .NET Standard 2.0 和 .NET Framework，
/// 因为这些平台不支持 System.Text.Json，需要使用 DataContractJsonSerializer 的底层写入器。
/// </remarks>
internal static class XmlDictionaryWriterExtensions
{
    /// <summary>
    /// 开始写入 JSON 对象，并返回一个可释放的包装器用于自动结束元素。
    /// </summary>
    /// <param name="jsonWriter">JSON 写入器。</param>
    /// <param name="name">对象名称，默认为 "root"。</param>
    /// <returns>可释放的元素包装器，释放时自动写入结束元素。</returns>
    /// <example>
    /// <code>
    /// using var _ = jsonWriter.WriteObject();
    /// jsonWriter.WriteProperty("name", "value");
    /// // 离开 using 作用域时自动写入结束元素
    /// </code>
    /// </example>
    public static ElementDisposer WriteObject(this XmlDictionaryWriter jsonWriter, string? name = null)
    {
        jsonWriter.WriteStartElement(name ?? "root", "");
        jsonWriter.WriteAttributeString("type", "object");
        return new ElementDisposer(jsonWriter);
    }

    /// <summary>
    /// 开始写入 JSON 数组，并返回一个可释放的包装器用于自动结束元素。
    /// </summary>
    /// <param name="jsonWriter">JSON 写入器。</param>
    /// <param name="name">数组属性名称。</param>
    /// <returns>可释放的元素包装器，释放时自动写入结束元素。</returns>
    /// <example>
    /// <code>
    /// using var _ = jsonWriter.WriteArray("items");
    /// // 写入数组元素...
    /// // 离开 using 作用域时自动写入结束元素
    /// </code>
    /// </example>
    public static ElementDisposer WriteArray(this XmlDictionaryWriter jsonWriter, string name)
    {
        jsonWriter.WriteStartElement(name);
        jsonWriter.WriteAttributeString("type", "array");
        return new ElementDisposer(jsonWriter);
    }

    /// <summary>
    /// 写入字符串类型的 JSON 属性。
    /// </summary>
    /// <param name="jsonWriter">JSON 写入器。</param>
    /// <param name="name">属性名称。</param>
    /// <param name="value">属性值。</param>
    public static void WriteProperty(this XmlDictionaryWriter jsonWriter, string name, string? value) =>
        jsonWriter.WriteElementString(name, value);

    /// <summary>
    /// 写入整数类型的 JSON 属性。
    /// </summary>
    /// <param name="jsonWriter">JSON 写入器。</param>
    /// <param name="name">属性名称。</param>
    /// <param name="value">属性值。</param>
    public static void WriteProperty(this XmlDictionaryWriter jsonWriter, string name, int value)
    {
        jsonWriter.WriteStartElement(name);
        jsonWriter.WriteValue(value);
        jsonWriter.WriteEndElement();
    }

    /// <summary>
    /// 写入双精度浮点数类型的 JSON 属性。
    /// </summary>
    /// <param name="jsonWriter">JSON 写入器。</param>
    /// <param name="name">属性名称。</param>
    /// <param name="value">属性值。</param>
    public static void WriteProperty(this XmlDictionaryWriter jsonWriter, string name, double value)
    {
        jsonWriter.WriteStartElement(name);
        jsonWriter.WriteValue(value);
        jsonWriter.WriteEndElement();
    }

    /// <summary>
    /// 写入布尔类型的 JSON 属性。
    /// </summary>
    /// <param name="jsonWriter">JSON 写入器。</param>
    /// <param name="name">属性名称。</param>
    /// <param name="value">属性值。</param>
    public static void WriteProperty(this XmlDictionaryWriter jsonWriter, string name, bool value)
    {
        jsonWriter.WriteStartElement(name);
        jsonWriter.WriteValue(value);
        jsonWriter.WriteEndElement();
    }

    /// <summary>
    /// 可释放的 XML 元素包装器，用于实现 RAII 模式自动关闭元素。
    /// </summary>
    /// <param name="writer">XML 写入器。</param>
    public readonly struct ElementDisposer(XmlDictionaryWriter writer) : IDisposable
    {
        private readonly XmlDictionaryWriter _writer = writer;

        /// <summary>
        /// 写入元素的结束标记。
        /// </summary>
        public void Dispose() => _writer.WriteEndElement();
    }
}
#endif
