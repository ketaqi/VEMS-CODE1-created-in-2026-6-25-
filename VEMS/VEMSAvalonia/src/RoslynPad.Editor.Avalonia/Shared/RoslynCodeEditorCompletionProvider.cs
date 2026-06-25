using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using RoslynPad.Roslyn;
using RoslynPad.Roslyn.SignatureHelp;
using RoslynPad.Roslyn.Snippets;

namespace RoslynPad.Editor;

/// <summary>
/// 基于Roslyn实现的代码编辑器补全提供程序，负责处理代码补全和签名帮助功能
/// </summary>
public sealed class RoslynCodeEditorCompletionProvider : ICodeEditorCompletionProvider
{
    /// <summary>
    /// 标记是否已初始化补全提供程序，用于应用域内仅初始化一次
    /// </summary>
    private static bool s_initialized;

    /// <summary>
    /// 当前文档的唯一标识
    /// </summary>
    private readonly DocumentId _documentId;

    /// <summary>
    /// Roslyn宿主服务，提供文档、代码分析等核心能力
    /// </summary>
    private readonly IRoslynHost _roslynHost;

    /// <summary>
    /// 代码片段信息服务，用于处理代码片段补全
    /// </summary>
    private readonly SnippetInfoService _snippetService;

    /// <summary>
    /// 初始化 <see cref="RoslynCodeEditorCompletionProvider"/> 实例
    /// </summary>
    /// <param name="documentId">当前文档ID</param>
    /// <param name="roslynHost">Roslyn宿主服务实例</param>
    public RoslynCodeEditorCompletionProvider(DocumentId documentId, IRoslynHost roslynHost)
    {
        _documentId = documentId;
        _roslynHost = roslynHost;
        _snippetService = (SnippetInfoService)_roslynHost.GetService<ISnippetInfoService>();
    }

    /// <summary>
    /// 预热补全提供程序，提前加载相关服务以加快首次输入响应速度
    /// </summary>
    internal void Warmup()
    {
        if (s_initialized) return;

        s_initialized = true;

        Task.Run(() =>
        {
            var document = _roslynHost.GetDocument(_documentId);
            if (document == null)
            {
                return;
            }

            // 预热补全服务
            var completionService = CompletionService.GetService(document);
            completionService?.GetCompletionsAsync(document, 0);

            // 预热签名帮助服务
            var signatureHelpProvider = _roslynHost.GetService<ISignatureHelpProvider>();
            signatureHelpProvider.GetItemsAsync(document, 0,
                new SignatureHelpTriggerInfo(SignatureHelpTriggerReason.InvokeSignatureHelpCommand));
        });
    }

    /// <summary>
    /// 获取指定位置的代码补全数据
    /// </summary>
    /// <param name="position">补全触发的文本偏移位置</param>
    /// <param name="triggerChar">触发补全的字符（如"."、"("等），可为null</param>
    /// <param name="useSignatureHelp">是否强制使用签名帮助</param>
    /// <returns>包含补全项、重载提供程序、选择模式的补全结果</returns>
    public async Task<CompletionResult> GetCompletionData(int position, char? triggerChar, bool useSignatureHelp)
    {
        IList<ICompletionDataEx>? completionData = null;
        IOverloadProviderEx? overloadProvider = null;
        var useHardSelection = true;

        var document = _roslynHost.GetDocument(_documentId);
        if (document == null)
        {
            return new CompletionResult(null, null, false);
        }

        // 处理签名帮助（方法参数提示）
        if (useSignatureHelp || triggerChar != null)
        {
            var signatureHelpProvider = _roslynHost.GetService<ISignatureHelpProvider>();
            var isSignatureHelp = useSignatureHelp || signatureHelpProvider.IsTriggerCharacter(triggerChar.GetValueOrDefault());
            if (isSignatureHelp)
            {
                var signatureHelp = await signatureHelpProvider.GetItemsAsync(
                    document,
                    position,
                    new SignatureHelpTriggerInfo(
                        useSignatureHelp
                            ? SignatureHelpTriggerReason.InvokeSignatureHelpCommand
                            : SignatureHelpTriggerReason.TypeCharCommand, triggerChar))
                    .ConfigureAwait(false);
                if (signatureHelp != null)
                {
                    overloadProvider = new RoslynOverloadProvider(signatureHelp);
                }
            }
        }

        // 若签名帮助未命中，则获取普通代码补全项
        if (overloadProvider == null && CompletionService.GetService(document) is { } completionService)
        {
            var completionTrigger = GetCompletionTrigger(triggerChar);
            var data = await completionService.GetCompletionsAsync(
                document,
                position,
                completionTrigger
                ).ConfigureAwait(false);

            if (data != null && data.ItemsList.Any())
            {
                useHardSelection = data.SuggestionModeItem == null;
                var text = await document.GetTextAsync().ConfigureAwait(false);
                var textSpanToText = new Dictionary<TextSpan, string>();

                // 筛选并转换补全项为编辑器可识别的格式
                completionData = data.ItemsList
                    .Where(item => MatchesFilterText(completionService, document, item, text, textSpanToText))
                    .Select(item => new RoslynCompletionData(document, item, _snippetService.SnippetManager))
                        .ToArray<ICompletionDataEx>();
            }
            else
            {
                completionData = Array.Empty<ICompletionDataEx>();
            }
        }

        return new CompletionResult(completionData, overloadProvider, useHardSelection);
    }

    /// <summary>
    /// 检查补全项是否匹配筛选文本
    /// </summary>
    /// <param name="completionService">Roslyn补全服务</param>
    /// <param name="document">当前文档</param>
    /// <param name="item">待检查的补全项</param>
    /// <param name="text">文档源文本</param>
    /// <param name="textSpanToText">文本跨度与文本的缓存字典</param>
    /// <returns>匹配返回true，否则返回false</returns>
    private static bool MatchesFilterText(CompletionService completionService, Document document, CompletionItem item, SourceText text, Dictionary<TextSpan, string> textSpanToText)
    {
        var filterText = GetFilterText(item, text, textSpanToText);
        if (string.IsNullOrEmpty(filterText)) return true;
        return completionService.FilterItems(document, [item], filterText).Length > 0;
    }

    /// <summary>
    /// 获取补全项的筛选文本（从文本跨度中提取）
    /// </summary>
    /// <param name="item">补全项</param>
    /// <param name="text">文档源文本</param>
    /// <param name="textSpanToText">文本跨度与文本的缓存字典</param>
    /// <returns>补全项对应的筛选文本</returns>
    private static string GetFilterText(CompletionItem item, SourceText text, Dictionary<TextSpan, string> textSpanToText)
    {
        var textSpan = item.Span;
        if (!textSpanToText.TryGetValue(textSpan, out var filterText))
        {
            filterText = text.GetSubText(textSpan).ToString();
            textSpanToText[textSpan] = filterText;
        }
        return filterText;
    }

    /// <summary>
    /// 根据触发字符创建补全触发器
    /// </summary>
    /// <param name="triggerChar">触发字符（可为null）</param>
    /// <returns>Roslyn补全触发器实例</returns>
    private static CompletionTrigger GetCompletionTrigger(char? triggerChar)
    {
        return triggerChar != null
            ? CompletionTrigger.CreateInsertionTrigger(triggerChar.Value)
            : CompletionTrigger.Invoke;
    }
}
