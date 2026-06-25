namespace RoslynPad.Build;

/// <summary>
/// 提供 <see cref="IEnumerable{T}"/> 的扩展方法。
/// </summary>
internal static class EnumerableExtensions
{
    /// <summary>
    /// 查找满足条件的第一个元素的索引。
    /// </summary>
    /// <typeparam name="T">元素类型。</typeparam>
    /// <param name="enumerable">要搜索的枚举。</param>
    /// <param name="predicate">匹配条件。</param>
    /// <returns>第一个满足条件的元素索引，未找到则返回 -1。</returns>
    public static int IndexOf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
    {
        var index = 0;

        foreach (var item in enumerable)
        {
            if (predicate(item))
            {
                return index;
            }

            ++index;
        }

        return -1;
    }
}
