using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VEMS.MathCore;

namespace VEMS.WorkBench
{

    /// <summary>
    /// VEMS Code File class
    /// </summary>
    public class VFileItem : ListViewItem, INotifyPropertyChanged
    {
        #region PropertyChanged ...
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        #region properties

        /// <summary>
        /// file name including full path
        /// </summary>
        public string? FullName { get; set; }
        
        /// <summary>
        /// short file name for display
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (FullName == null || FullName == "")
                    return " ";

                FileInfo fileInfo = new(FullName);
                return " " + fileInfo.Name;
            }
            set
            {
                if (value == null || value == "")
                    return;

                DisplayName = value;
                // path missing
                FullName = DisplayName;
            }
        }

        #endregion
        #region constructor

        public VFileItem()
        {
            // create RenamableText
            RenamableText reText = new();
            reText.txtblk.Text = DisplayName;
            reText.txtbox.Text = DisplayName;
            // set content to reText
            Content = reText;
        }

        public VFileItem(string fullname)
        {
            FullName = fullname;
            // create RenamableText
            RenamableText reText = new();
            reText.txtblk.Text = DisplayName;
            reText.txtbox.Text = DisplayName;
            // set content to reText
            Content = reText;
        }

        #endregion
        #region methods

        /// <summary>
        /// checks if there is at least one selected VFileItem
        /// </summary>
        /// <returns> true or false </returns>
        private bool CheckFileAvailable()
        { 
            if(this == null)
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
            if(!CheckFileAvailable()) { return; }
            Clipboard.SetText(FullName);
        }
            
        /// <summary>
        /// copies the path of the containing folder
        /// to the clipboard
        /// </summary>
        internal void CopyFolderPath()
        {
            if(!CheckFileAvailable()) { return; }
            Clipboard.SetText(GetFolderPath());
        }

        /// <summary>
        /// opens the folder containing the selected file
        /// </summary>
        internal void OpenContainingFolder()
        {
            if(!CheckFileAvailable()) { return; }
            Process.Start("Explorer", "/select," + FullName);
        }

        /// <summary>
        /// deletes the selected file
        /// </summary>
        [Obsolete]
        internal void DeleteFile()
        {
            if(!CheckFileAvailable()) { return; }

            MessageBox.Show($"currently selected file: {DisplayName}");
            // message box to confirm the action
            MessageBoxResult msgResult = MessageBox.Show("Do you really want to delete the selected file?",
                        "msg", MessageBoxButton.YesNoCancel);
            // cases
            switch (msgResult)
            {
                case MessageBoxResult.Yes:
                    {
                        if (FullName != null) { File.Delete(FullName); }
                        //appFiles.Remove(this);
                        break;
                    }
                case MessageBoxResult.No:
                    break;
                case MessageBoxResult.Cancel:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// delete the current file
        /// </summary>
        internal void Delete()
        {
            if (!CheckFileAvailable()) { return; }
            if(FullName != null)
            { File.Delete(FullName); }
        }


        public void Txt_content_KeyDown(object sender, KeyEventArgs e)

        {

            //if (e.Key == Key.Enter)
            //{
            //    this.Content = this.DataContext;
            //}

        }
        public void Rename()
        {
            // set the TextBlock to collapsed
            TextBlock block = ((RenamableText)Content).txtblk;
            block.Visibility = System.Windows.Visibility.Collapsed;
            // show the TextBox
            TextBox box = ((RenamableText)Content).txtbox;
            box.Visibility = System.Windows.Visibility.Visible;
            box.Focus();
            //box.SelectAll();
            int d = box.Text.Length - 4;
            if (d > 0)
            {
                box.Select(1, d);
            }


        }


        #endregion

    }



}
