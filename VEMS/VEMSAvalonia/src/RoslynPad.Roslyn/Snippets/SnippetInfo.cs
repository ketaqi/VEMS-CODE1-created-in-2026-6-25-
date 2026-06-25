namespace RoslynPad.Roslyn.Snippets;

/// <summary>
/// 代码片段信息模型，包含快捷方式、标题、描述等核心属性
/// </summary>
/// <param name="shortcut">代码片段快捷方式（唯一标识）</param>
/// <param name="title">代码片段标题</param>
/// <param name="description">代码片段描述信息</param>
public sealed class SnippetInfo(string shortcut, string title, string description)
{
    /// <summary>
    /// 获取代码片段的快捷方式
    /// </summary>
    public string Shortcut { get; } = shortcut;

    /// <summary>
    /// 获取代码片段的标题
    /// </summary>
    public string Title { get; } = title;

    /// <summary>
    /// 获取代码片段的描述信息
    /// </summary>
    public string Description { get; } = description;
}
