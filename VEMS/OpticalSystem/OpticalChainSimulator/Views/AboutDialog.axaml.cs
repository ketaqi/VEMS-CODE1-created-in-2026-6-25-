using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace OpticalChainSimulator.Views;

/// <summary>
/// 软件关于对话框窗口
/// 核心功能：展示软件的版本、版权等关于信息，提供关闭按钮退出对话框
/// </summary>
public partial class AboutDialog : Window
{
    /// <summary>
    /// 对话框构造函数
    /// 初始化窗口的XAML资源和控件
    /// </summary>
    public AboutDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 初始化窗口的XAML资源
    /// 加载并解析当前窗口的XAML文件，完成控件的创建和绑定
    /// </summary>
    private void InitializeComponent()
        => AvaloniaXamlLoader.Load(this);

    /// <summary>
    /// 关闭按钮点击事件处理方法
    /// 触发窗口关闭操作，退出关于对话框
    /// </summary>
    /// <param name="sender">事件发送者（关闭按钮）</param>
    /// <param name="e">路由事件参数</param>
    private void OnCloseClicked(object? sender, RoutedEventArgs e)
        => Close();
}