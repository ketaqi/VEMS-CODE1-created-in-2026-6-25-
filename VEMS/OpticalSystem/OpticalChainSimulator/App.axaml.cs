// 引入Avalonia应用生命周期相关命名空间
using Avalonia.Controls.ApplicationLifetimes;
// 引入微软依赖注入核心命名空间
using Microsoft.Extensions.DependencyInjection;
// 引入自定义的弹窗服务相关命名空间
using OpticalChainSimulator.Services.Dialog;

// 光学链模拟器应用程序主命名空间
namespace OpticalChainSimulator;

/// <summary>
/// 应用程序核心入口类，继承自Avalonia的Application基类
/// 负责应用初始化、生命周期管理和依赖注入配置
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// 静态属性：全局暴露应用的主窗口实例
    /// 方便其他模块快速访问主窗口对象
    /// </summary>
    public static Window? MainWindowInstance { get; private set; }

    /// <summary>
    /// 应用初始化方法（Avalonia框架生命周期方法）
    /// 主要用于加载XAML资源和完成基础初始化工作
    /// </summary>
    public override void Initialize()
    {
        // 加载当前应用的XAML资源，完成界面初始化
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// 应用框架初始化完成方法（Avalonia框架生命周期方法）
    /// 在此配置依赖注入、创建主窗口、初始化核心服务
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        // 判断应用生命周期是否为桌面端样式（Windows/Linux/macOS）
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // 1. 创建依赖注入服务集合
            var collection = new ServiceCollection();

            // 2. 注册弹窗服务为单例模式（全局唯一实例）
            // IDialogService：弹窗服务接口，DialogService：具体实现类
            collection.AddSingleton<IDialogService, DialogService>();

            // 3. 构建依赖注入容器，完成服务注册
            var services = collection.BuildServiceProvider();

            // 4. 从容器中获取弹窗服务实例
            var dialogService = services.GetRequiredService<IDialogService>();

            // 5. 通过构造函数注入方式创建主窗口视图模型
            // 避免强制将ViewModel注册到DI容器，保持灵活性
            var mainViewModel = new MainWindowViewModel(dialogService);

            // 6. 创建并配置主窗口
            desktop.MainWindow = new MainWindow
            {
                // 为主窗口绑定视图模型，实现MVVM架构的数据绑定
                DataContext = mainViewModel
            };

            // 7. 将主窗口实例赋值给静态属性，供全局访问
            MainWindowInstance = desktop.MainWindow;

            // 8. 移除Avalonia默认的数据验证器
            // 避免与CommunityToolkit的验证逻辑重复冲突
            BindingPlugins.DataValidators.RemoveAt(0);
        }

        // 调用基类的框架初始化完成方法，确保生命周期流程完整
        base.OnFrameworkInitializationCompleted();
    }
}