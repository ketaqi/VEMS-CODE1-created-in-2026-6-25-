using System.Runtime.CompilerServices;

namespace RoslynPad.UI;

/// <summary>
/// 对象扩展方法：提供通用的对象操作辅助功能。
/// </summary>
/// <remarks>
/// 此静态类包含适用于所有对象类型的扩展方法，
/// 用于简化常见的空值检查和断言操作。
/// </remarks>
public static class ObjectExtensions
{
    /// <summary>
    /// 确保值不为 <c>null</c>，如果为 <c>null</c> 则抛出异常。
    /// </summary>
    /// <typeparam name="T">值的类型。</typeparam>
    /// <param name="value">要检查的值。</param>
    /// <param name="expression">
    /// 调用时的表达式（由编译器通过 <see cref="CallerArgumentExpressionAttribute"/> 自动提供）。
    /// 用于生成更有意义的异常消息。
    /// </param>
    /// <returns>原始值（保证非空）。</returns>
    /// <exception cref="InvalidOperationException">当 <paramref name="value"/> 为 <c>null</c> 时抛出。</exception>
    /// <remarks>
    /// <para>
    /// 此方法用于在代码中明确表达"此处值不应为空"的断言，
    /// 比直接使用 <c>!</c> 操作符更具可读性，且能提供更好的错误信息。
    /// </para>
    /// <para>
    /// 异常消息将包含导致空值的表达式，便于调试。
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 简单用法
    /// var host = _roslynHost.NotNull();
    /// 
    /// // 链式调用
    /// var document = _viewModel.NotNull().Document.NotNull();
    /// 
    /// // 在属性访问中使用
    /// public RoslynHost RoslynHost => _roslynHost.NotNull();
    /// 
    /// // 异常消息示例：
    /// // "Expression not expected to be null: _roslynHost"
    /// </code>
    /// </example>
    public static T NotNull<T>(this T? value, [CallerArgumentExpression(nameof(value))] string expression = "") =>
        value ?? throw new InvalidOperationException("Expression not expected to be null: " + expression);
}
