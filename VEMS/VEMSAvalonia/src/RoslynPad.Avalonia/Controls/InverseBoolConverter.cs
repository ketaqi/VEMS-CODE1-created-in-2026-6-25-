using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace RoslynPad.Converters;

/// <summary>
/// 布尔值取反转换器：将 <see langword="true"/> 转换为 <see langword="false"/>，反之亦然。
/// </summary>
/// <remarks>
/// <para>
/// 此转换器用于 XAML 绑定中需要"反向逻辑"的场景，例如：
/// <list type="bullet">
///   <item><description><c>IsEnabled="{Binding IsLoading, Converter={x:Static converters:InverseBoolConverter.Instance}}"</c></description></item>
///   <item><description><c>IsVisible="{Binding IsEmpty, Converter={x:Static converters:InverseBoolConverter.Instance}}"</c></description></item>
/// </list>
/// </para>
/// <para>
/// 该转换器是对称的：<see cref="Convert"/> 和 <see cref="ConvertBack"/> 执行相同的取反操作。
/// </para>
/// <para>
/// 非布尔输入处理：当输入值不是 <see cref="bool"/> 类型时，返回 <see cref="AvaloniaProperty.UnsetValue"/>，
/// 这将导致绑定使用回退值或默认值。
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;!-- 在 XAML 资源中引用 --&gt;
/// &lt;Window xmlns:converters="clr-namespace:RoslynPad.Converters"&gt;
///     
///     &lt;!-- 使用单例实例 --&gt;
///     &lt;Button IsEnabled="{Binding IsBusy, 
///             Converter={x:Static converters:InverseBoolConverter.Instance}}" /&gt;
///     
///     &lt;!-- 或在资源中声明 --&gt;
///     &lt;Window.Resources&gt;
///         &lt;converters:InverseBoolConverter x:Key="InverseBool" /&gt;
///     &lt;/Window.Resources&gt;
///     &lt;Button IsEnabled="{Binding IsBusy, Converter={StaticResource InverseBool}}" /&gt;
/// &lt;/Window&gt;
/// </code>
/// </example>
public class InverseBoolConverter : IValueConverter
{
    /// <summary>
    /// 获取转换器的单例实例。
    /// </summary>
    /// <value>
    /// 可在 XAML 中通过 <c>{x:Static converters:InverseBoolConverter.Instance}</c> 引用，
    /// 避免重复创建实例。
    /// </value>
    public static readonly InverseBoolConverter Instance = new();

    /// <summary>
    /// 将布尔值转换为其逻辑非。
    /// </summary>
    /// <param name="value">源值，期望为 <see cref="bool"/> 类型。</param>
    /// <param name="targetType">目标类型（通常为 <see cref="bool"/>）。</param>
    /// <param name="parameter">转换器参数（未使用）。</param>
    /// <param name="culture">区域信息（未使用）。</param>
    /// <returns>
    /// 如果 <paramref name="value"/> 为 <see cref="bool"/>，返回其逻辑非；
    /// 否则返回 <see cref="AvaloniaProperty.UnsetValue"/>。
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return AvaloniaProperty.UnsetValue;
    }

    /// <summary>
    /// 将布尔值反向转换（同样执行逻辑非）。
    /// </summary>
    /// <param name="value">目标值，期望为 <see cref="bool"/> 类型。</param>
    /// <param name="targetType">源类型（通常为 <see cref="bool"/>）。</param>
    /// <param name="parameter">转换器参数（未使用）。</param>
    /// <param name="culture">区域信息（未使用）。</param>
    /// <returns>
    /// 如果 <paramref name="value"/> 为 <see cref="bool"/>，返回其逻辑非；
    /// 否则返回 <see cref="AvaloniaProperty.UnsetValue"/>。
    /// </returns>
    /// <remarks>
    /// 由于取反操作是对称的，<see cref="ConvertBack"/> 与 <see cref="Convert"/> 行为完全相同。
    /// </remarks>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return AvaloniaProperty.UnsetValue;
    }
}
