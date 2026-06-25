using System;
using Avalonia.Data.Converters;
using System.Globalization;

namespace OpticalChainSimulator.Converters
{
    /// <summary>
    /// 布尔值与下拉框索引双向转换器
    /// 实现IValueConverter接口，用于布尔值和ComboBox.SelectedIndex的双向转换
    /// 核心规则：false ↔ 0，true ↔ 1，适配仅有两个选项的下拉框场景
    /// </summary>
    public class BoolToIndexConverter : IValueConverter
    {
        /// <summary>
        /// 正向转换：布尔值转ComboBox选中索引
        /// </summary>
        /// <param name="value">源绑定值，期望为bool类型</param>
        /// <param name="targetType">目标绑定类型（本转换器返回int，此参数未实际使用）</param>
        /// <param name="parameter">转换器参数（本转换器未使用）</param>
        /// <param name="culture">文化信息（本转换器未使用，数值转换不依赖区域设置）</param>
        /// <returns>布尔值为true返回1，为false返回0；非布尔值默认返回0</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 布尔值转索引：true→1，false→0；非布尔值默认返回0
            if (value is bool b) return b ? 1 : 0;
            return 0;
        }

        /// <summary>
        /// 反向转换：ComboBox选中索引转布尔值
        /// 支持int类型索引或可解析为int的string类型索引
        /// </summary>
        /// <param name="value">目标绑定值，支持int/string类型（表示下拉框选中索引）</param>
        /// <param name="targetType">源绑定类型（本转换器返回bool，此参数未实际使用）</param>
        /// <param name="parameter">转换器参数（本转换器未使用）</param>
        /// <param name="culture">文化信息（本转换器仅用于基础数值解析，无特殊区域依赖）</param>
        /// <returns>索引为0返回false，非0返回true；无法解析为有效int时默认返回false</returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // int类型索引：0→false，非0→true
            if (value is int i) return i != 0;
            // string类型索引：尝试解析为int后按上述规则转换
            if (value is string s && int.TryParse(s, out var iv)) return iv != 0;
            // 无效值默认返回false
            return false;
        }
    }
}