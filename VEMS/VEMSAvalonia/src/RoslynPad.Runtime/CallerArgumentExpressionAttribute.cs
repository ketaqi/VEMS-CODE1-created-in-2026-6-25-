#if NETSTANDARD
namespace System.Runtime.CompilerServices;

/// <summary>
/// 允许捕获传递给方法的表达式的字符串表示形式。
/// 用于 .NET Standard 2.0 兼容性（.NET 5+ 已内置此特性）。
/// </summary>
/// <param name="parameterName">要捕获表达式的参数名称。</param>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
internal sealed class CallerArgumentExpressionAttribute(string parameterName) : Attribute
{
    /// <summary>
    /// 获取要捕获表达式的参数名称。
    /// </summary>
    public string ParameterName { get; } = parameterName;
}
#endif
