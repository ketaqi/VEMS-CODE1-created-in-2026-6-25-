using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using VEMS.MathCore;

namespace VEMS.WorkBench
{
    /// <summary>
    /// document tree structure
    /// </summary>
    public partial class VTreeItem : TreeViewItem, INotifyPropertyChanged
    {
        #region PropertyChanged ...

        /// <summary>
        /// property changed
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        
        /// <summary>
        /// on property changed
        /// </summary>
        /// <param name="propertyName"> name of property </param>
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
        #region properties

        /// <summary>
        /// full name
        /// </summary>
        private string fullName;

        /// <summary>
        /// full name containing the whole path 
        /// </summary>
        public string FullName
        {
            get => fullName;
            set
            {
                fullName = value;
                
                // define header: judge folder or file
                FileAttributes attr = File.GetAttributes(fullName);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    Header = new DirectoryInfo(fullName).Name;
                else
                    Header =$" - {new DirectoryInfo(fullName).Name}";

                OnPropertyChanged(nameof(FullName));
                OnPropertyChanged(nameof(Header));
            }
        }

        #endregion
        #region constructor

        /// <summary>
        /// default constructor
        /// </summary>
        public VTreeItem()
        {
            fullName = string.Empty;
        }

        #endregion
        #region methods


        /// <summary>
        /// checks if there is at least one selected VFileItem
        /// </summary>
        /// <returns> true or false </returns>
        private bool CheckFileAvailable()
        {
            if (this == null)
            {
                Printer.Warning("No available file(s) for the selected operation ...", false);
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// gets the path of the containing folder
        /// </summary>
        /// <returns> folder path </returns>
        private string? GetFolderPath()
        {
            return (FullName == null) ?
                null : Directory.GetParent(FullName)?.FullName;
        }


        /// <summary>
        /// copies the path of the selected file
        /// to the clipboard
        /// </summary>
        internal void CopyFilePath()
        {
            if (!CheckFileAvailable()) { return; }
            Clipboard.SetText(FullName);
        }

        /// <summary>
        /// copies the path of the containing folder
        /// to the clipboard
        /// </summary>
        internal void CopyFolderPath()
        {
            if (!CheckFileAvailable()) { return; }
            Clipboard.SetText(GetFolderPath());
        }

        /// <summary>
        /// opens the folder containing the selected file
        /// </summary>
        internal void OpenContainingFolder()
        {
            if (!CheckFileAvailable()) { return; }
            Process.Start("Explorer", "/select," + FullName);
        }

        /// <summary>
        /// delete the current file
        /// </summary>
        internal void Delete()
        {
            if (!CheckFileAvailable()) { return; }
            if (FullName != null)
            { File.Delete(FullName); }
        }

        #endregion

    }
}
