using System.Composition;
using RoslynPad.Roslyn.Snippets;

namespace RoslynPad.Editor;

/// <summary>
/// 代码片段信息服务，提供可用的代码片段列表。
/// </summary>
/// <remarks>
/// 此服务通过 MEF 导出，供 Roslyn 的代码补全系统使用。
/// </remarks>
[Export(typeof(ISnippetInfoService)), Shared]
internal sealed class SnippetInfoService : ISnippetInfoService
{
    /// <summary>
    /// 获取代码片段管理器。
    /// </summary>
    public SnippetManager SnippetManager { get; } = new SnippetManager();

    /// <summary>
    /// 获取所有可用的代码片段。
    /// </summary>
    /// <returns>代码片段信息的枚举。</returns>
    public IEnumerable<SnippetInfo> GetSnippets()
    {
        return SnippetManager.Snippets.Select(x => new SnippetInfo(x.Name, x.Name, x.Description));
    }
}
