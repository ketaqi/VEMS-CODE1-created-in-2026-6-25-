using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using System.Diagnostics;

namespace OpticalChainSimulator.Views.Docks;

/// <summary>
/// 元件库工具视图控件
/// 核心功能：绑定主窗口ViewModel，同步选中的光学元件状态到3D视口（MainView），
/// 实现VM中SelectedElement变化时，视口对应元件自动高亮
/// </summary>
public partial class LibraryToolView : UserControl
{
    /// <summary>
    /// 3D视口核心控件实例（用于执行元件高亮操作）
    /// </summary>
    private MainView _mainView;

    /// <summary>
    /// 构造函数：初始化控件资源，订阅数据上下文变化事件
    /// </summary>
    public LibraryToolView()
    {
        // 初始化控件XAML资源（Avalonia控件必备）
        InitializeComponent();

        // 订阅数据上下文变化事件：当DataContext切换为MainWindowViewModel时执行绑定逻辑
        this.DataContextChanged += MainWindow_DataContextChanged;
    }

    /// <summary>
    /// 数据上下文变化事件处理方法
    /// 仅当上下文为MainWindowViewModel时，执行ViewModel绑定逻辑
    /// </summary>
    /// <param name="sender">事件发送者（当前UserControl）</param>
    /// <param name="e">事件参数</param>
    private void MainWindow_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            AttachViewModel(vm);
    }

    /// <summary>
    /// 绑定主窗口ViewModel并同步元件高亮状态
    /// 核心逻辑：① 初始同步已选中元件到视口高亮；② 订阅VM选中状态变化，实时同步高亮
    /// </summary>
    /// <param name="vm">主窗口ViewModel实例</param>
    private void AttachViewModel(MainWindowViewModel vm)
    {
        // 1. 初始同步：将VM中已选中的元件ID同步到视口，执行高亮（安全调用，避免空引用）
        var sel = vm.SelectedElement;
        // 获取选中元件的唯一标识（无选中时为空Guid）
        Guid id = sel?.ToModel().Id ?? Guid.Empty;

        if (_mainView != null)
        {
            // 视口控件就绪时，直接高亮对应元件
            _mainView.HighlightElement(id);
        }
        else
        {
            // 视口控件未就绪时，输出调试日志，延迟高亮
            Debug.WriteLine("AttachViewModel: MainViewControl not found - highlight deferred");
        }

        // 2. 订阅VM的PropertyChanged事件：实时同步选中元件高亮状态
        vm.PropertyChanged += (s, e) =>
        {
            // 仅监听SelectedElement属性变化（避免无关属性触发）
            if (e.PropertyName == nameof(vm.SelectedElement))
            {
                // 切换到UI线程执行：Avalonia控件操作必须在UI线程，避免跨线程异常
                Dispatcher.UIThread.Post(() =>
                {
                    // 获取最新选中的元件及其ID
                    var selected = vm.SelectedElement;
                    var selectedId = selected?.ToModel().Id ?? Guid.Empty;

                    if (_mainView != null)
                    {
                        // 视口控件就绪时，高亮最新选中的元件
                        _mainView.HighlightElement(selectedId);
                    }
                });
            }
        };
    }

    /// <summary>
    /// 初始化控件XAML资源（Avalonia框架要求，加载并解析XAML文件）
    /// </summary>
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}