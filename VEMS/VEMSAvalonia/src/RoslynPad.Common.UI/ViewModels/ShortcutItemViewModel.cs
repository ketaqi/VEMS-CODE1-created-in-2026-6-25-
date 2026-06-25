using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RoslynPad.UI.ViewModels
{
    public class ShortcutItemViewModel : NotificationObject
    {
        public string CommandName { get; set; } = "";
        public string Shortcut { get; set; } = "";
        public required ICommand EditShortcutCommand { get; set; }
        public required ICommand ResetShortcutCommand { get; set; }
    }

}
