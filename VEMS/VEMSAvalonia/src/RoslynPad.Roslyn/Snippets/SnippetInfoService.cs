using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host.Mef;

namespace RoslynPad.Roslyn.Snippets;

/// <summary>
/// C# 语言代码片段信息服务的实现类，适配 Roslyn 框架的代码片段信息服务接口
/// </summary>
[ExportLanguageService(typeof(Microsoft.CodeAnalysis.Snippets.ISnippetInfoService), LanguageNames.CSharp)]
[method: ImportingConstructor]
internal sealed class SnippetInfoService([Import(AllowDefault = true)] ISnippetInfoService inner) : Microsoft.CodeAnalysis.Snippets.ISnippetInfoService
{
    /// <summary>
    /// 获取可用的代码片段信息集合（若存在内部服务则适配转换，否则返回空集合）
    /// </summary>
    /// <returns>适配后的 Roslyn 代码片段信息枚举集合</returns>
    public IEnumerable<Microsoft.CodeAnalysis.Snippets.SnippetInfo> GetSnippetsIfAvailable()
    {
        return inner?.GetSnippets().Select(x =>
            new Microsoft.CodeAnalysis.Snippets.SnippetInfo(x.Shortcut, x.Title, x.Description, ""))
            ?? [];
    }

    /// <summary>
    /// 非阻塞式检查代码片段快捷方式是否存在（当前实现固定返回 false）
    /// </summary>
    /// <param name="shortcut">要检查的代码片段快捷方式</param>
    /// <returns>始终返回 false</returns>
    public bool SnippetShortcutExists_NonBlocking(string? shortcut)
    {
        return false;
    }

    /// <summary>
    /// 判断是否应对指定代码片段进行格式化（当前实现固定返回 false）
    /// </summary>
    /// <param name="snippetInfo">代码片段信息实例</param>
    /// <returns>始终返回 false</returns>
    public bool ShouldFormatSnippet(Microsoft.CodeAnalysis.Snippets.SnippetInfo snippetInfo)
    {
        return false;
    }
}
