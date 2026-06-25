using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace RoslynPad.Editor;

/// <summary>
/// 代码片段管理器，提供内置的代码片段定义。
/// </summary>
/// <remarks>
/// 包含常用的 C# 代码片段，如 for、foreach、if、while 等。
/// 某些片段仅在特定平台上可用（如 Windows 的 WPF 片段）。
/// </remarks>
internal sealed class SnippetManager
{
    internal readonly ImmutableDictionary<string, CodeSnippet> _defaultSnippets;

    /// <summary>
    /// 初始化 <see cref="SnippetManager"/> 类的新实例。
    /// </summary>
    public SnippetManager()
    {
        var snippets = GetGeneralSnippets();
        snippets.AddRange(GetPlatformSnippets());

        _defaultSnippets = snippets.ToImmutableDictionary(x => x.Name);
    }

    /// <summary>
    /// 获取所有可用的代码片段。
    /// </summary>
    public IEnumerable<CodeSnippet> Snippets => _defaultSnippets.Values;

    /// <summary>
    /// 根据名称查找代码片段。
    /// </summary>
    /// <param name="name">片段名称。</param>
    /// <returns>找到的代码片段，未找到则返回 null。</returns>
    public CodeSnippet? FindSnippet(string name)
    {
        _defaultSnippets.TryGetValue(name, out var snippet);
        return snippet;
    }

    /// <summary>
    /// 获取通用代码片段（跨平台）。
    /// </summary>
    private List<CodeSnippet> GetGeneralSnippets()
    {
        var snippets = new List<CodeSnippet>
        {
            new("for", "for loop",
                "for (int ${counter=i} = 0; ${counter} < ${end}; ${counter}++)\n{\n\t${Selection}\n}",
                "for"),
            new("foreach", "foreach loop",
                "foreach (${var} ${element} in ${collection})\n{\n\t${Selection}\n}",
                "foreach"),
            new("if", "if statement",
                "if (${condition})\n{\n\t${Selection}\n}",
                "if"),
            new("ifnull", "if-null statement",
                "if (${condition} == null)\n{\n\t${Selection}\n}",
                "if"),
            new("ifnotnull", "if-not-null statement",
                "if (${condition} != null)\n{\n\t${Selection}\n}",
                "if"),
            new("ifelse", "if-else statement",
                "if (${condition})\n{\n\t${Selection}\n}\nelse\n{\n\t${Caret}\n}",
                "if"),
            new("while", "while loop",
                "while (${condition})\n{\n\t${Selection}\n}",
                "while"),
            new("prop", "Property",
                "public ${Type=object} ${Property=Property} { get; set; }${Caret}",
                "event"),
            new("propg", "Property with private setter",
                "public ${Type=object} ${Property=Property} { get; private set; }${Caret}",
                "event"),
            new("propfull", "Property with backing field",
                "${type} ${toFieldName(name)};\n\npublic ${type=int} ${name=Property}\n{\n\tget { return ${toFieldName(name)}; }\n\tset { ${toFieldName(name)} = value; }\n}${Caret}",
                "event"),
            new("propdp", "Dependency Property",
                "public static readonly DependencyProperty ${name}Property =" + Environment.NewLine
                + "\tDependencyProperty.Register(\"${name}\", typeof(${type}), typeof(${ClassName})," + Environment.NewLine
                + "\t                            new FrameworkPropertyMetadata());" + Environment.NewLine
                + "" + Environment.NewLine
                + "public ${type=int} ${name=Property}\n{" + Environment.NewLine
                + "\tget { return (${type})GetValue(${name}Property); }" + Environment.NewLine
                + "\tset { SetValue(${name}Property, value); }"
                + Environment.NewLine + "}${Caret}",
                "event"),
            new("switch", "Switch statement",
                "switch (${condition})\n{\n\t${Caret}\n}",
                "switch"),
            new("try", "Try-catch statement",
                "try\n{\n\t${Selection}\n}\ncatch (Exception)\n{\n\t${Caret}\n\tthrow;\n}",
                "try"),
            new("trycf", "Try-catch-finally statement",
                "try\n{\n\t${Selection}\n}\ncatch (Exception)\n{\n\t${Caret}\n\tthrow;\n}\nfinally\n{\n\t\n}",
                "try"),
            new("tryf", "Try-finally statement",
                "try\n{\n\t${Selection}\n}\nfinally\n{\n\t${Caret}\n}",
                "try"),
            new("using", "Using statement",
                "using (${resource=null})\n{\n\t${Selection}\n}",
                "try"),
            new("cw", "Console.WriteLine",
                "Console.WriteLine(${Selection})",
                "if")
        };
        return snippets;
    }

    /// <summary>
    /// 获取平台特定的代码片段。
    /// </summary>
    private List<CodeSnippet> GetPlatformSnippets()
    {
        var snippets = new List<CodeSnippet>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            snippets.AddRange(GetWindowsSnippets());
        }
        return snippets;
    }

    /// <summary>
    /// 获取 Windows 平台特定的代码片段。
    /// </summary>
    private List<CodeSnippet> GetWindowsSnippets()
    {
        return
        [
            new CodeSnippet(
                "desktopapp",
                "#r Framework-include and await Helpers.RunWpfAsync()",
                "#r \"framework: Microsoft.WindowsDesktop.App\"\nawait Helpers.RunWpfAsync();\n\n${Selection}",
                "#r")
        ];
    }
}
