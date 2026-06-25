// File: src/RoslynPad.Avalonia/DocumentView.axaml.cs

using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using DialogHostAvalonia;
using Microsoft.CodeAnalysis.Text;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using RoslynPad.Build;
using RoslynPad.Editor;
using RoslynPad.UI;
using RoslynPad.ViewModels;

namespace RoslynPad;

/// <summary>
/// 文档编辑视图（Avalonia）：承载 <see cref="RoslynCodeEditor"/>，并与
/// <see cref="OpenDocumentViewModel"/> 建立联动（加载/保存、搜索高亮、主题与字体、控制台输入等）。
/// 同时提供“自由拖拽多光标（VS Code 风格）”与“Ctrl+滚轮缩放”能力。
/// </summary>
/// <remarks>
/// 生命周期：在 <see cref="OnDataContextChanged(object?, System.EventArgs)"/> 中完成一次性初始化与事件订阅；
/// 在 <see cref="Dispose"/> / <see cref="OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs)"/> 中写回未保存文本。<br/>
/// 线程模型：编辑器 UI 相关操作均在 UI 线程；异步加载文本与对话框回调后通过 <see cref="Dispatcher.UIThread"/> 封送回 UI。<br/>
/// 可访问性：右键菜单项可用性在打开时动态刷新，避免误操作。
/// </remarks>
partial class DocumentView : UserControl, IDisposable
{
    /// <summary>代码编辑器实例（来自 XAML 名为 "Editor" 的 <see cref="RoslynCodeEditor"/>）。</summary>
    private readonly RoslynCodeEditor _editor;

    /// <summary>当前绑定的文档视图模型。</summary>
    private OpenDocumentViewModel? _viewModel;

    /// <summary>自由多光标管理器（拖拽进入多光标模式、列选、并行输入等）。</summary>
    private FreehandMultiCaretManager? _multiCaret;

    // ====== 缩放参数（仅用于 Ctrl + 鼠标滚轮） ======
    private const double ZoomMin = 8.0;
    private const double ZoomMax = 56.0;
    private const double ZoomStep = 1.0;

    // —— 新增字段 —— //
    private Window? _hostWindow;
    private bool _closingHooked;
    private bool _reclosing; // 用于防止递归关闭
    // 右键菜单持有引用，避免被 GC
    //private MenuFlyout? _editorFlyout;
    //private ContextMenu? _editorContextMenuFallback;

    /// <summary>
    /// 构造函数：查找编辑器控件、设置编辑器选项、注册指针/键盘/滚轮等输入事件，
    /// 并处理右键菜单可用性刷新。
    /// </summary>
    /// <exception cref="InvalidOperationException">找不到名为 "Editor" 的控件。</exception>
    public DocumentView()
    {
        InitializeComponent();

        _editor = this.FindControl<RoslynCodeEditor>("Editor")
                  ?? throw new InvalidOperationException("Missing Editor");

        DataContextChanged += OnDataContextChanged;

        // 编辑器选项：启用矩形选中与虚拟空格，使得列选/对齐更自然
        _editor.Options.EnableRectangularSelection = true;
        _editor.Options.EnableVirtualSpace = true;

        // ——既有事件：保持原有行为——
        _editor.AddHandler(InputElement.PointerPressedEvent, Editor_OnPointerPressed,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
        _editor.AddHandler(InputElement.PointerMovedEvent, Editor_OnPointerMoved,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
        _editor.AddHandler(InputElement.PointerReleasedEvent, Editor_OnPointerReleased,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
        _editor.AddHandler(InputElement.PointerCaptureLostEvent, Editor_OnPointerCaptureLost,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);

        // Ctrl+滚轮缩放
        _editor.AddHandler(InputElement.PointerWheelChangedEvent, Editor_OnPointerWheelChanged,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);

        // Esc 退出多光标模式
        _editor.AddHandler(InputElement.KeyDownEvent, (s, e) =>
        {
            if (_multiCaret is { IsActive: true } && e.Key == Key.Escape) _multiCaret.Cancel();
        }, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);

        // 如果使用 ContextMenu（非 Flyout），在 Opened 时刷新各项可用性
        if (_editor.ContextMenu is ContextMenu menu)
        {
            menu.Opened += ContextMenu_Opened;
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // 拿到当前文档视图所在的顶层窗口
        var top = TopLevel.GetTopLevel(this) as Window;
        if (top is null) return;

        // 如果是主窗口，让主窗口现有的 DockableClosed 逻辑处理即可；只针对“浮动窗口”挂钩
        if (top is MainWindow) return;

        // 仅挂一次
        if (!_closingHooked)
        {
            _hostWindow = top;
            _hostWindow.Closing += OnHostWindowClosingAsync;
            _closingHooked = true;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // 清理事件
        if (_hostWindow is not null)
        {
            _hostWindow.Closing -= OnHostWindowClosingAsync;
            _hostWindow = null;
            _closingHooked = false;
        }
        base.OnDetachedFromVisualTree(e);
    }

    private async void OnHostWindowClosingAsync(object? sender, WindowClosingEventArgs e)
    {
        if (_reclosing) return;

        var vm = DataContext as OpenDocumentViewModel;
        if (vm is null || !vm.IsDirty) return;

        e.Cancel = true;

        var fileName = vm.Title ?? vm.Document?.Name ?? "未命名";
        var box = MessageBoxManager.GetMessageBoxStandard(
            "提示",
            $"文档 [{fileName}] 已修改，是否保存？",
            ButtonEnum.YesNoCancel,
            Icon.Warning
        );

        var result = await box
            .ShowWindowDialogAsync((Window)sender!)
            .ConfigureAwait(true);   // ★ 显式留在 UI 线程

        if (result == ButtonResult.Yes)
        {
            var saved = await vm
                .SaveAsync(promptSave: false)
                .ConfigureAwait(true);   // ★ 显式留在 UI 线程

            if (saved == SaveResult.Save)
            {
                _reclosing = true;
                _hostWindow?.Close();
            }
        }
        else if (result == ButtonResult.No)
        {
            // —— 与主窗口 FinalizeClose 保持一致 —— //
            try
            {
                // 1) 关闭 Roslyn 文档（若已创建）
                if (vm.HasDocumentId)
                    vm.MainViewModel.RoslynHost?.CloseDocument(vm.DocumentId);
            }
            catch { /* 忽略关闭中的异常，确保能继续移除 */ }

            // 2) 释放视图资源（VM 里已有 Close()）
            vm.Close();

            // 3) 从 OpenDocuments 集合里移除该 VM
            _ = vm.MainViewModel.OpenDocuments.Remove(vm);

            // 4) 关闭浮动窗口（避免递归）
            _reclosing = true;
            _hostWindow?.Close();
        }
        // Cancel：保持打开
    }


    //进行中英文修改后的编辑器右键菜单栏
    private void MenuItem_SelectAll_Click(object? sender, RoutedEventArgs e)
    {
        DoSelectAll();
    }

    /// <summary>菜单：复制（基于当前选区）。</summary>
    private void MenuItem_Copy_Click(object? sender, RoutedEventArgs e)
    {
        _editor.Focus();
        _editor.Copy();
    }

    /// <summary>菜单：剪切（只读时不可用）。</summary>
    private void MenuItem_Cut_Click(object? sender, RoutedEventArgs e)
    {
        _editor.Focus();
        _editor.Cut();
    }

    /// <summary>菜单：粘贴（只读或剪贴板空时不可用）。</summary>
    private void MenuItem_Paste_Click(object? sender, RoutedEventArgs e)
    {
        _editor.Focus();
        _editor.Paste();
    }

    /// <summary>
    /// 右键菜单打开时动态刷新“全选/复制/剪切/粘贴”的可用性（结合只读与选区与剪贴板状态）。
    /// </summary>
    private async void ContextMenu_Opened(object? sender, EventArgs e)
    {
        var menu = sender as ContextMenu;
        if (menu == null) return;

        bool hasSelection = _editor.SelectionLength > 0;
        bool isReadOnly = _editor.IsReadOnly;

        // 粘贴可用性：只读 = 禁用；否则还需检测剪贴板是否有文本
        bool canPaste = !isReadOnly;
        var top = TopLevel.GetTopLevel(this);
        if (top?.Clipboard != null)
        {
            var text = await top.Clipboard.GetTextAsync().ConfigureAwait(true);
            canPaste = canPaste && !string.IsNullOrEmpty(text);
        }

        if (menu.Items[0] is MenuItem selectAllItem)
            selectAllItem.IsEnabled = true;
        if (menu.Items[1] is MenuItem copyItem)
            copyItem.IsEnabled = hasSelection;
        if (menu.Items[2] is MenuItem cutItem)
            cutItem.IsEnabled = hasSelection && !isReadOnly;
        if (menu.Items[3] is MenuItem pasteItem)
            pasteItem.IsEnabled = canPaste;
    }

    /// <summary>
    /// 统一“全选”实现：先取消多光标 → 聚焦编辑器 → 直接设置全选区。
    /// </summary>
    private void DoSelectAll()
    {
        try { _multiCaret?.Cancel(); } catch { /* 忽略异常，保证交互流畅 */ }

        _editor.Focus();
        var doc = _editor.Document;
        if (doc is not null)
        {
            _editor.Select(0, doc.TextLength);
        }
    }

    /// <summary>强类型访问器：当前文档 VM。若未绑定将抛出异常。</summary>
    public OpenDocumentViewModel ViewModel => _viewModel.NotNull();

    /// <summary>
    /// DataContext 切换到 <see cref="OpenDocumentViewModel"/> 时执行初始化：搜索高亮、主题/字体联动、
    /// 文本加载与 Roslyn 初始化、文本变化回调、多光标管理器初始化等。
    /// </summary>
    private async void OnDataContextChanged(object? sender, EventArgs args)
    {
        if (DataContext is not OpenDocumentViewModel viewModel) return;
        _viewModel = viewModel;

        // —— 搜索联动 —— //
        viewModel.SearchHighlightRequested += (spans, currentIndex) => _editor.HighlightSpans(spans, currentIndex);
        viewModel.EditorControl = _editor;
        viewModel.OnEditorFontFamilyChanged(viewModel.EditorFontFamily);
        viewModel.SearchJumpRequested += span => _editor.JumpToSpan(span);
        viewModel.SearchClearHighlightRequested += () => _editor.ClearSearchHighlight();

        // —— 交互/编辑联动 —— //
        viewModel.NuGet.PackageInstalled += NuGetOnPackageInstalled;
        viewModel.ReadInput += OnReadInput;
        viewModel.EditorFocus += (o, e) => _editor.Focus();

        // —— 主题与字号 —— //
        viewModel.MainViewModel.EditorFontSizeChanged += size => _editor.FontSize = size;
        viewModel.MainViewModel.ThemeChanged += OnThemeChanged;
        _editor.FontSize = viewModel.MainViewModel.EditorFontSize;

        // —— 字体家族：优先使用预解析对象，其次按名称解析 —— //
        void SetFontFamily()
        {
            var fontObj = viewModel.MainViewModel.SelectedFontFamilyObject;
            if (fontObj != null) _editor.FontFamily = fontObj;
            else
            {
                var fontName = viewModel.MainViewModel.EditorFontFamily;
                try { _editor.FontFamily = FontFamily.Parse(fontName); } catch { /* 忽略非法字体名 */ }
            }
        }
        SetFontFamily();

        // —— 文本加载 & Roslyn 初始化 —— //
        var documentText = await viewModel.LoadTextAsync().ConfigureAwait(true);

        var documentId = await _editor.InitializeAsync(
            viewModel.MainViewModel.RoslynHost,
            new ThemeClassificationColors(viewModel.MainViewModel.Theme),
            viewModel.WorkingDirectory, documentText, viewModel.SourceCodeKind).ConfigureAwait(true);

        viewModel.Initialize(documentId, OnError,
            () => new TextSpan(_editor.SelectionStart, _editor.SelectionLength),
            this);

        // 文本改变：更新未保存文本缓存 + 通知 VM（以便标题 * 等状态）
        _editor.Document.TextChanged += (o, e) =>
        {
            viewModel.UpdateUnsavedText(_editor.Document.Text);
            viewModel.OnTextChanged();
        };

        // —— 初始化多光标管理 —— //
        _multiCaret = new FreehandMultiCaretManager(_editor);
    }

    // ===== 指针事件处理（多光标/列选 与 正常编辑行为的衔接） =====

    /// <summary>
    /// PointerPressed：右键放行（给 ContextMenu）；中键尝试进入“自由拖拽多光标”模式；
    /// 左键在多光标态下先退出多光标。
    /// </summary>
    private void Editor_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var pt1 = e.GetCurrentPoint(_editor);
        if (pt1.Properties.IsRightButtonPressed)
            return; // 右键：不拦截，允许弹出上下文菜单

        if (_multiCaret is null) return;
        var pt = e.GetCurrentPoint(_editor);

        // 左键：若已处于多光标模式则先退出
        if (pt.Properties.IsLeftButtonPressed && _multiCaret.IsActive)
        {
            _multiCaret.Cancel();
            e.Pointer.Capture(null);
            e.Handled = true;
            return;
        }

        // 中键：尝试进入自由拖拽多光标/列选模式
        if (!pt.Properties.IsMiddleButtonPressed) return;

        if (_multiCaret.TryBegin(e))
        {
            e.Pointer.Capture(_editor);
            e.Handled = true;
        }
    }

    /// <summary>PointerMoved：更新多光标拖拽轨迹。</summary>
    private void Editor_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_multiCaret?.IsDragging != true) return;
        if (_multiCaret.UpdateDrag(e)) e.Handled = true;
    }

    /// <summary>PointerReleased：结束拖拽并进入多光标模式（中键释放）。</summary>
    private void Editor_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_multiCaret?.IsDragging != true) return;
        var pt = e.GetCurrentPoint(_editor);
        if (pt.Properties.PointerUpdateKind != PointerUpdateKind.MiddleButtonReleased) return;
        _multiCaret.EndDragAndEnterMultiCaretMode(e.Pointer);
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    /// <summary>PointerCaptureLost：仅取消拖拽（不退出已激活的多光标）。</summary>
    private void Editor_OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        _multiCaret?.CancelDragOnly();
    }

    /// <summary>
    /// Ctrl + 鼠标滚轮缩放编辑器字体；未按 Ctrl 时保持默认滚动行为。
    /// </summary>
    private void Editor_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if ((e.KeyModifiers & KeyModifiers.Control) == 0) return; // 只在按住 Ctrl 时缩放

        var dir = Math.Sign(e.Delta.Y);
        if (dir == 0) return;

        var cur = _editor.FontSize;
        var next = Math.Clamp(cur + (dir > 0 ? ZoomStep : -ZoomStep), ZoomMin, ZoomMax);

        if (Math.Abs(next - cur) > double.Epsilon)
        {
            _editor.FontSize = next;
            _editor.TextArea?.TextView?.InvalidateVisual(); // 触发重绘
        }

        e.Handled = true; // 阻断向下滚动行为
    }

    // ===== 其它辅助：控制台输入/主题变更/NuGet 安装/错误回调/清理 =====

    /// <summary>
    /// 弹出输入对话框以模拟“控制台输入”，并在关闭后将文本回传给 VM。
    /// </summary>
    private async void OnReadInput()
    {
        var textBox = new TextBox();
        var dialog = new HeaderedContentControl
        {
            Header = "Console Input",
            Content = textBox,
            Background = Brushes.White,
        };
        textBox.Loaded += (o, e) => textBox.Focus();
        textBox.KeyDown += (o, e) =>
        {
            if (e.Key == Key.Enter) DialogHost.Close(MainWindow.DialogHostIdentifier);
        };
        await DialogHost.Show(dialog, MainWindow.DialogHostIdentifier).ConfigureAwait(true);
        ViewModel.SendInput(textBox.Text ?? string.Empty);
    }

    /// <summary>主题变更时更新编辑器的语法分类配色。</summary>
    private void OnThemeChanged(object? sender, EventArgs e)
    {
        _editor.ClassificationHighlightColors = new ThemeClassificationColors(ViewModel.MainViewModel.Theme);
    }

    /// <summary>
    /// NuGet 包安装完成后，在文首插入 <c>#r "nuget: 包, 版本"</c> 指令，便于脚本引用。
    /// </summary>
    private void NuGetOnPackageInstalled(PackageData package)
    {
        _ = this.GetDispatcher().InvokeAsync(() =>
        {
            var text = $"#r \"nuget: {package.Id}, {package.Version}\"{Environment.NewLine}";
            _editor.Document.Insert(0, text, AnchorMovementType.Default);
        });
    }

    /// <summary>统一错误回调（占位，可扩展到状态栏/消息面板）。</summary>
    private void OnError(ExceptionResultObject? e) { }

    /// <summary>
    /// 释放：在销毁前把当前编辑器文本写回 VM 的未保存缓存，避免数据丢失。
    /// </summary>
    public void Dispose()
    {
        try
        {
            if (DataContext is OpenDocumentViewModel vm && _editor?.Document is TextDocument td)
            {
                vm.UpdateUnsavedText(td.Text);
            }
        }
        catch { /* 忽略清理期异常 */ }
    }

    // =====================================================================
    // VS Code 风格多光标：拖拽 + 选区；锚点(TextAnchor)跟踪；滚动期间也刷新拖拽轨迹
    // =====================================================================

    /// <summary>
    /// 自由拖拽多光标管理器：负责在中键拖拽时绘制选择预览、生成多光标/多选区，
    /// 并拦截输入/回车/Tab/删除等编辑操作按“从后到前”批量执行。
    /// </summary>
    private sealed class FreehandMultiCaretManager : IBackgroundRenderer, IDisposable
    {
        private readonly RoslynCodeEditor _editor;
        private readonly TextArea _area;
        private readonly TextView _tv;

        /// <summary>将 VisualLine 的文档坐标系 Y 转换为视图坐标系 Y（考虑滚动）。</summary>
        private double GetViewYForVisualLine(VisualLine vl)
        {
            var vls = _tv.VisualLines;
            if (vls.Count == 0) return vl.VisualTop;
            var first = vls[0];
            return vl.VisualTop - first.VisualTop;
        }

        /// <summary>拖拽中标志。</summary>
        public bool IsDragging { get; private set; }

        /// <summary>是否处于“多光标激活”状态。</summary>
        public bool IsActive => _multiCaretActive;

        // —— 拖拽/预览状态字段 —— //
        private int _anchorLine;        // 1-based
        private int _currentLine;       // 1-based
        private int _anchorVC;          // 0-based

        /// <summary>拖动阶段：每行矩形选择范围（起止可视列）。</summary>
        private readonly Dictionary<int, (int StartVC, int EndVC)> _dragRectPerLine = new();

        /// <summary>TextView 坐标系下，最近一次指针位置（用于滚动时继续更新预览）。</summary>
        private Point _lastViewPos;

        private double _lastPreviewX;
        private const double PreviewThresholdPx = 0.5;

        private bool _multiCaretActive;

        private IPointer? _capturedPointer;

        /// <summary>内部光标/选区信息（由锚点跟踪实际偏移）。</summary>
        private sealed class MCaret
        {
            public int Line;           // 1-based
            public int VC;             // 0-based
            public TextAnchor Anchor;  // AfterInsertion
            public TextAnchor SelStart; // BeforeInsertion
            public TextAnchor SelEnd;   // AfterInsertion
            public bool HasSelection => SelStart.Offset != SelEnd.Offset;

            public MCaret(int line, int vc, TextAnchor anchor, TextAnchor selStart, TextAnchor selEnd)
            { Line = line; VC = vc; Anchor = anchor; SelStart = selStart; SelEnd = selEnd; }
        }
        private readonly List<MCaret> _carets = new();

        private bool _handlingKeyDown;

        // 预览/光标绘制用画笔与刷子
        private readonly Pen _caretPen = new(new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0x99, 0xFF)), 1);
        private readonly IBrush _selectionBrush = new SolidColorBrush(Color.FromArgb(0x44, 0x33, 0x99, 0xFF));

        private readonly DispatcherTimer _blinkTimer;
        private bool _blinkOn = true;

        /// <summary>
        /// 构造：挂接编辑器与 TextView、注册按键/输入、启动光标闪烁计时器，并在可见行变化时更新预览。
        /// </summary>
        public FreehandMultiCaretManager(RoslynCodeEditor editor)
        {
            _editor = editor;
            _area = editor.TextArea ?? throw new InvalidOperationException("TextArea not ready");
            _tv = _area.TextView ?? throw new InvalidOperationException("TextView not ready");

            _tv.BackgroundRenderers.Add(this);

            _area.AddHandler(InputElement.KeyDownEvent, OnKeyDown,
                RoutingStrategies.Tunnel, handledEventsToo: true);

            _area.TextEntering += OnTextEntering;

            _blinkTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(530) };
            _blinkTimer.Tick += (_, __) =>
            {
                if (!IsDragging && !_multiCaretActive) return;
                _blinkOn = !_blinkOn;
                _tv.InvalidateVisual();
            };
            _blinkTimer.Start();

            // 可见行变化（滚动/折叠/布局变更）：拖拽中继续更新轨迹
            _tv.VisualLinesChanged += (_, __) =>
            {
                if (IsDragging)
                {
                    UpdateDragFromViewPoint(_lastViewPos);
                }
                if (IsDragging || _multiCaretActive) _tv.InvalidateVisual();
            };
        }

        /// <summary>释放：停止闪烁计时器。</summary>
        public void Dispose() => _blinkTimer.Stop();

        // ===== 生命周期：开始/更新/结束拖拽，多光标激活/取消 =====

        /// <summary>
        /// 尝试开始多光标拖拽：清空状态、捕获指针并延迟到布局稳定后计算命中。
        /// </summary>
        public bool TryBegin(PointerPressedEventArgs e)
        {
            if (_editor.Document is null) return false;

            IsDragging = false;
            _multiCaretActive = false;
            _carets.Clear();
            _dragRectPerLine.Clear();

            _capturedPointer = e.Pointer;

            _editor.Focus();

            // 使用 Dispatcher 延迟处理，确保滚动后的视图更新完成
            return TryBeginAsync(e);
        }

        private bool TryBeginAsync(PointerPressedEventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (!TryBeginInternal(e))
                {
                    Dispatcher.UIThread.Post(() => TryBeginInternal(e), DispatcherPriority.Background);
                }
            }, DispatcherPriority.Render);

            return true; // 先返回 true 让事件被捕获
        }

        private bool TryBeginInternal(PointerPressedEventArgs e)
        {
            try
            {
                _tv.InvalidateVisual();
                _tv.EnsureVisualLines();

                Dispatcher.UIThread.Post(() =>
                {
                    _tv.EnsureVisualLines();
                    var pView = e.GetPosition(_tv);

                    var pos = GetReliablePosition(pView);
                    if (pos is null)
                        return;

                    IsDragging = true;

                    _anchorLine = Math.Max(1, pos.Value.Line);
                    _currentLine = _anchorLine;
                    _anchorVC = ClampVC(_anchorLine, Math.Max(0, pos.Value.VisualColumn));

                    _dragRectPerLine[_anchorLine] = (_anchorVC, _anchorVC);

                    _lastViewPos = pView;
                    _lastPreviewX = pView.X;

                    _area.ClearSelection();
                    _tv.InvalidateVisual();
                }, DispatcherPriority.Background);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 在 TextView 坐标系中取得可靠的命中位置：优先 <see cref="TextView.GetPosition(Point)"/>，
        /// 失败时在可见行集合中手工计算（含滚动偏移修正）。
        /// </summary>
        private TextViewPosition? GetReliablePosition(Point viewPoint)
        {
            try
            {
                var doc = _editor.Document;
                if (doc is null) return null;

                // 重试几次，给布局/滚动一个缓冲
                for (int retry = 0; retry < 3; retry++)
                {
                    _tv.EnsureVisualLines();
                    var pos = _tv.GetPosition(viewPoint);
                    if (pos.HasValue) return pos;
                    System.Threading.Thread.Sleep(1);
                }

                var visibleLines = _tv.VisualLines;
                if (visibleLines.Count == 0) return null;

                // 将视图坐标 Y 转为文档坐标 Y
                double firstTop = visibleLines[0].VisualTop;
                double docY = viewPoint.Y + firstTop;

                // 定位命中的 VisualLine
                VisualLine? targetVisualLine = null;
                foreach (var vl in visibleLines)
                {
                    if (docY >= vl.VisualTop && docY < vl.VisualTop + vl.Height)
                    {
                        targetVisualLine = vl;
                        break;
                    }
                }

                if (targetVisualLine == null)
                    targetVisualLine = (docY < visibleLines[0].VisualTop)
                        ? visibleLines[0]
                        : visibleLines[visibleLines.Count - 1];

                if (targetVisualLine == null) return null;

                // 估算命中的是哪一“文档行”
                int targetDocLine;
                if (targetVisualLine.FirstDocumentLine.LineNumber == targetVisualLine.LastDocumentLine.LineNumber)
                {
                    targetDocLine = targetVisualLine.FirstDocumentLine.LineNumber;
                }
                else
                {
                    double relativeY = docY - targetVisualLine.VisualTop;
                    double lineRatio = relativeY / targetVisualLine.Height;
                    int lineCount = targetVisualLine.LastDocumentLine.LineNumber - targetVisualLine.FirstDocumentLine.LineNumber + 1;
                    int lineOffset = (int)Math.Floor(lineRatio * lineCount);
                    targetDocLine = targetVisualLine.FirstDocumentLine.LineNumber + Math.Clamp(lineOffset, 0, lineCount - 1);
                }

                int visualColumn = CalculateVisualColumnFromX(targetDocLine, viewPoint.X, targetVisualLine);
                return new TextViewPosition(targetDocLine, 1) { VisualColumn = Math.Max(0, visualColumn) };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 根据 X 坐标估算可视列（VisualColumn），优先用二分搜索对齐到最近字符点，
        /// 失败时回退到“基于平均字符宽”的估算。
        /// </summary>
        private int CalculateVisualColumnFromX(int line, double x, VisualLine visualLine)
        {
            try
            {
                var lineStartPos = new TextViewPosition(line, 1);
                var lineStartPoint = _tv.GetVisualPosition(lineStartPos, VisualYPosition.LineMiddle);

                if (x <= lineStartPoint.X) return 0;

                int maxVC = GetLineVisualLength(line) + 20;
                int left = 0, right = maxVC;
                int bestVC = 0;
                double bestDistance = double.MaxValue;

                while (left <= right)
                {
                    int mid = (left + right) / 2;
                    var testPos = new TextViewPosition(line, 1) { VisualColumn = mid };
                    var testPoint = _tv.GetVisualPosition(testPos, VisualYPosition.LineMiddle);

                    double distance = Math.Abs(testPoint.X - x);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestVC = mid;
                    }

                    if (testPoint.X < x) left = mid + 1;
                    else right = mid - 1;
                }

                return bestVC;
            }
            catch
            {
                // 回退：估算一个平均字符宽
                var lineStartPos = new TextViewPosition(line, 1);
                var lineStartPoint = _tv.GetVisualPosition(lineStartPos, VisualYPosition.LineMiddle);

                double charWidth = 10.0;
                try
                {
                    var testPos = new TextViewPosition(line, 1) { VisualColumn = 10 };
                    var testPoint = _tv.GetVisualPosition(testPos, VisualYPosition.LineMiddle);
                    if (testPoint.X > lineStartPoint.X)
                        charWidth = (testPoint.X - lineStartPoint.X) / 10.0;
                }
                catch { }

                int estimatedVC = (int)Math.Round((x - lineStartPoint.X) / charWidth);
                return Math.Max(0, estimatedVC);
            }
        }

        /// <summary>更新拖拽预览（指针移动）。</summary>
        public bool UpdateDrag(PointerEventArgs e) => UpdateDragFromViewPoint(e.GetPosition(_tv));

        /// <summary>核心：从 TextView 坐标更新当前拖拽状态，并触发重绘。</summary>
        private bool UpdateDragFromViewPoint(Point pView)
        {
            if (!IsDragging) return false;

            _tv.EnsureVisualLines();
            var pos = GetReliablePosition(pView);
            if (pos is null) return false;

            _lastViewPos = pView;

            var newLine = Math.Max(1, pos.Value.Line); // NOTE: case-insensitive to keep original; actual C# is Math.Max
            newLine = Math.Max(1, pos.Value.Line);

            if (newLine != _currentLine)
            {
                int step = newLine > _currentLine ? 1 : -1;
                for (int line = _currentLine + step; line != newLine + step; line += step)
                {
                    if (IsLineVisible(line))
                    {
                        int vcRaw = GetVisualColumnAtX(line, pView.X);
                        int vc = ClampVC(line, vcRaw);
                        var start = Math.Min(_anchorVC, vc);
                        var end = Math.Max(_anchorVC, vc);
                        _dragRectPerLine[line] = (start, end);
                    }
                    else
                    {
                        if (!_dragRectPerLine.ContainsKey(line))
                            _dragRectPerLine[line] = (_anchorVC, _anchorVC);
                    }
                }
                _currentLine = newLine;
                _tv.InvalidateVisual();
            }

            if (IsLineVisible(_currentLine))
            {
                int vcRaw = GetVisualColumnAtX(_currentLine, pView.X);
                int vc = ClampVC(_currentLine, vcRaw);
                var start = Math.Min(_anchorVC, vc);
                var end = Math.Max(_anchorVC, vc);
                _dragRectPerLine[_currentLine] = (start, end);
            }
            else if (!_dragRectPerLine.ContainsKey(_currentLine))
            {
                _dragRectPerLine[_currentLine] = (_anchorVC, _anchorVC);
            }

            if (Math.Abs(pView.X - _lastPreviewX) >= 0.5)
            {
                _lastPreviewX = pView.X;
                _tv.InvalidateVisual();
            }
            return true;
        }

        /// <summary>
        /// 结束拖拽并进入“多光标激活”模式：把每行的预览矩形转换为具体的选区与光标锚点。
        /// </summary>
        public void EndDragAndEnterMultiCaretMode(IPointer? pointer = null)
        {
            if (!IsDragging) return;
            IsDragging = false;

            (pointer ?? _capturedPointer)?.Capture(null);
            _capturedPointer = null;

            // 若当前行还没入表，这里补一份（使用最后视图位置估算）
            if (!_dragRectPerLine.ContainsKey(_currentLine))
            {
                if (IsLineVisible(_currentLine))
                {
                    int vcRaw = GetVisualColumnAtX(_currentLine, _lastViewPos.X);
                    int vc = ClampVC(_currentLine, vcRaw);
                    var start = Math.Min(_anchorVC, vc);
                    var end = Math.Max(_anchorVC, vc);
                    _dragRectPerLine[_currentLine] = (start, end);
                }
                else
                {
                    _dragRectPerLine[_currentLine] = (_anchorVC, _anchorVC);
                }
            }

            _carets.Clear();

            int startLine = _anchorLine, endLine = _currentLine, step = startLine <= endLine ? 1 : -1;
            bool caretAtRight = _lastViewPos.X >= GetXByVisualColumn(_anchorLine, _anchorVC);

            for (int line = startLine; line != endLine + step; line += step)
            {
                var span = _dragRectPerLine.TryGetValue(line, out var tmp) ? tmp : (_anchorVC, _anchorVC);
                var (spanStart, spanEnd) = span;

                // 若当前行可见且还是点选，依据最后 X 再算一次
                if (IsLineVisible(line) && spanStart == spanEnd && spanStart == _anchorVC)
                {
                    int vcRawNow = GetVisualColumnAtX(line, _lastViewPos.X);
                    int vcNow = ClampVC(line, vcRawNow);
                    spanStart = Math.Min(_anchorVC, vcNow);
                    spanEnd = Math.Max(_anchorVC, vcNow);
                }

                int lineVis = GetLineVisualLength(line);
                int sVC = Math.Clamp(spanStart, 0, lineVis);
                int eVC = Math.Clamp(spanEnd, 0, lineVis);

                var (offA, _) = GetOffsetAndPadForVC(line, sVC);
                var (offB, _) = GetOffsetAndPadForVC(line, eVC);
                int selStartOff = Math.Min(offA, offB);
                int selEndOff = Math.Max(offA, offB);

                int caretVC = caretAtRight ? eVC : sVC;
                var (caretOff, _) = GetOffsetAndPadForVC(line, caretVC);

                selStartOff = Math.Clamp(selStartOff, 0, _editor.Document.TextLength);
                selEndOff = Math.Clamp(selEndOff, 0, _editor.Document.TextLength);
                caretOff = Math.Clamp(caretOff, 0, _editor.Document.TextLength);

                var selStart = _editor.Document.CreateAnchor(selStartOff);
                selStart.MovementType = AnchorMovementType.BeforeInsertion;
                var selEnd = _editor.Document.CreateAnchor(selEndOff);
                selEnd.MovementType = AnchorMovementType.AfterInsertion;

                var caretAnchor = _editor.Document.CreateAnchor(caretOff);
                caretAnchor.MovementType = AnchorMovementType.AfterInsertion;

                _carets.Add(new MCaret(line, caretVC, caretAnchor, selStart, selEnd));
            }

            // 按 offset 降序执行编辑命令，避免前面修改影响后面偏移
            _carets.Sort((a, b) => b.Anchor.Offset.CompareTo(a.Anchor.Offset));
            _multiCaretActive = _carets.Count > 0;

            _editor.Focus();
            SyncFromAnchors();
            _tv.InvalidateVisual();
        }

        /// <summary>仅取消拖拽阶段（不退出已进入的多光标模式）。</summary>
        public void CancelDragOnly()
        {
            IsDragging = false;
            _capturedPointer?.Capture(null);
            _capturedPointer = null;
            _tv.InvalidateVisual();
        }

        /// <summary>取消多光标模式并清理预览/选区。</summary>
        public void Cancel()
        {
            IsDragging = false;
            _multiCaretActive = false;
            _carets.Clear();
            _dragRectPerLine.Clear();
            _capturedPointer?.Capture(null);
            _capturedPointer = null;
            _area.ClearSelection();
            _tv.InvalidateVisual();
        }

        // ===== 背景渲染：在可见行上绘制选择矩形与多光标竖线 =====

        /// <inheritdoc/>
        public KnownLayer Layer => KnownLayer.Caret;

        /// <inheritdoc/>
        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (textView.Document is null) return;
            textView.EnsureVisualLines();

            if (!IsDragging && !_multiCaretActive) return;

            if (IsDragging)
            {
                // 拖拽预览：按行绘制矩形
                foreach (var kv in _dragRectPerLine)
                {
                    int line = kv.Key; if (!IsLineVisible(line)) continue;
                    var (s0, e0) = kv.Value; if (e0 < s0) (s0, e0) = (e0, s0);
                    int s = ClampVC(line, s0);
                    int e = ClampVC(line, e0);
                    var x1 = GetXByVisualColumn(line, s);
                    var x2 = GetXByVisualColumn(line, e);
                    if (!TryGetLineBounds(line, out var top, out var bottom)) continue;
                    var rect = new Rect(new Point(x1, top), new Point(x2, bottom));
                    drawingContext.FillRectangle(_selectionBrush, rect);
                }
                // 预览插入光标
                if (_blinkOn && IsLineVisible(_currentLine))
                {
                    int vcRaw = GetVisualColumnAtX(_currentLine, _lastViewPos.X);
                    int vc = ClampVC(_currentLine, vcRaw);
                    var x = GetXByVisualColumn(_currentLine, vc);
                    if (TryGetLineBounds(_currentLine, out var t, out var b))
                        drawingContext.DrawLine(_caretPen, new Point(x, t), new Point(x, b));
                }
            }
            else
            {
                // 多光标模式：绘制每个选区与光标
                foreach (var c in _carets)
                {
                    if (!IsLineVisible(c.Line)) continue;

                    int sOff = Math.Min(c.SelStart.Offset, c.SelEnd.Offset);
                    int eOff = Math.Max(c.SelStart.Offset, c.SelEnd.Offset);
                    if (sOff != eOff)
                    {
                        int sVC = GetVCAtOffset(sOff);
                        int eVC = GetVCAtOffset(eOff);
                        var x1 = GetXByVisualColumn(c.Line, sVC);
                        var x2 = GetXByVisualColumn(c.Line, eVC);
                        if (TryGetLineBounds(c.Line, out var top, out var bottom))
                        {
                            var rect = new Rect(new Point(x1, top), new Point(x2, bottom));
                            drawingContext.FillRectangle(_selectionBrush, rect);
                        }
                    }

                    if (_blinkOn && TryGetLineBounds(c.Line, out var t, out var b))
                    {
                        var x = GetXByVisualColumn(c.Line, c.VC);
                        drawingContext.DrawLine(_caretPen, new Point(x, t), new Point(x, b));
                    }
                }
            }
        }

        // ===== 输入与命令处理：按 offset 降序批处理，保持插入/删除稳定性 =====

        /// <summary>文本输入（普通字符）：对每个光标执行插入/替换，并更新锚点与 VC。</summary>
        private void OnTextEntering(object? sender, TextInputEventArgs e)
        {
            if (!_multiCaretActive || string.IsNullOrEmpty(e.Text) || _editor.Document is null)
                return;

            e.Handled = true;

            SyncFromAnchors();

            using var _ = _editor.Document.RunUpdate();

            var list = _carets.OrderByDescending(c => c.Anchor.Offset).ToList();

            foreach (var c in list)
            {
                if (c.HasSelection)
                {
                    int s = Math.Min(c.SelStart.Offset, c.SelEnd.Offset);
                    int eoff = Math.Max(c.SelStart.Offset, c.SelEnd.Offset);
                    _editor.Document.Remove(s, eoff - s);

                    int off = s;
                    _editor.Document.Insert(off, e.Text!);
                    off += e.Text!.Length;

                    c.Anchor = _editor.Document.CreateAnchor(off); c.Anchor.MovementType = AnchorMovementType.AfterInsertion;
                    c.SelStart = _editor.Document.CreateAnchor(off); c.SelStart.MovementType = AnchorMovementType.BeforeInsertion;
                    c.SelEnd = _editor.Document.CreateAnchor(off); c.SelEnd.MovementType = AnchorMovementType.AfterInsertion;

                    c.VC = GetVCAtOffset(off);
                    var loc = _editor.Document.GetLocation(off);
                    c.Line = loc.Line;
                }
                else
                {
                    int off = Math.Clamp(c.Anchor.Offset, 0, _editor.Document.TextLength);
                    int currentVC = GetVCAtOffset(off);
                    int padCnt = Math.Max(0, c.VC - currentVC);
                    if (padCnt > 0) { _editor.Document.Insert(off, new string(' ', padCnt)); off += padCnt; }

                    _editor.Document.Insert(off, e.Text!);
                    off += e.Text!.Length;

                    c.Anchor = _editor.Document.CreateAnchor(off); c.Anchor.MovementType = AnchorMovementType.AfterInsertion;
                    c.SelStart = _editor.Document.CreateAnchor(off); c.SelStart.MovementType = AnchorMovementType.BeforeInsertion;
                    c.SelEnd = _editor.Document.CreateAnchor(off); c.SelEnd.MovementType = AnchorMovementType.AfterInsertion;

                    c.VC = GetVCAtOffset(off);
                    var loc = _editor.Document.GetLocation(off);
                    c.Line = loc.Line;
                }
            }

            SyncFromAnchors();
            _tv.InvalidateVisual();
        }

        /// <summary>
        /// 键盘关键键处理：撤销/重做时刷新显示；Enter/Tab/Backspace/Delete 在多光标模式下批处理。
        /// </summary>
        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (_editor.Document is null) return;

            bool ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            bool shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
            if (ctrl && _multiCaretActive &&
                ((e.Key == Key.Z && !shift) || e.Key == Key.Y || (e.Key == Key.Z && shift)))
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (_multiCaretActive) { SyncFromAnchors(); _tv.InvalidateVisual(); }
                }, DispatcherPriority.Background);
                return;
            }

            if (!_multiCaretActive) return;

            if (_handlingKeyDown) return;
            _handlingKeyDown = true;
            try
            {
                if (e.Key is Key.Back or Key.Delete or Key.Enter or Key.Tab)
                {
                    e.Handled = true;
                    using var _ = _editor.Document.RunUpdate();

                    var list = _carets.OrderByDescending(c => c.Anchor.Offset).ToList();
                    var doc = _editor.Document;

                    if (e.Key == Key.Enter)
                    {
                        foreach (var c in list)
                        {
                            if (c.HasSelection)
                            {
                                int s = Math.Min(c.SelStart.Offset, c.SelEnd.Offset);
                                int eoff = Math.Max(c.SelStart.Offset, c.SelEnd.Offset);
                                doc.Remove(s, eoff - s);
                                int off = s;

                                doc.Insert(off, Environment.NewLine);
                                off += Environment.NewLine.Length;

                                c.Anchor = doc.CreateAnchor(off); c.Anchor.MovementType = AnchorMovementType.AfterInsertion;
                                c.SelStart = doc.CreateAnchor(off); c.SelEnd = doc.CreateAnchor(off);
                                c.SelStart.MovementType = AnchorMovementType.BeforeInsertion;
                                c.SelEnd.MovementType = AnchorMovementType.AfterInsertion;

                                c.VC = GetVCAtOffset(off);
                                c.Line = doc.GetLocation(off).Line;
                            }
                            else
                            {
                                int off = Math.Clamp(c.Anchor.Offset, 0, doc.TextLength);
                                int currentVC = GetVCAtOffset(off);
                                int padCnt = Math.Max(0, c.VC - currentVC);
                                if (padCnt > 0) { doc.Insert(off, new string(' ', padCnt)); off += padCnt; }

                                doc.Insert(off, Environment.NewLine);
                                off += Environment.NewLine.Length;

                                c.Anchor = doc.CreateAnchor(off); c.Anchor.MovementType = AnchorMovementType.AfterInsertion;
                                c.SelStart = doc.CreateAnchor(off); c.SelEnd = doc.CreateAnchor(off);
                                c.SelStart.MovementType = AnchorMovementType.BeforeInsertion;
                                c.SelEnd.MovementType = AnchorMovementType.AfterInsertion;

                                c.VC = GetVCAtOffset(off);
                                c.Line = doc.GetLocation(off).Line;
                            }
                        }
                        SyncFromAnchors();
                        _tv.InvalidateVisual();
                        return;
                    }

                    if (e.Key == Key.Tab)
                    {
                        var tab = _area.Options.IndentationString ?? "    ";
                        foreach (var c in list)
                        {
                            if (c.HasSelection)
                            {
                                int s = Math.Min(c.SelStart.Offset, c.SelEnd.Offset);
                                int eoff = Math.Max(c.SelStart.Offset, c.SelEnd.Offset);
                                doc.Remove(s, eoff - s);
                                int off = s;

                                doc.Insert(off, tab);
                                off += tab.Length;

                                c.Anchor = doc.CreateAnchor(off); c.Anchor.MovementType = AnchorMovementType.AfterInsertion;
                                c.SelStart = doc.CreateAnchor(off); c.SelStart.MovementType = AnchorMovementType.BeforeInsertion;
                                c.SelEnd = doc.CreateAnchor(off); c.SelEnd.MovementType = AnchorMovementType.AfterInsertion;

                                c.VC = GetVCAtOffset(off);
                                c.Line = doc.GetLocation(off).Line;
                            }
                            else
                            {
                                int off = Math.Clamp(c.Anchor.Offset, 0, doc.TextLength);
                                int currentVC = GetVCAtOffset(off);
                                int padCnt = Math.Max(0, c.VC - currentVC);
                                if (padCnt > 0) { doc.Insert(off, new string(' ', padCnt)); off += padCnt; }

                                doc.Insert(off, tab);
                                off += tab.Length;

                                c.Anchor = doc.CreateAnchor(off); c.Anchor.MovementType = AnchorMovementType.AfterInsertion;
                                c.SelStart = doc.CreateAnchor(off); c.SelStart.MovementType = AnchorMovementType.BeforeInsertion;
                                c.SelEnd = doc.CreateAnchor(off); c.SelEnd.MovementType = AnchorMovementType.AfterInsertion;

                                c.VC = GetVCAtOffset(off);
                                c.Line = doc.GetLocation(off).Line;
                            }
                        }
                        SyncFromAnchors();
                        _tv.InvalidateVisual();
                        return;
                    }

                    if (e.Key == Key.Back)
                    {
                        foreach (var c in list)
                        {
                            if (c.HasSelection)
                            {
                                int s = Math.Min(c.SelStart.Offset, c.SelEnd.Offset);
                                int eoff = Math.Max(c.SelStart.Offset, c.SelEnd.Offset);
                                doc.Remove(s, eoff - s);
                                int off = s;

                                c.Anchor = doc.CreateAnchor(off); c.Anchor.MovementType = AnchorMovementType.AfterInsertion;
                                c.SelStart = doc.CreateAnchor(off); c.SelStart.MovementType = AnchorMovementType.BeforeInsertion;
                                c.SelEnd = doc.CreateAnchor(off); c.SelEnd.MovementType = AnchorMovementType.AfterInsertion;

                                c.VC = GetVCAtOffset(off);
                                c.Line = doc.GetLocation(off).Line;
                            }
                            else
                            {
                                int off = Math.Clamp(c.Anchor.Offset, 0, doc.TextLength);
                                var locLine = doc.GetLocation(off).Line;
                                var line = doc.Lines[Math.Clamp(locLine - 1, 0, doc.LineCount - 1)];
                                if (off > line.Offset)
                                {
                                    doc.Remove(off - 1, 1);
                                    off -= 1;
                                }

                                c.Anchor = doc.CreateAnchor(off); c.Anchor.MovementType = AnchorMovementType.AfterInsertion;
                                c.SelStart = doc.CreateAnchor(off); c.SelStart.MovementType = AnchorMovementType.BeforeInsertion;
                                c.SelEnd = doc.CreateAnchor(off); c.SelEnd.MovementType = AnchorMovementType.AfterInsertion;

                                c.VC = GetVCAtOffset(off);
                                c.Line = doc.GetLocation(off).Line;
                            }
                        }
                        SyncFromAnchors();
                        _tv.InvalidateVisual();
                        return;
                    }

                    if (e.Key == Key.Delete)
                    {
                        foreach (var c in list)
                        {
                            if (c.HasSelection)
                            {
                                int s = Math.Min(c.SelStart.Offset, c.SelEnd.Offset);
                                int eoff = Math.Max(c.SelStart.Offset, c.SelEnd.Offset);
                                doc.Remove(s, eoff - s);
                                int off = s;

                                c.Anchor = doc.CreateAnchor(off); c.Anchor.MovementType = AnchorMovementType.AfterInsertion;
                                c.SelStart = doc.CreateAnchor(off); c.SelStart.MovementType = AnchorMovementType.BeforeInsertion;
                                c.SelEnd = doc.CreateAnchor(off); c.SelEnd.MovementType = AnchorMovementType.AfterInsertion;

                                c.VC = GetVCAtOffset(off);
                                c.Line = doc.GetLocation(off).Line;
                            }
                            else
                            {
                                int off = Math.Clamp(c.Anchor.Offset, 0, doc.TextLength);
                                if (off < doc.TextLength)
                                {
                                    doc.Remove(off, 1);
                                }

                                c.Anchor = doc.CreateAnchor(off); c.Anchor.MovementType = AnchorMovementType.AfterInsertion;
                                c.SelStart = doc.CreateAnchor(off); c.SelStart.MovementType = AnchorMovementType.BeforeInsertion;
                                c.SelEnd = doc.CreateAnchor(off); c.SelEnd.MovementType = AnchorMovementType.AfterInsertion;

                                c.VC = GetVCAtOffset(off);
                                c.Line = doc.GetLocation(off).Line;
                            }
                        }
                        SyncFromAnchors();
                        _tv.InvalidateVisual();
                        return;
                    }
                }
            }
            finally
            {
                _handlingKeyDown = false;
            }
        }

        // ===== 工具方法：Line/VC/Bounds/Offset 互转与可见性判断 =====

        /// <summary>判断行号是否在可见行范围内。</summary>
        private bool IsLineVisible(int line)
        {
            _tv.EnsureVisualLines();
            var vls = _tv.VisualLines;
            if (vls.Count == 0) return false;
            var first = vls[0].FirstDocumentLine.LineNumber;
            var last = vls[^1].LastDocumentLine.LineNumber;
            return line >= first && line <= last;
        }

        /// <summary>获取某文档行在视图中的上下边界（像素）。</summary>
        private bool TryGetLineBounds(int line, out double top, out double bottom)
        {
            _tv.EnsureVisualLines();
            var vls = _tv.VisualLines;
            if (vls.Count == 0) { top = bottom = 0; return false; }
            foreach (var vl in vls)
            {
                var first = vl.FirstDocumentLine.LineNumber;
                var last = vl.LastDocumentLine.LineNumber;
                if (line < first || line > last) continue;
                var viewTop = GetViewYForVisualLine(vl);
                top = viewTop;
                bottom = viewTop + vl.Height;
                return true;
            }
            top = bottom = 0;
            return false;
        }

        /// <summary>获取行的“可视长度”（以字符数近似）。</summary>
        private int GetLineVisualLength(int line)
        {
            var doc = _editor.Document!;
            line = Math.Clamp(line, 1, doc.LineCount);
            var l = doc.Lines[line - 1];
            var text = doc.GetText(l.Offset, l.Length);
            return text.Length;
        }

        /// <summary>限制 VC 在合法范围内。</summary>
        private int ClampVC(int line, int vc) => Math.Clamp(vc, 0, GetLineVisualLength(line));

        /// <summary>由行号/可视列求视图坐标 X。</summary>
        private double GetXByVisualColumn(int line, int vc)
        {
            var pos = new TextViewPosition(line, 1) { VisualColumn = Math.Max(0, vc) };
            var p = _tv.GetVisualPosition(pos, VisualYPosition.LineMiddle);
            return p.X;
        }

        /// <summary>在一行中根据 X 反推最接近的可视列（使用二分搜索）。</summary>
        private int GetVisualColumnAtX(int line, double x)
        {
            var pos = new TextViewPosition(line, 1);
            var baseP = _tv.GetVisualPosition(pos, VisualYPosition.LineMiddle);
            if (x <= baseP.X) return 0;
            int max = GetLineVisualLength(line) + 50;
            int lo = 0, hi = max, ans = 0; double best = double.MaxValue;
            while (lo <= hi)
            {
                int mid = (lo + hi) / 2;
                var test = new TextViewPosition(line, 1) { VisualColumn = mid };
                var p = _tv.GetVisualPosition(test, VisualYPosition.LineMiddle);
                var d = Math.Abs(p.X - x);
                if (d < best) { best = d; ans = mid; }
                if (p.X < x) lo = mid + 1; else hi = mid - 1;
            }
            return Math.Clamp(ans, 0, max);
        }

        /// <summary>
        /// 给定行/VC 返回文档 offset，并在 VC 超出行长时返回需要补的空格数（用于列对齐）。
        /// </summary>
        private (int Offset, int PadSpaces) GetOffsetAndPadForVC(int line, int vc)
        {
            var doc = _editor.Document!;
            line = Math.Clamp(line, 1, doc.LineCount);
            var l = doc.Lines[line - 1];
            int visLen = GetLineVisualLength(line);
            int clamped = Math.Clamp(vc, 0, visLen);
            return (l.Offset + clamped, Math.Max(0, vc - visLen));
        }

        /// <summary>由文档 offset 反求 VC。</summary>
        private int GetVCAtOffset(int offset)
        {
            var doc = _editor.Document!;
            offset = Math.Clamp(offset, 0, doc.TextLength);
            var loc = doc.GetLocation(offset);
            var line = loc.Line;
            var l = doc.Lines[line - 1];
            return Math.Max(0, offset - l.Offset);
        }

        /// <summary>根据 Anchor 的 Offset 同步内部光标行/列状态（撤销/重做后调用）。</summary>
        private void SyncFromAnchors()
        {
            if (_editor.Document is null) return;
            foreach (var c in _carets)
            {
                c.Anchor = _editor.Document.CreateAnchor(Math.Clamp(c.Anchor.Offset, 0, _editor.Document.TextLength));
                c.Anchor.MovementType = AnchorMovementType.AfterInsertion;
                c.SelStart = _editor.Document.CreateAnchor(Math.Clamp(c.SelStart.Offset, 0, _editor.Document.TextLength));
                c.SelStart.MovementType = AnchorMovementType.BeforeInsertion;
                c.SelEnd = _editor.Document.CreateAnchor(Math.Clamp(c.SelEnd.Offset, 0, _editor.Document.TextLength));
                c.SelEnd.MovementType = AnchorMovementType.AfterInsertion;

                var loc = _editor.Document.GetLocation(c.Anchor.Offset);
                c.Line = loc.Line;
                c.VC = GetVCAtOffset(c.Anchor.Offset);
            }
        }
    }
}
