using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RoslynPad.Converters
{
    /// <summary>
    /// 工具栏图标转换器，根据工具选中状态返回对应的图标路径。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 用途：在侧边工具栏中，为当前激活的工具显示高亮图标，
    /// 未激活的工具显示普通图标。
    /// </para>
    /// <para>
    /// 支持的工具类型：
    /// <list type="bullet">
    ///   <item><description><c>Explorer</c> - 资源管理器</description></item>
    ///   <item><description><c>Search</c> - 搜索</description></item>
    ///   <item><description><c>SourceControl</c> - 源代码管理</description></item>
    ///   <item><description><c>Debug</c> - 调试</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// 资源命名要求：
    /// <list type="bullet">
    ///   <item><description>普通状态：<c>/Resources/{Tool}_Normal.png</c></description></item>
    ///   <item><description>激活状态：<c>/Resources/{Tool}_Hover.png</c></description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// XAML 使用示例：
    /// <code language="xml">
    /// <![CDATA[
    /// <Image Source="{Binding SelectedTool, 
    ///                         Converter={StaticResource ToolIconConverter}, 
    ///                         ConverterParameter=Explorer}"/>
    /// ]]>
    /// </code>
    /// 其中 <c>SelectedTool</c> 为当前选中的工具名称，
    /// <c>ConverterParameter</c> 为此图标代表的工具名称。
    /// </example>
    public class ToolIconConverter : IValueConverter
    {
        /// <summary>
        /// 默认图标路径（回退值）。
        /// </summary>
        private const string DefaultIconPath = "/Resources/Explorer_Normal. png";

        /// <summary>
        /// 根据当前选中工具与目标工具名返回相应图标路径。
        /// </summary>
        /// <param name="value">
        /// 当前选中的工具名称，期望为 <see cref="string"/>（如 <c>"Explorer"</c>）。
        /// </param>
        /// <param name="targetType">目标类型。</param>
        /// <param name="parameter">
        /// 此图标代表的工具名称（如 <c>"Search"</c>）。
        /// </param>
        /// <param name="culture">区域信息（未使用）。</param>
        /// <returns>
        /// 对应的资源路径字符串。
        /// 若工具名匹配则返回 Hover 图标，否则返回 Normal 图标。
        /// 若参数无效则返回默认图标。
        /// </returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var selectedTool = value?.ToString();
            var thisTool = parameter?.ToString();

            // 参数缺失时返回默认图标
            if (string.IsNullOrEmpty(thisTool))
            {
                return DefaultIconPath;
            }

            // 判断当前工具是否激活（大小写不敏感）
            var isActive = string.Equals(selectedTool, thisTool, StringComparison.OrdinalIgnoreCase);

            // 根据工具名返回对应图标路径
            return thisTool switch
            {
                "Explorer" => isActive
                    ? "/Resources/Explorer_Hover.png"
                    : "/Resources/Explorer_Normal.png",

                "Search" => isActive
                    ? "/Resources/Search_Hover.png"
                    : "/Resources/Search_Normal.png",

                "SourceControl" => isActive
                    ? "/Resources/SourceControl_Hover.png"
                    : "/Resources/SourceControl_Normal.png",

                "Debug" => isActive
                    ? "/Resources/RunDebug_Hover.png"
                    : "/Resources/RunDebug_Normal.png",

                // 未知工具名时返回默认图标
                _ => DefaultIconPath
            };
        }

        /// <summary>
        /// 不支持反向转换。
        /// </summary>
        /// <param name="value">目标值。</param>
        /// <param name="targetType">源类型。</param>
        /// <param name="parameter">转换参数。</param>
        /// <param name="culture">区域信息。</param>
        /// <returns>不返回，始终抛出异常。</returns>
        /// <exception cref="NotImplementedException">
        /// 始终抛出，因为从图标路径无法可靠反推工具名和激活状态。
        /// </exception>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException("ToolIconConverter 不支持反向转换。");
    }
}
