using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RoslynPad.Converters
{
    /// <summary>
    /// 将 0~1 之间的小数转换为百分比文本的转换器。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 转换规则：
    /// <list type="bullet">
    ///   <item><description>空值按 <c>0</c> 处理</description></item>
    ///   <item><description>输入值被钳制在 <c>[0, 1]</c> 区间</description></item>
    ///   <item><description>结果四舍五入为整数百分比并追加 <c>"%"</c></description></item>
    /// </list>
    /// </para>
    /// <para>
    /// 此转换器为单向转换，不支持 <see cref="ConvertBack"/>。
    /// </para>
    /// </remarks>
    /// <example>
    /// XAML 使用示例：
    /// <code language="xml">
    /// <![CDATA[
    /// <TextBlock Text="{Binding Progress, Converter={StaticResource DoubleToPercentageTextConverter}}"/>
    /// 
    /// <!-- 示例值映射 -->
    /// <!-- 0. 0   → "0%"   -->
    /// <!-- 0.5   → "50%"  -->
    /// <!-- 0.756 → "76%"  -->
    /// <!-- 1.0   → "100%" -->
    /// ]]>
    /// </code>
    /// </example>
    public class DoubleToPercentageTextConverter : IValueConverter
    {
        /// <summary>
        /// 将 0~1 的小数转换为百分比字符串。
        /// </summary>
        /// <param name="value">
        /// 源值，期望为 <see cref="double"/> 或 <see cref="Nullable{Double}"/>。
        /// 空值按 0 处理。
        /// </param>
        /// <param name="targetType">目标类型，通常为 <see cref="string"/>。</param>
        /// <param name="parameter">转换参数（未使用）。</param>
        /// <param name="culture">区域信息（未使用）。</param>
        /// <returns>
        /// 格式为 <c>"0%"</c> 到 <c>"100%"</c> 的百分比字符串。
        /// </returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 读取数值，空值按 0 处理
            var percent = value as double? ?? 0;

            // 钳制范围到 [0, 1]
            percent = Math.Clamp(percent, 0, 1);

            // 乘以 100 后四舍五入为整数，拼接百分号
            var intPercent = (int)Math.Round(percent * 100.0);
            return $"{intPercent}%";
        }

        /// <summary>
        /// 不支持反向转换。
        /// </summary>
        /// <param name="value">目标值。</param>
        /// <param name="targetType">源类型。</param>
        /// <param name="parameter">转换参数。</param>
        /// <param name="culture">区域信息。</param>
        /// <returns>始终返回 <see langword="null"/>。</returns>
        /// <remarks>
        /// 从百分比字符串反向转换为小数存在歧义（如 "75%" 可能来自 0.749 或 0.754），
        /// 因此不支持反向转换。
        /// </remarks>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => null;
    }
}
