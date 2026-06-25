using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.CodeAnalysis.Classification;
using RoslynPad.Roslyn.Classification;
using RoslynPad.Themes;

namespace RoslynPad.Editor;

/// <summary>
/// 基于 VS Code 主题的语法分类高亮颜色提供器。
/// </summary>
/// <remarks>
/// 此类从 VS Code 主题文件中读取颜色配置，并将其映射到 Roslyn 分类类型名称。
/// 支持从嵌入的 scopes-vscode.json 和 scopes-roslyn.json 文件加载作用域映射。
/// </remarks>
public class ThemeClassificationColors : IClassificationHighlightColors
{
    private static readonly ImmutableArray<(string classification, string[] scopes)> s_classifiedScopes = GetClassifiedScopes();

    private static readonly JsonSerializerOptions s_serializerOptions = new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private readonly Dictionary<string, HighlightingColor> _colors;

    /// <summary>
    /// 获取或设置静态符号的高亮颜色。
    /// </summary>
    public HighlightingColor StaticSymbolColor { get; protected set; } = new();

    /// <summary>
    /// 获取括号匹配的高亮颜色。
    /// </summary>
    public HighlightingColor BraceMatchingColor { get; protected set; }

    /// <summary>
    /// 使用指定的主题初始化 <see cref="ThemeClassificationColors"/> 类的新实例。
    /// </summary>
    /// <param name="theme">VS Code 主题对象。</param>
    public ThemeClassificationColors(Theme theme)
    {
        BraceMatchingColor = new HighlightingColor
        {
            Background = new SimpleHighlightingBrush(theme.Type == ThemeType.Dark ? Color.FromArgb(60, 200, 200, 200) : Color.FromArgb(150, 219, 224, 204))
        }.AsFrozen();

        DefaultBrush = GetColorFromTheme(theme, "editor.foreground");

        _colors = s_classifiedScopes
            .Select(t => (t.classification, color: GetColorForScopes(theme, t.scopes, DefaultBrush)))
            .Append((classification: ClassificationTypeNames.StaticSymbol, color: StaticSymbolColor))
            .Append((classification: AdditionalClassificationTypeNames.BraceMatching, color: BraceMatchingColor))
            .ToDictionary(t => t.classification, t => t.color, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 获取分类作用域映射。
    /// </summary>
    private static ImmutableArray<(string classification, string[] scopes)> GetClassifiedScopes()
    {
        var vsCodeScopes = ReadScopes("scopes-vscode");
        var roslynScopes = ReadScopes("scopes-roslyn");

        var scopes = new Dictionary<string, string[]>(vsCodeScopes, StringComparer.OrdinalIgnoreCase);
        foreach (var scope in roslynScopes)
        {
            scopes[scope.Key] = scope.Value;
        }

        var classificationsMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var classification in SemanticTokensSchema.ClassificationTypeNameToTokenName.Concat(SemanticTokensSchema.ClassificationTypeNameToCustomTokenName))
        {
            classificationsMap[classification.Value] = classification.Key;
        }

        return scopes.Select(d => (name: d.Key, found: classificationsMap.TryGetValue(d.Key, out var classification), classification: classification!, scopes: d.Value))
            .Where(d => d.found)
            .Select(d => (d.classification, d.scopes))
            .ToImmutableArray();
    }

    /// <summary>
    /// 从嵌入资源读取作用域映射。
    /// </summary>
    private static Dictionary<string, string[]> ReadScopes(string name) => DeserializeResource<Dictionary<string, string[]>>(name);

    /// <summary>
    /// 反序列化嵌入资源。
    /// </summary>
    private static T DeserializeResource<T>(string name)
    {
        using var stream = typeof(ThemeClassificationColors).Assembly.GetManifestResourceStream($"RoslynPad.Editor.Shared.Resources.{name}.json")
            ?? throw new InvalidOperationException("Stream not found");
        return JsonSerializer.Deserialize<T>(stream, s_serializerOptions)
            ?? throw new InvalidOperationException($"Empty {name}.json");
    }

    /// <summary>
    /// 从主题获取指定名称的颜色。
    /// </summary>
    private static HighlightingColor GetColorFromTheme(Theme theme, string name) => new HighlightingColor
    {
        Foreground = theme.TryGetColor(name) is { } value ? ParseBrush(value) : null
    }.AsFrozen();

    /// <summary>
    /// 根据作用域获取颜色。
    /// </summary>
    private static HighlightingColor GetColorForScopes(Theme theme, string[] scopes, HighlightingColor defaultColor) =>
        scopes.Select(theme.TryGetScopeSettings).FirstOrDefault(s => s is not null) is { } scopeSettings
        ? new HighlightingColor
        {
            FontWeight = scopeSettings.Value.FontStyle?.Contains("bold", StringComparison.OrdinalIgnoreCase) == true ? FontWeights.Bold : null,
            FontStyle = scopeSettings.Value.FontStyle?.Contains("italic", StringComparison.OrdinalIgnoreCase) == true ? FontStyles.Italic : null,
            Foreground = ParseBrush(scopeSettings.Value.Foreground)
        }.AsFrozen()
        : defaultColor;

    /// <summary>
    /// 解析颜色字符串为画刷。
    /// </summary>
    private static SimpleHighlightingBrush? ParseBrush(string? value) => value is null ? null : new(Parsers.ParseColor(value));

    /// <summary>
    /// 获取默认画刷。
    /// </summary>
    public HighlightingColor DefaultBrush { get; }

    /// <summary>
    /// 根据分类类型名称获取画刷。
    /// </summary>
    /// <param name="classificationTypeName">分类类型名称。</param>
    /// <returns>对应的高亮颜色。</returns>
    public HighlightingColor GetBrush(string classificationTypeName) =>
        _colors.TryGetValue(classificationTypeName, out var color) ? color : DefaultBrush;
}
