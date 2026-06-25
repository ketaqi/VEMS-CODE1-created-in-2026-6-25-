using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RoslynPad.CustomConverter
{
    //上方菜单栏快捷键切换选项卡时使用的转换器——ZDM
    /// <summary>
    /// 功能区（Ribbon）页签是否选中的转换器。
    /// </summary>
    /// <remarks>
    /// 用途：将当前“选中页签键（string）”与某个页签的“键（parameter）”进行比较，
    /// 若两者相等则返回 <see langword="true"/>，否则返回 <see langword="false"/>。
    /// 典型绑定方式：
    /// <code>
    /// IsSelected="{Binding SelectedTabKey,
    ///                      Converter={StaticResource RibbonTabKeyToIsSelectedConverter},
    ///                      ConverterParameter=Home}"
    /// </code>
    /// 线程模型：仅用于绑定，不涉及跨线程访问。
    /// </remarks>
    public class RibbonTabKeyToIsSelectedConverter : IValueConverter
    {
        /// <summary>
        /// 将“当前选中键”和“页签键参数”比较，得到布尔值用于 <c>IsSelected</c>。
        /// </summary>
        /// <param name="value">绑定源的当前选中键，期望为 <see cref="string"/>；可为空。</param>
        /// <param name="targetType">目标类型，通常为 <see cref="bool"/>。</param>
        /// <param name="parameter">页签自身的键（例如 <c>"Home"</c>），期望为 <see cref="string"/>；可为空。</param>
        /// <param name="culture">区域信息（未使用）。</param>
        /// <returns>
        /// 当 <paramref name="value"/> 与 <paramref name="parameter"/> 都为 <see cref="string"/> 且值相等时返回 <see langword="true"/>；
        /// 否则返回 <see langword="false"/>。
        /// </returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 使用 as 转换，避免类型不匹配抛异常；不相等（含任一为空）则返回 false
            var key = value as string;
            var tabKey = parameter as string;
            return key == tabKey;
        }

        /// <summary>
        /// 不支持反向转换：从布尔选中状态推导“选中键”没有明确语义。
        /// </summary>
        /// <param name="value">目标值（通常为 <see cref="bool"/>）。</param>
        /// <param name="targetType">源类型（通常为 <see cref="string"/>）。</param>
        /// <param name="parameter">页签键（未使用）。</param>
        /// <param name="culture">区域信息（未使用）。</param>
        /// <returns>不返回；始终抛出 <see cref="NotSupportedException"/>。</returns>
        /// <exception cref="NotSupportedException">始终抛出，表示不支持反向转换。</exception>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("RibbonTabKeyToIsSelectedConverter 不支持 ConvertBack。");
        }
    }
}
