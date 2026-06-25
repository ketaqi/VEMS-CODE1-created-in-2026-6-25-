using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia;

namespace RoslynPad.UI.Dialogs
{
    public class InputDialog : Window
    {
        private readonly TextBox _inputBox;
        public string InputText => _inputBox?.Text ?? string.Empty;

        public InputDialog(string title, string message, string defaultValue = "")
        {
            Title = title;
            Width = 400;
            Height = 160;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var okButton = new Button { Content = "确定", IsDefault = true, Width = 80, Margin = new Thickness(8) };
            var cancelButton = new Button { Content = "取消", IsCancel = true, Width = 80, Margin = new Thickness(8) };

            _inputBox = new TextBox { Text = defaultValue, Margin = new Thickness(8), Width = 360 };

            okButton.Click += (_, _) => Close(true);
            cancelButton.Click += (_, _) => Close(false);

            Content = new StackPanel
            {
                Children =
            {
                new TextBlock { Text = message, Margin = new Thickness(8) },
                _inputBox,
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    Children = { okButton, cancelButton }
                }
            }
            };
        }

        public async Task<string?> ShowDialogAsync(Window owner)
        {
            var result = await ShowDialog<bool>(owner).ConfigureAwait(true);
            return result ? InputText : null;
        }
    }

}
