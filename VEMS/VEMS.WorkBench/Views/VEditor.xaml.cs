using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace VEMS.WorkBench
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class VEditor : Window
    {
        private VEditorModel vEditorWPFModel;

        public VEditor()
        {
            InitializeComponent();

            vEditorWPFModel = new VEditorModel();
            DataContext = vEditorWPFModel;
            // console outputter
            vEditorWPFModel.VConsole.TxtBox = this.txtBox_Console;

            // handling treeview of folders/documents ...
            vEditorWPFModel.AppTree.IsExpanded = true;
            MTrv.ItemsSource = new ObservableCollection<VTreeItem>() { vEditorWPFModel.AppTree };
            MTrv.SelectedItemChanged += (o, e) =>
            {
                vEditorWPFModel.SelectedVTreeItem = (VTreeItem)MTrv.SelectedItem;
            };


            //Console.SetOut(vEditorWPFModel.InConsole);
            //VEMSConsole OutConsole = new() { txtBox = this.txtBox_Console };
            //Console.SetOut(OutConsole);

            // on window closing ... handling the event
            this.Closing += (o, e) => 
            {
                if(vEditorWPFModel.CanSafelyExitApp())
                    Environment.Exit(0);
                else
                {
                    MessageBoxResult msgResult = System.Windows.MessageBox.Show("Changes are not saved. Do you want to save the changes?",
                        "VEMS WorkBench", MessageBoxButton.YesNoCancel);
                    switch (msgResult)
                    {
                        case MessageBoxResult.Yes:
                            vEditorWPFModel.SaveAllFiles();
                            if(vEditorWPFModel.CanSafelyExitApp())
                                Environment.Exit(0);
                            else
                                e.Cancel = true;
                            break;
                        case MessageBoxResult.No:
                            Environment.Exit(0);
                            break;
                        case MessageBoxResult.Cancel:
                            e.Cancel = true;
                            break;
                        default:
                            e.Cancel = false;
                            break;
                    }
                }
            };
        }


        #region short cut keyboard combinations

        /*/// <summary>
        /// shortcut key binding for New
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> e </param>
        private void ShortCutNew_Executed(object sender, ExecutedRoutedEventArgs e)
            => vEditorWPFModel.NewEmpty();//NewVEditTab_Empty();

        /// <summary>
        /// shortcut key binding for Open
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> e </param>
        private void ShortCutOpen_Executed(object sender, ExecutedRoutedEventArgs e)
            => vEditorWPFModel.OpenFile();

        /// <summary>
        /// shortcut key binding for Save 
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> e </param>
        private void ShortCutSave_Executed(object sender, ExecutedRoutedEventArgs e)
            => vEditorWPFModel.SaveFile();
        
        /// <summary>
        /// shortcut key binding for Save As
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> e </param>
        private void ShortCutSaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
            => vEditorWPFModel.SaveFileAs();

        /// <summary>
        /// shortcut key binding for Close
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> e </param>
        private void ShortCutCloseTab_Executed(object sender, ExecutedRoutedEventArgs e)
            => vEditorWPFModel.CloseFile();

        /// <summary>
        /// shortcut key binding for Find/Search
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> e </param>
        private void ShortCutFind_Executed(object sender, ExecutedRoutedEventArgs e)
            => vEditorWPFModel.Find();

        /// <summary>
        /// shortcut key binding for Replace
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> e </param>
        private void ShortCutReplace_Executed(object sender, ExecutedRoutedEventArgs e)
            => vEditorWPFModel.Replace();

        /// <summary>
        /// shortcut key binding for Format
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> e </param>
        private void ShortCutFormat_Executed(object sender, ExecutedRoutedEventArgs e)
            => vEditorWPFModel.FormatCode();

        /// <summary>
        /// shortcut key binding for Comment
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> e </param>
        private void ShortCutComment_Executed(object sender, ExecutedRoutedEventArgs e)
            => vEditorWPFModel.CommentLines(); // CommentLinesCommand.Execute(e);

        /// <summary>
        /// shortcut key binding for Uncomment
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> e </param>
        private void ShortCutUncomment_Executed(object sender, ExecutedRoutedEventArgs e)
            => vEditorWPFModel.UncommentLines(); //UncommentLinesCommand.Execute(e);

        /// <summary>
        /// shortcut key binding for Diagnostic
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> e </param>
        private void ShortCutDiagnostic_Executed(object sender, ExecutedRoutedEventArgs e)
            => vEditorWPFModel.CompileDiagnostic();

        /// <summary>
        /// shortcut key binding for Run
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> e </param>
        private void ShortCutRun_Executed(object sender, ExecutedRoutedEventArgs e)
            => vEditorWPFModel.Run();

        /// <summary>
        /// shortcut key binding for ClearMessage
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> e </param>
        private void ShortCutClearMessage_Executed(object sender, ExecutedRoutedEventArgs e)
            => vEditorWPFModel.ClearConsole();
*/


        #endregion

        #region test ...

        /// <summary>
        /// renames the selected file
        /// </summary>
        /// <param name="sender"> sender </param>
        /// <param name="e"> e </param>
        /// <exception cref="NotImplementedException"></exception>
        private void ContextMenuItem_Rename(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();

            //int i = vEditorWPFModel.SelectedVFileIndex;
            // get the TextBlock and the TextBox
            //TextBlock txtblk = (TextBlock)((Grid)VFileListView.SelectedItem).Children[0];
            //TextBox txtbox = (TextBox)((Grid)VFileListView.SelectedItem).Children[1];
            // altering visibility
            //txtblk.Visibility = Visibility.Collapsed;
            //txtbox.Visibility = Visibility.Visible;

            //vEditorWPFModel.SelectedVFileItem.Rename();

        }

        private void txtblk_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.TextBox txt = (System.Windows.Controls.TextBox)((Grid)((TextBlock)sender).Parent).Children[1];
            txt.Visibility = Visibility.Visible;
            ((TextBlock)sender).Visibility = Visibility.Collapsed;

        }

        private void txtbox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBlock tb = (TextBlock)((Grid)((System.Windows.Controls.TextBox)sender).Parent).Children[0];
            tb.Text = ((System.Windows.Controls.TextBox)sender).Text;
            tb.Visibility = Visibility.Visible;
            ((System.Windows.Controls.TextBox)sender).Visibility = Visibility.Collapsed;
        }


        private void txtbox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)

                // your event handler here
                e.Handled = true;
            System.Windows.MessageBox.Show("Enter pressed");
        }








        //private void ListViewItem_MouseRightDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ChangedButton == MouseButton.Right)
        //    {
        //        MessageBox.Show("Mouse Right Down");


        //        //vEditorWPFModel.NewVEditTab_LoadSelected();
        //    }


        //    //VCodeFile vFile = vEditorWPFModel.SelectedVFile;
        //    //MessageBox.Show("file name = " + vFile.DisplayName
        //    //MessageBox.Show()
        //}

        //private void ListViewItem_PreviousMouseRightDown(object sender, MouseButtonEventArgs e)
        //{
        //    MessageBox.Show("Previous Mouse Right Down");
        //}

        //private void ListViewItem_PreviousMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    //MessageBox.Show("Mouse Down");
        //    if (e.ChangedButton == MouseButton.Right)
        //    {
        //        MessageBox.Show("Mouse Right Down: " + e.ClickCount.ToString() );
        //    }
        //    else if (e.ChangedButton == MouseButton.Left)
        //    {
        //        MessageBox.Show("Mouse Left Down: " + e.ClickCount.ToString());

        //    }


        //}

        //private void OnMouseEnterItem(object sender, MouseEventArgs e)
        //{
        //    Console.WriteLine("Index: " + VFileListView.Items.IndexOf(sender));
        //}


        #endregion

    }
}
   
