using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace OpticalChainSimulator.Converters
{
    /// <summary>
    /// 布尔值取反转换器
    /// 实现IValueConverter接口，用于XAML绑定中对布尔值进行反向转换
    /// 核心用途：将原布尔值取反（true→false，false→true），例如控制UI元素的隐藏/显示、启用/禁用等反向逻辑
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        /// <summary>
        /// 正向转换：对输入的布尔值进行取反操作
        /// </summary>
        /// <param name="value">源绑定值，期望为bool类型</param>
        /// <param name="targetType">目标绑定类型（本转换器未实际使用）</param>
        /// <param name="parameter">转换器参数（本转换器未使用）</param>
        /// <param name="culture">文化信息（本转换器未使用）</param>
        /// <returns>输入为bool类型时返回其取反值；非bool类型时默认返回true</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 输入为布尔值则取反，否则返回true
            if (value is bool b) return !b;
            return true;
        }

        /// <summary>
        /// 反向转换：对输入的布尔值进行取反操作（与正向转换逻辑一致）
        /// </summary>
        /// <param name="value">目标绑定值，期望为bool类型</param>
        /// <param name="targetType">源绑定类型（本转换器未实际使用）</param>
        /// <param name="parameter">转换器参数（本转换器未使用）</param>
        /// <param name="culture">文化信息（本转换器未使用）</param>
        /// <returns>输入为bool类型时返回其取反值；非bool类型时默认返回true</returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 输入为布尔值则取反，否则返回true
            if (value is bool b) return !b;
            return true;
        }
    }
}