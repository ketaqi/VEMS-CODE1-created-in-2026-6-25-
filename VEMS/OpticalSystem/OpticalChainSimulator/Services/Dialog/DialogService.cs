using System;
using System.Threading.Tasks;

namespace OpticalChainSimulator.Services.Dialog;

/// <summary>
/// 对话框服务的具体实现类
/// 实现 IDialogService 接口，提供模态对话框和关于对话框的具体显示逻辑
/// 基于 Avalonia 框架的窗口机制实现对话框的创建与显示
/// </summary>
public class DialogService : IDialogService
{
    /// <summary>
    /// 显示通用模态对话框（泛型版本）
    /// 模态对话框会阻塞当前主窗口交互，直到对话框关闭
    /// </summary>
    /// <typeparam name="TViewModel">对话框绑定的视图模型类型，限制为引用类型</typeparam>
    /// <param name="viewModel">对话框对应的视图模型实例，作为数据上下文绑定到对话框窗口</param>
    /// <returns>异步任务，任务完成表示对话框已关闭</returns>
    /// <exception cref="InvalidOperationException">当无法获取应用主窗口时抛出该异常</exception>
    public async Task ShowDialogAsync<TViewModel>(TViewModel viewModel) where TViewModel : class
    {
        // 获取 Avalonia 应用的桌面程序生命周期对象，用于定位主窗口
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            // 获取应用主窗口作为对话框的所有者窗口
            var mainWindow = desktopLifetime.MainWindow;
            if (mainWindow == null)
            {
                // 主窗口为空时抛出异常，确保对话框有合法的所有者
                throw new InvalidOperationException("无法获取主窗口作为对话框的所有者。");
            }

            // 简化实现：无论传入何种 ViewModel，均创建 AboutDialog 窗口
            var dialog = new AboutDialog
            {
                // 将传入的 ViewModel 绑定到对话框的 DataContext（数据上下文）
                DataContext = viewModel,
                // 设置对话框启动位置为所有者窗口居中
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            // 以模态方式显示对话框，等待对话框关闭后继续执行
            await dialog.ShowDialog(mainWindow);
        }
    }

    /// <summary>
    /// 显示关于对话框（专用版本）
    /// 无需传入 ViewModel，内部自动创建 AboutDialog 及对应的 ViewModel
    /// </summary>
    /// <returns>异步任务，任务完成表示关于对话框已关闭</returns>
    /// <exception cref="InvalidOperationException">当无法获取应用主窗口时抛出该异常</exception>
    public async Task ShowAboutDialogAsync()
    {
        // 获取 Avalonia 应用的桌面程序生命周期对象
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            // 获取应用主窗口作为对话框的所有者窗口
            var mainWindow = desktopLifetime.MainWindow;
            if (mainWindow == null)
                // 主窗口为空时抛出异常
                throw new InvalidOperationException("无法获取主窗口");

            // 创建关于对话框实例
            var dlg = new AboutDialog
            {
                // 绑定专属的 AboutDialogViewModel 作为数据上下文
                DataContext = new AboutDialogViewModel(),
                // 设置对话框在主窗口居中显示
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            // 以模态方式显示关于对话框
            await dlg.ShowDialog(mainWindow);
        }
    }
}