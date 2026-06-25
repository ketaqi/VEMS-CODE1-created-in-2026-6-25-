using System.Buffers;

namespace RoslynPad.Converters;

/// <summary>
/// 字符串搜索工具类：提供高性能的字符索引查找功能。
/// </summary>
/// <remarks>
/// <para>
/// 此类利用 .NET 8+ 的 <see cref="SearchValues{T}"/> 进行优化的字符搜索，
/// 提供零分配（zero-allocation）的索引枚举能力。
/// </para>
/// <para>
/// 典型使用场景：
/// <list type="bullet">
///   <item><description>查找文本中所有特定字符的位置</description></item>
///   <item><description>高性能的控制字符检测</description></item>
///   <item><description>文本高亮的位置计算</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var searchValues = SearchValues.Create(['a', 'e', 'i', 'o', 'u']);
/// foreach (var index in StringSearch.GetIndices("hello world", searchValues))
/// {
///     Console.WriteLine($"元音字母位置: {index}");
/// }
/// </code>
/// </example>
public static class StringSearch
{
    /// <summary>
    /// 获取文本中所有匹配字符的索引枚举器。
    /// </summary>
    /// <param name="text">要搜索的文本。</param>
    /// <param name="searchValues">要查找的字符集合。</param>
    /// <returns>可枚举的索引枚举器（支持 <c>foreach</c>）。</returns>
    /// <remarks>
    /// 此方法返回一个结构体枚举器，避免了堆分配，适合高频调用场景。
    /// </remarks>
    public static IndicesEnumerator GetIndices(string text, SearchValues<char> searchValues) => new(text, searchValues);

    /// <summary>
    /// 字符索引枚举器：以零分配方式枚举文本中所有匹配字符的位置。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此结构体实现了枚举器模式，但为了性能不实现 <see cref="IEnumerable{T}"/> 接口。
    /// 可直接用于 <c>foreach</c> 语句。
    /// </para>
    /// <para>
    /// 内部使用 <see cref="MemoryExtensions.IndexOfAny{T}(ReadOnlySpan{T}, SearchValues{T})"/> 
    /// 进行 SIMD 优化的搜索。
    /// </para>
    /// </remarks>
    public struct IndicesEnumerator
    {
        private readonly string _text;
        private readonly SearchValues<char> _searchValues;

        /// <summary>
        /// 初始化 <see cref="IndicesEnumerator"/> 结构的新实例。
        /// </summary>
        /// <param name="text">要搜索的文本。</param>
        /// <param name="searchValues">要查找的字符集合。</param>
        public IndicesEnumerator(string text, SearchValues<char> searchValues)
        {
            _text = text;
            _searchValues = searchValues;
            Reset();
        }

        /// <summary>
        /// 获取当前匹配字符的索引位置。
        /// </summary>
        /// <value>
        /// 当前字符在原始文本中的从零开始的索引；
        /// 如果尚未调用 <see cref="MoveNext"/> 或已枚举完毕，值为 -1。
        /// </value>
        public int Current { get; private set; }

        /// <summary>
        /// 前进到下一个匹配的字符位置。
        /// </summary>
        /// <returns>
        /// 如果找到下一个匹配字符返回 <c>true</c>；
        /// 如果已到达文本末尾返回 <c>false</c>。
        /// </returns>
        public bool MoveNext()
        {
            var current = Current + 1;
            var index = _text.AsSpan(current).IndexOfAny(_searchValues);
            if (index >= 0)
            {
                Current = current + index;
                return true;
            }

            Reset();
            return false;
        }

        /// <summary>
        /// 将枚举器重置到初始状态。
        /// </summary>
        public void Reset() => Current = -1;

        /// <summary>
        /// 返回枚举器自身（支持 <c>foreach</c> 语法）。
        /// </summary>
        /// <returns>当前枚举器实例。</returns>
        public readonly IndicesEnumerator GetEnumerator() => this;
    }
}
