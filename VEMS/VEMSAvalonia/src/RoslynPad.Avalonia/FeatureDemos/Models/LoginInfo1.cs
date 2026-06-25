using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using PropertyModels.ComponentModel;
using RoslynPad.FeatureDemos.Views;
using RoslynPad.ViewModels;

namespace RoslynPad.FeatureDemos.Models
{
    public class LoginInfo1 : MiniReactiveObject
    {

        private readonly MainViewModel _mainViewModel;
        public LoginInfo1(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
        }

        #region TopItemsShortcut
        [Category("TopItemsShortcut")]
        [DisplayName("FileMenu")]
        [Description("快捷键")] [Hotkey]
        public string FileMenu
        {
            get => _mainViewModel.FileMenu;
            set
            {
                if (_mainViewModel.FileMenu != value)
                {
                    _mainViewModel.FileMenu = value;
                }
            }
        }

        [Category("TopItemsShortcut")]
        [DisplayName("CodeMenu")]
        [Description("快捷键")] [Hotkey]
        public string CodeMenu
        {
            get => _mainViewModel.CodeMenu;
            set
            {
                if (_mainViewModel.CodeMenu != value)
                {
                    _mainViewModel.CodeMenu = value;
                }
            }
        }
        [Category("TopItemsShortcut")]
        [DisplayName("OutputMenu")]
        [Description("快捷键")] [Hotkey]
        public string OutputMenu
        {
            get => _mainViewModel.OutputMenu;
            set
            {
                if (_mainViewModel.OutputMenu != value)
                {
                    _mainViewModel.OutputMenu = value;
                }
            }
        }

        [Category("TopItemsShortcut")]
        [DisplayName("HelpMenu")]
        [Description("快捷键")] [Hotkey]
        public string HelpMenu
        {
            get => _mainViewModel.HelpMenu;
            set
            {
                if (_mainViewModel.HelpMenu != value)
                {
                    _mainViewModel.HelpMenu = value;
                }
            }
        }
        #endregion

        #region FileShortcut
        [Category("FileShortcut")]
        [DisplayName("NewFileShortcut")]
        [Description("快捷键")] [Hotkey]
        public string NewFileShortcut
        {
            get => _mainViewModel.NewFileShortcut;
            set
            {
                if (_mainViewModel.NewFileShortcut != value)
                {
                    _mainViewModel.NewFileShortcut = value;
                }
            }
        }

        [Category("FileShortcut")]
        [DisplayName("OpenFileShortcut")]
        [Description("快捷键")] [Hotkey]
        public string OpenFileShortcut
        {
            get => _mainViewModel.OpenFileShortcut;
            set
            {
                if (_mainViewModel.OpenFileShortcut != value)
                {
                    _mainViewModel.OpenFileShortcut = value;
                }
            }
        }

        [Category("FileShortcut")]
        [DisplayName("OpenFolderShortcut")]
        [Description("快捷键")] [Hotkey]
        public string OpenFolderShortcut
        {
            get => _mainViewModel.OpenFolderShortcut;
            set
            {
                if (_mainViewModel.OpenFolderShortcut != value)
                {
                    _mainViewModel.OpenFolderShortcut = value;
                }
            }
        }

        [Category("FileShortcut")]
        [DisplayName("DefaultFolderShortcut")]
        [Description("快捷键")] [Hotkey]
        public string DefaultFolderShortcut
        {
            get => _mainViewModel.DefaultFolderShortcut;
            set
            {
                if (_mainViewModel.DefaultFolderShortcut != value)
                {
                    _mainViewModel.DefaultFolderShortcut = value;
                }
            }
        }

        [Category("FileShortcut")]
        [DisplayName("SaveFileShortcut")]
        [Description("快捷键")] [Hotkey]
        public string SaveFileShortcut
        {
            get => _mainViewModel.SaveFileShortcut;
            set
            {
                if (_mainViewModel.SaveFileShortcut != value)
                {
                    _mainViewModel.SaveFileShortcut = value;
                }
            }
        }

        [Category("FileShortcut")]
        [DisplayName("SaveAsShortcut")]
        [Description("快捷键")] [Hotkey]
        public string SaveAsShortcut
        {
            get => _mainViewModel.SaveAsShortcut;
            set
            {
                if (_mainViewModel.SaveAsShortcut != value)
                {
                    _mainViewModel.SaveAsShortcut = value;
                }
            }
        }

        [Category("FileShortcut")]
        [DisplayName("SaveAllShortcut")]
        [Description("快捷键")] [Hotkey]
        public string SaveAllShortcut
        {
            get => _mainViewModel.SaveAllShortcut;
            set
            {
                if (_mainViewModel.SaveAllShortcut != value)
                {
                    _mainViewModel.SaveAllShortcut = value;
                }
            }
        }

        [Category("FileShortcut")]
        [DisplayName("CloseTabShortcut")]
        [Description("快捷键")] [Hotkey]
        public string CloseTabShortcut
        {
            get => _mainViewModel.CloseTabShortcut;
            set
            {
                if (_mainViewModel.CloseTabShortcut != value)
                {
                    _mainViewModel.CloseTabShortcut = value;
                }
            }
        }

        [Category("FileShortcut")]
        [DisplayName("CloseAllShortcut")]
        [Description("快捷键")] [Hotkey]
        public string CloseAllShortcut
        {
            get => _mainViewModel.CloseAllShortcut;
            set
            {
                if (_mainViewModel.CloseAllShortcut != value)
                {
                    _mainViewModel.CloseAllShortcut = value;
                }
            }
        }

        [Category("FileShortcut")]
        [DisplayName("ExitAppShortcut")]
        [Description("快捷键")] [Hotkey]
        public string ExitAppShortcut
        {
            get => _mainViewModel.ExitAppShortcut;
            set
            {
                if (_mainViewModel.ExitAppShortcut != value)
                {
                    _mainViewModel.ExitAppShortcut = value;
                }
            }
        }
        #endregion

        #region CodeShortcut
        //[Category("CodeShortcut")]
        //[DisplayName("SearchTextShortcut")]
        //[Description("快捷键")] [Hotkey]
        //public string SearchTextShortcut
        //{
        //    get => _mainViewModel.SearchTextShortcut;
        //    set
        //    {
        //        if (_mainViewModel.SearchTextShortcut != value)
        //        {
        //            _mainViewModel.SearchTextShortcut = value;
        //        }
        //    }
        //}

        //[Category("CodeShortcut")]
        //[DisplayName("ReplaceTextShortcut")]
        //[Description("快捷键")] [Hotkey]
        //public string ReplaceTextShortcut
        //{
        //    get => _mainViewModel.ReplaceTextShortcut;
        //    set
        //    {
        //        if (_mainViewModel.ReplaceTextShortcut != value)
        //        {
        //            _mainViewModel.ReplaceTextShortcut = value;
        //        }
        //    }
        //}

        [Category("CodeShortcut")]
        [DisplayName("FormatCodeShortcut")]
        [Description("快捷键")] [Hotkey]
        public string FormatCodeShortcut
        {
            get => _mainViewModel.FormatCodeShortcut;
            set
            {
                if (_mainViewModel.FormatCodeShortcut != value)
                {
                    _mainViewModel.FormatCodeShortcut = value;
                }
            }
        }

        [Category("CodeShortcut")]
        [DisplayName("CommentSelectionShortcut")]
        [Description("快捷键")] [Hotkey]
        public string CommentSelectionShortcut
        {
            get => _mainViewModel.CommentSelectionShortcut;
            set
            {
                if (_mainViewModel.CommentSelectionShortcut != value)
                {
                    _mainViewModel.CommentSelectionShortcut = value;
                }
            }
        }

        [Category("CodeShortcut")]
        [DisplayName("UncommentSelectionShortcut")]
        [Description("快捷键")] [Hotkey]
        public string UncommentSelectionShortcut
        {
            get => _mainViewModel.UncommentSelectionShortcut;
            set
            {
                if (_mainViewModel.UncommentSelectionShortcut != value)
                {
                    _mainViewModel.UncommentSelectionShortcut = value;
                }
            }
        }

        [Category("CodeShortcut")]
        [DisplayName("DebugCodeShortcut")]
        [Description("快捷键")] [Hotkey]
        public string DebugCodeShortcut
        {
            get => _mainViewModel.DebugCodeShortcut;
            set
            {
                if (_mainViewModel.DebugCodeShortcut != value)
                {
                    _mainViewModel.DebugCodeShortcut = value;
                }
            }
        }

        [Category("CodeShortcut")]
        [DisplayName("RunCodeShortcut")]
        [Description("快捷键")] [Hotkey]
        public string RunCodeShortcut
        {
            get => _mainViewModel.RunCodeShortcut;
            set
            {
                if (_mainViewModel.RunCodeShortcut != value)
                {
                    _mainViewModel.RunCodeShortcut = value;
                }
            }
        }

        //[Category("CodeShortcut")]
        //[DisplayName("AddClassShortcut")]
        //[Description("快捷键")] [Hotkey]
        //public string AddClassShortcut
        //{
        //    get => _mainViewModel.AddClassShortcut;
        //    set
        //    {
        //        if (_mainViewModel.AddClassShortcut != value)
        //        {
        //            _mainViewModel.AddClassShortcut = value;
        //        }
        //    }
        //}

        //[Category("CodeShortcut")]
        //[DisplayName("AddMethodShortcut")]
        //[Description("快捷键")] [Hotkey]
        //public string AddMethodShortcut
        //{
        //    get => _mainViewModel.AddMethodShortcut;
        //    set
        //    {
        //        if (_mainViewModel.AddMethodShortcut != value)
        //        {
        //            _mainViewModel.AddMethodShortcut = value;
        //        }
        //    }
        //}
        #endregion

        #region OutputShortcut
        [Category("OutputShortcut")]
        [DisplayName("CopyMessageShortcut")]
        [Description("快捷键")] [Hotkey]
        public string CopyMessageShortcut
        {
            get => _mainViewModel.CopyMessageShortcut;
            set
            {
                if (_mainViewModel.CopyMessageShortcut != value)
                {
                    _mainViewModel.CopyMessageShortcut = value;
                }
            }
        }

        [Category("OutputShortcut")]
        [DisplayName("ClearMessageShortcut")]
        [Description("快捷键")] [Hotkey]
        public string ClearMessageShortcut
        {
            get => _mainViewModel.ClearMessageShortcut;
            set
            {
                if (_mainViewModel.ClearMessageShortcut != value)
                {
                    _mainViewModel.ClearMessageShortcut = value;
                }
            }
        }


        //[Category("OutputShortcut")]
        //[DisplayName("AllFiguresMinimizedShortcut")]
        //[Description("快捷键")] [Hotkey]
        //public string AllFiguresMinimizedShortcut
        //{
        //    get => _mainViewModel.AllFiguresMinimizedShortcut;
        //    set
        //    {
        //        if (_mainViewModel.AllFiguresMinimizedShortcut != value)
        //        {
        //            _mainViewModel.AllFiguresMinimizedShortcut = value;
        //        }
        //    }
        //}

        //[Category("OutputShortcut")]
        //[DisplayName("CloseAllFiguresShortcut")]
        //[Description("快捷键")] [Hotkey]
        //public string CloseAllFiguresShortcut
        //{
        //    get => _mainViewModel.CloseAllFiguresShortcut;
        //    set
        //    {
        //        if (_mainViewModel.CloseAllFiguresShortcut != value)
        //        {
        //            _mainViewModel.CloseAllFiguresShortcut = value;
        //        }
        //    }
        //}
        #endregion

        #region HelpShortcut1
        //[Category("HelpShortcut")]
        //[DisplayName("GetStartedShortcut")]
        //[Description("快捷键")] [Hotkey]
        //public string GetStartedShortcut
        //{
        //    get => _mainViewModel.GetStartedShortcut;
        //    set
        //    {
        //        if (_mainViewModel.GetStartedShortcut != value)
        //        {
        //            _mainViewModel.GetStartedShortcut = value;
        //        }
        //    }
        //}

        //[Category("HelpShortcut")]
        //[DisplayName("VEMSDocsShortcut")]
        //[Description("快捷键")] [Hotkey]
        //public string VEMSDocsShortcut
        //{
        //    get => _mainViewModel.VEMSDocsShortcut;
        //    set
        //    {
        //        if (_mainViewModel.VEMSDocsShortcut != value)
        //        {
        //            _mainViewModel.VEMSDocsShortcut = value;
        //        }
        //    }
        //}

        //[Category("HelpShortcut")]
        //[DisplayName("LinksShortcut")]
        //[Description("快捷键")] [Hotkey]
        //public string LinksShortcut
        //{
        //    get => _mainViewModel.LinksShortcut;
        //    set
        //    {
        //        if (_mainViewModel.LinksShortcut != value)
        //        {
        //            _mainViewModel.LinksShortcut = value;
        //        }
        //    }
        //}

        [Category("HelpShortcut1")]
        [DisplayName("HelpShortcut")]
        [Description("快捷键")] [Hotkey]
        public string HelpShortcut
        {
            get => _mainViewModel.HelpShortcut;
            set
            {
                if (_mainViewModel.HelpShortcut != value)
                {
                    _mainViewModel.HelpShortcut = value;
                }
            }
        }

        [Category("HelpShortcut1")]
        [DisplayName("LicenseInfoShortcut")]
        [Description("快捷键")] 
        [Hotkey] 
        public string LicenseInfoShortcut
        {
            get => _mainViewModel.LicenseInfoShortcut;
            set
            {
                if (_mainViewModel.LicenseInfoShortcut != value)
                {
                    _mainViewModel.LicenseInfoShortcut = value;
                }
            }
        }

        [Category("HelpShortcut1")]
        [DisplayName("UpdateAppShortcut")]
        [Description("快捷键")] [Hotkey]
        public string UpdateAppShortcut
        {
            get => _mainViewModel.UpdateAppShortcut;
            set
            {
                if (_mainViewModel.UpdateAppShortcut != value)
                {
                    _mainViewModel.UpdateAppShortcut = value;
                }
            }
        }

        [Category("HelpShortcut1")]
        [DisplayName("AboutVEMSShortcut")]
        [Description("快捷键")] [Hotkey]
        public string AboutVEMSShortcut
        {
            get => _mainViewModel.AboutVEMSShortcut;
            set
            {
                if (_mainViewModel.AboutVEMSShortcut != value)
                {
                    _mainViewModel.AboutVEMSShortcut = value;
                }
            }
        }

        [Category("HelpShortcut1")]
        [DisplayName("ContactUSShortcut")]
        [Description("快捷键")] [Hotkey]
        public string ContactUSShortcut
        {
            get => _mainViewModel.ContactUSShortcut;
            set
            {
                if (_mainViewModel.ContactUSShortcut != value)
                {
                    _mainViewModel.ContactUSShortcut = value;
                }
            }
        }

        [Category("HelpShortcut1")]
        [DisplayName("JoinShortcut")]
        [Description("快捷键")] [Hotkey]
        public string JoinShortcut
        {
            get => _mainViewModel.JoinShortcut;
            set
            {
                if (_mainViewModel.JoinShortcut != value)
                {
                    _mainViewModel.JoinShortcut = value;
                }
            }
        }
        #endregion

    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class EncryptData1 : MiniReactiveObject
    {

        public EncryptionPolicy Policy { get; set; } = EncryptionPolicy.RequireEncryption;

        public RSAEncryptionPaddingMode PaddingMode { get; set; } = RSAEncryptionPaddingMode.Pkcs1;
    }
}
