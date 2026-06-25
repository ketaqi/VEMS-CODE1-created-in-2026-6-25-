using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Microsoft.CodeAnalysis.CodeActions;
using RoslynPad.Roslyn;
using RoslynPad.Roslyn.CodeActions;

namespace RoslynPad.Formatting
{
    /// <summary>
    /// 将 <see cref="CodeAction"/> 转换为对应图标图像的值转换器。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此转换器用于在 UI 中为不同类型的代码操作显示相应的图标。
    /// 例如，"添加 using"操作可能显示一个加号图标，而"重命名"操作可能显示一个编辑图标。
    /// </para>
    /// <para>
    /// 此类同时实现 <see cref="MarkupExtension"/>，可直接在 XAML 中作为标记扩展使用。
    /// </para>
    /// </remarks>
    /// <example>
    /// XAML 使用示例：
    /// <code language="xml">
    /// <![CDATA[
    /// <Image Source="{Binding CodeAction, Converter={formatting:CodeActionToGlyphConverter}}" 
    ///        Width="16" Height="16"/>
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="CodeAction"/>
    /// <seealso cref="CodeActionExtensions.GetGlyph"/>
    internal sealed class CodeActionToGlyphConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// 返回此转换器实例，使其可作为标记扩展使用。
        /// </summary>
        /// <param name="serviceProvider">服务提供程序（未使用）。</param>
        /// <returns>当前转换器实例。</returns>
        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        /// <summary>
        /// 将 <see cref="CodeAction"/> 转换为对应的图标图像源。
        /// </summary>
        /// <param name="value">源值，期望为 <see cref="CodeAction"/>。</param>
        /// <param name="targetType">目标类型（未使用）。</param>
        /// <param name="parameter">转换参数（未使用）。</param>
        /// <param name="culture">区域信息（未使用）。</param>
        /// <returns>
        /// 若 <paramref name="value"/> 为 <see cref="CodeAction"/>，返回对应的图像源；
        /// 否则返回 <see langword="null"/>。
        /// </returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            (value as CodeAction)?.GetGlyph().ToImageSource();

        /// <summary>
        /// 不支持反向转换。
        /// </summary>
        /// <param name="value">目标值。</param>
        /// <param name="targetType">源类型。</param>
        /// <param name="parameter">转换参数。</param>
        /// <param name="culture">区域信息。</param>
        /// <returns>不返回，始终抛出异常。</returns>
        /// <exception cref="NotSupportedException">始终抛出，表示不支持反向转换。</exception>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
