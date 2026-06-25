using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using OpticalChainSimulator.Factories;
using OpticalChainSimulator.Services;
using OpticalChainSimulator.Services.SceneService;
using OpticalChainSimulator.ViewModels;
using OpticalChainSimulator.Views.Docks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OpticalChainSimulator.Views;

/// <summary>
/// 应用主窗口类
/// 负责主窗口初始化、ViewModel绑定、布局工厂创建、场景服务注入、场景元素点击事件处理等核心逻辑
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// 快捷获取绑定到主窗口的视图模型（MainWindowViewModel）
    /// 从DataContext转换而来，可能为null
    /// </summary>
    public MainWindowViewModel? Vm => DataContext as MainWindowViewModel;

    /// <summary>
    /// 已附加的视图模型实例（用于事件订阅/取消订阅）
    /// </summary>
    private MainWindowViewModel? _attachedVm;

    /// <summary>
    /// 主视图实例（关联视口文档视图）
    /// </summary>
    private MainView? _mainView;

    /// <summary>
    /// 主窗口构造函数
    /// 初始化控件并监听DataContext变化，完成ViewModel附加
    /// </summary>
    public MainWindow()
    {
        // 初始化窗口XAML资源
        InitializeComponent();

        // 监听DataContext变化，重新附加ViewModel
        DataContextChanged += (_, __) => AttachViewModel();
        // 立即执行ViewModel附加逻辑
        AttachViewModel();
    }

    #region 工程命令绑定必备的窗口操作
    /// <summary>
    /// 窗口加载完成事件处理方法
    /// 核心作用：将主窗口实例传递给ViewModel，支持ViewModel中调用窗口相关操作（如文件对话框）
    /// </summary>
    /// <param name="e">路由事件参数</param>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        // 调用基类的OnLoaded方法，保证窗口生命周期流程完整
        base.OnLoaded(e);

        // 将当前窗口实例赋值给ViewModel的MainWindow属性
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.MainWindow = this;
        }
    }
    #endregion

    /// <summary>
    /// 初始化窗口XAML资源
    /// 封装AvaloniaXamlLoader.Load方法，完成窗口界面元素的加载
    /// </summary>
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    /// <summary>
    /// 附加ViewModel并完成核心初始化
    /// 包括：ViewModel事件订阅、布局工厂创建、场景服务注入、视口事件绑定
    /// </summary>
    private void AttachViewModel()
    {
        // 取消原有ViewModel的事件订阅（避免内存泄漏）
        if (_attachedVm != null)
        {
            _attachedVm.PropertyChanged -= OnVmPropertyChanged;
        }

        // 更新当前附加的ViewModel实例
        _attachedVm = Vm;

        if (_attachedVm != null)
        {
            // 订阅ViewModel的属性变更事件（用于监听SelectedElement变化）
            _attachedVm.PropertyChanged += OnVmPropertyChanged;

            // 1. 创建布局工厂并初始化布局
            var factory = new OpticStudioDockFactory(_attachedVm);
            var layout = factory.CreateLayout();
            factory.InitLayout(layout);

            // 将布局工厂和布局实例赋值给ViewModel
            _attachedVm.Factory = factory;
            _attachedVm.Layout = layout;

            // 调试输出：打印当前元件数量
            System.Diagnostics.Debug.WriteLine($"[MainWindow] AttachViewModel: Elements. Count = {_attachedVm.Elements.Count}");

            // 2. 异步（后台优先级）初始化场景服务和视口事件
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    // 从布局中查找视口文档视图（ViewportDocumentView）
                    var viewportDoc = FindDockable<ViewportDocumentView>(layout);
                    if (viewportDoc != null)
                    {
                        // 获取主视图实例
                        _mainView = viewportDoc.MainViewInstance;
                        if (_mainView != null)
                        {
                            // 核心：注入场景服务（触发场景同步逻辑）
                            _attachedVm.SceneService = new AvaloniaSceneService(_mainView);
                            // 订阅场景对象点击事件
                            _mainView.SceneObjectClicked += OnSceneObjectClicked;

                            // 调试输出：打印场景中反射镜数量
                            System.Diagnostics.Debug.WriteLine($"[MainWindow] SceneService injected.  Scene mirrors count:  {_mainView.Viewport?.Scene.Mirrors.Count ?? 0}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 捕获异常并输出调试日志，避免窗口初始化崩溃
                    System.Diagnostics.Debug.WriteLine($"[MainWindow] AttachViewModel error: {ex}");
                }
            }, DispatcherPriority.Background);
        }
    }

    /// <summary>
    /// 递归查找布局中的指定类型Dockable控件
    /// </summary>
    /// <typeparam name="T">要查找的控件类型（需继承自IDockable）</typeparam>
    /// <param name="root">布局根节点</param>
    /// <returns>找到的控件实例（未找到返回null）</returns>
    private T? FindDockable<T>(IDockable? root) where T : class
    {
        // 直接匹配根节点类型
        if (root is T target) return target;

        // 递归遍历子节点（如果是IDock类型）
        if (root is IDock dock)
        {
            foreach (var child in dock.VisibleDockables ?? Enumerable.Empty<IDockable>())
            {
                var found = FindDockable<T>(child);
                if (found != null) return found;
            }
        }

        // 未找到返回null
        return null;
    }

    /// <summary>
    /// ViewModel属性变更事件处理方法
    /// 核心监听：SelectedElement属性变化，同步高亮视口中对应的元件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">属性变更参数</param>
    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 仅处理SelectedElement属性变更
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedElement))
        {
            var vm = Vm;
            // 存在选中元件且主视图实例有效时，执行高亮操作
            if (vm?.SelectedElement != null && _mainView != null)
            {
                // 异步（后台优先级）执行高亮，避免阻塞UI线程
                Dispatcher.UIThread.Post(() =>
                {
                    try { _mainView.HighlightElement(vm.SelectedElement.Id); }
                    catch { } // 捕获异常，避免高亮失败导致程序崩溃
                }, DispatcherPriority.Background);
            }
        }
    }

    /// <summary>
    /// 场景对象点击事件处理方法
    /// 核心逻辑：同步视口点击的元件到ViewModel的选中状态，并高亮该元件
    /// </summary>
    /// <param name="type">光学元件类型</param>
    /// <param name="sceneIndex">元件在场景中的索引</param>
    /// <param name="elementId">元件唯一标识（GUID）</param>
    private void OnSceneObjectClicked(Models.OpticalElementType type, int sceneIndex, Guid elementId)
    {
        var vm = Vm;
        if (vm == null) return;

        // 异步（后台优先级）处理选中逻辑，避免阻塞视口渲染
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                // 高亮视口中对应的元件（如果元件ID有效）
                if (_mainView != null && elementId != Guid.Empty)
                    _mainView.HighlightElement(elementId);

                // 从ViewModel的元件列表中查找对应ID的元件
                var hit = vm.Elements.FirstOrDefault(x => x.Id == elementId);
                if (hit != null)
                {
                    // 更新ViewModel的选中状态（选中元件+选中索引）
                    vm.SelectedElement = hit;
                    vm.SelectedIndex = vm.Elements.IndexOf(hit);
                }
            }
            catch { } // 捕获异常，避免点击事件处理失败导致程序崩溃
        }, DispatcherPriority.Background);
    }
}