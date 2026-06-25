using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Reflection;
using System.Windows.Input;
using Avalonia;
using Avalonia.Styling;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using RoslynPad.UI;
using RoslynPad.ViewModels;

namespace RoslynPad;

/// <summary>
/// Avalonia 平台下的主视图模型（MainViewModel）派生实现：
/// 负责在 Avalonia 环境中补充平台相关的组合（Composition）程序集、暗色主题判断，以及系统主题切换监听。
/// </summary>
/// <param name="serviceProvider">应用服务提供器（DI 容器入口）。</param>
/// <param name="telemetryProvider">遥测/埋点提供器。</param>
/// <param name="commands">命令提供器（封装应用命令集合）。</param>
/// <param name="settings">应用设置存取接口。</param>
/// <param name="nugetViewModel">NuGet 管理相关 ViewModel。</param>
/// <param name="documentWatcher">文档文件变化监视器。</param>
/// <param name="appDispatcher">应用 UI 调度器（用于封送到 UI 线程）。</param>
/// <param name="logger">日志记录器。</param>
/// <remarks>
/// 该类采用 C# 主构造函数语法，将依赖项直接透传给基类 <see cref="MainViewModel"/>。
/// </remarks>
[Export(typeof(MainViewModel)), Shared]
[method: ImportingConstructor]
public class MainViewModelAvalonia(
    IServiceProvider serviceProvider,
    ITelemetryProvider telemetryProvider,
    ICommandProvider commands,
    IApplicationSettings settings,
    NuGetViewModel nugetViewModel,
    DocumentFileWatcher documentWatcher,
    IAppDispatcher appDispatcher,
    ILogger<MainViewModel> logger
    ) :
    MainViewModel(serviceProvider, telemetryProvider, commands, settings, nugetViewModel, documentWatcher, appDispatcher, logger)
{


    /// <summary>
    /// 参与 MEF/组合的程序集列表：
    /// 在基类基础上额外加入 Avalonia 相关的 Roslyn 与 Editor 实现程序集。
    /// </summary>
    /// <remarks>
    /// 这样可确保在 Avalonia 版本中解析到正确的平台实现（例如编辑器、Roslyn 绑定等）。
    /// </remarks>
    protected override ImmutableArray<Assembly> CompositionAssemblies => base.CompositionAssemblies
        .Add(Assembly.Load(new AssemblyName("RoslynPad.Roslyn.Avalonia")))
        .Add(Assembly.Load(new AssemblyName("RoslynPad.Editor.Avalonia")));

    /// <summary>
    /// 判断当前系统主题是否为暗色模式。
    /// </summary>
    /// <returns>若系统主题为暗色返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    /// <remarks>
    /// 在 Avalonia 中通过 <see cref="Application.Current"/> 的 <see cref="Application.ActualThemeVariant"/> 获取实际主题变体。
    /// </remarks>
    protected override bool IsSystemDarkTheme() => Application.Current?.ActualThemeVariant == ThemeVariant.Dark;

    /// <summary>
    /// 监听系统主题变化，并在需要时触发回调。
    /// </summary>
    /// <param name="onChange">当系统主题发生变化且应用未强制指定主题时调用的回调。</param>
    /// <remarks>
    /// Avalonia 会在 <see cref="Application.ActualThemeVariantChanged"/> 事件中通知主题变化。<br/>
    /// 当 <see cref="Application.RequestedThemeVariant"/> 为 <see langword="null"/> 时，表示跟随系统主题；
    /// 此时才触发 <paramref name="onChange"/>，避免覆盖用户显式指定的主题。
    /// </remarks>
    protected override void ListenToSystemThemeChanges(Action onChange)
    {
        if (Application.Current is { } app)
        {
            app.ActualThemeVariantChanged += (_, _) =>
            {
                if (app.RequestedThemeVariant is null)
                {
                    onChange();
                }
            };
        }
    }




}
