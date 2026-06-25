using System;

namespace RoslynPad.FeatureDemos.Views
{
    /// <summary>
    /// 标记属性为快捷键绑定属性的特性。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 当属性标记此特性时，属性网格将使用专用的快捷键编辑器控件，
    /// 允许用户通过按下键盘组合键来设置快捷键值。
    /// </para>
    /// <para>
    /// 此特性与 <see cref="HotkeyCellEditFactory"/> 配合使用，
    /// 提供快捷键捕获和格式化功能。
    /// </para>
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code language="csharp">
    /// public class KeyboardSettings
    /// {
    ///     [Hotkey("save")]
    ///     public string SaveShortcut { get; set; } = "Ctrl+S";
    ///     
    ///     [Hotkey("copy")]
    ///     public string CopyShortcut { get; set; } = "Ctrl+C";
    ///     
    ///     [Hotkey]  // 使用默认 ID（null）
    ///     public string CustomShortcut { get; set; } = string.Empty;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="HotkeyCellEditFactory"/>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class HotkeyAttribute : Attribute
    {
        /// <summary>
        /// 获取快捷键的标识符。
        /// </summary>
        /// <value>
        /// 快捷键的唯一标识符，可用于在配置文件或本地化资源中引用；
        /// 若未指定则为 <see langword="null"/>。
        /// </value>
        public string? Id { get; }

        /// <summary>
        /// 初始化 <see cref="HotkeyAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="id">
        /// 快捷键的标识符；可为 <see langword="null"/>。
        /// 用于标识此快捷键绑定的用途或在配置中引用。
        /// </param>
        public HotkeyAttribute(string? id = null)
        {
            Id = id;
        }
    }
}
