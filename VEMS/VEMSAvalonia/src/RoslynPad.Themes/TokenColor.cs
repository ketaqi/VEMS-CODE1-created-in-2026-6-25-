using System.Text.Json.Serialization;

namespace RoslynPad.Themes;

/// <summary>
/// 表示语法高亮标记的颜色配置。
/// </summary>
/// <param name="Scope">适用的语法作用域列表。</param>
/// <param name="Settings">颜色和样式设置。</param>
public record TokenColor(
    [property: JsonConverter(typeof(ListOrSingleJsonConverter<string>))] List<string>? Scope,
    TokenColorSettings? Settings
);
