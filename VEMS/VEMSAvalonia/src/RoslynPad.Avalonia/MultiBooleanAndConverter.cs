using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace RoslynPad
{
    /// <summary>
    /// 多值布尔“与”转换器（支持区域匹配）。
    /// </summary>
    /// <remarks>
    /// 用途：在 <c>MultiBinding</c> 中同时判断多个条件是否满足，以决定某控件的可见性/启用等状态。<br/>
    /// 规则：
    /// <list type="number">
    /// <item>
    /// <description><b>values.Count == 2</b>：<c>values[0]</c> 视为 <c>isVisible</c>（bool），<c>values[1]</c> 视为 <c>region</c>（int）；返回 <c>isVisible &amp;&amp; region == targetRegion</c>。</description>
    /// </item>
    /// <item>
    /// <description><b>values.Count &gt;= 3</b>：<c>values[0]</c> 视为 <c>isVisible</c>（bool），<c>values[1]</c> 视为 <c>isFloating</c>（bool），<c>values[2]</c> 视为 <c>region</c>（int）；返回 <c>isVisible &amp;&amp; isFloating &amp;&amp; region == targetRegion</c>。</description>
    /// </item>
    /// </list>
    /// 其中 <c>targetRegion</c> 由 <c>ConverterParameter</c> 传入（字符串型数字），默认值为 <c>1</c>。<br/>
    /// 示例（XAML）：
    /// <code language="xml">
    /// &lt;MultiBinding Converter="{StaticResource MultiBooleanAndConverter}" ConverterParameter="2"&gt;
    ///   &lt;Binding Path="IsVisible"/&gt;
    ///   &lt;Binding Path="Region"/&gt;
    /// &lt;/MultiBinding&gt;
    /// </code>
    /// 或包含浮动状态的三参：
    /// <code language="xml">
    /// &lt;MultiBinding Converter="{StaticResource MultiBooleanAndConverter}" ConverterParameter="3"&gt;
    ///   &lt;Binding Path="IsVisible"/&gt;
    ///   &lt;Binding Path="IsFloating"/&gt;
    ///   &lt;Binding Path="Region"/&gt;
    /// &lt;/MultiBinding&gt;
    /// </code>
    /// 线程模型：仅供绑定使用，不涉及跨线程访问。
    /// </remarks>
    public class MultiBooleanAndConverter : IMultiValueConverter
    {
        /// <summary>
        /// 依据 <paramref name="values"/> 的个数与 <paramref name="parameter"/>（目标区域）进行“与”判断。
        /// </summary>
        /// <param name="values">
        /// 绑定值列表：<br/>
        /// - 当数量为 2：<c>[isVisible(bool), region(int)]</c>；<br/>
        /// - 当数量 ≥ 3：<c>[isVisible(bool), isFloating(bool), region(int), ...]</c>（多余项忽略）。
        /// </param>
        /// <param name="targetType">目标类型（通常是 <see cref="bool"/> 或 <see cref="object"/>）。</param>
        /// <param name="parameter">区域匹配参数（字符串数字），如 <c>"1"</c>；解析失败则取默认值 <c>1</c>。</param>
        /// <param name="culture">区域信息（未使用）。</param>
        /// <returns>
        /// 当满足约定条件时返回 <see langword="true"/>；否则返回 <see langword="false"/>。当 <paramref name="values"/> 为空或数量不符合时也返回 <see langword="false"/>。
        /// </returns>
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count == 2)
            {
                // 只判断可见性和区域
                bool isVisible = values[0] is bool b1 && b1;
                int region = values[1] is int i ? i : 0;

                // 解析目标区域；parameter 期望为字符串数字，失败则默认 1
                int targetRegion = parameter is string s && int.TryParse(s, out var p) ? p : 1;

                return isVisible && region == targetRegion;
            }
            else if (values.Count >= 3)
            {
                // 判断可见性、浮动状态和区域
                bool isVisible = values[0] is bool b1 && b1;
                bool isFloating = values[1] is bool b2 && b2;
                int region = values[2] is int i ? i : 0;

                int targetRegion = parameter is string s && int.TryParse(s, out var p) ? p : 1;

                return isVisible && isFloating && region == targetRegion;
            }

            // 数量不足时，无法满足逻辑
            return false;
        }

        /// <summary>
        /// 不支持反向转换；多值“与”无法从单个布尔结果推导原始多个输入。
        /// </summary>
        /// <param name="value">目标值（通常为 <see cref="bool"/>）。</param>
        /// <param name="targetTypes">源类型数组。</param>
        /// <param name="parameter">转换参数（未使用）。</param>
        /// <param name="culture">区域信息（未使用）。</param>
        /// <returns>不返回；始终抛出 <see cref="NotImplementedException"/>。</returns>
        /// <exception cref="NotImplementedException">始终抛出，表示不支持反向转换。</exception>
        public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();

        // ——— 旧实现备忘（保留以便回滚/参考） ———
        // public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        // {
        //     if (values.Count < 3) return false;
        //     bool isVisible = values[0] is bool b1 && b1;
        //     bool isFloating = values[1] is bool b2 && b2;
        //     int region = values[2] is int i ? i : 0;
        //     int targetRegion = parameter is string s && int.TryParse(s, out var p) ? p : 1;
        //     return isVisible && isFloating && region == targetRegion;
        // }
        // public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
