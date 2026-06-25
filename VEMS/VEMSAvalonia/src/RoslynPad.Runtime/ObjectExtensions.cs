using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace RoslynPad.Runtime;

/// <summary>
/// 提供对象的 Dump 扩展方法。
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// 将对象输出到结果面板。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    /// <param name="o">要输出的对象。</param>
    /// <param name="header">输出标题（默认为调用表达式）。</param>
    /// <param name="maxDepth">最大遍历深度。</param>
    /// <param name="maxExpandedDepth">最大展开深度。</param>
    /// <param name="maxEnumerableLength">最大枚举长度。</param>
    /// <param name="maxStringLength">最大字符串长度。</param>
    /// <param name="line">调用行号（自动获取）。</param>
    /// <returns>原始对象（支持链式调用）。</returns>
    public static T Dump<T>(this T o, [CallerArgumentExpression(nameof(o))] string? header = null, int maxDepth = DumpQuotas.DefaultMaxDepth, int maxExpandedDepth = DumpQuotas.DefaultMaxExpandedDepth, int maxEnumerableLength = DumpQuotas.DefaultMaxEnumerableLength, int maxStringLength = DumpQuotas.DefaultMaxStringLength, [CallerLineNumber] int? line = null)
    {
        Dumped?.Invoke(new DumpData(o, header, line, new DumpQuotas(maxDepth, maxExpandedDepth, maxEnumerableLength, maxStringLength)));
        return o;
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// 将 Span 输出到结果面板。
    /// </summary>
    public static Span<T> Dump<T>(this Span<T> o, [CallerArgumentExpression(nameof(o))] string? header = null, int maxDepth = DumpQuotas.DefaultMaxDepth, int maxExpandedDepth = DumpQuotas.DefaultMaxExpandedDepth, int maxEnumerableLength = DumpQuotas.DefaultMaxEnumerableLength, int maxStringLength = DumpQuotas.DefaultMaxStringLength, [CallerLineNumber] int? line = null)
    {
        Dump(o.ToArray(), header, maxDepth, maxExpandedDepth, maxEnumerableLength, maxStringLength, line);
        return o;
    }

    /// <summary>
    /// 将 ReadOnlySpan 输出到结果面板。
    /// </summary>
    public static ReadOnlySpan<T> Dump<T>(this ReadOnlySpan<T> o, [CallerArgumentExpression(nameof(o))] string? header = null, int maxDepth = DumpQuotas.DefaultMaxDepth, int maxExpandedDepth = DumpQuotas.DefaultMaxExpandedDepth, int maxEnumerableLength = DumpQuotas.DefaultMaxEnumerableLength, int maxStringLength = DumpQuotas.DefaultMaxStringLength, [CallerLineNumber] int? line = null)
    {
        Dump(o.ToArray(), header, maxDepth, maxExpandedDepth, maxEnumerableLength, maxStringLength, line);
        return o;
    }

    /// <summary>
    /// 将 Memory 输出到结果面板。
    /// </summary>
    public static Memory<T> Dump<T>(this Memory<T> o, [CallerArgumentExpression(nameof(o))] string? header = null, int maxDepth = DumpQuotas.DefaultMaxDepth, int maxExpandedDepth = DumpQuotas.DefaultMaxExpandedDepth, int maxEnumerableLength = DumpQuotas.DefaultMaxEnumerableLength, int maxStringLength = DumpQuotas.DefaultMaxStringLength, [CallerLineNumber] int? line = null)
    {
        Dump(o.ToArray(), header, maxDepth, maxExpandedDepth, maxEnumerableLength, maxStringLength, line);
        return o;
    }

    /// <summary>
    /// 将 ReadOnlyMemory 输出到结果面板。
    /// </summary>
    public static ReadOnlyMemory<T> Dump<T>(this ReadOnlyMemory<T> o, [CallerArgumentExpression(nameof(o))] string? header = null, int maxDepth = DumpQuotas.DefaultMaxDepth, int maxExpandedDepth = DumpQuotas.DefaultMaxExpandedDepth, int maxEnumerableLength = DumpQuotas.DefaultMaxEnumerableLength, int maxStringLength = DumpQuotas.DefaultMaxStringLength, [CallerLineNumber] int? line = null)
    {
        Dump(o.ToArray(), header, maxDepth, maxExpandedDepth, maxEnumerableLength, maxStringLength, line);
        return o;
    }
#endif

    /// <summary>
    /// 使用选择器转换对象后输出到结果面板。
    /// </summary>
    /// <typeparam name="T">源对象类型。</typeparam>
    /// <typeparam name="TResult">转换后的类型。</typeparam>
    /// <param name="o">要输出的对象。</param>
    /// <param name="selector">转换选择器。</param>
    /// <param name="header">输出标题。</param>
    /// <param name="maxDepth">最大遍历深度。</param>
    /// <param name="maxExpandedDepth">最大展开深度。</param>
    /// <param name="maxEnumerableLength">最大枚举长度。</param>
    /// <param name="maxStringLength">最大字符串长度。</param>
    /// <param name="line">调用行号。</param>
    /// <returns>原始对象（支持链式调用）。</returns>
    public static T DumpAs<T, TResult>(this T o, Func<T, TResult>? selector, [CallerArgumentExpression(nameof(o))] string? header = null, int maxDepth = DumpQuotas.DefaultMaxDepth, int maxExpandedDepth = DumpQuotas.DefaultMaxExpandedDepth, int maxEnumerableLength = DumpQuotas.DefaultMaxEnumerableLength, int maxStringLength = DumpQuotas.DefaultMaxStringLength, [CallerLineNumber] int? line = null)
    {
        Dump(selector != null ? (object?)selector.Invoke(o) : null, header, maxDepth, maxExpandedDepth, maxEnumerableLength, maxStringLength, line);
        return o;
    }

    /// <summary>
    /// 输出枚举的第一个元素。
    /// </summary>
    /// <typeparam name="TEnumerable">枚举类型。</typeparam>
    /// <param name="enumerable">枚举对象。</param>
    /// <param name="header">输出标题。</param>
    /// <param name="maxDepth">最大遍历深度。</param>
    /// <param name="maxExpandedDepth">最大展开深度。</param>
    /// <param name="maxEnumerableLength">最大枚举长度。</param>
    /// <param name="maxStringLength">最大字符串长度。</param>
    /// <param name="line">调用行号。</param>
    /// <returns>原始枚举（支持链式调用）。</returns>
    public static TEnumerable DumpFirst<TEnumerable>(this TEnumerable enumerable, [CallerArgumentExpression(nameof(enumerable))] string? header = null, int maxDepth = DumpQuotas.DefaultMaxDepth, int maxExpandedDepth = DumpQuotas.DefaultMaxExpandedDepth, int maxEnumerableLength = DumpQuotas.DefaultMaxEnumerableLength, int maxStringLength = DumpQuotas.DefaultMaxStringLength, [CallerLineNumber] int? line = null)
        where TEnumerable : IEnumerable
    {
        Dump(enumerable?.Cast<object>().FirstOrDefault(), header, maxDepth, maxExpandedDepth, maxEnumerableLength, maxStringLength, line);
        return enumerable!;
    }

    /// <summary>
    /// 输出枚举的最后一个元素。
    /// </summary>
    /// <typeparam name="TEnumerable">枚举类型。</typeparam>
    /// <param name="enumerable">枚举对象。</param>
    /// <param name="header">输出标题。</param>
    /// <param name="maxDepth">最大遍历深度。</param>
    /// <param name="maxExpandedDepth">最大展开深度。</param>
    /// <param name="maxEnumerableLength">最大枚举长度。</param>
    /// <param name="maxStringLength">最大字符串长度。</param>
    /// <param name="line">调用行号。</param>
    /// <returns>原始枚举（支持链式调用）。</returns>
    public static TEnumerable DumpLast<TEnumerable>(this TEnumerable enumerable, [CallerArgumentExpression(nameof(enumerable))] string? header = null, int maxDepth = DumpQuotas.DefaultMaxDepth, int maxExpandedDepth = DumpQuotas.DefaultMaxExpandedDepth, int maxEnumerableLength = DumpQuotas.DefaultMaxEnumerableLength, int maxStringLength = DumpQuotas.DefaultMaxStringLength, [CallerLineNumber] int? line = null)
        where TEnumerable : IEnumerable
    {
        Dump(enumerable?.Cast<object>().LastOrDefault(), header, maxDepth, maxExpandedDepth, maxEnumerableLength, maxStringLength, line);
        return enumerable!;
    }

    /// <summary>
    /// 输出枚举中指定索引的元素。
    /// </summary>
    /// <typeparam name="TEnumerable">枚举类型。</typeparam>
    /// <param name="enumerable">枚举对象。</param>
    /// <param name="index">元素索引。</param>
    /// <param name="header">输出标题。</param>
    /// <param name="maxDepth">最大遍历深度。</param>
    /// <param name="maxExpandedDepth">最大展开深度。</param>
    /// <param name="maxEnumerableLength">最大枚举长度。</param>
    /// <param name="maxStringLength">最大字符串长度。</param>
    /// <param name="line">调用行号。</param>
    /// <returns>原始枚举（支持链式调用）。</returns>
    public static TEnumerable DumpElementAt<TEnumerable>(this TEnumerable enumerable, int index, [CallerArgumentExpression(nameof(enumerable))] string? header = null, int maxDepth = DumpQuotas.DefaultMaxDepth, int maxExpandedDepth = DumpQuotas.DefaultMaxExpandedDepth, int maxEnumerableLength = DumpQuotas.DefaultMaxEnumerableLength, int maxStringLength = DumpQuotas.DefaultMaxStringLength, [CallerLineNumber] int? line = null)
        where TEnumerable : IEnumerable
    {
        Dump(enumerable?.Cast<object>().ElementAtOrDefault(index), header, maxDepth, maxExpandedDepth, maxEnumerableLength, maxStringLength, line);
        return enumerable!;
    }

    /// <summary>
    /// Dump 事件，当调用 Dump 方法时触发。
    /// </summary>
    internal static event DumpDelegate? Dumped;

    /// <summary>
    /// Dump 委托类型。
    /// </summary>
    /// <param name="data">Dump 数据。</param>
    internal delegate void DumpDelegate(in DumpData data);
}

/// <summary>
/// Dump 数据结构。
/// </summary>
/// <param name="Object">要输出的对象。</param>
/// <param name="Header">输出标题。</param>
/// <param name="Line">调用行号。</param>
/// <param name="Quotas">输出配额限制。</param>
internal record struct DumpData(object? Object, string? Header, int? Line, DumpQuotas Quotas);

/// <summary>
/// Dump 输出配额限制。
/// </summary>
/// <param name="MaxDepth">最大遍历深度。</param>
/// <param name="MaxExpandedDepth">最大展开深度。</param>
/// <param name="MaxEnumerableLength">最大枚举长度。</param>
/// <param name="MaxStringLength">最大字符串长度。</param>
internal record struct DumpQuotas(int MaxDepth, int MaxExpandedDepth, int MaxEnumerableLength, int MaxStringLength)
{
    /// <summary>默认最大深度。</summary>
    internal const int DefaultMaxDepth = 4;
    /// <summary>默认最大展开深度。</summary>
    internal const int DefaultMaxExpandedDepth = 1;
    /// <summary>默认最大字符串长度。</summary>
    internal const int DefaultMaxStringLength = 10000;
    /// <summary>默认最大枚举长度。</summary>
    internal const int DefaultMaxEnumerableLength = 10000;

    /// <summary>获取默认配额。</summary>
    public static DumpQuotas Default { get; } = new DumpQuotas(DefaultMaxDepth, DefaultMaxExpandedDepth, DefaultMaxEnumerableLength, DefaultMaxStringLength);

    /// <summary>
    /// 返回深度减一的新配额。
    /// </summary>
    [Pure]
    internal DumpQuotas StepDown() => this with { MaxDepth = MaxDepth - 1, MaxExpandedDepth = MaxExpandedDepth - 1 };
}
