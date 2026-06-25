using System.Text.Json;

namespace RoslynPad.Themes;

/// <summary>
/// Visual Studio Code 主题文件读取器，用于解析 VS Code 兼容的 JSON 主题文件。
/// </summary>
public class VsCodeThemeReader : IThemeReader
{
    private static readonly JsonSerializerOptions s_serializerOptions = new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private static readonly Lazy<Task<Theme>> s_vsDarkTheme = new(() => ReadThemeEmebeddedResourceAsync("vs2019_dark"));
    private static readonly Lazy<Task<Theme>> s_vsLightTheme = new(() => ReadThemeEmebeddedResourceAsync("vs2019_light"));

    /// <summary>
    /// 异步读取主题文件，包括继承的基础主题。
    /// </summary>
    /// <param name="file">主题文件路径。</param>
    /// <param name="type">主题类型。</param>
    /// <returns>合并后的完整主题对象。</returns>
    public async Task<Theme> ReadThemeAsync(string file, ThemeType type)
    {
        var themes = new Stack<Theme>();
        var originTheme = await ReadThemeFileAsync(file).ConfigureAwait(false);
        themes.Push(originTheme);

        var includeTheme = originTheme;
        while (includeTheme.Include is not null)
        {
            var includePath = Path.Combine(Path.GetDirectoryName(file).NotNull(), includeTheme.Include);
            includeTheme = await ReadThemeFileAsync(includePath).ConfigureAwait(false);
            themes.Push(includeTheme);
        }

        var baseTheme = await (type == ThemeType.Dark ? s_vsDarkTheme.Value : s_vsLightTheme.Value).ConfigureAwait(false);
        themes.Push(baseTheme);

        var theme = new Theme(new VsCodeColorRegistry())
        {
            Name = originTheme.Name,
            Colors = [],
            TokenColors = [],
        };

        while (themes.TryPop(out var nextTheme))
        {
            if (nextTheme.Colors is not null)
            {
                foreach (var color in nextTheme.Colors)
                {
                    theme.Colors[color.Key] = color.Value;
                }
            }

            if (nextTheme.TokenColors is not null)
            {
                foreach (var tokenColor in nextTheme.TokenColors)
                {
                    theme.TokenColors.Add(tokenColor);
                }
            }
        }

        theme.TokenColors.Reverse();

        foreach (var tokenColor in theme.TokenColors)
        {
            if (tokenColor.Settings is null || tokenColor.Scope is null)
            {
                continue;
            }

            foreach (var scope in tokenColor.Scope)
            {
                theme.ScopeSettings.TryAdd(scope, tokenColor.Settings);
            }
        }

        theme.Type = type;
        return theme;
    }

    /// <summary>
    /// 从文件系统读取主题文件。
    /// </summary>
    private static async Task<Theme> ReadThemeFileAsync(string file)
    {
        using var stream = File.OpenRead(file);
        return await ReadThemeAsync(stream).ConfigureAwait(false);
    }

    /// <summary>
    /// 从程序集嵌入资源读取主题。
    /// </summary>
    private static async Task<Theme> ReadThemeEmebeddedResourceAsync(string name)
    {
        using var stream = typeof(VsCodeThemeReader).Assembly.GetManifestResourceStream($"RoslynPad.Themes.Themes.{name}.json")
            ?? throw new InvalidOperationException("Stream not found");
        return await ReadThemeAsync(stream).ConfigureAwait(false);
    }

    /// <summary>
    /// 从流中异步反序列化主题。
    /// </summary>
    private static async Task<Theme> ReadThemeAsync(Stream stream)
    {
        var theme = await JsonSerializer.DeserializeAsync<Theme>(stream, s_serializerOptions).ConfigureAwait(false);
        return theme.NotNull();
    }
}
