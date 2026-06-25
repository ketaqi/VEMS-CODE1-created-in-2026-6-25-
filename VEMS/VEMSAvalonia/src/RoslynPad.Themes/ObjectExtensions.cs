using System.Runtime.CompilerServices;

namespace RoslynPad.Themes;

/// <summary>
/// 提供对象的扩展方法。
/// </summary>
internal static class ObjectExtensions
{
    /// <summary>
    /// 确保值不为 <c>null</c>，如果为 <c>null</c> 则抛出异常。
    /// </summary>
    /// <typeparam name="T">值的类型。</typeparam>
    /// <param name="value">要检查的值。</param>
    /// <param name="expression">调用时的表达式（由编译器自动提供）。</param>
    /// <returns>非空的值。</returns>
    /// <exception cref="InvalidOperationException">当值为 <c>null</c> 时抛出。</exception>
    public static T NotNull<T>(this T? value, [CallerArgumentExpression(nameof(value))] string expression = "") =>
        value ?? throw new InvalidOperationException("Expression not expected to be null: " + expression);
}
