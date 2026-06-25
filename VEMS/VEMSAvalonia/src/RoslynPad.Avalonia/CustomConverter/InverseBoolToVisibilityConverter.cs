using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RoslynPad.CustomConverter
{
    /// <summary>
    /// 反向布尔值到可见性的转换器（Avalonia 版本）。
    /// </summary>
    /// <remarks>
    /// <para>
    /// Avalonia 的 <see cref="Avalonia. Visual.IsVisible"/> 等属性为 <see cref="bool"/> 类型，
    /// 本转换器用于在绑定时将布尔值取反：<see langword="true"/> → <see langword="false"/>，
    /// <see langword="false"/> → <see langword="true"/>。
    /// </para>
    /// <para>
    /// 主要用途：文件重命名编辑框的可见性控制，非编辑状态时显示文件名。
    /// </para>
    /// <para>
    /// 注意：本转换器与 <see cref="BoolToVisibilityConverter"/>（设置 <c>Invert="True"</c>）功能等效，
    /// 提供本类是为了简化常见的取反场景，避免每次都需要设置 <c>Invert</c> 属性。
    /// </para>
    /// </remarks>
    /// <example>
    /// XAML 使用示例：
    /// <code language="xml">
    /// <![CDATA[
    /// <StackPanel>
    ///   <!-- IsEditing 为 true 时隐藏文件名，显示编辑框 -->
    ///   <TextBlock Text="{Binding FileName}" 
    ///              IsVisible="{Binding IsEditing, Converter={StaticResource InverseBoolToVis}}"/>
    ///   <TextBox Text="{Binding FileName}" 
    ///            IsVisible="{Binding IsEditing}"/>
    ///            
    ///   <!-- IsBusy 为 true 时禁用按钮 -->
    ///   <Button Content="提交" 
    ///           IsEnabled="{Binding IsBusy, Converter={StaticResource InverseBoolToVis}}"/>
    /// </StackPanel>
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="BoolToVisibilityConverter"/>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// 将布尔值取反后返回。
        /// </summary>
        /// <param name="value">
        /// 源值，期望为 <see cref="bool"/> 或 <see cref="Nullable{Boolean}"/>。
        /// 若为 <see langword="null"/> 或非布尔类型，则按 <see langword="false"/> 处理。
        /// </param>
        /// <param name="targetType">目标类型，通常为 <see cref="bool"/>。</param>
        /// <param name="parameter">转换参数（未使用）。</param>
        /// <param name="culture">区域信息（未使用）。</param>
        /// <returns>
        /// <paramref name="value"/> 的逻辑非值。
        /// 即：<see langword="true"/> 返回 <see langword="false"/>，
        /// <see langword="false"/>（或空值）返回 <see langword="true"/>。
        /// </returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 使用 as bool?  安全读取；非布尔或 null 时按 false 处理
            var boolValue = value as bool? ?? false;
            return !boolValue;
        }

        /// <summary>
        /// 反向转换：同样返回逻辑非值。
        /// </summary>
        /// <param name="value">
        /// 目标值，期望为 <see cref="bool"/> 或 <see cref="Nullable{Boolean}"/>。
        /// 若为 <see langword="null"/> 或非布尔类型，则按 <see langword="false"/> 处理。
        /// </param>
        /// <param name="targetType">源类型，通常为 <see cref="bool"/>。</param>
        /// <param name="parameter">转换参数（未使用）。</param>
        /// <param name="culture">区域信息（未使用）。</param>
        /// <returns>
        /// <paramref name="value"/> 的逻辑非值。
        /// </returns>
        /// <remarks>
        /// 支持双向绑定场景（例如 <c>CheckBox</c> ↔ <c>ViewModel</c>），
        /// 反向转换同样执行取反逻辑以保持一致性。
        /// </remarks>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var boolValue = value as bool? ?? false;
            return !boolValue;
        }
    }
}
