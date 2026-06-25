using Avalonia.Controls;
using Avalonia.Threading;
using OpticalChainSimulator.Services.SceneService;
using OpticalChainSimulator.ViewModels.Docks;

namespace OpticalChainSimulator.Views.Docks;

/// <summary>
/// 视口文档视图控件
/// 核心功能：绑定3D视口控件（MainView），处理视口内光学元件的单击/双击事件，
/// 同步视口选中状态到主窗口ViewModel，实现元件高亮、列表选中/展开等交互联动
/// </summary>
public partial class ViewportDocumentView : UserControl
{
    /// <summary>
    /// 主窗口ViewModel实例（用于数据绑定和状态同步）
    /// </summary>
    private MainWindowViewModel _vm;

    /// <summary>
    /// 3D视口核心控件实例（用于元素高亮、事件订阅等操作）
    /// </summary>
    private MainView _mainView;

    /// <summary>
    /// 构造函数：初始化控件、订阅事件、绑定ViewModel、查找3D视口控件
    /// </summary>
    public ViewportDocumentView()
    {
        // 初始化控件XAML资源
        InitializeComponent();

        // 订阅数据上下文变化事件，用于绑定主窗口ViewModel
        this.DataContextChanged += MainWindow_DataContextChanged;

        // 初始化主窗口ViewModel（参数null为兼容构造）
        _vm = new MainWindowViewModel(null);
        DataContext = _vm;

        // 查找XAML中命名为MainViewControl的3D视口控件
        _mainView = this.FindControl<MainView>("MainViewControl");
        var mv = this.FindControl<MainView>("MainViewControl");

        // 3D视口控件存在时，订阅其交互事件
        if (mv != null)
        {
            // 注释：原元素创建事件（暂未启用）
            //mv.ElementCreated += OnViewElementCreated;

            // 订阅视口对象单击事件：同步选中状态到VM并高亮
            mv.SceneObjectClicked += OnSceneObjectClicked;

            // 订阅视口对象双击事件：测试新增的双击交互逻辑
            mv.SceneObjectDoubleClicked += OnSceneObjectDoubleClicked;
        }
    }

    /// <summary>
    /// 对外暴露3D视口控件实例，供外部访问
    /// </summary>
    public MainView? MainViewInstance => this.FindControl<MainView>("MainViewControl");

    /// <summary>
    /// 视觉树附加事件：控件加入可视化树后，尝试绑定3D视口到主窗口ViewModel
    /// （确保控件完全加载后再执行绑定，避免查找不到控件/VM）
    /// </summary>
    /// <param name="e">视觉树附加事件参数</param>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // 执行3D视口与主VM的绑定逻辑
        TryBindMainViewToMainWindowVm();
    }

    /// <summary>
    /// 核心绑定方法：将3D视口控件的DataContext绑定到主窗口ViewModel
    /// 支持延迟重试机制，确保控件/VM就绪后再绑定
    /// </summary>
    private void TryBindMainViewToMainWindowVm()
    {
        // 查找内部的3D视口控件
        var mainView = this.FindControl<MainView>("MainViewControl");
        if (mainView == null)
        {
            // 控件未就绪：延迟一帧（Loaded优先级）重试
            Avalonia.Threading.Dispatcher.UIThread.Post(TryBindMainViewToMainWindowVm, Avalonia.Threading.DispatcherPriority.Loaded);
            return;
        }

        // 查找视觉树根节点（宿主Window），获取主窗口ViewModel
        var root = this.GetVisualRoot() as Window;
        if (root?.DataContext is MainWindowViewModel mainVm)
        {
            // 仅绑定3D视口的DataContext（不改变当前UserControl的DataContext）
            if (mainView.DataContext != mainVm)
            {
                mainView.DataContext = mainVm;

                // 可选：主VM的SceneService为空时，初始化场景服务（关联3D视口）
                if (mainVm.SceneService == null)
                {
                    mainVm.SceneService = new AvaloniaSceneService(mainView);
                }
            }
        }
        else
        {
            // 主VM未就绪：延迟一帧（Loaded优先级）重试
            Avalonia.Threading.Dispatcher.UIThread.Post(TryBindMainViewToMainWindowVm, Avalonia.Threading.DispatcherPriority.Loaded);
        }
    }

    #region 单击/双击交互：选中高亮 + 状态同步到VM
    /// <summary>
    /// 获取当前数据上下文对应的主窗口ViewModel（快捷属性）
    /// </summary>
    private MainWindowViewModel? Vm => DataContext as MainWindowViewModel;

    /// <summary>
    /// 视口对象单击事件处理方法
    /// 核心逻辑：① 立即高亮视口中的选中元素（视觉反馈）；② 同步选中状态到VM，展开元素并滚动列表
    /// </summary>
    /// <param name="type">选中元素的类型（反射镜/透镜/探测器等）</param>
    /// <param name="sceneIndex">元素在场景中的索引</param>
    /// <param name="elementId">元素唯一标识（Guid）</param>
    private void OnSceneObjectClicked(OpticalElementType type, int sceneIndex, Guid elementId)
    {
        System.Diagnostics.Debug.WriteLine($"OnSceneObjectClicked: type={type} sceneIndex={sceneIndex} elementId={elementId}");

        // 第一步：UI线程立即执行视觉反馈（高亮元素），提升用户体验
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                // 获取3D视口控件实例（兜底查找）
                var mv = _mainView ?? this.FindControl<MainView>("MainViewControl");
                if (mv != null)
                {
                    // 有元素ID时，直接高亮对应元素
                    if (elementId != Guid.Empty)
                    {
                        mv.HighlightElement(elementId);
                        System.Diagnostics.Debug.WriteLine($"OnSceneObjectClicked: highlighted elementId={elementId} in MainView");
                    }
                    else
                    {
                        // 无元素ID时：按类型+场景索引回退查找，高亮对应元素（尽力策略）
                        var sc = mv.Viewport?.Scene;
                        Guid fallbackId = Guid.Empty;
                        if (sc != null)
                        {
                            switch (type)
                            {
                                case OpticalElementType.Mirror:
                                    if (sceneIndex >= 0 && sceneIndex < sc.Mirrors.Count) fallbackId = sc.Mirrors[sceneIndex].ElementId;
                                    break;
                                case OpticalElementType.Lens:
                                    if (sceneIndex >= 0 && sceneIndex < sc.Lenses.Count) fallbackId = sc.Lenses[sceneIndex].ElementId;
                                    break;
                                case OpticalElementType.Detector:
                                    if (sceneIndex >= 0 && sceneIndex < sc.Detectors.Count) fallbackId = sc.Detectors[sceneIndex].ElementId;
                                    break;
                                case OpticalElementType.Box:
                                    if (sceneIndex >= 0 && sceneIndex < sc.Boxes.Count) fallbackId = sc.Boxes[sceneIndex].ElementId;
                                    break;
                                case OpticalElementType.PortBox:
                                    if (sceneIndex >= 0 && sceneIndex < sc.PortBoxes.Count) fallbackId = sc.PortBoxes[sceneIndex].ElementId;
                                    break;
                            }
                        }
                        // 找到回退ID时，执行高亮
                        if (fallbackId != Guid.Empty)
                        {
                            mv.HighlightElement(fallbackId);
                            System.Diagnostics.Debug.WriteLine($"OnSceneObjectClicked: fallback highlighted id={fallbackId}");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("OnSceneObjectClicked: MainView not found for highlighting");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("OnSceneObjectClicked (highlight) error: " + ex);
            }
        });

        // 第二步：同步选中状态到ViewModel（VM就绪时执行）
        if (Vm == null)
        {
            System.Diagnostics.Debug.WriteLine("OnSceneObjectClicked: Vm == null, skipping VM selection update");
            return;
        }

        OpticalElementViewModel? match = null;
        int matchIndex = -1;

        // 优先按元素ID匹配VM中的元件
        if (elementId != Guid.Empty)
        {
            for (int i = 0; i < Vm.Elements.Count; i++)
            {
                if (Vm.Elements[i].Id == elementId)
                {
                    match = Vm.Elements[i];
                    matchIndex = i;
                    break;
                }
            }
        }

        // 退化策略：无ID/未匹配到时，按元素类型匹配第一个
        if (match == null)
        {
            for (int i = 0; i < Vm.Elements.Count; i++)
            {
                if (Vm.Elements[i].Type == type)
                {
                    match = Vm.Elements[i];
                    matchIndex = i;
                    break;
                }
            }
        }

        // 匹配到元件时，更新VM选中状态并展开/滚动列表
        if (match != null)
        {
            Vm.SelectedIndex = matchIndex;
            Vm.SelectedElement = match;
            match.IsExpanded = true; // 展开元件参数面板

            // UI线程滚动列表到选中项，确保用户可见
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    var lb = this.FindControl<ListBox>("ElementsListBox");
                    if (lb != null)
                        lb.ScrollIntoView(match);
                }
                catch { }
            });
        }
    }

    /// <summary>
    /// 视口对象双击事件处理方法（测试新增）
    /// 核心逻辑：高亮双击的元素（扩展交互逻辑）
    /// </summary>
    /// <param name="type">双击元素的类型</param>
    /// <param name="sceneIndex">元素在场景中的索引</param>
    /// <param name="elementId">元素唯一标识</param>
    private void OnSceneObjectDoubleClicked(OpticalElementType type, int sceneIndex, Guid elementId)
    {
        System.Diagnostics.Debug.WriteLine($"这里这里: type={type} sceneIndex={sceneIndex} elementId={elementId}");

        // 获取3D视口控件实例（兜底查找）
        var mv = _mainView ?? this.FindControl<MainView>("MainViewControl");
        if (mv != null)
        {
            // 有元素ID时，调用双击专用的高亮方法
            if (elementId != Guid.Empty)
            {
                mv.HighlightElement1(elementId);
                System.Diagnostics.Debug.WriteLine($"OnSceneObjectDoubleClicked: highlighted elementId={elementId} in MainView");
            }
        }
    }

    /// <summary>
    /// 数据上下文变化事件处理方法
    /// 上下文为MainWindowViewModel时，执行VM绑定逻辑
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void MainWindow_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            AttachViewModel(vm);
    }

    /// <summary>
    /// 绑定ViewModel并同步初始状态
    /// 核心：① 初始选中元素同步高亮；② 订阅VM选中状态变化，实时同步高亮
    /// </summary>
    /// <param name="vm">主窗口ViewModel</param>
    private void AttachViewModel(MainWindowViewModel vm)
    {
        // 初始同步：VM中已选中的元素，在视口中高亮
        var sel = vm.SelectedElement;
        Guid id = sel?.ToModel().Id ?? Guid.Empty;
        if (_mainView != null)
        {
            _mainView.HighlightElement(id);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("AttachViewModel: MainViewControl not found - highlight deferred");
        }

        // 订阅VM的SelectedElement属性变化，实时同步视口高亮
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(vm.SelectedElement))
            {
                Dispatcher.UIThread.Post(() =>
                {
                    var selected = vm.SelectedElement;
                    var selectedId = selected?.ToModel().Id ?? Guid.Empty;
                    if (_mainView != null)
                    {
                        _mainView.HighlightElement(selectedId);
                    }
                });
            }
        };
    }
    #endregion

    /// <summary>
    /// 初始化控件XAML资源（Avalonia必备）
    /// </summary>
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}