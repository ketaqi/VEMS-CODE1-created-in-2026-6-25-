using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaEdit.Search;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;
using NuGet.Configuration;
using NuGet.Packaging;
using ReactiveUI;
using RoslynPad.Build;
using RoslynPad.Editor;
using RoslynPad.Roslyn;
using RoslynPad.Roslyn.Rename;
using RoslynPad.Themes;
using RoslynPad.UI.Dialogs;
using RoslynPad.Utilities;
using static System.Net.Mime.MediaTypeNames;
using Application = Avalonia.Application;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.IntegralTransforms;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Numerics;
using Avalonia.PropertyGrid.Services;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Document;
using DynamicData;
using RoslynPad.UI;
using NotificationObject = RoslynPad.ViewModels.NotificationObject;
using FileDialogFilter = RoslynPad.UI.FileDialogFilter;
using RoslynPad.Models;

namespace RoslynPad.ViewModels;

/// <summary>
/// 应用主 ViewModel。
/// <para>
/// 职责：文档树与打开文档管理、主题/语言/字体设置、命令聚合（新建/打开/保存/运行/调试/查找替换等）、
/// 预览内容加载、NuGet/遥测、性能测试入口等。
/// </para>
/// <remarks>
/// - 所有 UI 更新通过 <see cref="Dispatcher.UIThread"/> 切回 UI 线程；
/// - 预览文本加载具备取消与序号保护，避免竞态覆盖；
/// - 未更改任何业务逻辑，仅补充注释与少量说明性行内注释。
/// </remarks>
/// </summary>
public abstract class MainViewModel : NotificationObject, IDisposable
{
    // ======= 侧边栏（多文档树）可见性状态 =======
    private bool _isDocumentTreeViewVisible = true;
    private bool _isUserPreferencesViewVisible;
    private bool _isPerformanceSettingsViewVisible;
    private bool _isDLLExpandViewVisible;
    private bool _isDocumentTreeView4Visible;
    private bool _isRunAndDebugViewVisible;
    private static readonly Version s_currentVersion = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version();

    // ======= 依赖服务 =======
    private readonly IServiceProvider _serviceProvider;          // IoC 服务定位
    private readonly ITelemetryProvider _telemetryProvider;      // 遥测/错误上报
    private readonly ICommandProvider _commands;                 // 统一命令工厂
    private readonly DocumentFileWatcher _documentFileWatcher;   // 文档变更监控
    private readonly string _editorConfigPath;                   // .editorconfig 路径
    private readonly VsCodeThemeReader _themeManager;            // VSCode 主题读取器

    // ======= 运行时状态 =======
    private OpenDocumentViewModel? _previewOpenDocument; // 预览页签（临时）
    private OpenDocumentViewModel? _currentOpenDocument; // 当前激活文档
    //private GlobalResultsViewModel? _currentOpenDocument1; // 当前激活文档
    private bool _hasUpdate;                              // 是否检测到新版本
    private int _editorFontSize;                       // 编辑器字号

    private string? _searchText;                          // 搜索关键字
    private bool _isWithinSearchResults;                  // 搜索过滤开关
    private bool _isInitialized;                          // 初始化完成标志
    private DocumentViewModel _documentRoot;              // 文档树根
    private DocumentWatcher? _documentWatcher;            // 文档树文件系统监视器
    private RoslynHost? _roslynHost;                      // Roslyn 宿主
    private Theme? _theme;                                // 当前主题
                                                          //private bool? _isSystemDarkTheme;                   // （保留位）



    /// <summary>可供 UI 绑定的设置快照。</summary>
    /// <summary>
    /// Settings。
    /// </summary>
    public IApplicationSettingsValues Settings { get; }

    /// <summary>文档树根节点（TreeView 数据源）。</summary>
    public DocumentViewModel DocumentRoot
    {
        get => _documentRoot;
        private set => SetProperty(ref _documentRoot, value);
    }

    /// <summary>
    /// Roslyn 宿主，外部总能拿到非空值。
    /// </summary>
    public RoslynHost RoslynHost
    {
        get => _roslynHost.NotNull();
        private set => _roslynHost = value;
    }

    /// <summary>
    /// 是否已完成初始化（影响某些命令/菜单可见性）。
    /// </summary>
    public bool IsInitialized
    {
        get => _isInitialized;
        private set
        {
            SetProperty(ref _isInitialized, value);
            OnPropertyChanged(nameof(HasNoOpenDocuments));
        }
    }

    /// <summary>
    /// 构造函数：注入依赖、准备命令、初始化文档树/设置/主题等。
    /// </summary>
    /// <summary>
    /// 执行 Main View Model 操作。
    /// </summary>
    public MainViewModel(
        IServiceProvider serviceProvider,
        ITelemetryProvider telemetryProvider,
        ICommandProvider commands,
        IApplicationSettings settings,
        NuGetViewModel nugetViewModel,
        DocumentFileWatcher documentFileWatcher,
        IAppDispatcher appDispatcher,
        ILogger<MainViewModel> logger
        )
    {
        GlobalResults = new GlobalResultsViewModel(commands);
        _serviceProvider = serviceProvider;
        _telemetryProvider = telemetryProvider;
        _commands = commands;
        _documentFileWatcher = documentFileWatcher;
        _themeManager = new VsCodeThemeReader();

        // 1) 载入默认设置与 editorconfig 路径
        settings.LoadDefault();
        _editorConfigPath = Path.Combine(settings.GetDefaultDocumentPath(), ".editorconfig");
        Settings = settings.Values;

        // 2) 遥测初始化 + 监听错误变化 -> 触发 UI 通知
        _telemetryProvider.Initialize(s_currentVersion.ToString(), settings);
        _telemetryProvider.LastErrorChanged += () =>
        {
            OnPropertyChanged(nameof(LastError));
            OnPropertyChanged(nameof(HasError));
        };

        // 3) 依赖 VM 与核心命令
        NuGet = nugetViewModel;

        // ===== 文件/文档命令 =====
        NewDocumentCommand = commands.Create<SourceCodeKind>(CreateNewDocument);
        OpenFileCommand = commands.CreateAsync(OpenFile);
        CloseCurrentDocumentCommand = commands.CreateAsync(CloseCurrentDocument);
        CloseDocumentCommand = commands.CreateAsync<OpenDocumentViewModel>(CloseDocument);

        // ===== 错误/反馈 =====
        ClearErrorCommand = commands.Create(_telemetryProvider.ClearLastError);
        ReportProblemCommand = commands.Create(ReportProblem);

        // ===== 设置/优化 =====
        EditUserDocumentPathCommand = commands.Create(EditUserDocumentPath);
        ToggleOptimizationCommand = commands.Create(() => Settings.OptimizeCompilation = !Settings.OptimizeCompilation);
        ClearRestoreCacheCommand = commands.Create(ClearRestoreCache);

        // 4) 字体与文档树初始化
        _editorFontSize = Settings.EditorFontSize;
        _editorFontFamily = Settings.EditorFontFamily;
        _outputFontSize = Settings.OutputFontSize;
        _editorFontFamily1 = Settings.EditorFontFamily1;

        _documentRoot = CreateDocumentRoot();

        // 打开的文档集合（页签区）
        OpenDocuments = [];
        OpenDocuments.CollectionChanged += (sender, args) =>
            OnPropertyChanged(nameof(HasNoOpenDocuments));

        // ===== 侧边栏文档树（5 组互斥显示）命令 =====
        ShowDocumentTreeViewCommand = commands.Create(() =>
        {
            IsDocumentTreeViewVisible = true;
            IsUserPreferencesViewVisible = false;
            IsPerformanceSettingsViewVisible = false;
            IsDLLExpandViewVisible = false;
            IsDocumentTreeView4Visible = false;
            IsRunAndDebugViewVisible = false;
        });

        ShowUserPreferencesViewCommand = commands.Create(() =>
        {
            IsDocumentTreeViewVisible = false;
            IsUserPreferencesViewVisible = true;
            IsPerformanceSettingsViewVisible = false;
            IsDLLExpandViewVisible = false;
            IsDocumentTreeView4Visible = false;
            IsRunAndDebugViewVisible = false;
        });

        ShowPerformanceSettingsViewCommand = commands.Create(() =>
        {
            IsDocumentTreeViewVisible = false;
            IsUserPreferencesViewVisible = false;
            IsPerformanceSettingsViewVisible = true;
            IsDLLExpandViewVisible = false;
            IsDocumentTreeView4Visible = false;
            IsRunAndDebugViewVisible = false;
        });

        ShowDLLExpandViewCommand = commands.Create(() =>
        {
            IsDocumentTreeViewVisible = false;
            IsUserPreferencesViewVisible = false;
            IsPerformanceSettingsViewVisible = false;
            IsDLLExpandViewVisible = true;
            IsDocumentTreeView4Visible = false;
            IsRunAndDebugViewVisible = false;
        });

        ShowDocumentTreeView4Command = commands.Create(() =>
        {
            IsDocumentTreeViewVisible = false;
            IsUserPreferencesViewVisible = false;
            IsPerformanceSettingsViewVisible = false;
            IsDLLExpandViewVisible = false;
            IsDocumentTreeView4Visible = true;
            IsRunAndDebugViewVisible = false;
        });

        ShowRunAndDebugViewCommand = commands.Create(() =>
        {
            IsDocumentTreeViewVisible = false;
            IsUserPreferencesViewVisible = false;
            IsPerformanceSettingsViewVisible = false;
            IsDLLExpandViewVisible = false;
            IsDocumentTreeView4Visible = false;
            IsRunAndDebugViewVisible = true;

        });

        // ===== 字号调整（示例：一个设定为固定 96，一个递减 1）=====
        IncreaseFontSizeCommand = commands.Create(() => EditorFontSize = 96);
        DecreaseFontSizeCommand = commands.Create(() => EditorFontSize -= 1);

        // ===== 主题/语言切换 =====
        CycleThemeCommand = commands.Create(CycleTheme);
        SwitchLanguageCommand = commands.Create<string>(ExecuteSwitchLanguage);

        // ===== 目录开关、运行/保存/调试、面板、关于等 =====
        OpenFolderCommand1 = commands.CreateAsync(OpenFolderAsync1);
        CloseFolderCommand1 = commands.CreateAsync(CloseFolder);
        RunCommand1 = commands.CreateAsync(RunCurrentDocument);
        SaveAsCommand1 = commands.Create(SaveAsCurrentDocument);
        //SaveAllDocumentsCommand     = commands.CreateAsync(SaveAllDocumentsAsync);
        CloseAllDocumentsCommand = commands.CreateAsync(CloseAllDocuments);
        DebugCurrentDocumentCommand = commands.Create(DebugCurrentDocument);
        RunCommand = commands.Create(Run);
        OpenBaiduCommand = commands.Create(OpenBaidu);
        OpenLicenseCommand = commands.Create(License);
        OpenhelpCommand = commands.Create(help);
        OpenSearchPanelCommand = commands.Create(OpenSearchPanel);
        OpenReplacePanelCommand = commands.Create(OpenReplacePanel);
        OpenContactCommand = commands.Create(Contact);
        OpenAboutVEMSCommand = commands.Create(AboutVEMS);
        SaveCurrentDocumentCommand = commands.Create(SaveCurrentDocument);
        CopyAllResultsCommand = commands.Create(CopyAllResults);
        ClearResultsCommand = commands.Create(ClearResults);

        // ===== 初始路径 & 控制台 & 折叠面板 =====
        FolderPathInput = Settings.DocumentPath ?? DocumentRoot?.Path ?? string.Empty;
        OpenConsoleCommand = commands.Create(ExecuteOpenConsole);
        IsExpanded = false; // “更多设置”面板折叠

        // ===== 新建模板面板开关（ReactiveCommand 示例）=====
        ShowNewDoc1Command = ReactiveCommand.Create(() => IsNewDoc1Visible = true);
        HideNewDoc1Command = ReactiveCommand.Create(() => IsNewDoc1Visible = false);

        LoadShortcutsCommand = commands.Create(LoadShortcuts);

        FormatCurrentDocumentCommand = commands.Create(FormatCurrentDocument);

        UncommentSelectionCommand = commands.Create(UncommentSelection);

        CommentSelectionCommand = commands.Create(CommentSelection);

        ApplyTheme(InitialTheme);


        ApplyThemeCommand = commands.Create<string>(themeName =>
        {
            ApplyTheme(themeName);
        });

        OpenInitialFolderCommand = commands.Create(OpenInitialFolder);

        ExportShortcutsCommand = commands.CreateAsync(ExportShortcutsWithDialogAsync);
        ImportShortcutsCommand = commands.CreateAsync(ImportShortcutsWithDialogAsync);

        ExitApplicationCommand = commands.CreateAsync(ExitApplication);
        LoadShortcutsCommand = commands.Create(LoadShortcuts);


        LoadShortcuts();

        GetCurrentOsPlatform();

        PrintOpenDocumentsCommand = commands.Create(PrintOpenDocuments);

        TerminateCommand = commands.Create(Terminate);

        SetDefaultPlatformCommand = commands.Create(SetDefaultPlatform);

        _platformsFactory = serviceProvider.GetRequiredService<IPlatformsFactory>();

        InitializePlatforms();

        //NuGet1 = serviceProvider.GetRequiredService<NuGetDocumentViewModel>();

        GlobalResults.EditorFontFamily1 = EditorFontFamily1;
        GlobalResults.FontSize = OutputFontSize;
        ExecuteSwitchLanguage(InitialLanguage);
    }

    /// <summary>
    /// Nu Get1。
    /// </summary>
    public NuGetDocumentViewModel? NuGet1 => _currentOpenDocument?.NuGet;
    /// <summary>
    /// 执行 new 操作。
    /// </summary>
    public ObservableCollection<string> ProcessFilterOptions { get; } = new() { "All" };
    private void InitializePlatforms()
    {
        AvailablePlatforms = _platformsFactory.GetExecutionPlatforms();
        var names = AvailablePlatforms.Select(p => p.ToString()).ToArray().AsReadOnly();
        AvailablePlatformNames = names;
        var platforms = _platformsFactory?.GetExecutionPlatforms()
                        ?? Array.Empty<ExecutionPlatform>();

    }

    private IReadOnlyList<string>? _availablePlatformNames;
    // 新增：AvailablePlatforms 的字符串表示，供其他地方以 string 操作
    public IReadOnlyList<string> AvailablePlatformNames
    {
        get => _availablePlatformNames ?? throw new ArgumentNullException(nameof(_availablePlatformNames));

        private set => SetProperty(ref _availablePlatformNames, value ?? Array.Empty<string>());
    }
    public IReadOnlyList<ExecutionPlatform> AvailablePlatforms
    {
        get => _availablePlatforms ?? throw new ArgumentNullException(nameof(_availablePlatforms));
        private set => SetProperty(ref _availablePlatforms, value);
    }

    private readonly IPlatformsFactory _platformsFactory;

    private IReadOnlyList<ExecutionPlatform>? _availablePlatforms;

    /// <summary>
    /// Process Filter Options1。
    /// </summary>
    public ObservableCollection<string>? ProcessFilterOptions1 => CurrentOpenDocument?.ProcessFilterOptions;
    // 简单示例：每次访问时用 LINQ 映射
    // 即时映射为 string[]，每次访问都会计算


    /// <summary>
    /// 命令：Terminate Command。
    /// </summary>
    public ICommand TerminateCommand { get; }
    /// <summary>
    /// 命令：Set Default Platform Command。
    /// </summary>
    public ICommand SetDefaultPlatformCommand { get; }

    //新增
    /// <summary>
    /// 执行 Terminate 操作。
    /// </summary>
    public void Terminate()
    {
        if (CurrentOpenDocument != null && CurrentOpenDocument.TerminateCommand.CanExecute())
        {
            CurrentOpenDocument.TerminateCommand.Execute();
            Console.WriteLine("Terminated调用");
            OutputResult("[Terminate]", "TerminateCommand executed for document: " + (CurrentOpenDocument.Title ?? "(无标题)"), null, null);
        }
    }
    /// <summary>
    /// 执行 Set Default Platform 操作。
    /// </summary>
    public void SetDefaultPlatform()
    {
        if (CurrentOpenDocument != null && CurrentOpenDocument.SetDefaultPlatformCommand.CanExecute())
        {
            CurrentOpenDocument.SetDefaultPlatformCommand.Execute();
            Console.WriteLine("SetDefaultPlatform调用");
            OutputResult("[SetDefaultPlatform]", "SetDefaultPlatformCommand executed for document: " + (CurrentOpenDocument.Title ?? "(无标题)"), null, null);
        }
    }
    private int _selectedRibbonTabIndex;
    /// <summary>
    /// 顶部菜单 Ribbon 的当前选中页索引（配合快捷键/按钮在不同菜单页间切换）。
    /// </summary>
    public int SelectedRibbonTabIndex
    {
        get => _selectedRibbonTabIndex;
        set
        {
            // 仅记录切换，实际 UI 由绑定刷新
            Console.WriteLine($"[SelectedRibbonTabIndex] Setter: old={_selectedRibbonTabIndex}, new={value}");
            SetProperty(ref _selectedRibbonTabIndex, value);
        }
    }
    /// <summary>
    /// 快捷切换到 Ribbon 页签 0 的命令。
    /// </summary>
    /// <summary>
    /// 命令：Create。
    /// </summary>
    public ICommand ShowMenuItem1TabCommand => _commands.Create(() =>
    {
        Console.WriteLine("[ShowMenuItem1TabCommand] 执行，切换到 Tab 0");
        SelectedRibbonTabIndex = 0; // 切换到第 0 页
    });
    /// <summary>
    /// 快捷切换到 Ribbon 页签 1 的命令。
    /// </summary>
    /// <summary>
    /// 命令：Create。
    /// </summary>
    public ICommand ShowMenuItem2TabCommand => _commands.Create(() =>
    {
        Console.WriteLine("[ShowMenuItem2TabCommand] 执行，切换到 Tab 1");
        SelectedRibbonTabIndex = 1; // 切换到第 1 页
    });
    /// <summary>
    /// 快捷切换到 Ribbon 页签 2 的命令。
    /// </summary>
    /// <summary>
    /// 命令：Create。
    /// </summary>
    public ICommand ShowMenuItem3TabCommand => _commands.Create(() =>
    {
        Console.WriteLine("[ShowMenuItem3TabCommand] 执行，切换到 Tab 2");
        SelectedRibbonTabIndex = 2; // 切换到第 2 页
    });
    /// <summary>
    /// 快捷切换到 Ribbon 页签 3 的命令。
    /// </summary>
    /// <summary>
    /// 命令：Create。
    /// </summary>
    public ICommand ShowMenuItem4TabCommand => _commands.Create(() =>
    {
        Console.WriteLine("[ShowMenuItem4TabCommand] 执行，切换到 Tab 3");
        SelectedRibbonTabIndex = 3; // 切换到第 3 页
    });

    /// <summary>
    /// 是否启用“复数矩阵乘法”性能/功能测试。
    /// </summary>
    private bool _isComplexMatrixMultiplyEnabled;

    /// <summary>
    /// 复数矩阵乘法开关。
    /// <para>为 <c>true</c> 时在相应菜单/流程中启用复数矩阵乘法测试。</para>
    /// </summary>
    public bool IsComplexMatrixMultiplyEnabled
    {
        get => _isComplexMatrixMultiplyEnabled;
        set => SetProperty(ref _isComplexMatrixMultiplyEnabled, value);
    }

    /// <summary>
    /// 是否启用“复数线性方程求解”性能/功能测试。
    /// </summary>
    private bool _isComplexLinearSolveEnabled;

    /// <summary>
    /// 复数线性求解开关。
    /// <para>为 <c>true</c> 时在相应菜单/流程中启用复数线性方程组求解测试。</para>
    /// </summary>
    public bool IsComplexLinearSolveEnabled
    {
        get => _isComplexLinearSolveEnabled;
        set => SetProperty(ref _isComplexLinearSolveEnabled, value);
    }

    /// <summary>
    /// 是否启用“复数奇异值分解（SVD）”性能/功能测试。
    /// </summary>
    private bool _isComplexSvdEnabled;

    /// <summary>
    /// 复数 SVD 开关。
    /// <para>为 <c>true</c> 时在相应菜单/流程中启用复数奇异值分解测试。</para>
    /// </summary>
    public bool IsComplexSvdEnabled
    {
        get => _isComplexSvdEnabled;
        set => SetProperty(ref _isComplexSvdEnabled, value);
    }

    /// <summary>
    /// 是否启用“复数特征值分解（Eigen）”性能/功能测试。
    /// </summary>
    private bool _isComplexEigenEnabled;

    /// <summary>
    /// 复数特征值分解开关。
    /// <para>为 <c>true</c> 时在相应菜单/流程中启用复数特征值分解测试。</para>
    /// </summary>
    public bool IsComplexEigenEnabled
    {
        get => _isComplexEigenEnabled;
        set => SetProperty(ref _isComplexEigenEnabled, value);
    }

    /// <summary>
    /// 是否启用“复数快速傅里叶变换（FFT）”性能/功能测试。
    /// </summary>
    private bool _isComplexFftEnabled;

    /// <summary>
    /// 复数 FFT 开关。
    /// <para>为 <c>true</c> 时在相应菜单/流程中启用复数 FFT 测试。</para>
    /// </summary>
    public bool IsComplexFftEnabled
    {
        get => _isComplexFftEnabled;
        set => SetProperty(ref _isComplexFftEnabled, value);
    }

    /// <summary>
    /// 单项测试的重复运行次数（最小为 1）。
    /// <para>用于统计平均/最小/最大耗时时的样本次数。</para>
    /// </summary>
    private int _testRunCount = 3;

    /// <summary>
    /// 测试重复次数属性。
    /// <para>小于 1 的设置将被强制修正为 1。</para>
    /// </summary>
    public int TestRunCount
    {
        get => _testRunCount;
        set
        {

            if (value < 1) value = 1; // 保底：至少运行一次
            var A = value.ToString(CultureInfo.InvariantCulture);
            // 原始模板
            var template = "Number of tests modified to " + A;

            // 使用统一输出函数输出包含插入内容的消息
            OutputResult("[TestRunCount]", template, null, null);
            SetProperty(ref _testRunCount, value);
        }
    }

    // ...已有内容...

    // 矩阵乘法性能测试（支持复数）
    /// <summary>
    /// 触发“矩阵乘法”性能测试的命令。
    /// </summary>
    /// <summary>
    /// 命令：Create Async。
    /// </summary>
    public ICommand TestMatrixMultiplyCommand => _commands.CreateAsync(ExecuteTestMatrixMultiply);

    /// <summary>
    /// 触发“线性方程组求解”性能测试的命令。
    /// </summary>
    /// <summary>
    /// 命令：Create Async。
    /// </summary>
    public ICommand TestMatrixMultiplyCommand1 => _commands.CreateAsync(ExecuteTestMatrixMultiply1);

    /// <summary>
    /// 触发“SVD（奇异值分解）”性能测试的命令。
    /// </summary>
    /// <summary>
    /// 命令：Create Async。
    /// </summary>
    public ICommand TestMatrixMultiplyCommand2 => _commands.CreateAsync(ExecuteTestMatrixMultiply2);

    /// <summary>
    /// 触发“特征值分解（Eigen/EVD）”性能测试的命令。
    /// </summary>
    /// <summary>
    /// 命令：Create Async。
    /// </summary>
    public ICommand TestMatrixMultiplyCommand3 => _commands.CreateAsync(ExecuteTestMatrixMultiply3);

    /// <summary>
    /// 触发“FFT（快速傅里叶变换）”性能测试的命令。
    /// </summary>
    /// <summary>
    /// 命令：Create Async。
    /// </summary>
    public ICommand TestMatrixMultiplyCommand4 => _commands.CreateAsync(ExecuteTestMatrixMultiply4);

    /// <summary>
    /// 执行“矩阵乘法”性能测试：按预设尺寸多次运行并统计平均/最小/最大耗时。
    /// </summary>
    /// <summary>
    /// 执行 Execute Test Matrix Multiply 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task ExecuteTestMatrixMultiply()
    {
        // 保持无参签名（但改为异步 Task 返回类型），先输出 TestRunCount（使用不依赖区域设置的格式）
        var A = TestRunCount.ToString(CultureInfo.InvariantCulture);
        OutputResult("[TestRunCount]", "Number of tests modified to " + A, "[Info]", null);

        int[] sizes = { 101, 201, 501, 1001, 2001 };
        var results = new List<(int size, double avg, double min, double max)>();

        foreach (var size in sizes)
        {
            var startMsg = $"Running test for matrix size {size}x{size} ... IsComplex={IsComplexFftEnabled}";
            Console.WriteLine(startMsg);
            // 按约定的格式同时输出到结果面板（类型为 Info）
            OutputResult("[Fast Fourier]", startMsg, "[Info]", null);

            // 在后台线程执行耗时的 TestFft，以免阻塞 UI 线程
            var (avg, min, max) = await Task.Run(() => TestMatrixMultiply(size, IsComplexFftEnabled, TestRunCount)).ConfigureAwait(false);

            results.Add((size, avg, min, max));
        }

        // 打印并输出汇总表头
        var header = "Matrix Size\t||\tAVG [ms]\t||\tMIN [ms]\t||\tMAX [ms]";
        Console.WriteLine();
        Console.WriteLine(header);
        OutputResult("[Fast Fourier]", header, "[Summary]", null);

        // 输出每个结果行：控制台 + 结果面板（数值使用 InvariantCulture 格式化）
        foreach (var r in results)
        {
            var line = $"[{r.size}x{r.size}]\t||\t{r.avg:F10}\t||\t{r.min:F10}\t||\t{r.max:F10}";
            Console.WriteLine(line);
            OutputResult("[Fast Fourier]", line, "[Result]", null);
        }
    }
    /// <summary>
    /// 执行“线性方程组求解”性能测试：按预设尺寸多次运行并统计平均/最小/最大耗时。
    /// </summary>
    /// <summary>
    /// 执行 Execute Test Matrix Multiply1 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task ExecuteTestMatrixMultiply1()
    {
        // 保持无参签名（但改为异步 Task 返回类型），先输出 TestRunCount（使用不依赖区域设置的格式）
        var A = TestRunCount.ToString(CultureInfo.InvariantCulture);
        OutputResult("[TestRunCount]", "Number of tests modified to " + A, "[Info]", null);

        int[] sizes = { 101, 201, 501, 1001, 2001 };
        var results = new List<(int size, double avg, double min, double max)>();

        foreach (var size in sizes)
        {
            var startMsg = $"Running test for matrix size {size}x{size} ... IsComplex={IsComplexFftEnabled}";
            Console.WriteLine(startMsg);
            // 按约定的格式同时输出到结果面板（类型为 Info）
            OutputResult("[Fast Fourier]", startMsg, "[Info]", null);

            // 在后台线程执行耗时的 TestFft，以免阻塞 UI 线程
            var (avg, min, max) = await Task.Run(() => TestLinearSystemSolve(size, IsComplexFftEnabled, TestRunCount)).ConfigureAwait(false);

            results.Add((size, avg, min, max));
        }

        // 打印并输出汇总表头
        var header = "Matrix Size\t||\tAVG [ms]\t||\tMIN [ms]\t||\tMAX [ms]";
        Console.WriteLine();
        Console.WriteLine(header);
        OutputResult("[Fast Fourier]", header, "[Summary]", null);

        // 输出每个结果行：控制台 + 结果面板（数值使用 InvariantCulture 格式化）
        foreach (var r in results)
        {
            var line = $"[{r.size}x{r.size}]\t||\t{r.avg:F10}\t||\t{r.min:F10}\t||\t{r.max:F10}";
            Console.WriteLine(line);
            OutputResult("[Fast Fourier]", line, "[Result]", null);
        }
    }
    /// <summary>
    /// 执行“SVD（奇异值分解）”性能测试：按预设尺寸多次运行并统计平均/最小/最大耗时。
    /// </summary>
    /// <summary>
    /// 执行 Execute Test Matrix Multiply2 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task ExecuteTestMatrixMultiply2()
    {
        // 保持无参签名（但改为异步 Task 返回类型），先输出 TestRunCount（使用不依赖区域设置的格式）
        var A = TestRunCount.ToString(CultureInfo.InvariantCulture);
        OutputResult("[TestRunCount]", "Number of tests modified to " + A, "[Info]", null);

        int[] sizes = { 101, 201, 501, 1001, 2001 };
        var results = new List<(int size, double avg, double min, double max)>();

        foreach (var size in sizes)
        {
            var startMsg = $"Running test for matrix size {size}x{size} ... IsComplex={IsComplexFftEnabled}";
            Console.WriteLine(startMsg);
            // 按约定的格式同时输出到结果面板（类型为 Info）
            OutputResult("[Fast Fourier]", startMsg, "[Info]", null);

            // 在后台线程执行耗时的 TestFft，以免阻塞 UI 线程
            var (avg, min, max) = await Task.Run(() => TestSvd(size, IsComplexFftEnabled, TestRunCount)).ConfigureAwait(false);

            results.Add((size, avg, min, max));
        }

        // 打印并输出汇总表头
        var header = "Matrix Size\t||\tAVG [ms]\t||\tMIN [ms]\t||\tMAX [ms]";
        Console.WriteLine();
        Console.WriteLine(header);
        OutputResult("[Fast Fourier]", header, "[Summary]", null);

        // 输出每个结果行：控制台 + 结果面板（数值使用 InvariantCulture 格式化）
        foreach (var r in results)
        {
            var line = $"[{r.size}x{r.size}]\t||\t{r.avg:F10}\t||\t{r.min:F10}\t||\t{r.max:F10}";
            Console.WriteLine(line);
            OutputResult("[Fast Fourier]", line, "[Result]", null);
        }
    }

    /// <summary>
    /// 执行“特征值分解（Eigen/EVD）”性能测试：按预设尺寸多次运行并统计平均/最小/最大耗时。
    /// </summary>
    /// <summary>
    /// 执行 Execute Test Matrix Multiply3 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task ExecuteTestMatrixMultiply3()
    {
        // 保持无参签名（但改为异步 Task 返回类型），先输出 TestRunCount（使用不依赖区域设置的格式）
        var A = TestRunCount.ToString(CultureInfo.InvariantCulture);
        OutputResult("[TestRunCount]", "Number of tests modified to " + A, "[Info]", null);

        int[] sizes = { 101, 201, 501, 1001, 2001 };
        var results = new List<(int size, double avg, double min, double max)>();

        foreach (var size in sizes)
        {
            var startMsg = $"Running test for matrix size {size}x{size} ... IsComplex={IsComplexFftEnabled}";
            Console.WriteLine(startMsg);
            // 按约定的格式同时输出到结果面板（类型为 Info）
            OutputResult("[Fast Fourier]", startMsg, "[Info]", null);

            // 在后台线程执行耗时的 TestFft，以免阻塞 UI 线程
            var (avg, min, max) = await Task.Run(() => TestEigen(size, IsComplexFftEnabled, TestRunCount)).ConfigureAwait(false);

            results.Add((size, avg, min, max));
        }

        // 打印并输出汇总表头
        var header = "Matrix Size\t||\tAVG [ms]\t||\tMIN [ms]\t||\tMAX [ms]";
        Console.WriteLine();
        Console.WriteLine(header);
        OutputResult("[Fast Fourier]", header, "[Summary]", null);

        // 输出每个结果行：控制台 + 结果面板（数值使用 InvariantCulture 格式化）
        foreach (var r in results)
        {
            var line = $"[{r.size}x{r.size}]\t||\t{r.avg:F10}\t||\t{r.min:F10}\t||\t{r.max:F10}";
            Console.WriteLine(line);
            OutputResult("[Fast Fourier]", line, "[Result]", null);
        }
    }

    /// <summary>
    /// 执行“FFT（快速傅里叶变换）”性能测试：按预设尺寸多次运行并统计平均/最小/最大耗时。
    /// </summary>
    /// <summary>
    /// 执行 Execute Test Matrix Multiply4 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task ExecuteTestMatrixMultiply4()
    {
        // 保持无参签名（但改为异步 Task 返回类型），先输出 TestRunCount（使用不依赖区域设置的格式）
        var A = TestRunCount.ToString(CultureInfo.InvariantCulture);
        OutputResult("[TestRunCount]", "Number of tests modified to " + A, "[Info]", null);

        int[] sizes = { 101, 201, 501, 1001, 2001 };
        var results = new List<(int size, double avg, double min, double max)>();

        foreach (var size in sizes)
        {
            var startMsg = $"Running test for matrix size {size}x{size} ... IsComplex={IsComplexFftEnabled}";
            Console.WriteLine(startMsg);
            // 按约定的格式同时输出到结果面板（类型为 Info）
            OutputResult("[Fast Fourier]", startMsg, "[Info]", null);

            // 在后台线程执行耗时的 TestFft，以免阻塞 UI 线程
            var (avg, min, max) = await Task.Run(() => TestFft(size, IsComplexFftEnabled, TestRunCount)).ConfigureAwait(false);

            results.Add((size, avg, min, max));
        }

        // 打印并输出汇总表头
        var header = "Matrix Size\t||\tAVG [ms]\t||\tMIN [ms]\t||\tMAX [ms]";
        Console.WriteLine();
        Console.WriteLine(header);
        OutputResult("[Fast Fourier]", header, "[Summary]", null);

        // 输出每个结果行：控制台 + 结果面板（数值使用 InvariantCulture 格式化）
        foreach (var r in results)
        {
            var line = $"[{r.size}x{r.size}]\t||\t{r.avg:F10}\t||\t{r.min:F10}\t||\t{r.max:F10}";
            Console.WriteLine(line);
            OutputResult("[Fast Fourier]", line, "[Result]", null);
        }
    }
    /// <summary>
    /// 核心：矩阵乘法性能测试。根据 <paramref name="complex"/> 选择复数/实数矩阵，重复 <paramref name="runs"/> 次统计。
    /// </summary>
    /// <param name="size">矩阵阶数（size×size）。</param>
    /// <param name="complex">是否为复数运算。</param>
    /// <param name="runs">重复次数。</param>
    /// <returns>平均/最小/最大耗时（毫秒）。</returns>
    private (double avg, double min, double max) TestMatrixMultiply(int size, bool complex, int runs)
    {
        double min = double.MaxValue, max = double.MinValue, sum = 0;
        if (complex)
        {
            var matrixA = Matrix<Complex>.Build.Random(size, size);
            var matrixB = Matrix<Complex>.Build.Random(size, size);
            for (var i = 0; i < runs; i++)
            {
                var sw = Stopwatch.StartNew();
                var result = matrixA * matrixB; // 复数矩阵乘法
                sw.Stop();
                var ms = sw.Elapsed.TotalMilliseconds;
                min = Math.Min(min, ms);
                max = Math.Max(max, ms);
                sum += ms;
                Console.WriteLine($"------- Complex Test Run #{i} -------");
            }
        }
        else
        {
            var matrixA = Matrix<double>.Build.Random(size, size);
            var matrixB = Matrix<double>.Build.Random(size, size);
            for (var i = 0; i < runs; i++)
            {
                var sw = Stopwatch.StartNew();
                var result = matrixA * matrixB; // 实数矩阵乘法
                sw.Stop();
                var ms = sw.Elapsed.TotalMilliseconds;
                min = Math.Min(min, ms);
                max = Math.Max(max, ms);
                sum += ms;
                Console.WriteLine($"------- Real Test Run #{i} -------");
            }
        }
        Console.WriteLine($"Time Cost: [Average] {sum / runs} [ms]; [Minimum] {min} [ms]; [Maximum] {max} [ms].");
        return (sum / runs, min, max);
    }

    /// <summary>
    /// 线性方程组求解性能测试：根据 <paramref name="complex"/> 选择复数/实数，重复 <paramref name="runs"/> 次。
    /// </summary>
    private (double avg, double min, double max) TestLinearSystemSolve(int size, bool complex, int runs)
    {
        double min = double.MaxValue, max = double.MinValue, sum = 0;
        if (complex)
        {
            var matrixA = Matrix<Complex>.Build.Random(size, size);
            var vectorB = MathNet.Numerics.LinearAlgebra.Vector<Complex>.Build.Random(size);
            for (var i = 0; i < runs; i++)
            {
                var sw = Stopwatch.StartNew();
                var x = matrixA.Solve(vectorB); // 复数求解
                sw.Stop();
                var ms = sw.Elapsed.TotalMilliseconds;
                min = Math.Min(min, ms);
                max = Math.Max(max, ms);
                sum += ms;
                Console.WriteLine($"------- Complex Linear Solve Run #{i} -------");
            }
        }
        else
        {
            var matrixA = Matrix<double>.Build.Random(size, size);
            var vectorB = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Random(size);
            for (var i = 0; i < runs; i++)
            {
                var sw = Stopwatch.StartNew();
                var x = matrixA.Solve(vectorB); // 实数求解
                sw.Stop();
                var ms = sw.Elapsed.TotalMilliseconds;
                min = Math.Min(min, ms);
                max = Math.Max(max, ms);
                sum += ms;
                Console.WriteLine($"------- Real Linear Solve Run #{i} -------");
            }
        }
        Console.WriteLine($"Linear Solve: [Average] {sum / runs} [ms]; [Minimum] {min} [ms]; [Maximum] {max} [ms].");
        return (sum / runs, min, max);
    }

    /// <summary>
    /// SVD（奇异值分解）性能测试：按 <paramref name="runs"/> 次统计复数/实数的耗时指标。
    /// </summary>
    private (double avg, double min, double max) TestSvd(int size, bool complex, int runs)
    {
        double min = double.MaxValue, max = double.MinValue, sum = 0;
        if (complex)
        {
            var matrixA = Matrix<Complex>.Build.Random(size, size);
            for (var i = 0; i < runs; i++)
            {
                var sw = Stopwatch.StartNew();
                var svd = matrixA.Svd(); // 复数 SVD
                sw.Stop();
                var ms = sw.Elapsed.TotalMilliseconds;
                min = Math.Min(min, ms);
                max = Math.Max(max, ms);
                sum += ms;
                Console.WriteLine($"------- Complex SVD Run #{i} -------");
            }
        }
        else
        {
            var matrixA = Matrix<double>.Build.Random(size, size);
            for (var i = 0; i < runs; i++)
            {
                var sw = Stopwatch.StartNew();
                var svd = matrixA.Svd(); // 实数 SVD
                sw.Stop();
                var ms = sw.Elapsed.TotalMilliseconds;
                min = Math.Min(min, ms);
                max = Math.Max(max, ms);
                sum += ms;
                Console.WriteLine($"------- Real SVD Run #{i} -------");
            }
        }
        Console.WriteLine($"SVD: [Average] {sum / runs} [ms]; [Minimum] {min} [ms]; [Maximum] {max} [ms].");
        return (sum / runs, min, max);
    }

    /// <summary>
    /// 特征值分解（Eigen/EVD）性能测试：按 <paramref name="runs"/> 次统计复数/实数的耗时指标。
    /// </summary>
    private (double avg, double min, double max) TestEigen(int size, bool complex, int runs)
    {
        double min = double.MaxValue, max = double.MinValue, sum = 0;
        if (complex)
        {
            var matrixA = Matrix<Complex>.Build.Random(size, size);
            for (var i = 0; i < runs; i++)
            {
                var sw = Stopwatch.StartNew();
                var evd = matrixA.Evd(); // 复数 EVD
                sw.Stop();
                var ms = sw.Elapsed.TotalMilliseconds;
                min = Math.Min(min, ms);
                max = Math.Max(max, ms);
                sum += ms;
                Console.WriteLine($"------- Complex Eigen Run #{i} -------");
            }
        }
        else
        {
            var matrixA = Matrix<double>.Build.Random(size, size);
            for (var i = 0; i < runs; i++)
            {
                var sw = Stopwatch.StartNew();
                var evd = matrixA.Evd(); // 实数 EVD
                sw.Stop();
                var ms = sw.Elapsed.TotalMilliseconds;
                min = Math.Min(min, ms);
                max = Math.Max(max, ms);
                sum += ms;
                Console.WriteLine($"------- Real Eigen Run #{i} -------");
            }
        }
        Console.WriteLine($"Eigen: [Average] {sum / runs} [ms]; [Minimum] {min} [ms]; [Maximum] {max} [ms].");
        return (sum / runs, min, max);
    }

    /// <summary>
    /// FFT（快速傅里叶变换）性能测试：按 <paramref name="runs"/> 次统计复数/实数的耗时指标。
    /// </summary>
    private (double avg, double min, double max) TestFft(int size, bool complex, int runs)
    {
        double min = double.MaxValue, max = double.MinValue, sum = 0;
        if (complex)
        {
            // 复数样本：实部/虚部均随机
            var samples = new Complex[size];
            var rand = new Random();
            for (var i = 0; i < size; i++)
            {
                samples[i] = new Complex(rand.NextDouble(), rand.NextDouble());
            }
            for (var i = 0; i < runs; i++)
            {
                var data = (Complex[])samples.Clone(); // 防止就地变换污染原数组
                var sw = Stopwatch.StartNew();
                Fourier.Forward(data, FourierOptions.Default);
                sw.Stop();
                var ms = sw.Elapsed.TotalMilliseconds;
                min = Math.Min(min, ms);
                max = Math.Max(max, ms);
                sum += ms;
                Console.WriteLine($"------- Complex FFT Run #{i} -------");
            }
        }
        else
        {
            // 实数样本：仅实部随机，虚部为 0（使用 Complex32 节省内存）
            var samples = new Complex32[size];
            var rand = new Random();
            for (var i = 0; i < size; i++)
            {
                samples[i] = new Complex32((float)rand.NextDouble(), 0f);
            }
            for (var i = 0; i < runs; i++)
            {
                var data = (Complex32[])samples.Clone(); // 防止就地变换污染原数组
                var sw = Stopwatch.StartNew();
                Fourier.Forward(data, FourierOptions.Default);
                sw.Stop();
                var ms = sw.Elapsed.TotalMilliseconds;
                min = Math.Min(min, ms);
                max = Math.Max(max, ms);
                sum += ms;
                Console.WriteLine($"------- Real FFT Run #{i} -------");
            }
        }
        Console.WriteLine($"FFT: [Average] {sum / runs} [ms]; [Minimum] {min} [ms]; [Maximum] {max} [ms].");
        return (sum / runs, min, max);
    }
    /// <summary>当前使用的 FFT Provider 名称。</summary>
    public string FftLibraryInfo
    {
        get
        {
            var provider = MathNet.Numerics.Providers.FourierTransform.FourierTransformControl.Provider;
            return $"{provider.GetType().Assembly.GetName().Name}";
        }
    }
    /// <summary>当前使用的 LAPACK Provider 名称。</summary>
    public string LapackLibraryInfo
    {
        get
        {
            var provider = MathNet.Numerics.Providers.LinearAlgebra.LinearAlgebraControl.Provider;
            return $"{provider.GetType().Assembly.GetName().Name}";
        }
    }
    /// <summary>当前使用的 BLAS Provider 名称。</summary>
    public string BlasLibraryInfo
    {
        get
        {
            var provider = MathNet.Numerics.Providers.LinearAlgebra.LinearAlgebraControl.Provider;
            return $"{provider.GetType().Assembly.GetName().Name}";
        }
    }
    /// <summary>打印所有打开文档的标题与路径（调试辅助）。</summary>
    /// <summary>
    /// 命令：Print Open Documents Command。
    /// </summary>
    public ICommand PrintOpenDocumentsCommand { get; }
    /// <summary>
    /// 调试输出：打印当前已打开文档的标题与路径列表到控制台。
    /// </summary>
    /// <remarks>
    /// - 若 <c>OpenDocuments</c> 为空，则输出提示并返回；
    /// - 否则逐项输出“标题/路径”，其中空标题与空路径会用占位文本显示。
    /// </remarks>
    /// <summary>
    /// 执行 Print Open Documents 操作。
    /// </summary>
    public void PrintOpenDocuments()
    {
        if (OpenDocuments.Count == 0)
        {
            Console.WriteLine("[PrintOpenDocuments] 当前没有打开的文档。");
            return;
        }

        Console.WriteLine("[PrintOpenDocuments] 当前打开的文档列表：");
        foreach (var doc in OpenDocuments)
        {
            var title = doc.Title ?? "(无标题)";
            var path = doc.Document?.Path ?? "(无路径)";
            Console.WriteLine($"- 标题: {title}，路径: {path}");
        }
    }

    /// <summary>打开“查找”面板：若选中有文本则带入搜索框。</summary>
    /// <remarks>若当前没有编辑器实例，则将提示信息写入结果区。</remarks>
    private void OpenSearchPanel()
    {
        // 1) 拿到当前编辑器
        var editor = CurrentOpenDocument?.EditorControl as RoslynCodeEditor;
        if (editor is null)
        {
            // 可选：把错误写到结果区，避免静默失败
            OutputResult("[Search]", "没有可用的编辑器实例（可能未打开文档）。", "Info", null);
            return;
        }

        // 2) UI 线程打开面板，并将选中文本作为默认搜索词
        Dispatcher.UIThread.Post(() =>
        {
            var panel = SearchPanel.Install(editor);
            var selected = editor.SelectedText;
            if (!string.IsNullOrWhiteSpace(selected))
                panel.SearchPattern = selected;

            panel.Open();
            editor.Focus();
        }, DispatcherPriority.Background);
    }    /// <summary>打开“替换”面板：自动安装并切换到替换模式。</summary>
    private void OpenReplacePanel()
    {
        var editor = CurrentOpenDocument?.EditorControl as RoslynCodeEditor;
        if (editor is null) return;

        Dispatcher.UIThread.Post(() =>
        {
            var panel = SearchPanel.Install(editor);
            var selected = editor.SelectedText;
            if (!string.IsNullOrWhiteSpace(selected))
                panel.SearchPattern = selected;

            panel.IsReplaceMode = true;
            panel.Open();
        }, DispatcherPriority.Background);
    }

    // ===== 输出框/编辑器字体属性 =====

    /// <summary>输出区字体名称（字符串形式）。</summary>
    public string EditorFontFamily1
    {
        get => _editorFontFamily1;
        set
        {
            if (_editorFontFamily1 != value)
            {
                _editorFontFamily1 = value;
                OnPropertyChanged(nameof(_editorFontFamily1));
                CurrentOpenDocument?.OnEditorFontFamilyChanged1(_editorFontFamily1);
                Console.WriteLine($"[MainViewModel] EditorFontFamily1输出框字体切换为: {_editorFontFamily1}");
                SaveShortcuts();
                // >>> 新增同步
                GlobalResults.OnEditorFontFamilyChanged1(_editorFontFamily1);
                GlobalResults.EditorFontFamily1 = _editorFontFamily1;
            }
        }
    }
    private string _editorFontFamily1;

    /// <summary>输出区选中的字体对象（用于下拉列表双向绑定）。</summary>
	private FontFamily? _selectedFontFamilyObject1;
    public FontFamily? SelectedFontFamilyObject1
    {
        get => _selectedFontFamilyObject1;
        set
        {
            if (_selectedFontFamilyObject1 != value)
            {
                _selectedFontFamilyObject1 = value;
                if (value != null)
                    EditorFontFamily1 = value.Name;
                OnPropertyChanged(nameof(SelectedFontFamilyObject1));
                SaveShortcuts();

                // 输出到结果面板，保持与之前相同的 OutputResult 格式
                if (value != null)
                {
                    OutputResult("[EditorFontFamily1]", "The output font has been changed to " + value.Name, "[Info]", null);
                }
            }
        }
    }
    private int _outputFontSize = 50;
    /// <summary>输出区字号</summary>
    /// <summary>输出区字号</summary>
    public int OutputFontSize
    {
        get => _outputFontSize;
        set
        {
            if (_outputFontSize != value)
            {
                Console.WriteLine($"[MainViewModel] MV之前输出器字号: {value}");
                _outputFontSize = value;
                OnPropertyChanged(nameof(OutputFontSize));
                Console.WriteLine($"[MainViewModel] MV当前输出器字号: {value}");
                SaveShortcuts();

                // 输出到结果面板，保持与之前相同的 OutputResult 格式
                var msg = "The output font size has been changed to " + value.ToString(CultureInfo.InvariantCulture);
                OutputResult("[OutputFontSize]", msg, "[Info]", null);
                // >>> 新增同步
                GlobalResults.FontSize = _outputFontSize;
            }
        }
    }
    /// <summary>输出区可选字号列表。</summary>
    //public IReadOnlyList<int> OutputFontSizes { get; } = new int[]
    //{
    //    8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 32, 36, 40, 48, 56, 64, 72, 80, 88, 96
    //};

    /// <summary>编辑器可选字号列表。</summary>
    //public IReadOnlyList<int> EditorFontSizes { get; } = new int[]
    //{
    //    8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 32, 36, 40, 48, 56, 64, 72, 80, 88, 96
    //};

    /// <summary>系统字体列表（用于下拉显示）。</summary>
    /// <summary>
    /// System Font Families。
    /// </summary>
    public ObservableCollection<FontFamily> SystemFontFamilies { get; } =
        new ObservableCollection<FontFamily>(
            FontManager.Current.SystemFonts.OrderBy(f => f.Name)
        );
    private FontFamily? _selectedFontFamilyObject;
    /// <summary>编辑器选中的字体对象（与 <see cref="EditorFontFamily"/> 同步）。</summary>
    public FontFamily? SelectedFontFamilyObject
    {
        get => _selectedFontFamilyObject;
        set
        {
            if (_selectedFontFamilyObject != value)
            {
                _selectedFontFamilyObject = value;
                if (value != null)
                    EditorFontFamily = value.Name;
                OnPropertyChanged(nameof(SelectedFontFamilyObject));
                SaveShortcuts();

                // 输出到结果面板，保持与之前相同的 OutputResult 格式
                if (value != null)
                {
                    OutputResult("[EditorFontFamily]", "The editor font has been changed to " + value.Name, "[Info]", null);
                }
            }
        }
    }

    /// <summary>
    /// 获取当前操作系统平台（Windows / Linux / macOS）。
    /// </summary>
    /// <summary>
    /// 执行 Get Current Os Platform 操作。
    /// </summary>
    public static string GetCurrentOsPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "Windows";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "Linux";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "macOS";
        return "Unknown";
    }

    /// <summary>当前操作系统平台字符串。</summary>
    /// <summary>
    /// 执行 Get Current Os Platform 操作。
    /// </summary>
    public string CurrentOsPlatform => GetCurrentOsPlatform();

    /// <summary>编辑器字体（字符串，逗号分隔优先级）。</summary>
    public string EditorFontFamily
    {
        get => _editorFontFamily;
        set
        {
            if (_editorFontFamily != value)
            {
                _editorFontFamily = value;
                OnPropertyChanged(nameof(EditorFontFamily));
                SaveShortcuts();

                // 同步通知所有打开的文档更新字体
                foreach (var doc in OpenDocuments)
                {
                    doc.OnEditorFontFamilyChanged(EditorFontFamily);
                    Console.WriteLine($"[MainViewModel] 通知文档: {doc.Title} 字体切换为: {value}");
                }
            }
        }
    }
    private string _editorFontFamily;

    // ========== 菜单/窗口/文件 相关命令属性（声明） ==========
    /// <summary>
    /// 命令：Exit Application Command。
    /// </summary>
    public ICommand ExitApplicationCommand { get; }
    /// <summary>
    /// 命令：Export Shortcuts Command。
    /// </summary>
    public ICommand ExportShortcutsCommand { get; }
    /// <summary>
    /// 命令：Import Shortcuts Command。
    /// </summary>
    public ICommand ImportShortcutsCommand { get; }
    /// <summary>
    /// 命令：Open Initial Folder Command。
    /// </summary>
    public IDelegateCommand OpenInitialFolderCommand { get; }

    /// <summary>
    /// 打开初始文件夹（使用 Settings/InitialFolderPath），并刷新文档树。
    /// </summary>
    /// <summary>
    /// 执行 Open Initial Folder 操作。
    /// </summary>
    public void OpenInitialFolder()
    {
        var folderPath = InitialFolderPath;
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            Console.WriteLine($"[OpenInitialFolder] ·    Ч 򲻴   : {folderPath}");
            return;
        }
        //Settings.DocumentPath = folderPath;
        //DocumentRoot = CreateDocumentRoot();
        //OnPropertyChanged(nameof(DocumentRoot));
        ////OnPropertyChanged(nameof(DocumentRoot.Children));
        //// 如有需要可加：
        //OnPropertyChanged(nameof(DocumentRoot.VisibleChildren));
        Console.WriteLine($"[OpenInitialFolder] 路径无效或不存在: {folderPath}");
        Console.WriteLine("FPATH" + InitialFolderPath);
        Settings.DocumentPath = InitialFolderPath;
        DocumentRoot = DocumentViewModel.CreateRoot(InitialFolderPath);
        DocumentRoot.EnsureMainViewModel(this);
        OnPropertyChanged(nameof(DocumentRoot));
        // 如有需要可加：
        OnPropertyChanged(nameof(DocumentRoot.VisibleChildren));
    }
    /// <summary>
    /// 应用主题的命令（可带字符串参数指定主题名/标识）。
    /// </summary>
    /// <summary>
    /// 命令：Apply Theme Command。
    /// </summary>
    public ICommand ApplyThemeCommand { get; }

    /// <summary>
    /// 取消注释选中代码的命令。
    /// </summary>
    /// <summary>
    /// 命令：Uncomment Selection Command。
    /// </summary>
    public IDelegateCommand UncommentSelectionCommand { get; }

    /// <summary>
    /// 注释选中代码的命令。
    /// </summary>
    /// <summary>
    /// 命令：Comment Selection Command。
    /// </summary>
    public IDelegateCommand CommentSelectionCommand { get; }

    /// <summary>
    /// 对当前文档执行代码格式化的命令。
    /// </summary>
    /// <summary>
    /// 命令：Format Current Document Command。
    /// </summary>
    public IDelegateCommand FormatCurrentDocumentCommand { get; }


    /// <summary>格式化当前文档（委托给 OpenDocumentViewModel 的命令）。</summary>
    /// <summary>
    /// 执行 Format Current Document 操作。
    /// </summary>
    public void FormatCurrentDocument()
    {
        if (CurrentOpenDocument?.FormatDocumentCommand.CanExecute() == true)
        {
            CurrentOpenDocument.FormatDocumentCommand.Execute();
            Console.WriteLine("[MainViewModel] FormatDocumentCommand 已执行");
        }
        else
        {
            Console.WriteLine("[MainViewModel] FormatDocumentCommand 不可执行");
        }
    }

    /// <summary>取消注释当前选择区域。</summary>
    /// <summary>
    /// 执行 Uncomment Selection 操作。
    /// </summary>
    public void UncommentSelection()
    {
        if (CurrentOpenDocument?.UncommentSelectionCommand.CanExecute() == true)
        {
            CurrentOpenDocument.UncommentSelectionCommand.Execute();
            Console.WriteLine("[MainViewModel] CommentSelectionCommand 已执行");
        }
        else
        {
            Console.WriteLine("[MainViewModel] CommentSelectionCommand 不可执行");
        }
    }

    /// <summary>注释当前选择区域。</summary>
    /// <summary>
    /// 执行 Comment Selection 操作。
    /// </summary>
    public void CommentSelection()
    {
        if (CurrentOpenDocument?.CommentSelectionCommand.CanExecute() == true)
        {
            CurrentOpenDocument.CommentSelectionCommand.Execute();
            Console.WriteLine("[MainViewModel] CommentSelectionCommand 已执行");
        }
        else
        {
            Console.WriteLine("[MainViewModel] CommentSelectionCommand 不可执行");
        }
    }

    /// <summary>加载快捷键配置（会刷新相关绑定）。</summary>
    /// <summary>
    /// 命令：Load Shortcuts Command。
    /// </summary>
    public ICommand LoadShortcutsCommand { get; }

    /// <summary>“更多设置”侧边区域是否展开。</summary>
    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    /// <summary>开发者模式控制台命令。</summary>
    /// <summary>
    /// 命令：Open Console Command。
    /// </summary>
    public ICommand OpenConsoleCommand { get; }
    /// <summary>打开并初始化开发者控制台（Windows 下分配控制台并重定向 stdout/stderr）。</summary>
    /// <summary>
    /// 执行 Execute Open Console 操作。
    /// </summary>
    public void ExecuteOpenConsole()
    {
        // 跨平台控制台初始化
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            AllocConsole();
            // 关键：重定向标准输出
            var stdOut = Console.OpenStandardOutput();
            var writer = new StreamWriter(stdOut) { AutoFlush = true };
            Console.SetOut(writer);
            Console.SetError(writer);
            // 设置控制台代码页为 65001（UTF-8）
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }
        Console.WriteLine("控制台已启动");
        // 示例：输出启动信息（实际可扩展为交互式控制台）
        Console.WriteLine("RoslynPad Console Activated");
        Console.WriteLine($"System Time: {DateTime.Now}");
        Console.WriteLine("中文测试：你好，世界！");
    }

    // Windows平台API声明（置于类外部）
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();

    /// <summary>
    /// 设置初始文档目录（用户直接输入路径，重启后生效的版本）。
    /// </summary>
    /// <param name="folderPath">用户输入的文件夹路径。</param>
    /// <summary>
    /// 执行 Open Folder By Path1 操作。
    /// </summary>
    public void OpenFolderByPath1(string? folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            Console.WriteLine($"·    Ч 򲻴   : {folderPath}");
            return;
        }

        Settings.DocumentPath = folderPath;
        DocumentRoot = CreateDocumentRoot();
        OnPropertyChanged(nameof(DocumentRoot));
        OnPropertyChanged(nameof(DocumentRoot.Children));
        Console.WriteLine($" ѳ ʼ   ļ     ·  : {folderPath}");
        // 以上：仅变更 Settings.DocumentPath 与重建文档树，不做额外 I/O
    }
    /// <summary>
    /// 通过路径设置初始文档目录的命令（立即重建文档树）。
    /// </summary>
    /// <remarks>
    /// 绑定到输入框/按钮等处，参数为用户输入的文件夹路径字符串。
    /// </remarks>
    /// <summary>
    /// 命令：Open Folder By Path1 Command。
    /// </summary>
    public IDelegateCommand<string> OpenFolderByPath1Command => _commands.Create<string>(OpenFolderByPath1);

    /// <summary>
    /// “初始/当前文件夹路径”的后备字段。
    /// </summary>
    private string? _folderPathInput;

    /// <summary>
    /// 用户界面显示与编辑的文件夹路径。
    /// </summary>
    /// <remarks>
    /// 设置时通知 <see cref="FolderPathInput"/> 与派生显示字段
    /// <see cref="FolderPathInputWithCount"/>
    /// </remarks>
    public string? FolderPathInput
    {
        get => _folderPathInput;
        set
        {
            if (_folderPathInput != value)
            {
                _folderPathInput = value;
                OnPropertyChanged(nameof(FolderPathInput));
                OnPropertyChanged(nameof(FolderPathInputWithCount));
            }
        }
    }

    /// <summary>
    /// 初始文件计数：文档根目录的直接子项数量（文件/文件夹）。
    /// </summary>
    /// <summary>
    /// Initial File Count。
    /// </summary>
    public int InitialFileCount => DocumentRoot?.Children?.Count ?? 0;


    /// <summary>
    /// 组合显示“路径 + 文件数”（示例："C:\Work (文件数: 12)"）。
    /// </summary>
    /// <summary>
    /// Folder Path Input With Count。
    /// </summary>
    public string FolderPathInputWithCount => $"{FolderPathInput ?? ""} (文件数: {InitialFileCount})";

    /// <summary>
    /// 命令：Copy All Results Command。
    /// </summary>
    public IDelegateCommand CopyAllResultsCommand { get; set; }
    /// <summary>
    /// 命令：Clear Results Command。
    /// </summary>
    public IDelegateCommand ClearResultsCommand { get; set; }

    /// <summary>
    /// 复制当前文档“结果面板”的全部内容到剪贴板（若命令可执行则转发到当前文档的 CopyAllResultsCommand）。
    /// </summary>
    /// <summary>
    /// 执行 Copy All Results 操作。
    /// </summary>
    public void CopyAllResults()
    {
        GlobalResults.CopyAllResultsCommand.Execute();

    }

    /// <summary>
    /// 清空当前文档“结果面板”的内容（若命令可执行则转发到当前文档的 ClearResultsCommand）。
    /// </summary>
    /// <summary>
    /// 执行 Clear Results 操作。
    /// </summary>
    public void ClearResults()
    {
        GlobalResults.ClearResultsCommand.Execute();
        if (CurrentOpenDocument?.ClearResultsCommand.CanExecute() == true)
        {
            CurrentOpenDocument.ClearResultsCommand.Execute();
            Console.WriteLine("[MainViewModel] ClearResultsCommand 已执行");
        }
        else
        {
            Console.WriteLine("[MainViewModel] ClearResultsCommand 不可执行");
        }
    }

    /// <summary>
    /// 保存当前打开的文档（若命令可执行则转发到当前文档的 SaveCommand）。
    /// </summary>
    /// <summary>
    /// 执行 Save Current Document 操作。
    /// </summary>
    public void SaveCurrentDocument()
    {
        if (CurrentOpenDocument?.SaveCommand.CanExecute() == true)
        {
            CurrentOpenDocument.SaveCommand.Execute();
            Console.WriteLine("[MainViewModel] SaveCommand 已执行，文档: " + CurrentOpenDocument.Title);
        }
        else
        {
            Console.WriteLine("[MainViewModel] SaveCommand 不可执行");
        }
    }

    /// <summary>
    /// 命令：Save Current Document Command。
    /// </summary>
    public IDelegateCommand SaveCurrentDocumentCommand { get; set; }

    private DocumentViewModel? _selectedDocument;

    /// <summary>文档树中当前选中的节点；变更时驱动预览与相关派生状态刷新。</summary>
    public DocumentViewModel? SelectedDocument
    {
        get => _selectedDocument;
        set
        {
            if (ReferenceEquals(_selectedDocument, value)) return;

            // 规范化与校验
            if (value is not null)
            {
                if (string.IsNullOrEmpty(value.Name))
                {
                    value.Rename(Path.GetFileName(value.Path));
                }
                if (string.IsNullOrEmpty(value.Path))
                {
                    throw new InvalidOperationException("DocumentViewModel.Path 未初始化");
                }
            }

            _selectedDocument = value;

            Console.WriteLine($"[MainViewModel] SelectedDocument 赋值: {value?.Name}, Path={value?.Path}");

            // 通知绑定
            OnPropertyChanged(); // nameof(SelectedDocument)
            OnPropertyChanged(nameof(IsPreviewVisible));

            // 清空/更新预览
            if (value is null || value.IsFolder)
            {
                // 文件夹或无选中项：清空预览文本
                _ = LoadPreviewAsync(null);
                return;
            }

            // 1) 文本预览（异步，带竞态/取消保护）
            _ = LoadPreviewAsync(value);

            // 2) 预览页签（在 Dock 中以临时标签打开/复用）
            ShowPreviewTab(value);
        }
    }

    /// <summary>预览文本（仅截取文件前若干 KB；二进制文件将提示不可预览）。</summary>
    private string? _previewText;
    public string? PreviewText
    {
        get => _previewText;
        private set
        {
            if (!Equals(_previewText, value))
            {
                _previewText = value;
                OnPropertyChanged(nameof(PreviewText));
            }
        }
    }
    /// <summary>是否显示预览区（选中项存在且为文件）。</summary>
    /// <summary>
    /// Is Preview Visible。
    /// </summary>
    public bool IsPreviewVisible => SelectedDocument != null && !(SelectedDocument?.IsFolder ?? true);
    // _previewCts 用于取消上一轮的异步预览任务；
    private CancellationTokenSource? _previewCts;
    // _previewSeq 为自增序号，确保仅最后一次请求的结果生效（避免竞态覆盖）。
    private long _previewSeq;
    /// <summary>异步加载预览文本：带取消与序号保护，避免竞态覆盖。</summary>
    // 替换整个方法
    private async Task LoadPreviewAsync(DocumentViewModel? doc)
    {
        // 为本次请求打序号，仅允许最新结果生效
        var requestId = Interlocked.Increment(ref _previewSeq);

        // 先原子交换并异步取消/释放上一次的 CTS（修 VSTHRD103）
        var prev = Interlocked.Exchange(ref _previewCts, null);
        if (prev != null)
        {
            try { await prev.CancelAsync().ConfigureAwait(false); }
            catch { /* ignore */ }
            finally { prev.Dispose(); }
        }

        // 为本次请求创建新的 CTS
        var cts = new CancellationTokenSource();
        _previewCts = cts;
        var ct = cts.Token;

        try
        {
            OnPropertyChanged(nameof(IsPreviewVisible));

            if (doc is null || doc.IsFolder)
            {
                ApplyPreview(null, requestId);
                return;
            }

            var path = doc.Path;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                ApplyPreview(null, requestId);
                return;
            }

            const int maxBytes = 8192; // 8KB 预览
            byte[] buffer;

            using (var fs = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete,
                bufferSize: 4096,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                var len = (int)Math.Min(maxBytes, fs.Length);
                buffer = new byte[len];
                var read = await fs.ReadAsync(buffer.AsMemory(0, len), ct).ConfigureAwait(false); // CA1835
                if (read < buffer.Length)
                    Array.Resize(ref buffer, read);
            }

            ct.ThrowIfCancellationRequested();

            // 粗略二进制判定
            ReadOnlySpan<byte> span = buffer.AsSpan();
            var control = 0;
            for (var i = 0; i < span.Length; i++)
            {
                var b = span[i];
                if (b == 0) { control = int.MaxValue; break; }                 // NUL
                if (b < 0x09 || b > 0x0D && b < 0x20) control++;            // 其他控制字符
            }
            if (control > span.Length / 8)
            {
                ApplyPreview("二进制文件，无法预览。", requestId);
                return;
            }

            // UTF-8 优先，失败用系统默认编码兜底
            string text;
            try
            {
                text = System.Text.Encoding.UTF8.GetString(buffer);
            }
            catch
            {
                text = System.Text.Encoding.Default.GetString(buffer);
            }

            const int maxChars = 4000;
            if (text.Length > maxChars)
                text = string.Concat(text.AsSpan(0, maxChars), "\n…");        // CA1845

            ApplyPreview(text, requestId);
        }
        catch (OperationCanceledException)
        {
            // 被取消：忽略
        }
        catch (Exception ex)
        {
            ApplyPreview($"预览失败：{ex.Message}", requestId);
        }
        // 仅当请求仍是“最新”时才应用结果
        void ApplyPreview(string? text, long rid)
        {
            if (rid != Volatile.Read(ref _previewSeq)) return;
            PreviewText = text;
            OnPropertyChanged(nameof(IsPreviewVisible));
        }
    }

    /// <summary>
    /// 在当前打开文档的“结果面板”中输出 VEMS 的简介信息。
    /// </summary>
    /// <remarks>
    /// - 若 <c>CurrentOpenDocument</c> 为空，则不输出任何结果；
    /// - 输出包含两条记录：缩写释义与团队信息；
    /// - 仅追加到结果面板，不触发其他逻辑。
    /// </remarks>
    private void AboutVEMS()
    {
        Console.WriteLine("About VEMS 方法已被调用");

        OutputResult("[About VEMS]", "VEMS stands for Virtual ElectroMagnetic Solutions", "[About VEMS]", null);
        OutputResult("[About VEMS]", "VEMS is developed by the Computational Photonics group, KLAMOS, CAS", "[About VEMS]", null);


    }

    /// <summary>显示联系信息到当前文档的结果面板。</summary>
    /// <remarks>仅在 <c>CurrentOpenDocument</c> 非空时写入结果。</remarks>
    private void Contact()
    {
        Console.WriteLine("Contact 方法已被调用");

        OutputResult("[Contact]", "Contact zhangsite@ciomp.ac.cn to learn more", "[Contact]", null);

    }
    /// <summary>显示许可信息到当前文档的结果面板。</summary>
    private void License()
    {

        Console.WriteLine("License 方法已被调用");

        OutputResult("[License]", "Software license is valid for education only", "[License]", null);
        OutputResult("[License]", "License will expire on 2025/9/30 0:00:00", "[License]", null);

    }

    /// <summary>显示帮助信息到当前文档的结果面板。</summary>
    private void help()
    {

        Console.WriteLine("help 方法已被调用");

        OutputResult("[Help]", "Feel free to contact zhangsite@ciomp.ac.cn", "[Help]", null);

    }

    /// <summary>打开外部网站（VEMS 软件页面）。</summary>
    /// <remarks>失败时将异常信息写入结果面板。</remarks>
    private void OpenBaidu()
    {
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
            OutputResult("[OpenBaidu]", ex.Message, "[OpenBaidu]", null);

        }
    }
    /// <summary>
    /// 打开外部链接（Baidu/VEMS 页面等）的命令。
    /// </summary>
    /// <summary>
    /// 命令：Open Baidu Command。
    /// </summary>
    public ICommand OpenBaiduCommand { get; }

    /// <summary>
    /// 打开“帮助（Help）”信息的命令。
    /// </summary>
    /// <summary>
    /// 命令：Openhelp Command。
    /// </summary>
    public IDelegateCommand OpenhelpCommand { get; }

    /// <summary>
    /// 打开“查找（Search）”面板的命令。
    /// </summary>
    /// <summary>
    /// 命令：Open Search Panel Command。
    /// </summary>
    public IDelegateCommand OpenSearchPanelCommand { get; }

    /// <summary>
    /// 打开“替换（Replace）”面板的命令。
    /// </summary>
    /// <summary>
    /// 命令：Open Replace Panel Command。
    /// </summary>
    public IDelegateCommand OpenReplacePanelCommand { get; }

    /// <summary>
    /// 打开“许可证（License）”信息的命令。
    /// </summary>
    /// <summary>
    /// 命令：Open License Command。
    /// </summary>
    public IDelegateCommand OpenLicenseCommand { get; }

    /// <summary>
    /// 打开“About VEMS”信息的命令。
    /// </summary>
    /// <summary>
    /// 命令：Open About VEMSCommand。
    /// </summary>
    public IDelegateCommand OpenAboutVEMSCommand { get; }

    /// <summary>
    /// 打开“联系（Contact）”信息的命令。
    /// </summary>
    /// <summary>
    /// 命令：Open Contact Command。
    /// </summary>
    public IDelegateCommand OpenContactCommand { get; }

    /// <summary>
    /// 运行当前打开文档的命令（无参）。
    /// </summary>
    /// <summary>
    /// 命令：Run Command。
    /// </summary>
    public ICommand RunCommand { get; }

    /// <summary>
    /// 执行“运行（Run）”动作：将调用当前文档的 <c>RunCommand</c>（若可执行）。
    /// </summary>
    /// <summary>
    /// 执行 Run 操作。
    /// </summary>
    public void Run()
    {
        if (CurrentOpenDocument != null && CurrentOpenDocument.RunCommand.CanExecute())
        {
            CurrentOpenDocument.RunCommand.Execute();
        }
    }

    /// <summary>
    /// 调试当前文档的命令（无参）。
    /// </summary>
    /// <summary>
    /// 命令：Debug Current Document Command。
    /// </summary>
    public IDelegateCommand DebugCurrentDocumentCommand { get; }

    /// <summary>
    /// 执行“调试（Debug）”动作：将调用当前文档的 <c>DebugCommand</c>（若可执行）。
    /// </summary>
    /// <summary>
    /// 执行 Debug Current Document 操作。
    /// </summary>
    public void DebugCurrentDocument()
    {
        if (CurrentOpenDocument != null && CurrentOpenDocument.DebugCommand.CanExecute())
        {
            CurrentOpenDocument.DebugCommand.Execute();
        }
    }

    /// <summary>
    /// 关闭所有已打开文档的命令（逐个触发关闭逻辑）。
    /// </summary>
    /// <summary>
    /// 命令：Close All Documents Command。
    /// </summary>
    public ICommand CloseAllDocumentsCommand { get; }

    //public IDelegateCommand SaveAllDocumentsCommand { get; }

    /// <summary>
    /// 异步保存所有已打开文档。
    /// </summary>
    /// <param name="promptSave">
    /// 是否在保存前提示确认（由文档自身实现决定具体行为）。
    /// </param>
    /// <summary>
    /// 执行 Save All Documents Async 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task SaveAllDocumentsAsync(bool promptSave = false)
    {
        foreach (var doc in OpenDocuments)
        {
            await doc.SaveAsync(promptSave).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// “保存全部”命令（调用 <see cref="SaveAllDocumentsAsync(bool)"/>）。
    /// </summary>
    /// <summary>
    /// 命令：Create Async。
    /// </summary>
    public IDelegateCommand SaveAllDocumentsCommand => _commands.CreateAsync(() => SaveAllDocumentsAsync());

    /// <summary>
    /// “另存为”命令（无参）。
    /// </summary>
    /// <summary>
    /// 命令：Save As Command1。
    /// </summary>
    public IDelegateCommand SaveAsCommand1 { get; }

    /// <summary>
    /// 执行“另存为”：转发到当前文档的 <c>SaveAsCommand</c>（若可执行）。
    /// </summary>
    /// <summary>
    /// 执行 Save As Current Document 操作。
    /// </summary>
    public void SaveAsCurrentDocument()
    {
        //if (CurrentOpenDocument != null)
        //{
        //    var result = await CurrentOpenDocument.SaveAsAsync().ConfigureAwait(true);
        //    Console.WriteLine($"SaveAsCurrentDocument 执行结果: {result}");
        //}
        //else
        //{
        //    Console.WriteLine("SaveAsCurrentDocument 失败：没有打开的文档");
        //}
        if (CurrentOpenDocument != null && CurrentOpenDocument.SaveCommand.CanExecute())
        {
            CurrentOpenDocument.SaveAsCommand.Execute();
            Console.WriteLine("SaveAsCurrentDocument 失败：没有打开的文档");
        }
    }

    /// <summary>
    /// “运行当前文档”命令（异步包装）。
    /// </summary>
    /// <summary>
    /// 命令：Run Command1。
    /// </summary>
    public IDelegateCommand RunCommand1 { get; }

    /// <summary>
    /// 异步执行“运行当前文档”：在切换上下文后调用当前文档的 <c>RunCommand</c>（若可执行）。
    /// </summary>
    /// <summary>
    /// 执行 Run Current Document 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task RunCurrentDocument()
    {
        await Task.Yield();
        if (CurrentOpenDocument != null && CurrentOpenDocument.RunCommand.CanExecute())
        {
            CurrentOpenDocument.RunCommand.Execute();
        }
    }

    /// <summary>
    /// 关闭当前文件夹的命令（通常会弹出选择新目录的流程）。
    /// </summary>
    /// <summary>
    /// 命令：Close Folder Command1。
    /// </summary>
    public IDelegateCommand CloseFolderCommand1 { get; }


    // 命令执行方法
    private void ExecuteSwitchLanguage(string? culture)
    {
        if (culture == null)
        {
            // 处理 null 值：使用默认语言或记录日志
            culture = "en-US";
            Console.WriteLine("[SwitchLanguage] 输入为 null，默认切换到 en-US");
        }
        else
        {
            Console.WriteLine($"[SwitchLanguage] 切换到语言: {culture}");
        }

        InitialLanguage = culture;
        LocalizationService.Default.SelectCulture(culture);
        LocalizationManager.Instance.LoadLanguage(new CultureInfo(culture));
        //LocalizationService.Default.SelectCulture(culture);
        OnPropertyChanged(nameof(Localized));
        // 新增：显式通知所有包装属性
        RaiseLocalizationWrappers();
        // 打印部分关键本地化内容，确认切换是否生效
        Console.WriteLine($"[SwitchLanguage] 当前语言: {culture}");
        Console.WriteLine($"[SwitchLanguage] 菜单项示例: {Localized["MainWindow.MenuItem1"]}");
    }
    /// <summary>清理 NuGet 还原缓存目录（roslynpad/restore）。</summary>
    /// <remarks>使用 <c>IOUtilities.PerformIO</c> 捕获并忽略 I/O 异常，避免影响 UI。</remarks>
    private void ClearRestoreCache()
    {
        IOUtilities.PerformIO(() => Directory.Delete(Path.Combine(Path.GetTempPath(), "roslynpad", "restore"), recursive: true));
    }

    /// <summary>监听系统主题变化，并在变化时调用回调。</summary>
    /// <param name="onChange">系统主题发生变化时执行的回调。</param>
    /// <remarks>由派生平台实现（例如 Windows、Linux、macOS）。</remarks>
    /// <summary>
    /// 执行 Listen To System Theme Changes 操作。
    /// </summary>
    protected abstract void ListenToSystemThemeChanges(Action onChange);

    /// <summary>初始化应用（只执行一次）。</summary>
    /// <remarks>调用内部 <see cref="InitializeInternal"/>，并处理异常与最终清理。</remarks>
    /// <summary>
    /// 执行 Initialize 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task Initialize()
    {
        if (IsInitialized) return;

        try
        {
            await InitializeInternal().ConfigureAwait(true);

            IsInitialized = true;
        }
        catch (Exception e)
        {
            _telemetryProvider.ReportError(e);
        }
    }

    /// <summary>
    /// 组合（MEF/DI）使用到的程序集集合。
    /// </summary>
    /// <remarks>
    /// 默认仅返回当前程序集（<see cref="MainViewModel"/> 所在程序集）。
    /// 若需要扩展编辑器功能或导入其他部件，可在派生类中重写并追加更多程序集。
    /// </remarks>
    /// <summary>
    /// 执行 typeof 操作。
    /// </summary>
    protected virtual ImmutableArray<Assembly> CompositionAssemblies => [typeof(MainViewModel).Assembly];

    /// <summary>内部初始化逻辑。</summary>
    /// <remarks>包含 RoslynHost 初始化、主题/语言、文档树与监听、命令行/自动保存文档恢复等。</remarks>
    private async Task InitializeInternal()
    {
        RoslynHost = await Task.Run(() => new RoslynHost(CompositionAssemblies,
            RoslynHostReferences.NamespaceDefault.With(imports: ["RoslynPad.Runtime"]),
            disabledDiagnostics: ["CS1701", "CS1702", "CS7011", "CS8097"],
            analyzerConfigFiles: [_editorConfigPath]))
            .ConfigureAwait(true);

        OpenDocumentFromCommandLine();
        //await OpenAutoSavedDocuments().ConfigureAwait(true);

        if (HasCachedUpdate())
        {
            HasUpdate = true;
        }
        else
        {
            var task = Task.Run(CheckForUpdates);
        }
    }

    /// <summary>从命令行参数尝试打开指定文件（args[1]）。</summary>
    /// <remarks>仅当文件存在时才会调用 <c>OpenDocument</c>。</remarks>
    private void OpenDocumentFromCommandLine()
    {
        var args = Environment.GetCommandLineArgs();

        if (args.Length > 1)
        {
            var filePath = args[1];

            if (File.Exists(filePath))
            {
                var document = DocumentViewModel.FromPath(filePath);
                OpenDocument(document);
            }
        }
    }

    /// <summary>尝试打开自动保存的文档。</summary>
    /// <remarks>从文档根路径加载自动保存文档集合，若为空则创建新文档。</remarks>
    private async Task OpenAutoSavedDocuments()
    {
        var documents = await Task.Run(() => LoadAutoSavedDocuments(DocumentRoot.Path)).ConfigureAwait(true);

        OpenDocuments.AddRange(documents);

        if (OpenDocuments.Count == 0)
        {
            CreateNewDocument();
        }
        else
        {
            CurrentOpenDocument = OpenDocuments[0];
        }
    }

    private IEnumerable<OpenDocumentViewModel> LoadAutoSavedDocuments(string root)
    {
        return IOUtilities.EnumerateFilesRecursive(root, $"*{DocumentViewModel.AutoSaveSuffix}.*")
            .Select(DocumentViewModel.FromPath)
            .Where(IsRelevantDocument)
            .Select(GetOpenDocumentViewModel);
    }
    /// <summary>
    /// 自定义GetOpenDocumentViewModel
    /// </summary>
    /// <param name="documentViewModel"></param>
    /// <returns></returns>
    private OpenDocumentViewModel GetOpenDocumentViewModel(DocumentViewModel? documentViewModel = null)
    {
        var d = _serviceProvider.GetRequiredService<OpenDocumentViewModel>();
        d.SetDocument(documentViewModel);

        // 订阅字体变化事件
        //EditorFontFamilyChanged += d.OnEditorFontFamilyChanged;
        // 初始化字体
        d.OnEditorFontFamilyChanged(EditorFontFamily);

        return d;
    }


    /// <summary>
    /// 判断当前系统主题是否为暗色（平台相关）。
    /// </summary>
    /// <remarks>
    /// 由派生平台实现（Windows/Linux/macOS 等）；用于在“跟随系统主题”模式下选择暗/亮主题。
    /// </remarks>
    /// <summary>
    /// 执行 Is System Dark Theme 操作。
    /// </summary>
    protected abstract bool IsSystemDarkTheme();

    /// <summary>
    /// 主窗口标题文本。
    /// </summary>
    /// <remarks>
    /// 内部示例计算了版本号字符串 <c>currentVersion</c>，但当前返回值固定为 <c>"VEMS 0.1"</c>。
    /// 若需显示真实版本，可在不改变逻辑的前提下（例如在其他位置）使用 <c>currentVersion</c>。
    /// </remarks>
    public string WindowTitle
    {
        get
        {
            var currentVersion = s_currentVersion switch
            {
                { Minor: <= 0, Build: <= 0 } => s_currentVersion.Major.ToString(CultureInfo.InvariantCulture),
                { Build: <= 0 } => $"{s_currentVersion.Major}.{s_currentVersion.Minor}",
                _ => s_currentVersion.ToString()
            };
            return "VEMS " + 0.1;
        }
    }

    /// <summary>
    /// 打开问题反馈页面（GitHub Issues）的便捷方法。
    /// </summary>
    /// <remarks>
    /// 通过 <see cref="Task.Run(Action)"/> 在后台线程启动默认浏览器打开链接；
    /// 使用 <see cref="ProcessStartInfo.UseShellExecute"/> 以便由系统选择默认浏览器处理 URL。
    /// </remarks>
    private static void ReportProblem()
    {
        _ = Task.Run(() => Process.Start(
            new ProcessStartInfo
            {
                FileName = "https://github.com/aelij/RoslynPad/issues",
                UseShellExecute = true,
            }));
    }

    /// <summary>
    /// 是否检测到可用更新（由外部更新检查流程设置）。
    /// </summary>
    public bool HasUpdate
    {
        get => _hasUpdate; private set => SetProperty(ref _hasUpdate, value);
    }

    /// <summary>判断设置中缓存的最新版号是否高于当前版本。</summary>
    private bool HasCachedUpdate()
    {
        return Version.TryParse(Settings.LatestVersion, out var latestVersion) &&
               latestVersion > s_currentVersion;
    }

    /// <summary>联网检查并缓存最新版本号。</summary>
    /// <remarks>成功则更新 <c>Settings.LatestVersion</c>；失败记录为当前版本。</remarks>
    private async Task CheckForUpdates()
    {
        string latestVersionString;
        using (var client = new HttpClient())
        {
            try
            {
                latestVersionString = await client.GetStringAsync("https://roslynpad.net/latest").ConfigureAwait(false);
            }
            catch
            {
                return;
            }
        }

        if (Version.TryParse(latestVersionString, out var latestVersion))
        {
            if (latestVersion > s_currentVersion)
            {
                HasUpdate = true;
            }
            Settings.LatestVersion = latestVersionString;
        }
    }

    /// <summary>
    /// 创建并返回文档树根节点，并绑定文件系统监听器。
    /// </summary>
    /// <remarks>
    /// - 释放旧的 <c>_documentWatcher</c>，避免重复订阅；
    /// - 基于 <c>Settings.EffectiveDocumentPath</c> 构建根节点；
    /// - 调用 <c>EnsureMainViewModel(this)</c> 以确保节点持有主 VM 引用（必需）；
    /// - 使用 <c>DocumentWatcher</c> 订阅文件变更，同步树结构。
    /// </remarks>
    /// <returns>新的文档根节点。</returns>
    [MemberNotNull(nameof(_documentWatcher))]
    private DocumentViewModel CreateDocumentRoot()
    {
        _documentWatcher?.Dispose();
        var root = DocumentViewModel.CreateRoot(Settings.EffectiveDocumentPath);
        root.EnsureMainViewModel(this); // 必须加这一句！
        _documentWatcher = new DocumentWatcher(_documentFileWatcher, root);
        return root;
    }

    //修改初始文件夹-拉起窗口-重启生效
    /// <summary>
    /// 执行 Edit User Document Path 操作。
    /// </summary>
    public void EditUserDocumentPath()
    {
        var dialog = _serviceProvider.GetRequiredService<IFolderBrowserDialog>();
        dialog.ShowEditBox = true;
        dialog.SelectedPath = Settings.EffectiveDocumentPath;

        if (dialog.Show() == true)
        {
            var documentPath = dialog.SelectedPath;
            if (!DocumentRoot.Path.Equals(documentPath, StringComparison.OrdinalIgnoreCase))
            {
                Settings.DocumentPath = documentPath;

                DocumentRoot = CreateDocumentRoot();
            }
        }
    }

    /// <summary>NuGet 视图模型。</summary>
    /// <summary>
    /// Nu Get。
    /// </summary>
    public NuGetViewModel NuGet { get; }

    /// <summary>打开的文档集合。</summary>
    /// <summary>
    /// Open Documents。
    /// </summary>
    public ObservableCollection<OpenDocumentViewModel> OpenDocuments { get; }

    //public OpenDocumentViewModel? CurrentOpenDocument
    //{
    //    get => _currentOpenDocument;
    //    set
    //    {
    //        if (value == null) return; // prevent binding from clearing the value
    //        SetProperty(ref _currentOpenDocument, value);
    //        OnPropertyChanged(nameof(ActiveContent));
    //    }
    //}
    /// <summary>
    /// 当前激活的打开文档视图模型。
    /// </summary>
    /// <remarks>
    /// 切换逻辑：
    /// 1) 在切换离开旧文档前，若其 <c>IsDirty</c> 则触发自动保存（<c>AutoSaveAsync()</c>）；
    /// 2) 切换到新文档后，若检测到其自动保存文件则打印日志并读取内容（恢复到编辑器处留有 TODO 注释）；
    /// 3) 更新底层字段并通知 <see cref="ActiveContent"/> 绑定刷新。
    /// </remarks>
    public OpenDocumentViewModel? CurrentOpenDocument
    {
        get => _currentOpenDocument;
        set
        {
            // 1. 切换前自动暂存
            if (_currentOpenDocument != null && _currentOpenDocument.IsDirty)
            {
                Console.WriteLine($"[ ݴ ]      Զ      ĵ : {_currentOpenDocument.Title}");
                _currentOpenDocument.AutoSaveAsync().Wait();
                Console.WriteLine($"[ ݴ ]  Զ        : {_currentOpenDocument.Title}");
            }

            // 2. 切换到新文档时自动恢复
            if (value != null && value.Document != null)
            {
                var autoSavePath = value.Document.GetAutoSavePath();
                if (File.Exists(autoSavePath))
                {
                    Console.WriteLine($"[恢复] 检测到自动保存文件: {autoSavePath}");
                    var tempText = File.ReadAllText(autoSavePath);
                    Console.WriteLine($"[恢复] 自动保存内容长度: {tempText.Length}");
                    // 这里需要有方法将内容恢复到编辑器
                    // 例如：value.LoadText(tempText);
                    // 或自定义恢复逻辑
                }
                else
                {
                    Console.WriteLine($"[恢复] 未检测到自动保存文件: {autoSavePath}");
                }
            }

            Console.WriteLine($"[切换] 当前文档切换为: {value?.Title}");
            SetProperty(ref _currentOpenDocument, value);
            OnPropertyChanged(nameof(ActiveContent));
            //OnPropertyChanged(nameof(AvailablePlatforms2));
        }
    }

    /// <summary>
    /// Dock/内容区绑定的当前激活内容（通常等同于 <see cref="CurrentOpenDocument"/>）。
    /// </summary>
    /// <remarks>
    /// 设置逻辑：当传入的是 <see cref="OpenDocumentViewModel"/> 时，同步到
    /// <see cref="CurrentOpenDocument"/>；否则清空当前文档（如切换到“Home”等非文档页）。
    /// </remarks>
    public object? ActiveContent
    {
        get => _currentOpenDocument;
        set
        {
            // 如果是文档页：同步成当前文档（触发后续 Dock 激活）
            if (value is OpenDocumentViewModel viewModel)
            {
                CurrentOpenDocument = viewModel;
                OnPropertyChanged(); // ActiveContent 自身的通知
                return;
            }

            // 如果不是文档页（例如 Home）：明确清空当前文档
            ClearCurrentOpenDocument();  // 会发出 CurrentOpenDocument 的通知
            OnPropertyChanged();         // 再发一次 ActiveContent 的通知
        }
    }

    /// <summary>
    /// 清空当前打开文档并发送必要的属性变更通知。
    /// </summary>
    /// <remarks>
    /// 同时触发 <see cref="CurrentOpenDocument"/> 与 <see cref="ActiveContent"/> 的
    /// PropertyChanged，确保 UI 与 Dock 同步刷新。
    /// </remarks>
    private void ClearCurrentOpenDocument()
    {
        if (_currentOpenDocument == null) return;

        _currentOpenDocument = null;

        // 同时通知两者的变化，保证 UI 与 Dock 同步
        OnPropertyChanged(nameof(CurrentOpenDocument));
        OnPropertyChanged(nameof(ActiveContent));
    }

    /// <summary>
    /// “打开文件夹”命令（异步流程入口由对应方法实现）。
    /// </summary>
    /// <summary>
    /// 命令：Open Folder Command1。
    /// </summary>
    public IDelegateCommand OpenFolderCommand1 { get; }

    //public ICommand LightThemeCommand { get; }
    //public ICommand DarkThemeCommand { get; }
    // 添加在MainViewModel类中
    /// <summary>
    /// 循环切换主题（Light → Dark → System → …）的命令。
    /// </summary>
    /// <summary>
    /// 命令：Cycle Theme Command。
    /// </summary>
    public IDelegateCommand CycleThemeCommand { get; }

    // 需要补充的放大/缩小命令
    /// <summary>
    /// 将编辑器字号放大的命令。
    /// </summary>
    /// <summary>
    /// 命令：Increase Font Size Command。
    /// </summary>
    public ICommand IncreaseFontSizeCommand { get; }

    /// <summary>
    /// 将编辑器字号缩小的命令。
    /// </summary>
    /// <summary>
    /// 命令：Decrease Font Size Command。
    /// </summary>
    public ICommand DecreaseFontSizeCommand { get; }

    /// <summary>
    /// 显示“文档树视图 #0”的命令（其余视图会被关闭）。
    /// </summary>
    /// <summary>
    /// 命令：Show Document Tree View Command。
    /// </summary>
    public ICommand ShowDocumentTreeViewCommand { get; }

    /// <summary>
    /// 显示“文档树视图 #1”的命令（其余视图会被关闭）。
    /// </summary>
    /// <summary>
    /// 命令：Show User Preferences View Command。
    /// </summary>
    public ICommand ShowUserPreferencesViewCommand { get; }

    /// <summary>
    /// 显示“文档树视图 #2”的命令（其余视图会被关闭）。
    /// </summary>
    /// <summary>
    /// 命令：Show Performance Settings View Command。
    /// </summary>
    public ICommand ShowPerformanceSettingsViewCommand { get; }

    /// <summary>
    /// 显示“文档树视图 #3”的命令（其余视图会被关闭）。
    /// </summary>
    /// <summary>
    /// 命令：Show DLLExpand View Command。
    /// </summary>
    public ICommand ShowDLLExpandViewCommand { get; }

    /// <summary>
    /// 显示“文档树视图 #4”的命令（其余视图会被关闭）。
    /// </summary>
    /// <summary>
    /// 命令：Show Document Tree View4 Command。
    /// </summary>
    public ICommand ShowDocumentTreeView4Command { get; }

    /// <summary>
    /// 显示“文档树视图 #5”的命令（其余视图会被关闭）。
    /// </summary>
    /// <summary>
    /// 命令：Show Run And Debug View Command。
    /// </summary>
    public ICommand ShowRunAndDebugViewCommand { get; }

    /// <summary>
    /// 新建文档的命令，参数为源代码类型（<see cref="SourceCodeKind"/>）。
    /// </summary>
    /// <summary>
    /// 命令：New Document Command。
    /// </summary>
    public IDelegateCommand<SourceCodeKind> NewDocumentCommand { get; }

    /// <summary>
    /// 打开文件对话框并打开所选文档的命令。
    /// </summary>
    /// <summary>
    /// 命令：Open File Command。
    /// </summary>
    public IDelegateCommand OpenFileCommand { get; }

    /// <summary>
    /// 编辑用户文档根路径的命令。
    /// </summary>
    /// <summary>
    /// 命令：Edit User Document Path Command。
    /// </summary>
    public IDelegateCommand EditUserDocumentPathCommand { get; }

    /// <summary>
    /// 关闭当前活动文档的命令。
    /// </summary>
    /// <summary>
    /// 命令：Close Current Document Command。
    /// </summary>
    public IDelegateCommand CloseCurrentDocumentCommand { get; }

    /// <summary>
    /// 关闭指定打开文档的命令。
    /// </summary>
    /// <summary>
    /// 命令：Close Document Command。
    /// </summary>
    public IDelegateCommand<OpenDocumentViewModel> CloseDocumentCommand { get; }

    /// <summary>
    /// 切换是否启用编译优化（OptimizeCompilation）的命令。
    /// </summary>
    /// <summary>
    /// 命令：Toggle Optimization Command。
    /// </summary>
    public IDelegateCommand ToggleOptimizationCommand { get; }

    /// <summary>
    /// 清理 NuGet 还原缓存目录（roslynpad/restore）的命令。
    /// </summary>
    /// <summary>
    /// 命令：Clear Restore Cache Command。
    /// </summary>
    public IDelegateCommand ClearRestoreCacheCommand { get; }

    /// <summary>
    /// 以“预览标签”的方式打开文档：单击浏览时复用同一个预览页签；
    /// 如果文档已作为正式页签打开，则直接激活该正式页签。
    /// </summary>
    /// <param name="document">要预览/打开的文档节点。</param>
    /// <summary>
    /// 执行 Show Preview Tab 操作。
    /// </summary>
    public void ShowPreviewTab(DocumentViewModel document)
    {
        if (document == null || document.IsFolder) return;

        // 1) 如果已有“正式页签”，直接激活
        var existing = OpenDocuments.FirstOrDefault(
            x => !ReferenceEquals(x, _previewOpenDocument) && x.Document?.Path == document.Path);
        if (existing != null)
        {
            CurrentOpenDocument = existing;
            return;
        }

        // 2) 没有正式页签 —— 用“新建的预览 VM”替换旧预览 VM，确保内容刷新
        var newPreview = GetOpenDocumentViewModel(document);

        if (_previewOpenDocument == null)
        {
            _previewOpenDocument = newPreview;
            OpenDocuments.Add(_previewOpenDocument);
        }
        else
        {
            var idx = OpenDocuments.IndexOf(_previewOpenDocument);

            // 先关闭旧预览 VM 关联的 Roslyn 文档，避免缓存/句柄导致不刷新
            try
            {
                if (_previewOpenDocument.HasDocumentId)
                    RoslynHost?.CloseDocument(_previewOpenDocument.DocumentId);
                _previewOpenDocument.Close();
            }
            catch { /* 忽略清理异常 */ }

            if (idx >= 0)
                OpenDocuments[idx] = newPreview;   // 用新 VM 直接替换集合项
            else
                OpenDocuments.Add(newPreview);     // 理论上不会走到

            _previewOpenDocument = newPreview;
        }

        CurrentOpenDocument = _previewOpenDocument;
    }

    /// <summary>
    /// 正式打开文档为常驻页签：
    /// - 若该文档已打开（包括预览页签），则激活并将其从预览“转正”；
    /// - 若未打开，则创建新的打开文档 VM 并添加为页签。
    /// </summary>
    /// <param name="document">要打开的文档节点。</param>
    /// <summary>
    /// 执行 Open Document 操作。
    /// </summary>
    public void OpenDocument(DocumentViewModel document)
    {
        if (document == null || document.IsFolder) return;

        // 已经打开（预览或正式）→ 激活，并将其“钉住”为正式页签
        var opened = OpenDocuments.FirstOrDefault(x => x.Document?.Path == document.Path);
        if (opened != null)
        {
            CurrentOpenDocument = opened;

            // 如果当前这个就是预览页签，把预览标记清掉，后续单击不再复用它
            if (ReferenceEquals(opened, _previewOpenDocument))
                _previewOpenDocument = null;

            return;
        }

        // 未打开 → 新建正式页签
        var vm = GetOpenDocumentViewModel(document);
        OpenDocuments.Add(vm);
        CurrentOpenDocument = vm;
    }

    /// <summary>
    /// 打开文件对话框并加载所选文档。
    /// </summary>
    /// <remarks>
    /// - 支持过滤 C# 源文件（cs, csx）；<br/>
    /// - 若存在对应的 autosave 文件，优先加载 autosave 内容；<br/>
    /// - 调用 <see cref="OpenDocument(DocumentViewModel)"/> 完成打开/切换逻辑。
    /// </remarks>
    /// <summary>
    /// 执行 Open File 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task OpenFile()
    {
        if (!IsInitialized) return;

        var dialog = _serviceProvider.GetRequiredService<IOpenFileDialog>();
        dialog.Filter = new FileDialogFilter("C# Files", "cs", "csx");
        var fileNames = await dialog.ShowAsync().ConfigureAwait(true);
        if (fileNames == null)
        {
            return;
        }
        var raw = fileNames.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(raw)) return;

        // 如果是 file: URI -> 转为本地路径
        if (Uri.TryCreate(raw, UriKind.Absolute, out var uri) && uri.IsFile)
        {
            raw = uri.LocalPath; // 会把 file:///C:/... 转成 C:\...
        }
        // 归一化路径：避免 Windows 下大小写不一致导致的路径比较问题
        var filePath = IOUtilities.NormalizeFilePath(raw);


        var document = DocumentViewModel.FromPath(filePath);

        if (!document.IsAutoSave)
        {
            var autoSavePath = document.GetAutoSavePath();
            if (File.Exists(autoSavePath))
            {
                document = DocumentViewModel.FromPath(autoSavePath);
            }
        }

        OpenDocument(document);
    }

    /// <summary>
    /// 新建一个空白文档并切换为当前文档。
    /// </summary>
    /// <param name="kind">文档类型（Regular 或 Script）。</param>
    /// <summary>
    /// 执行 Create New Document 操作。
    /// </summary>
    public void CreateNewDocument(SourceCodeKind kind = SourceCodeKind.Regular)
    {
        var openDocument = GetOpenDocumentViewModel();
        openDocument.SourceCodeKind = kind;
        OpenDocuments.Add(openDocument);
        CurrentOpenDocument = openDocument;
    }

    /// <summary>
    /// 关闭指定的已打开文档（带“是否保存”的交互确认）。
    /// </summary>
    /// <param name="document">要关闭的文档 ViewModel；若为 <c>null</c> 则提示无法关闭。</param>
    /// <summary>
    /// 执行 Close Document 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task CloseDocument(OpenDocumentViewModel? document)
    {
        if (document == null)
        {
            var box1 = MessageBoxManager.GetMessageBoxStandard(
               "警告",
               "未选中文档，无法关闭。",
               ButtonEnum.Ok,
               Icon.Warning
           );
            // 推荐：传递主窗口作为 Owner
            var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
            if (mainWindow != null)
                await box1.ShowWindowDialogAsync(mainWindow).ConfigureAwait(true);
            else
                await box1.ShowAsync().ConfigureAwait(true);
            return;
        }

        var exists = document.Document != null;
        var isDirty = document.IsDirty; // 如无此属性请替换为实际判断
        // 已存在且有变化
        if (isDirty)
        {
            var fileName = document.Title ?? document.Document?.Name ?? "未知文件";
            var box = MessageBoxManager.GetMessageBoxStandard(
                "警告",
                $"文档 [{fileName}] 已修改，是否保存？",
                ButtonEnum.YesNoCancel,
                Icon.Warning
            );
            // 获取主窗口实例
            var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
            ButtonResult msgResult;
            if (mainWindow != null)
                msgResult = await box.ShowWindowDialogAsync(mainWindow).ConfigureAwait(true);
            else
                msgResult = await box.ShowAsync().ConfigureAwait(true);

            if (msgResult == ButtonResult.Yes)
            {
                SaveCurrentDocument();
                if (document.HasDocumentId)
                {
                    RoslynHost?.CloseDocument(document.DocumentId);
                }
                OpenDocuments.Remove(document);
                document.Close();
            }
            else if (msgResult == ButtonResult.No)
            {
                if (document.HasDocumentId)
                {
                    RoslynHost?.CloseDocument(document.DocumentId);
                }
                OpenDocuments.Remove(document);
                document.Close();
            }
            // 取消则不关闭
            return;
        }

        // 不存在且无修改
        if (!isDirty)
        {
            if (document.HasDocumentId)
            {
                RoslynHost?.CloseDocument(document.DocumentId);
            }
            OpenDocuments.Remove(document);
            document.Close();
            return;
        }
    }
    /// <summary>
    /// 执行 Auto Save Open Documents 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task AutoSaveOpenDocuments()
    {
        foreach (var document in OpenDocuments)
        {
            await document.AutoSaveAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 关闭当前文档（相当于对 <see cref="CurrentOpenDocument"/> 调用 <see cref="CloseDocument(OpenDocumentViewModel?)"/>）。
    /// </summary>
    private async Task CloseCurrentDocument()
    {
        if (CurrentOpenDocument == null) return;
        await CloseDocument(CurrentOpenDocument).ConfigureAwait(false);
        if (!OpenDocuments.Any())
        {
            ClearCurrentOpenDocument();
        }
    }

    /// <summary>
    /// 关闭全部已打开文档（逐个触发保存确认逻辑）。
    /// </summary>
    /// <summary>
    /// 执行 Close All Documents 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task CloseAllDocuments()
    {
        // 不能在枚举时修改集合：先复制一份
        var openDocs = new ObservableCollection<OpenDocumentViewModel>(OpenDocuments);
        foreach (var document in openDocs)
        {
            await CloseDocument(document).ConfigureAwait(false);
        }
    }
    /// <summary>应用退出时的清理入口：自动保存全部打开文档，并清理临时构建目录。</summary>
    /// <summary>
    /// 执行 On Exit 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task OnExit()
    {
        await AutoSaveOpenDocuments().ConfigureAwait(false);
        IOUtilities.PerformIO(() => Directory.Delete(Path.Combine(Path.GetTempPath(), "roslynpad", "build"), recursive: true));
    }

    /// <summary>最后一次错误（将 <see cref="AggregateException"/> 扁平化后返回）。</summary>
    public Exception? LastError
    {
        get
        {
            var exception = _telemetryProvider.LastError;
            var aggregateException = exception as AggregateException;
            return aggregateException?.Flatten() ?? exception;
        }
    }

    /// <summary>
    /// 是否存在最后一次错误（<see cref="LastError"/> 非空）。
    /// </summary>
    /// <summary>
    /// Has Error。
    /// </summary>
    public bool HasError => LastError != null;

    /// <summary>
    /// 清除最后一次错误的命令（通常调用遥测提供器的清除方法）。
    /// </summary>
    /// <summary>
    /// 命令：Clear Error Command。
    /// </summary>
    public IDelegateCommand ClearErrorCommand { get; }

    /// <summary>
    /// 是否允许发送遥测/错误信息（持久化到 <see cref="Settings"/>）。
    /// </summary>
    public bool SendTelemetry
    {
        get => Settings.SendErrors;
        set
        {
            Settings.SendErrors = value;
            OnPropertyChanged(nameof(SendTelemetry));
        }
    }

    /// <summary>
    /// 是否处于“已初始化但没有任何打开文档”的状态（可用于空态 UI）。
    /// </summary>
    /// <summary>
    /// Has No Open Documents。
    /// </summary>
    public bool HasNoOpenDocuments => IsInitialized && OpenDocuments.Count == 0;

    /// <summary>
    /// 报告问题的命令（通常会打开 Issues 页面或收集日志）。
    /// </summary>
    /// <summary>
    /// 命令：Report Problem Command。
    /// </summary>
    public IDelegateCommand ReportProblemCommand { get; }

    /// <summary>
    /// 编辑器字号的最小值。
    /// </summary>
    /// <summary>
    /// Minimum Font Size。
    /// </summary>
    public const double MinimumFontSize = 8;

    /// <summary>
    /// 编辑器字号的最大值。
    /// </summary>
    /// <summary>
    /// Maximum Font Size。
    /// </summary>
    public const double MaximumFontSize = 72;

    /// <summary>校验字号是否处于允许范围。</summary>
    /// <summary>
    /// 执行 Is Valid Font Size 操作。
    /// </summary>
    public static bool IsValidFontSize(double value) => value >= MinimumFontSize && value <= MaximumFontSize;

    /// <summary>编辑器字号：变更将同步保存到设置，并触发 <see cref="EditorFontSizeChanged"/>。</summary>
    public int EditorFontSize
    {
        get => _editorFontSize;
        set
        {
            if (!IsValidFontSize(value))
            {
                return;
            }

            if (SetProperty(ref _editorFontSize, value))
            {
                Settings.EditorFontSize = value;
                EditorFontSizeChanged?.Invoke(value);
                // 可选：输出日志
                Console.WriteLine($"[MainViewModel] 当前编辑器字号: {value}");
                SaveShortcuts();

                // 输出到结果面板，保持与之前相同的 OutputResult 格式
                var msg = "The editor font size has been changed to " + value.ToString(CultureInfo.InvariantCulture);
                OutputResult("[EditorFontSize]", msg, "[Info]", null);
            }
        }
    }
    /// <summary>当编辑器字号变更时触发（用于通知各编辑器实例）。</summary>
    public event Action<double>? EditorFontSizeChanged;

    /// <summary>在当前文档根下创建一个新的文档节点。</summary>
    /// <summary>
    /// 执行 Add Document 操作。
    /// </summary>
    public DocumentViewModel AddDocument(string documentName)
    {
        return DocumentRoot.CreateNew(documentName);
    }

    /// <summary>搜索文本：设置后会进行节流调度触发搜索。</summary>
    public string? SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                // 实时节流搜索（250ms）
                ScheduleSearchDebounced();
            }
        }
    }

    /// <summary>是否处于“仅显示搜索结果”模式。</summary>
    public bool IsWithinSearchResults
    {
        get => _isWithinSearchResults;
        private set
        {
            SetProperty(ref _isWithinSearchResults, value);
            OnPropertyChanged(nameof(CanClearSearch));
        }
    }

    /// <summary>是否可执行“清除搜索结果”。</summary>
    /// <summary>
    /// 执行 Is Null Or Empty 操作。
    /// </summary>
    public bool CanClearSearch => IsWithinSearchResults || !string.IsNullOrEmpty(SearchText);

    /// <summary>执行搜索命令（异步）。</summary>
    /// <summary>
    /// 命令：Create Async。
    /// </summary>
    public IDelegateCommand SearchCommand => _commands.CreateAsync(Search);

    /// <summary>执行实际搜索逻辑：支持文件名匹配与正则全文检索。</summary>
    private async Task Search()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            ClearSearch();
            return;
        }

        // 仅按“名称包含”匹配：只设置文件名匹配标记
        if (!SearchFileContents)
        {
            IsWithinSearchResults = true;

            foreach (var document in GetAllDocumentsForSearch(DocumentRoot))
            {
                document.IsSearchMatch = SearchDocumentName(document);
            }

            return;
        }

        Regex? regex = null;
        if (SearchUsingRegex)
        {
            regex = CreateSearchRegex();

            if (regex == null)
            {
                return;
            }
        }

        IsWithinSearchResults = true;

        foreach (var document in GetAllDocumentsForSearch(DocumentRoot))
        {
            if (SearchDocumentName(document))
            {
                document.IsSearchMatch = true;
            }
            else
            {
                await SearchInFile(document, regex).ConfigureAwait(false);
            }
        }

        /// <summary>仅在文件名中匹配关键字（忽略大小写）。</summary>
        bool SearchDocumentName(DocumentViewModel document)
        {
            return document.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>根据 <see cref="SearchText"/> 构建正则，超时 5 秒；失败返回 null。</summary>
        Regex? CreateSearchRegex()
        {
            try
            {
                var regex = new Regex(SearchText, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5));

                ClearError(nameof(SearchText), "Regex");

                return regex;
            }
            catch (ArgumentException)
            {
                SetError(nameof(SearchText), "Regex", "Invalid regular expression");

                return null;
            }
        }

        async Task SearchInFile(DocumentViewModel document, Regex? regex)
        {
            // a regex can span many lines so we need to load the entire file;
            // otherwise, search line-by-line

            if (regex != null)
            {
                var documentText = await IOUtilities.ReadAllTextAsync(document.Path).ConfigureAwait(false);
                try
                {
                    document.IsSearchMatch = regex.IsMatch(documentText);
                }
                catch (RegexMatchTimeoutException)
                {
                    document.IsSearchMatch = false;
                }
            }
            else
            {
                // need IAsyncEnumerable here, but for now just push it to the thread-pool
                await Task.Run(() =>
                {
                    var lines = IOUtilities.ReadLines(document.Path);
                    document.IsSearchMatch = lines.Any(line =>
                        line.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                }).ConfigureAwait(false);
            }
        }

        // 展开到命中，并刷新“可见树”
        ExpandToMatches(DocumentRoot);
        DocumentRoot.RefreshSearchVisibility();

    }

    /// <summary>获取用于搜索遍历的文档序列（递归枚举文件夹中的文件）。</summary>
    private static IEnumerable<DocumentViewModel> GetAllDocumentsForSearch(DocumentViewModel root)
    {
        var children = root.Children;
        if (children is null)
        {
            yield break;
        }

        foreach (var document in children)
        {
            if (document.IsFolder)
            {
                foreach (var childDocument in GetAllDocumentsForSearch(document))
                {
                    yield return childDocument;
                }

                // TODO: I'm lazy :)
                document.IsSearchMatch = document.Children?.Any(c => c.IsSearchMatch) == true;
            }
            else
            {
                yield return document;
            }
        }
    }
    /// <summary>
    /// 是否在“文件内容”中进行搜索。
    /// <para>为 <c>false</c> 时仅按文件名匹配；为 <c>true</c> 时会在全文中匹配（可配合正则）。</para>
    /// </summary>
    public bool SearchFileContents
    {
        get => Settings.SearchFileContents;
        set
        {
            Settings.SearchFileContents = value;
            if (!value)
            {
                // 关闭“全文搜索”时，强制关闭正则开关，避免误用
                SearchUsingRegex = false;
            }
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 是否使用正则表达式进行搜索。
    /// <para>开启正则将自动打开 <see cref="SearchFileContents"/>（正则仅对“全文”有效）。</para>
    /// </summary>
    public bool SearchUsingRegex
    {
        get => Settings.SearchUsingRegex;
        set
        {
            Settings.SearchUsingRegex = value;
            if (value)
            {
                // 开启正则时，确保启用全文搜索
                SearchFileContents = true;
            }
            OnPropertyChanged();
        }
    }

    /// <summary>清除搜索结果的命令（重置关键字与命中标记）。</summary>
    /// <summary>
    /// 命令：Create。
    /// </summary>
    public IDelegateCommand ClearSearchCommand => _commands.Create(ClearSearch);

    /// <summary>是否使用“跟随系统主题”模式（只读，随主题变更时更新）。</summary>
    /// <summary>
    /// Use System Theme。
    /// </summary>
    public bool UseSystemTheme { get; private set; }

    /// <summary>当前主题类型（Dark / Light / System）。</summary>
    /// <summary>
    /// Theme Type。
    /// </summary>
    public ThemeType ThemeType { get; private set; }

    /// <summary>
    /// 当前主题对象（非空包装）。
    /// <para>设置时触发 <see cref="ThemeChanged"/> 事件，通知相关 UI 刷新。</para>
    /// </summary>
    public Theme Theme
    {
        get => _theme.NotNull();
        private set
        {
            _theme = value;
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>主题变更事件：当 <see cref="Theme"/> 更新时触发。</summary>
    public event EventHandler<EventArgs>? ThemeChanged;

    /// <summary>
    /// 清除搜索：重置 <see cref="SearchText"/>、退出“仅显示搜索结果”，并恢复所有节点可见。
    /// </summary>
    private void ClearSearch()
    {
        SearchText = null;
        IsWithinSearchResults = false;
        ClearErrors(nameof(SearchText));

        foreach (var document in GetAllDocumentsForSearch(DocumentRoot))
        {
            document.IsSearchMatch = true;
        }

        DocumentRoot.RefreshSearchVisibility();
        // 如需全部折叠也可在此处处理（例如 CollapseAll(...)）
    }

    /// <summary>
    /// 文档树文件监听器：监听磁盘文件变化，同步更新 <see cref="DocumentRoot"/> 树结构。
    /// </summary>
    private class DocumentWatcher : IDisposable
    {
        private static readonly char[] s_pathSeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

        private readonly DocumentViewModel _documentRoot;
        private readonly IDisposable _subscription;

        /// <summary>
        /// 构造监听器，并订阅 <see cref="DocumentFileWatcher"/> 的变化事件。
        /// </summary>
        /// <param name="watcher">底层文件系统变更观察者。</param>
        /// <param name="documentRoot">文档树根节点。</param>
        /// <summary>
        /// 执行 Document Watcher 操作。
        /// </summary>
        public DocumentWatcher(DocumentFileWatcher watcher, DocumentViewModel documentRoot)
        {
            _documentRoot = documentRoot;
            watcher.Path = documentRoot.Path;
            _subscription = watcher.Subscribe(OnDocumentFileChanged);
        }

        /// <summary>释放底层订阅。</summary>
        /// <summary>
        /// 执行 Dispose 操作。
        /// </summary>
        public void Dispose() => _subscription.Dispose();

        /// <summary>
        /// 处理单次文件变更：按路径逐层定位节点，并根据变更类型更新/移动/删除。
        /// </summary>
        private void OnDocumentFileChanged(DocumentFileChanged data)
        {
            var pathParts = data.Path.Substring(_documentRoot.Path.Length)
                .Split(s_pathSeparators, StringSplitOptions.RemoveEmptyEntries);

            var current = _documentRoot;

            for (var index = 0; index < pathParts.Length; index++)
            {
                if (!current.IsChildrenInitialized)
                {
                    break;
                }

                var part = pathParts[index];
                var isLast = index == pathParts.Length - 1;

                var parent = current;
                current = current.InternalChildren[part];

                // the current part is not in the tree
                if (current is null)
                {
                    if (data.Type != DocumentFileChangeType.Deleted)
                    {
                        var currentPath = isLast && data.Type == DocumentFileChangeType.Renamed
                            ? data.NewPath
                            : Path.Combine(_documentRoot.Path, Path.Combine(pathParts.Take(index + 1).ToArray()));

                        var newDocument = DocumentViewModel.FromPath(currentPath!);
                        if (!newDocument.IsAutoSave &&
                            IsRelevantDocument(newDocument))
                        {
                            parent.AddChild(newDocument);
                        }
                    }

                    break;
                }

                // 已存在的最后一层（实际文件）
                if (isLast)
                {
                    switch (data.Type)
                    {
                        case DocumentFileChangeType.Renamed:
                            if (data.NewPath != null)
                            {
                                current.ChangePath(data.NewPath);
                                // 移动到新位置（父节点可能变化）
                                parent.InternalChildren.Remove(current);
                                if (IsRelevantDocument(current))
                                {
                                    parent.AddChild(current);
                                }
                            }
                            break;
                        case DocumentFileChangeType.Deleted:
                            parent.InternalChildren.Remove(current);
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 判断文档是否与树展示相关：文件夹或扩展名在允许列表中。
    /// </summary>
    private static bool IsRelevantDocument(DocumentViewModel document)
    {
        return document.IsFolder ||
            DocumentViewModel.RelevantFileExtensions.Contains(Path.GetExtension(document.Name));
    }
    /// <summary>释放文件监视器等资源。</summary>
    /// <summary>
    /// 执行 Dispose 操作。
    /// </summary>
    public void Dispose()
    {
        _documentFileWatcher?.Dispose();
    }

    /// <summary>IsDocumentTreeViewVisible 开关（侧栏树是否可见）。</summary>
    public bool IsDocumentTreeViewVisible
    {
        get => _isDocumentTreeViewVisible;
        set => SetProperty(ref _isDocumentTreeViewVisible, value);
    }

    /// <summary>IsUserPreferencesViewVisible 开关（侧栏树是否可见）。</summary>
    public bool IsUserPreferencesViewVisible
    {
        get => _isUserPreferencesViewVisible;
        set => SetProperty(ref _isUserPreferencesViewVisible, value);
    }

    /// <summary>IsPerformanceSettingsViewVisible 开关（侧栏树是否可见）。</summary>
    public bool IsPerformanceSettingsViewVisible
    {
        get => _isPerformanceSettingsViewVisible;
        set => SetProperty(ref _isPerformanceSettingsViewVisible, value);
    }

    /// <summary>IsDLLExpandViewVisible 开关（侧栏树是否可见）。</summary>
    public bool IsDLLExpandViewVisible
    {
        get => _isDLLExpandViewVisible;
        set => SetProperty(ref _isDLLExpandViewVisible, value);
    }

    /// <summary>IsDocumentTreeView4Visible 开关（侧栏树是否可见）。</summary>
    public bool IsDocumentTreeView4Visible
    {
        get => _isDocumentTreeView4Visible;
        set => SetProperty(ref _isDocumentTreeView4Visible, value);
    }

    /// <summary>IsRunAndDebugViewVisible 开关（侧栏树是否可见）。</summary>
    public bool IsRunAndDebugViewVisible
    {
        get => _isRunAndDebugViewVisible;
        set => SetProperty(ref _isRunAndDebugViewVisible, value);
    }

    // 主题循环切换方法
    /// <summary>循环切换内置主题（Light → Dark → System → Light）。</summary>
    private void CycleTheme()
    {
        var currentTheme = Settings.BuiltInTheme;
        var nextTheme = currentTheme switch
        {
            BuiltInTheme.Light => BuiltInTheme.Dark,
            BuiltInTheme.Dark => BuiltInTheme.System,
            BuiltInTheme.System => BuiltInTheme.Light,
            _ => BuiltInTheme.Light
        };

        ChangeTheme(nextTheme);
    }

    /// <summary>切换内置主题并刷新状态。</summary>
    /// <summary>
    /// 执行 Change Theme 操作。
    /// </summary>
    public void ChangeTheme(BuiltInTheme theme)
    {
        Settings.BuiltInTheme = theme;
        Settings.CustomThemePath = null; // 清除自定义主题路径

        // 重新加载主题
        UseSystemTheme = Settings.CustomThemePath is null && Settings.BuiltInTheme == BuiltInTheme.System;

        (string? path, ThemeType type) themeInfo = theme switch
        {
            BuiltInTheme.System => GetBuiltinThemePath(BuiltInTheme.System),
            BuiltInTheme.Light => (GetOsSpecificThemePath("light_modern.json"), ThemeType.Light),
            BuiltInTheme.Dark => (GetOsSpecificThemePath("dark_modern.json"), ThemeType.Dark),
            BuiltInTheme.VEMS => (GetOsSpecificThemePath("VEMS_modern.json"), ThemeType.Light), // VEMS主题为Light类型
            BuiltInTheme.Quiet => (GetOsSpecificThemePath("Quiet_modern.json"), ThemeType.Light), // VEMS主题为Light类型
            BuiltInTheme.Solarized => (GetOsSpecificThemePath("Solarized_modern.json"), ThemeType.Light), // VEMS主题为Light类型
            BuiltInTheme.Red => (GetOsSpecificThemePath("Red_modern.json"), ThemeType.Dark), // VEMS主题为Light类型
            BuiltInTheme.Abyss => (GetOsSpecificThemePath("Abyss_modern.json"), ThemeType.Dark), // VEMS主题为Light类型
            BuiltInTheme.KimbieDark => (GetOsSpecificThemePath("KimbieDark_modern.json"), ThemeType.Dark), // VEMS主题为Light类型
            _ => (GetOsSpecificThemePath("light_modern.json"), ThemeType.Light)
        };

        LoadTheme(themeInfo.path, themeInfo.type);
        InitialTheme = theme.ToString();

    }

    /// <summary>
    /// 应用指定的内置主题到应用程序：更新 <see cref="Theme"/>，
    /// 设置 Avalonia 的 <see cref="Application.RequestedThemeVariant"/>，
    /// 并替换全局资源字典中的 <see cref="ThemeDictionary"/>。
    /// </summary>
    /// <param name="theme">要应用的内置主题枚举值。</param>
    /// <remarks>
    /// - 先调用 <see cref="ChangeTheme(BuiltInTheme)"/> 更新内部状态与 <see cref="Theme"/>；
    /// - 若未启用“跟随系统主题”（<see cref="UseSystemTheme"/> == false），
    ///   则根据 <see cref="ThemeType"/> 设置 <see cref="Application.RequestedThemeVariant"/>；
    /// - 用新的 <see cref="ThemeDictionary"/> 替换全局资源字典，影响全局样式/颜色等资源。
    /// </remarks>
    /// <summary>
    /// 执行 Apply Theme 操作。
    /// </summary>
    public void ApplyTheme(BuiltInTheme theme)
    {
        ChangeTheme(theme);

        if (Application.Current is not { } app)
            return;

        // 设置 Avalonia 的 RequestedThemeVariant
        if (!UseSystemTheme)
        {
            app.RequestedThemeVariant = ThemeType switch
            {
                ThemeType.Light => ThemeVariant.Light,
                ThemeType.Dark => ThemeVariant.Dark,

                _ => null
            };
        }

        // 替换全局资源字典
        if (_themeDictionary is not null)
        {
            app.Resources.MergedDictionaries.Remove(_themeDictionary);
        }
        _themeDictionary = new ThemeDictionary(Theme);
        app.Resources.MergedDictionaries.Add(_themeDictionary);
    }

    /// <summary>
    /// 根据主题名字符串应用内置主题的便捷方法。
    /// </summary>
    /// <param name="themeName">
    /// 主题名字符串；若为 <c>null</c> 或空白，将回退到 <see cref="BuiltInTheme.Light"/>。
    /// </param>
    /// <remarks>
    /// - 使用 <see cref="Enum.TryParse{TEnum}(string?, out TEnum)"/> 将字符串转换为 <see cref="BuiltInTheme"/>；
    /// - 解析失败时回退为 <see cref="BuiltInTheme.Light"/>；
    /// - 内部最终调用 <see cref="ApplyTheme(BuiltInTheme)"/> 完成实际应用。
    /// </remarks>
    /// <summary>
    /// 执行 Apply Theme 操作。
    /// </summary>
    public void ApplyTheme(string? themeName)
    {
        if (string.IsNullOrWhiteSpace(themeName))
        {
            // 传入空值时，使用默认主题
            ApplyTheme(BuiltInTheme.Light);
            return;
        }
        if (Enum.TryParse<BuiltInTheme>(themeName, out var theme))
        {
            ApplyTheme(theme);
        }
        else
        {
            ApplyTheme(BuiltInTheme.Light);
        }
    }

    private ThemeDictionary? _themeDictionary;
    // 辅助方法：获取操作系统特定的主题路径
    private static string GetOsSpecificThemePath(string fileName)
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? Path.Combine(AppContext.BaseDirectory, "..", "Resources", "Themes", fileName)
            : Path.Combine(AppContext.BaseDirectory, "Themes", fileName);
    }

    // 辅助方法：加载主题
    private void LoadTheme(string? themeFile, ThemeType type)
    {
        if (themeFile is null) return;

        ThemeType = type;
        Theme = _themeManager.ReadThemeAsync(themeFile, type).GetAwaiter().GetResult();
    }
    // 辅助方法：获取内置主题路径
    private (string? path, ThemeType type) GetBuiltinThemePath(BuiltInTheme builtInTheme)
    {
        if (builtInTheme == BuiltInTheme.System)
        {
            var isSystemDarkTheme = IsSystemDarkTheme();
            builtInTheme = isSystemDarkTheme ? BuiltInTheme.Dark : BuiltInTheme.Light;
        }

        (string file, ThemeType type) theme = builtInTheme switch
        {
            BuiltInTheme.Light => ("light_modern.json", ThemeType.Light),
            BuiltInTheme.Dark => ("dark_modern.json", ThemeType.Dark),
            _ => ("light_modern.json", ThemeType.Light),
        };

        return (GetOsSpecificThemePath(theme.file), theme.type);
    }

    /// <summary>
    /// 全局本地化管理器（单例）。
    /// </summary>
    /// <summary>
    /// Localized。
    /// </summary>
    public LocalizationManager Localized => LocalizationManager.Instance;

    /// <summary>
    /// 语言切换命令（参数通常为目标语言代码，如 "en-US" / "zh-CN"）。
    /// </summary>
    // 在 ViewModel 中定义切换命令
    /// <summary>
    /// 命令：Switch Language Command。
    /// </summary>
    public ICommand SwitchLanguageCommand { get; }

    /// <summary>
    /// 通过文件夹选择对话框打开并切换当前工作目录；成功选择后重建文档树。
    /// </summary>
    /// <summary>
    /// 执行 Open Folder Async1 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task OpenFolderAsync1()
    {
        await Task.Yield(); // 或 await Task.Run(() => { ... }); 视具体需求
        var dialog = _serviceProvider.GetRequiredService<IFolderBrowserDialog>();
        dialog.ShowEditBox = false;
        //dialog.SelectedPath = Settings.EffectiveDocumentPath;
        dialog.SelectedPath = DocumentRoot.Path;
        if (dialog.Show() == true)
        {
            var folderPath = dialog.SelectedPath;
            if (!string.IsNullOrEmpty(folderPath) && !DocumentRoot.Path.Equals(folderPath, StringComparison.OrdinalIgnoreCase))
            {
                //Console.WriteLine("FPATH" + folderPath);
                //Settings.DocumentPath = folderPath;
                //Console.WriteLine("HAXI" + DocumentRoot.GetHashCode());
                //Console.WriteLine("NAME" + DocumentRoot.Name);
                //Console.WriteLine("QIANPATH" + DocumentRoot.Path);
                //DocumentRoot = CreateDocumentRoot();
                //DocumentRoot.ChangePath(folderPath);
                //Console.WriteLine("HAXI" + DocumentRoot.GetHashCode());
                //Console.WriteLine("NAME" + DocumentRoot.Name);
                //Console.WriteLine("HOUPATH" + DocumentRoot.Path);
                //OnPropertyChanged(nameof(DocumentRoot));
                //var children = DocumentRoot.Children;
                //var children = DocumentRoot.Children;
                //children?.Clear();
                //foreach (var child in DocumentViewModel.Create...t(folderPath).Children ?? Enumerable.Empty<DocumentViewModel>())
                //{
                //    children?.Add(child);
                //}
                //OnPropertyChanged(nameof(DocumentRoot.Children));
                //OnPropertyChanged(nameof(DocumentRoot.VisibleChildren));
                Console.WriteLine("FPATH" + folderPath);
                Settings.DocumentPath = folderPath;
                DocumentRoot = DocumentViewModel.CreateRoot(folderPath);
                DocumentRoot.EnsureMainViewModel(this);
                OnPropertyChanged(nameof(DocumentRoot));
                // 如有需要可加：
                OnPropertyChanged(nameof(DocumentRoot.VisibleChildren));
            }
        }
    }

    /// <summary>
    /// 关闭当前文件夹视图：重建默认文档树、清空打开的文档并通知 UI 刷新。
    /// </summary>
    /// <summary>
    /// 执行 Close Folder 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task CloseFolder()
    {
        await Task.Yield(); // 或 await Task.Run(() => { ... }); 视具体需求
                            // 清空文件夹树数据源
                            //FolderItems.Clear();

        // 重置 DocumentRoot 为默认路径（如用户文档目录或空）
        //Settings.DocumentPath = string.Empty;
        DocumentRoot = CreateDocumentRoot();
        OnPropertyChanged(nameof(DocumentRoot));

        // 清空当前打开的文档列表
        OpenDocuments.Clear();
        //CurrentOpenDocument = null;

        // 通知界面刷新
        OnPropertyChanged(nameof(DocumentRoot.Children));
    }

    /// <summary>
    /// 搜索节流相关：取消令牌与“悬挂任务”占位（避免 CS4014）。
    /// </summary>
    // 字段
    private CancellationTokenSource? _searchCts;
    private Task? _pendingSearchTask; // 仅用于持有任务，避免 CS4014

    /// <summary>
    /// 以 250ms 节流调度搜索：在输入停止一定时间后触发 <see cref="Search"/> 或清空结果。
    /// </summary>
    private void ScheduleSearchDebounced()
    {
        _searchCts?.Cancel();
        var cts = _searchCts = new CancellationTokenSource();

        _pendingSearchTask = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(250, cts.Token).ConfigureAwait(false); // 250ms 节流
                if (cts.IsCancellationRequested) return;

                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    // 如果 ClearSearch 是 async：
                    // await ClearSearch().ConfigureAwait(false);
                    // 如果 ClearSearch 是同步（void），就保留下面这一句：
                    ClearSearch();
                }
                else
                {
                    // Search 是 async Task => 必须 await，修复 CS4014
                    await Search().ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) { /* 输入继续变化，正常取消 */ }
            catch (Exception ex)
            {
                _telemetryProvider?.ReportError(ex);
            }
        });
    }

    /// <summary>
    /// 可选：匹配后自动展开祖先节点，改善检索命中的可见性体验。
    /// </summary>
    /// <param name="node">起始节点（通常为根）。</param>
    // 可选：匹配后自动展开祖先
    private static void ExpandToMatches(DocumentViewModel node)
    {
        var anyChildVisible = false;
        if (node.Children != null)
        {
            foreach (var c in node.Children)
            {
                ExpandToMatches(c);
                anyChildVisible |= c.IsVisible;
            }
        }
        node.IsExpanded = node.IsSearchMatch || anyChildVisible;
    }

    /// <summary>
    /// “新建文档模板”面板是否可见的标志位。
    /// </summary>
    private bool _isNewDoc1Visible;

    /// <summary>
    /// 绑定到 UI 的“新建文档模板”开关属性。
    /// </summary>
    public bool IsNewDoc1Visible
    {
        get => _isNewDoc1Visible;
        set => SetProperty(ref _isNewDoc1Visible, value);
    }

    /// <summary>
    /// 打开“新建文档模板”面板的命令。
    /// </summary>
    /// <summary>
    /// 命令：Show New Doc1 Command。
    /// </summary>
    public ICommand ShowNewDoc1Command { get; }

    /// <summary>
    /// 关闭“新建文档模板”面板的命令。
    /// </summary>
    /// <summary>
    /// 命令：Hide New Doc1 Command。
    /// </summary>
    public ICommand HideNewDoc1Command { get; }

    /// <summary>
    /// 快捷键项集合（用于绑定列表或网格进行查看/编辑）。
    /// </summary>
    /// <summary>
    /// 执行 new 操作。
    /// </summary>
    public ObservableCollection<ShortcutItemViewModel> ShortcutItems { get; } = new();

    //快捷键相关配置
    //文件菜单
    /// <summary>
    /// “文件(File)”主菜单的快捷键后备字段（可配置）。
    /// </summary>
    private string _fileMenu = "Ctrl+F";

    /// <summary>
    /// “文件(File)”主菜单的快捷键（用于快捷提示与保存到用户配置）。
    /// </summary>
    public string FileMenu
    {
        get => _fileMenu;
        set
        {
            if (_fileMenu != value)
            {
                _fileMenu = value;
                OnPropertyChanged(nameof(FileMenu));
                OnPropertyChanged(nameof(ExitAppTip));
                SaveShortcuts(); LoadShortcuts();

                OutputResult("[FileMenu]", "The file menu shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>
    /// “文件(File)”主菜单在 UI 上显示的提示文本（附带当前快捷键）。
    /// </summary>
    public string FileMenuTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem1"];
            return $"{text} ({FileMenu})";
        }
    }

    /// <summary>
    /// “新建(New)”快捷键后备字段（可配置）。
    /// </summary>
    private string _newFileShortcut = "Ctrl+N";

    /// <summary>
    /// “新建(New)”快捷键（更改后触发属性变更与持久化保存）。
    /// </summary>
    public string NewFileShortcut
    {
        get => _newFileShortcut;
        set
        {
            if (_newFileShortcut != value)
            {
                _newFileShortcut = value;
                OnPropertyChanged(nameof(NewFileShortcut));
                OnPropertyChanged(nameof(ExitAppTip));
                SaveShortcuts();

                OutputResult("[NewFileShortcut]", "The new file shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>
    /// “新建(New)”菜单项在 UI 上显示的提示文本（附带当前快捷键）。
    /// </summary>
    public string NewFileTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem111"];
            return $"{text} ({NewFileShortcut})";
        }
    }

    /// <summary>
    /// “打开文件(Open)”快捷键后备字段（可配置）。
    /// </summary>
    private string _openFileShortcut = "Ctrl+O";

    public string OpenFileShortcut
    {
        get => _openFileShortcut;
        set
        {
            if (_openFileShortcut != value)
            {
                _openFileShortcut = value;
                OnPropertyChanged(nameof(OpenFileShortcut));
                OnPropertyChanged(nameof(ExitAppTip));
                SaveShortcuts();

                OutputResult("[OpenFileShortcut]", "The open file shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }
    /// <summary>“打开文件”菜单提示文本（含快捷键）。</summary>
    public string OpenFileTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem112"];
            return $"{text} ({OpenFileShortcut})";
        }
    }

    /// <summary>“打开文件夹”快捷键的后备字段。</summary>
    private string _openFolderShortcut = "Ctrl+Shift+O";
    /// <summary>“打开文件夹”快捷键（可配置）。</summary>
    public string OpenFolderShortcut
    {
        get => _openFolderShortcut;
        set
        {
            if (_openFolderShortcut != value)
            {
                _openFolderShortcut = value;
                OnPropertyChanged(nameof(OpenFolderShortcut));
                OnPropertyChanged(nameof(ExitAppTip));
                SaveShortcuts();

                OutputResult("[OpenFolderShortcut]", "The open folder shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }
    /// <summary>“打开文件夹”菜单提示文本（含快捷键）。</summary>
    public string OpenFolderTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem121"];
            return $"{text} ({OpenFolderShortcut})";
        }
    }

    /// <summary>“打开默认目录”快捷键后备字段。</summary>
    private string _defaultFolderShortcut = "Ctrl+D";
    /// <summary>“打开默认目录”快捷键（可配置）。</summary>
    public string DefaultFolderShortcut
    {
        get => _defaultFolderShortcut;
        set
        {
            if (_defaultFolderShortcut != value)
            {
                _defaultFolderShortcut = value;
                OnPropertyChanged(nameof(DefaultFolderShortcut));
                OnPropertyChanged(nameof(ExitAppTip));
                SaveShortcuts();

                OutputResult("[DefaultFolderShortcut]", "The default folder shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }
    /// <summary>“打开默认目录”菜单提示文本（含快捷键）。</summary>
    public string DefaultFolderTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem122"];
            return $"{text} ({DefaultFolderShortcut})";
        }
    }

    /// <summary>“保存”快捷键后备字段。</summary>
    private string _saveFileShortcut = "Ctrl+S";
    /// <summary>“保存”快捷键（可配置）。</summary>
    public string SaveFileShortcut
    {
        get => _saveFileShortcut;
        set
        {
            if (_saveFileShortcut != value)
            {
                _saveFileShortcut = value;
                OnPropertyChanged(nameof(SaveFileShortcut));
                OnPropertyChanged(nameof(ExitAppTip));
                SaveShortcuts();

                OutputResult("[SaveFileShortcut]", "The save file shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }
    /// <summary>“保存”菜单提示文本（含快捷键）。</summary>
    public string SaveFileTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem131"];
            return $"{text} ({SaveFileShortcut})";
        }
    }

    /// <summary>“另存为”快捷键后备字段。</summary>
    private string _saveAsShortcut = "Ctrl+Shift+S";
    /// <summary>“另存为”快捷键（可配置）。</summary>
    public string SaveAsShortcut
    {
        get => _saveAsShortcut;
        set
        {
            if (_saveAsShortcut != value)
            {
                _saveAsShortcut = value;
                OnPropertyChanged(nameof(SaveAsShortcut));
                OnPropertyChanged(nameof(ExitAppTip));
                SaveShortcuts();

                OutputResult("[SaveAsShortcut]", "The save as shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }
    /// <summary>“另存为”菜单提示文本（含快捷键）。</summary>
    public string SaveAsTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem132"];
            return $"{text} ({SaveAsShortcut})";
        }
    }

    /// <summary>
    /// “全部保存(Save All)”快捷键后备字段（可配置）。
    /// </summary>
    private string _saveAllShortcut = "Ctrl+Alt+S";

    /// <summary>
    /// “全部保存(Save All)”快捷键（变更后会保存到配置并刷新相关提示）。
    /// </summary>
    public string SaveAllShortcut
    {
        get => _saveAllShortcut;
        set
        {
            if (_saveAllShortcut != value)
            {
                _saveAllShortcut = value;
                OnPropertyChanged(nameof(SaveAllShortcut));
                OnPropertyChanged(nameof(ExitAppTip));
                SaveShortcuts();

                OutputResult("[SaveAllShortcut]", "The save all shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>“全部保存”菜单提示文本（含快捷键）。</summary>
    public string SaveAllTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem133"];
            return $"{text} ({SaveAllShortcut})";
        }
    }

    /// <summary>
    /// “关闭当前标签(Close Tab)”快捷键后备字段（可配置）。
    /// </summary>
    private string _closeTabShortcut = "Ctrl+W";

    /// <summary>
    /// “关闭当前标签(Close Tab)”快捷键（变更后会保存到配置并刷新相关提示）。
    /// </summary>
    public string CloseTabShortcut
    {
        get => _closeTabShortcut;
        set
        {
            if (_closeTabShortcut != value)
            {
                _closeTabShortcut = value;
                OnPropertyChanged(nameof(CloseTabShortcut));
                OnPropertyChanged(nameof(ExitAppTip));
                SaveShortcuts();

                OutputResult("[CloseTabShortcut]", "The close tab shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>“关闭当前标签”菜单提示文本（含快捷键）。</summary>
    public string CloseTabTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem141"];
            return $"{text} ({CloseTabShortcut})";
        }
    }

    /// <summary>
    /// “关闭全部标签(Close All Tabs)”快捷键后备字段（可配置）。
    /// </summary>
    private string _closeAllShortcut = "Ctrl+Shift+W";

    /// <summary>
    /// “关闭全部标签(Close All Tabs)”快捷键（变更后会保存到配置并刷新相关提示）。
    /// </summary>
    public string CloseAllShortcut
    {
        get => _closeAllShortcut;
        set
        {
            if (_closeAllShortcut != value)
            {
                _closeAllShortcut = value;
                OnPropertyChanged(nameof(CloseAllShortcut));
                OnPropertyChanged(nameof(ExitAppTip));
                SaveShortcuts();

                OutputResult("[CloseAllShortcut]", "The close all tabs shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>“关闭全部标签”菜单提示文本（含快捷键）。</summary>
    public string CloseAllTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem142"];
            return $"{text} ({CloseAllShortcut})";
        }
    }

    /// <summary>
    /// “退出应用(Exit App)”快捷键后备字段（可配置）。
    /// </summary>
    private string _exitAppShortcut = "Alt+F4";

    /// <summary>
    /// “退出应用(Exit App)”快捷键（变更后会保存到配置并刷新相关提示）。
    /// </summary>
    public string ExitAppShortcut
    {
        get => _exitAppShortcut;
        set
        {
            if (_exitAppShortcut != value)
            {
                _exitAppShortcut = value;
                OnPropertyChanged(nameof(ExitAppShortcut));
                OnPropertyChanged(nameof(ExitAppTip));
                SaveShortcuts();

                OutputResult("[ExitAppShortcut]", "The exit app shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>“退出应用”菜单提示文本（含快捷键）。</summary>
    public string ExitAppTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem143"];
            return $"{text} ({ExitAppShortcut})";
        }
    }
    //==============================
    // 代码菜单（快捷键 & 提示）
    //==============================

    private string _codeMenu = "Ctrl+E";

    /// <summary>“代码”菜单的快捷键（可配置）。</summary>
    public string CodeMenu
    {
        get => _codeMenu;
        set
        {
            if (_codeMenu != value)
            {
                _codeMenu = value;
                OnPropertyChanged(nameof(CodeMenu));
                SaveShortcuts(); LoadShortcuts();
                OutputResult("[CodeMenu]", "The code menu shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>“代码”菜单提示文本（含快捷键）。</summary>
    public string CodeMenuTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem2"];
            return $"{text} ({CodeMenu})";
        }
    }

    //==============================
    // 查找 / 替换 / 格式化代码
    //==============================

    private string _searchTextShortcut = "Ctrl+F";

    /// <summary>“查找(Find)”快捷键（可配置）。</summary>
    public string SearchTextShortcut
    {
        get => _searchTextShortcut;
        set
        {
            if (_searchTextShortcut != value)
            {
                _searchTextShortcut = value;
                OnPropertyChanged(nameof(SearchTextShortcut));
                SaveShortcuts(); LoadShortcuts();
            }
        }
    }

    /// <summary>“查找(Find)”菜单提示文本（含快捷键）。</summary>
    public string SearchTextTip
    {
        get
        {
            var text = Localized["MainWindow.MenuItem211"];
            return $"{text} ({SearchTextShortcut})";
        }
    }

    private string _replaceTextShortcut = "Ctrl+H";

    /// <summary>“替换(Replace)”快捷键（可配置）。</summary>
    public string ReplaceTextShortcut
    {
        get => _replaceTextShortcut;
        set
        {
            if (_replaceTextShortcut != value)
            {
                _replaceTextShortcut = value;
                OnPropertyChanged(nameof(ReplaceTextShortcut));
                SaveShortcuts(); LoadShortcuts();
            }
        }
    }

    /// <summary>“替换(Replace)”菜单提示文本（含快捷键）。</summary>
    public string ReplaceTextTip
    {
        get
        {
            var text = Localized["MainWindow.MenuItem212"];
            return $"{text} ({ReplaceTextShortcut})";
        }
    }

    private string _formatCodeShortcut = "Ctrl+D";
    public string FormatCodeShortcut
    {
        get => _formatCodeShortcut;
        set
        {
            if (_formatCodeShortcut != value)
            {
                _formatCodeShortcut = value;
                OnPropertyChanged(nameof(FormatCodeShortcut));
                SaveShortcuts(); LoadShortcuts();
                OutputResult("[FormatCodeShortcut]", "The format code shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>“格式化代码(Format)”菜单提示文本（含快捷键）。</summary>
    public string FormatCodeTip
    {
        get
        {
            var text = Localized["MainWindow.MenuItem221"];
            return $"{text} ({FormatCodeShortcut})";
        }
    }

    private string _commentSelectionShortcut = "Ctrl+C";

    /// <summary>“注释所选(Comment Selection)”快捷键（可配置）。</summary>
    public string CommentSelectionShortcut
    {
        get => _commentSelectionShortcut;
        set
        {
            if (_commentSelectionShortcut != value)
            {
                _commentSelectionShortcut = value;
                OnPropertyChanged(nameof(CommentSelectionShortcut));
                SaveShortcuts(); LoadShortcuts();
                OutputResult("[CommentSelectionShortcut]", "The comment selection shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>“注释所选(Comment Selection)”菜单提示文本（含快捷键）。</summary>
    public string CommentSelectionTip
    {
        get
        {
            var text = Localized["MainWindow.MenuItem231"];
            return $"{text} ({CommentSelectionShortcut})";
        }
    }

    private string _uncommentSelectionShortcut = "Ctrl+U";

    /// <summary>“取消注释所选(Uncomment Selection)”快捷键（可配置）。</summary>
    public string UncommentSelectionShortcut
    {
        get => _uncommentSelectionShortcut;
        set
        {
            if (_uncommentSelectionShortcut != value)
            {
                _uncommentSelectionShortcut = value;
                OnPropertyChanged(nameof(UncommentSelectionShortcut));
                SaveShortcuts(); LoadShortcuts();
                OutputResult("[UncommentSelectionShortcut]", "The uncomment selection shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>“取消注释所选(Uncomment Selection)”菜单提示文本（含快捷键）。</summary>
    public string UncommentSelectionTip
    {
        get
        {
            var text = Localized["MainWindow.MenuItem232"];
            return $"{text} ({UncommentSelectionShortcut})";
        }
    }

    //==============================
    // 调试 / 运行 / 生成方法
    //==============================

    private string _debugCodeShortcut = "F5";

    /// <summary>“调试(Debug)”快捷键（可配置）。</summary>
    public string DebugCodeShortcut
    {
        get => _debugCodeShortcut;
        set
        {
            if (_debugCodeShortcut != value)
            {
                _debugCodeShortcut = value;
                OnPropertyChanged(nameof(DebugCodeShortcut));
                SaveShortcuts(); LoadShortcuts();
                OutputResult("[DebugCodeShortcut]", "The debug code shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>“调试(Debug)”菜单提示文本（含快捷键）。</summary>
    public string DebugCodeTip
    {
        get
        {
            var text = Localized["MainWindow.MenuItem241"];
            return $"{text} ({DebugCodeShortcut})";
        }
    }

    private string _runCodeShortcut = "Ctrl+F5";
    public string RunCodeShortcut
    {
        get => _runCodeShortcut;
        set
        {
            if (_runCodeShortcut != value)
            {
                _runCodeShortcut = value;
                OnPropertyChanged(nameof(RunCodeShortcut));
                SaveShortcuts(); LoadShortcuts();
                OutputResult("[RunCodeShortcut]", "The run code shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>“运行(Run)”菜单提示文本（含快捷键）。</summary>
    public string RunCodeTip
    {
        get
        {
            var text = Localized["MainWindow.MenuItem242"];
            return $"{text} ({RunCodeShortcut})";
        }
    }

    private string _addClassShortcut = "Ctrl+Shift+A";
    public string AddClassShortcut
    {
        get => _addClassShortcut;
        set
        {
            if (_addClassShortcut != value)
            {
                _addClassShortcut = value;
                OnPropertyChanged(nameof(AddClassShortcut));
                SaveShortcuts(); LoadShortcuts();
            }
        }
    }

    private string _addMethodShortcut = "Ctrl+Shift+M";

    /// <summary>“生成方法(Add Method)”快捷键（可配置）。</summary>
    public string AddMethodShortcut
    {
        get => _addMethodShortcut;
        set
        {
            if (_addMethodShortcut != value)
            {
                _addMethodShortcut = value;
                OnPropertyChanged(nameof(AddMethodShortcut));
                SaveShortcuts(); LoadShortcuts();
            }
        }
    }
    //==============================
    // 输出面板（清空/复制）
    //==============================

    // 输出菜单
    private string _outputMenu = "Ctrl+O";

    /// <summary>“输出(Output)”菜单快捷键（可配置）。</summary>
    public string OutputMenu
    {
        get => _outputMenu;
        set
        {
            if (_outputMenu != value)
            {
                _outputMenu = value;
                OnPropertyChanged(nameof(OutputMenu));
                SaveShortcuts(); LoadShortcuts();
                OutputResult("[OutputMenu]", "The output menu shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>“输出(Output)”菜单提示文本（含快捷键）。</summary>
    public string OutputMenuTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem3"];
            return $"{text} ({OutputMenu})";
        }
    }

    private string _copyMessageShortcut = "Ctrl+C";
    public string CopyMessageShortcut
    {
        get => _copyMessageShortcut;
        set
        {
            if (_copyMessageShortcut != value)
            {
                _copyMessageShortcut = value;
                OnPropertyChanged(nameof(CopyMessageShortcut));
                SaveShortcuts(); LoadShortcuts();
                OutputResult("[CopyMessageShortcut]", "The copy message shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>“复制全部消息(Copy All)”菜单提示文本（含快捷键）。</summary>
    public string CopyMessageTip
    {
        get
        {
            var text = Localized["MainWindow.MenuItem311"];
            return $"{text} ({CopyMessageShortcut})";
        }
    }

    private string _clearMessageShortcut = "Ctrl+L";
    public string ClearMessageShortcut
    {
        get => _clearMessageShortcut;
        set
        {
            if (_clearMessageShortcut != value)
            {
                _clearMessageShortcut = value;
                OnPropertyChanged(nameof(ClearMessageShortcut));
                SaveShortcuts(); LoadShortcuts();
                OutputResult("[ClearMessageShortcut]", "The clear message shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>“清空消息(Clear)”菜单提示文本（含快捷键）。</summary>
    public string ClearMessageTip
    {
        get
        {
            var text = Localized["MainWindow.MenuItem312"];
            return $"{text} ({ClearMessageShortcut})";
        }
    }

    //==============================
    // 图形窗口（最小化 / 全部关闭）
    //==============================

    private string _allFiguresMinimizedShortcut = "Ctrl+M";

    /// <summary>“最小化全部图形窗口”快捷键（可配置）。</summary>
    public string AllFiguresMinimizedShortcut
    {
        get => _allFiguresMinimizedShortcut;
        set
        {
            if (_allFiguresMinimizedShortcut != value)
            {
                _allFiguresMinimizedShortcut = value;
                OnPropertyChanged(nameof(AllFiguresMinimizedShortcut));
                SaveShortcuts(); LoadShortcuts();
            }
        }
    }

    /// <summary>“最小化全部图形窗口”菜单提示文本（含快捷键）。</summary>
    public string AllFiguresMinimizedTip
    {
        get
        {
            var text = Localized["MainWindow.MenuItem321"];
            return $"{text} ({AllFiguresMinimizedShortcut})";
        }
    }

    private string _closeAllFiguresShortcut = "Ctrl+Shift+M";

    /// <summary>“关闭全部图形窗口”快捷键（可配置）。</summary>
    public string CloseAllFiguresShortcut
    {
        get => _closeAllFiguresShortcut;
        set
        {
            if (_closeAllFiguresShortcut != value)
            {
                _closeAllFiguresShortcut = value;
                OnPropertyChanged(nameof(CloseAllFiguresShortcut));
                SaveShortcuts(); LoadShortcuts();
            }
        }
    }

    /// <summary>“关闭全部图形窗口”菜单提示文本（含快捷键）。</summary>
    public string CloseAllFiguresTip
    {
        get
        {
            var text = Localized["MainWindow.MenuItem322"];
            return $"{text} ({CloseAllFiguresShortcut})";
        }
    }
    /// <summary>
    /// “帮助(Help)”主菜单的快捷键后备字段（可配置）。
    /// </summary>
    private string _helpMenu = "F1";

    /// <summary>
    /// “帮助(Help)”主菜单的快捷键（更改后触发属性变更并持久化保存）。 
    /// </summary>
    public string HelpMenu
    {
        get => _helpMenu;
        set
        {
            if (_helpMenu != value)
            {
                _helpMenu = value;
                OnPropertyChanged(nameof(HelpMenu));
                SaveShortcuts(); LoadShortcuts();
                OutputResult("[HelpMenu]", "The help menu shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>
    /// “帮助(Help)”菜单的提示文本（包含当前快捷键）。
    /// </summary>
    public string HelpMenuTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem4"];
            return $"{text} ({HelpMenu})";
        }
    }

    /// <summary>
    /// “快速上手(Get Started)”快捷键后备字段（可配置）。
    /// </summary>
    private string _getStartedShortcut = "Ctrl+G";

    /// <summary>
    /// “快速上手(Get Started)”快捷键（更改后触发属性变更并持久化保存）。
    /// </summary>
    public string GetStartedShortcut
    {
        get => _getStartedShortcut;
        set
        {
            if (_getStartedShortcut != value)
            {
                _getStartedShortcut = value;
                OnPropertyChanged(nameof(GetStartedShortcut));
                SaveShortcuts(); LoadShortcuts();
                OutputResult("[GetStartedShortcut]", "The get started shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>
    /// “快速上手(Get Started)”菜单提示文本（包含当前快捷键）。
    /// </summary>
    public string GetStartedTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem411"];
            return $"{text} ({GetStartedShortcut})";
        }
    }

    /// <summary>
    /// “VEMS 文档”快捷键后备字段（可配置）。
    /// </summary>
    private string _vemsDocsShortcut = "Ctrl+Alt+G";

    /// <summary>
    /// “VEMS 文档”快捷键（更改后触发属性变更并持久化保存）。
    /// </summary>
    public string VEMSDocsShortcut
    {
        get => _vemsDocsShortcut;
        set
        {
            if (_vemsDocsShortcut != value)
            {
                _vemsDocsShortcut = value;
                OnPropertyChanged(nameof(VEMSDocsShortcut));
                SaveShortcuts(); LoadShortcuts();
                OutputResult("[VEMSDocsShortcut]", "The VEMS docs shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>“VEMS 文档”菜单提示文本（含快捷键）。</summary>
    public string VEMSDocsTip
    {
        get
        {
            var text = Localized["MainWindow.MenuItem412"];
            return $"{text} ({VEMSDocsShortcut})";
        }
    }

    private string _linksShortcut = "Ctrl+Shift+G";

    /// <summary>“相关链接(Links)”快捷键（可配置）。</summary>
    public string LinksShortcut
    {
        get => _linksShortcut;
        set
        {
            if (_linksShortcut != value)
            {
                _linksShortcut = value;
                OnPropertyChanged(nameof(LinksShortcut));
                SaveShortcuts(); LoadShortcuts();
                OutputResult("[LinksShortcut]", "The links shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>“相关链接(Links)”菜单提示文本（含快捷键）。</summary>
    public string LinksTip
    {
        get
        {
            var text = Localized["MainWindow.MenuItem413"];
            return $"{text} ({LinksShortcut})";
        }
    }

    private string _helpShortcut = "Ctrl+Shift+H";
    public string HelpShortcut
    {
        get => _helpShortcut;
        set
        {
            if (_helpShortcut != value)
            {
                _helpShortcut = value;
                OnPropertyChanged(nameof(HelpShortcut));
                SaveShortcuts(); LoadShortcuts();
                OutputResult("[HelpShortcut]", "The help shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }

    /// <summary>“帮助(Help)”菜单提示文本（含快捷键）。</summary>
    public string HelpTip
    {
        get
        {
            var text = Localized["MainWindow.MenuItem414"];
            return $"{text} ({HelpShortcut})";
        }
    }
    /// <summary>“许可信息(License Info)”快捷键后备字段。</summary>
    private string _licenseInfoShortcut = "Ctrl+I";
    /// <summary>“许可信息(License Info)”快捷键（可配置）。</summary>
    public string LicenseInfoShortcut
    {
        get => _licenseInfoShortcut;
        set
        {
            if (_licenseInfoShortcut != value)
            {
                _licenseInfoShortcut = value;
                OnPropertyChanged(nameof(LicenseInfoShortcut));
                SaveShortcuts(); LoadShortcuts();
                OutputResult("[LicenseInfoShortcut]", "The license info shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }
    /// <summary>“许可信息(License Info)”菜单提示文本（含快捷键）。</summary>
    public string LicenseInfoTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem421"];
            return $"{text} ({LicenseInfoShortcut})";
        }
    }

    /// <summary>“检查更新(Update App)”快捷键后备字段。</summary>
    private string _updateAppShortcut = "Ctrl+U";
    /// <summary>“检查更新(Update App)”快捷键（可配置）。</summary>
    public string UpdateAppShortcut
    {
        get => _updateAppShortcut;
        set
        {
            if (_updateAppShortcut != value)
            {
                _updateAppShortcut = value;
                OnPropertyChanged(nameof(UpdateAppShortcut));
                SaveShortcuts(); LoadShortcuts();
                OutputResult("[UpdateAppShortcut]", "The update app shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }
    /// <summary>“检查更新(Update App)”菜单提示文本（含快捷键）。</summary>
    public string UpdateAppTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem422"];
            return $"{text} ({UpdateAppShortcut})";
        }
    }

    /// <summary>“关于 VEMS(About)”快捷键后备字段。</summary>
    private string _aboutVEMSShortcut = "Ctrl+A";
    /// <summary>“关于 VEMS(About)”快捷键（可配置）。</summary>
    public string AboutVEMSShortcut
    {
        get => _aboutVEMSShortcut;
        set
        {
            if (_aboutVEMSShortcut != value)
            {
                _aboutVEMSShortcut = value;
                OnPropertyChanged(nameof(AboutVEMSShortcut));
                SaveShortcuts(); LoadShortcuts();
                OutputResult("[AboutVEMSShortcut]", "The about VEMS shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }
    /// <summary>“关于 VEMS(About)”菜单提示文本（含快捷键）。</summary>
    public string AboutVEMSTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem431"];
            return $"{text} ({AboutVEMSShortcut})";
        }
    }

    /// <summary>“联系我们(Contact Us)”快捷键后备字段。</summary>
    private string _contactUSShortcut = "Ctrl+Shift+A";
    /// <summary>“联系我们(Contact Us)”快捷键（可配置）。</summary>
    public string ContactUSShortcut
    {
        get => _contactUSShortcut;
        set
        {
            if (_contactUSShortcut != value)
            {
                _contactUSShortcut = value;
                OnPropertyChanged(nameof(ContactUSShortcut));
                SaveShortcuts(); LoadShortcuts();
                OutputResult("[ContactUSShortcut]", "The contact us shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }
    /// <summary>“联系我们(Contact Us)”菜单提示文本（含快捷键）。</summary>
    public string ContactUSTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem432"];
            return $"{text} ({ContactUSShortcut})";
        }
    }

    /// <summary>“加入(Join)”快捷键后备字段。</summary>
    private string _joinShortcut = "Ctrl+J";
    /// <summary>“加入(Join)”快捷键（可配置）。</summary>
    public string JoinShortcut
    {
        get => _joinShortcut;
        set
        {
            if (_joinShortcut != value)
            {
                _joinShortcut = value;
                OnPropertyChanged(nameof(JoinShortcut));
                SaveShortcuts();
                LoadShortcuts();
                OutputResult("[JoinShortcut]", "The join shortcut has been changed to " + value, "[Info]", null);
            }
        }
    }
    /// <summary>“加入(Join)”菜单提示文本（含快捷键）。</summary>
    public string JoinShortcutTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.MenuItem433"];
            return $"{text} ({JoinShortcut})";
        }
    }

    //==============================
    // 左侧活动栏：资源管理器 / 搜索 / 源代码管理 / 运行调试 / 偏好 / 性能 / 主题 / 语言 / 控制台
    //==============================

    /// <summary>“打开资源管理器”快捷键后备字段。</summary>
    private string _openExplorerShortcut = "Ctrl+Alt+E";
    /// <summary>“打开资源管理器”快捷键（可配置）。</summary>
    public string OpenExplorerShortcut
    {
        get => _openExplorerShortcut;
        set
        {
            if (_openExplorerShortcut != value)
            {
                _openExplorerShortcut = value;
                OnPropertyChanged(nameof(OpenExplorerShortcut));
                SaveShortcuts(); LoadShortcuts();
            }
        }
    }
    /// <summary>“打开资源管理器”菜单提示文本（含快捷键）。</summary>
    public string OpenExplorerTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.leftMenuItem1"];
            return $"{text} ({OpenExplorerShortcut})";
        }
    }

    /// <summary>“打开搜索”快捷键后备字段。</summary>
    private string _openSearchShortcut = "Ctrl+Alt+F";
    /// <summary>“打开搜索”快捷键（可配置）。</summary>
    public string OpenSearchShortcut
    {
        get => _openSearchShortcut;
        set
        {
            if (_openSearchShortcut != value)
            {
                _openSearchShortcut = value;
                OnPropertyChanged(nameof(OpenSearchShortcut));
                SaveShortcuts(); LoadShortcuts();
            }
        }
    }
    /// <summary>“打开搜索”菜单提示文本（含快捷键）。</summary>
    public string OpenSearchTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.leftMenuItem5"];
            return $"{text} ({OpenSearchShortcut})";
        }
    }

    /// <summary>“打开源代码管理”快捷键后备字段。</summary>
    private string _openSourceControlShortcut = "Ctrl+Alt+S";
    /// <summary>“打开源代码管理”快捷键（可配置）。</summary>
    public string OpenSourceControlShortcut
    {
        get => _openSourceControlShortcut;
        set
        {
            if (_openSourceControlShortcut != value)
            {
                _openSourceControlShortcut = value;
                OnPropertyChanged(nameof(OpenSourceControlShortcut));
                SaveShortcuts(); LoadShortcuts();
            }
        }
    }
    /// <summary>“打开源代码管理”菜单提示文本（含快捷键）。</summary>
    public string OpenSourceControlTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.leftMenuItem6"];
            return $"{text} ({OpenSourceControlShortcut})";
        }
    }

    /// <summary>“打开运行与调试”快捷键后备字段。</summary>
    private string _openRunDebugShortcut = "Ctrl+Alt+R";
    /// <summary>“打开运行与调试”快捷键（可配置）。</summary>
    public string OpenRunDebugShortcut
    {
        get => _openRunDebugShortcut;
        set
        {
            if (_openRunDebugShortcut != value)
            {
                _openRunDebugShortcut = value;
                OnPropertyChanged(nameof(OpenRunDebugShortcut));
                SaveShortcuts(); LoadShortcuts();
            }
        }
    }
    /// <summary>“打开运行与调试”菜单提示文本（含快捷键）。</summary>
    public string OpenRunDebugTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.leftMenuItem7"];
            return $"{text} ({OpenRunDebugShortcut})";
        }
    }

    /// <summary>“打开用户偏好(Preferences)”快捷键后备字段。</summary>
    private string _openUserPreferencesShortcut = "Ctrl+Alt+U";
    /// <summary>“打开用户偏好(Preferences)”快捷键（可配置）。</summary>
    public string OpenUserPreferencesShortcut
    {
        get => _openUserPreferencesShortcut;
        set
        {
            if (_openUserPreferencesShortcut != value)
            {
                _openUserPreferencesShortcut = value;
                OnPropertyChanged(nameof(OpenUserPreferencesShortcut));
                SaveShortcuts(); LoadShortcuts();
            }
        }
    }
    /// <summary>“打开用户偏好(Preferences)”菜单提示文本（含快捷键）。</summary>
    public string OpenUserPreferencesTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.leftMenuItem2"];
            return $"{text} ({OpenUserPreferencesShortcut})";
        }
    }

    /// <summary>“打开性能设置(Performance)”快捷键后备字段。</summary>
    // 打开性能设置
    private string _openUserPreformanceShortcut = "Ctrl+Alt+U";
    /// <summary>“打开性能设置(Performance)”快捷键（可配置）。</summary>
    public string OpenUserPreformanceShortcut
    {
        get => _openUserPreformanceShortcut;
        set
        {
            if (_openUserPreformanceShortcut != value)
            {
                _openUserPreformanceShortcut = value;
                OnPropertyChanged(nameof(OpenUserPreformanceShortcut));
                SaveShortcuts(); LoadShortcuts();
            }
        }
    }
    /// <summary>“打开性能设置(Performance)”菜单提示文本（含快捷键）。</summary>
    public string OpenUserPreformanceTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.leftMenuItem3"];
            return $"{text} ({OpenUserPreformanceShortcut})";
        }
    }

    /// <summary>“打开主题(Theme)”快捷键后备字段。</summary>
    // 打开主题
    private string _openThemeShortcut = "Ctrl+Alt+T";
    /// <summary>“打开主题(Theme)”快捷键（可配置）。</summary>
    public string OpenThemeShortcut
    {
        get => _openThemeShortcut;
        set
        {
            if (_openThemeShortcut != value)
            {
                _openThemeShortcut = value;
                OnPropertyChanged(nameof(OpenThemeShortcut));
                SaveShortcuts(); LoadShortcuts();
            }
        }
    }
    /// <summary>“打开主题(Theme)”菜单提示文本（含快捷键）。</summary>
    public string OpenThemeTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.leftMenuItem7"];
            return $"{text} ({OpenThemeShortcut})";
        }
    }

    /// <summary>“打开语言(Language)”快捷键后备字段。</summary>
    // 打开语言
    private string _openLanguageShortcut = "Ctrl+Alt+L";
    /// <summary>“打开语言(Language)”快捷键（可配置）。</summary>
    public string OpenLanguageShortcut
    {
        get => _openLanguageShortcut;
        set
        {
            if (_openLanguageShortcut != value)
            {
                _openLanguageShortcut = value;
                OnPropertyChanged(nameof(OpenLanguageShortcut));
                SaveShortcuts(); LoadShortcuts();
            }
        }
    }
    /// <summary>“打开语言(Language)”菜单提示文本（含快捷键）。</summary>
    public string OpenLanguageTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.leftMenuItem7"];
            return $"{text} ({OpenLanguageShortcut})";
        }
    }
    // 默认路径
    /// <summary>
    /// 应用启动时的默认工作目录路径（用于构建文档树根）。
    /// </summary>
    private string _initialFolderPath = "D:\\VEMSWorkbenchAvalonia\\src\\RoslynPad.Avalonia\\bin\\Debug\\net9.0";

    /// <summary>
    /// 默认工作目录路径的可绑定属性。变更后会触发通知并持久化到快捷配置。
    /// </summary>
    public string InitialFolderPath
    {
        get => _initialFolderPath;
        set
        {
            if (_initialFolderPath != value)
            {
                _initialFolderPath = value;
                OnPropertyChanged(nameof(InitialFolderPath));
                SaveShortcuts();

                // 输出到结果面板，显示新默认路径
                OutputResult("[InitialFolderPath]", "The default path has been changed to " + value, null, null);
            }
        }
    }
    // 初始主题
    /// <summary>
    /// 应用的初始主题标识（与自定义主题名一致，如 "VEMS"）。
    /// </summary>
    private string _initialTheme = "VEMS";

    /// <summary>
    /// 初始主题名称的可绑定属性。变更后触发通知并持久化保存。
    /// </summary>
    public string InitialTheme
    {
        get => _initialTheme;
        set
        {
            if (_initialTheme != value)
            {
                _initialTheme = value;
                OnPropertyChanged(nameof(InitialTheme));
                SaveShortcuts();
            }
        }
    }

    // 初始语言
    /// <summary>
    /// 应用的初始语言/区域代码（例如 "zh-CN"、"en-US"）。
    /// </summary>
    private string _initialLanguage = "en-US";

    /// <summary>
    /// 初始语言的可绑定属性。变更后触发通知并持久化保存，同时打印当前操作系统信息。
    /// </summary>
    public string InitialLanguage
    {
        get => _initialLanguage;
        set
        {
            if (_initialLanguage != value)
            {
                _initialLanguage = value;
                OnPropertyChanged(nameof(InitialLanguage));
                SaveShortcuts();
                Console.WriteLine($"[系统检测] 当前操作系统: {GetCurrentOsPlatform()}");

            }
        }
    }

    /// <summary>
    /// “切换到预设主题”的快捷键后备字段（示例为 Ctrl+NumPad7）。
    /// </summary>
    private string _initialTheme1 = "Ctrl+NumPad7";

    /// <summary>
    /// “切换到预设主题”的快捷键属性。变更后触发通知并持久化保存。
    /// </summary>
    public string InitialTheme1
    {
        get => _initialTheme1;
        set
        {
            if (_initialTheme1 != value)
            {
                _initialTheme1 = value;
                OnPropertyChanged(nameof(InitialTheme1));
                SaveShortcuts();
            }
        }
    }

    /// <summary>
    /// “切换到预设语言”的快捷键后备字段（示例为 Ctrl+NumPad1）。
    /// </summary>
    private string _initialLanguage1 = "Ctrl+NumPad1";

    /// <summary>
    /// “切换到预设语言”的快捷键属性。变更后触发通知并持久化保存，同时打印当前操作系统信息。
    /// </summary>
    public string InitialLanguage1
    {
        get => _initialLanguage1;
        set
        {
            if (_initialLanguage1 != value)
            {
                _initialLanguage1 = value;
                OnPropertyChanged(nameof(InitialLanguage1));
                SaveShortcuts();
                Console.WriteLine($"[系统检测] 当前操作系统: {GetCurrentOsPlatform()}");
            }
        }
    }

    // 其他Tip
    /// <summary>
    /// “设置(Setting)”菜单的提示文本（包含“用户偏好”快捷键）。
    /// </summary>
    public string SettingTip
    {
        get
        {
            // Localized 是你的多语言资源管理器
            var text = Localized["MainWindow.leftMenuItem7"];
            return $"{text} ({OpenUserPreferencesShortcut})";
        }
    }


    /// <summary>
    /// 将当前快捷键与偏好设置序列化为 JSON 并保存到 <c>user_shortcuts.json</c>。
    /// </summary>
    /// <summary>
    /// 执行 Save Shortcuts 操作。
    /// </summary>
    public void SaveShortcuts()
    {
        var shortcutData = new ShortcutData
        {
            OpenUserPreformanceShortcut = OpenUserPreformanceShortcut,
            InitialTheme1 = InitialTheme1,
            InitialLanguage1 = InitialLanguage1,
            EditorFontSize = EditorFontSize,
            OutputFontSize = OutputFontSize,
            EditorFontFamily1 = EditorFontFamily1,
            //SelectedFontFamily = SelectedFontFamily,
            EditorFontFamily = EditorFontFamily,
            InitialFolderPath = InitialFolderPath,
            InitialTheme = InitialTheme,
            InitialLanguage = InitialLanguage,
            FileMenu = FileMenu,
            NewFileShortcut = NewFileShortcut,
            OpenFileShortcut = OpenFileShortcut,
            OpenFolderShortcut = OpenFolderShortcut,
            DefaultFolderShortcut = DefaultFolderShortcut,
            SaveFileShortcut = SaveFileShortcut,
            SaveAsShortcut = SaveAsShortcut,
            SaveAllShortcut = SaveAllShortcut,
            CloseTabShortcut = CloseTabShortcut,
            CloseAllShortcut = CloseAllShortcut,
            ExitAppShortcut = ExitAppShortcut,
            CodeMenu = CodeMenu,
            SearchTextShortcut = SearchTextShortcut,
            ReplaceTextShortcut = ReplaceTextShortcut,
            FormatCodeShortcut = FormatCodeShortcut,
            CommentSelectionShortcut = CommentSelectionShortcut,
            UncommentSelectionShortcut = UncommentSelectionShortcut,
            DebugCodeShortcut = DebugCodeShortcut,
            RunCodeShortcut = RunCodeShortcut,
            AddClassShortcut = AddClassShortcut,
            AddMethodShortcut = AddMethodShortcut,
            OutputMenu = OutputMenu,
            CopyMessageShortcut = CopyMessageShortcut,
            ClearMessageShortcut = ClearMessageShortcut,
            AllFiguresMinimizedShortcut = AllFiguresMinimizedShortcut,
            CloseAllFiguresShortcut = CloseAllFiguresShortcut,
            HelpMenu = HelpMenu,
            GetStartedShortcut = GetStartedShortcut,
            VEMSDocsShortcut = VEMSDocsShortcut,
            LinksShortcut = LinksShortcut,
            HelpShortcut = HelpShortcut,
            LicenseInfoShortcut = LicenseInfoShortcut,
            UpdateAppShortcut = UpdateAppShortcut,
            AboutVEMSShortcut = AboutVEMSShortcut,
            ContactUSShortcut = ContactUSShortcut,
            JoinShortcut = JoinShortcut,
            // 新增功能快捷键
            OpenExplorerShortcut = OpenExplorerShortcut,
            OpenSearchShortcut = OpenSearchShortcut,
            OpenSourceControlShortcut = OpenSourceControlShortcut,
            OpenRunDebugShortcut = OpenRunDebugShortcut,
            OpenUserPreferencesShortcut = OpenUserPreferencesShortcut,
            OpenThemeShortcut = OpenThemeShortcut,
            OpenLanguageShortcut = OpenLanguageShortcut
        };
        var json = JsonSerializer.Serialize(shortcutData, ShortcutJsonOptions);
        File.WriteAllText("user_shortcuts.json", json);
    }

    /// <summary>
    /// JSON 序列化选项：缩进 + 放宽转义（便于中文与符号展示）。
    /// </summary>
    private static readonly JsonSerializerOptions ShortcutJsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// 从 <c>user_shortcuts.json</c> 加载快捷键配置并应用到当前 ViewModel。
    /// </summary>
    /// <summary>
    /// 执行 Load Shortcuts 操作。
    /// </summary>
    public void LoadShortcuts()
    {
        if (File.Exists("user_shortcuts.json"))
        {
            var json = File.ReadAllText("user_shortcuts.json");
            Console.WriteLine("[LoadShortcuts]JSON: " + json);

            try
            {
                var shortcutData = JsonSerializer.Deserialize<ShortcutData>(json);
                if (shortcutData != null)
                {
                    OpenUserPreformanceShortcut = shortcutData.OpenUserPreformanceShortcut;
                    InitialTheme1 = shortcutData.InitialTheme1;
                    InitialLanguage1 = shortcutData.InitialLanguage1;
                    EditorFontSize = shortcutData.EditorFontSize;
                    OutputFontSize = shortcutData.OutputFontSize;
                    //SelectedFontFamily = shortcutData.SelectedFontFamily;
                    EditorFontFamily = shortcutData.EditorFontFamily;
                    EditorFontFamily1 = shortcutData.EditorFontFamily1;

                    InitialFolderPath = shortcutData.InitialFolderPath;
                    InitialFolderPath = shortcutData.InitialFolderPath;
                    InitialTheme = shortcutData.InitialTheme;
                    ApplyTheme(InitialTheme);
                    InitialLanguage = shortcutData.InitialLanguage;
                    ExecuteSwitchLanguage(InitialLanguage);
                    LocalizationService.Default.SelectCulture(InitialLanguage);
                    LocalizationService.Default.SelectCulture("en-US");
                    FileMenu = shortcutData.FileMenu;
                    NewFileShortcut = shortcutData.NewFileShortcut;
                    OpenFileShortcut = shortcutData.OpenFileShortcut;
                    OpenFolderShortcut = shortcutData.OpenFolderShortcut;
                    DefaultFolderShortcut = shortcutData.DefaultFolderShortcut;
                    SaveFileShortcut = shortcutData.SaveFileShortcut;
                    SaveAsShortcut = shortcutData.SaveAsShortcut;
                    SaveAllShortcut = shortcutData.SaveAllShortcut;
                    CloseTabShortcut = shortcutData.CloseTabShortcut;
                    CloseAllShortcut = shortcutData.CloseAllShortcut;
                    ExitAppShortcut = shortcutData.ExitAppShortcut;
                    CodeMenu = shortcutData.CodeMenu;
                    SearchTextShortcut = shortcutData.SearchTextShortcut;
                    ReplaceTextShortcut = shortcutData.ReplaceTextShortcut;
                    FormatCodeShortcut = shortcutData.FormatCodeShortcut;
                    CommentSelectionShortcut = shortcutData.CommentSelectionShortcut;
                    UncommentSelectionShortcut = shortcutData.UncommentSelectionShortcut;
                    DebugCodeShortcut = shortcutData.DebugCodeShortcut;
                    RunCodeShortcut = shortcutData.RunCodeShortcut;
                    AddClassShortcut = shortcutData.AddClassShortcut;
                    AddMethodShortcut = shortcutData.AddMethodShortcut;
                    OutputMenu = shortcutData.OutputMenu;
                    CopyMessageShortcut = shortcutData.CopyMessageShortcut;
                    ClearMessageShortcut = shortcutData.ClearMessageShortcut;
                    AllFiguresMinimizedShortcut = shortcutData.AllFiguresMinimizedShortcut;
                    CloseAllFiguresShortcut = shortcutData.CloseAllFiguresShortcut;
                    HelpMenu = shortcutData.HelpMenu;
                    GetStartedShortcut = shortcutData.GetStartedShortcut;
                    VEMSDocsShortcut = shortcutData.VEMSDocsShortcut;
                    LinksShortcut = shortcutData.LinksShortcut;
                    HelpShortcut = shortcutData.HelpShortcut;
                    LicenseInfoShortcut = shortcutData.LicenseInfoShortcut;
                    UpdateAppShortcut = shortcutData.UpdateAppShortcut;
                    AboutVEMSShortcut = shortcutData.AboutVEMSShortcut;
                    ContactUSShortcut = shortcutData.ContactUSShortcut;
                    JoinShortcut = shortcutData.JoinShortcut;
                    // 新增功能快捷键
                    OpenExplorerShortcut = shortcutData.OpenExplorerShortcut;
                    OpenSearchShortcut = shortcutData.OpenSearchShortcut;
                    OpenSourceControlShortcut = shortcutData.OpenSourceControlShortcut;
                    OpenRunDebugShortcut = shortcutData.OpenRunDebugShortcut;
                    OpenUserPreferencesShortcut = shortcutData.OpenUserPreferencesShortcut;
                    OpenThemeShortcut = shortcutData.OpenThemeShortcut;
                    OpenLanguageShortcut = shortcutData.OpenLanguageShortcut;

                    // 同步下拉字体对象（若系统字体可用）
                    if (SystemFontFamilies != null && SystemFontFamilies.Any())
                    {
                        var fontObj = SystemFontFamilies.FirstOrDefault(f => f.Name == EditorFontFamily);
                        if (fontObj != null)
                            SelectedFontFamilyObject = fontObj;
                    }
                    if (SystemFontFamilies != null && SystemFontFamilies.Any())
                    {
                        var fontObj = SystemFontFamilies.FirstOrDefault(f => f.Name == EditorFontFamily1);
                        if (fontObj != null)
                            SelectedFontFamilyObject1 = fontObj;
                    }

                    Console.WriteLine("[LoadShortcuts] 加载成功，所有快捷键已赋值。");
                }
                else
                {
                    Console.WriteLine("[LoadShortcuts] 反序列化失败：对象为空。");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadShortcuts] 解析失败: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("[LoadShortcuts] 未找到 user_shortcuts.json，使用默认配置。");
        }
    }

    /// <summary>
    /// 另存为当前快捷键配置到指定路径。
    /// </summary>
    /// <param name="exportPath">目标导出路径（*.json）。</param>
    /// <summary>
    /// 执行 Export Shortcuts 操作。
    /// </summary>
    public void ExportShortcuts(string exportPath)
    {
        try
        {
            var shortcutData = new ShortcutData
            {
                OpenUserPreformanceShortcut = OpenUserPreformanceShortcut,
                InitialTheme1 = InitialTheme1,
                InitialLanguage1 = InitialLanguage1,
                EditorFontSize = EditorFontSize,
                OutputFontSize = OutputFontSize,
                EditorFontFamily1 = EditorFontFamily1,
                EditorFontFamily = EditorFontFamily,
                InitialFolderPath = InitialFolderPath,
                InitialTheme = InitialTheme,
                InitialLanguage = InitialLanguage,
                FileMenu = FileMenu,
                NewFileShortcut = NewFileShortcut,
                OpenFileShortcut = OpenFileShortcut,
                OpenFolderShortcut = OpenFolderShortcut,
                DefaultFolderShortcut = DefaultFolderShortcut,
                SaveFileShortcut = SaveFileShortcut,
                SaveAsShortcut = SaveAsShortcut,
                SaveAllShortcut = SaveAllShortcut,
                CloseTabShortcut = CloseTabShortcut,
                CloseAllShortcut = CloseAllShortcut,
                ExitAppShortcut = ExitAppShortcut,
                CodeMenu = CodeMenu,
                SearchTextShortcut = SearchTextShortcut,
                ReplaceTextShortcut = ReplaceTextShortcut,
                FormatCodeShortcut = FormatCodeShortcut,
                CommentSelectionShortcut = CommentSelectionShortcut,
                UncommentSelectionShortcut = UncommentSelectionShortcut,
                DebugCodeShortcut = DebugCodeShortcut,
                RunCodeShortcut = RunCodeShortcut,
                AddClassShortcut = AddClassShortcut,
                AddMethodShortcut = AddMethodShortcut,
                OutputMenu = OutputMenu,
                CopyMessageShortcut = CopyMessageShortcut,
                ClearMessageShortcut = ClearMessageShortcut,
                AllFiguresMinimizedShortcut = AllFiguresMinimizedShortcut,
                CloseAllFiguresShortcut = CloseAllFiguresShortcut,
                HelpMenu = HelpMenu,
                GetStartedShortcut = GetStartedShortcut,
                VEMSDocsShortcut = VEMSDocsShortcut,
                LinksShortcut = LinksShortcut,
                HelpShortcut = HelpShortcut,
                LicenseInfoShortcut = LicenseInfoShortcut,
                UpdateAppShortcut = UpdateAppShortcut,
                AboutVEMSShortcut = AboutVEMSShortcut,
                ContactUSShortcut = ContactUSShortcut,
                JoinShortcut = JoinShortcut,
                OpenExplorerShortcut = OpenExplorerShortcut,
                OpenSearchShortcut = OpenSearchShortcut,
                OpenSourceControlShortcut = OpenSourceControlShortcut,
                OpenRunDebugShortcut = OpenRunDebugShortcut,
                OpenUserPreferencesShortcut = OpenUserPreferencesShortcut,
                OpenThemeShortcut = OpenThemeShortcut,
                OpenLanguageShortcut = OpenLanguageShortcut
            };
            var json = JsonSerializer.Serialize(shortcutData, ShortcutJsonOptions);
            File.WriteAllText(exportPath, json);
            Console.WriteLine($"[ExportShortcuts]  ѵ     : {exportPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExportShortcuts] 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 从指定路径导入快捷键配置（支持 file:// URI）。
    /// </summary>
    /// <param name="importPath">配置文件路径（*.json 或 file://）。</param>
    /// <summary>
    /// 执行 Import Shortcuts 操作。
    /// </summary>
    public void ImportShortcuts(string importPath)
    {
        try
        {
            // 如果是 file:// 形式，转换为本地路径
            if (importPath.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                importPath = new Uri(importPath).LocalPath;
            }
            // 后续逻辑不变
            if (!File.Exists(importPath))
            {
                Console.WriteLine($"[ImportShortcuts] 文件不存在: {importPath}");
                return;
            }
            var json = File.ReadAllText(importPath);
            var shortcutData = JsonSerializer.Deserialize<ShortcutData>(json);
            if (shortcutData != null)
            {
                OpenUserPreformanceShortcut = shortcutData.OpenUserPreformanceShortcut;
                InitialLanguage1 = shortcutData.InitialLanguage1;
                InitialTheme1 = shortcutData.InitialTheme1;
                EditorFontSize = shortcutData.EditorFontSize;
                OutputFontSize = shortcutData.OutputFontSize;
                EditorFontFamily = shortcutData.EditorFontFamily;
                InitialFolderPath = shortcutData.InitialFolderPath;
                InitialTheme = shortcutData.InitialTheme;
                ApplyTheme(InitialTheme);
                InitialLanguage = shortcutData.InitialLanguage;
                ExecuteSwitchLanguage(InitialLanguage);
                FileMenu = shortcutData.FileMenu;
                NewFileShortcut = shortcutData.NewFileShortcut;
                OpenFileShortcut = shortcutData.OpenFileShortcut;
                OpenFolderShortcut = shortcutData.OpenFolderShortcut;
                DefaultFolderShortcut = shortcutData.DefaultFolderShortcut;
                SaveFileShortcut = shortcutData.SaveFileShortcut;
                SaveAsShortcut = shortcutData.SaveAsShortcut;
                SaveAllShortcut = shortcutData.SaveAllShortcut;
                CloseTabShortcut = shortcutData.CloseTabShortcut;
                CloseAllShortcut = shortcutData.CloseAllShortcut;
                ExitAppShortcut = shortcutData.ExitAppShortcut;
                CodeMenu = shortcutData.CodeMenu;
                SearchTextShortcut = shortcutData.SearchTextShortcut;
                ReplaceTextShortcut = shortcutData.ReplaceTextShortcut;
                FormatCodeShortcut = shortcutData.FormatCodeShortcut;
                CommentSelectionShortcut = shortcutData.CommentSelectionShortcut;
                UncommentSelectionShortcut = shortcutData.UncommentSelectionShortcut;
                DebugCodeShortcut = shortcutData.DebugCodeShortcut;
                RunCodeShortcut = shortcutData.RunCodeShortcut;
                AddClassShortcut = shortcutData.AddClassShortcut;
                AddMethodShortcut = shortcutData.AddMethodShortcut;
                OutputMenu = shortcutData.OutputMenu;
                CopyMessageShortcut = shortcutData.CopyMessageShortcut;
                ClearMessageShortcut = shortcutData.ClearMessageShortcut;
                AllFiguresMinimizedShortcut = shortcutData.AllFiguresMinimizedShortcut;
                CloseAllFiguresShortcut = shortcutData.CloseAllFiguresShortcut;
                HelpMenu = shortcutData.HelpMenu;
                GetStartedShortcut = shortcutData.GetStartedShortcut;
                VEMSDocsShortcut = shortcutData.VEMSDocsShortcut;
                LinksShortcut = shortcutData.LinksShortcut;
                HelpShortcut = shortcutData.HelpShortcut;
                LicenseInfoShortcut = shortcutData.LicenseInfoShortcut;
                UpdateAppShortcut = shortcutData.UpdateAppShortcut;
                AboutVEMSShortcut = shortcutData.AboutVEMSShortcut;
                ContactUSShortcut = shortcutData.ContactUSShortcut;
                JoinShortcut = shortcutData.JoinShortcut;
                OpenExplorerShortcut = shortcutData.OpenExplorerShortcut;
                OpenSearchShortcut = shortcutData.OpenSearchShortcut;
                OpenSourceControlShortcut = shortcutData.OpenSourceControlShortcut;
                OpenRunDebugShortcut = shortcutData.OpenRunDebugShortcut;
                OpenUserPreferencesShortcut = shortcutData.OpenUserPreferencesShortcut;
                OpenThemeShortcut = shortcutData.OpenThemeShortcut;
                OpenLanguageShortcut = shortcutData.OpenLanguageShortcut;
                SaveShortcuts(); // 同步保存到主配置
                Console.WriteLine($"[ImportShortcuts] 导入并应用成功: {importPath}");
            }
            else
            {
                Console.WriteLine("[ImportShortcuts] 反序列化失败，shortcutData为null");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ImportShortcuts] 导入失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 通过“另存为”对话框导出快捷键配置为 JSON。
    /// </summary>
    /// <summary>
    /// 执行 Export Shortcuts With Dialog Async 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task ExportShortcutsWithDialogAsync()
    {
        var dialog = _serviceProvider.GetRequiredService<ISaveFileDialog>();
        dialog.Filters = new List<FileDialogFilter>
    {
        new FileDialogFilter("JSON 文件", "json")
    };
        dialog.InitialFileName = "user_shortcuts.json";
        dialog.DefaultExtension = "json";
        dialog.Title = "另存为快捷键配置";
        var exportPath = await dialog.ShowAsync().ConfigureAwait(true);
        if (!string.IsNullOrWhiteSpace(exportPath))
        {
            ExportShortcuts(exportPath);
        }
    }

    /// <summary>
    /// 通过“打开文件”对话框导入快捷键配置 JSON。
    /// </summary>
    /// <summary>
    /// 执行 Import Shortcuts With Dialog Async 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task ImportShortcutsWithDialogAsync()
    {
        var dialog = _serviceProvider.GetRequiredService<IOpenFileDialog>();

        // 仅允许选择 .json 文件
        dialog.Filters = new List<FileDialogFilter>
    {
        new FileDialogFilter("JSON 文件", "json")
    };
        dialog.Title = "导入快捷键配置";
        var files = await dialog.ShowAsync().ConfigureAwait(true);

        Console.WriteLine("[ImportShortcutsWithDialogAsync] 打开文件对话框返回: " + (files == null ? "null" : string.Join(", ", files)));

        if (files != null && files.Length > 0 && !string.IsNullOrWhiteSpace(files[0]))
        {
            Console.WriteLine("[ImportShortcutsWithDialogAsync] 选中文件: " + files[0]);
            ImportShortcuts(files[0]);
        }
        else
        {
            Console.WriteLine("[ImportShortcutsWithDialogAsync] 未选择有效文件，导入取消。");
        }
    }
    /// <summary>
    /// 关闭指定文档的扩展流程（带保存提示）。若 <paramref name="document"/> 为 null，则直接尝试关闭应用。
    /// </summary>
    /// <param name="document">要关闭的文档；为 <c>null</c> 时表示当前无文档（走应用退出流程）。</param>
    /// <summary>
    /// 执行 Close Document1 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task CloseDocument1(OpenDocumentViewModel? document)
    {
        Console.WriteLine($"[CloseDocument1] 正在关闭文档");

        if (document == null)
        {
            // 未传入文档：直接关闭应用
            CloseApp();
            return;
        }

        var exists = document.Document != null;
        var isDirty = document.IsDirty;
        Console.WriteLine($"[CloseDocument1] 正在关闭文档: {document.Title}, exists={exists}, isDirty={isDirty}");

        // 1) 文档已修改：弹出“是否保存”的确认框
        if (isDirty)
        {
            var fileName = document.Title ?? document.Document?.Name ?? "未知文件";
            Console.WriteLine($"[CloseDocument1] 文档 [{fileName}] 已修改，弹出保存提示。");
            var box = MessageBoxManager.GetMessageBoxStandard(
                "警告",
                $"文档 [{fileName}] 已修改，是否保存？",
                ButtonEnum.YesNoCancel,
                Icon.Warning
            );
            // 关联主窗口作为对话框 Owner（如果可用）
            var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
            ButtonResult msgResult;
            if (mainWindow != null)
                msgResult = await box.ShowWindowDialogAsync(mainWindow).ConfigureAwait(true);
            else
                msgResult = await box.ShowAsync().ConfigureAwait(true);

            Console.WriteLine($"[CloseDocument1] 用户选择: {msgResult}");

            if (msgResult == ButtonResult.Yes)
            {
                // 用户选择保存：先保存再关闭
                Console.WriteLine($"[CloseDocument1] 用户选择保存，开始保存文档: {fileName}");
                SaveCurrentDocument();
                if (document.HasDocumentId)
                {
                    RoslynHost?.CloseDocument(document.DocumentId);
                }
                OpenDocuments.Remove(document);
                document.Close();
                Console.WriteLine($"[CloseDocument1] 文档已保存并关闭: {fileName}");
            }
            else if (msgResult == ButtonResult.No)
            {
                // 用户选择不保存：直接关闭
                Console.WriteLine($"[CloseDocument1] 用户选择不保存，直接关闭文档: {fileName}");
                if (document.HasDocumentId)
                {
                    RoslynHost?.CloseDocument(document.DocumentId);
                }
                OpenDocuments.Remove(document);
                document.Close();
                Console.WriteLine($"[CloseDocument1] 文档已关闭: {fileName}");
                CloseApp(); // 关闭完文档后尝试退出应用
            }
            else
            {
                // Cancel：用户取消关闭
                Console.WriteLine($"[CloseDocument1] 用户取消关闭文档: {fileName}");
            }
            return;
        }

        // 2) 文档未修改：直接关闭
        if (!isDirty)
        {
            var fileName = document.Title ?? document.Document?.Name ?? "未知文件";
            Console.WriteLine($"[CloseDocument1] 文档 [{fileName}] 未修改，直接关闭。");
            if (document.HasDocumentId)
            {
                RoslynHost?.CloseDocument(document.DocumentId);
            }
            OpenDocuments.Remove(document);
            document.Close();
            Console.WriteLine($"[CloseDocument1] 文档已关闭: {fileName}");
            CloseApp(); // 关闭完文档后尝试退出应用
            return;
        }
    }

    /// <summary>
    /// 退出应用：对所有打开文档执行 <see cref="CloseDocument1(OpenDocumentViewModel?)"/>。
    /// </summary>
    /// <summary>
    /// 执行 Exit Application 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task ExitApplication()
    {
        Console.WriteLine("[ExitApplication] 开始退出应用，准备关闭所有文档。");
        var openDocs = new ObservableCollection<OpenDocumentViewModel>(OpenDocuments);

        if (openDocs.Count == 0)
        {
            Console.WriteLine("[ExitApplication] 没有打开的文档，调用 CloseDocument1(null)。");
            await CloseDocument1(null).ConfigureAwait(false);
        }
        else
        {
            foreach (var document in openDocs)
            {
                Console.WriteLine($"[ExitApplication] 关闭文档: {document.Title}");
                await CloseDocument1(document).ConfigureAwait(false);
            }
        }

        Console.WriteLine("[ExitApplication] 所有文档已关闭，退出流程结束。");
    }

    /// <summary>
    /// 退出应用的备用实现：先关闭所有文档，再清理临时目录，最后关闭 Avalonia 应用。
    /// </summary>
    /// <summary>
    /// 执行 Exit Application1 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async Task ExitApplication1()
    {
        // 自动保存并关闭所有打开文档
        await CloseAllDocuments().ConfigureAwait(true);

        // 可选：清理临时构建目录
        IOUtilities.PerformIO(() => Directory.Delete(Path.Combine(Path.GetTempPath(), "roslynpad", "build"), recursive: true));

        Console.WriteLine("[ExitApplication] 已保存所有文档并清理临时文件，准备退出应用。");

        // 关闭 Avalonia 应用
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    /// <summary>
    /// 关闭应用：清理临时构建目录并关闭 Avalonia。
    /// </summary>
    private void CloseApp()
    {
        IOUtilities.PerformIO(() => Directory.Delete(Path.Combine(Path.GetTempPath(), "roslynpad", "build"), recursive: true));
        Console.WriteLine("[ExitApplication] 已保存所有文档并清理临时文件，准备退出应用。");
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    // =========================
    // 左侧 SplitView（侧栏）状态
    // =========================

    private bool _isSidebarOpen = true;

    /// <summary>
    /// 侧栏是否打开（影响 <see cref="SidebarOpenLength"/>）。
    /// </summary>
    public bool IsSidebarOpen
    {
        get => _isSidebarOpen;
        set
        {
            if (SetProperty(ref _isSidebarOpen, value))
                OnPropertyChanged(nameof(SidebarOpenLength)); // 依赖属性联动
        }
    }

    private double _sidebarWidth = 350; // 默认宽度

    /// <summary>
    /// 侧栏宽度（像素）。设置时限定到 [180, 800]。
    /// </summary>
    public double SidebarWidth
    {
        get => _sidebarWidth;
        set
        {
            var clamped = Math.Clamp(value, 180, 800); // 限制范围
            if (SetProperty(ref _sidebarWidth, clamped))
                OnPropertyChanged(nameof(SidebarOpenLength)); // 依赖属性联动
        }
    }

    /// <summary>
    /// SplitView 打开状态下的总宽度（= 活动栏 48 + 功能栏宽度）。
    /// </summary>
    /// <summary>
    /// Sidebar Open Length。
    /// </summary>
    public double SidebarOpenLength => 48 + SidebarWidth;
    //// --- 新增：活动栏按钮命令 ---
    //public ICommand ShowExplorerCommand { get; }
    //public ICommand CollapseSidebarCommand { get; }

    // === 本地化包装属性（添加到 MainViewModel 类中） ===
    /// <summary>
    /// LM Left Menu Item1。
    /// </summary>
    public string LM_LeftMenuItem1 => Localized["MainWindow.leftMenuItem41"];
    /// <summary>
    /// LM Left Menu Item2。
    /// </summary>
    public string LM_LeftMenuItem2 => Localized["MainWindow.leftMenuItem42"];
    /// <summary>
    /// LM Left Menu Item3。
    /// </summary>
    public string LM_LeftMenuItem3 => Localized["MainWindow.leftMenuItem43"];
    /// <summary>
    /// LM Left Menu Item4。
    /// </summary>
    public string LM_LeftMenuItem4 => Localized["MainWindow.leftMenuItem44"];
    /// <summary>
    /// LM Left Menu Item5。
    /// </summary>
    public string LM_LeftMenuItem5 => Localized["MainWindow.leftMenuItem51"];
    /// <summary>
    /// LM Left Menu Item6。
    /// </summary>
    public string LM_LeftMenuItem6 => Localized["MainWindow.leftMenuItem52"];
    /// <summary>
    /// LM Left Menu Item7。
    /// </summary>
    public string LM_LeftMenuItem7 => Localized["MainWindow.leftMenuItem53"];
    /// <summary>
    /// LM Left Menu Item8。
    /// </summary>
    public string LM_LeftMenuItem8 => Localized["MainWindow.leftMenuItem54"];
    /// <summary>
    /// LM Left Menu Item9。
    /// </summary>
    public string LM_LeftMenuItem9 => Localized["MainWindow.leftMenuItem55"];
    /// <summary>
    /// LM Left Menu Item10。
    /// </summary>
    public string LM_LeftMenuItem10 => Localized["MainWindow.leftMenuItem56"];
    // 如后续还要别的 key，按同样模式继续加

    /// <summary>
    /// 触发左侧菜单本地化包装属性的 PropertyChanged 通知，
    /// 用于在语言切换后刷新绑定文本。
    /// </summary>
    private void RaiseLocalizationWrappers()
    {
        OnPropertyChanged(nameof(LM_LeftMenuItem1));
        OnPropertyChanged(nameof(LM_LeftMenuItem2));
        OnPropertyChanged(nameof(LM_LeftMenuItem3));
        OnPropertyChanged(nameof(LM_LeftMenuItem4));
        OnPropertyChanged(nameof(LM_LeftMenuItem5));
        OnPropertyChanged(nameof(LM_LeftMenuItem6));
        OnPropertyChanged(nameof(LM_LeftMenuItem7));
        OnPropertyChanged(nameof(LM_LeftMenuItem8));
        OnPropertyChanged(nameof(LM_LeftMenuItem9));
        OnPropertyChanged(nameof(LM_LeftMenuItem10));
        // 继续把新增的包装属性列进来
    }

    #region DllImport

    private string _selectedDllPath = "D:\\WorkspaceByZangDianMin\\VEMS\\VEMS最新\\VEMSWorkbenchAvalonia1014\\src\\RoslynPad.Avalonia\\bin\\Debug\\net9.0\\Avalonia.Controls.ColorPicker.dll";
    private string _dllImportStatus = string.Empty;
    private IBrush _dllImportStatusColor = Brushes.Black;

    #region 本地DLL导入相关属性

    /// <summary>选中的DLL文件路径</summary>
    public string SelectedDllPath
    {
        get => _selectedDllPath;
        set
        {
            if (_selectedDllPath != value)
            {
                _selectedDllPath = value;
                OutputResult("[SelectedDllPath]", "The selected DLL path has been changed to " + value, null, null);
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanImportDll));
            }
        }
    }

    /// <summary>DLL导入状态信息</summary>
    public string DllImportStatus
    {
        get => _dllImportStatus;
        set
        {
            if (_dllImportStatus != value)
            {
                _dllImportStatus = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>DLL导入状态文字颜色</summary>
    public IBrush DllImportStatusColor
    {
        get => _dllImportStatusColor;
        set
        {
            if (_dllImportStatusColor != value)
            {
                _dllImportStatusColor = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>是否可以导入DLL（有有效路径时为true）</summary>
    /// <summary>
    /// 执行 Is Null Or White Space 操作。
    /// </summary>
    public bool CanImportDll => !string.IsNullOrWhiteSpace(SelectedDllPath) && File.Exists(SelectedDllPath);

    #endregion

    #region 事件处理

    /// <summary>导入DLL按钮点击事件</summary>
    /// <summary>
    /// 执行 Import Dll Button Click 操作。
    /// </summary>
    /// <remarks>异步方法。</remarks>
    public async void ImportDllButton_Click()
    {
        await ImportDllAsync().ConfigureAwait(true);
        OutputResult("[ImportDll]", "ImportDllButton_Click executed. Selected DLL path: " + SelectedDllPath, null, null);
    }

    #endregion

    #region 本地DLL导入功能实现



    /// <summary>
    /// 导入选中的DLL文件到当前文档
    /// </summary>
    private async Task ImportDllAsync()
    {
        try
        {
            if (!CanImportDll)
            {
                UpdateImportStatus("Error: No valid DLL file selected", Brushes.Red);
                return;
            }

            var editor = CurrentOpenDocument?.EditorControl as RoslynCodeEditor;
            if (editor == null)
            {
                UpdateImportStatus("Error: No active editor found", Brushes.Red);
                return;
            }

            // 生成DLL引用行，格式与NuGet保持一致
            var referenceText = GenerateDllReferenceText(SelectedDllPath);

            // 插入到文档开头（模仿NuGet包导入的行为）
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                editor.Document.Insert(0, referenceText, AnchorMovementType.Default);
            });

            // 更新状态
            var fileName = Path.GetFileName(SelectedDllPath);
            UpdateImportStatus($"Successfully imported: {fileName}", Brushes.Green);

            // 清空路径，为下次导入做准备
            await Task.Delay(2000).ConfigureAwait(false); // 显示成功信息2秒
            //SelectedDllPath = string.Empty;
            //UpdateImportStatus(string.Empty, Brushes.Black);
        }
        catch (Exception ex)
        {
            UpdateImportStatus($"Error importing DLL: {ex.Message}", Brushes.Red);
        }
    }

    /// <summary>
    /// 生成DLL引用文本，格式类似于NuGet的 #r "nuget: package, version"
    /// </summary>
    /// <param name="dllPath">DLL文件路径</param>
    /// <returns>引用文本行</returns>
    private static string GenerateDllReferenceText(string dllPath)
    {
        // 使用绝对路径确保引用正确
        var absolutePath = Path.GetFullPath(dllPath);

        // 转换为正斜杠，避免转义问题
        var normalizedPath = absolutePath.Replace('\\', '/');

        // 生成引用行，格式：#r "path/to/dll"
        return $"#r \"{normalizedPath}\"{Environment.NewLine}";
    }


    /// <summary>
    /// 更新DLL导入状态显示
    /// </summary>
    /// <param name="message">状态消息</param>
    /// <param name="color">消息颜色</param>
    private void UpdateImportStatus(string message, IBrush color)
    {
        DllImportStatus = message;
        DllImportStatusColor = color;
    }

    #endregion

    #endregion
    /// <summary>
    /// 便捷重载：追加一条记录到【当前文档】或【全局输出】（Header/Value/Type/LineNumber 可选）
    /// </summary>
    /// <summary>
    /// 执行 Output Result 操作。
    /// </summary>
    public void OutputResult(string header, string value, string? type = null, int? lineNumber = null)
    {
        // 在 header 前拼接时间
        var timeStr = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ";
        var ro = new ResultObject
        {
            Header = timeStr + header,
            Value = value,
            Type = type ?? header,
            LineNumber = lineNumber
        };
        OutputResultsToTarget(new[] { ro });
    }

    /// <summary>
    /// 将ResultObject安全追加到【当前文档】或【全局输出】（在UI线程执行）。
    /// </summary>
    private void OutputResultsToTarget(IEnumerable<ResultObject> results)
    {
        if (results == null) return;

        // 优先当前文档，否则GlobalResults
        //var vm = (CurrentOpenDocument as IResultsViewModel) ?? GlobalResults;
        var vm1 = GlobalResults;
        if (vm1 == null)
        {
            // 理论不会发生，兜底：打印到控制台
            foreach (var r in results)
                Console.WriteLine($"[{r?.Header}] {r?.Value} (Type={r?.Type}, Line={r?.LineNumber})");
            return;
        }

        // 线程安全，AddResult通常操作ObservableCollection，还是放UI线程
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                foreach (var r in results)
                {
                    if (r == null) continue;
                    //vm.AddResult(r);
                    vm1.AddResult(r);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OutputResultsToTarget] Exception: {ex}");
            }
        }, DispatcherPriority.Background);
    }
    /// <summary>
    /// Global Results。
    /// </summary>
    public GlobalResultsViewModel GlobalResults { get; }

    public IResultsViewModel ActiveResultsViewModel
        => CurrentOpenDocument as IResultsViewModel ?? GlobalResults;
}



