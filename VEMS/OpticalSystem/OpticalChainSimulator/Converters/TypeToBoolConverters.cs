using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace OpticalChainSimulator.Converters;

/// <summary>
/// 光学元件类型相等性布尔转换器
/// 实现IValueConverter接口，用于XAML绑定中判断光学元件类型（OpticalElementType）是否与指定参数匹配，返回布尔值
/// 核心用途：根据元件类型动态控制UI元素的可见性、可用性等（如仅对光源类型显示特定配置项）
/// XAML使用示例：
/// <code>
/// IsVisible="{Binding Type, Converter={StaticResource TypeEqualsConverter}, ConverterParameter=Source}"
/// </code>
/// 注意：ConverterParameter必须是OpticalElementType枚举的有效名称（字符串形式）
/// </summary>
public class TypeEqualsConverter : IValueConverter
{
    /// <summary>
    /// 正向转换：将源绑定值（光学元件类型）与转换器参数（目标类型）比较，返回是否相等的布尔值
    /// </summary>
    /// <param name="value">源绑定值，支持两种类型：
    /// 1. OpticalElementType枚举实例；
    /// 2. 可解析为OpticalElementType的字符串
    /// </param>
    /// <param name="targetType">目标绑定类型（本转换器固定返回bool，此参数未实际使用）</param>
    /// <param name="parameter">转换器参数，需为OpticalElementType枚举的字符串名称</param>
    /// <param name="culture">文化信息（本转换器未使用，因枚举解析不依赖区域设置）</param>
    /// <returns>源值与目标参数类型相等返回true；值/参数无效、解析失败时返回false</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        // 转换器参数为空，直接返回false
        if (parameter is null)
            return false;

        // 定义变量存储当前解析后的光学元件类型
        OpticalChainSimulator.Models.OpticalElementType current;

        // 情况1：源值是OpticalElementType枚举类型，直接赋值
        if (value is OpticalChainSimulator.Models.OpticalElementType t)
        {
            current = t;
        }
        // 情况2：源值是字符串类型，尝试解析为OpticalElementType枚举
        else if (value is string vs &&
                 Enum.TryParse<OpticalChainSimulator.Models.OpticalElementType>(vs, out var parsedFromString))
        {
            current = parsedFromString;
        }
        // 源值类型无效（非枚举/非可解析字符串），返回false
        else
        {
            return false;
        }

        // 解析转换器参数（字符串）为目标OpticalElementType枚举，比较是否与当前类型相等
        if (parameter is string ps &&
            Enum.TryParse<OpticalChainSimulator.Models.OpticalElementType>(ps, out var target))
        {
            return current == target;
        }

        // 参数解析失败（非字符串/枚举名称无效），返回false
        return false;
    }

    /// <summary>
    /// 反向转换：本转换器为单向转换（仅用于判断类型相等），不支持反向逻辑
    /// </summary>
    /// <param name="value">目标绑定值（本转换器未使用）</param>
    /// <param name="targetType">源绑定类型（本转换器未使用）</param>
    /// <param name="parameter">转换器参数（本转换器未使用）</param>
    /// <param name="culture">文化信息（本转换器未使用）</param>
    /// <returns>始终返回BindingOperations.DoNothing，表示不执行任何反向转换操作</returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        return BindingOperations.DoNothing;
    }
}