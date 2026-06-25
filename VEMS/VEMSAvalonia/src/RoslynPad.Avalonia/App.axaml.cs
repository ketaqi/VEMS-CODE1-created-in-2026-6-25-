using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.PropertyGrid.Services;
using Live.Avalonia;
using RoslynPad.FontAwesome;
using RoslynPad.IconsFront;
using RoslynPad.MaterialDesign;
using RoslynPad.Resources;
using RoslynPad.Themes;
using RoslynPad.UI;

namespace RoslynPad
{
    /// <summary>
    /// 应用入口类（Avalonia）：
    /// - 负责初始化 XAML 资源、图标提供者、本地化与主题资源字典；
    /// - 在桌面生命周期下创建主窗口，调试场景可启用 Live.Avalonia 热重载；
    /// - 实现 <see cref="ILiveView"/> 以提供热重载承载的根视图。
    /// </summary>
    public sealed class App : Application, ILiveView
    {

        /// <summary>
        /// 内嵌 VS Code 主题资源的 URI（通过 <see cref="AssetLoader"/> 读取）。
        /// </summary>
        /// <remarks>
        /// 若目标环境的 <c>VsCodeThemeReader</c> 仅接受文件路径，本应用会将内嵌资源
        /// 暂存到临时目录再读取，以兼容不同版本实现。
        /// </remarks>
        private const string ThemeAssetUri = "avares://RoslynPad/Themes/dark_modern.json";

        /// <summary>
        /// 框架初始化：加载 XAML 资源并在运行时配置本地化、图标与主题。
        /// </summary>
        public override void Initialize()
        {

            // 先加载 XAML 资源，保证设计器有样式
            AvaloniaXamlLoader.Load(this);

            // 设计时保持“轻量”，不要做任何可能抛异常的初始化
            if (Design.IsDesignMode)
                return;

            try
            {
                //var viewModel = new MainViewModel(); // 或有参数 new MainViewModel(...)
                //var language = viewModel.InitialLanguage ?? "en-US";
                //LocalizationService.Default.SelectCulture(language);
                //LocalizationManager.Instance.LoadLanguage(new CultureInfo(language));
                LocalizationService.Default.SelectCulture("en-US");
                // 注册可用的图标提供者（FontAwesome / Material）
                IconProvider.Current
                    .Register<FontAwesomeIconProvider>()
                    .Register<MaterialDesignIconProvider>();

                // 合并图标资源字典（用于控件模板中引用）
                Resources.MergedDictionaries.Add(new Icons());

                // 尝试加载 VS Code 风格主题（优先内嵌资源）
                TryLoadVsCodeTheme();
            }
            catch (Exception ex)
            {
                // 主题/图标失败不应阻止应用启动：输出日志并继续
                Console.WriteLine("[App.Initialize] " + ex);
            }
        }

        /// <summary>
        /// 框架完成初始化后的入口：根据运行环境决定使用热重载宿主或直接显示主窗口。
        /// </summary>
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (!Design.IsDesignMode && ShouldUseLiveHost())
                {
                    // 调试体验：Live.Avalonia 热重载宿主
                    var host = new LiveViewHost(this, Console.WriteLine);
                    host.StartWatchingSourceFilesForHotReloading();
                    desktop.MainWindow = host;
                    host.Show();
                }
                else
                {
                    // 常规运行：直接创建主窗口
                    desktop.MainWindow = new MainWindow();
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        /// <summary>
        /// <see cref="ILiveView"/>：返回用于热重载预览的根视图。
        /// </summary>
        /// <param name="window">由热重载宿主创建的窗口。</param>
        /// <returns>主窗口的根内容控件；若为空则返回提示文本。</returns>
        public object CreateView(Window window)
        {
            // 通过 MainWindow 生成完整的可视树，再把根内容“解绑”并交给宿主
            var temp = new MainWindow(); // 会 InitializeComponent

            if (temp.Content is Control root)
            {
                // 解绑 Content，避免重复 Parent
                temp.Content = null;

                // 同步 DataContext，保证热重载窗口与实际窗口的数据上下文一致
                if (window.DataContext == null)
                    window.DataContext = temp.DataContext;

                return root;
            }

            // 兜底：没有可视内容时给出简单提示
            return new TextBlock { Text = "MainWindow.Content 为 null，无法预览。" };
        }

        /// <summary>
        /// 决定是否启用 Live.Avalonia 热重载宿主。
        /// </summary>
        /// <remarks>
        /// 规则（仅 <c>DEBUG</c> 编译）：优先读取环境变量 <c>LIVE_AVALONIA</c>（非空且不等于“0”则启用），
        /// 否则在“未附加调试器”时默认启用；Release 构建恒为 <see langword="false"/>。
        /// </remarks>
        private static bool ShouldUseLiveHost()
        {
#if DEBUG
            if (Design.IsDesignMode)
                return false;

            var env = Environment.GetEnvironmentVariable("LIVE_AVALONIA");
            if (!string.IsNullOrWhiteSpace(env) && env != "0")
                return true;

            // 默认在未附加调试器时启用
            return !Debugger.IsAttached;
#else
            return false;
#endif
        }

        /// <summary>
        /// 尝试加载 VS Code 主题：优先读取内嵌资源；失败时回退到相对磁盘路径。
        /// </summary>
        private void TryLoadVsCodeTheme()
        {
            try
            {
                var reader = new VsCodeThemeReader();

                // 优先：内嵌资源（avares://）
                var uri = new Uri(ThemeAssetUri);
                if (AssetLoader.Exists(uri))
                {
                    // 某些 VsCodeThemeReader 仅接受“文件路径”：
                    // 将内嵌资源写到临时文件后再读取，兼容所有版本
                    var tmp = Path.Combine(Path.GetTempPath(), "dark_modern.json");
                    using (var src = AssetLoader.Open(uri))
                    using (var dst = File.Create(tmp))
                        src.CopyTo(dst);

                    var theme = reader.ReadThemeAsync(tmp, ThemeType.Dark).GetAwaiter().GetResult();
                    Resources.MergedDictionaries.Add(new ThemeDictionary(theme));
                    return;
                }

                // 回退：相对磁盘路径（保持与项目目录结构兼容）
                var fallback = reader.ReadThemeAsync("Themes/dark_modern.json", ThemeType.Dark)
                                     .GetAwaiter().GetResult();
                Resources.MergedDictionaries.Add(new ThemeDictionary(fallback));
            }
            catch (Exception ex)
            {
                // 主题失败非致命：打印日志并继续运行
                Console.WriteLine("[TryLoadVsCodeTheme] " + ex);
            }
        }
    }
}
