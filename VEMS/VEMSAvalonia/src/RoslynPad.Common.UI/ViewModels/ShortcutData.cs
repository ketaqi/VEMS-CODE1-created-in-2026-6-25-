using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RoslynPad.UI.MainViewModel;

namespace RoslynPad.UI.ViewModels
{
    public class ShortcutData
    {
        public required string InitialLanguage1 { get; set; }
        public required string InitialTheme1 { get; set; }
        public required int EditorFontSize { get; set; }
        public required int OutputFontSize { get; set; }
        public required string EditorFontFamily1 { get; set; }
        public required string EditorFontFamily { get; set; }
        public required string InitialLanguage { get; set; }
        public required string InitialTheme { get; set; }
        public required string InitialFolderPath { get; set; }
        public required string FileMenu { get; set; }
        public required string NewFileShortcut { get; set; }
        public required string OpenFileShortcut { get; set; }
        public required string OpenFolderShortcut { get; set; }
        public required string DefaultFolderShortcut { get; set; }
        public required string SaveFileShortcut { get; set; }
        public required string SaveAsShortcut { get; set; }
        public required string SaveAllShortcut { get; set; }
        public required string CloseTabShortcut { get; set; }
        public required string CloseAllShortcut { get; set; }
        public required string ExitAppShortcut { get; set; }
        public required string CodeMenu { get; set; }
        public required string SearchTextShortcut { get; set; }
        public required string ReplaceTextShortcut { get; set; }
        public required string FormatCodeShortcut { get; set; }
        public required string CommentSelectionShortcut { get; set; }
        public required string UncommentSelectionShortcut { get; set; }
        public required string DebugCodeShortcut { get; set; }
        public required string RunCodeShortcut { get; set; }
        public required string AddClassShortcut { get; set; }
        public required string AddMethodShortcut { get; set; }
        public required string OutputMenu { get; set; }
        public required string CopyMessageShortcut { get; set; }
        public required string ClearMessageShortcut { get; set; }
        public required string AllFiguresMinimizedShortcut { get; set; }
        public required string CloseAllFiguresShortcut { get; set; }
        public required string HelpMenu { get; set; }
        public required string GetStartedShortcut { get; set; }
        public required string VEMSDocsShortcut { get; set; }
        public required string LinksShortcut { get; set; }
        public required string HelpShortcut { get; set; }
        public required string LicenseInfoShortcut { get; set; }
        public required string UpdateAppShortcut { get; set; }
        public required string AboutVEMSShortcut { get; set; }
        public required string ContactUSShortcut { get; set; }
        public required string JoinShortcut { get; set; }

        // 新增功能快捷键
        public required string OpenExplorerShortcut { get; set; }
        public required string OpenSearchShortcut { get; set; }
        public required string OpenSourceControlShortcut { get; set; }
        public required string OpenRunDebugShortcut { get; set; }
        public required string OpenUserPreferencesShortcut { get; set; }
        public required string OpenUserPreformanceShortcut { get; set; }
        public required string OpenThemeShortcut { get; set; }
        public required string OpenLanguageShortcut { get; set; }
    }

}
