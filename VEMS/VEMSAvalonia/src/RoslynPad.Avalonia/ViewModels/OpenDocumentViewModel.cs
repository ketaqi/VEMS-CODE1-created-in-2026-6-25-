using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Composition;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using AvaloniaEdit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet.Configuration;
using NuGet.Packaging;
using RoslynPad.Build;

using RoslynPad.Editor;
using RoslynPad.Roslyn.Rename;
using RoslynPad.UI;
using RoslynPad.UI.Dialogs;
using RoslynPad.Utilities;
using FileDialogFilter = RoslynPad.UI.FileDialogFilter;

namespace RoslynPad.ViewModels;

/// <summary>
/// 打开的文档（编辑页签）的 ViewModel。
/// <para>负责：文档生命周期、保存/自动保存、运行/终止、结果输出、格式化、重命名符号、搜索替换、后台运行等。</para>
/// </summary>
[Export]
public class OpenDocumentViewModel : NotificationObject, IDisposable, IResultsViewModel
{
    /// <summary>默认新建文档名（未关联真实文件时显示）。</summary>
    private const string DefaultDocumentName = "New";

    /// <summary>普通 C# 文件扩展名。</summary>
    private const string RegularFileExtension = ".cs";

    /// <summary>C# 脚本文件扩展名。</summary>
    private const string ScriptFileExtension = ".csx";

    /// <summary>IL 视图默认提示文本。</summary>
    private const string DefaultILText = "// Run to view IL";

    /// <summary>依赖注入容器。</summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>UI 调度器（用于切回 UI 线程执行属性变更/命令等）。</summary>
    private readonly IAppDispatcher _dispatcher;

    /// <summary>遥测/错误上报。</summary>
    private readonly ITelemetryProvider _telemetryProvider;

    /// <summary>日志记录器。</summary>
    private readonly ILogger<OpenDocumentViewModel> _logger;

    /// <summary>平台工厂：提供可用执行平台及 dotnet 可执行路径等。</summary>
    private readonly IPlatformsFactory _platformsFactory;

    /// <summary>运行结果集合（输出/错误/还原信息等）。</summary>
    private readonly ObservableCollection<IResultObject> _results;

    /// <summary>NuGet restore 结果（单独缓存，用于 ClearResults 时恢复）。</summary>
    private readonly List<RestoreResultObject> _restoreResults;

    /// <summary>执行宿主（负责编译/运行脚本或程序）。</summary>
    private ExecutionHost? _executionHost;

    /// <summary>执行宿主参数（工作目录、引用、导入等）。</summary>
    private ExecutionHostParameters? _executionHostParameters;

    /// <summary>运行取消令牌源（用于终止当前运行）。</summary>
    private CancellationTokenSource? _runCts;

    private bool _isRunning;
    private bool _isDirty;
    private ExecutionPlatform? _platform;
    private bool _isSaving;
    private IDisposable? _viewDisposable;
    private Action<ExceptionResultObject?>? _onError;
    private Func<TextSpan>? _getSelection;
    private string? _ilText;
    private bool _isInitialized;
    private bool _isLiveMode;
    private Timer? _liveModeTimer;
    private DocumentViewModel? _document;
    private bool _isRestoring;
    private IReadOnlyList<ExecutionPlatform>? _availablePlatforms;
    private DocumentId? _documentId;
    private bool _restoreSuccessful;
    private double? _reportedProgress;
    private SourceCodeKind? _sourceCodeKind;
    private string? _selectedText;

    /// <summary>
    /// 是否当前作为“浮窗”显示（由视图在附着可视树时设置/清空）。
    /// <para>该属性仅作为状态标记，不影响核心逻辑。</para>
    /// </summary>
    public bool IsFloating { get; set; }

    /// <summary>
    /// 由视图提供的“把自己置前激活”的回调。
    /// <para>MainViewModel 只需调用，不依赖 Avalonia 类型。</para>
    /// </summary>
    public Action? ActivateView { get; set; }

    /// <summary>当前打开文档实例的唯一标识（用于 BuildPath 等）。</summary>
    public string Id { get; }

    /// <summary>构建/运行产生的临时目录（每个文档一个）。</summary>
    public string BuildPath { get; }

    /// <summary>
    /// 运行工作目录：
    /// <para>若已关联 Document，则为 Document 所在目录；否则为文档根目录。</para>
    /// </summary>
    public string WorkingDirectory => Document != null
        ? Path.GetDirectoryName(Document.Path)!
        : MainViewModel.DocumentRoot.Path;

    /// <summary>
    /// 当前选中文本（若非空，运行时可只运行选中片段）。
    /// </summary>
    public string? SelectedText
    {
        get => _selectedText;
        set => SetProperty(ref _selectedText, value);
    }

    /// <summary>
    /// 输出结果集合（只读枚举）。
    /// </summary>
    public IEnumerable<IResultObject> Results => _results;

    /// <summary>切换 Live Mode（自动运行）命令。</summary>
    public IDelegateCommand ToggleLiveModeCommand { get; }

    /// <summary>设置默认平台命令（写入设置）。</summary>
    public IDelegateCommand SetDefaultPlatformCommand { get; }

    /// <summary>
    /// 是否开启 Live Mode：
    /// <para>开启后会立即触发一次 RunAsync，并启动定时器（由 OnTextChanged 触发延迟运行）。</para>
    /// </summary>
    public bool IsLiveMode
    {
        get => _isLiveMode;
        private set
        {
            if (!SetProperty(ref _isLiveMode, value)) return;
            RunCommand.RaiseCanExecuteChanged();

            if (value)
            {
                _ = RunAsync();

                _liveModeTimer ??= new Timer(o => _dispatcher.InvokeAsync(() =>
                {
                    _ = RunAsync();
                }), state: null, Timeout.Infinite, Timeout.Infinite);
            }
        }
    }

    /// <summary>
    /// 源代码类型（Regular 或 Script）。
    /// <para>默认根据 Document 扩展名判断；也允许外部覆盖。</para>
    /// </summary>
    public SourceCodeKind SourceCodeKind
    {
        get
        {
            if (_sourceCodeKind is not null)
            {
                return _sourceCodeKind.Value;
            }

            var isScript = Path.GetExtension(Document?.Name)?.Equals(ScriptFileExtension, StringComparison.OrdinalIgnoreCase);
            return isScript is null
                ? throw new InvalidOperationException("Document not initialized")
                : (_sourceCodeKind ??= isScript == true ? SourceCodeKind.Script : SourceCodeKind.Regular);
        }
        set => _sourceCodeKind = value;
    }

    /// <summary>
    /// 根据 <see cref="SourceCodeKind"/> 返回默认扩展名（.cs 或 .csx）。
    /// </summary>
    private string GetFileExtension() =>
        SourceCodeKind == SourceCodeKind.Script ? ScriptFileExtension : RegularFileExtension;

    /// <summary>
    /// 当前关联的文档树节点（可能为空：未保存的新文档）。
    /// <para>更换 Document 时会订阅/退订其属性变化，以刷新标题与工作目录等。</para>
    /// </summary>
    public DocumentViewModel? Document
    {
        get => _document;
        private set
        {
            if (ReferenceEquals(_document, value)) return;

            if (_document is not null)
                _document.PropertyChanged -= OnDocumentPropertyChanged;

            _document = value;

            if (_document is not null)
                _document.PropertyChanged += OnDocumentPropertyChanged;

            OnPropertyChanged(nameof(Document));
            OnPropertyChanged(nameof(Title));          // 页签标题刷新
            OnPropertyChanged(nameof(TabTitle));       // 标签标题（含 *）刷新
            OnPropertyChanged(nameof(WorkingDirectory));
        }
    }

    /// <summary>
    /// IL 文本（当 ShowIL=true 且运行完成/反汇编后更新）。
    /// </summary>
    public string ILText
    {
        get => _ilText ?? string.Empty;
        private set => SetProperty(ref _ilText, value);
    }

    /// <summary>
    /// 构造函数：初始化依赖、命令、平台列表、默认值及与 MainViewModel 的联动。
    /// </summary>
    [ImportingConstructor]
    public OpenDocumentViewModel(
        IServiceProvider serviceProvider,
        MainViewModel mainViewModel,
        ICommandProvider commands,
        IAppDispatcher appDispatcher,
        ITelemetryProvider telemetryProvider,
        ILogger<OpenDocumentViewModel> logger)
    {
        // 文档唯一 ID 与构建目录
        Id = Guid.NewGuid().ToString("n");
        BuildPath = Path.Combine(Path.GetTempPath(), "roslynpad", "build", Id);
        Directory.CreateDirectory(BuildPath);

        _telemetryProvider = telemetryProvider;
        _logger = logger;
        _platformsFactory = serviceProvider.GetRequiredService<IPlatformsFactory>();
        _serviceProvider = serviceProvider;
        _results = [];
        _restoreResults = [];

        MainViewModel = mainViewModel;

        // 输出字体大小初始化并与设置联动
        FontSize = MainViewModel.OutputFontSize;
        MainViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.OutputFontSize))
            {
                FontSize = MainViewModel.OutputFontSize;
            }
        };

        CommandProvider = commands;

        // NuGet 文档 VM
        NuGet = serviceProvider.GetRequiredService<NuGetDocumentViewModel>();

        // 初始设为 true：允许首次运行直接等待 restore
        _restoreSuccessful = true;

        _dispatcher = appDispatcher;

        // 命令初始化
        OpenBuildPathCommand = commands.Create(OpenBuildPath);
        RunCommand = commands.CreateAsync(RunInBackgroundAsync, () => !IsRunning && RestoreSuccessful && Platform != null);
        TerminateCommand = commands.CreateAsync(TerminateSelectedAsync, () => Platform != null);
        FormatDocumentCommand = commands.CreateAsync(FormatDocumentAsync);
        CommentSelectionCommand = commands.CreateAsync(() => CommentUncommentSelectionAsync(CommentAction.Comment));
        UncommentSelectionCommand = commands.CreateAsync(() => CommentUncommentSelectionAsync(CommentAction.Uncomment));
        RenameSymbolCommand = commands.CreateAsync(RenameSymbolAsync);
        ToggleLiveModeCommand = commands.Create(() => IsLiveMode = !IsLiveMode);
        SetDefaultPlatformCommand = commands.Create(SetDefaultPlatform);

        // IL 默认提示
        ILText = DefaultILText;

        // 平台初始化
        InitializePlatforms();

        // 搜索面板（构造时默认可用）
        IsSearchPanelVisible = true;
        ToggleSearchPanelCommand = commands.Create(() =>
        {
            IsSearchPanelVisible = false;
            SearchClearHighlightRequested?.Invoke();
        });
        FindTextCommand = commands.Create(FindText);
        ReplaceTextCommand = commands.Create(ExecuteReplaceText);
        JumpToNextHighlightCommand = commands.Create(JumpToNextHighlight);
        JumpToPrevHighlightCommand = commands.Create(JumpToPrevHighlight);

        // Debug 命令（使用 Debug 优化级别）
        DebugCommand = commands.CreateAsync(DebugAsync, () => !IsRunning && RestoreSuccessful && Platform != null);

        // 复制输出/错误相关命令（使用 RelayCommand）
        CopyErrorCommand = new ViewModels.RelayCommand<object>(CopyError);
        CopyErrorLineCommand = new ViewModels.RelayCommand<object>(CopyErrorLine);

        // 保存命令
        SaveAsCommand = commands.CreateAsync(SaveAsAsync);
        ClearResultsCommand = commands.Create(ClearOutputResults);
        CopyAllResultsCommand = commands.Create(CopyAllOutputResults);
        SaveCommand = commands.CreateAsync(() => SaveAsync(promptSave: false));
        SaveAsCommand = commands.CreateAsync(SaveAsAsync);

        // 初始化编辑器/输出字体
        OnEditorFontFamilyChanged(mainViewModel.EditorFontFamily);
        OnEditorFontFamilyChanged1(mainViewModel.EditorFontFamily1);
    }

    // ===================== 字体/编辑器相关 =====================

    /// <summary>
    /// 当输出区域字体变更时更新（由外部调用/绑定）。
    /// </summary>
    public void OnEditorFontFamilyChanged1(string newFont1)
    {
        EditorFontFamily1 = newFont1;
        Console.WriteLine($"[OpenDocumentViewModel] OutputFontFamily 已变更: {EditorFontFamily1}");
    }

    private string _editorFontFamily1 = "Consolas";

    /// <summary>输出区域字体名称。</summary>
    public string EditorFontFamily1
    {
        get => _editorFontFamily1;
        set
        {
            if (_editorFontFamily1 != value)
            {
                _editorFontFamily1 = value;
                OnPropertyChanged(nameof(EditorFontFamily1));
            }
        }
    }

    private double _fontSize;

    /// <summary>输出区域字体大小。</summary>
    public double FontSize
    {
        get => _fontSize;
        set
        {
            if (_fontSize != value)
            {
                _fontSize = value;
                OnPropertyChanged(nameof(FontSize));
                Console.WriteLine($"[OpenDocumentViewModel] OutputFontSize 已变更: {_fontSize}");
            }
        }
    }

    private string _editorFontFamily = "Consolas";

    /// <summary>编辑器字体名称。</summary>
    public string EditorFontFamily
    {
        get => _editorFontFamily;
        set
        {
            if (_editorFontFamily != value)
            {
                _editorFontFamily = value;
                OnPropertyChanged(nameof(EditorFontFamily));
            }
        }
    }

    /// <summary>
    /// 编辑器控件引用（由视图注入/赋值，用于实时刷新字体等）。
    /// </summary>
    public RoslynCodeEditor? EditorControl { get; set; }

    /// <summary>
    /// 输出结果控件引用（由视图注入/赋值）。
    /// </summary>
    public ResultObject? ResultControl { get; set; }

    /// <summary>
    /// 当编辑器字体变更时更新，并尝试立即应用到 EditorControl。
    /// </summary>
    public void OnEditorFontFamilyChanged(string newFont)
    {
        EditorFontFamily = newFont;
        OnPropertyChanged(nameof(EditorFontFamily));
        Console.WriteLine($"[OpenDocumentViewModel] EditorFontFamily 已变更: {EditorFontFamily}");

        // 使编辑器字体实时刷新（若控件已就绪）
        if (EditorControl != null)
        {
            EditorControl.FontFamily = new FontFamily(newFont);
            EditorControl.InvalidateVisual();
            Console.WriteLine($"[OpenDocumentViewModel] EditorControl 字体已应用: {EditorControl.FontFamily}");
        }
        else
        {
            Console.WriteLine("[OpenDocumentViewModel] EditorControl 为 null，无法刷新字体");
        }
    }

    /// <summary>复制全部结果命令。</summary>
    public IDelegateCommand CopyAllResultsCommand { get; }

    /// <summary>清空结果命令。</summary>
    public IDelegateCommand ClearResultsCommand { get; }

    /// <summary>
    /// 清空当前输出结果集合，并触发 UI 刷新与“结果可用”通知。
    /// </summary>
    private void ClearOutputResults()
    {
        lock (_results)
        {
            _results.Clear();
        }
        ResultsAvailable?.Invoke();
        OnPropertyChanged(nameof(ResultsText));
    }

    /// <summary>
    /// 将所有结果拼接为文本，并请求复制到剪贴板。
    /// <para>同时会触发 RequestCopyText（用于视图/外部处理）。</para>
    /// </summary>
    private void CopyAllOutputResults()
    {
        var builder = new StringBuilder();
        lock (_results)
        {
            foreach (var result in _results)
            {
                result.WriteTo(builder);
                builder.AppendLine();
            }
        }
        var allText = builder.ToString();
        RequestCopyText?.Invoke(allText);
        CopyToClipboard(allText);
    }

    /// <summary>另存为命令。</summary>
    public IDelegateCommand SaveAsCommand { get; }

    // ===================== 执行宿主/平台初始化 =====================

    /// <summary>
    /// 初始化执行宿主（ExecutionHost）并订阅其事件。
    /// <para>仅在 DocumentId/Platform 等准备好后调用。</para>
    /// </summary>
    [MemberNotNull(nameof(_executionHost))]
    private void InitializeExecutionHost()
    {
        var roslynHost = MainViewModel.RoslynHost;

        _executionHostParameters = new ExecutionHostParameters(
            BuildPath,
            _serviceProvider.GetRequiredService<NuGetViewModel>().ConfigPath,
            roslynHost.DefaultImports,
            roslynHost.DisabledDiagnostics,
            WorkingDirectory,
            SourceCodeKind);

        _executionHost = new ExecutionHost(_executionHostParameters, roslynHost, _logger)
        {
            Name = Document?.Name ?? "Untitled",
            DocumentId = DocumentId,
            Platform = Platform.NotNull(),
            DotNetExecutable = _platformsFactory.DotNetExecutable
        };

        // 订阅运行期事件：输出/异常/输入/编译错误/IL/Restore 状态/进度
        _executionHost.Dumped += ExecutionHostOnDump;
        _executionHost.Error += ExecutionHostOnError;
        _executionHost.ReadInput += ExecutionHostOnInputRequest;
        _executionHost.CompilationErrors += ExecutionHostOnCompilationErrors;
        _executionHost.Disassembled += ExecutionHostOnDisassembled;
        _executionHost.RestoreStarted += OnRestoreStarted;
        _executionHost.RestoreCompleted += OnRestoreCompleted;
        _executionHost.ProgressChanged += p => ReportedProgress = p.Progress;
    }

    /// <summary>
    /// 将当前选择的平台写入设置，作为默认平台。
    /// </summary>
    private void SetDefaultPlatform()
    {
        if (Platform is not null)
        {
            MainViewModel.Settings.DefaultPlatformName = Platform.ToString();
        }
    }

    /// <summary>
    /// 初始化可用执行平台列表，并生成字符串名称列表、以及映射到 PlatformID 的辅助列表。
    /// </summary>
    private void InitializePlatforms()
    {
        AvailablePlatforms = _platformsFactory.GetExecutionPlatforms();
        var names = AvailablePlatforms.Select(p => p.ToString()).ToArray().AsReadOnly();
        AvailablePlatformNames = names;

        // 将平台名称粗略映射到 PlatformID（用于 UI 分类/显示等）
        AvailablePlatforms1 = (AvailablePlatforms ?? Array.Empty<ExecutionPlatform>())
            .Select(ep =>
            {
                var name = (ep.Name ?? "").ToLowerInvariant();
                if (name.Contains("win") || name.Contains("windows")) return PlatformID.Win32NT;
                if (name.Contains("mac") || name.Contains("osx") || name.Contains("macos")) return PlatformID.MacOSX;
                if (name.Contains("linux") || name.Contains("unix")) return PlatformID.Unix;
                if (name.Contains("wasm") || name.Contains("browser")) return PlatformID.Other;
                if (name.Contains("xbox")) return PlatformID.Xbox;
                return PlatformID.Other;
            })
            .ToList();
    }

    /// <summary>
    /// NuGet Restore 开始回调：标记正在还原。
    /// </summary>
    private void OnRestoreStarted() => IsRestoring = true;

    /// <summary>
    /// NuGet Restore 完成回调：
    /// <para>成功：更新 RoslynHost 中当前文档的引用与分析器，然后触发 DocumentUpdated。</para>
    /// <para>失败：将错误输出到结果集合。</para>
    /// </summary>
    private void OnRestoreCompleted(RestoreResult restoreResult)
    {
        if (_executionHost is null)
        {
            return;
        }

        IsRestoring = false;

        lock (_results)
        {
            _restoreResults.Clear();
            //ClearResults();
        }

        if (restoreResult.Success)
        {
            var host = MainViewModel.RoslynHost;
            var document = host.GetDocument(DocumentId);
            if (document == null)
            {
                return;
            }

            var project = document.Project;

            // 将 restore 得到的引用与分析器应用到项目
            project = project
                .WithMetadataReferences(_executionHost.MetadataReferences)
                .WithAnalyzerReferences(_executionHost.Analyzers);

            document = project.GetDocument(DocumentId);

            host.UpdateDocument(document!);
            OnDocumentUpdated();
        }
        else
        {
            foreach (var error in restoreResult.Errors)
            {
                AddRestoreResult(new RestoreResultObject(error, "Error"));
            }
        }

        RestoreSuccessful = restoreResult.Success;
    }

    /// <summary>是否正在进行 NuGet restore。</summary>
    public bool IsRestoring
    {
        get => _isRestoring;
        private set => SetProperty(ref _isRestoring, value);
    }

    /// <summary>
    /// Restore 是否成功：
    /// <para>变化时会刷新 RunCommand 的可执行状态。</para>
    /// </summary>
    public bool RestoreSuccessful
    {
        get => _restoreSuccessful;
        private set
        {
            if (SetProperty(ref _restoreSuccessful, value))
            {
                _dispatcher.InvokeAsync(RunCommand.RaiseCanExecuteChanged);
            }
        }
    }

    /// <summary>
    /// 文档（Roslyn 文档对象）更新事件触发器。
    /// </summary>
    private void OnDocumentUpdated() => DocumentUpdated?.Invoke(this, EventArgs.Empty);

    /// <summary>当 Roslyn 文档引用/内容更新时触发。</summary>
    public event EventHandler? DocumentUpdated;

    /// <summary>请求读取输入（运行期需要用户输入时）。</summary>
    public event Action? ReadInput;

    /// <summary>结果集合有变更时的通知。</summary>
    public event Action? ResultsAvailable;

    /// <summary>
    /// 添加运行结果对象到结果集合，并通知 UI 刷新。
    /// </summary>
    public void AddResult(IResultObject o)
    {
        lock (_results)
        {
            _results.Add(o);
        }

        ResultsAvailable?.Invoke();
        OnPropertyChanged(nameof(ResultsText));
    }

    /// <summary>
    /// 添加 restore 结果，并同步输出到 MainViewModel 的输出窗口。
    /// </summary>
    private void AddRestoreResult(RestoreResultObject o)
    {
        lock (_results)
        {
            _restoreResults.Add(o);
            MainViewModel.OutputResult(
                $"[{o.Severity}]",
                o.Message,
                o.Severity,
                null
            );
        }
        OnPropertyChanged(nameof(ResultsText));
    }

    /// <summary>
    /// 执行宿主请求输入：切到 UI 线程触发 <see cref="ReadInput"/>。
    /// </summary>
    private void ExecutionHostOnInputRequest() => _dispatcher.InvokeAsync(() =>
    {
        ReadInput?.Invoke();
    }, AppDispatcherPriority.Low);

    /// <summary>
    /// 执行宿主 Dump 输出：写入 MainViewModel 的输出窗口。
    /// </summary>
    private void ExecutionHostOnDump(ResultObject result)
    {
        MainViewModel.OutputResult(result.Header ?? "[Exception]", result.Value ?? "", result.Type, result.LineNumber);
    }

    /// <summary>
    /// 执行宿主错误输出：切到 UI 线程后通知错误回调并输出到 MainViewModel。
    /// </summary>
    private void ExecutionHostOnError(ExceptionResultObject errorResult) => _dispatcher.InvokeAsync(() =>
    {
        _onError?.Invoke(errorResult);
        if (errorResult != null)
        {
            MainViewModel.OutputResult(
                errorResult.Header ?? "[Exception]",
                errorResult.Value ?? "",
                errorResult.Type ?? "异常",
                errorResult.LineNumber
            );
        }
    }, AppDispatcherPriority.Low);

    /// <summary>
    /// 执行宿主编译错误：逐条输出到 MainViewModel。
    /// </summary>
    private void ExecutionHostOnCompilationErrors(IList<CompilationErrorResultObject> errors)
    {
        foreach (var error in errors)
        {
            MainViewModel.OutputResult(
                $"[{error.Severity}]",
                $"{error.Message} (Code={error.ErrorCode}, Col={error.Column})",
                "编译错误",
                error.LineNumber
            );
        }
    }

    /// <summary>
    /// 执行宿主反汇编回调：更新 IL 文本。
    /// </summary>
    private void ExecutionHostOnDisassembled(string il) => ILText = il;

    /// <summary>
    /// 绑定 Document（来自文档树），并根据其 autosave 状态初始化 IsDirty。
    /// </summary>
    public void SetDocument(DocumentViewModel? document)
    {
        Document = document;
        IsDirty = document?.IsAutoSave == true;
    }

    /// <summary>
    /// Document 属性变化监听：当名称或路径变动时刷新标题与工作目录。
    /// </summary>
    private void OnDocumentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DocumentViewModel.Name) ||
            e.PropertyName == nameof(DocumentViewModel.Path))
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(TabTitle));
            OnPropertyChanged(nameof(WorkingDirectory));
        }
    }

    /// <summary>
    /// 向执行宿主发送输入（例如 Console.ReadLine 的输入）。
    /// </summary>
    public void SendInput(string input) => _ = _executionHost?.SendInputAsync(input);

    // ===================== 重命名/注释/格式化 =====================

    /// <summary>
    /// 触发 Roslyn “重命名符号”流程：
    /// <para>从当前光标选择处获取符号，弹出重命名对话框，确认后更新文档。</para>
    /// </summary>
    private async Task RenameSymbolAsync()
    {
        var host = MainViewModel.RoslynHost;
        var document = host.GetDocument(DocumentId);
        if (document == null || _getSelection == null)
        {
            return;
        }

        var symbol = await RenameHelper.GetRenameSymbol(document, _getSelection().Start).ConfigureAwait(true);
        if (symbol == null) return;

        var dialog = _serviceProvider.GetRequiredService<IRenameSymbolDialog>();
        dialog.Initialize(symbol.Name);
        await dialog.ShowAsync().ConfigureAwait(true);
        if (dialog.ShouldRename)
        {
            var newSolution = await Renamer.RenameSymbolAsync(
                document.Project.Solution,
                symbol,
                new SymbolRenameOptions(),
                dialog.SymbolName ?? string.Empty).ConfigureAwait(true);

            var newDocument = newSolution.GetDocument(DocumentId);
            // TODO: 可能需要更新��个 solution（这里按原逻辑仅更新当前文档）
            host.UpdateDocument(newDocument!);
        }
        OnEditorFocus();
    }

    /// <summary>注释/取消注释操作类型。</summary>
    private enum CommentAction
    {
        Comment,
        Uncomment
    }

    /// <summary>
    /// 对选中行执行“注释/取消注释”（单行注释 //）。
    /// </summary>
    private async Task CommentUncommentSelectionAsync(CommentAction action)
    {
        const string singleLineCommentString = "//";

        var document = MainViewModel.RoslynHost.GetDocument(DocumentId);
        if (document == null)
        {
            return;
        }

        if (_getSelection == null)
        {
            return;
        }

        var selection = _getSelection();

        var documentText = await document.GetTextAsync().ConfigureAwait(false);
        var changes = new List<TextChange>();
        var lines = documentText.Lines.SkipWhile(x => !x.Span.IntersectsWith(selection))
            .TakeWhile(x => x.Span.IntersectsWith(selection)).ToArray();

        if (action == CommentAction.Comment)
        {
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(documentText.GetSubText(line.Span).ToString()))
                {
                    changes.Add(new TextChange(new TextSpan(line.Start, 0), singleLineCommentString));
                }
            }
        }
        else if (action == CommentAction.Uncomment)
        {
            foreach (var line in lines)
            {
                var text = documentText.GetSubText(line.Span).ToString();
                if (text.TrimStart().StartsWith(singleLineCommentString, StringComparison.Ordinal))
                {
                    changes.Add(new TextChange(new TextSpan(
                        line.Start + text.IndexOf(singleLineCommentString, StringComparison.Ordinal),
                        singleLineCommentString.Length), string.Empty));
                }
            }
        }

        if (changes.Count == 0) return;

        MainViewModel.RoslynHost.UpdateDocument(document.WithText(documentText.WithChanges(changes)));
        if (action == CommentAction.Uncomment && MainViewModel.Settings.FormatDocumentOnComment)
        {
            await FormatDocumentAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 格式化当前文档并回写到 RoslynHost。
    /// </summary>
    private async Task FormatDocumentAsync()
    {
        var document = MainViewModel.RoslynHost.GetDocument(DocumentId);
        var formattedDocument = await Formatter.FormatAsync(document!).ConfigureAwait(false);
        MainViewModel.RoslynHost.UpdateDocument(formattedDocument);
    }

    // ===================== 平台选择相关 =====================

    /// <summary>
    /// 可用执行平台列表。
    /// </summary>
    public IReadOnlyList<ExecutionPlatform> AvailablePlatforms
    {
        get => _availablePlatforms ?? throw new ArgumentNullException(nameof(_availablePlatforms));
        private set => SetProperty(ref _availablePlatforms, value);
    }

    private List<PlatformID> _availablePlatformIds1 = new();

    /// <summary>
    /// 可用平台的 PlatformID 映射（用于 UI 分类/展示）。
    /// </summary>
    public List<PlatformID> AvailablePlatforms1
    {
        get => _availablePlatformIds1 ?? throw new ArgumentNullException(nameof(_availablePlatformIds1));
        private set => SetProperty(ref _availablePlatformIds1, value);
    }

    /// <summary>
    /// 当前选择的执行平台。
    /// <para>变更时会更新 ExecutionHost、刷新引用、更新命令可用性，并在初始化后尝试终止当前执行。</para>
    /// </summary>
    public ExecutionPlatform? Platform
    {
        get => _platform;
        set
        {
            if (value == null) return;

            if (SetProperty(ref _platform, value))
            {
                if (_executionHost is not null)
                {
                    _executionHost.Platform = value;
                }

                UpdatePackages();

                RunCommand.RaiseCanExecuteChanged();
                TerminateCommand.RaiseCanExecuteChanged();

                if (_isInitialized)
                {
                    TerminateCommand.Execute();
                }
            }
        }
    }

    // 用于平台名称的中间量（字符串列表）
    private IReadOnlyList<string>? _availablePlatformNames;
    private string? _selectedPlatformName;

    /// <summary>
    /// 可用平台名称列表（供以 string 形式绑定/选择）。
    /// </summary>
    public IReadOnlyList<string> AvailablePlatformNames
    {
        get => _availablePlatformNames ?? throw new ArgumentNullException(nameof(_availablePlatformNames));
        private set => SetProperty(ref _availablePlatformNames, value ?? Array.Empty<string>());
    }

    /// <summary>
    /// 通过字符串读写选中的平台名称：
    /// <para>set 时会匹配并赋值给 <see cref="Platform"/>，从而触发原有平台切换逻辑。</para>
    /// </summary>
    public string SelectedPlatformName
    {
        get => _selectedPlatformName ?? Platform?.ToString() ?? string.Empty;
        set
        {
            if (value is null) throw new InvalidOperationException();

            if (SetProperty(ref _selectedPlatformName, value))
            {
                // 根据字符串匹配可用平台
                if (AvailablePlatforms != null && AvailablePlatforms.Count > 0)
                {
                    var matched = AvailablePlatforms.FirstOrDefault(p =>
                        string.Equals(p.ToString(), value, StringComparison.Ordinal));
                    if (matched != null)
                    {
                        Platform = matched;
                    }
                    else
                    {
                        // 未匹配到则忽略（保持当前 Platform）
                    }
                }
            }

            MainViewModel.OutputResult("[SelectedPlatformName]", "The selected platform has been changed to " + value, null, null);
        }
    }

    // ===================== 终止/运行控制（前台） =====================

    /// <summary>
    /// 终止当前执行（前台）。
    /// <para>会重置取消令牌，并调用 ExecutionHost.TerminateAsync。</para>
    /// </summary>
    private async Task TerminateAsync()
    {
        ResetCancellation();
        try
        {
            await Task.Run(() => _executionHost?.TerminateAsync()).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _telemetryProvider.ReportError(e);
            throw;
        }
        finally
        {
            SetIsRunning(false);
        }
    }

    /// <summary>
    /// 线程安全设置 IsRunning（通过 dispatcher 切回 UI 线程）。
    /// </summary>
    private void SetIsRunning(bool value) => _dispatcher.InvokeAsync(() => IsRunning = value);

    // ===================== 自动保存 =====================

    /// <summary>
    /// 自动保存：
    /// <para>仅当 IsDirty=true 时执行；若 Document 尚未存在，则生成一个 autosave 文件路径。</para>
    /// </summary>
    public async Task AutoSaveAsync()
    {
        if (!IsDirty) return;

        var document = Document;

        if (document == null)
        {
            var index = 1;
            string path;

            do
            {
                path = Path.Combine(
                    WorkingDirectory,
                    DocumentViewModel.GetAutoSaveName("Program" + index++ + GetFileExtension()));
            }
            while (File.Exists(path));

            document = DocumentViewModel.FromPath(path);
        }

        Document = document;

        await SaveDocumentAsync(Document.GetAutoSavePath()).ConfigureAwait(false);
    }

    /// <summary>
    /// 打开当前文档的 BuildPath（资源管理器/文件管理器）。
    /// </summary>
    public void OpenBuildPath() => _ = Task.Run(() =>
    {
        try
        {
            Process.Start(new ProcessStartInfo(new Uri("file://" + BuildPath).ToString()) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _telemetryProvider.ReportError(ex);
        }
    });

    // ===================== 保存（底层写盘） =====================

    /// <summary>
    /// 将 Roslyn 文档内容保存到指定路径（按行写入，保持文本行结构）。
    /// <para>仅当已初始化（DocumentId 等就绪）时生效。</para>
    /// </summary>
    private async Task SaveDocumentAsync(string path)
    {
        if (!_isInitialized) return;

        var document = MainViewModel.RoslynHost.GetDocument(DocumentId);
        if (document == null)
        {
            return;
        }

        var text = await document.GetTextAsync().ConfigureAwait(false);

        using var writer = File.CreateText(path);
        for (var lineIndex = 0; lineIndex < text.Lines.Count - 1; ++lineIndex)
        {
            var lineText = text.Lines[lineIndex].ToString();
            await writer.WriteLineAsync(lineText).ConfigureAwait(false);
        }

        await writer.WriteAsync(text.Lines[text.Lines.Count - 1].ToString()).ConfigureAwait(false);
    }

    /// <summary>
    /// 初始化打开文档（由外部在 View/Editor 绑定后调用）。
    /// <para>设置 DocumentId、错误回调、选择范围获取器、视图释放句柄等；并完成 ExecutionHost 初始化。</para>
    /// </summary>
    internal void Initialize(
        DocumentId documentId,
        Action<ExceptionResultObject?> onError,
        Func<TextSpan> getSelection,
        IDisposable viewDisposable)
    {
        _viewDisposable = viewDisposable;
        _onError = onError;
        _getSelection = getSelection;
        DocumentId = documentId;

        // 优先使用设置中的默认平台，否则取第一个可用平台
        Platform = AvailablePlatforms.FirstOrDefault(p => p.ToString() == MainViewModel.Settings.DefaultPlatformName) ??
                   AvailablePlatforms.FirstOrDefault();

        InitializeExecutionHost();

        _isInitialized = true;

        UpdatePackages();

        TerminateCommand?.Execute();
    }

    /// <summary>
    /// 当前 Roslyn 文档 ID（初始化后可用）。
    /// </summary>
    public DocumentId DocumentId
    {
        get => _documentId ?? throw new ArgumentNullException(nameof(_documentId));
        private set => _documentId = value;
    }

    /// <summary>
    /// 是否已拥有 DocumentId（即已初始化）。
    /// </summary>
    public bool HasDocumentId => _documentId is not null;

    /// <summary>所属主 ViewModel。</summary>
    public MainViewModel MainViewModel { get; }

    /// <summary>命令提供器（用于创建同步/异步命令）。</summary>
    public ICommandProvider CommandProvider { get; }

    /// <summary>NuGet 相关 ViewModel。</summary>
    public NuGetDocumentViewModel NuGet { get; }

    /// <summary>
    /// 页面标题：若有关联真实文档且不是“仅 autosave”，则显示文件名；否则显示默认名 + 扩展名。
    /// </summary>
    public string Title => Document != null && !Document.IsAutoSaveOnly
        ? Document.Name
        : DefaultDocumentName + GetFileExtension();

    public IDelegateCommand OpenBuildPathCommand { get; }
    public IDelegateCommand SaveCommand { get; }
    public IDelegateCommand RunCommand { get; }
    public IDelegateCommand TerminateCommand { get; }
    public IDelegateCommand FormatDocumentCommand { get; }
    public IDelegateCommand CommentSelectionCommand { get; }
    public IDelegateCommand UncommentSelectionCommand { get; }
    public IDelegateCommand RenameSymbolCommand { get; }

    /// <summary>
    /// 是否正在运行（前台/当前页签执行）。
    /// <para>变化时会刷新 RunCommand 的可执行状态。</para>
    /// </summary>
    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (SetProperty(ref _isRunning, value))
            {
                _dispatcher.InvokeAsync(RunCommand.RaiseCanExecuteChanged);
            }
        }
    }

    // ===================== 运行（前台） =====================

    /// <summary>
    /// 执行当前文档（前台运行）。
    /// <para>会自动保存所有打开文档；按 IsDirty 决定运行 autosave 路径还是原路径。</para>
    /// </summary>
    private async Task RunAsync()
    {
        if (IsRunning || _executionHost is null || _executionHostParameters is null)
        {
            return;
        }

        ReportedProgress = null;

        var cancellationToken = ResetCancellation();

        await MainViewModel.AutoSaveOpenDocuments().ConfigureAwait(true);

        var documentPath = IsDirty ? Document?.GetAutoSavePath() : Document?.Path;
        if (documentPath is null)
        {
            return;
        }

        SetIsRunning(true);

        StartExec();

        if (!ShowIL)
        {
            ILText = DefaultILText;
        }

        try
        {
            if (_executionHost is not null && _executionHostParameters is not null)
            {
                // 确保执行工作目录与当前脚本路径一致（路径可能在加载后发生变化）
                if (_executionHostParameters.WorkingDirectory != WorkingDirectory)
                    _executionHostParameters.WorkingDirectory = WorkingDirectory;

                await _executionHost.ExecuteAsync(documentPath, ShowIL, OptimizationLevel, cancellationToken).ConfigureAwait(true);
            }
        }
        catch (CompilationErrorException ex)
        {
            // Roslyn 脚本编译异常：逐条输出诊断信息
            foreach (var diagnostic in ex.Diagnostics)
            {
                var startLinePosition = diagnostic.Location.GetLineSpan().StartLinePosition;
                MainViewModel.OutputResult(
                    $"[{diagnostic.Severity}]",
                    $"{diagnostic.GetMessage(CultureInfo.InvariantCulture)} (Code={diagnostic.Id}, Col={startLinePosition.Character})",
                    "编译错误",
                    startLinePosition.Line
                );
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            MainViewModel.OutputResult(
                "[Exception]",
                ex.ToString(),
                "异常",
                null
            );
        }
        finally
        {
            SetIsRunning(false);
            ReportedProgress = null;
        }
    }

    /// <summary>
    /// 开始执行前的准备动作：
    /// <para>清理上一次错误显示等（按原代码仅调用 _onError(null)）。</para>
    /// </summary>
    private void StartExec()
    {
        //ClearResults();
        _onError?.Invoke(null);
    }

    /// <summary>
    /// 清空结果集合并回填 restore 结果（按原逻辑保留）。
    /// </summary>
    private void ClearResults()
    {
        lock (_results)
        {
            _results.Clear();
            _results.AddRange(_restoreResults);
        }
        OnPropertyChanged(nameof(ResultsText));
    }

    /// <summary>
    /// 根据设置决定编译优化级别：OptimizeCompilation=true => Release，否则 Debug。
    /// </summary>
    private OptimizationLevel OptimizationLevel =>
        MainViewModel.Settings.OptimizeCompilation ? OptimizationLevel.Release : OptimizationLevel.Debug;

    /// <summary>
    /// 更新引用/包（通常触发 restore）。
    /// </summary>
    private void UpdatePackages(bool alwaysRestore = true) =>
        _ = _executionHost?.UpdateReferencesAsync(alwaysRestore);

    /// <summary>
    /// 获取用于执行的代码：
    /// <para>若存在 SelectedText，则返回选中文本；否则返回整个文档文本。</para>
    /// </summary>
    private async Task<string> GetCodeAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(SelectedText))
        {
            return SelectedText;
        }

        var document = MainViewModel.RoslynHost.GetDocument(DocumentId);
        if (document == null)
        {
            return string.Empty;
        }

        return (await document.GetTextAsync(cancellationToken)
            .ConfigureAwait(false)).ToString();
    }

    /// <summary>
    /// 重置运行取消令牌：取消并释放旧 CTS，创建新的 CTS 并返回其 Token。
    /// </summary>
    private CancellationToken ResetCancellation()
    {
        if (_runCts != null)
        {
            _runCts.Cancel();
            _runCts.Dispose();
        }

        var runCts = new CancellationTokenSource();
        _runCts = runCts;
        return runCts.Token;
    }

    /// <summary>
    /// 加载文档文本：
    /// <list type="number">
    /// <item>若存在未保存内存快照且 IsDirty=true，则优先返回快照。</item>
    /// <item>否则从磁盘读取 Document.Path。</item>
    /// </list>
    /// </summary>
    public async Task<string> LoadTextAsync()
    {
        // ① 优先使用未保存快照（仅当确实有未保存修改时）
        if (_unsavedTextSnapshot is not null && IsDirty)
            return _unsavedTextSnapshot;

        // ② 否则从磁盘读
        if (Document == null || string.IsNullOrEmpty(Document.Path) || !File.Exists(Document.Path))
            return string.Empty;

        using var fileStream = File.OpenText(Document.Path);
        return await fileStream.ReadToEndAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// 关闭页面（释放与视图绑定的资源）。
    /// </summary>
    public void Close()
    {
        _viewDisposable?.Dispose();
    }

    /// <summary>
    /// 当前运行进度（由 ExecutionHost 上报）。
    /// </summary>
    public double? ReportedProgress
    {
        get => _reportedProgress;
        private set
        {
            if (_reportedProgress != value)
            {
                SetProperty(ref _reportedProgress, value);
                OnPropertyChanged(nameof(HasReportedProgress));
            }
        }
    }

    /// <summary>是否存在进度值（用于 UI 显示进度条等）。</summary>
    public bool HasReportedProgress => ReportedProgress.HasValue;

    /// <summary>是否显示 IL（由 UI 控制）。</summary>
    public bool ShowIL { get; set; }

    /// <summary>请求编辑器获得焦点的事件（例如重命名后回到编辑器）。</summary>
    public event EventHandler? EditorFocus;

    /// <summary>触发 EditorFocus 事件。</summary>
    private void OnEditorFocus()
    {
        EditorFocus?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 文本变更回调（由视图在编辑器文本变化时调用）：
    /// <para>标记脏；若 LiveMode 开启则延迟触发 Run；并更新引用（不强制 restore）。</para>
    /// </summary>
    public void OnTextChanged()
    {
        IsDirty = true;

        if (IsLiveMode)
        {
            _liveModeTimer?.Change(MainViewModel.Settings.LiveModeDelayMs, Timeout.Infinite);
        }

        UpdatePackages(alwaysRestore: false);
    }

    /// <summary>
    /// 释放资源。
    /// </summary>
    public void Dispose()
    {
        //MainViewModel.EditorFontFamilyChanged -= OnEditorFontFamilyChanged;
        _runCts?.Dispose();
    }

    /// <summary>请求编辑器跳转到指定位置（line/column）。</summary>
    public event Action<(int line, int column)>? EditorChangeLocation;

    /// <summary>
    /// 若结果带行号，则请求编辑器跳转到对应行列。
    /// </summary>
    public void TryJumpToLine(IResultWithLineNumber result)
    {
        if (result.LineNumber is { } lineNumber)
        {
            EditorChangeLocation?.Invoke((lineNumber, result.Column));
        }
    }

    // ===================== 保存/另存为 =====================

    /// <summary>
    /// 另存为：
    /// <para>弹出保存文件对话框，必要时补扩展名；写盘后绑定/更新 Document。</para>
    /// </summary>
    public async Task<SaveResult> SaveAsAsync()
    {
        if (_isSaving) return SaveResult.Cancel;

        _isSaving = true;
        try
        {
            var dialog = _serviceProvider.GetRequiredService<ISaveFileDialog>();

            // 文件类型过滤器（保持原风格）
            dialog.Filter = new FileDialogFilter("C# Files", "cs", "csx");

            // 首选扩展（脚本 .csx，普通 .cs）
            var preferredExt = SourceCodeKind == SourceCodeKind.Script ? ".csx" : ".cs";

            // 初始文件名与目录
            dialog.InitialFileName = Document?.Name ?? "New" + preferredExt;
            dialog.InitialDirectory = Path.GetDirectoryName(Document?.Path) ?? MainViewModel.DocumentRoot.Path;

            var chosenPath = await dialog.ShowAsync().ConfigureAwait(true);
            if (string.IsNullOrWhiteSpace(chosenPath))
                return SaveResult.Cancel;

            // 兜底补扩展名
            var finalPath = EnsureExtension(chosenPath, preferredExt);

            // 实际写盘
            await SaveDocumentAsync(finalPath).ConfigureAwait(true);

            // 首次保存：挂回真实 Document；否则仅更新路径
            if (Document == null || Document.IsAutoSaveOnly)
            {
                Document = DocumentViewModel.FromPath(finalPath);
            }
            else
            {
                Document.ChangePath(finalPath);
            }

            IsDirty = false;
            _unsavedTextSnapshot = null;
            Document?.DeleteAutoSave();

            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(TabTitle));
            OnPropertyChanged(nameof(Document));

            return SaveResult.Save;
        }
        finally
        {
            _isSaving = false;
        }
    }

    /// <summary>
    /// 若用户未输入扩展名或只输入了结尾的“.”，则追加 preferredExt（如 .cs / .csx）。
    /// <para>用户显式输入了任何扩展名（.cs/.csx/.txt 等）则原样使用。</para>
    /// </summary>
    private static string EnsureExtension(string path, string preferredExt)
    {
        if (string.IsNullOrEmpty(path)) return path;

        // 去掉可能的结尾点
        path = path.TrimEnd('.');

        var ext = Path.GetExtension(path);
        if (string.IsNullOrEmpty(ext))
            return path + preferredExt;

        return path;
    }

    /// <summary>
    /// 保存入口：
    /// <para>若已有真实路径（Document 且非 AutoSaveOnly）则直接保存；否则走 SaveAs。</para>
    /// </summary>
    public async Task<SaveResult> SaveAsync(bool promptSave)
    {
        if (_isSaving) return SaveResult.Cancel;

        if (Document != null && !Document.IsAutoSaveOnly)
        {
            _isSaving = true;
            try
            {
                return await SaveToRealPathAsync().ConfigureAwait(true);
            }
            finally
            {
                _isSaving = false;
            }
        }

        // 没有真实路径时，走“另存为”
        return await SaveAsAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// 保存到 Document.GetSavePath()（真实路径）。
    /// <para>保存成功后清理脏标记、清空未保存快照并删除 autosave 文件。</para>
    /// </summary>
    async Task<SaveResult> SaveToRealPathAsync()
    {
        if (Document == null)
        {
            // 文档对象不存在，无法保存
            return SaveResult.Cancel;
        }

        await SaveDocumentAsync(Document.GetSavePath()).ConfigureAwait(true);
        IsDirty = false;
        _unsavedTextSnapshot = null; // 保存成功后清空未保存快照
        Document.DeleteAutoSave();
        return SaveResult.Save;
    }

    /// <summary>
    /// 查找所有匹配项并返回其 <see cref="TextSpan"/> 列表。
    /// <para>支持：大小写、全词匹配、正则。</para>
    /// <para>注意：此方法内部使用同步 Result 取文本（保持原逻辑）。</para>
    /// </summary>
    public IReadOnlyList<TextSpan> FindAll(string searchText, bool matchCase = false, bool wholeWord = false, bool useRegex = false)
    {
        var results = new List<TextSpan>();
        var document = MainViewModel.RoslynHost.GetDocument(DocumentId);
        if (document == null || string.IsNullOrEmpty(searchText))
            return results;

        var text = document.GetTextAsync().Result.ToString();
        var options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
        var pattern = useRegex ? searchText : Regex.Escape(searchText);
        if (wholeWord)
            pattern = $@"\b{pattern}\b";

        foreach (Match match in Regex.Matches(text, pattern, options))
        {
            results.Add(new TextSpan(match.Index, match.Length));
        }
        return results;
    }

    /// <summary>
    /// 替换全部匹配项（异步）。
    /// <para>若替换后文本有变化，则更新 Roslyn 文档并触发 <see cref="OnTextChanged"/>。</para>
    /// </summary>
    public async Task<int> ReplaceAllAsync(string searchText, string replaceText, bool matchCase = false, bool wholeWord = false, bool useRegex = false)
    {
        var document = MainViewModel.RoslynHost.GetDocument(DocumentId);
        if (document == null || string.IsNullOrEmpty(searchText))
            return 0;

        var text = await document.GetTextAsync().ConfigureAwait(false);
        var original = text.ToString();

        var options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
        var pattern = useRegex ? searchText : Regex.Escape(searchText);
        if (wholeWord)
            pattern = $@"\b{pattern}\b";

        var replaced = Regex.Replace(original, pattern, replaceText, options);

        if (original != replaced)
        {
            var newText = SourceText.From(replaced);
            MainViewModel.RoslynHost.UpdateDocument(document.WithText(newText));
            OnTextChanged();
            return Regex.Matches(original, pattern, options).Count;
        }
        return 0;
    }

    // ===================== 搜索面板 UI 状态 =====================

    private bool _isSearchPanelVisible;

    /// <summary>
    /// 搜索面板是否可见（用于 UI 绑定）。
    /// </summary>
    public bool IsSearchPanelVisible
    {
        get => _isSearchPanelVisible;
        set
        {
            if (_isSearchPanelVisible != value)
            {
                _isSearchPanelVisible = value;
                OnPropertyChanged(nameof(IsSearchPanelVisible));
            }
        }
    }

    private string? _searchText;

    /// <summary>
    /// 搜索关键字（UI 输入）。
    /// </summary>
    public string? SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string? _replaceText;

    /// <summary>
    /// 替换文本（UI 输入）。
    /// </summary>
    public string? ReplaceText
    {
        get => _replaceText;
        set => SetProperty(ref _replaceText, value);
    }

    private string? _searchResultMessage;

    /// <summary>
    /// 搜索结果提示信息（用于 UI 显示，例如 “找到 N 处匹配项”）。
    /// </summary>
    public string? SearchResultMessage
    {
        get => _searchResultMessage;
        set => SetProperty(ref _searchResultMessage, value);
    }

    /// <summary>关闭/隐藏搜索面板命令。</summary>
    public IDelegateCommand ToggleSearchPanelCommand { get; }

    /// <summary>执行查找命令。</summary>
    public IDelegateCommand FindTextCommand { get; }

    /// <summary>执行替换命令（ReplaceAll）。</summary>
    public IDelegateCommand ReplaceTextCommand { get; }

    /// <summary>
    /// 执行替换（异步 void，供命令直接调用，保持原行为）。
    /// </summary>
    private async void ExecuteReplaceText()
    {
        if (!string.IsNullOrEmpty(SearchText))
        {
            await ReplaceAllAsync(SearchText ?? string.Empty, ReplaceText ?? string.Empty).ConfigureAwait(true);
        }
    }

    private IReadOnlyList<TextSpan> _searchResults = Array.Empty<TextSpan>();

    /// <summary>
    /// 当前搜索匹配的全部范围（用于高亮/跳转）。
    /// </summary>
    public IReadOnlyList<TextSpan> SearchResults
    {
        get => _searchResults;
        private set => SetProperty(ref _searchResults, value);
    }

    /// <summary>
    /// 请求视图高亮匹配项：
    /// <para>参数1：所有匹配范围；参数2：当前高亮索引（-1 表示无）。</para>
    /// </summary>
    public event Action<IReadOnlyList<TextSpan>, int>? SearchHighlightRequested;

    /// <summary>
    /// 请求视图跳转到某个匹配范围（通常用于定位光标并滚动到可视区域）。
    /// </summary>
    public event Action<TextSpan>? SearchJumpRequested;

    /// <summary>
    /// 执行查找：更新 SearchResults / SearchResultMessage，并触发高亮与首次跳转。
    /// </summary>
    private void FindText()
    {
        var results = FindAll(SearchText ?? string.Empty);
        SearchResults = results;
        SearchResultMessage = results.Count > 0
            ? $"已找到 {results.Count} 处匹配项"
            : "未找到匹配项";

        _currentHighlightIndex = results.Count > 0 ? 0 : -1;
        SearchHighlightRequested?.Invoke(results, _currentHighlightIndex);

        // 自动跳转到第一个匹配项
        if (results.Count > 0)
        {
            SearchJumpRequested?.Invoke(results[0]);
        }
    }

    /// <summary>
    /// 请求视图清除搜索高亮（隐藏面板/清空时调用）。
    /// </summary>
    public event Action? SearchClearHighlightRequested;

    /// <summary>跳转到下一个高亮项命令。</summary>
    public IDelegateCommand JumpToNextHighlightCommand { get; }

    /// <summary>跳转到上一个高亮项命令。</summary>
    public IDelegateCommand JumpToPrevHighlightCommand { get; }

    private int _currentHighlightIndex = -1;

    /// <summary>
    /// 跳到下一个匹配项（循环）。
    /// </summary>
    private void JumpToNextHighlight()
    {
        if (SearchResults.Count == 0) return;
        _currentHighlightIndex = (_currentHighlightIndex + 1) % SearchResults.Count;
        SearchHighlightRequested?.Invoke(SearchResults, _currentHighlightIndex);
        SearchJumpRequested?.Invoke(SearchResults[_currentHighlightIndex]);
    }

    /// <summary>
    /// 跳到上一个匹配项（循环）。
    /// </summary>
    private void JumpToPrevHighlight()
    {
        if (SearchResults.Count == 0) return;
        _currentHighlightIndex = (_currentHighlightIndex - 1 + SearchResults.Count) % SearchResults.Count;
        SearchHighlightRequested?.Invoke(SearchResults, _currentHighlightIndex);
        SearchJumpRequested?.Invoke(SearchResults[_currentHighlightIndex]);
    }

    // ===================== Debug 运行 =====================

    /// <summary>Debug 命令（使用 Debug 优化级别）。</summary>
    public IDelegateCommand DebugCommand { get; }

    /// <summary>
    /// Debug 执行：整体结构与 RunAsync 类似，但强制 OptimizationLevel.Debug。
    /// </summary>
    private async Task DebugAsync()
    {
        if (IsRunning || _executionHost is null || _executionHostParameters is null)
        {
            return;
        }

        ReportedProgress = null;

        var cancellationToken = ResetCancellation();

        await MainViewModel.AutoSaveOpenDocuments().ConfigureAwait(true);

        var documentPath = IsDirty ? Document?.GetAutoSavePath() : Document?.Path;
        if (documentPath is null)
        {
            return;
        }

        SetIsRunning(true);

        StartExec();

        if (!ShowIL)
        {
            ILText = DefaultILText;
        }

        try
        {
            if (_executionHost is not null && _executionHostParameters is not null)
            {
                // 使用 Debug 优化级别
                await _executionHost.ExecuteAsync(documentPath, ShowIL, OptimizationLevel.Debug, cancellationToken).ConfigureAwait(true);
            }

            // 编译完成后输出提示
            MainViewModel.OutputResult("[Debug]", "DEBUG编译完成", "[Debug]", null);
            Console.WriteLine("Debug 方法已被调用");
        }
        catch (CompilationErrorException ex)
        {
            foreach (var diagnostic in ex.Diagnostics)
            {
                var startLinePosition = diagnostic.Location.GetLineSpan().StartLinePosition;
                MainViewModel.OutputResult(
                    $"[{diagnostic.Severity}]",
                    $"{diagnostic.GetMessage(CultureInfo.InvariantCulture)} (Code={diagnostic.Id}, Col={startLinePosition.Character})",
                    "编译错误",
                    startLinePosition.Line
                );
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            MainViewModel.OutputResult("[Exception]", ex.ToString(), "异常", null);
        }
        finally
        {
            SetIsRunning(false);
            ReportedProgress = null;
        }
    }

    // ===================== 复制输出/错误到剪贴板 =====================

    /// <summary>复制错误内容命令（复制“详情文本”）。</summary>
    public ICommand CopyErrorCommand { get; }

    /// <summary>复制错误行命令（复制“表格行文本”）。</summary>
    public ICommand CopyErrorLineCommand { get; }

    /// <summary>
    /// 请求将指定文本复制（给视图/外部处理，例如弹 Toast）。
    /// </summary>
    public event Action<string>? RequestCopyText;

    /// <summary>
    /// 复制错误内容：根据不同结果类型抽取合适字段并写入剪贴板。
    /// </summary>
    private void CopyError(object? error)
    {
        string? text = null;
        if (error is CompilationErrorResultObject ce)
            text = ce.Message;
        else if (error is ResultObject ro)
            text = ro.Value;
        else if (error is RestoreResultObject rr)
            text = rr.Value;

        Console.WriteLine($"[CopyError] 参数类型: {error?.GetType().Name}");
        Console.WriteLine($"[CopyError] 提取内容: {text}");

        if (!string.IsNullOrEmpty(text))
        {
            RequestCopyText?.Invoke(text);
            CopyToClipboard(text);
        }
        else
        {
            Console.WriteLine("[CopyError] 没有可复制的内容");
        }
    }

    /// <summary>
    /// 复制错误行：输出为 Tab 分隔的单行文本，便于粘贴到表格/日志中。
    /// </summary>
    private void CopyErrorLine(object? error)
    {
        string? line = null;
        if (error is CompilationErrorResultObject ce)
            line = $"{ce.Severity}\t{ce.ErrorCode}\t{ce.Message}\t{ce.LineNumber}\t{ce.Column}";
        else if (error is ResultObject ro)
            line = $"{ro.Header}\t{ro.Value}\t{ro.Type}";
        else if (error is RestoreResultObject rr)
            line = $"{rr.Severity}\t{rr.Message}";

        Console.WriteLine($"[CopyErrorLine] 参数类型: {error?.GetType().Name}");
        Console.WriteLine($"[CopyErrorLine] 提取内容: {line}");

        if (!string.IsNullOrEmpty(line))
        {
            RequestCopyText?.Invoke(line);
            CopyToClipboard(line);
        }
        else
        {
            Console.WriteLine("[CopyErrorLine] 没有可复制的内容");
        }
    }

    /// <summary>
    /// 当前选中的结果项（用于 UI 绑定/右键操作等）。
    /// </summary>
    public object? SelectedResultItem { get; set; }

    /// <summary>
    /// TreeView 选中项变化处理：记录 SelectedResultItem（按原代码保留）。
    /// </summary>
    private void OnTreeViewSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SelectedResultItem = (sender as TreeView)?.SelectedItem;
    }

    /// <summary>
    /// 将文本复制到系统剪贴板（异步 void，保持原行为）。
    /// </summary>
    private async void CopyToClipboard(string text)
    {
        var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.Windows.FirstOrDefault(w => w.IsActive)
            : null;

        if (window != null)
        {
            var clipboard = window.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(text).ConfigureAwait(true);
                Console.WriteLine("[CopyError] 已复制到剪贴板: " + text);

                var clipboardText = await clipboard.GetTextAsync().ConfigureAwait(true);
                Console.WriteLine("[CopyError] 剪贴板内容: " + clipboardText);
            }
            else
            {
                Console.WriteLine("[CopyError] 未能获取剪贴板对象（Clipboard 为 null）");
            }
        }
        else
        {
            Console.WriteLine("[CopyError] 未找到活动窗口，无法访问剪贴板");
        }
    }

    // ===================== 后台进程过滤（用于终止后台运行） =====================

    private string _selectedProcessFilter = "All";

    /// <summary>
    /// 当前选择的后台进程过滤器：
    /// <para>"All" 表示全部；或 "PID {n}" 表示单个进程。</para>
    /// </summary>
    public string SelectedProcessFilter
    {
        get => _selectedProcessFilter;
        set => SetProperty(ref _selectedProcessFilter, value);
    }

    /// <summary>
    /// 进程过滤器选项列表（包含 "All" 以及运行中 PID 项）。
    /// </summary>
    public ObservableCollection<string> ProcessFilterOptions { get; } = new() { "All" };

    // ===================== 后台运行 =====================

    /// <summary>
    /// 后台执行：把当前代码写入临时 Run.csx，并在独立 build 目录中运行。
    /// <para>会将后台执行的输出/错误/编译错误/restore 信息镜像到主输出窗口，并写入日志。</para>
    /// </summary>
    private async Task RunInBackgroundAsync()
    {
        if (!_isInitialized || Platform is null)
            return;

        var code = await GetCodeAsync(CancellationToken.None).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(code)) return;

        // 独立的后台构建目录
        var bgBuildPath = Path.Combine(BuildPath, "bg-" + Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(bgBuildPath);

        var host = MainViewModel.RoslynHost;
        var parameters = new ExecutionHostParameters(
            bgBuildPath,
            _serviceProvider.GetRequiredService<NuGetViewModel>().ConfigPath,
            host.DefaultImports,
            host.DisabledDiagnostics,
            WorkingDirectory,
            SourceCodeKind.Script // 当前逻辑按脚本执行（保持原行为）
        );

        var bgHost = new ExecutionHost(parameters, host, _logger)
        {
            Name = (Document?.Name ?? "Untitled") + " (background)",
            DocumentId = DocumentId,
            Platform = Platform,
            DotNetExecutable = _platformsFactory.DotNetExecutable
        };

        // 将当前代码写入 Run.csx
        var runPath = Path.Combine(bgBuildPath, "Run.csx");
        File.WriteAllText(runPath, code, Encoding.UTF8);

        // 日志文件（记录后台运行信息）
        var timeTag = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var logPath = Path.Combine(bgBuildPath, $"background-{timeTag}.log");
        using (File.CreateText(logPath)) { } // 创建/清空

        void AppendLine(string line)
        {
            try { File.AppendAllText(logPath, line + Environment.NewLine); } catch { /* 忽略写日志异常 */ }
        }

        // ============= 将执行期事件“镜像”到结果栏（同时继续写日志） =============

        // 标准输出/Dump
        bgHost.Dumped += r =>
        {
            var text = r?.Value?.ToString() ?? string.Empty;
            AppendLine(text);
            if (!string.IsNullOrWhiteSpace(text))
            {
                _dispatcher.InvokeAsync(() =>
                {
                    MainViewModel.OutputResult("[Run]", text, "输出", null);
                }, AppDispatcherPriority.Low);
            }
        };

        // 异常输出
        bgHost.Error += err =>
        {
            var text = err?.Value ?? string.Empty;
            AppendLine(text);
            if (!string.IsNullOrWhiteSpace(text))
            {
                _dispatcher.InvokeAsync(() =>
                {
                    MainViewModel.OutputResult("[Exception]", text, "异常", null);
                }, AppDispatcherPriority.Low);
            }
        };

        // 编译错误输出
        bgHost.CompilationErrors += errs =>
        {
            foreach (var e in errs)
            {
                var sev = e?.Severity?.ToString() ?? "Error";
                var msg = e?.Message ?? string.Empty;

                string? codeStr = null;
                try
                {
                    codeStr = e?.GetType().GetProperty("ErrorCode")?.GetValue(e) as string;
                }
                catch
                {
                    // ignore
                }
                var code = string.IsNullOrWhiteSpace(codeStr) ? "CS0000" : codeStr!;

                var line = 0;
                var col = 0;
                try { line = e?.LineNumber ?? 0; } catch { }
                try { col = e?.Column ?? 0; } catch { }

                AppendLine($"[{sev}] {msg} (L{line},C{col})");

                _dispatcher.InvokeAsync(() =>
                {
                    MainViewModel.OutputResult($"[{sev}]", $"{msg} (Code={code}, Col={col})", "编译错误", line);
                }, AppDispatcherPriority.Low);
            }
        };

        // 反汇编输出
        bgHost.Disassembled += il =>
        {
            AppendLine("===== IL =====");
            AppendLine(il ?? string.Empty);
            AppendLine("===== END IL =====");

            if (!string.IsNullOrEmpty(il))
            {
                _dispatcher.InvokeAsync(() =>
                {
                    MainViewModel.OutputResult("[IL]", il, "信息", null);
                }, AppDispatcherPriority.Low);
            }
        };

        // restore 状态输出
        bgHost.RestoreStarted += () =>
        {
            AppendLine("[restore] started");
            _dispatcher.InvokeAsync(() =>
            {
                MainViewModel.OutputResult("[restore]", "started", "信息", null);
            }, AppDispatcherPriority.Low);
        };

        bgHost.RestoreCompleted += rr =>
        {
            AppendLine($"[restore] {(rr.Success ? "success" : "failed")}");
            _dispatcher.InvokeAsync(() =>
            {
                if (rr.Success)
                {
                    MainViewModel.OutputResult("[restore]", "success", "信息", null);
                }
                else
                {
                    foreach (var er in rr.Errors)
                    {
                        AppendLine("[restore-error] " + er);
                        AddRestoreResult(new RestoreResultObject(er, "Error"));
                    }
                }
            }, AppDispatcherPriority.Low);
        };

        // ========================= 镜像结束 =========================

        // 注册后台运行项（先把 Host/Job 填好，Stop 才能立即生效）
        var info = BackgroundRunManager.Instance.Register(Document?.Name ?? "Untitled", runPath, logPath);
        info.Host = bgHost;
        info.Job = ProcessJob.Create();

        AppendLine($"[info] runPath={runPath}");
        AppendLine($"[info] TEMP={Environment.GetEnvironmentVariable("TEMP")}");
        AppendLine($"[info] Path.GetTempPath()={Path.GetTempPath()}");

        // —— 订阅一次：拿到 PID 就入 Job + 更新下拉框；执行完毕后会退订
        void OnProcessStarted(int pid)
        {
            info.ProcessId = pid;
            try { info.Job?.TryAddProcess(pid); } catch { }
            AppendLine($"[job] attached pid={pid}");

            var tag = $"PID {pid}";
            _dispatcher.InvokeAsync(() =>
            {
                if (!ProcessFilterOptions.Contains("All"))
                    ProcessFilterOptions.Insert(0, "All");
                if (!ProcessFilterOptions.Contains(tag))
                    ProcessFilterOptions.Add(tag);
                if (string.IsNullOrEmpty(SelectedProcessFilter))
                    SelectedProcessFilter = "All";

                // 可见化提示新进程
                MainViewModel.OutputResult("[Run]", $"Process started: {tag}", "信息", null);
            }, AppDispatcherPriority.Normal);
        }
        try { bgHost.ProcessStarted += OnProcessStarted; } catch { /* 兼容不同分支 */ }

        // 还原引用后执行
        await bgHost.UpdateReferencesAsync(true).ConfigureAwait(false);

        info.Task = Task.Run(async () =>
        {
            try
            {
                AppendLine("[start] " + DateTime.Now.ToString("O", CultureInfo.InvariantCulture));
                _dispatcher.InvokeAsync(() =>
                {
                    MainViewModel.OutputResult("[Run]", "started", "信息", null);
                }, AppDispatcherPriority.Low);

                await bgHost.ExecuteAsync(runPath, /*showIL*/ false, OptimizationLevel, info.Cancellation.Token).ConfigureAwait(false);

                AppendLine("[done] " + DateTime.Now.ToString("O", CultureInfo.InvariantCulture));
                _dispatcher.InvokeAsync(() =>
                {
                    MainViewModel.OutputResult("[Run]", "done", "信息", null);
                }, AppDispatcherPriority.Low);

                BackgroundRunManager.Instance.MarkCompleted(info);
            }
            catch (OperationCanceledException)
            {
                AppendLine("[canceled]");
                _dispatcher.InvokeAsync(() =>
                {
                    MainViewModel.OutputResult("[Run]", "canceled", "信息", null);
                }, AppDispatcherPriority.Low);
                BackgroundRunManager.Instance.MarkCompleted(info);
            }
            catch (Exception ex)
            {
                AppendLine("[exception] " + ex);
                _dispatcher.InvokeAsync(() =>
                {
                    AddResult(new ExceptionResultObject { Value = ex.ToString() });
                }, AppDispatcherPriority.Low);
                BackgroundRunManager.Instance.MarkCompleted(info, ex);
            }
            finally
            {
                // 退订（若事件存在）
                try { bgHost.ProcessStarted -= OnProcessStarted; } catch { }

                // 执行结束：如果该 PID 在下拉中，移除并回退到 All
                if (info.ProcessId is int pid)
                {
                    var tag = $"PID {pid}";
                    _dispatcher.InvokeAsync(() =>
                    {
                        if (!ProcessFilterOptions.Contains("All"))
                            ProcessFilterOptions.Insert(0, "All");

                        if (SelectedProcessFilter == tag)
                            SelectedProcessFilter = "All";

                        ProcessFilterOptions.Remove(tag);
                    }, AppDispatcherPriority.Normal);
                }
            }
        });
    }

    // ===================== 终止（前台 + 后台） =====================

    /// <summary>
    /// 终止当前选中的执行：
    /// <list type="bullet">
    /// <item>前台：若存在 _executionHost，则终止。</item>
    /// <item>后台：根据 SelectedProcessFilter 终止所有或单个 PID 的后台任务。</item>
    /// </list>
    /// </summary>
    private async Task TerminateSelectedAsync()
    {
        ResetCancellation();

        try
        {
            // 停止“前台”执行（若有）
            if (_executionHost is not null)
                await _executionHost.TerminateAsync().ConfigureAwait(false);

            // 停止“后台”子进程（All 或 单 PID）
            if (Document != null)
            {
                if (string.Equals(SelectedProcessFilter, "All", StringComparison.OrdinalIgnoreCase))
                {
                    await BackgroundRunManager.Instance.CancelAllForDocumentAsync(Document.Name, _logger).ConfigureAwait(false);

                    // 全停：只保留 All 并选中它
                    _dispatcher.InvokeAsync(() =>
                    {
                        for (var i = ProcessFilterOptions.Count - 1; i >= 0; i--)
                        {
                            if (!string.Equals(ProcessFilterOptions[i], "All", StringComparison.OrdinalIgnoreCase))
                                ProcessFilterOptions.RemoveAt(i);
                        }
                        if (!ProcessFilterOptions.Contains("All"))
                            ProcessFilterOptions.Insert(0, "All");
                        SelectedProcessFilter = "All";
                    }, AppDispatcherPriority.Normal);
                }
                else if (int.TryParse(SelectedProcessFilter.Replace("PID", "", StringComparison.OrdinalIgnoreCase).Trim(), out var pid))
                {
                    await BackgroundRunManager.Instance.CancelByPidAsync(pid, _logger).ConfigureAwait(false);

                    // 单 PID 停止后，回到 All（不清空其他 PID 选项）
                    _dispatcher.InvokeAsync(() =>
                    {
                        if (!ProcessFilterOptions.Contains("All"))
                            ProcessFilterOptions.Insert(0, "All");
                        SelectedProcessFilter = "All";
                    }, AppDispatcherPriority.Normal);
                }
            }
        }
        catch (Exception e)
        {
            _telemetryProvider.ReportError(e);
            throw;
        }
        finally
        {
            SetIsRunning(false);
        }
    }

    // ===================== Results 文本聚合（用于复制/显示） =====================

    /// <summary>
    /// 将当前 Results 集合格式化为文本（Tab 分隔），便于复制/保存。
    /// </summary>
    public string ResultsText
    {
        get
        {
            var sb = new StringBuilder();
            foreach (var item in Results)
            {
                if (item is CompilationErrorResultObject err)
                    sb.AppendLine(CultureInfo.InvariantCulture, $"{err.Severity}\t{err.ErrorCode}\t{err.Message}\t{err.LineNumber}\t{err.Column}");
                else if (item is RestoreResultObject restore)
                    sb.AppendLine(CultureInfo.InvariantCulture, $"{restore.Severity}\t{restore.Message}");
                else if (item is ResultObject res)
                    sb.AppendLine(CultureInfo.InvariantCulture, $"{res.Header}\t{res.Value}\t{res.Type}");
            }
            return sb.ToString();
        }
    }

    // ===================== 预览模式 / 未保存快照 / Tab 标题 =====================

    // 09-10：预览模式开关（用于 UI/交互，按原逻辑保留）
    private bool _isPreviewMode;

    /// <summary>
    /// 是否处于预览模式（具体含义由 UI/调用方决定）。
    /// </summary>
    public bool IsPreviewMode
    {
        get => _isPreviewMode;
        set
        {
            if (_isPreviewMode != value)
            {
                _isPreviewMode = value;
                OnPropertyChanged(nameof(IsPreviewMode));
            }
        }
    }

    /// <summary>
    /// 未保存文本的内存快照（用于标签切换/卸载后恢复）。
    /// </summary>
    private string? _unsavedTextSnapshot;

    /// <summary>
    /// 由视图在文本变化/卸载时调用，更新未保存文本的内存快照。
    /// </summary>
    public void UpdateUnsavedText(string? text)
    {
        _unsavedTextSnapshot = text;
    }

    /// <summary>
    /// 若已保存（IsDirty=false），则清空未保存快照。
    /// </summary>
    public void ClearUnsavedTextSnapshotIfSaved()
    {
        if (!IsDirty) _unsavedTextSnapshot = null;
    }

    /// <summary>
    /// 标签显示标题：未保存时在 Title 后追加 “ * ”。
    /// </summary>
    public string TabTitle => IsDirty ? $"{Title} *" : Title;

    /// <summary>
    /// 脏标记：表示是否存在未保存修改。
    /// <para>变化时会同步刷新 <see cref="TabTitle"/>。</para>
    /// </summary>
    public bool IsDirty
    {
        get => _isDirty;
        private set
        {
            if (SetProperty(ref _isDirty, value))
            {
                // 脏状态变化时，刷新 TabTitle（用于页签显示）
                OnPropertyChanged(nameof(TabTitle));
            }
        }
    }

    /// <summary>
    /// 本地化管理器（用于 XAML 绑定）。
    /// </summary>
    public LocalizationManager Localized => LocalizationManager.Instance;
}
