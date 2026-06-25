using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using RoslynPad.Roslyn.Completion.Providers;
using RoslynPad.Utilities;

namespace RoslynPad.ViewModels
{
    /// <summary>
    /// NuGet 包数据模型，封装包的元数据信息。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此类实现 <see cref="INuGetPackage"/> 接口，用于在 UI 中显示 NuGet 包信息，
    /// 并提供包的安装功能。
    /// </para>
    /// <para>
    /// 支持延迟加载包的其他可用版本。
    /// </para>
    /// </remarks>
    public sealed class PackageData : INuGetPackage
    {
        /// <summary>
        /// 原始包搜索元数据。
        /// </summary>
        private readonly IPackageSearchMetadata? _package;

        /// <summary>
        /// 使用包 ID 和版本初始化 <see cref="PackageData"/> 类的新实例（内部使用）。
        /// </summary>
        /// <param name="id">包 ID。</param>
        /// <param name="version">包版本。</param>
        private PackageData(string id, NuGetVersion version)
        {
            Id = id;
            Version = version;
        }

        /// <summary>
        /// 使用包搜索元数据初始化 <see cref="PackageData"/> 类的新实例。
        /// </summary>
        /// <param name="package">NuGet 包搜索元数据。</param>
        public PackageData(IPackageSearchMetadata package)
        {
            _package = package;
            Id = package.Identity.Id;
            Version = package.Identity.Version;
        }

        /// <summary>
        /// 获取包的唯一标识符。
        /// </summary>
        /// <value>NuGet 包 ID（如 "Newtonsoft.Json"）。</value>
        public string Id { get; }

        /// <summary>
        /// 获取包的当前版本。
        /// </summary>
        /// <value>NuGet 版本对象。</value>
        public NuGetVersion Version { get; }

        /// <summary>
        /// 获取包的其他可用版本。
        /// </summary>
        /// <value>按版本降序排列的版本列表。</value>
        public ImmutableArray<PackageData> OtherVersions { get; private set; }

        /// <inheritdoc/>
        /// <remarks>
        /// 返回版本字符串列表，优先返回最新的稳定版本。
        /// </remarks>
        IEnumerable<string> INuGetPackage.Versions
        {
            get
            {
                if (!OtherVersions.IsDefaultOrEmpty)
                {
                    // 优先返回最新稳定版本
                    var lastStable = OtherVersions.FirstOrDefault(v => !v.Version.IsPrerelease);
                    if (lastStable != null)
                    {
                        yield return lastStable.Version.ToString();
                    }

                    // 返回其他版本
                    foreach (var version in OtherVersions)
                    {
                        if (version != lastStable)
                        {
                            yield return version.Version.ToString();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取或设置安装包命令。
        /// </summary>
        /// <value>用于安装此包的委托命令。</value>
        /// <remarks>
        /// 此属性由 <see cref="NuGetDocumentViewModel"/> 在搜索完成后设置。
        /// </remarks>
        public IDelegateCommand? InstallPackageCommand { get; internal set; }

        /// <summary>
        /// 异步初始化包数据，加载所有可用版本。
        /// </summary>
        /// <returns>表示异步操作的任务。</returns>
        public async Task Initialize()
        {
            if (_package == null)
            {
                return;
            }

            var versions = await _package.GetVersionsAsync().ConfigureAwait(false);
            OtherVersions = [.. versions
                .Select(x => new PackageData(Id, x. Version))
                .OrderByDescending(x => x.Version)];
        }
    }
}
