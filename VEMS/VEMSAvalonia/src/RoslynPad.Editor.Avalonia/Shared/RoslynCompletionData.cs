using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using RoslynPad.Roslyn;
using RoslynPad.Roslyn.Completion;

namespace RoslynPad.Editor;

/// <summary>
/// 基于Roslyn的补全项数据实现，封装Roslyn补全项并适配编辑器补全逻辑
/// </summary>
internal sealed class RoslynCompletionData : ICompletionDataEx, INotifyPropertyChanged
{
    /// <summary>
    /// 当前文档实例
    /// </summary>
    private readonly Document _document;

    /// <summary>
    /// Roslyn原生补全项
    /// </summary>
    private readonly CompletionItem _item;

    /// <summary>
    /// 代码片段管理器，用于处理代码片段补全
    /// </summary>
    private readonly SnippetManager _snippetManager;

    /// <summary>
    /// 补全项对应的图标标识
    /// </summary>
    private readonly Glyph _glyph;

    /// <summary>
    /// 延迟加载的描述信息任务
    /// </summary>
    private readonly Lazy<Task> _descriptionTask;

    /// <summary>
    /// 补全项的描述装饰器
    /// </summary>
    private Decorator? _description;

    /// <summary>
    /// 初始化 <see cref="RoslynCompletionData"/> 实例
    /// </summary>
    /// <param name="document">当前文档</param>
    /// <param name="item">Roslyn原生补全项</param>
    /// <param name="snippetManager">代码片段管理器</param>
    public RoslynCompletionData(Document document, CompletionItem item, SnippetManager snippetManager)
    {
        _document = document;
        _item = item;
        _snippetManager = snippetManager;
        Text = item.DisplayTextPrefix + item.DisplayText + item.DisplayTextSuffix;
        Content = Text;
        _glyph = item.GetGlyph();
        _descriptionTask = new Lazy<Task>(RetrieveDescription);
    }

    /// <summary>
    /// 完成补全项的插入操作
    /// </summary>
    /// <param name="textArea">编辑器文本区域</param>
    /// <param name="completionSegment">补全对应的文本段</param>
    /// <param name="e">事件参数</param>
    public async void Complete(TextArea textArea, ISegment completionSegment, EventArgs e)
    {
        // 优先处理代码片段补全
        if (_glyph == Glyph.Snippet && CompleteSnippet(textArea, completionSegment, e) ||
            CompletionService.GetService(_document) is not { } completionService)
        {
            return;
        }

        // 获取Roslyn计算的文本变更
        var changes = await completionService.GetChangeAsync(_document, _item, null).ConfigureAwait(true);

        var textChange = changes.TextChange;
        var document = textArea.Document;
        using (document.RunUpdate())
        {
            // 移除补全窗口打开期间输入的多余字符（Roslyn文档未实时更新）
            if (completionSegment.EndOffset > textChange.Span.End)
            {
                document.Replace(
                    new TextSegment { StartOffset = textChange.Span.End, EndOffset = completionSegment.EndOffset },
                    string.Empty);
            }

            // 应用补全文本变更
            document.Replace(textChange.Span.Start, textChange.Span.Length,
                new StringTextSource(textChange.NewText));
        }

        // 更新光标位置
        if (changes.NewPosition != null)
        {
            textArea.Caret.Offset = changes.NewPosition.Value;
        }
    }

    /// <summary>
    /// 处理代码片段的补全插入
    /// </summary>
    /// <param name="textArea">编辑器文本区域</param>
    /// <param name="completionSegment">补全对应的文本段</param>
    /// <param name="e">事件参数</param>
    /// <returns>成功插入返回true，否则返回false</returns>
    private bool CompleteSnippet(TextArea textArea, ISegment completionSegment, EventArgs e)
    {
        char? completionChar = null;
        var textArgs = e as CommonTextEventArgs;
        if (textArgs != null && textArgs.Text?.Length > 0)
        {
            completionChar = textArgs.Text[0];
        }
        else if (e is KeyEventArgs kea && kea.Key == Key.Tab)
        {
            completionChar = '\t';
        }

        // Tab键触发代码片段插入
        if (completionChar == '\t')
        {
            var snippet = _snippetManager.FindSnippet(_item.DisplayText);
            if (snippet != null)
            {
                var editorSnippet = snippet.CreateAvalonEditSnippet();
                using (textArea.Document.RunUpdate())
                {
                    // 移除补全占位符后插入代码片段
                    textArea.Document.Remove(completionSegment.Offset, completionSegment.Length);
                    editorSnippet.Insert(textArea);
                }
                if (textArgs != null)
                {
                    textArgs.Handled = true;
                }

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 补全项对应的图标
    /// </summary>
    public CommonImage? Image => _glyph.ToImageSource();

    /// <summary>
    /// 补全项显示文本
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// 补全项内容（用于显示）
    /// </summary>
    public object Content { get; }

    /// <summary>
    /// 补全项描述信息（延迟加载）
    /// </summary>
    public object Description
    {
        get
        {
            if (_description == null)
            {
                _description = new Decorator();
#if AVALONIA
                _description.Initialized += (o, e) => { var task = _descriptionTask.Value; };
#else
                _description.Loaded += (o, e) => { var task = _descriptionTask.Value; };
#endif
            }

            return _description;
        }
    }

    /// <summary>
    /// 异步获取补全项的描述信息
    /// </summary>
    /// <returns>异步任务</returns>
    private async Task RetrieveDescription()
    {
        if (_description == null ||
            CompletionService.GetService(_document) is not { } completionService)
        {
            return;
        }

        // 获取Roslyn提供的描述信息并转换为编辑器可显示的格式
        var description = await Task.Run(() => completionService.GetDescriptionAsync(_document, _item)).ConfigureAwait(true);
        _description.Child = description?.TaggedParts.ToTextBlock();
    }

    /// <summary>
    /// 补全项优先级（用于排序）
    /// </summary>
    public double Priority { get; private set; }

    /// <summary>
    /// 是否默认选中该补全项
    /// </summary>
    public bool IsSelected => _item.Rules.MatchPriority == MatchPriority.Preselect;

    /// <summary>
    /// 排序文本（用于补全列表排序）
    /// </summary>
    public string SortText => _item.SortText;

    /// <summary>
    /// 属性变更事件
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 触发属性变更事件
    /// </summary>
    /// <param name="propertyName">属性名称（自动填充）</param>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
