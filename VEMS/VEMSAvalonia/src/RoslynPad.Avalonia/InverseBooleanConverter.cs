using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// 菜单需要

namespace RoslynPad;

/// <summary>
/// 反向布尔值转换器（Avalonia 版）。
/// </summary>
/// <remarks>
/// 用途：在绑定中对布尔值取反，常用于 <c>IsEnabled</c> / <c>IsVisible</c> 这类目标属性需要“与源相反状态”的场景。<br/>
/// 典型用法（XAML）：
/// <code language="xml">
/// &lt;Window xmlns:local="clr-namespace:RoslynPad"&gt;
///   &lt;Window.Resources&gt;
///     &lt;local:InverseBooleanConverter x:Key="InverseBooleanConverter"/&gt;
///   &lt;/Window.Resources&gt;
///
///   &lt;Button
///       Content="提交"
///       IsEnabled="{Binding IsBusy, Converter={StaticResource InverseBooleanConverter}}"/&gt;
/// &lt;/Window&gt;
/// </code>
/// 线程模型：仅用于 UI 绑定转换，不涉及跨线程访问。  
/// 兼容性：当源值不是 <see cref="bool"/> 时，保持原值返回（不抛异常）。
/// </remarks>
public class InverseBooleanConverter : IValueConverter
{
    /// <summary>
    /// 将布尔值取反后返回；若源值不是 <see cref="bool"/>，则原样返回。
    /// </summary>
    /// <param name="value">源值；期望为 <see cref="bool"/> 或 <see cref="bool?"/>。</param>
    /// <param name="targetType">目标类型（通常为 <see cref="bool"/>）。</param>
    /// <param name="parameter">未使用。</param>
    /// <param name="culture">区域信息（未使用）。</param>
    /// <returns>
    /// 当 <paramref name="value"/> 为布尔时返回其逻辑非；否则返回 <paramref name="value"/> 原值。
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;

    /// <summary>
    /// 反向转换同样执行逻辑非；若目标值不是 <see cref="bool"/>，则原样返回。
    /// </summary>
    /// <param name="value">目标值；期望为 <see cref="bool"/> 或 <see cref="bool?"/>。</param>
    /// <param name="targetType">源类型（通常为 <see cref="bool"/>）。</param>
    /// <param name="parameter">未使用。</param>
    /// <param name="culture">区域信息（未使用）。</param>
    /// <returns>
    /// 当 <paramref name="value"/> 为布尔时返回其逻辑非；否则返回 <paramref name="value"/> 原值。
    /// </returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;
}
