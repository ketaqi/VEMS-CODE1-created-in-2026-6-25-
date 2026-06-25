namespace RoslynPad.Roslyn.Snippets;

/// <summary>
/// 代码片段信息服务接口，定义获取代码片段信息的契约
/// </summary>
public interface ISnippetInfoService
{
    /// <summary>
    /// 获取所有代码片段信息的集合
    /// </summary>
    /// <returns>包含所有代码片段信息的枚举集合</returns>
    IEnumerable<SnippetInfo> GetSnippets();
}
