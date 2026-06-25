using Avalonia;
using Avalonia.Controls;

namespace RoslynPad.IconsFront
{
    /// <summary>
    /// 提供图标相关的 Avalonia 附加属性。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此类定义了可附加到 <see cref="ContentControl"/> 的图标属性，
    /// 允许在 XAML 中声明式地为控件设置图标。
    /// </para>
    /// <para>
    /// 当 <see cref="IconProperty"/> 值变更时，会自动创建 <see cref="Icon"/> 控件
    /// 并设置为控件的内容。
    /// </para>
    /// </remarks>
    /// <example>
    /// XAML 使用示例：
    /// <code language="xml">
    /// <![CDATA[
    /// <Button icons: Attached. Icon="fa-solid fa-save" Content="保存"/>
    /// <ContentControl icons:Attached.Icon="fa-brands fa-github"/>
    /// ]]>
    /// </code>
    /// </example>
    public static class Attached
    {
        /// <summary>
        /// 标识 <see cref="IconProperty"/> Avalonia 附加属性。
        /// </summary>
        public static readonly AttachedProperty<string> IconProperty =
            AvaloniaProperty.RegisterAttached<Icon, ContentControl, string>("Icon", string.Empty);

        /// <summary>
        /// 静态构造函数，订阅属性变更事件。
        /// </summary>
        static Attached()
        {
            IconProperty.Changed.Subscribe(HandleIconChanged);
        }

        /// <summary>
        /// 获取指定控件的图标值。
        /// </summary>
        /// <param name="target">目标控件。</param>
        /// <returns>图标标识字符串。</returns>
        public static string GetIcon(ContentControl target)
        {
            return target.GetValue(IconProperty);
        }

        /// <summary>
        /// 设置指定控件的图标值。
        /// </summary>
        /// <param name="target">目标控件。</param>
        /// <param name="value">图标标识字符串（如 "fa-solid fa-user"）。</param>
        public static void SetIcon(ContentControl target, string value)
        {
            target.SetValue(IconProperty, value);
        }

        /// <summary>
        /// 处理图标属性变更，创建并设置图标控件。
        /// </summary>
        /// <param name="args">属性变更事件参数。</param>
        private static void HandleIconChanged(AvaloniaPropertyChangedEventArgs<string> args)
        {
            if (args.Sender is not ContentControl target)
            {
                return;
            }

            target.Content = new Icon
            {
                Value = args.NewValue.HasValue ? args.NewValue.Value : string.Empty
            };
        }
    }
}
