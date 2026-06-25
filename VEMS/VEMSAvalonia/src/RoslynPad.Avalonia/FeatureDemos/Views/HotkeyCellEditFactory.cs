using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.PropertyGrid.Controls;
using Avalonia.PropertyGrid.Controls.Factories;
using PropertyModels.Extensions;

namespace RoslynPad.FeatureDemos.Views
{
    /// <summary>
    /// 快捷键属性单元格编辑工厂。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此工厂为标记了 <see cref="HotkeyAttribute"/> 的字符串属性创建专用的快捷键编辑控件。
    /// </para>
    /// <para>
    /// 功能特性：
    /// <list type="bullet">
    ///   <item><description>捕获键盘组合键并格式化为可读字符串（如 "Ctrl+Shift+S"）</description></item>
    ///   <item><description>支持 Escape、Delete、Backspace 清除快捷键</description></item>
    ///   <item><description>只读文本框防止手动输入，只能通过按键设置</description></item>
    ///   <item><description>支持所有常见修饰键组合（Ctrl、Shift、Alt、Win）</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// 在属性网格中使用：
    /// <code language="csharp">
    /// // 模型类
    /// public class ShortcutSettings
    /// {
    ///     [Hotkey("run")]
    ///     public string RunShortcut { get; set; } = "F5";
    /// }
    /// 
    /// // 注册工厂
    /// propertyGrid. Factories.AddFactory(new HotkeyCellEditFactory());
    /// </code>
    /// </example>
    /// <seealso cref="HotkeyAttribute"/>
    public class HotkeyCellEditFactory : AbstractCellEditFactory
    {
        /// <summary>
        /// 获取工厂的导入优先级。
        /// </summary>
        /// <value>返回 1000，表示高优先级。</value>
        /// <remarks>
        /// 较高的优先级确保此工厂在默认字符串编辑工厂之前被调用，
        /// 从而正确处理标记了 <see cref="HotkeyAttribute"/> 的属性。
        /// </remarks>
        public override int ImportPriority => 1000;

        /// <summary>
        /// 为标记了 <see cref="HotkeyAttribute"/> 的属性创建快捷键编辑控件。
        /// </summary>
        /// <param name="context">属性单元格上下文，包含属性和目标对象信息。</param>
        /// <returns>
        /// 若属性标记了 <see cref="HotkeyAttribute"/>，返回配置好的 <see cref="TextBox"/> 控件；
        /// 否则返回 <see langword="null"/> 以让其他工厂处理。
        /// </returns>
        public override Control? HandleNewProperty(PropertyCellContext context)
        {
            // 只处理标记了 [Hotkey] 特性的属性
            var attr = context.Property?.GetCustomAttribute<HotkeyAttribute>();
            if (attr is null)
            {
                return null;
            }

            var textBox = new TextBox
            {
                IsReadOnly = true,                              // 只读，防止手动输入
                Watermark = "按下快捷键…",                        // 水印提示
                HorizontalContentAlignment = HorizontalAlignment.Left,
                IsTabStop = true                                // 接收键盘 Tab 键导航焦点
            };

            // 同步属性初始值到控件
            SyncFromProperty(context, textBox);

            // 防止控件被外部逻辑禁用
            textBox.AttachedToVisualTree += (_, _) =>
            {
                textBox.IsEnabled = true;      // 防止被默认逻辑禁用
                textBox.IsReadOnly = true;     // 保持只读状态
            };

            // 使用隧道策略捕获键盘事件
            textBox.AddHandler(InputElement.KeyDownEvent, (s, e) =>
            {
                HandleKeyDown(context, textBox, e);
            }, RoutingStrategies.Tunnel);

            // 文本变更时输出调试信息
            textBox.TextChanged += (s, e) =>
            {
                var key = context.Property?.Name;
                var value = textBox.Text;
                Debug.WriteLine($"键值 {key}，修改为 {value}");
            };

            return textBox;
        }

        /// <summary>
        /// 处理属性值变更，同步到编辑控件。
        /// </summary>
        /// <param name="context">属性单元格上下文。</param>
        /// <returns>若成功处理返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        public override bool HandlePropertyChanged(PropertyCellContext context)
        {
            if (context.CellEdit is not TextBox textBox)
            {
                return false;
            }

            SyncFromProperty(context, textBox);
            return true;
        }

        #region 私有辅助方法

        /// <summary>
        /// 处理键盘按下事件，捕获并设置快捷键。
        /// </summary>
        /// <param name="context">属性单元格上下文。</param>
        /// <param name="textBox">快捷键文本框。</param>
        /// <param name="e">键盘事件参数。</param>
        private void HandleKeyDown(PropertyCellContext context, TextBox textBox, KeyEventArgs e)
        {
            var key = e.Key;
            var modifiers = e.KeyModifiers;

            // Escape、Delete、Backspace 用于清除快捷键
            if (key is Key.Escape or Key.Delete or Key.Back)
            {
                SetPropertyValue(context, string.Empty);
                e.Handled = true;
                return;
            }

            // 忽略单独的修饰键按下
            if (IsModifierKey(key))
            {
                return;
            }

            // 格式化并设置快捷键
            var gesture = FormatGesture(modifiers, key);
            if (!string.IsNullOrEmpty(gesture))
            {
                SetPropertyValue(context, gesture);
                e.Handled = true;
            }
        }

        /// <summary>
        /// 从属性同步数据到文本框。
        /// </summary>
        /// <param name="context">属性单元格上下文，包含目标对象和属性信息。</param>
        /// <param name="textBox">要同步数据的文本框控件。</param>
        private static void SyncFromProperty(PropertyCellContext context, TextBox textBox)
        {
            var keyString = context.Property.GetValue(context.Target) as string;
            textBox.Text = keyString ?? string.Empty;
        }

        /// <summary>
        /// 判断指定的键是否为修饰键。
        /// </summary>
        /// <param name="key">需要判断的键盘按键。</param>
        /// <returns>若是修饰键（Ctrl、Shift、Alt、Win）则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        private static bool IsModifierKey(Key key) =>
            key is Key.LeftCtrl or Key.RightCtrl
                or Key.LeftShift or Key.RightShift
                or Key.LeftAlt or Key.RightAlt
                or Key.LWin or Key.RWin;

        /// <summary>
        /// 格式化键盘手势为可读的字符串表示形式。
        /// </summary>
        /// <param name="modifiers">键盘修饰符组合。</param>
        /// <param name="key">主按键。</param>
        /// <returns>
        /// 格式化后的手势字符串，格式为 "修饰符+主键"（例如 "Ctrl+C"）；
        /// 若无法格式化则返回空字符串。
        /// </returns>
        /// <example>
        /// <code language="csharp">
        /// // 返回 "Ctrl+Shift+S"
        /// var gesture = FormatGesture(KeyModifiers.Control | KeyModifiers. Shift, Key. S);
        /// 
        /// // 返回 "Space"
        /// var space = FormatGesture(KeyModifiers.None, Key. Space);
        /// </code>
        /// </example>
        private static string FormatGesture(KeyModifiers modifiers, Key key)
        {
            // 特殊处理：无修饰符的空格键
            if (key == Key.Space && modifiers == KeyModifiers.None)
            {
                return "Space";
            }

            // 构建修饰符列表
            var parts = new List<string>();

            if (modifiers.HasFlag(KeyModifiers.Control))
            {
                parts.Add("Ctrl");
            }
            if (modifiers.HasFlag(KeyModifiers.Shift))
            {
                parts.Add("Shift");
            }
            if (modifiers.HasFlag(KeyModifiers.Alt))
            {
                parts.Add("Alt");
            }
            if (modifiers.HasFlag(KeyModifiers.Meta))
            {
                parts.Add("Win");
            }

            // 获取主按键的字符串表示
            var mainKey = KeyToString(key);
            if (string.IsNullOrEmpty(mainKey))
            {
                return string.Empty;
            }

            // 添加主按键并用 "+" 连接
            parts.Add(mainKey);
            return string.Join("+", parts);
        }

        /// <summary>
        /// 将键盘按键转换为可读的字符串表示。
        /// </summary>
        /// <param name="key">要转换的按键。</param>
        /// <returns>按键的字符串表示。</returns>
        private static string KeyToString(Key key)
        {
            // 功能键 F1-F24 和字母键 A-Z
            if (key is >= Key.F1 and <= Key.F24 or >= Key.A and <= Key.Z)
            {
                return key.ToString();
            }

            // 数字键 0-9
            if (key is >= Key.D0 and <= Key.D9)
            {
                return key.ToString().TrimStart('D');
            }

            // 数字小键盘 NumPad0-NumPad9
            if (key is >= Key.NumPad0 and <= Key.NumPad9)
            {
                return "Num" + (key - Key.NumPad0);
            }

            // 其他特殊按键
            return key switch
            {
                Key.Space => "Space",
                Key.Enter => "Enter",
                Key.Tab => "Tab",
                Key.Back => "Backspace",
                Key.Delete => "Delete",
                Key.Insert => "Insert",
                Key.Escape => "Esc",
                Key.Up => "Up",
                Key.Down => "Down",
                Key.Left => "Left",
                Key.Right => "Right",
                Key.Home => "Home",
                Key.End => "End",
                Key.PageUp => "PageUp",
                Key.PageDown => "PageDown",
                Key.OemPlus => "+",
                Key.OemMinus => "-",
                Key.OemComma => ",",
                Key.OemPeriod => ".",
                Key.Oem1 => ";",
                Key.Oem2 => "/",
                Key.Oem3 => "`",
                Key.Oem4 => "[",
                Key.Oem5 => "\\",
                Key.Oem6 => "]",
                Key.Oem7 => "'",
                _ => key.ToString()
            };
        }

        #endregion
    }
}
