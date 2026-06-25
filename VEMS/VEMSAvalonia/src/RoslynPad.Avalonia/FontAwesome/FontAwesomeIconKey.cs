using System;
using RoslynPad.FontAwesome.Models;

namespace RoslynPad.FontAwesome
{
    /// <summary>
    /// 表示 Font Awesome 图标的键（标识符），用于解析和查找图标。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此类解析 Font Awesome 图标的字符串标识符，支持以下格式：
    /// <list type="bullet">
    ///   <item><description><c>"fa-icon-name"</c> - 仅图标名称（使用默认样式）</description></item>
    ///   <item><description><c>"fa-solid fa-icon-name"</c> - 样式 + 图标名称</description></item>
    ///   <item><description><c>"fas fa-icon-name"</c> - 简写样式 + 图标名称</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// 还支持从旧版 Font Awesome（v5 及更早版本）到 v6 的图标名称映射。
    /// </para>
    /// </remarks>
    /// <example>
    /// 解析图标键：
    /// <code language="csharp">
    /// // 解析带样式的图标
    /// if (FontAwesomeIconKey.TryParse("fa-solid fa-user", out var key))
    /// {
    ///     Console.WriteLine($"图标:  {key.Value}, 样式: {key.Style}");
    ///     // 输出:  图标: user, 样式:  Solid
    /// }
    /// 
    /// // 解析仅名称的图标
    /// if (FontAwesomeIconKey.TryParse("fa-home", out var key2))
    /// {
    ///     Console.WriteLine($"图标: {key2.Value}");
    ///     // 输出: 图标:  house（自动转换旧版名称）
    /// }
    /// </code>
    /// </example>
    internal partial class FontAwesomeIconKey
    {
        /// <summary>
        /// Font Awesome 图标键的前缀。
        /// </summary>
        private const string _faKeyPrefix = "fa-";

        /// <summary>
        /// 用于分割字符串的字符数组（预分配以避免 CA1861 警告）。
        /// </summary>
        private static readonly char[] _splitChars = { ' ' };

        /// <summary>
        /// 获取或设置图标名称（不含前缀）。
        /// </summary>
        /// <value>图标的名称标识符，如 <c>"user"</c>、<c>"home"</c> 等。</value>
        public required string Value { get; set; }

        /// <summary>
        /// 获取或设置图标的样式。
        /// </summary>
        /// <value>
        /// 图标样式；若未指定样式则为 <see langword="null"/>，
        /// 此时将使用图标的第一个可用样式。
        /// </value>
        public Style? Style { get; set; }

        /// <summary>
        /// 尝试解析 Font Awesome 图标键字符串。
        /// </summary>
        /// <param name="value">要解析的图标键字符串。</param>
        /// <param name="key">
        /// 解析成功时返回 <see cref="FontAwesomeIconKey"/> 实例；
        /// 解析失败时返回 <see langword="null"/>。
        /// </param>
        /// <returns>解析成功返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        /// <remarks>
        /// 支持的格式：
        /// <list type="bullet">
        ///   <item><description>单部分：<c>"fa-icon-name"</c></description></item>
        ///   <item><description>双部分：<c>"fa-solid fa-icon-name"</c> 或 <c>"fas fa-icon-name"</c></description></item>
        /// </list>
        /// </remarks>
        public static bool TryParse(string value, out FontAwesomeIconKey? key)
        {
            var parts = value.Split(_splitChars, StringSplitOptions.RemoveEmptyEntries);

            // 单部分格式：仅图标名称
            if (parts.Length == 1)
            {
                key = new FontAwesomeIconKey
                {
                    Value = GetValue(parts[0]),
                };
                return true;
            }

            // 双部分格式：样式 + 图标名称
            if (parts.Length == 2)
            {
                key = new FontAwesomeIconKey
                {
                    Style = GetStyle(parts[0]),
                    Value = GetValue(parts[1]),
                };
                return true;
            }

            key = null;
            return false;
        }

        /// <summary>
        /// 从样式字符串解析图标样式。
        /// </summary>
        /// <param name="value">样式字符串，如 <c>"fa-solid"</c> 或 <c>"fas"</c>。</param>
        /// <returns>对应的 <see cref="Models.Style"/> 枚举值；若无法识别则返回 <see langword="null"/>。</returns>
        private static Style? GetStyle(string value)
        {
            return value.ToUpperInvariant() switch
            {
                "FA-SOLID" or "FAS" => Models.Style.Solid,
                "FA-REGULAR" or "FAR" => Models.Style.Regular,
                "FA-BRANDS" or "FAB" => Models.Style.Brands,
                _ => null,
            };
        }

        /// <summary>
        /// 从图标字符串提取图标名称。
        /// </summary>
        /// <param name="input">输入字符串，如 <c>"fa-user"</c>。</param>
        /// <returns>去除前缀后的图标名称，并转换旧版名称为 v6 名称。</returns>
        private static string GetValue(string input)
        {
            var value = input.Length <= _faKeyPrefix.Length
                ? string.Empty
                : input.Substring(_faKeyPrefix.Length);

            return SupportLegacy(value);
        }
    }
}
