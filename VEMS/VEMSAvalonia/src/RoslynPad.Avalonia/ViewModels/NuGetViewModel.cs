using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Credentials;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using RoslynPad.Roslyn.Completion.Providers;
using RoslynPad.UI;
using IPackageSourceProvider = NuGet.Configuration.IPackageSourceProvider;
using PackageSource = NuGet.Configuration.PackageSource;
using PackageSourceProvider = NuGet.Configuration.PackageSourceProvider;
using Settings = NuGet.Configuration.Settings;

namespace RoslynPad.ViewModels
{
    /// <summary>
    /// NuGet 视图模型，提供 NuGet 包搜索和管理功能。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此类实现 <see cref="INuGetCompletionProvider"/> 接口，
    /// 为代码编辑器提供 NuGet 包的自动完成支持。
    /// </para>
    /// <para>
    /// 使用 NuGet. Protocol 库与 NuGet 源进行通信，
    /// 支持多个包源的并行搜索。
    /// </para>
    /// </remarks>
    [Export]
    [Export(typeof(INuGetCompletionProvider))]
    [Shared]
    public sealed class NuGetViewModel : NotificationObject, INuGetCompletionProvider
    {
        /// <summary>
        /// 最大搜索结果数量。
        /// </summary>
        private const int MaxSearchResults = 50;

        /// <summary>
        /// 源仓库提供程序。
        /// </summary>
        private readonly CommandLineSourceRepositoryProvider? _sourceRepositoryProvider;

        /// <summary>
        /// 初始化异常信息（若初始化失败）。
        /// </summary>
        private readonly ExceptionDispatchInfo? _initializationException;

        /// <summary>
        /// 获取 NuGet 配置文件路径。
        /// </summary>
        /// <value>配置文件的完整路径。</value>
        public string ConfigPath { get; set; }

        /// <summary>
        /// 获取全局包文件夹路径。
        /// </summary>
        /// <value>NuGet 全局包缓存目录。</value>
        public string GlobalPackageFolder { get; }

        /// <summary>
        /// 初始化 <see cref="NuGetViewModel"/> 类的新实例。
        /// </summary>
        /// <param name="telemetryProvider">遥测提供程序（可选）。</param>
        /// <param name="appSettings">应用程序设置。</param>
        [ImportingConstructor]
        public NuGetViewModel(
            [Import(AllowDefault = true)] ITelemetryProvider? telemetryProvider,
            IApplicationSettings appSettings)
        {
            try
            {
                var settings = LoadSettings();
                ConfigPath = settings.GetConfigFilePaths().First();
                GlobalPackageFolder = SettingsUtility.GetGlobalPackagesFolder(settings);

                DefaultCredentialServiceUtility.SetupDefaultCredentialService(NullLogger.Instance, nonInteractive: false);

                var sourceProvider = new PackageSourceProvider(settings);
                _sourceRepositoryProvider = new CommandLineSourceRepositoryProvider(sourceProvider);
            }
            catch (Exception e)
            {
                _initializationException = ExceptionDispatchInfo.Capture(e);
                ConfigPath = string.Empty;
                GlobalPackageFolder = string.Empty;
            }

            // 加载 NuGet 设置，带重试机制
            Settings LoadSettings()
            {
                Settings? settings = null;
                const int retries = 3;

                for (var i = 1; i <= retries; i++)
                {
                    try
                    {
                        settings = new Settings(appSettings.GetDefaultDocumentPath(), "RoslynPad. nuget. config");
                    }
                    catch (NuGetConfigurationException ex)
                    {
                        if (i == retries)
                        {
                            telemetryProvider?.ReportError(ex);
                            throw;
                        }
                    }
                }

                return settings!;
            }
        }

        /// <summary>
        /// 异步搜索 NuGet 包。
        /// </summary>
        /// <param name="searchTerm">搜索词。</param>
        /// <param name="includePrerelease">是否包含预发行版本。</param>
        /// <param name="exactMatch">是否精确匹配包名。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>匹配的包数据列表。</returns>
        /// <remarks>
        /// 此方法会并行搜索所有已启用的包源，
        /// 并合并结果返回。
        /// </remarks>
        public async Task<IReadOnlyList<PackageData>> GetPackagesAsync(
            string searchTerm,
            bool includePrerelease,
            bool exactMatch,
            CancellationToken cancellationToken)
        {
            _initializationException?.Throw();

            if (_sourceRepositoryProvider is null)
            {
                return [];
            }

            var filter = new SearchFilter(includePrerelease);
            var packages = new List<PackageData>();

            foreach (var sourceRepository in _sourceRepositoryProvider.GetRepositories())
            {
                IPackageSearchMetadata[]? result;
                try
                {
                    result = await sourceRepository
                        .SearchAsync(searchTerm, filter, MaxSearchResults, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (FatalProtocolException)
                {
                    // 跳过无法访问的源
                    continue;
                }

                // 精确匹配时只保留完全匹配的包
                if (exactMatch)
                {
                    var match = result.FirstOrDefault(c =>
                        string.Equals(c.Identity.Id, searchTerm, StringComparison.OrdinalIgnoreCase));
                    result = match != null ? [match] : null;
                }

                if (result?.Length > 0)
                {
                    var repositoryPackages = result
                        .Select(x => new PackageData(x))
                        .ToArray();

                    // 并行初始化包数据
                    await Task.WhenAll(repositoryPackages.Select(x => x.Initialize()))
                        .ConfigureAwait(false);

                    packages.AddRange(repositoryPackages);
                }
            }

            return packages;
        }

        /// <inheritdoc/>
        async Task<IReadOnlyList<INuGetPackage>> INuGetCompletionProvider.SearchPackagesAsync(
            string searchString,
            bool exactMatch,
            CancellationToken cancellationToken)
        {
            var packages = await GetPackagesAsync(
                searchString,
                includePrerelease: true,
                exactMatch,
                cancellationToken).ConfigureAwait(false);

            return packages;
        }

        /// <summary>
        /// 命令行源仓库提供程序实现。
        /// </summary>
        /// <remarks>
        /// 此类管理 NuGet 包源仓库，并提供源仓库的缓存。
        /// </remarks>
        private class CommandLineSourceRepositoryProvider : ISourceRepositoryProvider
        {
            /// <summary>
            /// NuGet 资源提供程序列表。
            /// </summary>
            private readonly List<Lazy<INuGetResourceProvider>> _resourceProviders;

            /// <summary>
            /// 源仓库列表。
            /// </summary>
            private readonly List<SourceRepository> _repositories;

            /// <summary>
            /// 源仓库缓存，确保每个包源只有一个仓库实例。
            /// </summary>
            private static readonly ConcurrentDictionary<PackageSource, SourceRepository> s_cachedSources = new();

            /// <summary>
            /// 初始化 <see cref="CommandLineSourceRepositoryProvider"/> 类的新实例。
            /// </summary>
            /// <param name="packageSourceProvider">包源提供程序。</param>
            public CommandLineSourceRepositoryProvider(IPackageSourceProvider packageSourceProvider)
            {
                PackageSourceProvider = packageSourceProvider;

                _resourceProviders = [.. Repository.Provider.GetCoreV3()];

                // 创建已启用源的仓库
                _repositories = PackageSourceProvider.LoadPackageSources()
                    .Where(s => s.IsEnabled)
                    .Select(CreateRepository)
                    .ToList();
            }

            /// <inheritdoc/>
            public IEnumerable<SourceRepository> GetRepositories() => _repositories;

            /// <inheritdoc/>
            public SourceRepository CreateRepository(PackageSource source)
            {
                return s_cachedSources.GetOrAdd(source, new SourceRepository(source, _resourceProviders));
            }

            /// <inheritdoc/>
            public SourceRepository CreateRepository(PackageSource source, FeedType type)
            {
                return s_cachedSources.GetOrAdd(source, new SourceRepository(source, _resourceProviders, type));
            }

            /// <inheritdoc/>
            public IPackageSourceProvider PackageSourceProvider { get; }
        }
    }
}
