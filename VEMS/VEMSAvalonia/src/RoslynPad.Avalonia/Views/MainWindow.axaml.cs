using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Composition.Hosting;
using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.PropertyGrid.Services;
using Avalonia.Reactive;
using Avalonia.Styling;
using Avalonia.Threading;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core.Events;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PropertyModels.Localization;
using RoslynPad.FeatureDemos;
using RoslynPad.FeatureDemos.Views;
using RoslynPad.Themes;
using RoslynPad.UI;
using RoslynPad.UI.Utilities;
using RoslynPad.Utilities;
using RoslynPad.ViewModels;
using Semi.Avalonia;
using MsgIcon = MsBox.Avalonia.Enums.Icon;
namespace RoslynPad;

/// <summary>
/// 应用主窗口：
/// - 负责依赖注入容器初始化、ViewModel 绑定、主题切换；
/// - 承载 Dock 文档区与侧边栏显示/折叠；
/// - 管理全局快捷键绑定；
/// - 提供简单的 CPU/RAM 监控 UI 更新；
/// - 统一资源释放与事件解绑，防止内存泄漏。
/// </summary>
/// <remarks>
/// 线程模型：除后台监控（Task.Run）外，其余均在 UI 线程。UI 更新通过 <see cref="Dispatcher.UIThread"/> 进行封送。
/// Dock 集成：通过 <see cref="DocumentsPane"/> 以及工厂方法动态创建与激活标签页。
/// 主题：订阅 <see cref="MainViewModel.ThemeChanged"/>，根据 <see cref="ThemeType"/> 切换 <see cref="Application.RequestedThemeVariant"/>。
/// </remarks>
partial class MainWindow : Window, IDisposable
{
    public ICultureData CurrentCulture
    {
        get => GetValue(CurrentCultureProperty);
        set => SetValue(CurrentCultureProperty, value);
    }

    public static readonly StyledProperty<ICultureData> CurrentCultureProperty =
        AvaloniaProperty.Register<MainWindow, ICultureData>(
            nameof(CurrentCulture),
            defaultValue: LocalizationService.Default.CultureData);

    public ICultureData[] AllCultures => [.. _allCultures];
    private readonly List<ICultureData> _allCultures = [];
    //——— 资源 & 生命周期字段 ———//
    
    private bool _disposed;

    /// <summary>用于保证 <see cref="Dispose(bool)"/> 幂等的互斥锁。</summary>
    private readonly object _disposeLock = new();

    /// <summary>DialogHost 标识符（与 XAML 中定义一致）。</summary>
    public const string DialogHostIdentifier = "Main";

    /// <summary>主 VM，承载文档集合、主题状态、快捷键配置等。</summary>
    private readonly MainViewModel _viewModel = null!;

    /// <summary>当前合并到 Application 资源的主题字典，用于动态替换。</summary>
    private ThemeDictionary? _themeDictionary;

    /// <summary>
    /// 对外公开的主 VM 访问器（便于绑定与测试）。
    /// </summary>
    public MainViewModel ViewModel => _viewModel;

    /// <summary>
    /// 记住上一次的功能区宽度（像素）。收起侧边栏前记录，展开时恢复。
    /// </summary>
    private double _lastSideWidth = 300;

    /// <summary>
    /// 当前 DataContext（若实现了 INotifyPropertyChanged）缓存，用于订阅/退订属性变更。
    /// </summary>
    private INotifyPropertyChanged? _currentVm;

    // 小工具：按索引拿列（1 = 功能区，2 = 分隔条）
    private ColumnDefinition GetSideCol() => MainAreaGrid.ColumnDefinitions[1];
    private ColumnDefinition GetSplitCol() => MainAreaGrid.ColumnDefinitions[2];

    /// <summary>
    /// 构造函数：完成 DI 容器与 ViewModel 创建、主题订阅、快捷键初始化、关键输入事件拦截。
    /// </summary>
    /// <remarks>
    /// - 使用 <see cref="ServiceCollection"/> 与 <see cref="ContainerConfiguration"/> 组合 MEF / DI；
    /// - 通过 <see cref="AddHandler{TEventArgs}(RoutedEvent{TEventArgs}, EventHandler{TEventArgs}?, RoutingStrategies, bool)"/> 拦截单独 Alt；
    /// - 订阅 <see cref="Opened"/> / <see cref="Closed"/> 以管理监控任务。
    /// </remarks>
    public MainWindow()
    {
        // 1) 准备 DI 服务
        var services = new ServiceCollection();

        services.AddLogging(
        #if DEBUG
                l => l.AddDebug()
        #endif
        );

        // 2) 组合容器并导出 ServiceProvider
        var container = new ContainerConfiguration()
            .WithProvider(new ServiceCollectionExportDescriptorProvider(services))
            .WithAssembly(Assembly.GetEntryAssembly());
        var locator = container.CreateContainer().GetExport<IServiceProvider>();

        // 3) 解析 MainViewModel，绑定 DataContext
        _viewModel = locator.GetRequiredService<MainViewModel>();
        _viewModel.OpenDocuments.CollectionChanged += OpenDocuments_CollectionChanged;
        _viewModel.ThemeChanged += OnViewModelThemeChanged;
        //_viewModel.InitializeTheme();

        DataContext = _viewModel;

        // 4) 加载模板与控件树
        InitializeComponent();

        //拦截“单独 Alt”以避免进入菜单助记键模式（Access Keys）
        AddHandler(KeyDownEvent, OnAltSuppressKeyDown, RoutingStrategies.Tunnel);
        AddHandler(KeyUpEvent, OnAltSuppressKeyUp, RoutingStrategies.Tunnel);

        // 5) 初始化窗口字号
        if (_viewModel.Settings.WindowFontSize.HasValue)
        {
            FontSize = _viewModel.Settings.WindowFontSize.Value;
        }
        Opened += OnWindowOpened;
        Closed += OnWindowClosed;

        //ApplyTheme(BuiltInTheme.Light);

        // 6) 初始化快捷键绑定，并在相关属性变化时动态更新
        UpdateKeyBindings(); // 初始化时设置

        // 监听所有快捷键属性变化（集中列举在数组中）
        var shortcutProperties = new[]
        {
            nameof(_viewModel.NewFileShortcut),
            nameof(_viewModel.OpenFileShortcut),
            nameof(_viewModel.OpenFolderShortcut),
            nameof(_viewModel.SaveFileShortcut),
            nameof(_viewModel.SaveAsShortcut),
            nameof(_viewModel.SaveAllShortcut),
            nameof(_viewModel.CloseTabShortcut),
            nameof(_viewModel.CloseAllShortcut),
            nameof(_viewModel.ExitAppShortcut),
            nameof(_viewModel.SearchTextShortcut),
            nameof(_viewModel.ReplaceTextShortcut),
            nameof(_viewModel.FormatCodeShortcut),
            nameof(_viewModel.CommentSelectionShortcut),
            nameof(_viewModel.UncommentSelectionShortcut),
            nameof(_viewModel.DebugCodeShortcut),
            nameof(_viewModel.RunCodeShortcut),
            nameof(_viewModel.CopyMessageShortcut),
            nameof(_viewModel.ClearMessageShortcut),
            //nameof(_viewModel.HelpMenu),
            nameof(_viewModel.HelpShortcut),
            nameof(_viewModel.LicenseInfoShortcut),
            nameof(_viewModel.UpdateAppShortcut),
            nameof(_viewModel.AboutVEMSShortcut),
            nameof(_viewModel.ContactUSShortcut),
            nameof(_viewModel.InitialLanguage),
            nameof(_viewModel.OpenSourceControlShortcut),
            nameof(_viewModel.OpenUserPreferencesShortcut),
            nameof(_viewModel.OpenUserPreformanceShortcut),
            nameof(_viewModel.OpenRunDebugShortcut),
            nameof(_viewModel.OpenExplorerShortcut),
            nameof(_viewModel.OpenSearchShortcut),
            nameof(_viewModel.HelpMenu),
            nameof(_viewModel.OutputMenu),
            nameof(_viewModel.CodeMenu),
            nameof(_viewModel.FileMenu),
            nameof(_viewModel.OpenThemeShortcut),
            nameof(_viewModel.OpenLanguageShortcut),
        };

        _viewModel.PropertyChanged += (s, e) =>
        {
            if (shortcutProperties.Contains(e.PropertyName))
            {
                UpdateKeyBindings();
            }
        };

        // 7) ActivityBar 双击与按下：双通道收起侧边栏（更稳）
        ActivityBar.AddHandler(InputElement.PointerPressedEvent,
            ActivityBar_OnPointerPressed,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
            handledEventsToo: true);

        ActivityBar.AddHandler(Gestures.DoubleTappedEvent,
            ActivityBar_OnDoubleTapped,
            RoutingStrategies.Bubble,
            handledEventsToo: true);

        // 8) 跟踪功能区实际宽度，拖动 GridSplitter 时更新 _lastSideWidth
        SidePaneHost.PropertyChanged += (s, e) =>
        {
            if (e.Property == BoundsProperty)
            {
                var w = SidePaneHost.Bounds.Width;
                if (w > 0)
                    _lastSideWidth = w;
            }
        };

        // 9) DataContext 变化时，重新订阅 VM 的 PropertyChanged（避免事件丢失）
        this.DataContextChanged += (_, __) => WireVmPropertyChanged();
        WireVmPropertyChanged();

        if (_viewModel != null)
        {
            // 创建 SimpleObject1，并把 vm 传进去，保证 _mainViewModel != null
            var demo = new RoslynPad.FeatureDemos.Models.SimpleObject1(_viewModel, "PropertyGrid demo item");

            LocalizationService.Default.SelectCulture(_viewModel.InitialLanguage);
            // 直接把 demo 绑定到 PropertyGrid（或其它 UI 元素）
            //MyPropertyGrid.SelectedObject = demo;

            // 或者把 demo 加入到 RoslynPad 层的集合（比如有个视图层集合）
            // someCollection.Add(demo);
        }

        LocalizationService.Default.AddExtraService(new SampleLocalizationService());//注册实例
        _allCultures.AddRange(LocalizationService.Default.GetCultures());//读取下拉列表

        this.GetObservable(CurrentCultureProperty).Subscribe(new AnonymousObserver<ICultureData>(OnCurrentCultureChanged));
        // 临时代码：放到 MainWindow 构造里或 App.OnFrameworkInitializationCompleted（UI 线程）
        var svc = LocalizationService.Default as Avalonia.PropertyGrid.Localization.AssemblyJsonAssetLocalizationService;
        Console.WriteLine($"[Localization Debug] Default service exists: {svc != null}");
        if (svc != null)
        {
            // 反射或访问内部字段会更难，最好直接实例化一个新的 service 指向预期 assembly 或 uri（下面也给示例）
            // 但我们可以直接打印 GetCultures 结果:
            var cultures = svc.GetCultures();
            Console.WriteLine($"[Localization Debug] GetCultures returned: {cultures?.Length ?? 0}");
#pragma warning disable CS8602 // 解引用可能出现空引用。
            foreach (var c in cultures)
                Console.WriteLine($"[Localization Debug] culture: {c.Culture?.Name} path: {c.Path}");
#pragma warning restore CS8602 // 解引用可能出现空引用。
            
        }
        AllCultures1.Clear();
        foreach (var culture in LocalizationService.Default.GetCultures() ?? Array.Empty<ICultureData>())
            AllCultures1.Add(culture);

        if (_viewModel != null)
        {


            LocalizationService.Default.SelectCulture(_viewModel.InitialLanguage);

        }
    }
    private static void OnCurrentCultureChanged(ICultureData? newCulture)
    {
        if (newCulture != null)
        {
            LocalizationService.Default.SelectCulture(newCulture.Culture.Name);
        }
    }
    public ObservableCollection<ICultureData> AllCultures1 { get; } = new ObservableCollection<ICultureData>();
    /// <summary>
    /// 重新绑定 DataContext 的 PropertyChanged（先退订旧对象，再订阅新对象），防止内存泄漏与事件错绑。
    /// </summary>
    private void WireVmPropertyChanged()
    {
        if (_currentVm is not null)
            _currentVm.PropertyChanged -= VmOnPropertyChanged;

        _currentVm = DataContext as INotifyPropertyChanged;

        if (_currentVm is not null)
            _currentVm.PropertyChanged += VmOnPropertyChanged;
    }

    /// <summary>
    /// 统一处理 VM 的关键属性变化：侧边栏可见性联动、当前文档切换时激活对应标签。
    /// </summary>
    private void VmOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 侧边栏若“任一面板可见”，自动展开侧栏
        if (e.PropertyName is "IsDocumentTreeViewVisible"
                             or "IsUserPreferencesViewVisible"
                             or "IsPerformanceSettingsViewVisible"
                             or "IsDLLExpandViewVisible"
                             or "IsDocumentTreeView4Visible"
                             or "IsRunAndDebugViewVisible"
                             )
        {
            if (IsAnySidePaneVisible())
                ExpandSidebar();
            return;
        }

        // 当前打开文档切换 -> Dock 跳转到对应标签
        if (e.PropertyName == nameof(MainViewModel.CurrentOpenDocument))
        {
            if (DataContext is MainViewModel vm && vm.CurrentOpenDocument is not null)
            {
                ActivateDocumentTabByVm(vm.CurrentOpenDocument);
            }
        }
    }

    /// <summary>
    /// 通过 VM 实例在 Dock 中查找或创建对应的 Document 标签，并置为激活/聚焦。
    /// </summary>
    /// <param name="vm">要激活的文档 VM。</param>
    private void ActivateDocumentTabByVm(OpenDocumentViewModel vm)
    {
        if (DocumentsPane?.Factory is not { } factory || vm is null) return;

        // 先找已存在的标签
        var existing = factory.FindDockable(
            DocumentsPane,
            d => d is Dock.Model.Avalonia.Controls.Document doc &&
                 (ReferenceEquals(doc.DataContext, vm) || doc.Id == vm.Id)
        );

        if (existing is Dock.Model.Avalonia.Controls.Document found)
        {
            // 保险：激活前先把标题更新为 TabTitle（包含 *）
            found.Title = vm.TabTitle;
            factory.SetActiveDockable(found);
            factory.SetFocusedDockable(DocumentsPane, found);
            return;
        }

        // 没找到则创建
        var newDoc = new Dock.Model.Avalonia.Controls.Document
        {
            Id = vm.Id,
            DataContext = vm,
            Title = vm.TabTitle, // ★ 用 TabTitle 初始化
            Content = DocumentsPane.DocumentTemplate?.Content,
            CanClose = true
        };

        // ★ 关键：同时监听 Title / IsDirty 两个属性，一旦变化就同步标签文字为 TabTitle
        void SyncTitleOrDirty(object? s, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(OpenDocumentViewModel.Title) ||
                args.PropertyName == nameof(OpenDocumentViewModel.IsDirty))
            {
                newDoc.Title = vm.TabTitle;
            }
        }
        vm.PropertyChanged += SyncTitleOrDirty;

        factory.AddDockable(DocumentsPane, newDoc);
        factory.SetActiveDockable(newDoc);
        factory.SetFocusedDockable(DocumentsPane, newDoc);
    }

    /// <summary>
    /// 判断侧边栏是否有任一面板处于可见状态（由 VM 多个 bool 控制）。
    /// </summary>
    private bool IsAnySidePaneVisible()
    {
        var vm = DataContext;
        if (vm is null) return false;

        bool GetBool(string name)
        {
            var p = vm.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            var v = p?.GetValue(vm);
            return v is bool b && b;
        }

        return GetBool("IsDocumentTreeViewVisible")
            || GetBool("IsUserPreferencesViewVisible")
            || GetBool("IsPerformanceSettingsViewVisible")
            || GetBool("IsDLLExpandViewVisible")
            || GetBool("IsDocumentTreeView4Visible")
            || GetBool("IsRunAndDebugViewVisible");
    }

    // —— 双击收起：两种通道都收 —— //
    private void ActivityBar_OnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        CollapseSidebar();
    }

    private void ActivityBar_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.ClickCount >= 2)
        {
            e.Handled = true;
            CollapseSidebar();
        }
    }

    /// <summary>
    /// 展开侧边栏：恢复为「上次记录宽度」与可见分隔条。
    /// </summary>
    private void ExpandSidebar()
    {
        var sideCol = GetSideCol();
        var splitCol = GetSplitCol();

        // 已经是展开状态就不重复设置
        if (sideCol.Width.IsAbsolute && sideCol.Width.Value > 0)
            return;

        if (_lastSideWidth <= 0) _lastSideWidth = 300;

        sideCol.Width = new GridLength(_lastSideWidth, GridUnitType.Pixel);
        splitCol.Width = new GridLength(4, GridUnitType.Pixel);
    }

    /// <summary>
    /// 收起侧边栏：记录当前宽度 → 功能区列宽=0、分隔条=0，并将对应 VM 布尔位全部关闭。
    /// </summary>
    private void CollapseSidebar()
    {
        var sideCol = GetSideCol();
        var splitCol = GetSplitCol();

        // 记录当前宽度作为“下次展开”的参考
        var w = SidePaneHost.Bounds.Width;
        if (w > 0) _lastSideWidth = w;

        sideCol.Width = new GridLength(0, GridUnitType.Pixel);
        splitCol.Width = new GridLength(0, GridUnitType.Pixel);

        // 关闭 5 个面板的可见性（VM 已经有这些属性）
        var vm = DataContext;
        if (vm is null) return;

        void SetBool(string name, bool value)
        {
            var p = vm.GetType().GetProperty(name);
            if (p is { CanWrite: true } && p.PropertyType == typeof(bool))
                p.SetValue(vm, value);
        }

        SetBool("IsDocumentTreeViewVisible", false);
        SetBool("IsUserPreferencesViewVisible", false);
        SetBool("IsPerformanceSettingsViewVisible", false);
        SetBool("IsDLLExpandViewVisible", false);
        SetBool("IsDocumentTreeView4Visible", false);
        SetBool("IsRunAndDebugViewVisible", false);
    }

    /// <summary>
    /// 按下事件：仅按下 Alt（无其他修饰/按键）时吞掉，避免进入“Access Keys”助记键模式。
    /// </summary>
    private void OnAltSuppressKeyDown(object? sender, KeyEventArgs e)
    {
        // 只有 Alt 被按下（LeftAlt/RightAlt 等价），且没有其他修饰
        bool onlyAlt =
            (e.Key == Key.LeftAlt || e.Key == Key.RightAlt) &&
            e.KeyModifiers.HasFlag(KeyModifiers.Alt);

        if (onlyAlt)
        {
            e.Handled = true; // ★ 不让系统进入 Access Keys 模式
        }
    }

    /// <summary>
    /// 抬起事件：配套吞掉“仅 Alt”的 KeyUp，避免上层处理为激活菜单。
    /// </summary>
    private void OnAltSuppressKeyUp(object? sender, KeyEventArgs e)
    {
        bool onlyAltUp =
             (e.Key == Key.LeftAlt || e.Key == Key.RightAlt) &&
             e.KeyModifiers == KeyModifiers.None;

        if (onlyAltUp)
        {
            e.Handled = true;
        }
    }

    /// <summary>
    /// Dock 激活项变化时，同步 <see cref="MainViewModel.ActiveContent"/>。
    /// </summary>
    private void OnActiveDockableChanged(object sender, ActiveDockableChangedEventArgs e)
    {
        if (e.Dockable is Document document)
        {
            ViewModel.ActiveContent = document.DataContext;
        }
    }

    /// <summary>
    /// 响应 ViewModel 的主题变化：设置应用主题，并替换主题资源字典。
    /// </summary>
    private void OnViewModelThemeChanged(object? sender, EventArgs e)
    {
        if (Application.Current is not { } app)
        {
            return;
        }

        if (!ViewModel.UseSystemTheme)
        {
            app.RequestedThemeVariant = ViewModel.ThemeType switch
            {
                ThemeType.Light => ThemeVariant.Light,
                ThemeType.Dark => ThemeVariant.Dark,
                _ => null
            };
        }

        if (_themeDictionary is not null)
        {
            app.Resources.MergedDictionaries.Remove(_themeDictionary);
        }

        _themeDictionary = new ThemeDictionary(_viewModel.Theme);
    }

    /// <summary>
    /// Dock 标签关闭时的保存提示流程：如已修改，则先恢复标签再弹出对话框，按选择决定是否最终关闭。
    /// </summary>
    private async void OnDockableClosedAsync(object? sender, DockableClosedEventArgs e)
    {
        if (e.Dockable is not Document document || document.DataContext is not OpenDocumentViewModel vm)
            return;

        if (!vm.IsDirty)
        {
            FinalizeClose(vm);
            return;
        }

        // 关键：先把被 Dock 关闭的标签立刻恢复到界面，再去询问用户（避免 UI 紧张状态不一致）
        RestoreClosedTab(document, vm);

        var fileName = vm.Title ?? vm.Document?.Name ?? "未命名";
        var owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        var box = MessageBoxManager.GetMessageBoxStandard(
            "提示",
            $"文档 [{fileName}] 已修改，是否保存？",
            ButtonEnum.YesNoCancel,
            MsgIcon.Warning
        );

        var result = owner is null
            ? await box.ShowAsync().ConfigureAwait(true)
            : await box.ShowWindowDialogAsync(owner).ConfigureAwait(true);

        if (result == ButtonResult.Yes)
        {
            var save = await vm.SaveAsync(promptSave: false).ConfigureAwait(true);
            if (save == SaveResult.Save)
            {
                FinalizeClose(vm);
            }
            return;
        }
        else if (result == ButtonResult.No)
        {
            FinalizeClose(vm);
            return;
        }
        else
        {
            return;
        }

    }

    /// <summary>
    /// 监听 OpenDocuments 集合变化：为新增文档添加 Dock 标签并激活；移除时清理对应 Dockable。
    /// </summary>
    private void OpenDocuments_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (DocumentsPane.Factory is not { } factory) return;

        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems.OfType<OpenDocumentViewModel>())
            {
                if (factory.FindDockable(DocumentsPane, d => d.Id == item.Id) is { } dockable)
                {
                    factory.RemoveDockable(dockable, collapse: false);
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems.OfType<OpenDocumentViewModel>())
            {
                var document = new Document
                {
                    Id = item.Id,
                    DataContext = item,
                    Content = DocumentsPane.DocumentTemplate?.Content,
                    CanClose = true
                };

                // ★ 绑定到 TabTitle（包含 *），而非 Title
                document.Bind(Document.TitleProperty, new Binding("TabTitle") { Source = item });

                factory.AddDockable(DocumentsPane, document);
                factory.SetActiveDockable(document);
                factory.SetFocusedDockable(DocumentsPane, document);
            }
        }
    }

    /// <inheritdoc/>
    protected override async void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // 初始化 ViewModel（异步），使用 ConfigureAwait(true) 以回到 UI 上下文
        await _viewModel.Initialize().ConfigureAwait(true);
    }

    /// <summary>窗口最小化。</summary>
    private void Minimize_Click(object? sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    /// <summary>窗口最大化/还原切换。</summary>
    private void Maximize_Click(object? sender, RoutedEventArgs e)
    {
        this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    /// <summary>关闭窗口。</summary>
    private void Close_Click(object? sender, RoutedEventArgs? e)
    {
        this.Close();
    }

    /// <summary>
    /// 标题栏按下时支持拖动窗口（仅左键）。
    /// </summary>
    private void TopBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e); // 关键：启动窗口拖动
        }
    }

    /// <summary>
    /// 简单示例方法：返回两数相加的结果（用于说明注释模板）。
    /// </summary>
    /// <param name="a">加数 A。</param>
    /// <param name="b">加数 B。</param>
    /// <returns>A + B 的和。</returns>
    /// <remarks>仅示例用途；与窗口逻辑无关。</remarks>
    public int Calculate(int a, int b)
    {
        return a + b;
    }

    /// <summary>监控任务的取消令牌源；窗口关闭或重新启动监控时取消。</summary>
    private CancellationTokenSource? _monitorCts;

    /// <inheritdoc/>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        StartMonitoring();
    }

    /// <summary>
    /// 启动 CPU/RAM 监控后台任务：周期获取指标并封送回 UI 更新显示。
    /// </summary>
    /// <remarks>
    /// - 先停止可能存在的旧任务以避免重复；<br/>
    /// - 获取指标使用 <see cref="GetSystemMetricsAsync"/>（包含最小 500ms 采样）；<br/>
    /// - UI 更新通过 <see cref="Dispatcher.UIThread.InvokeAsync(Action)"/>，并在取消后双重检查令牌。
    /// </remarks>
    private void StartMonitoring()
    {
        // 先停止任何现有的监控
        StopMonitoring();

        _monitorCts = new CancellationTokenSource();
        var token = _monitorCts.Token;

        // 启动后台监控任务
        _ = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var metrics = await GetSystemMetricsAsync(token).ConfigureAwait(false);

                    // 使用InvokeAsync确保窗口关闭时不会尝试更新UI
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        // 双重检查令牌状态，确保窗口未关闭
                        if (!token.IsCancellationRequested)
                        {
                            UpdatePerformanceUI(metrics);
                        }
                    });

                    // 等待1.5秒，但可被取消
                    await Task.Delay(1500, token).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    // 监控任务被取消，正常退出
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"监控异常: {ex.Message}");
                }
            }
        }, token);
    }

    /// <summary>
    /// 停止监控任务并释放令牌源（幂等）。
    /// </summary>
    private void StopMonitoring()
    {
        var cts = Interlocked.Exchange(ref _monitorCts, null);
        if (cts == null) return;

        try
        {
            cts.Cancel();
            cts.Dispose();
        }
        catch (ObjectDisposedException) { /* 已释放忽略 */ }
    }

    /// <summary>
    /// 采样当前进程 CPU 占用率与工作集内存。
    /// </summary>
    /// <param name="token">取消令牌。</param>
    /// <returns>
    /// 返回一个二元组：(CpuUsage 百分比；MemoryBytes 工作集字节数)。若取消则 CpuUsage=null, MemoryBytes=0。
    /// </returns>
    /// <remarks>
    /// CPU 计算方式：在 <c>500ms</c> 采样窗口内比较 <see cref="Process.TotalProcessorTime"/> 增量 / (逻辑核数 * 窗口时长)。
    /// </remarks>
    public static async Task<(float? CpuUsage, long MemoryBytes)> GetSystemMetricsAsync(
        CancellationToken token = default)
    {
        try
        {
            using var process = Process.GetCurrentProcess();

            // 第一次CPU时间采样
            var cpuTimeStart = process.TotalProcessorTime;
            var stopwatch = Stopwatch.StartNew();

            // 异步等待避免阻塞UI
            await Task.Delay(500, token).ConfigureAwait(false);

            // 第二次采样
            var cpuTimeEnd = process.TotalProcessorTime;
            stopwatch.Stop();

            // 计算CPU利用率
            double cpuTimeUsed = (cpuTimeEnd - cpuTimeStart).TotalMilliseconds;
            double elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            float? cpuUsagePercent = elapsedMilliseconds > 0 ?
                (float)(cpuTimeUsed / (Environment.ProcessorCount * elapsedMilliseconds) * 100) :
                null;

            // 获取当前内存使用
            long memoryBytes = process.WorkingSet64;

            return (cpuUsagePercent, memoryBytes);
        }
        catch (TaskCanceledException)
        {
            return (null, 0); // 任务取消正常返回
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"系统监控失败: {ex.Message}");
            return (null, 0);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 受控释放：停止监控、解绑事件并标记已释放。
    /// </summary>
    /// <param name="disposing">true：显示释放；false：终结器路径。</param>
    protected virtual void Dispose(bool disposing)
    {
        lock (_disposeLock)
        {
            if (_disposed) return;

            if (disposing)
            {
                // 释放托管资源
                StopMonitoring();

                // 解绑事件防止内存泄漏
                _viewModel.ThemeChanged -= OnViewModelThemeChanged;
                _viewModel.OpenDocuments.CollectionChanged -= OpenDocuments_CollectionChanged;
            }

            _disposed = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnClosed(EventArgs e)
    {
        // 调用基类方法前先释放资源
        Dispose(true);
        base.OnClosed(e);
    }

    /// <summary>窗口 Opened 事件：补充启动监控（与 OnOpened 呼应）。</summary>
    private void OnWindowOpened(object? sender, EventArgs e)
    {
        StartMonitoring();
    }

    /// <summary>窗口 Closed 事件：调用 <see cref="Dispose()"/> 确保资源释放。</summary>
    private void OnWindowClosed(object? sender, EventArgs e)
    {
        // 调用 Dispose 确保所有资源释放
        Dispose();
    }

    /// <summary>
    /// 将监控数据渲染到 UI：格式化 CPU（百分比）与内存（自适应单位）。
    /// </summary>
    /// <param name="metrics">CPU/内存指标。</param>
    private void UpdatePerformanceUI((float? CpuUsage, long MemoryBytes) metrics)
    {
        // CPU显示格式化
        CpuText.Text = metrics.CpuUsage.HasValue
            ? $"{metrics.CpuUsage.Value:F1}%"
            : "N/A";

        // 内存显示格式化（自动转换单位）
        string FormatMemory(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB" };
            int unitIndex = 0;
            double size = bytes;

            while (size > 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }
            return $"{size:F1} {units[unitIndex]}";
        }

        MemText.Text = FormatMemory(metrics.MemoryBytes);
    }

    /// <summary>
    /// 根据 ViewModel 当前配置，重建全局快捷键绑定表（先清空再添加）。
    /// </summary>
    private void UpdateKeyBindings()
    {
        // 清除所有旧的 KeyBinding
        KeyBindings.Clear();

        // 文件菜单快捷键
        if (!string.IsNullOrWhiteSpace(_viewModel.FileMenu))
        {
            var gesture = KeyGesture.Parse(_viewModel.FileMenu);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.ShowMenuItem1TabCommand
            });
        }

        // 新建文档快捷键
        if (!string.IsNullOrWhiteSpace(_viewModel.NewFileShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.NewFileShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.NewDocumentCommand
            });
        }

        // 打开文件快捷键
        if (!string.IsNullOrWhiteSpace(_viewModel.OpenFileShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.OpenFileShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.OpenFileCommand
            });
        }

        // 打开文件夹快捷键
        if (!string.IsNullOrWhiteSpace(_viewModel.OpenFolderShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.OpenFolderShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.OpenFolderCommand1
            });
        }

        // 默认文件夹快捷键（如有命令可绑定）
        if (!string.IsNullOrWhiteSpace(_viewModel.DefaultFolderShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.DefaultFolderShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.OpenInitialFolderCommand
            });
        }

        // 保存文件快捷键
        if (!string.IsNullOrWhiteSpace(_viewModel.SaveFileShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.SaveFileShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.SaveCurrentDocumentCommand
            });
        }

        // 另存为快捷键
        if (!string.IsNullOrWhiteSpace(_viewModel.SaveAsShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.SaveAsShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.SaveAsCommand1
            });
        }

        // 全部保存快捷键
        if (!string.IsNullOrWhiteSpace(_viewModel.SaveAllShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.SaveAllShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.SaveAllDocumentsCommand
            });
        }

        // 关闭标签快捷键
        if (!string.IsNullOrWhiteSpace(_viewModel.CloseTabShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.CloseTabShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.CloseCurrentDocumentCommand
            });
        }

        // 关闭全部快捷键
        if (!string.IsNullOrWhiteSpace(_viewModel.CloseAllShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.CloseAllShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.CloseAllDocumentsCommand
            });
        }

        // 退出应用快捷键
        if (!string.IsNullOrWhiteSpace(_viewModel.ExitAppShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.ExitAppShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = new DelegateCommand(() => Close_Click(this, null))
            });
        }

        // 代码菜单快捷键
        if (!string.IsNullOrWhiteSpace(_viewModel.CodeMenu))
        {
            var gesture = KeyGesture.Parse(_viewModel.CodeMenu);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.ShowMenuItem2TabCommand
            });
        }

        // 格式化代码
        if (!string.IsNullOrWhiteSpace(_viewModel.FormatCodeShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.FormatCodeShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.FormatCurrentDocumentCommand
            });
        }

        // 注释选中
        if (!string.IsNullOrWhiteSpace(_viewModel.CommentSelectionShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.CommentSelectionShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.CommentSelectionCommand
            });
        }

        // 取消注释
        if (!string.IsNullOrWhiteSpace(_viewModel.UncommentSelectionShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.UncommentSelectionShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.UncommentSelectionCommand
            });
        }

        // 调试
        if (!string.IsNullOrWhiteSpace(_viewModel.DebugCodeShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.DebugCodeShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.DebugCurrentDocumentCommand
            });
        }

        // 运行
        if (!string.IsNullOrWhiteSpace(_viewModel.RunCodeShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.RunCodeShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.RunCommand
            });
        }

        // 输出菜单
        if (!string.IsNullOrWhiteSpace(_viewModel.OutputMenu))
        {
            var gesture = KeyGesture.Parse(_viewModel.OutputMenu);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.ShowMenuItem3TabCommand
            });
        }

        // 复制消息
        if (!string.IsNullOrWhiteSpace(_viewModel.CopyMessageShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.CopyMessageShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.CopyAllResultsCommand
            });
        }

        // 清空消息
        if (!string.IsNullOrWhiteSpace(_viewModel.ClearMessageShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.ClearMessageShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.ClearResultsCommand
            });
        }

        // 帮助菜单
        if (!string.IsNullOrWhiteSpace(_viewModel.HelpMenu))
        {
            var gesture = KeyGesture.Parse(_viewModel.HelpMenu);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.ShowMenuItem4TabCommand
            });
        }

        // 帮助
        if (!string.IsNullOrWhiteSpace(_viewModel.HelpShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.HelpShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.OpenhelpCommand,
                CommandParameter = "4"
            });
        }

        // 许可证信息
        if (!string.IsNullOrWhiteSpace(_viewModel.LicenseInfoShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.LicenseInfoShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.OpenLicenseCommand
            });
        }

        // 更新应用
        if (!string.IsNullOrWhiteSpace(_viewModel.UpdateAppShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.UpdateAppShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.OpenBaiduCommand
            });
        }

        // 关于 VEMS
        if (!string.IsNullOrWhiteSpace(_viewModel.AboutVEMSShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.AboutVEMSShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.OpenAboutVEMSCommand
            });
        }

        // 联系我们
        if (!string.IsNullOrWhiteSpace(_viewModel.ContactUSShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.ContactUSShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.OpenContactCommand
            });
        }

        // 加入我们（这里用来打开主题菜单）
        if (!string.IsNullOrWhiteSpace(_viewModel.JoinShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.JoinShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = new DelegateCommand(() => OpenThemeMenu())
            });
        }

        // 打开资源管理器（侧边栏）
        if (!string.IsNullOrWhiteSpace(_viewModel.OpenExplorerShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.OpenExplorerShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.ShowDocumentTreeViewCommand
            });
        }

        // 打开搜索
        if (!string.IsNullOrWhiteSpace(_viewModel.OpenSearchShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.OpenSearchShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.ShowUserPreferencesViewCommand
            });
        }

        // 打开源代码管理
        if (!string.IsNullOrWhiteSpace(_viewModel.OpenSourceControlShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.OpenSourceControlShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.ShowPerformanceSettingsViewCommand
            });
        }

        // 打开运行和调试
        if (!string.IsNullOrWhiteSpace(_viewModel.OpenRunDebugShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.OpenRunDebugShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.ShowDLLExpandViewCommand
            });
        }

        // 打开用户习惯
        if (!string.IsNullOrWhiteSpace(_viewModel.OpenUserPreferencesShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.OpenUserPreferencesShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.ShowDocumentTreeView4Command
            });
        }

        // 打开性能设置（复用 2）
        if (!string.IsNullOrWhiteSpace(_viewModel.OpenUserPreformanceShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.OpenUserPreformanceShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = _viewModel.ShowPerformanceSettingsViewCommand
            });
        }

        // 打开主题菜单
        if (!string.IsNullOrWhiteSpace(_viewModel.OpenThemeShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.OpenThemeShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = new DelegateCommand(() => OpenThemeMenu())
            });
        }

        // 打开语言菜单
        if (!string.IsNullOrWhiteSpace(_viewModel.OpenLanguageShortcut))
        {
            var gesture = KeyGesture.Parse(_viewModel.OpenLanguageShortcut);
            KeyBindings.Add(new KeyBinding
            {
                Gesture = gesture,
                Command = new DelegateCommand(() => OpenLanguageMenu())
            });
        }
    }

    //private void OpenDocuments_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    //{
    //    // 旧实现（含 CanClose 委托拦截）已保留在此，若需回滚可恢复
    //}

    /// <summary>
    /// 若 Dock 在关闭回调中移除了标签，这里将其恢复（或新建）并激活，以便提示“是否保存”后保持 UI 一致。
    /// </summary>
    /// <param name="closedDoc">Dock 关闭时的 Document 实例。</param>
    /// <param name="vm">对应文档 VM。</param>
    private void RestoreClosedTab(Document closedDoc, OpenDocumentViewModel vm)
    {
        if (DocumentsPane?.Factory is not { } factory) return;

        // 有的 Dock 版本在 Closed 后无法复用原 Document；尝试复用，不行则新建
        try
        {
            factory.AddDockable(DocumentsPane, closedDoc);
            factory.SetActiveDockable(closedDoc);
            factory.SetFocusedDockable(DocumentsPane, closedDoc);
        }
        catch
        {
            var newDoc = new Document
            {
                Id = vm.Id,
                DataContext = vm,
                Content = DocumentsPane.DocumentTemplate?.Content,
                CanClose = true
            };
            newDoc.Bind(Document.TitleProperty, new Avalonia.Data.Binding("Title") { Source = vm });
            factory.AddDockable(DocumentsPane, newDoc);
            factory.SetActiveDockable(newDoc);
            factory.SetFocusedDockable(DocumentsPane, newDoc);
        }
    }

    /// <summary>
    /// 鼠标拖拽右侧细条时实时调整 <see cref="MainViewModel.SidebarWidth"/>，用于绑定驱动布局。
    /// </summary>
    private void SidebarResizeThumb_OnDragDelta(object? sender, Avalonia.Input.VectorEventArgs e)
    {
        // e.Vector.X 为水平位移；向右为正
        ViewModel.SidebarWidth += e.Vector.X;
    }

    /// <summary>
    /// 真正执行文档关闭：尝试关闭 Roslyn 文档，再从 VM 集合移除。
    /// </summary>
    private void FinalizeClose(OpenDocumentViewModel vm)
    {
        try
        {
            if (vm.HasDocumentId)
                _viewModel.RoslynHost?.CloseDocument(vm.DocumentId);
        }
        catch { /* 忽略清理异常，保证 UI 一致性 */ }

        vm.Close();
        _ = _viewModel.OpenDocuments.Remove(vm);
    }

    /// <summary>
    /// 打开主题下拉菜单（用于快捷键/命令触发）。
    /// </summary>
    public void OpenThemeMenu()
    {
        var btn = this.FindControl<Button>("SettingsButton");
        if (btn?.Flyout != null)
        {
            btn.Flyout.ShowAt(btn);
            Console.WriteLine("[ShowSettingsFlyout] 已弹出设置菜单");
        }
        var menuBar = this.FindControl<Menu>("MainMenu");
        menuBar?.Focus();

        var cThemeMenu = this.FindControl<MenuItem>("CThemeMenu");
        if (cThemeMenu != null)
        {
            cThemeMenu.IsSubMenuOpen = true;
            Console.WriteLine("[OpenThemeMenu] 已展开菜单项 C主题");
        }
    }

    /// <summary>
    /// 打开语言下拉菜单（用于快捷键/命令触发）。
    /// </summary>
    public void OpenLanguageMenu()
    {
        var btn = this.FindControl<Button>("SettingsButton");
        if (btn?.Flyout != null)
        {
            btn.Flyout.ShowAt(btn);
            Console.WriteLine("[ShowSettingsFlyout] 已弹出设置菜单");
        }
        var menuBar = this.FindControl<Menu>("MainMenu");
        menuBar?.Focus();

        var CLanguage = this.FindControl<MenuItem>("CLanguage");
        if (CLanguage != null)
        {
            CLanguage.IsSubMenuOpen = true;
            Console.WriteLine("[OpenThemeMenu] 已展开语言项 语言");
        }
    }
}
