using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using RoslynPad.UI;

namespace RoslynPad.CustomConverter
{
    /// <summary>
    /// 文件类型到图标标识的值转换器。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 用途：在文件树列表等 UI 控件中，根据文件夹或文件扩展名返回对应的 Font Awesome 图标标识字符串。
    /// </para>
    /// <para>
    /// 支持两种绑定源类型：
    /// <list type="bullet">
    ///   <item>
    ///     <term><see cref="DocumentViewModel"/></term>
    ///     <description>
    ///     优先使用此类型。若 <see cref="DocumentViewModel. IsFolder"/> 为 <see langword="true"/> 则返回文件夹图标；
    ///     否则根据 <see cref="DocumentViewModel.Name"/> 的扩展名进行映射。
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="bool"/></term>
    ///     <description>直接表示"是否为文件夹"。</description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// 未匹配到的扩展名将统一返回通用文件图标 <c>"fa-solid fa-file"</c>。
    /// </para>
    /// </remarks>
    /// <example>
    /// XAML 使用示例：
    /// <code language="xml">
    /// <![CDATA[
    /// <TreeView ItemsSource="{Binding Documents}">
    ///   <TreeView.ItemTemplate>
    ///     <DataTemplate>
    ///       <StackPanel Orientation="Horizontal">
    ///         <Icon Classes="{Binding Converter={StaticResource FileTypeToIcon}}"/>
    ///         <TextBlock Text="{Binding Name}"/>
    ///       </StackPanel>
    ///     </DataTemplate>
    ///   </TreeView.ItemTemplate>
    /// </TreeView>
    /// ]]>
    /// </code>
    /// </example>
    public class FileTypeToIconValueConverter : IValueConverter
    {
        /// <summary>
        /// 文件夹图标标识。
        /// </summary>
        private const string FolderIcon = "fa-solid fa-folder";

        /// <summary>
        /// 默认文件图标标识。
        /// </summary>
        private const string DefaultFileIcon = "fa-solid fa-file";

        /// <summary>
        /// 根据源值的类型和文件扩展名返回对应的 Font Awesome 图标标识。
        /// </summary>
        /// <param name="value">
        /// 绑定源值。优先期望为 <see cref="DocumentViewModel"/>；
        /// 也兼容 <see cref="bool"/>（表示是否为文件夹）。
        /// </param>
        /// <param name="targetType">目标类型，通常为 <see cref="string"/>。</param>
        /// <param name="parameter">转换参数（未使用）。</param>
        /// <param name="culture">区域信息（未使用）。</param>
        /// <returns>
        /// 对应的 Font Awesome 图标标识字符串。
        /// 例如：<c>"fa-solid fa-folder"</c>（文件夹）、<c>"fa-solid fa-c"</c>（C# 文件）等。
        /// 若无法识别，返回 <c>"fa-solid fa-file"</c>。
        /// </returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 情况1：绑定源为 DocumentViewModel
            if (value is DocumentViewModel viewModel)
            {
                // 优先判断是否为文件夹
                if (viewModel.IsFolder)
                {
                    return FolderIcon;
                }

                // 根据扩展名映射到具体图标（大小写不敏感）
                var extension = Path.GetExtension(viewModel.Name).ToLowerInvariant();
                return MapExtensionToIcon(extension);
            }

            // 情况2：绑定源为布尔值（是否文件夹）
            if (value is bool isFolder)
            {
                return isFolder ? FolderIcon : DefaultFileIcon;
            }

            // 默认返回通用文件图标
            return DefaultFileIcon;
        }

        /// <summary>
        /// 将文件扩展名映射到对应的图标标识。
        /// </summary>
        /// <param name="extension">文件扩展名（小写，包含点号）。</param>
        /// <returns>对应的 Font Awesome 图标标识字符串。</returns>
        private static string MapExtensionToIcon(string extension)
        {
            return extension switch
            {
                // 编程语言文件
                ".cs" or ".csx" => "fa-solid fa-c",           // C# 文件
                ".c" or ".cpp" => "fa-solid fa-c",            // C/C++ 文件
                ".java" => "fa-solid fa-j",                    // Java 文件
                ".py" => "fa-solid fa-p",                      // Python 文件
                ".js" => "fa-brands fa-node-js",               // JavaScript 文件
                ".rb" => "fa-solid fa-leaf",                   // Ruby 文件
                ".php" => "fa-solid fa-hippo",                 // PHP 文件
                ".pl" => "fa-solid fa-horse",                  // Perl 文件
                ".vb" or ".fs" => "fa-solid fa-file-code",     // VB. NET / F# 文件

                // 标记语言和样式文件
                ".xml" => "fa-solid fa-rss",                   // XML 文件
                ".json" => "fa-brands fa-node-js",             // JSON 文件
                ".html" => "fa-solid fa-code",                 // HTML 文件
                ".css" => "fa-solid fa-hashtag",               // CSS 文件
                ".md" => "fa-solid fa-down-long",              // Markdown 文件
                ".axaml" => "fa-solid fa-file-excel",          // Avalonia XAML 文件

                // 配置文件
                ".yml" or ".yaml" => "fa-solid fa-circle-exclamation",  // YAML 文件
                ".ini" => "fa-solid fa-align-left",            // INI 配置文件
                ".toml" => "fa-solid fa-gear",                 // TOML 配置文件

                // 脚本文件
                ".bat" => "fa-solid fa-desktop",               // Windows 批处理文件
                ".ps1" => "fa-solid fa-align-left",            // PowerShell 脚本
                ".sh" => "fa-solid fa-dollar-sign",            // Shell 脚本
                ".sql" => "fa-solid fa-database",              // SQL 脚本

                // Git 相关文件
                ".gitignore" or ".gitconfig" or ".gitattributes" => "fa-solid fa-square-share-nodes",

                // 容器相关
                ".dockerfile" => "fa-solid fa-fish-fins",      // Dockerfile

                // 文本文件
                ". txt" => "fa-solid fa-file-pen",              // 纯文本文件

                // 未识别的扩展名
                _ => DefaultFileIcon
            };
        }

        /// <summary>
        /// 不支持反向转换。
        /// </summary>
        /// <param name="value">目标值（通常为图标标识字符串）。</param>
        /// <param name="targetType">源类型。</param>
        /// <param name="parameter">转换参数（未使用）。</param>
        /// <param name="culture">区域信息（未使用）。</param>
        /// <returns>不返回，始终抛出异常。</returns>
        /// <exception cref="NotImplementedException">
        /// 始终抛出，因为从图标标识无法可靠地反推文件类型。
        /// </exception>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException("FileTypeToIconValueConverter 不支持反向转换：图标标识无法可靠反推文件类型。");
        }
    }
}
