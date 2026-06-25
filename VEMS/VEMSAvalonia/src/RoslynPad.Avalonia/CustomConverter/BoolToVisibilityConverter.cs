using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RoslynPad.CustomConverter
{
    /// <summary>
    /// 布尔值到可见性的转换器（Avalonia 版本）。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 名称沿用 WPF 习惯的 <c>BoolToVisibilityConverter</c>，但在 Avalonia 中
    /// <see cref="Avalonia.Visual.IsVisible"/> 等属性为 <see cref="bool"/> 类型，
    /// 因此本转换器实际返回布尔值，而非 WPF 的 <c>Visibility</c> 枚举。
    /// </para>
    /// <para>
    /// 主要用途：文件重命名编辑框的可见性控制，编辑状态时显示重命名框。
    /// </para>
    /// <para>
    /// 提供 <see cref="Invert"/> 属性以支持"取反"场景（例如"当 Busy 时隐藏按钮"）。
    /// </para>
    /// </remarks>
    /// <example>
    /// XAML 使用示例：
    /// <code language="xml">
    /// <![CDATA[
    /// <Window xmlns: conv="clr-namespace:RoslynPad. CustomConverter">
    ///   <Window.Resources>
    ///     <conv:BoolToVisibilityConverter x:Key="BoolToVis"/>
    ///     <conv:BoolToVisibilityConverter x:Key="InvertBoolToVis" Invert="True"/>
    ///   </Window.Resources>
    ///   
    ///   <!-- 正常模式：true 显示，false 隐藏 -->
    ///   <TextBox IsVisible="{Binding IsEditing, Converter={StaticResource BoolToVis}}"/>
    ///   
    ///   <!-- 取反模式：true 隐藏，false 显示 -->
    ///   <Button IsVisible="{Binding IsBusy, Converter={StaticResource InvertBoolToVis}}"
    ///           Content="只在非 Busy 时可见"/>
    /// </Window>
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="InverseBoolToVisibilityConverter"/>
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// 获取或设置是否取反转换结果。
        /// </summary>
        /// <value>
        /// <see langword="true"/> 表示取反（<c>true → false</c>，<c>false ��� true</c>）；
        /// <see langword="false"/> 表示保持原值。默认为 <see langword="false"/>。
        /// </value>
        public bool Invert { get; set; }

        /// <summary>
        /// 将源布尔值转换为目标布尔值（可选取反）。
        /// </summary>
        /// <param name="value">
        /// 源值，期望为 <see cref="bool"/> 或 <see cref="Nullable{Boolean}"/>。
        /// 若为 <see langword="null"/> 或非布尔类型，则按 <see langword="false"/> 处理。
        /// </param>
        /// <param name="targetType">目标类型，通常为 <see cref="bool"/>。</param>
        /// <param name="parameter">转换参数（未使用）。</param>
        /// <param name="culture">区域信息（未使用）。</param>
        /// <returns>
        /// 当 <see cref="Invert"/> 为 <see langword="false"/> 时返回原值；
        /// 当 <see cref="Invert"/> 为 <see langword="true"/> 时返回取反后的值。
        /// </returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 兼容空值与非布尔类型：统一按 false 处理
            var boolValue = value as bool? ?? false;
            return Invert ? !boolValue : boolValue;
        }

        /// <summary>
        /// 反向转换：将目标布尔值转换回源布尔值（可选取反）。
        /// </summary>
        /// <param name="value">
        /// 目标值，期望为 <see cref="bool"/> 或 <see cref="Nullable{Boolean}"/>。
        /// 若为 <see langword="null"/> 或非布尔类型，则按 <see langword="false"/> 处理。
        /// </param>
        /// <param name="targetType">源类型，通常为 <see cref="bool"/>。</param>
        /// <param name="parameter">转换参数（未使用）。</param>
        /// <param name="culture">区域信息（未使用）。</param>
        /// <returns>
        /// 当 <see cref="Invert"/> 为 <see langword="false"/> 时返回原值；
        /// 当 <see cref="Invert"/> 为 <see langword="true"/> 时返回取反后的值。
        /// </returns>
        /// <remarks>
        /// 支持双向绑定场景，转换逻辑与 <see cref="Convert"/> 相同。
        /// </remarks>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var boolValue = value as bool? ?? false;
            return Invert ? !boolValue : boolValue;
        }
    }
}
