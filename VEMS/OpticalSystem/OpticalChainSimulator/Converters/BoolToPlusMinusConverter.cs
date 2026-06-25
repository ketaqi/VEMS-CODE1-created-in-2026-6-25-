using System;
using Avalonia.Data.Converters;
using System.Globalization;

namespace OpticalChainSimulator.Converters;

/// <summary>
/// 布尔值转加减符号转换器
/// 实现IValueConverter接口，将表示“展开/折叠”状态的布尔值转换为全角加减符号，用于UI折叠面板等交互场景
/// 核心规则：true（展开）→ "−"（全角减号），false（折叠）→ "+"（全角加号）
/// </summary>
public class BoolToPlusMinusConverter : IValueConverter
{
    /// <summary>
    /// 正向转换：布尔值（展开状态）转全角加减符号
    /// </summary>
    /// <param name="value">源绑定值，期望为bool类型（表示是否展开）</param>
    /// <param name="targetType">目标绑定类型（本转换器返回string，此参数未实际使用）</param>
    /// <param name="parameter">转换器参数（本转换器未使用）</param>
    /// <param name="culture">文化信息（本转换器未使用，符号显示不依赖区域设置）</param>
    /// <returns>布尔值为true返回全角减号"−"，为false返回全角加号"+"；非布尔值默认返回"+"</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // 判断输入是否为布尔值，根据展开状态返回对应全角符号（确保显示一致性）
        if (value is bool isExpanded)
        {
            return isExpanded ? "−" : "+";  // ✅ 使用全角符号确保显示
        }
        // 非布尔值默认返回折叠状态的加号
        return "+";
    }

    /// <summary>
    /// 反向转换：本转换器仅支持单向转换（布尔→符号），反向逻辑未实现
    /// </summary>
    /// <param name="value">目标绑定值（本转换器未使用）</param>
    /// <param name="targetType">源绑定类型（本转换器未使用）</param>
    /// <param name="parameter">转换器参数（本转换器未使用）</param>
    /// <param name="culture">文化信息（本转换器未使用）</param>
    /// <exception cref="NotImplementedException">调用反向转换时始终抛出该异常</exception>
    /// <returns>无有效返回值（异常终止）</returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}