using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Roslyn.Utilities;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.PooledObjects;

namespace RoslynPad.Roslyn.Completion.Providers;

/// <summary>
/// 全局程序集缓存（GAC）补全辅助类
/// </summary>
/// <param name="itemRules">补全项规则</param>
internal sealed class GlobalAssemblyCacheCompletionHelper(CompletionItemRules itemRules)
{
    /// <summary>
    /// 延迟加载的程序集简单名称列表
    /// </summary>
    private static readonly Lazy<List<string>> s_lazyAssemblySimpleNames =
         new(() => GlobalAssemblyCache.Instance.GetAssemblySimpleNames().ToList());

    /// <summary>
    /// 补全项规则
    /// </summary>
    private readonly CompletionItemRules _itemRules = itemRules;

    /// <summary>
    /// 异步获取GAC相关的补全项
    /// </summary>
    /// <param name="directoryPath">目录路径（实际为程序集名称前缀）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>补全项数组</returns>
    public Task<ImmutableArray<CompletionItem>> GetItemsAsync(string directoryPath, CancellationToken cancellationToken)
    {
        return Task.Run(() => GetItems(directoryPath, cancellationToken));
    }

    /// <summary>
    /// 获取GAC相关的补全项（内部方法，用于测试）
    /// </summary>
    /// <param name="directoryPath">目录路径（实际为程序集名称前缀）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>补全项数组</returns>
    internal ImmutableArray<CompletionItem> GetItems(string directoryPath, CancellationToken cancellationToken)
    {
        var result = ArrayBuilder<CompletionItem>.GetInstance();

        // 查找逗号分隔符（程序集名称和版本的分隔）
        var comma = directoryPath.IndexOf(',');
        if (comma >= 0)
        {
            // 提取程序集名称前缀并获取匹配的程序集标识
            var partialName = directoryPath.Substring(0, comma);
            foreach (var identity in GetAssemblyIdentities(partialName))
            {
                result.Add(CommonCompletionItem.Create(
                    identity.GetDisplayName(), "", glyph: Microsoft.CodeAnalysis.Glyph.Assembly, rules: _itemRules));
            }
        }
        else
        {
            // 无逗号时返回所有程序集简单名称
            foreach (var displayName in s_lazyAssemblySimpleNames.Value)
            {
                cancellationToken.ThrowIfCancellationRequested();
                result.Add(CommonCompletionItem.Create(
                    displayName, "", glyph: Microsoft.CodeAnalysis.Glyph.Assembly, rules: _itemRules));
            }
        }

        return result.ToImmutableAndFree();
    }

    /// <summary>
    /// 获取匹配前缀的程序集标识列表
    /// </summary>
    /// <param name="partialName">程序集名称前缀</param>
    /// <returns>程序集标识枚举</returns>
    private IEnumerable<AssemblyIdentity> GetAssemblyIdentities(string partialName)
    {
        return IOUtilities.PerformIO(
            () => GlobalAssemblyCache.Instance.GetAssemblyIdentities(partialName),
            SpecializedCollections.EmptyEnumerable<AssemblyIdentity>());
    }
}
