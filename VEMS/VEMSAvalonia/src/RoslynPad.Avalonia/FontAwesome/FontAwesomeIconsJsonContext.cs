using System.Collections.Generic;
using System.Text.Json.Serialization;
using RoslynPad.FontAwesome.Models;

namespace RoslynPad.FontAwesome
{
    /// <summary>
    /// Font Awesome 图标 JSON 序列化上下文。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此类使用 . NET 源生成器为 Font Awesome 图标数据提供高性能的 JSON 序列化支持。
    /// </para>
    /// <para>
    /// 源生成的序列化器避免了运行时反射，提供更好的性能和 AOT 编译兼容性。
    /// </para>
    /// </remarks>
    /// <seealso cref="FontAwesomeIconProvider"/>
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(Dictionary<string, FontAwesomeIcon>))]
    internal partial class FontAwesomeIconsJsonContext : JsonSerializerContext
    {
    }
}
