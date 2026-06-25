using Avalonia;
using AvaloniaMenuItem = Avalonia.Controls.MenuItem;

namespace RoslynPad.IconsFront
{
    /// <summary>
    /// 提供菜单项图标相关的 Avalonia 附加属性。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此类定义了可附加到 <see cref="AvaloniaMenuItem"/> 的图标属性，
    /// 允许在 XAML 中声明式地为菜单项设置图标。
    /// </para>
    /// </remarks>
    /// <example>
    /// XAML 使用示例：
    /// <code language="xml">
    /// <![CDATA[
    /// <MenuItem Header="保存" icons:MenuItem.Icon="fa-solid fa-save"/>
    /// <MenuItem Header="复制" icons:MenuItem.Icon="fa-solid fa-copy"/>
    /// ]]>
    /// </code>
    /// </example>
    public static class MenuItem
    {
        /// <summary>
        /// 标识 <see cref="IconProperty"/> Avalonia 附加属性。
        /// </summary>
        public static readonly AttachedProperty<string> IconProperty =
            AvaloniaProperty.RegisterAttached<Icon, AvaloniaMenuItem, string>("Icon", string.Empty);

        /// <summary>
        /// 静态构造函数，订阅属性变更事件。
        /// </summary>
        static MenuItem()
        {
            IconProperty.Changed.Subscribe(HandleIconChanged);
        }

        /// <summary>
        /// 获取指定菜单项的图标值。
        /// </summary>
        /// <param name="target">目标菜单项。</param>
        /// <returns>图标标识字符串。</returns>
        public static string GetIcon(AvaloniaMenuItem target)
        {
            return target.GetValue(IconProperty);
        }

        /// <summary>
        /// 设置指定菜单项的图标值。
        /// </summary>
        /// <param name="target">目标菜单项。</param>
        /// <param name="value">图标标识字符串（如 "fa-solid fa-save"）。</param>
        public static void SetIcon(AvaloniaMenuItem target, string value)
        {
            target.SetValue(IconProperty, value);
        }

        /// <summary>
        /// 处理图标属性变更，创建并设置图标控件。
        /// </summary>
        /// <param name="args">属性变更事件参数。</param>
        private static void HandleIconChanged(AvaloniaPropertyChangedEventArgs<string> args)
        {
            if (args.Sender is not AvaloniaMenuItem target)
            {
                return;
            }

            var newValue = args.NewValue.GetValueOrDefault(string.Empty);
            if (newValue is not null)
            {
                target.Icon = new Icon
                {
                    Value = newValue,
                };
            }
        }
    }
}
