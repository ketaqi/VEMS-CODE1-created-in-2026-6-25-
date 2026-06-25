using System.IO;

namespace RoslynPad.Editor;

/// <summary>
/// 代码文本编辑器控件，继承自AvalonEdit的TextEditor，扩展了代码补全、提示、文件操作等功能
/// </summary>
public partial class CodeTextEditor : TextEditor
{
    private CodeEditorCompletionWindow? _completionWindow;
    private CodeEditorOverloadInsightWindow? _insightWindow;
    private ToolTip? _toolTip;

    /// <summary>
    /// 初始化<see cref="CodeTextEditor"/>类的新实例
    /// </summary>
    public CodeTextEditor()
    {
        ShowLineNumbers = true;

        Options = new TextEditorOptions
        {
            ConvertTabsToSpaces = true,
            AllowScrollBelowDocument = true,
            IndentationSize = 4,
            EnableEmailHyperlinks = false,
            ShowBoxForControlCharacters = true,
        };

        TextArea.TextView.VisualLinesChanged += OnVisualLinesChanged;
        TextArea.TextEntering += OnTextEntering;
        TextArea.TextEntered += OnTextEntered;

        var commandBindings = TextArea.CommandBindings;
        var deleteLineCommand = commandBindings.OfType<CommandBinding>().FirstOrDefault(x =>
            x.Command == AvalonEditCommands.DeleteLine);
        if (deleteLineCommand != null)
        {
            commandBindings.Remove(deleteLineCommand);
        }

        var contextMenu = new ContextMenu
        {
            ItemsSource = new[]
            {
                new MenuItem { Command = ApplicationCommands.Cut },
                new MenuItem { Command = ApplicationCommands.Copy },
                new MenuItem { Command = ApplicationCommands.Paste }
            }
        };
        ContextMenu = contextMenu;

        Initialize();
    }

    /// <summary>
    /// 部分初始化方法，供分部类扩展
    /// </summary>
    partial void Initialize();

    /// <summary>
    /// 获取一个值，指示代码补全窗口是否处于打开状态
    /// </summary>
    public bool IsCompletionWindowOpen => _completionWindow?.IsVisible == true;

    /// <summary>
    /// 关闭代码补全窗口
    /// </summary>
    public void CloseCompletionWindow()
    {
        if (_completionWindow != null)
        {
            _completionWindow.Close();
            _completionWindow = null;
        }
    }

    /// <summary>
    /// 获取一个值，指示重载提示窗口是否处于打开状态
    /// </summary>
    public bool IsInsightWindowOpen => _insightWindow?.IsVisible == true;

    /// <summary>
    /// 关闭重载提示窗口
    /// </summary>
    public void CloseInsightWindow()
    {
        if (_insightWindow != null)
        {
            _insightWindow.Close();
            _insightWindow = null;
        }
    }

    /// <summary>
    /// 处理键盘按下事件，扩展Ctrl+Space/Shift+Ctrl+Space触发补全/签名帮助
    /// </summary>
    /// <param name="e">键盘事件参数</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Space && e.HasModifiers(ModifierKeys.Control))
        {
            e.Handled = true;
            var mode = e.HasModifiers(ModifierKeys.Shift)
                ? TriggerMode.SignatureHelp
                : TriggerMode.Completion;
            _ = ShowCompletion(mode);
        }
    }

    /// <summary>
    /// 补全触发模式枚举
    /// </summary>
    private enum TriggerMode
    {
        /// <summary>文本输入触发</summary>
        Text,
        /// <summary>主动补全触发</summary>
        Completion,
        /// <summary>签名帮助触发</summary>
        SignatureHelp
    }

    /// <summary>
    /// 定义ToolTipRequest路由事件
    /// </summary>
    public static readonly RoutedEvent ToolTipRequestEvent = CommonEvent.Register<CodeTextEditor, ToolTipRequestEventArgs>(
        nameof(ToolTipRequest), RoutingStrategy.Bubble);

    /// <summary>
    /// 获取或设置异步提示请求委托
    /// </summary>
    public Func<ToolTipRequestEventArgs, Task>? AsyncToolTipRequest { get; set; }

    /// <summary>
    /// 提示请求事件，在需要显示工具提示时触发
    /// </summary>
    public event EventHandler<ToolTipRequestEventArgs> ToolTipRequest
    {
        add => AddHandler(ToolTipRequestEvent, value);
        remove => RemoveHandler(ToolTipRequestEvent, value);
    }

    /// <summary>
    /// 视觉行变更时关闭当前工具提示
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="e">事件参数</param>
    private void OnVisualLinesChanged(object? sender, EventArgs e)
    {
        _toolTip?.Close(this);
    }

    /// <summary>
    /// 鼠标悬停停止时关闭工具提示
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="e">鼠标事件参数</param>
    private void OnMouseHoverStopped(object? sender, MouseEventArgs e)
    {
        if (_toolTip != null)
        {
            _toolTip.Close(this);
            e.Handled = true;
        }
    }

    /// <summary>
    /// 处理鼠标悬停事件，触发工具提示请求并显示提示
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="e">鼠标事件参数</param>
    private async void OnMouseHover(object? sender, MouseEventArgs e)
    {
        TextViewPosition? position;
        try
        {
            position = TextArea.TextView.GetPositionFloor(e.GetPosition(TextArea.TextView) + TextArea.TextView.ScrollOffset);
        }
        catch (ArgumentOutOfRangeException)
        {
            // TODO: check why this happens
            e.Handled = true;
            return;
        }
        var args = new ToolTipRequestEventArgs { InDocument = position.HasValue };
        if (!position.HasValue || position.Value.Location.IsEmpty || position.Value.IsAtEndOfLine)
        {
            return;
        }

        args.LogicalPosition = position.Value.Location;
        args.Position = Document.GetOffset(position.Value.Line, position.Value.Column);

        RaiseEvent(args);

        if (args.ContentToShow == null)
        {
            var asyncRequest = AsyncToolTipRequest?.Invoke(args);
            if (asyncRequest != null)
            {
                await asyncRequest.ConfigureAwait(true);
            }
        }

        if (args.ContentToShow == null)
        {
            return;
        }

        if (_toolTip == null)
        {
            _toolTip = new ToolTip { MaxWidth = 400 };
            InitializeToolTip();
        }

        if (args.ContentToShow is string stringContent)
        {
            _toolTip.SetContent(this, new TextBlock
            {
                Text = stringContent,
                TextWrapping = TextWrapping.Wrap
            });
        }
        else
        {
            _toolTip.SetContent(this, new ContentPresenter
            {
                Content = args.ContentToShow,
                MaxWidth = 400
            });
        }

        e.Handled = true;
        _toolTip.Open(this);

        AfterToolTipOpen();
    }

    /// <summary>
    /// 初始化工具提示控件（供分部类扩展）
    /// </summary>
    partial void InitializeToolTip();

    /// <summary>
    /// 工具提示打开后执行的操作（供分部类扩展）
    /// </summary>
    partial void AfterToolTipOpen();

    #region Open & Save File

    /// <summary>
    /// 打开指定路径的文件并加载到编辑器
    /// </summary>
    /// <param name="fileName">文件完整路径</param>
    /// <exception cref="FileNotFoundException">当指定文件不存在时抛出</exception>
    public void OpenFile(string fileName)
    {
        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException(fileName);
        }

        _completionWindow?.Close();
        _insightWindow?.Close();

        Load(fileName);
        Document.FileName = fileName;

        SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(fileName));
    }

    /// <summary>
    /// 将当前文档保存到文件（仅当Document.FileName有值时生效）
    /// </summary>
    /// <returns>保存成功返回true，否则返回false</returns>
    public bool SaveFile()
    {
        if (string.IsNullOrEmpty(Document.FileName))
        {
            return false;
        }

        Save(Document.FileName);
        return true;
    }

    #endregion

    #region Code Completion

    /// <summary>
    /// 获取或设置代码补全提供器
    /// </summary>
    public ICodeEditorCompletionProvider? CompletionProvider { get; set; }

    /// <summary>
    /// 文本输入完成时触发补全检查
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="e">文本组合事件参数</param>
    private void OnTextEntered(object? sender, TextCompositionEventArgs e)
    {
        _ = ShowCompletion(TriggerMode.Text);
    }

    /// <summary>
    /// 根据指定触发模式显示代码补全或签名帮助窗口
    /// </summary>
    /// <param name="triggerMode">补全触发模式</param>
    /// <returns>异步任务</returns>
    private async Task ShowCompletion(TriggerMode triggerMode)
    {
        if (CompletionProvider == null)
        {
            return;
        }

        GetCompletionDocument(out var offset);
        var completionChar = triggerMode == TriggerMode.Text ? Document.GetCharAt(offset - 1) : (char?)null;
        var results = await CompletionProvider.GetCompletionData(offset, completionChar,
                    triggerMode == TriggerMode.SignatureHelp).ConfigureAwait(true);
        if (results.OverloadProvider != null)
        {
            results.OverloadProvider.Refresh();

            if (_insightWindow != null && _insightWindow.IsOpen())
            {
                _insightWindow.Provider = results.OverloadProvider;
            }
            else
            {
                _insightWindow = new CodeEditorOverloadInsightWindow(TextArea)
                {
                    Provider = results.OverloadProvider,
                };

                InitializeInsightWindow();

                _insightWindow.Closed += (o, args) => _insightWindow = null;
                _insightWindow.Show();
            }
            return;
        }

        if (_completionWindow?.IsOpen() != true && results.CompletionData != null && results.CompletionData.Any())
        {
            _insightWindow?.Close();

            // 按下点号后打开代码补全窗口
            _completionWindow = new CodeEditorCompletionWindow(TextArea)
            {
                MinWidth = 300,
                CloseWhenCaretAtBeginning = triggerMode == TriggerMode.Completion || triggerMode == TriggerMode.Text,
                UseHardSelection = results.UseHardSelection,
            };

            InitializeCompletionWindow();

            if (completionChar != null && char.IsLetterOrDigit(completionChar.Value))
            {
                _completionWindow.StartOffset -= 1;
            }

            var data = _completionWindow.CompletionList.CompletionData;
            ICompletionDataEx? selected = null;
            foreach (var completion in results.CompletionData)
            {
                if (completion.IsSelected)
                {
                    selected = completion;
                }

                data.Add(completion);
            }

            try
            {
                _completionWindow.CompletionList.SelectedItem = selected;
            }
            catch (Exception)
            {
                // TODO-AV: Fix this in AvaloniaEdit
            }

            _completionWindow.Closed += (o, args) => { _completionWindow = null; };
            _completionWindow.Show();
        }
    }

    /// <summary>
    /// 初始化重载提示窗口（供分部类扩展）
    /// </summary>
    partial void InitializeInsightWindow();

    /// <summary>
    /// 初始化代码补全窗口（供分部类扩展）
    /// </summary>
    partial void InitializeCompletionWindow();

    /// <summary>
    /// 文本输入过程中处理补全窗口的插入逻辑（非标识符字符时插入选中项）
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="args">文本组合事件参数</param>
    private void OnTextEntering(object? sender, TextCompositionEventArgs args)
    {
        if (args.Text?.Length > 0 && _completionWindow != null)
        {
            if (!IsCharIdentifier(args.Text[0]))
            {
                // 补全窗口打开时输入非标识符字符，插入当前选中项
                _completionWindow.CompletionList.RequestInsertion(args);
            }
        }
        // 不设置e.Handled=true，保留输入字符的默认行为
    }

    /// <summary>
    /// 检查指定字符是否为合法的标识符字符（字母、数字、下划线）
    /// </summary>
    /// <param name="c">待检查的字符</param>
    /// <returns>合法返回true，否则返回false</returns>
    private bool IsCharIdentifier(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_';
    }

    /// <summary>
    /// 获取用于代码补全的文档，并输出当前光标偏移量
    /// </summary>
    /// <param name="offset">输出参数，当前光标在文档中的偏移量</param>
    /// <returns>当前编辑器的文档实例</returns>
    protected virtual IDocument GetCompletionDocument(out int offset)
    {
        offset = CaretOffset;
        return Document;
    }

    #endregion
}
