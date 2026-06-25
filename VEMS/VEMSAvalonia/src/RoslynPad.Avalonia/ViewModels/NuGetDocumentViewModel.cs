using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using RoslynPad.UI;
using RoslynPad.Utilities;

namespace RoslynPad.ViewModels
{
    /// <summary>
    /// NuGet 文档视图模型，用于管理 NuGet 包搜索和安装功能。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此视图模型提供以下功能：
    /// <list type="bullet">
    ///   <item><description>异步搜索 NuGet 包</description></item>
    ///   <item><description>支持预发行版本筛选</description></item>
    ///   <item><description>支持精确匹配搜索</description></item>
    ///   <item><description>安装选定的包</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// 搜索操作支持取消，新的搜索请求会自动取消之前的搜索。
    /// </para>
    /// </remarks>
    [Export]
    public sealed class NuGetDocumentViewModel : NotificationObject
    {
        /// <summary>
        /// NuGet 视图模型引用。
        /// </summary>
        private readonly NuGetViewModel _nuGetViewModel;

        /// <summary>
        /// 遥测提供程序。
        /// </summary>
        private readonly ITelemetryProvider _telemetryProvider;

        /// <summary>
        /// 主视图模型引用（用于输出日志）。
        /// </summary>
        private readonly MainViewModel? _mainViewModel;

        /// <summary>
        /// 当前搜索词。
        /// </summary>
        private string? _searchTerm;

        /// <summary>
        /// 是否正在搜索。
        /// </summary>
        private bool _isSearching;

        /// <summary>
        /// 搜索取消令牌源。
        /// </summary>
        private CancellationTokenSource? _searchCts;

        /// <summary>
        /// 包菜单是否打开。
        /// </summary>
        private bool _isPackagesMenuOpen;

        /// <summary>
        /// 是否包含预发行版本。
        /// </summary>
        private bool _prerelease;

        /// <summary>
        /// 搜索结果包列表。
        /// </summary>
        private IReadOnlyList<PackageData> _packages;

        /// <summary>
        /// 获取或设置搜索到的包列表。
        /// </summary>
        /// <value>只读的包数据集合。</value>
        public IReadOnlyList<PackageData> Packages
        {
            get => _packages;
            private set => SetProperty(ref _packages, value);
        }

        /// <summary>
        /// 初始化 <see cref="NuGetDocumentViewModel"/> 类的新实例。
        /// </summary>
        /// <param name="nuGetViewModel">NuGet 视图模型。</param>
        /// <param name="commands">命令提供程序。</param>
        /// <param name="telemetryProvider">遥测提供程序。</param>
        /// <param name="mainViewModel">主视图模型（可选，用于日志输出）。</param>
        [ImportingConstructor]
        public NuGetDocumentViewModel(
            NuGetViewModel nuGetViewModel,
            ICommandProvider commands,
            ITelemetryProvider telemetryProvider,
            MainViewModel? mainViewModel)
        {
            _nuGetViewModel = nuGetViewModel;
            _telemetryProvider = telemetryProvider;
            _mainViewModel = mainViewModel;
            _packages = [];

            InstallPackageCommand = commands.Create<PackageData>(InstallPackage);
        }

        /// <summary>
        /// 获取安装包命令。
        /// </summary>
        public IDelegateCommand<PackageData> InstallPackageCommand { get; }

        /// <summary>
        /// 安装指定的 NuGet 包。
        /// </summary>
        /// <param name="package">要安装的包；若为 <see langword="null"/> 则不执行操作。</param>
        public void InstallPackage(PackageData? package)
        {
            if (package == null)
            {
                return;
            }

            OnPackageInstalled(package);
        }

        /// <summary>
        /// 触发包已安装事件。
        /// </summary>
        /// <param name="package">已安装的包。</param>
        private void OnPackageInstalled(PackageData package)
        {
            PackageInstalled?.Invoke(package);
        }

        /// <summary>
        /// 当包被安装时触发。
        /// </summary>
        public event Action<PackageData>? PackageInstalled;

        /// <summary>
        /// 获取一个值，指示是否正在执行搜索。
        /// </summary>
        /// <value>若正在搜索返回 <see langword="true"/>；否则返回 <see langword="false"/>。</value>
        public bool IsSearching
        {
            get => _isSearching;
            private set => SetProperty(ref _isSearching, value);
        }

        /// <summary>
        /// 获取或设置搜索词。
        /// </summary>
        /// <value>用户输入的搜索字符串。</value>
        /// <remarks>
        /// 设置此属性会自动触发异步搜索。
        /// 新的搜索请求会取消之前正在进行的搜索。
        /// </remarks>
        public string? SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (SetProperty(ref _searchTerm, value))
                {
                    PerformSearch();
                }
            }
        }

        /// <summary>
        /// 获取或设置包菜单是否打开。
        /// </summary>
        /// <value>若菜单打开返回 <see langword="true"/>；否则返回 <see langword="false"/>。</value>
        public bool IsPackagesMenuOpen
        {
            get => _isPackagesMenuOpen;
            set => SetProperty(ref _isPackagesMenuOpen, value);
        }

        /// <summary>
        /// 获取或设置是否使用精确匹配搜索。
        /// </summary>
        /// <value>若启用精确匹配返回 <see langword="true"/>；否则返回 <see langword="false"/>。</value>
        public bool ExactMatch { get; set; }

        /// <summary>
        /// 获取或设置是否包含预发行版本。
        /// </summary>
        /// <value>若包含预发行版本返回 <see langword="true"/>；否则返回 <see langword="false"/>。</value>
        /// <remarks>
        /// 更改此属性会自动重新执行搜索。
        /// </remarks>
        public bool Prerelease
        {
            get => _prerelease;
            set
            {
                if (SetProperty(ref _prerelease, value))
                {
                    PerformSearch();
                }
            }
        }

        /// <summary>
        /// 启动异步搜索操作。
        /// </summary>
        /// <remarks>
        /// 此方法会取消之前的搜索请求，并在后台线程上执行新的搜索。
        /// </remarks>
        private void PerformSearch()
        {
            if (string.IsNullOrEmpty(SearchTerm))
            {
                return;
            }

            // 取消之前的搜索
            _searchCts?.Cancel();
            var searchCts = new CancellationTokenSource();
            var cancellationToken = searchCts.Token;
            _searchCts = searchCts;

            _ = Task.Run(() => PerformSearchAsync(SearchTerm, cancellationToken), cancellationToken);
        }

        /// <summary>
        /// 异步执行包搜索。
        /// </summary>
        /// <param name="searchTerm">搜索词。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        private async Task PerformSearchAsync(string searchTerm, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                Packages = [];
                IsPackagesMenuOpen = false;
                return;
            }

            IsSearching = true;
            try
            {
                try
                {
                    // 输出搜索开始日志
                    _mainViewModel?.OutputResult("[SearchTerm]", "Start searching:  " + SearchTerm, null, null);
                    Console.WriteLine(SearchTerm);

                    // 执行异步搜索
                    var packages = await Task.Run(
                        () => _nuGetViewModel.GetPackagesAsync(
                            searchTerm,
                            includePrerelease: Prerelease,
                            exactMatch: ExactMatch,
                            cancellationToken: cancellationToken),
                        cancellationToken).ConfigureAwait(true);

                    Console.WriteLine(SearchTerm + " 结束");

                    // 输出搜索结束日志
                    _mainViewModel?.OutputResult("[SearchTerm]", "End Search:  " + SearchTerm, null, null);

                    cancellationToken.ThrowIfCancellationRequested();

                    // 为每个包设置安装命令
                    foreach (var package in packages)
                    {
                        package.InstallPackageCommand = InstallPackageCommand;
                    }

                    Packages = packages;
                    IsPackagesMenuOpen = Packages.Count > 0;
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    _telemetryProvider.ReportError(e);
                }
            }
            finally
            {
                IsSearching = false;
            }
        }

        /// <summary>
        /// 同步执行包搜索（备用方法）。
        /// </summary>
        /// <param name="searchTerm">搜索词。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <remarks>
        /// 此方法在线程池上同步执行异步搜索操作，
        /// 用于避免 UI 同步上下文导致的死锁。
        /// </remarks>
        private void PerformSearchSync(string searchTerm, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                Packages = Array.Empty<PackageData>();
                IsPackagesMenuOpen = false;
                return;
            }

            IsSearching = true;
            try
            {
                try
                {
                    // 在线程池上执行以避免死锁
                    var packages = Task.Run(
                        () => _nuGetViewModel.GetPackagesAsync(
                            searchTerm,
                            includePrerelease: Prerelease,
                            exactMatch: ExactMatch,
                            cancellationToken: cancellationToken),
                        cancellationToken).GetAwaiter().GetResult();

                    cancellationToken.ThrowIfCancellationRequested();

                    // 防止旧搜索结果覆盖新搜索
                    if (!string.Equals(searchTerm, SearchTerm, StringComparison.Ordinal))
                    {
                        return;
                    }

                    foreach (var package in packages)
                    {
                        package.InstallPackageCommand = InstallPackageCommand;
                    }

                    Packages = packages;
                    IsPackagesMenuOpen = Packages.Count > 0;
                }
                catch (OperationCanceledException)
                {
                    // 取消是预期行为，不做处理
                }
                catch (Exception e)
                {
                    _telemetryProvider.ReportError(e);
                }
            }
            finally
            {
                IsSearching = false;
            }
        }
    }
}
