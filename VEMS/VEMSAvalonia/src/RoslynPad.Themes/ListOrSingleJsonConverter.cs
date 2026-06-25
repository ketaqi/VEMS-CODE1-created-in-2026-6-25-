using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoslynPad.Themes;

/// <summary>
/// JSON 转换器，支持将单个值或数组统一反序列化为 <see cref="List{T}"/>。
/// </summary>
/// <typeparam name="T">列表元素的类型。</typeparam>
internal class ListOrSingleJsonConverter<T> : JsonConverter<List<T>>
{
    /// <summary>
    /// 从 JSON 读取数据并转换为列表。
    /// </summary>
    /// <param name="reader">JSON 读取器。</param>
    /// <param name="typeToConvert">要转换的目标类型。</param>
    /// <param name="options">序列化选项。</param>
    /// <returns>反序列化后的列表。</returns>
    public override List<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return [];
        }
        
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            return [JsonSerializer.Deserialize<T>(ref reader, options)!];
        }

        return JsonSerializer.Deserialize<List<T>>(ref reader, options);
    }

    /// <summary>
    /// 将列表写入 JSON（不支持）。
    /// </summary>
    /// <exception cref="NotSupportedException">此方法不支持序列化。</exception>
    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options) => throw new NotSupportedException();
}
