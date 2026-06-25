using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data.Converters;
using RoslynPad.UI;

namespace RoslynPad.CustomConverter
{
    // 文件树文件类型对应图标颜色的转换器——ZDM
    /// <summary>
    /// 根据文件类型（或“是否文件夹”）返回用于渲染的颜色名称的转换器。
    /// </summary>
    /// <remarks>
    /// 用途：在文件/文档列表、树控件中，为不同类型的节点提供直观的颜色区分。<br/>
    /// 支持两类输入：
    /// <list type="bullet">
    /// <item><description><see cref="DocumentViewModel"/>：若 <c>IsFolder</c> 为 <see langword="true"/> 返回文件夹色；否则依据 <c>Name</c> 扩展名映射。</description></item>
    /// <item><description><see cref="bool"/>：直接表示“是否文件夹”。</description></item>
    /// </list>
    /// 颜色值为可被 Avalonia 解析的颜色名字符串（如 <c>"Tomato"</c>、<c>"DimGray"</c>）。未匹配的扩展名走默认颜色。<br/>
    /// 线程模型：仅用于绑定转换，不涉及跨线程访问。
    /// </remarks>
    public class FileTypeToColorConverter : IValueConverter
    {
        /// <summary>
        /// 将文件（或文件夹）信息映射为颜色名称字符串。
        /// </summary>
        /// <param name="value">
        /// 源对象：优先期望 <see cref="DocumentViewModel"/>；也兼容 <see cref="bool"/>（表示是否文件夹）。
        /// </param>
        /// <param name="targetType">目标类型（通常为 <see cref="string"/>）。</param>
        /// <param name="parameter">未使用。</param>
        /// <param name="culture">区域信息（未使用）。</param>
        /// <returns>
        /// 颜色名称字符串（例如 <c>"burlywood"</c>、<c>"Tomato"</c>）。当无法识别时返回默认值。
        /// </returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DocumentViewModel vm)
            {
                // 文件夹优先：统一用“burlywood”
                if (vm.IsFolder)
                    return "burlywood"; // 文件夹颜色

                // 基于扩展名的简单映射；大小写不敏感
                var ext = Path.GetExtension(vm.Name).ToLowerInvariant();
                return ext switch
                {
                    ".cs" => "darkkhaki",
                    ".csx" => "DodgerBlue",
                    ".xml" => "Orange",
                    ".json" => "ForestGreen",
                    ".md" => "SlateGray",
                    ".dockerfile" => "Teal",
                    ".bat" => "DarkSlateGray",
                    ".ps1" => "RoyalBlue",
                    ".sql" => "MediumSeaGreen",
                    ".gitignore" => "DimGray",
                    ".gitconfig" => "DimGray",
                    ".gitattributes" => "DimGray",
                    ".yml" => "DarkOrange",
                    ".yaml" => "DarkOrange",
                    ".ini" => "DarkKhaki",
                    ".toml" => "DarkOliveGreen",
                    ".sh" => "DarkCyan",
                    ".py" => "Gold",
                    ".rb" => "MediumVioletRed",
                    ".php" => "Purple",
                    ".pl" => "Sienna",
                    ".js" => "darkkhaki",
                    ".html" => "Tomato",
                    ".css" => "MediumPurple",
                    ".java" => "Chocolate",
                    ".c" => "SteelBlue",
                    ".cpp" => "SteelBlue",
                    ".vb" => "MediumOrchid",
                    ".fs" => "MediumOrchid",
                    ".axaml" => "MediumSeaGreen",
                    ".txt" => "Gray",
                    _ => "DimGray" // 未覆盖的类型使用通用色
                };
            }

            // 兼容：直接传入 bool（表示是否文件夹）
            if (value is bool isFolder)
                return isFolder ? "burlywood" : "burlywood"; // 按原逻辑：两者均返回文件夹色

            // 默认返回：保持与“文件夹色”一致，避免视觉突兀
            return "burlywood";
        }

        /// <summary>
        /// 不支持反向转换：仅用于从类型推导颜色，无法从颜色可靠反推类型。
        /// </summary>
        /// <param name="value">目标值（通常为 <see cref="string"/> 颜色名）。</param>
        /// <param name="targetType">源类型。</param>
        /// <param name="parameter">未使用。</param>
        /// <param name="culture">区域信息（未使用）。</param>
        /// <returns>不返回；始终抛出 <see cref="NotImplementedException"/>。</returns>
        /// <exception cref="NotImplementedException">始终抛出，表示不支持反向转换。</exception>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
