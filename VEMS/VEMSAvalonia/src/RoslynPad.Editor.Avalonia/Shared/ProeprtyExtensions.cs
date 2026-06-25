namespace RoslynPad.Editor;

/// <summary>
/// 为<see cref="PropertyOptions"/>枚举提供扩展方法的静态类
/// </summary>
public static class ProeprtyExtensions
{
    /// <summary>
    /// 检查<see cref="PropertyOptions"/>枚举值是否包含指定的目标值
    /// </summary>
    /// <param name="options">要检查的枚举实例</param>
    /// <param name="value">要验证是否包含的目标值</param>
    /// <returns>若包含目标值则返回true，否则返回false</returns>
    public static bool Has(this PropertyOptions options, PropertyOptions value) =>
        (options & value) == value;
}
