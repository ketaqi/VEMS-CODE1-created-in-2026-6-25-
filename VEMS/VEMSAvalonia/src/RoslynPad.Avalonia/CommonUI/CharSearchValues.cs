using System.Buffers;
using System.Globalization;

namespace RoslynPad.UI;

/// <summary>
/// 字符搜索值集合：提供预定义的字符搜索值用于高性能文本处理。
/// </summary>
/// <remarks>
/// <para>
/// 此静态类提供预先计算的 <see cref="SearchValues{T}"/> 实例，
/// 用于高效检测文本中的控制字符等特殊字符。
/// </para>
/// <para>
/// <see cref="SearchValues{T}"/> 利用 SIMD 指令进行优化，
/// 比传统的循环检测性能高出数倍。
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 检查文本中是否包含控制字符
/// if (text.AsSpan().IndexOfAny(CharSearchValues.ControlChars) >= 0)
/// {
///     Console.WriteLine("文本包含控制字符");
/// }
/// 
/// // 获取所有控制字符的位置
/// foreach (var index in StringSearch.GetIndices(text, CharSearchValues.ControlChars))
/// {
///     Console.WriteLine($"控制字符位置: {index}");
/// }
/// </code>
/// </example>
public static class CharSearchValues
{
    /// <summary>
    /// 获取控制字符的搜索值集合。
    /// </summary>
    /// <value>
    /// 包含所有 Unicode 控制字符、格式字符和未分配字符（不含空白字符）的搜索值集合。
    /// </value>
    /// <remarks>
    /// <para>
    /// 包含的字符类别：
    /// <list type="bullet">
    ///   <item><description><see cref="UnicodeCategory.Control"/> - 控制字符（如 NUL、SOH 等）</description></item>
    ///   <item><description><see cref="UnicodeCategory.Format"/> - 格式字符（如零宽连接符等）</description></item>
    ///   <item><description><see cref="UnicodeCategory.OtherNotAssigned"/> - 未分配的代码点</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// 注意：空白字符（如空格、制表符、换行符）不包含在此集合中。
    /// </para>
    /// </remarks>
    public static SearchValues<char> ControlChars { get; } = SearchValues.Create(GetControlChars().ToArray());

    /// <summary>
    /// 枚举所有控制字符。
    /// </summary>
    /// <returns>控制字符的枚举。</returns>
    private static IEnumerable<char> GetControlChars() =>
        Enumerable.Range(char.MinValue, char.MaxValue + 1)
        .Select(c => (char)c)
        .Where(IsControl);

    /// <summary>
    /// 判断字符是否为控制字符。
    /// </summary>
    /// <param name="c">要检查的字符。</param>
    /// <returns>如果是控制字符（不含空白）返回 <c>true</c>。</returns>
    private static bool IsControl(char c) => !char.IsWhiteSpace(c) &&
        char.GetUnicodeCategory(c) switch
        {
            UnicodeCategory.Control or
            UnicodeCategory.Format or
            UnicodeCategory.OtherNotAssigned => true,
            _ => false,
        };
}
