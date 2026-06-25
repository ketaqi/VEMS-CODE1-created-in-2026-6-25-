using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;

namespace RoslynPad.Roslyn.SignatureHelp;

/// <summary>
/// 聚合式签名帮助提供程序，用于整合多个CSharp签名帮助提供程序的行为
/// </summary>
[Export(typeof(ISignatureHelpProvider)), Shared]
[method: ImportingConstructor]
internal sealed class AggregateSignatureHelpProvider(
    [ImportMany] IEnumerable<Lazy<Microsoft.CodeAnalysis.SignatureHelp.ISignatureHelpProvider, OrderableLanguageMetadata>> providers)
    : ISignatureHelpProvider
{
    /// <summary>
    /// 筛选后的CSharp签名帮助提供程序集合
    /// </summary>
    private readonly ImmutableArray<Microsoft.CodeAnalysis.SignatureHelp.ISignatureHelpProvider> _providers = providers
        .Where(x => x.Metadata.Language == LanguageNames.CSharp)
        .Select(x => x.Value)
        .ToImmutableArray();

    /// <summary>
    /// 判断指定字符是否为触发签名帮助的字符
    /// </summary>
    /// <param name="ch">待判断的字符</param>
    /// <returns>若任一提供程序判定为触发字符则返回true，否则返回false</returns>
    public bool IsTriggerCharacter(char ch)
    {
        return _providers.Any(p => p.IsTriggerCharacter(ch));
    }

    /// <summary>
    /// 判断指定字符是否为重新触发签名帮助的字符
    /// </summary>
    /// <param name="ch">待判断的字符</param>
    /// <returns>若任一提供程序判定为重新触发字符则返回true，否则返回false</returns>
    public bool IsRetriggerCharacter(char ch)
    {
        return _providers.Any(p => p.IsRetriggerCharacter(ch));
    }

    /// <summary>
    /// 异步获取指定位置的签名帮助项
    /// </summary>
    /// <param name="document">当前文档</param>
    /// <param name="position">获取签名帮助的位置（字符偏移量）</param>
    /// <param name="trigger">签名帮助触发信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>匹配的签名帮助项，无匹配项时返回null</returns>
    public async Task<SignatureHelpItems?> GetItemsAsync(Document document, int position, SignatureHelpTriggerInfo trigger, CancellationToken cancellationToken)
    {
        Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpItems? bestItems = null;

        // TODO(cyrusn): We're calling into extensions, we need to make ourselves resilient
        // to the extension crashing.
        foreach (var provider in _providers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentItems = await provider.GetItemsAsync(document, position, trigger.Inner, MemberDisplayOptions.Default, cancellationToken).ConfigureAwait(false);
            if (currentItems != null && currentItems.ApplicableSpan.IntersectsWith(position))
            {
                // 如果另一个提供程序提供了签名帮助项，则仅当这些项的起始位置晚于上一批项时才选用
                // 例如：Foo(new Bar($$ 仅显示 "new Bar(" 的签名帮助，而非 "Foo(...)"
                if (IsBetter(bestItems, currentItems.ApplicableSpan))
                {
                    bestItems = currentItems;
                }
            }
        }

        if (bestItems != null)
        {
            var items = new SignatureHelpItems(bestItems);
            if (items.SelectedItemIndex == null)
            {
                var selection = DefaultSignatureHelpSelector.GetSelection(
                    items.Items,
                    selectedItem: null,
                    userSelected: false,
                    items.SemanticParameterIndex,
                    items.SyntacticArgumentCount,
                    items.ArgumentName ?? string.Empty,
                    isCaseSensitive: true);

                if (selection.SelectedItem != null)
                {
                    items.SelectedItemIndex = items.Items.IndexOf(selection.SelectedItem);
                }
            }

            return items;
        }

        return null;
    }

    /// <summary>
    /// 判断当前签名帮助项的适用范围是否优于已选的最佳项
    /// </summary>
    /// <param name="bestItems">已选的最佳签名帮助项</param>
    /// <param name="currentTextSpan">当前签名帮助项的适用文本范围</param>
    /// <returns>若当前项更优则返回true，否则返回false</returns>
    private static bool IsBetter(Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpItems? bestItems, TextSpan? currentTextSpan)
    {
        return bestItems == null || currentTextSpan?.Start > bestItems.ApplicableSpan.Start;
    }

    /// <summary>
    /// 签名帮助选择结果结构体，包含选中项、是否用户选择、选中参数索引
    /// </summary>
    /// <param name="selectedItem">选中的签名帮助项</param>
    /// <param name="userSelected">是否由用户手动选择</param>
    /// <param name="selectedParameter">选中的参数索引</param>
    private readonly struct SignatureHelpSelection(SignatureHelpItem selectedItem, bool userSelected, int? selectedParameter)
    {
        /// <summary>
        /// 选中的参数索引
        /// </summary>
        public int? SelectedParameter { get; } = selectedParameter;

        /// <summary>
        /// 选中的签名帮助项
        /// </summary>
        public SignatureHelpItem SelectedItem { get; } = selectedItem;

        /// <summary>
        /// 是否由用户手动选择
        /// </summary>
        public bool UserSelected { get; } = userSelected;
    }

    /// <summary>
    /// 默认签名帮助选择器，用于筛选最佳签名帮助项和参数
    /// </summary>
    private static class DefaultSignatureHelpSelector
    {
        /// <summary>
        /// 获取签名帮助的最佳选择结果
        /// </summary>
        /// <param name="items">候选签名帮助项列表</param>
        /// <param name="selectedItem">当前选中项（可为null）</param>
        /// <param name="userSelected">当前选中项是否为用户选择</param>
        /// <param name="semanticParameterIndex">语义分析得到的参数索引</param>
        /// <param name="syntacticArgumentCount">语法分析得到的参数数量</param>
        /// <param name="argumentName">参数名称（可为空）</param>
        /// <param name="isCaseSensitive">参数名称匹配是否区分大小写</param>
        /// <returns>签名帮助选择结果</returns>
        public static SignatureHelpSelection GetSelection(
            IList<SignatureHelpItem> items,
            SignatureHelpItem? selectedItem,
            bool userSelected,
            int semanticParameterIndex,
            int syntacticArgumentCount,
            string argumentName,
            bool isCaseSensitive)
        {
            selectedItem = SelectBestItem(
                selectedItem,
                ref userSelected,
                items,
                semanticParameterIndex,
                syntacticArgumentCount,
                argumentName,
                isCaseSensitive);

            var selectedParameter = GetSelectedParameter(
                selectedItem,
                semanticParameterIndex,
                argumentName,
                isCaseSensitive);

            return new SignatureHelpSelection(selectedItem, userSelected, selectedParameter);
        }

        /// <summary>
        /// 获取选中的参数索引
        /// </summary>
        /// <param name="bestItem">最佳签名帮助项</param>
        /// <param name="parameterIndex">语义分析得到的参数索引</param>
        /// <param name="parameterName">参数名称（可为空）</param>
        /// <param name="isCaseSensitive">参数名称匹配是否区分大小写</param>
        /// <returns>选中的参数索引</returns>
        private static int GetSelectedParameter(
            SignatureHelpItem bestItem,
            int parameterIndex,
            string? parameterName,
            bool isCaseSensitive)
        {
            if (!string.IsNullOrEmpty(parameterName))
            {
                var comparer = isCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
                var selected = bestItem.Parameters
                    .Select((p, index) => (p, index))
                    .FirstOrDefault(p => comparer.Equals(p.p.Name, parameterName));

                if (selected.p != null)
                {
                    return selected.index;
                }
            }

            return parameterIndex;
        }

        /// <summary>
        /// 选择最佳的签名帮助项
        /// </summary>
        /// <param name="currentItem">当前选中项（可为null）</param>
        /// <param name="userSelected">引用参数，标识当前项是否为用户选择</param>
        /// <param name="filteredItems">筛选后的候选项列表</param>
        /// <param name="selectedParameter">选中的参数索引</param>
        /// <param name="argumentCount">参数数量</param>
        /// <param name="name">参数名称（可为空）</param>
        /// <param name="isCaseSensitive">参数名称匹配是否区分大小写</param>
        /// <returns>最佳签名帮助项</returns>
        private static SignatureHelpItem SelectBestItem(
            SignatureHelpItem? currentItem,
            ref bool userSelected,
            IList<SignatureHelpItem> filteredItems,
            int selectedParameter,
            int argumentCount,
            string? name,
            bool isCaseSensitive)
        {
            // 如果当前项仍适用，则保留
            if (currentItem != null && filteredItems.Contains(currentItem) &&
                IsApplicable(currentItem, argumentCount, name, isCaseSensitive))
            {
                // 若当前项是用户选择的，保持该状态
                return currentItem;
            }

            // 若当前项不再适用，选择新项且标记为非用户选择
            userSelected = false;

            // 查找第一个适用的项；若无，则选择最后一项（参数数量最接近）
            var result = filteredItems.FirstOrDefault(i => IsApplicable(i, argumentCount, name, isCaseSensitive));
            if (result != null)
            {
                currentItem = result;
                return currentItem;
            }

            // 若指定了参数名但未找到匹配项，尝试忽略参数名重新查找
            if (name != null)
            {
                return SelectBestItem(currentItem, ref userSelected, filteredItems, selectedParameter, argumentCount, null, isCaseSensitive);
            }

            // 若无匹配参数数量的项，选择最后一项；若当前项与最后一项参数数量相同则保留当前项
            var lastItem = filteredItems.Last();
            if (currentItem != null && (currentItem.IsVariadic || currentItem.Parameters.Length == lastItem.Parameters.Length))
            {
                return currentItem;
            }

            return lastItem;
        }

        /// <summary>
        /// 判断签名帮助项是否适用于当前参数上下文
        /// </summary>
        /// <param name="item">待判断的签名帮助项</param>
        /// <param name="argumentCount">参数数量</param>
        /// <param name="name">参数名称（可为空）</param>
        /// <param name="isCaseSensitive">参数名称匹配是否区分大小写</param>
        /// <returns>若适用则返回true，否则返回false</returns>
        private static bool IsApplicable(SignatureHelpItem item, int argumentCount, string? name, bool isCaseSensitive)
        {
            // 若指定了参数名，仅当项包含该参数名时适用
            if (name != null)
            {
                var comparer = isCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
                return item.Parameters.Any(p => comparer.Equals(p.Name, name));
            }

            // 项的参数数量大于等于当前参数索引时适用（如2个参数适用于索引0/1）
            if (item.Parameters.Length >= argumentCount)
            {
                return true;
            }

            // 可变参数项适用于任意参数数量
            if (item.IsVariadic)
            {
                return true;
            }

            // 特殊处理参数数量为0的场景（如"Goo("且无参数时仍判定为适用）
            return argumentCount == 0;
        }
    }
}
