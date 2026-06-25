using Microsoft.CodeAnalysis;
using RoslynPad.Roslyn;
using RoslynPad.Roslyn.BraceMatching;
using RoslynPad.Roslyn.Diagnostics;
using RoslynPad.Roslyn.Structure;
using RoslynPad.Roslyn.QuickInfo;
using Microsoft.CodeAnalysis.Formatting;
using System.Reactive.Linq;
using Microsoft.CodeAnalysis.Text;

namespace RoslynPad.Editor;

/// <summary>
/// 基于Roslyn的代码编辑器核心类，集成代码高亮、补全、折叠、语法诊断等功能
/// </summary>
public class RoslynCodeEditor : CodeTextEditor
{
    /// <summary>
    /// 文本标记服务，用于管理编辑器中的标记（如错误提示、查找高亮等）
    /// </summary>
    private readonly TextMarkerService _textMarkerService;

    /// <summary>
    /// 括号匹配高亮渲染器
    /// </summary>
    private BraceMatcherHighlightRenderer? _braceMatcherHighlighter;

    /// <summary>
    /// 上下文动作渲染器（代码修复/重构提示）
    /// </summary>
    private ContextActionsRenderer? _contextActionsRenderer;

    /// <summary>
    /// 分类高亮颜色配置
    /// </summary>
    private IClassificationHighlightColors? _classificationHighlightColors;

    /// <summary>
    /// Roslyn宿主服务
    /// </summary>
    private IRoslynHost? _roslynHost;

    /// <summary>
    /// 当前文档ID
    /// </summary>
    private DocumentId? _documentId;

    /// <summary>
    /// 快速信息提供程序（鼠标悬停提示）
    /// </summary>
    private IQuickInfoProvider? _quickInfoProvider;

    /// <summary>
    /// 括号匹配服务
    /// </summary>
    private IBraceMatchingService? _braceMatchingService;

    /// <summary>
    /// 括号匹配操作的取消令牌源
    /// </summary>
    private CancellationTokenSource? _braceMatchingCts;

    /// <summary>
    /// Roslyn语法高亮着色器
    /// </summary>
    private RoslynHighlightingColorizer? _colorizer;

    /// <summary>
    /// 代码块结构服务（用于代码折叠）
    /// </summary>
    private IBlockStructureService? _blockStructureService;

    /// <summary>
    /// 初始化 <see cref="RoslynCodeEditor"/> 实例
    /// </summary>
    public RoslynCodeEditor()
    {
        _textMarkerService = new TextMarkerService(this);
        TextArea.TextView.BackgroundRenderers.Add(_textMarkerService);
        TextArea.TextView.LineTransformers.Add(_textMarkerService);
        TextArea.Caret.PositionChanged += CaretOnPositionChanged;

        // 文本变更后延迟2秒刷新代码折叠（避免频繁刷新）
        Observable.FromEventPattern<EventHandler, EventArgs>(
            h => TextArea.TextView.Document.TextChanged += h,
            h => TextArea.TextView.Document.TextChanged -= h)
            .Throttle(TimeSpan.FromSeconds(2))
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(_ => RefreshFoldings().ConfigureAwait(true));
    }

    /// <summary>
    /// 文本变更事件处理（刷新代码折叠）
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="e">事件参数</param>
    private async void OnTextChanged(object? sender, EventArgs e)
    {
        await RefreshFoldings().ConfigureAwait(true);
    }

    /// <summary>
    /// 代码折叠管理器
    /// </summary>
    public FoldingManager? FoldingManager { get; private set; }

    /// <summary>
    /// 获取或设置是否启用代码折叠功能
    /// </summary>
    public bool IsCodeFoldingEnabled
    {
        get { return (bool)this.GetValue(IsCodeFoldingEnabledProperty); }
        set { this.SetValue(IsCodeFoldingEnabledProperty, value); }
    }

    /// <summary>
    /// 获取或设置是否启用括号自动补全功能
    /// </summary>
    public bool IsBraceCompletionEnabled
    {
        get { return (bool)this.GetValue(IsBraceCompletionEnabledProperty); }
        set { this.SetValue(IsBraceCompletionEnabledProperty, value); }
    }

    /// <summary>
    /// 代码折叠功能启用状态依赖属性
    /// </summary>
    public static readonly StyledProperty
#if AVALONIA
        <bool>
#endif
    IsCodeFoldingEnabledProperty =
    CommonProperty.Register<RoslynCodeEditor, bool>(nameof(IsCodeFoldingEnabledProperty), defaultValue: true);

    /// <summary>
    /// 括号自动补全功能启用状态依赖属性
    /// </summary>
    public static readonly StyledProperty
#if AVALONIA
        <bool>
#endif
        IsBraceCompletionEnabledProperty =
        CommonProperty.Register<RoslynCodeEditor, bool>(nameof(IsBraceCompletionEnabled), defaultValue: true);

    /// <summary>
    /// 上下文动作图标依赖属性
    /// </summary>
    public static readonly StyledProperty
#if AVALONIA
        <ImageSource>
#endif
        ContextActionsIconProperty = CommonProperty.Register<RoslynCodeEditor, ImageSource>(
        nameof(ContextActionsIcon), onChanged: OnContextActionsIconChanged);

    /// <summary>
    /// 上下文动作图标变更事件处理
    /// </summary>
    /// <param name="editor">编辑器实例</param>
    /// <param name="args">属性变更参数</param>
    private static void OnContextActionsIconChanged(RoslynCodeEditor editor, CommonPropertyChangedArgs<ImageSource> args)
    {
        if (editor._contextActionsRenderer != null)
        {
            editor._contextActionsRenderer.IconImage = args.NewValue;
        }
    }

    /// <summary>
    /// 获取或设置上下文动作图标
    /// </summary>
    public ImageSource ContextActionsIcon
    {
        get => (ImageSource)this.GetValue(ContextActionsIconProperty);
        set => this.SetValue(ContextActionsIconProperty, value);
    }

    /// <summary>
    /// 获取或设置分类高亮颜色配置
    /// </summary>
    public IClassificationHighlightColors? ClassificationHighlightColors
    {
        get => _classificationHighlightColors;
        set
        {
            _classificationHighlightColors = value;
            if (_braceMatcherHighlighter is not null && value is not null)
            {
                _braceMatcherHighlighter.ClassificationHighlightColors = value;
            }

            RefreshHighlighting();
        }
    }

    /// <summary>
    /// 创建文档事件
    /// </summary>
    public static readonly RoutedEvent CreatingDocumentEvent = CommonEvent.Register<RoslynCodeEditor, CreatingDocumentEventArgs>(nameof(CreatingDocument), RoutingStrategy.Bubble);

    /// <summary>
    /// 创建文档事件
    /// </summary>
    public event EventHandler<CreatingDocumentEventArgs> CreatingDocument
    {
        add => AddHandler(CreatingDocumentEvent, value);
        remove => RemoveHandler(CreatingDocumentEvent, value);
    }

    /// <summary>
    /// 触发创建文档事件
    /// </summary>
    /// <param name="e">事件参数</param>
    protected virtual void OnCreatingDocument(CreatingDocumentEventArgs e)
    {
        RaiseEvent(e);
    }

    /// <summary>
    /// 初始化Roslyn代码编辑器
    /// </summary>
    /// <param name="roslynHost">Roslyn宿主服务</param>
    /// <param name="highlightColors">分类高亮颜色配置</param>
    /// <param name="workingDirectory">工作目录</param>
    /// <param name="documentText">初始文档文本</param>
    /// <param name="sourceCodeKind">源代码类型（如脚本、普通代码）</param>
    /// <returns>当前文档ID</returns>
    public async ValueTask<DocumentId> InitializeAsync(IRoslynHost roslynHost, IClassificationHighlightColors highlightColors, string workingDirectory, string documentText, SourceCodeKind sourceCodeKind)
    {
        _roslynHost = roslynHost ?? throw new ArgumentNullException(nameof(roslynHost));
        _classificationHighlightColors = highlightColors ?? throw new ArgumentNullException(nameof(highlightColors));

        // 初始化括号匹配高亮器
        _braceMatcherHighlighter = new BraceMatcherHighlightRenderer(TextArea.TextView, _classificationHighlightColors);

        // 获取核心服务
        _quickInfoProvider = _roslynHost.GetService<IQuickInfoProvider>();
        _braceMatchingService = _roslynHost.GetService<IBraceMatchingService>();

        // 创建AvalonEdit文本容器
        var avalonEditTextContainer = new AvalonEditTextContainer(Document) { Editor = this };

        // 触发创建文档事件（允许外部扩展）
        var creatingDocumentArgs = new CreatingDocumentEventArgs(avalonEditTextContainer);
        OnCreatingDocument(creatingDocumentArgs);

        // 创建/获取文档ID
        _documentId = creatingDocumentArgs.DocumentId ??
            roslynHost.AddDocument(new DocumentCreationArgs(avalonEditTextContainer, workingDirectory, sourceCodeKind,
                avalonEditTextContainer.UpdateText));

        // 订阅诊断变更事件（错误/警告提示）
        roslynHost.GetWorkspaceService<IDiagnosticsUpdater>(_documentId).DiagnosticsChanged += ProcessDiagnostics;

        // 初始化格式化选项
        if (roslynHost.GetDocument(_documentId) is { } document)
        {
            var options = await document.GetOptionsAsync().ConfigureAwait(true);
            Options.IndentationSize = options.GetOption(FormattingOptions.IndentationSize);
            Options.ConvertTabsToSpaces = !options.GetOption(FormattingOptions.UseTabs);

            // 获取代码块结构服务
            _blockStructureService = document.GetLanguageService<IBlockStructureService>();
        }

        // 设置初始文档文本
        AppendText(documentText);
        Document.UndoStack.ClearAll();
        AsyncToolTipRequest = OnAsyncToolTipRequest;

        // 初始化上下文动作渲染器
        _contextActionsRenderer = new ContextActionsRenderer(this, _textMarkerService) { IconImage = ContextActionsIcon };
        //_contextActionsRenderer.Providers.Add(new RoslynContextActionProvider(_documentId, _roslynHost));

        // 初始化补全提供程序并预热
        var completionProvider = new RoslynCodeEditorCompletionProvider(_documentId, _roslynHost);
        completionProvider.Warmup();

        CompletionProvider = completionProvider;

        // 刷新语法高亮
        RefreshHighlighting();

        // 初始化代码折叠
        InstallFoldingManager();
        await RefreshFoldings().ConfigureAwait(true);

        return _documentId;
    }

    /// <summary>
    /// 刷新语法高亮
    /// </summary>
    public void RefreshHighlighting()
    {
        if (_colorizer != null)
        {
            TextArea.TextView.LineTransformers.Remove(_colorizer);
        }

        if (_documentId != null && _roslynHost != null && _classificationHighlightColors != null)
        {
            _colorizer = new RoslynHighlightingColorizer(_documentId, _roslynHost, _classificationHighlightColors);
            TextArea.TextView.LineTransformers.Insert(0, _colorizer);
        }
    }

    /// <summary>
    /// 光标位置变更事件处理（更新括号匹配高亮）
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="eventArgs">事件参数</param>
    private async void CaretOnPositionChanged(object? sender, EventArgs eventArgs)
    {
        if (_roslynHost == null || _documentId == null || _braceMatcherHighlighter == null)
        {
            return;
        }

        // 取消前一次未完成的括号匹配操作
        _braceMatchingCts?.Cancel();

        if (_braceMatchingService == null)
        {
            return;
        }

        var cts = new CancellationTokenSource();
        var token = cts.Token;
        _braceMatchingCts = cts;

        var document = _roslynHost.GetDocument(_documentId);
        if (document == null)
        {
            return;
        }

        try
        {
            var text = await document.GetTextAsync(token).ConfigureAwait(true);
            var caretOffset = CaretOffset;
            if (caretOffset <= text.Length)
            {
                // 获取括号匹配结果并更新高亮
                var result = await _braceMatchingService.GetAllMatchingBracesAsync(document, caretOffset, token).ConfigureAwait(true);
                _braceMatcherHighlighter.SetHighlight(result.leftOfPosition, result.rightOfPosition);
            }
        }
        catch (OperationCanceledException)
        {
            // 光标再次移动，忽略取消的操作（新操作会处理最新位置）
        }
    }

    /// <summary>
    /// 尝试跳转到匹配的括号位置
    /// </summary>
    private void TryJumpToBrace()
    {
        if (_braceMatcherHighlighter == null) return;

        var caret = CaretOffset;

        if (TryJumpToPosition(_braceMatcherHighlighter.LeftOfPosition, caret) ||
            TryJumpToPosition(_braceMatcherHighlighter.RightOfPosition, caret))
        {
            ScrollToLine(TextArea.Caret.Line);
        }
    }

    /// <summary>
    /// 尝试跳转到指定的括号匹配位置
    /// </summary>
    /// <param name="position">括号匹配结果</param>
    /// <param name="caret">当前光标偏移量</param>
    /// <returns>跳转成功返回true，否则返回false</returns>
    private bool TryJumpToPosition(BraceMatchingResult? position, int caret)
    {
        if (position != null)
        {
            if (position.Value.LeftSpan.Contains(caret))
            {
                CaretOffset = position.Value.RightSpan.End;
                return true;
            }

            if (position.Value.RightSpan.Contains(caret) || position.Value.RightSpan.End == caret)
            {
                CaretOffset = position.Value.LeftSpan.Start;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 处理异步工具提示请求（鼠标悬停显示代码信息）
    /// </summary>
    /// <param name="arg">工具提示请求参数</param>
    /// <returns>异步任务</returns>
    private async Task OnAsyncToolTipRequest(ToolTipRequestEventArgs arg)
    {
        if (_roslynHost == null || _documentId == null || _quickInfoProvider == null)
        {
            return;
        }

        // TODO: 考虑添加延迟，避免频繁请求
        var document = _roslynHost.GetDocument(_documentId);
        if (document == null)
        {
            return;
        }

        // 获取快速信息并设置工具提示
        var info = await _quickInfoProvider.GetItemAsync(document, arg.Position).ConfigureAwait(true);
        if (info != null)
        {
            arg.SetToolTip(info.Create());
        }
    }

    /// <summary>
    /// 处理诊断变更事件（更新错误/警告标记）
    /// </summary>
    /// <param name="args">诊断变更参数</param>
    protected async void ProcessDiagnostics(DiagnosticsChangedArgs args)
    {
        if (args.DocumentId != _documentId)
        {
            return;
        }

        await this.GetDispatcher();

        // 移除已删除的诊断标记
        _textMarkerService.RemoveAll(d => d.Tag is DiagnosticData diagnosticData && args.RemovedDiagnostics.Contains(diagnosticData));

        if (_roslynHost == null || _documentId == null)
        {
            return;
        }

        var document = _roslynHost.GetDocument(_documentId);
        if (document == null || !document.TryGetText(out var sourceText))
        {
            return;
        }

        // 添加新的诊断标记
        foreach (var diagnosticData in args.AddedDiagnostics)
        {
            if (diagnosticData.Severity == DiagnosticSeverity.Hidden || diagnosticData.IsSuppressed)
            {
                continue;
            }

            var span = diagnosticData.GetTextSpan(sourceText);
            if (span == null)
            {
                continue;
            }

            var marker = _textMarkerService.TryCreate(span.Value.Start, span.Value.Length);
            if (marker != null)
            {
                marker.Tag = diagnosticData;
                marker.MarkerColor = GetDiagnosticsColor(diagnosticData);
                marker.ToolTip = diagnosticData.Message;
            }
        }
    }

    /// <summary>
    /// 根据诊断级别获取对应的标记颜色
    /// </summary>
    /// <param name="diagnosticData">诊断数据</param>
    /// <returns>标记颜色</returns>
    private static Color GetDiagnosticsColor(DiagnosticData diagnosticData)
    {
        return diagnosticData.Severity switch
        {
            DiagnosticSeverity.Info => Colors.LimeGreen,
            DiagnosticSeverity.Warning => Colors.DodgerBlue,
            DiagnosticSeverity.Error => Colors.Red,
            _ => throw new ArgumentOutOfRangeException(nameof(diagnosticData)),
        };
    }

    /// <summary>
    /// 键盘按下事件处理（处理括号跳转等快捷键）
    /// </summary>
    /// <param name="e">键盘事件参数</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.HasModifiers(ModifierKeys.Control))
        {
            switch (e.Key)
            {
                case Key.OemCloseBrackets:
                    TryJumpToBrace();
                    break;
            }
        }
    }

    /// <summary>
    /// 刷新代码折叠（根据代码块结构更新折叠区域）
    /// </summary>
    /// <returns>异步任务</returns>
    public async Task RefreshFoldings()
    {
        if (FoldingManager == null || !IsCodeFoldingEnabled)
        {
            return;
        }

        if (_documentId == null || _roslynHost == null || _blockStructureService == null)
        {
            return;
        }

        var document = _roslynHost.GetDocument(_documentId);
        if (document == null)
        {
            return;
        }

        try
        {
            // 获取代码块结构并转换为折叠项
            var elements = await _blockStructureService.GetBlockStructureAsync(document).ConfigureAwait(true);

            var foldings = elements.Spans
                .Select(s => new NewFolding { Name = s.BannerText, StartOffset = s.TextSpan.Start, EndOffset = s.TextSpan.End })
                .OrderBy(item => item.StartOffset);

            // 更新折叠管理器
            FoldingManager?.UpdateFoldings(foldings, firstErrorOffset: 0);
        }
        catch
        {
            // 忽略折叠刷新异常
        }
    }

    /// <summary>
    /// 安装代码折叠管理器
    /// </summary>
    private void InstallFoldingManager()
    {
        if (!IsCodeFoldingEnabled)
        {
            return;
        }

        FoldingManager = FoldingManager.Install(TextArea);
    }

    /// <summary>
    /// 折叠所有可折叠区域
    /// </summary>
    public void FoldAllFoldings()
    {
        if (FoldingManager == null || !IsCodeFoldingEnabled)
        {
            return;
        }

        foreach (var foldingSection in FoldingManager.AllFoldings)
        {
            foldingSection.IsFolded = true;
        }
    }

    /// <summary>
    /// 展开所有折叠区域
    /// </summary>
    public void UnfoldAllFoldings()
    {
        if (FoldingManager == null || !IsCodeFoldingEnabled)
        {
            return;
        }

        foreach (var foldingSection in FoldingManager.AllFoldings)
            foldingSection.IsFolded = false;
    }

    /// <summary>
    /// 切换所有折叠区域的状态（全部折叠/全部展开）
    /// </summary>
    public void ToggleAllFoldings()
    {
        if (FoldingManager == null || !IsCodeFoldingEnabled)
        {
            return;
        }

        var fold = FoldingManager.AllFoldings.All(folding => !folding.IsFolded);

        foreach (var foldingSection in FoldingManager.AllFoldings)
            foldingSection.IsFolded = fold;
    }

    /// <summary>
    /// 切换当前光标位置的折叠区域状态
    /// </summary>
    public void ToggleCurrentFolding()
    {
        if (FoldingManager == null || !IsCodeFoldingEnabled)
        {
            return;
        }

        // 获取光标位置的下一个折叠项
        var folding = FoldingManager.GetNextFolding(TextArea.Caret.Offset);
        if (folding == null || TextArea.Document.GetLocation(folding.StartOffset).Line != TextArea.Document.GetLocation(TextArea.Caret.Offset).Line)
        {
            // 未找到则获取包含光标位置的最后一个折叠项
            folding = FoldingManager.GetFoldingsContaining(TextArea.Caret.Offset).LastOrDefault();
        }

        if (folding != null)
            folding.IsFolded = !folding.IsFolded;
    }

    /// <summary>
    /// 保存当前所有折叠状态
    /// </summary>
    /// <returns>折叠状态列表</returns>
    public IEnumerable<NewFolding> SaveFoldings()
    {
        if (FoldingManager == null || !IsCodeFoldingEnabled)
        {
            return [];
        }

        return FoldingManager?.AllFoldings
            .Select(folding => new NewFolding
            {
                StartOffset = folding.StartOffset,
                EndOffset = folding.EndOffset,
                Name = folding.Title,
                DefaultClosed = folding.IsFolded
            })
            .ToList() ?? [];
    }

    /// <summary>
    /// 恢复保存的折叠状态
    /// </summary>
    /// <param name="foldings">折叠状态列表</param>
    public void RestoreFoldings(IEnumerable<NewFolding> foldings)
    {
        if (FoldingManager == null || !IsCodeFoldingEnabled)
        {
            return;
        }

        FoldingManager.Clear();
        FoldingManager.UpdateFoldings(foldings, -1);
    }

    /// <summary>
    /// 查找高亮标记标签
    /// </summary>
    private const string SearchHighlightTag = "SearchHighlight";

    /// <summary>
    /// 当前查找项高亮标记标签
    /// </summary>
    private const string SearchCurrentTag = "SearchCurrentHighlight";

    /// <summary>
    /// 高亮显示指定的文本跨度（查找结果高亮）
    /// </summary>
    /// <param name="spans">文本跨度列表</param>
    /// <param name="currentIndex">当前选中的项索引（可选）</param>
    public void HighlightSpans(IReadOnlyList<TextSpan> spans, int? currentIndex = null)
    {
        // 清除所有查找高亮
        _textMarkerService.RemoveAll(m => m.Tag is string tag && (tag == SearchHighlightTag || tag == SearchCurrentTag));

        if (spans == null || spans.Count == 0)
            return;

        // 添加新的高亮标记
        for (int i = 0; i < spans.Count; i++)
        {
            var marker = _textMarkerService.TryCreate(spans[i].Start, spans[i].Length);
            if (marker != null)
            {
                if (currentIndex.HasValue && i == currentIndex.Value)
                {
                    marker.Tag = SearchCurrentTag;
                    marker.BackgroundColor = Colors.Orange; // 当前项高亮色
                }
                else
                {
                    marker.Tag = SearchHighlightTag;
                    marker.BackgroundColor = Colors.Yellow; // 普通高亮色
                }
            }
        }
    }

    /// <summary>
    /// 跳转到指定的文本跨度位置
    /// </summary>
    /// <param name="span">目标文本跨度</param>
    public void JumpToSpan(TextSpan span)
    {
        if (span.Start < 0 || span.Start >= Document.TextLength)
            return;

        // 设置光标位置并滚动到对应行
        CaretOffset = span.Start;
        ScrollToLine(Document.GetLineByOffset(span.Start).LineNumber);
    }

    /// <summary>
    /// 清除所有查找高亮标记
    /// </summary>
    public void ClearSearchHighlight()
    {
        _textMarkerService.RemoveAll(m => m.Tag is string tag && (tag == "SearchHighlight" || tag == "SearchCurrentHighlight"));
    }
}
