using NuGet.Common;
using NuGet.Protocol.Core.Types;

namespace RoslynPad.UI;

/// <summary>
/// NuGet 源仓库扩展方法：简化 <see cref="SourceRepository"/> 的包搜索操作。
/// </summary>
/// <remarks>
/// <para>
/// 此静态类提供对 NuGet <see cref="SourceRepository"/> 的便捷扩展，
/// 封装了获取搜索资源和执行搜索的复杂逻辑。
/// </para>
/// <para>
/// 使用场景：
/// <list type="bullet">
///   <item><description>在 NuGet 包管理器中搜索可用包</description></item>
///   <item><description>根据关键字查找包元数据</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
/// var filter = new SearchFilter(includePrerelease: false);
/// 
/// var packages = await repository.SearchAsync(
///     "Newtonsoft.Json",
///     filter,
///     pageSize: 10,
///     cancellationToken
/// );
/// 
/// foreach (var package in packages)
/// {
///     Console.WriteLine($"{package.Identity.Id} - {package.Identity.Version}");
/// }
/// </code>
/// </example>
internal static class SourceRepositoryExtensions
{
    /// <summary>
    /// 异步搜索 NuGet 包。
    /// </summary>
    /// <param name="sourceRepository">要搜索的 NuGet 源仓库。</param>
    /// <param name="searchText">搜索关键字。</param>
    /// <param name="searchFilter">搜索过滤器（如是否包含预发布版本）。</param>
    /// <param name="pageSize">返回结果的最大数量。</param>
    /// <param name="cancellationToken">用于取消操作的令牌。</param>
    /// <returns>
    /// 匹配搜索条件的包元数据数组；如果搜索资源不可用或无结果，返回空数组。
    /// </returns>
    /// <remarks>
    /// <para>
    /// 此方法会自动获取 <see cref="PackageSearchResource"/>，如果源仓库不支持搜索功能，
    /// 将返回空数组而非抛出异常。
    /// </para>
    /// <para>
    /// 搜索从第一页开始（skip = 0），如需分页，应自行实现分页逻辑。
    /// </para>
    /// </remarks>
    public static async Task<IPackageSearchMetadata[]> SearchAsync(
        this SourceRepository sourceRepository,
        string searchText,
        SearchFilter searchFilter,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var searchResource = await sourceRepository
            .GetResourceAsync<PackageSearchResource>(cancellationToken)
            .ConfigureAwait(false);

        if (searchResource != null)
        {
            var searchResults = await searchResource.SearchAsync(
                searchText,
                searchFilter,
                0,
                pageSize,
                NullLogger.Instance,
                cancellationToken).ConfigureAwait(false);

            if (searchResults != null)
            {
                return searchResults.ToArray();
            }
        }

        return [];
    }
}
