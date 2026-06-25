// FocusOnEditing.cs  —— 仅负责：进入编辑态时自动聚焦，并在文件时只选中主文件名

using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace RoslynPad.UI
{
    /// <summary>
    /// 当绑定值为 true 且控件可见时，自动聚焦并选择文本：
    /// - 文件夹：全选
    /// - 文件：只选中不含扩展名的部分
    ///
    /// XAML 引用：
    ///   xmlns:behaviors="clr-namespace:RoslynPad.UI"
    /// 用法（在内联重命名 TextBox 上）：
    ///   behaviors:FocusOnEditing.Enable="{Binding IsEditing}"
    /// </summary>
    public static class FocusOnEditing
    {
        public static readonly AttachedProperty<bool> EnableProperty =
            AvaloniaProperty.RegisterAttached<TextBox, bool>(
                "Enable",
                typeof(FocusOnEditing),
                defaultValue: false);

        public static void SetEnable(TextBox element, bool value) => element.SetValue(EnableProperty, value);
        public static bool GetEnable(TextBox element) => element.GetValue(EnableProperty);

        static FocusOnEditing()
        {
            // 使用“非泛型”签名以避免 CS1503（方法组不匹配）
            EnableProperty.Changed.AddClassHandler<TextBox>(OnEnableChanged);
        }

        private static void OnEnableChanged(TextBox tb, AvaloniaPropertyChangedEventArgs e)
        {
            // 只在从 false -> true 时触发
            if (e.NewValue is bool enabled && enabled)
            {
                // 若还未挂到可视树，等挂上再尝试
                tb.AttachedToVisualTree -= OnAttached;
                tb.AttachedToVisualTree += OnAttached;

                RequestFocus(tb);
            }
        }

        private static void OnAttached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.AttachedToVisualTree -= OnAttached;
                RequestFocus(tb);
            }
        }

        /// <summary>
        /// 不用 await，直接调度到渲染帧：确保容器/模板已创建，再 Focus + 选择
        /// </summary>
        private static void RequestFocus(TextBox tb)
        {
            // 在 Render 优先级调度一次，等模板/布局准备好
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    tb.BringIntoView(); // 虚拟化场景下有助于生成容器
                    tb.Focus();

                    var text = tb.Text ?? string.Empty;

                    if (tb.DataContext is DocumentViewModel vm && !vm.IsFolder)
                    {
                        // 文件：只选中主文件名（不含扩展名）
                        var baseNameLen = Path.GetFileNameWithoutExtension(text)?.Length ?? 0;
                        tb.SelectionStart = 0;
                        tb.SelectionEnd = Math.Max(0, baseNameLen);
                    }
                    else
                    {
                        // 文件夹：全选
                        tb.SelectAll();
                    }
                }
                catch
                {
                    // 忽略极少数控件生命周期时序问题
                }
            }, DispatcherPriority.Render);
        }
    }
}
