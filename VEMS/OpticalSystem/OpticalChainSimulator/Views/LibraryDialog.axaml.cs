using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using OpticalChainSimulator.yuanjianku;
using System;
using System.Diagnostics;

namespace OpticalChainSimulator.Views;

/// <summary>
/// 元件库对话框窗口类
/// 用于展示光学元件选择界面，支持用户选择不同类型元件（反射镜、透镜、探测器、光源等）或取消操作，
/// 通过与MainWindowViewModel交互实现窗口关闭和选择结果的返回
/// </summary>
public partial class LibraryDialog : Window
{
    /// <summary>
    /// 用户选择的操作标识
    /// 可选值："Mirror"（反射镜）、"Lens"（透镜）、null（取消选择）
    /// </summary>
    public string? SelectedAction { get; private set; }

    /// <summary>
    /// 主窗口视图模型实例（用于订阅关闭请求事件）
    /// </summary>
    private readonly MainWindowViewModel? _vm;

    /// <summary>
    /// 元件库对话框构造函数
    /// </summary>
    /// <param name="viewModel">主窗口视图模型实例（不能为空）</param>
    /// <exception cref="ArgumentNullException">当viewModel为null时抛出</exception>
    public LibraryDialog(MainWindowViewModel viewModel)
    {
        // 初始化对话框XAML资源
        InitializeComponent();

        // 设置数据上下文为主窗口ViewModel
        this.DataContext = viewModel;

        // 绑定取消按钮点击事件
        //BtnCancel.Click += OnCancelClicked;

        // 初始化主窗口ViewModel引用（做null校验，避免空引用）
        _vm = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        // 冗余保护：再次设置数据上下文（防止OpenLibraryDialogAsync未设置的情况）
        this.DataContext = viewModel;

        // 订阅ViewModel的RequestClose事件，响应VM发起的窗口关闭请求
        _vm.RequestClose += Vm_RequestClose;

        // 窗口关闭时取消事件订阅，避免内存泄漏
        this.Closed += (_, __) =>
        {
            if (_vm != null)
                _vm.RequestClose -= Vm_RequestClose;
        };
    }

    /// <summary>
    /// 响应ViewModel的关闭请求事件
    /// 接收选中的光学元件实例，关闭窗口并返回该实例
    /// </summary>
    /// <param name="result">选中的光学元件实例（可为null）</param>
    private void Vm_RequestClose(OpticalElement1? result)
    {
        // 在UI线程中执行窗口关闭操作，捕获异常避免崩溃
        try
        {
            // 转换元件类型为字符串（仅调试用途，未实际使用）
            string a = result.Type.ToString();
            // 关闭窗口并返回元件实例（Close泛型类型需与ShowDialog调用匹配）
            this.Close(result);
        }
        catch (Exception ex)
        {
            // 输出异常调试日志
            Debug.WriteLine($"Error closing LibraryDialog: {ex}");
        }
    }

    /// <summary>
    /// 取消按钮点击事件处理方法
    /// 重置选中操作标识为null，关闭窗口并返回null
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">路由事件参数</param>
    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        // 重置选中操作标识
        SelectedAction = null;
        // 关闭窗口并返回null（表示用户取消选择）
        Close(null);
    }

    /// <summary>
    /// 初始化对话框XAML资源
    /// </summary>
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}