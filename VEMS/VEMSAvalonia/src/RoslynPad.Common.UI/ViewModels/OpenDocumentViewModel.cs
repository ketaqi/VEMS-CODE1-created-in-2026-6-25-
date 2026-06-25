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
using RoslynPad.UI.Dialogs;
using RoslynPad.UI.ViewModels;
using RoslynPad.Utilities;


// ReSharper disable RedundantUsingDirective
// ReSharper restore RedundantUsingDirective

namespace RoslynPad.UI;

[Export]
public class OpenDocumentViewModel : NotificationObject, IDisposable
{
    private const string DefaultDocumentName = "New";
    private const string RegularFileExtension = ".cs";
    private const string ScriptFileExtension = ".csx";
    private const string DefaultILText = "// Run to view IL";

    private readonly IServiceProvider _serviceProvider;
    private readonly IAppDispatcher _dispatcher;
    private readonly ITelemetryProvider _telemetryProvider;
    private readonly ILogger<OpenDocumentViewModel> _logger;
    private readonly IPlatformsFactory _platformsFactory;
    private readonly ObservableCollection<IResultObject> _results;
    private readonly List<RestoreResultObject> _restoreResults;

    private ExecutionHost? _executionHost;
    private ExecutionHostParameters? _executionHostParameters;
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
    /// </summary>
    public bool IsFloating { get; set; }

    /// <summary>
    /// 由视图提供的“把自己置前激活”的回调。MainViewModel 只需调用，不依赖 Avalonia 类型。
    /// </summary>
    public Action? ActivateView { get; set; }
    public string Id { get; }
    public string BuildPath { get; }

    public string WorkingDirectory => Document != null
        ? Path.GetDirectoryName(Document.Path)!
        : MainViewModel.DocumentRoot.Path;

    public string? SelectedText
    {
        get => _selectedText;
        set => SetProperty(ref _selectedText, value);
    }

    public IEnumerable<IResultObject> Results => _results;

    public IDelegateCommand ToggleLiveModeCommand { get; }
    public IDelegateCommand SetDefaultPlatformCommand { get; }

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

    private string GetFileExtension() =>
        SourceCodeKind == SourceCodeKind.Script ? ScriptFileExtension : RegularFileExtension;

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
            OnPropertyChanged(nameof(Title));          // 页签标题跟着刷
            OnPropertyChanged(nameof(TabTitle));       // ★ 同步刷新标签显示标题
            OnPropertyChanged(nameof(WorkingDirectory));
        }
    }

    public string ILText
    {
        get => _ilText ?? string.Empty;
        private set => SetProperty(ref _ilText, value);
    }

    [ImportingConstructor]
    public OpenDocumentViewModel(IServiceProvider serviceProvider, MainViewModel mainViewModel, ICommandProvider commands, IAppDispatcher appDispatcher, ITelemetryProvider telemetryProvider, ILogger<OpenDocumentViewModel> logger)
    {
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
        FontSize = MainViewModel.OutputFontSize; // 初始化

        MainViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.OutputFontSize))
            {
                FontSize = MainViewModel.OutputFontSize; // 实时同步
            }
        };
        CommandProvider = commands;

        NuGet = serviceProvider.GetRequiredService<NuGetDocumentViewModel>();

        _restoreSuccessful = true; // initially set to true so we can immediately start running and wait for restore
        _dispatcher = appDispatcher;

        OpenBuildPathCommand = commands.Create(OpenBuildPath);
        
        RunCommand = commands.CreateAsync(RunInBackgroundAsync, () => !IsRunning && RestoreSuccessful && Platform != null);
        TerminateCommand = commands.CreateAsync(TerminateSelectedAsync, () => Platform != null);
        FormatDocumentCommand = commands.CreateAsync(FormatDocumentAsync);
        CommentSelectionCommand = commands.CreateAsync(() => CommentUncommentSelectionAsync(CommentAction.Comment));
        UncommentSelectionCommand = commands.CreateAsync(() => CommentUncommentSelectionAsync(CommentAction.Uncomment));
        RenameSymbolCommand = commands.CreateAsync(RenameSymbolAsync);
        ToggleLiveModeCommand = commands.Create(() => IsLiveMode = !IsLiveMode);
        SetDefaultPlatformCommand = commands.Create(SetDefaultPlatform);

        ILText = DefaultILText;

        InitializePlatforms();

        // 构造函数中初始化
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
        DebugCommand = commands.CreateAsync(DebugAsync, () => !IsRunning && RestoreSuccessful && Platform != null);
        OpenBaiduCommand = commands.Create(OpenBaidu);

        OpenLicenseCommand = commands.Create(License);

        OpenhelpCommand = commands.Create(help);

        OpenContactCommand = commands.Create(Contact);

        OpenAboutVEMSCommand = commands.Create(AboutVEMS);

        CopyErrorCommand = new ViewModels.RelayCommand<object>(CopyError);
        CopyErrorLineCommand = new ViewModels.RelayCommand<object>(CopyErrorLine);
        SaveAsCommand = commands.CreateAsync(SaveAsAsync);


        ClearResultsCommand = commands.Create(ClearOutputResults);

        CopyAllResultsCommand = commands.Create(CopyAllOutputResults);
        SaveCommand = commands.CreateAsync(() => SaveAsync(promptSave: false));
        SaveAsCommand = commands.CreateAsync(SaveAsAsync);

        OnEditorFontFamilyChanged(mainViewModel.EditorFontFamily);
        OnEditorFontFamilyChanged1(mainViewModel.EditorFontFamily1);

    }
    //监听输出框字体
    public void OnEditorFontFamilyChanged1(string newFont1)
    {
        EditorFontFamily1 = newFont1;
        Console.WriteLine($"[OpenDocumentViewModel] OutputFontFamily已变更: {EditorFontFamily1}");
    }

    private string _editorFontFamily1 = "Consolas"; // 可设置默认字体

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
    public double FontSize
    {
        get => _fontSize;
        set
        {
            if (_fontSize != value)
            {
                _fontSize = value;
                OnPropertyChanged(nameof(FontSize));
                Console.WriteLine($"[OpenDocumentViewModel] OVFontSize已变更: {_fontSize}");
            }
        }
    }

    private string _editorFontFamily = "Consolas"; // 可设置默认字体

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

    public RoslynCodeEditor? EditorControl { get; set; }

    public ResultObject? ResultControl { get; set; }

    public void OnEditorFontFamilyChanged(string newFont)
    {
        //if (EditorFontFamily != newFont)
        //{
            EditorFontFamily = newFont;
            OnPropertyChanged(nameof(EditorFontFamily));
            Console.WriteLine($"[OpenDocumentViewModel] EditorFontFamily已变更: {EditorFontFamily}");
        //下方使编辑器字体实时刷新
        if (EditorControl != null)
        {
            EditorControl.FontFamily = new FontFamily(newFont);
            EditorControl.InvalidateVisual();
            Console.WriteLine($"[OpenDocumentViewModel] EditorControl字体已应用: {EditorControl.FontFamily}");
        }
        else
        {
            Console.WriteLine("[OpenDocumentViewModel] EditorControl为null，无法刷新字体");
        }
        //}
    }

    public IDelegateCommand CopyAllResultsCommand { get; }
    public IDelegateCommand ClearResultsCommand { get; }

    private void ClearOutputResults()
    {
        lock (_results)
        {
            _results.Clear();
        }
        ResultsAvailable?.Invoke();
        OnPropertyChanged(nameof(ResultsText)); // 这一句很关键
    }

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

    //public async Task<SaveResult> SaveAsync(bool promptSave)
    //{
    //    if (_isSaving) return SaveResult.Cancel;

    //    // VS风格：只要有真实路径，直接保存
    //    if (Document != null && !Document.IsAutoSaveOnly)
    //    {
    //        _isSaving = true;
    //        try
    //        {
    //            await SaveDocumentAsync(Document.GetSavePath()).ConfigureAwait(true);
    //            IsDirty = false;
    //            Document?.DeleteAutoSave();
    //            return SaveResult.Save;
    //        }
    //        finally
    //        {
    //            _isSaving = false;
    //        }
    //    }
    //    // 没有真实路径时，走“另存为”逻辑
    //    return await SaveAsAsync().ConfigureAwait(true);
    //}

    public IDelegateCommand SaveAsCommand { get; }

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

        _executionHost.Dumped += ExecutionHostOnDump;
        _executionHost.Error += ExecutionHostOnError;
        _executionHost.ReadInput += ExecutionHostOnInputRequest;
        _executionHost.CompilationErrors += ExecutionHostOnCompilationErrors;
        _executionHost.Disassembled += ExecutionHostOnDisassembled;
        _executionHost.RestoreStarted += OnRestoreStarted;
        _executionHost.RestoreCompleted += OnRestoreCompleted;
        _executionHost.ProgressChanged += p => ReportedProgress = p.Progress;
    }

    private void SetDefaultPlatform()
    {
        if (Platform is not null)
        {
            MainViewModel.Settings.DefaultPlatformName = Platform.ToString();
        }
    }

    private void InitializePlatforms()
    {
        AvailablePlatforms = _platformsFactory.GetExecutionPlatforms();
        var names = AvailablePlatforms.Select(p => p.ToString()).ToArray().AsReadOnly();
        AvailablePlatformNames = names;
        //_availablePlatformNames = names;
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
    private void OnRestoreStarted() => IsRestoring = true;

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

    public bool IsRestoring
    {
        get => _isRestoring;
        private set => SetProperty(ref _isRestoring, value);
    }

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

    private void OnDocumentUpdated() => DocumentUpdated?.Invoke(this, EventArgs.Empty);

    public event EventHandler? DocumentUpdated;

    public event Action? ReadInput;

    public event Action? ResultsAvailable;

    public void AddResult(IResultObject o)
    {
        lock (_results)
        {
            _results.Add(o);
        }

        ResultsAvailable?.Invoke();
        OnPropertyChanged(nameof(ResultsText));

    }

    private void AddRestoreResult(RestoreResultObject o)
    {
        lock (_results)
        {
            _restoreResults.Add(o);
            AddResult(o);
        }
        OnPropertyChanged(nameof(ResultsText));

    }

    private void ExecutionHostOnInputRequest() => _dispatcher.InvokeAsync(() =>
    {
        ReadInput?.Invoke();
    }, AppDispatcherPriority.Low);

    private void ExecutionHostOnDump(ResultObject result) => AddResult(result);

    private void ExecutionHostOnError(ExceptionResultObject errorResult) => _dispatcher.InvokeAsync(() =>
    {
        _onError?.Invoke(errorResult);
        if (errorResult != null)
        {
            AddResult(errorResult);
        }
    }, AppDispatcherPriority.Low);

    private void ExecutionHostOnCompilationErrors(IList<CompilationErrorResultObject> errors)
    {
        foreach (var error in errors)
        {
            AddResult(error);
        }
    }

    private void ExecutionHostOnDisassembled(string il) => ILText = il;

    public void SetDocument(DocumentViewModel? document)
    {
        Document = document;                           // 直接用同一个实例
        IsDirty = document?.IsAutoSave == true;
    }

    private void OnDocumentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DocumentViewModel.Name) ||
            e.PropertyName == nameof(DocumentViewModel.Path))
        {
            OnPropertyChanged(nameof(Title));          // Dock 标签标题刷新
            OnPropertyChanged(nameof(TabTitle));       // ★ 同步刷新标签显示标题
            OnPropertyChanged(nameof(WorkingDirectory));
        }
    }


    public void SendInput(string input) => _ = _executionHost?.SendInputAsync(input);

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
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, symbol, new SymbolRenameOptions(), dialog.SymbolName ?? string.Empty).ConfigureAwait(true);
            var newDocument = newSolution.GetDocument(DocumentId);
            // TODO: possibly update entire solution
            host.UpdateDocument(newDocument!);
        }
        OnEditorFocus();
    }

    private enum CommentAction
    {
        Comment,
        Uncomment
    }

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

    private async Task FormatDocumentAsync()
    {
        var document = MainViewModel.RoslynHost.GetDocument(DocumentId);
        var formattedDocument = await Formatter.FormatAsync(document!).ConfigureAwait(false);
        MainViewModel.RoslynHost.UpdateDocument(formattedDocument);
    }

    public IReadOnlyList<ExecutionPlatform> AvailablePlatforms      
    {
        get => _availablePlatforms ?? throw new ArgumentNullException(nameof(_availablePlatforms));
        private set => SetProperty(ref _availablePlatforms, value);
    }
    //public string[] AvailablePlatformNames =>
    //AvailablePlatforms
    //    .Select(p => p?.ToString() ?? string.Empty)
    //    .Where(s => !string.IsNullOrWhiteSpace(s))
    //    .ToArray();
    private List<PlatformID> _availablePlatformIds1= new();
    public List<PlatformID> AvailablePlatforms1
    {
        get => _availablePlatformIds1 ?? throw new ArgumentNullException(nameof(_availablePlatformIds1));
        private set => SetProperty(ref _availablePlatformIds1, value);
    }
    
    public ExecutionPlatform? Platform
    {
        get => _platform;
        set
        {
            if (value == null) return;
            //throw new InvalidOperationException();

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

    // 新增：用于字符串中间量（平台名称列表）
    private IReadOnlyList<string>? _availablePlatformNames;
    private string? _selectedPlatformName;

    // 新增：AvailablePlatforms 的字符串表示，供其他地方以 string 操作
    public IReadOnlyList<string> AvailablePlatformNames
    {
        get => _availablePlatformNames ?? throw new ArgumentNullException(nameof(_availablePlatformNames));

        private set => SetProperty(ref _availablePlatformNames, value ?? Array.Empty<string>());
    }

    // 新增：通过字符串设置/读取选中的平台（会把 string -> ExecutionPlatform 并赋值给 Platform）
    public string SelectedPlatformName
    {
        get => _selectedPlatformName ?? Platform?.ToString() ?? string.Empty;
        set
        {
            if (value is null) throw new InvalidOperationException();

            // 如果字符串变化，则尝试根据名称找到 ExecutionPlatform 并赋值给 Platform
            if (SetProperty(ref _selectedPlatformName, value))
            {
                // 首先确保 AvailablePlatforms 已经初始化
                if (AvailablePlatforms != null && AvailablePlatforms.Count > 0)
                {
                    var matched = AvailablePlatforms.FirstOrDefault(p => string.Equals(p.ToString(), value, StringComparison.Ordinal));
                    if (matched != null)
                    {
                        // 将找到的平台赋给 Platform（这样会走 Platform 的 setter，触发原有逻辑）
                        Platform = matched;
                    }
                    else
                    {
                        // 如果找不到匹配的字符串，可以选择：
                        // - 忽略（保持当前 Platform）
                        // - 或者尝试匹配部分字符串、tfm、name 等（按需扩展）
                        // 这里暂时保持忽略以避免无效赋值
                    }
                }
            }
        }
    }

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

    private void SetIsRunning(bool value) => _dispatcher.InvokeAsync(() => IsRunning = value);

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
                path = Path.Combine(WorkingDirectory, DocumentViewModel.GetAutoSaveName(("Program" + index++) + GetFileExtension()));
            }
            while (File.Exists(path));

            document = DocumentViewModel.FromPath(path);
        }

        Document = document;

        await SaveDocumentAsync(Document.GetAutoSavePath()).ConfigureAwait(false);
    }

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

    //public async Task<SaveResult> SaveAsync(bool promptSave)
    //{
    //    if (_isSaving) return SaveResult.Cancel;
    //    if (!IsDirty && promptSave) return SaveResult.Save;

    //    _isSaving = true;
    //    try
    //    {
    //        var result = SaveResult.Save;
    //        if (Document == null || Document.IsAutoSaveOnly)
    //        {
    //            var dialog = _serviceProvider.GetRequiredService<ISaveDocumentDialog>();
    //            dialog.ShowDoNotSave = promptSave;
    //            dialog.AllowNameEdit = true;
    //            dialog.FilePathFactory = name => DocumentViewModel.GetDocumentPathFromName(WorkingDirectory, name);
    //            await dialog.ShowAsync().ConfigureAwait(true);
    //            result = dialog.Result;
    //            if (result == SaveResult.Save && dialog.DocumentName != null)
    //            {
    //                Document?.DeleteAutoSave();
    //                Document = MainViewModel.AddDocument(dialog.DocumentName + GetFileExtension());
    //                OnPropertyChanged(nameof(Title));
    //            }
    //        }
    //        else if (promptSave)
    //        {
    //            var dialog = _serviceProvider.GetRequiredService<ISaveDocumentDialog>();
    //            dialog.ShowDoNotSave = true;
    //            dialog.DocumentName = Document.Name;
    //            await dialog.ShowAsync().ConfigureAwait(true);
    //            result = dialog.Result;
    //        }

    //        if (result == SaveResult.Save && Document != null)
    //        {
    //            await SaveDocumentAsync(Document.GetSavePath()).ConfigureAwait(true);
    //            IsDirty = false;
    //        }

    //        if (result != SaveResult.Cancel)
    //        {
    //            Document?.DeleteAutoSave();
    //        }

    //        return result;
    //    }
    //    finally
    //    {
    //        _isSaving = false;
    //    }
    //}

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
        for (int lineIndex = 0; lineIndex < text.Lines.Count - 1; ++lineIndex)
        {
            var lineText = text.Lines[lineIndex].ToString();
            await writer.WriteLineAsync(lineText).ConfigureAwait(false);
        }

        await writer.WriteAsync(text.Lines[text.Lines.Count - 1].ToString()).ConfigureAwait(false);
    }

    internal void Initialize(DocumentId documentId,
        Action<ExceptionResultObject?> onError,
        Func<TextSpan> getSelection, IDisposable viewDisposable)
    {
        _viewDisposable = viewDisposable;
        _onError = onError;
        _getSelection = getSelection;
        DocumentId = documentId;

        Platform = AvailablePlatforms.FirstOrDefault(p => p.ToString() == MainViewModel.Settings.DefaultPlatformName) ??
                   AvailablePlatforms.FirstOrDefault();

        InitializeExecutionHost();

        _isInitialized = true;

        UpdatePackages();

        TerminateCommand?.Execute();
    }

    public DocumentId DocumentId
    {
        get => _documentId ?? throw new ArgumentNullException(nameof(_documentId));
        private set => _documentId = value;
    }

    public bool HasDocumentId => _documentId is not null;

    public MainViewModel MainViewModel { get; }
    public ICommandProvider CommandProvider { get; }
    public NuGetDocumentViewModel NuGet { get; }
    public string Title => Document != null && !Document.IsAutoSaveOnly ? Document.Name : DefaultDocumentName + GetFileExtension();
    public IDelegateCommand OpenBuildPathCommand { get; }
    public IDelegateCommand SaveCommand { get; }
    public IDelegateCommand RunCommand { get; }
    public IDelegateCommand TerminateCommand { get; }
    public IDelegateCommand FormatDocumentCommand { get; }
    public IDelegateCommand CommentSelectionCommand { get; }
    public IDelegateCommand UncommentSelectionCommand { get; }
    public IDelegateCommand RenameSymbolCommand { get; }

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
                // Make sure the execution working directory matches the current script path
                // which may have changed since we loaded.
                if (_executionHostParameters.WorkingDirectory != WorkingDirectory)
                    _executionHostParameters.WorkingDirectory = WorkingDirectory;

                await _executionHost.ExecuteAsync(documentPath, ShowIL, OptimizationLevel, cancellationToken).ConfigureAwait(true);
            }
        }
        catch (CompilationErrorException ex)
        {
            foreach (var diagnostic in ex.Diagnostics)
            {
                var startLinePosition = diagnostic.Location.GetLineSpan().StartLinePosition;
                AddResult(CompilationErrorResultObject.Create(diagnostic.Severity.ToString(), diagnostic.Id, diagnostic.GetMessage(CultureInfo.InvariantCulture), startLinePosition.Line, startLinePosition.Character));
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            AddResult(new ExceptionResultObject { Value = ex.ToString() });
        }
        finally
        {
            SetIsRunning(false);
            ReportedProgress = null;
        }
    }

    private void StartExec()
    {
        //ClearResults();

        _onError?.Invoke(null);
    }

    private void ClearResults()
    {
        lock (_results)
        {
            _results.Clear();
            _results.AddRange(_restoreResults);
        }
        OnPropertyChanged(nameof(ResultsText));

    }

    private OptimizationLevel OptimizationLevel => MainViewModel.Settings.OptimizeCompilation ? OptimizationLevel.Release : OptimizationLevel.Debug;

    private void UpdatePackages(bool alwaysRestore = true) =>
        _ = _executionHost?.UpdateReferencesAsync(alwaysRestore);

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


    public void Close()
    {
        _viewDisposable?.Dispose();
    }

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

    public bool HasReportedProgress => ReportedProgress.HasValue;

    public bool ShowIL { get; set; }

    public event EventHandler? EditorFocus;

    private void OnEditorFocus()
    {
        EditorFocus?.Invoke(this, EventArgs.Empty);
    }

    public void OnTextChanged()
    {
        IsDirty = true;

        if (IsLiveMode)
        {
            _liveModeTimer?.Change(MainViewModel.Settings.LiveModeDelayMs, Timeout.Infinite);
        }

        UpdatePackages(alwaysRestore: false);
    }

    public void Dispose()
    {
        //MainViewModel.EditorFontFamilyChanged -= OnEditorFontFamilyChanged;

        _runCts?.Dispose();
    }

    public event Action<(int line, int column)>? EditorChangeLocation;

    public void TryJumpToLine(IResultWithLineNumber result)
    {
        if (result.LineNumber is { } lineNumber)
        {
            EditorChangeLocation?.Invoke((lineNumber, result.Column));
        }
    }

    public async Task<SaveResult> SaveAsAsync()
    {
        if (_isSaving) return SaveResult.Cancel;

        _isSaving = true;
        try
        {
            var dialog = _serviceProvider.GetRequiredService<ISaveFileDialog>();
            // 过滤器（与你现有风格一致）
            dialog.Filter = new FileDialogFilter("C# Files", "cs", "csx");

            // 首选扩展（脚本用 .csx，普通用 .cs）
            var preferredExt = SourceCodeKind == SourceCodeKind.Script ? ".csx" : ".cs";

            // 初始文件名与目录
            dialog.InitialFileName = Document?.Name ?? ("New" + preferredExt);
            dialog.InitialDirectory = Path.GetDirectoryName(Document?.Path) ?? MainViewModel.DocumentRoot.Path;

            var chosenPath = await dialog.ShowAsync().ConfigureAwait(true);
            if (string.IsNullOrWhiteSpace(chosenPath))
                return SaveResult.Cancel;

            // —— 兜底补扩展名 —— //
            var finalPath = EnsureExtension(chosenPath, preferredExt);

            // 实际写盘
            await SaveDocumentAsync(finalPath).ConfigureAwait(true);

            // 首次保存要把真实 Document 挂回；已有则刷新路径/标题
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
    /// 用户显式输入了任何扩展名（.cs/.csx/.txt 等）则原样使用。
    /// </summary>
    private static string EnsureExtension(string path, string preferredExt)
    {
        if (string.IsNullOrEmpty(path)) return path;

        // 去掉可能的结尾点
        path = path.TrimEnd('.');

        var ext = Path.GetExtension(path);
        if (string.IsNullOrEmpty(ext))
            return path + preferredExt;

        return path; // 已有扩展名，尊重用户输入
    }
    //保存逻辑
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
        // 没有真实路径时，走“另存为”逻辑
        return await SaveAsAsync().ConfigureAwait(true);
    }

    // 完整替换
    async Task<SaveResult> SaveToRealPathAsync()
    {
        if (Document == null)
        {
            // 文档对象不存在，无法保存
            return SaveResult.Cancel;
        }

        await SaveDocumentAsync(Document.GetSavePath()).ConfigureAwait(true);
        IsDirty = false;
        _unsavedTextSnapshot = null; // ★ 保存成功后清空未保存快照
        Document.DeleteAutoSave();
        return SaveResult.Save;
    }




    public IReadOnlyList<TextSpan> FindAll(string searchText, bool matchCase = false, bool wholeWord = false, bool useRegex = false)
    {
        var results = new List<TextSpan>();
        var document = MainViewModel.RoslynHost.GetDocument(DocumentId);
        if (document == null || string.IsNullOrEmpty(searchText))
            return results;

        var text = document.GetTextAsync().Result.ToString();
        RegexOptions options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
        string pattern = useRegex ? searchText : Regex.Escape(searchText);
        if (wholeWord)
            pattern = $@"\b{pattern}\b";

        foreach (Match match in Regex.Matches(text, pattern, options))
        {
            results.Add(new TextSpan(match.Index, match.Length));
        }
        return results;
    }

    public async Task<int> ReplaceAllAsync(string searchText, string replaceText, bool matchCase = false, bool wholeWord = false, bool useRegex = false)
    {
        var document = MainViewModel.RoslynHost.GetDocument(DocumentId);
        if (document == null || string.IsNullOrEmpty(searchText))
            return 0;

        var text = await document.GetTextAsync().ConfigureAwait(false);
        string original = text.ToString();

        RegexOptions options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
        string pattern = useRegex ? searchText : Regex.Escape(searchText);
        if (wholeWord)
            pattern = $@"\b{pattern}\b";

        string replaced = Regex.Replace(original, pattern, replaceText, options);

        if (original != replaced)
        {
            var newText = SourceText.From(replaced);
            MainViewModel.RoslynHost.UpdateDocument(document.WithText(newText));
            OnTextChanged();
            return Regex.Matches(original, pattern, options).Count;
        }
        return 0;
    }

    private bool _isSearchPanelVisible;
    public bool IsSearchPanelVisible
    {
        get => _isSearchPanelVisible;
        //set => SetProperty(ref _isSearchPanelVisible, value);
        set
        {
            if (_isSearchPanelVisible != value)
            {
                _isSearchPanelVisible = value;
                OnPropertyChanged(nameof(IsSearchPanelVisible));
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMenuVisible)));
            }
        }
    }

    private string? _searchText;
    public string? SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string? _replaceText;
    public string? ReplaceText
    {
        get => _replaceText;
        set => SetProperty(ref _replaceText, value);
    }
    private string? _searchResultMessage;
    public string? SearchResultMessage
    {
        get => _searchResultMessage;
        set => SetProperty(ref _searchResultMessage, value);
    }


    public IDelegateCommand ToggleSearchPanelCommand { get; }
    public IDelegateCommand FindTextCommand { get; }
    public IDelegateCommand ReplaceTextCommand { get; }



    //private void FindText()
    //{
    //    var results = FindAll(SearchText ?? string.Empty);
    //    if (results.Count > 0)
    //    {
    //        SearchResultMessage = $"已找到 {results.Count} 处匹配项";
    //        // 可在此处调用高亮/定位逻辑
    //        // 例如：Editor.Highlight(results[0]);
    //    }
    //    else
    //    {
    //        SearchResultMessage = "未找到匹配项";
    //    }
    //    // 可触发UI刷新或定位
    //}


    private async void ExecuteReplaceText()
    {
        if (!string.IsNullOrEmpty(SearchText))
        {
            await ReplaceAllAsync(SearchText ?? string.Empty, ReplaceText ?? string.Empty).ConfigureAwait(true);
        }
    }

    private IReadOnlyList<TextSpan> _searchResults = Array.Empty<TextSpan>();

    public IReadOnlyList<TextSpan> SearchResults
    {
        get => _searchResults;
        private set => SetProperty(ref _searchResults, value);
    }

    //private void FindText()
    //{
    //    var results = FindAll(SearchText ?? string.Empty);
    //    SearchResults = results;
    //    SearchResultMessage = results.Count > 0
    //        ? $"已找到 {results.Count} 处匹配项"
    //        : "未找到匹配项";
    //}

    public event Action<IReadOnlyList<TextSpan>, int>? SearchHighlightRequested;

    public event Action<TextSpan>? SearchJumpRequested;

    private void FindText()
    {
        //var results = FindAll(SearchText ?? string.Empty);
        //SearchResults = results;
        //SearchResultMessage = results.Count > 0
        //    ? $"已找到 {results.Count} 处匹配项"
        //    : "未找到匹配项";

        //// 触发高亮事件
        //SearchHighlightRequested?.Invoke(results);

        //// 自动跳转到第一个匹配项
        //if (results.Count > 0)
        //{
        //    _currentHighlightIndex = 0;
        //    SearchJumpRequested?.Invoke(results[0]);
        //}
        var results = FindAll(SearchText ?? string.Empty);
        SearchResults = results;
        SearchResultMessage = results.Count > 0
            ? $"已找到 {results.Count} 处匹配项"
            : "未找到匹配项";

        _currentHighlightIndex = results.Count > 0 ? 0 : -1;
        SearchHighlightRequested?.Invoke(results, _currentHighlightIndex);

        if (results.Count > 0)
        {
            SearchJumpRequested?.Invoke(results[0]);
        }
    }

    public event Action? SearchClearHighlightRequested;

    public IDelegateCommand JumpToNextHighlightCommand { get; }
    public IDelegateCommand JumpToPrevHighlightCommand { get; }

    private int _currentHighlightIndex = -1;

    private void JumpToNextHighlight()
    {
        if (SearchResults.Count == 0) return;
        _currentHighlightIndex = (_currentHighlightIndex + 1) % SearchResults.Count;
        SearchHighlightRequested?.Invoke(SearchResults, _currentHighlightIndex);
        SearchJumpRequested?.Invoke(SearchResults[_currentHighlightIndex]);
    }

    private void JumpToPrevHighlight()
    {
        if (SearchResults.Count == 0) return;
        _currentHighlightIndex = (_currentHighlightIndex - 1 + SearchResults.Count) % SearchResults.Count;
        SearchHighlightRequested?.Invoke(SearchResults, _currentHighlightIndex);
        SearchJumpRequested?.Invoke(SearchResults[_currentHighlightIndex]);
    }

    public IDelegateCommand DebugCommand { get; }

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
            // 编译完成后输出信息
            var result1 = new ResultObject
            {
                Header = "[Debug]",
                Value = "DEBUG编译完成",
                Type = "[Debug]",
                LineNumber = null
            };
            AddResult(result1);
            Console.WriteLine("Debug 方法已被调用");
        }
        catch (CompilationErrorException ex)
        {
            foreach (var diagnostic in ex.Diagnostics)
            {
                var startLinePosition = diagnostic.Location.GetLineSpan().StartLinePosition;
                AddResult(CompilationErrorResultObject.Create(diagnostic.Severity.ToString(), diagnostic.Id, diagnostic.GetMessage(CultureInfo.InvariantCulture), startLinePosition.Line, startLinePosition.Character));
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            AddResult(new ExceptionResultObject { Value = ex.ToString() });
        }
        finally
        {
            SetIsRunning(false);
            ReportedProgress = null;
        }
    }


    public IDelegateCommand OpenhelpCommand { get; }
    public IDelegateCommand OpenLicenseCommand { get; }
    public IDelegateCommand OpenAboutVEMSCommand { get; }
    public IDelegateCommand OpenContactCommand { get; }
    public ICommand OpenBaiduCommand { get; }
    private void AboutVEMS()
    {
        Console.WriteLine("About VEMS 方法已被调用");
        var result1 = new ResultObject
        {
            Header = "[About VEMS]",
            Value = "VEMS stands for Virtual ElectroMagnetic Solutions",
            Type = "[About VEMS]",
            LineNumber = null
        };
        AddResult(result1);
        var result2 = new ResultObject
        {
            Header = "[About VEMS]",
            Value = "VEMS is developed by the Computational Photonics group, KLAMOS, CAS",
            Type = "[About VEMS]",
            LineNumber = null
        };
        AddResult(result2);
    }
    private void Contact()
    {
        Console.WriteLine("Contact 方法已被调用");
        var result = new ResultObject
        {
            Header = "[Contact]",
            Value = "Contact zhangsite@ciomp.ac.cn to learn more",
            Type = "[Contact]",
            LineNumber = null
        };
        AddResult(result);
    }
    //private void AddTestResult()
    //{
    //    Console.WriteLine("AddTestResult 方法已被调用");
    //    var result = new ResultObject
    //    {
    //        Header = "测试输出",
    //        Value = "这是自定义输出内容",
    //        Type = "系统",
    //        LineNumber = null
    //    };
    //    AddResult(result);
    //}
    private void License()
    {
        Console.WriteLine("License 方法已被调用");
        var result1 = new ResultObject
        {
            Header = "[License]",
            Value = "Software license is valid for education only",
            Type = "[License]",
            LineNumber = null
        };
        AddResult(result1);
        var result2 = new ResultObject
        {
            Header = "[License]",
            Value = "License will expire on 2025/9/30 0:00:00",
            Type = "[License]",
            LineNumber = null
        };
        AddResult(result2);
    }
    private void help()
    {
        Console.WriteLine("help 方法已被调用");
        var result = new ResultObject
        {
            Header = "[Help]",
            Value = "Feel free to contact zhangsite@ciomp.ac.cn",
            Type = "[Help]",
            LineNumber = null
        };
        AddResult(result);
    }
    private void OpenBaidu()
    {
        Console.WriteLine("About VEMS 方法已被调用");
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "http://159.226.165.79/published/vems-software/",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // 可选：将异常信息输出到结果区
            AddResult(new ResultObject
            {
                Header = "异常",
                Value = ex.Message,
                Type = "OpenBaidu",
                LineNumber = null
            });
        }
    }

    public ICommand CopyErrorCommand { get; }
    public ICommand CopyErrorLineCommand { get; }

    public event Action<string>? RequestCopyText;

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

    public object? SelectedResultItem { get; set; }

    private void OnTreeViewSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SelectedResultItem = (sender as TreeView)?.SelectedItem;
    }

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
                Console.WriteLine("[CopyError] 未能获取剪贴板对象（Clipboard为null）");
            }
        }
        else
        {
            Console.WriteLine("[CopyError] 未找到活动窗口，无法访问剪贴板");
        }
    }





    private string _selectedProcessFilter = "All";
    public string SelectedProcessFilter
    {
        get => _selectedProcessFilter;
        set => SetProperty(ref _selectedProcessFilter, value);
    }

    public ObservableCollection<string> ProcessFilterOptions { get; } = new() { "All" };

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
            SourceCodeKind.Script // 你当前项目以 .csx 为主
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
        File.WriteAllText(runPath, code, System.Text.Encoding.UTF8);

        // 日志文件
        var timeTag = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var logPath = Path.Combine(bgBuildPath, $"background-{timeTag}.log");
        using (File.CreateText(logPath)) { } // 创建/清空

        void AppendLine(string line)
        {
            try { File.AppendAllText(logPath, line + Environment.NewLine); } catch { /* ignore */ }
        }

        // ============= 把执行期事件“镜像”到结果栏（同时继续写日志） =============
        bgHost.Dumped += r =>
        {
            var text = r?.Value?.ToString() ?? string.Empty;
            AppendLine(text);
            if (!string.IsNullOrWhiteSpace(text))
            {
                _dispatcher.InvokeAsync(() =>
                {
                    AddResult(new ResultObject
                    {
                        Header = "[Run]",
                        Value = text,
                        Type = "输出",
                        LineNumber = null
                    });
                }, AppDispatcherPriority.Low);
            }
        };

        bgHost.Error += err =>
        {
            var text = err?.Value ?? string.Empty;
            AppendLine(text);
            if (!string.IsNullOrWhiteSpace(text))
            {
                _dispatcher.InvokeAsync(() =>
                {
                    AddResult(new ExceptionResultObject { Value = text });
                }, AppDispatcherPriority.Low);
            }
        };

        bgHost.CompilationErrors += errs =>
        {
            foreach (var e in errs)
            {
                var sev = e?.Severity?.ToString() ?? "Error";
                var msg = e?.Message ?? string.Empty;

                // ★ 修复：先用可空 string? 接住，随后用 ?? 给默认值
                string? codeStr = null;
                try
                {
                    codeStr = e?.GetType().GetProperty("ErrorCode")?.GetValue(e) as string;
                }
                catch
                {
                    // ignore
                }
                string code = string.IsNullOrWhiteSpace(codeStr) ? "CS0000" : codeStr!; // ! 因为上面已判空

                // 行列做兜底
                var line = 0;
                var col = 0;
                try { line = (int)(e?.LineNumber ?? 0); } catch { }
                try { col = (int)(e?.Column ?? 0); } catch { }

                AppendLine($"[{sev}] {msg} (L{line},C{col})");

                _dispatcher.InvokeAsync(() =>
                {
                    AddResult(CompilationErrorResultObject.Create(
                        sev,    
                        code,
                        msg,    
                        line,
                        col
                    ));
                }, AppDispatcherPriority.Low);
            }
        };


        bgHost.Disassembled += il =>
        {
            AppendLine("===== IL =====");
            AppendLine(il ?? string.Empty);
            AppendLine("===== END IL =====");

            if (!string.IsNullOrEmpty(il))
            {
                _dispatcher.InvokeAsync(() =>
                {
                    AddResult(new ResultObject
                    {
                        Header = "[IL]",
                        Value = il,
                        Type = "信息",
                        LineNumber = null
                    });
                }, AppDispatcherPriority.Low);
            }
        };

        bgHost.RestoreStarted += () =>
        {
            AppendLine("[restore] started");
            _dispatcher.InvokeAsync(() =>
            {
                AddResult(new ResultObject
                {
                    Header = "[restore]",
                    Value = "started",
                    Type = "信息",
                    LineNumber = null
                });
            }, AppDispatcherPriority.Low);
        };

        bgHost.RestoreCompleted += rr =>
        {
            AppendLine($"[restore] {(rr.Success ? "success" : "failed")}");
            _dispatcher.InvokeAsync(() =>
            {
                if (rr.Success)
                {
                    AddResult(new ResultObject
                    {
                        Header = "[restore]",
                        Value = "success",
                        Type = "信息",
                        LineNumber = null
                    });
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

                // 可见化提示一下新进程
                AddResult(new ResultObject
                {
                    Header = "[Run]",
                    Value = $"Process started: {tag}",
                    Type = "信息",
                    LineNumber = null
                });
            }, AppDispatcherPriority.Normal);
        }
        try { bgHost.ProcessStarted += OnProcessStarted; } catch { /* 兼容不同分支，无事 */ }

        // 还原引用后执行
        await bgHost.UpdateReferencesAsync(true).ConfigureAwait(false);

        info.Task = Task.Run(async () =>
        {
            try
            {
                AppendLine("[start] " + DateTime.Now.ToString("O", CultureInfo.InvariantCulture));
                _dispatcher.InvokeAsync(() =>
                {
                    AddResult(new ResultObject
                    {
                        Header = "[Run]",
                        Value = "started",
                        Type = "信息",
                        LineNumber = null
                    });
                }, AppDispatcherPriority.Low);

                await bgHost.ExecuteAsync(runPath, /*showIL*/ false, OptimizationLevel, info.Cancellation.Token).ConfigureAwait(false);

                AppendLine("[done] " + DateTime.Now.ToString("O", CultureInfo.InvariantCulture));
                _dispatcher.InvokeAsync(() =>
                {
                    AddResult(new ResultObject
                    {
                        Header = "[Run]",
                        Value = "done",
                        Type = "信息",
                        LineNumber = null
                    });
                }, AppDispatcherPriority.Low);

                BackgroundRunManager.Instance.MarkCompleted(info);
            }
            catch (OperationCanceledException)
            {
                AppendLine("[canceled]");
                _dispatcher.InvokeAsync(() =>
                {
                    AddResult(new ResultObject
                    {
                        Header = "[Run]",
                        Value = "canceled",
                        Type = "信息",
                        LineNumber = null
                    });
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
                        for (int i = ProcessFilterOptions.Count - 1; i >= 0; i--)
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

    //09-10
    private bool _isPreviewMode;
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

    // 未保存文本的内存快照（用于标签切换后恢复）
    private string? _unsavedTextSnapshot;

    /// <summary>
    /// 由视图在文本变化/卸载时调用，更新未保存文本的内存快照
    /// </summary>
    public void UpdateUnsavedText(string? text)
    {
        _unsavedTextSnapshot = text;
    }

    /// <summary>
    /// 若已保存（IsDirty=false），则清空未保存快照
    /// </summary>
    public void ClearUnsavedTextSnapshotIfSaved()
    {
        if (!IsDirty) _unsavedTextSnapshot = null;
    }

    // ① 新增：用于标签显示的标题（未保存时追加 *）
    public string TabTitle => IsDirty ? $"{Title} *" : Title;

    // ② 替换原有 IsDirty 属性为完整写法（注意 private set）
    public bool IsDirty
    {
        get => _isDirty;
        private set
        {
            if (SetProperty(ref _isDirty, value))
            {
                // 脏状态变化时，顺带刷新 TabTitle
                OnPropertyChanged(nameof(TabTitle));
            }
        }
    }



    public LocalizationManager Localized => LocalizationManager.Instance;
}
