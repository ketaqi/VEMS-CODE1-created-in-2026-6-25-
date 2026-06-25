using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Net.Sockets;

using RoslynPad.Roslyn;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;

using VEMS.MathCore;
using Document = Microsoft.CodeAnalysis.Document;
using TextChange = Microsoft.CodeAnalysis.Text.TextChange;
using TextLine = Microsoft.CodeAnalysis.Text.TextLine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net;
using System.Diagnostics;

namespace VEMS.WorkBench
{
    public partial class VEditorModel : ObservableObject
    {

        #region default fields

        // message box title
        private static string msgBoxTitle = "VEMS WorkBench";

        private string versionNumber = $"0000"; // $"{DateTime.Today.Year}{DateTime.Today.Month}{DateTime.Today.Day}";

        [Conditional("DEBUG")]
        private void SetReleaseVersion()
            => versionNumber = $"{DateTime.Today.Year}{DateTime.Today.Month}{DateTime.Today.Day}";

        /// <summary>
        /// text defines the title of the window
        /// </summary>
        public string WindowTitle => $"VEMS WorkBench [{versionNumber}]";

        #endregion
        #region properties

        #region ----- directories -----

        ///// <summary>
        ///// 用于打开新文件夹后的缓存路径
        ///// </summary>
        //public string FolderPath { get; set; } = DirectoryHelper.SampleDirectory;

        #endregion
        #region ----- V-Frames -----

        /// <summary>
        /// link to the list of all the VFrames 
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<VFrame> appFrames = ((App)Application.Current).Frames;

        /// <summary>
        /// index of the currently selected VFrame
        /// </summary>
        public int SelectedVFrameIndex { get; set; }

        #endregion
        #region ----- file & folder openers -----

        /// <summary>
        /// file opener
        /// </summary>
        private OpenFileDialog FileOpener { get; set; }
        
        /// <summary>
        /// file saver
        /// </summary>
        private SaveFileDialog FileSaver { get; set; }
        
        /// <summary>
        /// folder opener
        /// </summary>
        private System.Windows.Forms.FolderBrowserDialog FolderOpener { get; set; }

        #endregion
        #region ----- V-Files & V-Tree (under test) -----

        /// <summary>
        /// search model for V-Files
        /// </summary>
        public SearchModel SearchModel { get; set; } = new SearchModel();

        /// <summary>
        /// currently selected VFile
        /// </summary>
        [ObservableProperty]
        private VFileItem selectedVFileItem; //{ get; set; }

        /// <summary>
        /// link to the list of all the VFiles 
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<VFileItem> appFiles = 
            ((App)Application.Current).Files;

        /// <summary>
        /// currently selected VTreeItem
        /// </summary>
        [ObservableProperty]
        private VTreeItem selectedVTreeItem;

        [ObservableProperty]
        private VTreeItem appTree =
            ((App)Application.Current).Tree;

        [ObservableProperty]
        VTreeItem rootTree;

        [ObservableProperty]
        TreeViewItem mainTree;

        #endregion
        #region ----- network -----

        /// <summary>
        /// socket for parallel computing
        /// </summary>
        public Socket Host { get; set; }

        /// <summary>
        /// list of ip addresses on this computer
        /// </summary>
        [ObservableProperty]
        private List<IPAddress> ips;

        /// <summary>
        /// selected ip address on this computer
        /// for network communication
        /// </summary>
        [ObservableProperty]
        private IPAddress ip;

        /// <summary>
        /// selected port on this computer
        /// for network communication
        /// </summary>
        [ObservableProperty]
        private int port;

        /// <summary>
        /// ip address of the target computer
        /// </summary>
        [ObservableProperty]
        private IPAddress ipTarget;

        /// <summary>
        /// port of the target computer
        /// </summary>
        [ObservableProperty]
        private int portTarget;

        #endregion
        #region ----- code editor -----

        /// <summary>
        /// Roslyn host
        /// </summary>
        public RoslynHost RoslynHost { get; set; }

        /// <summary>
        /// currently selected tab index
        /// </summary>
        public int SelectedTabIndex { get; set; }

        /// <summary>
        /// list of all the VEditor Tabs
        /// </summary>
        public REditTabList VEditTabs { get; set; } = new REditTabList();

        #endregion
        #region ----- console -----

        /// <summary>
        /// V-Console
        /// </summary>
        public VEMSConsole VConsole { get; set; }

        #endregion
        #region ----- user settings ----- 

        /// <summary>
        /// user-preference
        /// </summary>
        public UserPreference Preference{ get; set; } 
            = new(xmlFileName: DirectoryHelper.ConfigDirectory + @"\Preference.xml");

        /// <summary>
        /// performance-setting
        /// </summary>
        public PerformanceSetting Performance { get; set; }
            = new(xmlFileName: DirectoryHelper.ConfigDirectory + @"\Performance.xml");

        #endregion
        #region ----- linear algebra benchmark -----

        /// <summary>
        /// linear algebra benchmark model
        /// </summary>
        public Benchmark Benchmark { get; set; } = new();

        #endregion

        #endregion
        #region constructor

        /// <summary>
        /// model class for VEditorWPF
        /// </summary>
        public VEditorModel()
        {
            SetReleaseVersion();
            // initiates
            InitiateAll();
            // relay commands
            //RelayCommands();
        }

        #endregion
        #region initialization methods

        private void InitiateAll()
        {
            InitiateConsole();
            InitiateSettings();
            InitiateOpenFolderDialog();
            //InitiateFonts();
            //InitiatePrinter();
            InitiateVFiles();
            InitiateVTree();
            SelectedVTreeItem = new();
            InitiateRoslynHost();
            InitiateOpenFileDialog();
            InitiateSaveFileDialog();
            SelectedTabIndex = 0;
            //NumCores = maxCores;
            // parallel
            Ips = Parallel.Network.CheckIPs();
            if (Ips != null) { Ip = Ips[0]; }
            Port = 2024;
        }

        /// <summary>
        /// initializes the console
        /// </summary>
        private void InitiateConsole()
        {
            VConsole = new()
            {
                TxtBox = new(),
                //FontFamily = new("Cascadia Code"),
                //FontSize = 14,
                Content = ">> Welcome to VEMS Workbench <<\r"
            };
            Console.SetOut(VConsole);
        }

        /// <summary>
        /// initializes user-settings
        /// </summary>
        private void InitiateSettings()
        {
            // preference
            Preference.LoadSettingsFromXml();
            VEditTabs.FontFamily = Preference.CodeEditorFontFamily;
            VEditTabs.FontSize = Preference.CodeEditorFontSize;
            VConsole.FontFamily = Preference.ConsoleFontFamily;
            VConsole.FontSize = Preference.ConsoleFontSize;

            // performance
            Performance.LoadSettingsFromXml();
        }

        /// <summary>
        /// initializes all the VFiles from the default folder
        /// </summary>
        private void InitiateVFiles()
        {
            string[] files = Directory.GetFiles(Preference.WorkingDirectory); //FolderPath);
            foreach (string file in files)
            { AppFiles.Add(new VFileItem(file)); }

            // sort according to Windows Explorer
            AppFiles = new ObservableCollection<VFileItem>(AppFiles.OrderBy(x => x.FullName));
            // initialize selected item
            SelectedVFileItem = new();
        }

        private void InitiateVTree()
        {
            DirectoryInfo workFolder = new(Preference.WorkingDirectory); //FolderPath);
            // TreeView version
            //MainTree = new TreeViewItem() { Header = workFolder.Name };
            //GetSubset(workFolder, MainTree);
            // VTreeView version
            AppTree = new VTreeItem() 
            { FullName = workFolder.FullName };
            GetSubsetV(workFolder, AppTree);
        }

        [Obsolete]
        private void GetSubset(DirectoryInfo folder, TreeViewItem node)
        {
            // folder handling
            foreach(DirectoryInfo subFolder in folder.GetDirectories())
            {
                TreeViewItem subFolderNode = new() { Header = subFolder.Name };
                node.Items.Add(subFolderNode);
                // try next sub-level
                if (subFolder.GetDirectories().Length != 0
                    || subFolder.GetFiles().Length != 0)
                { GetSubset(subFolder, subFolderNode); }
            }
            // file hanlding
            foreach(FileInfo file in folder.GetFiles())
            {
                TreeViewItem fileNode = new() { Header = $" - {file.Name}" };
                node.Items.Add(fileNode);
            }
        }

        private void GetSubsetV(DirectoryInfo folder, VTreeItem node)
        {
            // folder handling
            foreach (DirectoryInfo subFolder in folder.GetDirectories())
            {
                VTreeItem subFolderNode = new() { FullName = subFolder.FullName };
                node.Items.Add(subFolderNode);
                // try next sub-level
                if (subFolder.GetDirectories().Length != 0
                    || subFolder.GetFiles().Length != 0)
                { GetSubsetV(subFolder, subFolderNode); }
            }
            // file hanlding
            foreach (FileInfo file in folder.GetFiles())
            {
                VTreeItem fileNode = new() { FullName = file.FullName };
                node.Items.Add(fileNode);
            }
        }


        /// <summary>
        /// initialize RoslynHost with references
        /// </summary>
        private void InitiateRoslynHost()
            => RoslynHost = Coding.CreateHost();
       
        /// <summary>
        /// initializes the OpenFile dialog
        /// </summary>
        private void InitiateOpenFileDialog() => 
            FileOpener = new OpenFileDialog
        {
            InitialDirectory = Preference.WorkingDirectory, //FolderPath,
            Filter = "C# files (*.cs)|*.cs|Text files (*.txt)|*.txt|All files(*.*)|*.*",
            FilterIndex = 1,
            Title = "Open VEMS Code File",
            Multiselect = false
        };

        /// <summary>
        /// initializes the SaveFile dialog
        /// </summary>
        private void InitiateSaveFileDialog() => 
            FileSaver = new SaveFileDialog
        {
            InitialDirectory = Preference.WorkingDirectory,
            Filter = "C# files (*.cs)|*.cs|Text files (*.txt)|*.txt|All files(*.*)|*.*",
            FilterIndex = 1,
            Title = "Save VEMS Code File",
            DefaultExt = "cs",
            AddExtension = true
        };

        /// <summary>
        /// initializes the OpenFolder dialog
        /// </summary>
        private void InitiateOpenFolderDialog() =>
            FolderOpener = new System.Windows.Forms.FolderBrowserDialog
            {
                InitialDirectory = Preference.WorkingDirectory,
                //SelectedPath = FolderPath
                UseDescriptionForTitle = true,
                Description = "Select VEMS Workspace",             
            };

        #endregion
        #region commands [obsolete]

        //public ICommand ScriptCodeCommand { get; set; }
        //public ICommand StartTaskCommand { get; set; }
        //public ICommand StopTaskCommand { get; set; }
        //public ICommand CheckTaskStatusCommand { get; set; }
        //public ICommand LoadVFileCommand { get; set; }
        //public ICommand PrintRoslynHostRefCommand { get; set; }
        //public ICommand DeleteVFrameCommand { get; set; }
        //public ICommand ShowSelectedVFrameIndexCommand { get; set; }

        //private void RelayCommands()
        //{
        //    // only for internal tests
        //    //StartTaskCommand = new RelayCommand(StartTask);
        //    //ScriptCodeCommand = new RelayCommand(TestScript);
        //    //StopTaskCommand = new RelayCommand(StopTask);
        //    //CheckTaskStatusCommand = new RelayCommand(CheckTaskStatus);
        //    LoadVFileCommand = new RelayCommand(LoadVFileTest);
        //    PrintRoslynHostRefCommand = new RelayCommand(PrintRoslynHostRefs);
        //    DeleteVFrameCommand = new RelayCommand(DeleteVFrame);
        //    ShowSelectedVFrameIndexCommand = new RelayCommand(ShowSelectedVFrameIndex);
        //}

        #endregion
        #region methods & commands

        #region ------- file -------

        #region ===== common =====

        /// <summary>
        /// checks if there is at least one file available
        /// e.g. for save, save as, close ...
        /// </summary>
        private bool CheckTabAvailable()
        {
            if (VEditTabs.Count <= 0)
            {
                Printer.Warning("No available file(s) for the selected operation ...", false);
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion
        #region ===== folder and files =====

        /// <summary>
        /// loads folder specified by a target path
        /// </summary>
        /// <param name="targetPath"> full name of target path </param>
        private void LoadFolder(string targetPath)
        {
            // checks if the new path is the same
            if (Preference.WorkingDirectory.Equals(targetPath)) { return; }
            // changes the working directory
            Printer.Write($"target path: {targetPath}");
            Preference.WorkingDirectory = targetPath;
            // save to Xml???

            // refresh & updates
            AppFiles.Clear();
            InitiateVFiles();
            SearchModel.SearchName = "";
            FolderOpener.InitialDirectory = Preference.WorkingDirectory; // FolderPath;
            FileOpener.InitialDirectory = Preference.WorkingDirectory; //FolderPath;
            FileSaver.InitialDirectory = Preference.WorkingDirectory; //FolderPath;
        }

        /// <summary>
        /// command: open folder
        /// </summary>
        [RelayCommand]
        private void OpenNewFolder()
        {
            if (FolderOpener.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            { return; }

            LoadFolder(FolderOpener.SelectedPath);
        }

        /// <summary>
        /// command: Default folder
        /// </summary>
        [RelayCommand]
        private void OpenDefaultFolder()
            => LoadFolder(DirectoryHelper.SampleDirectory);

        /// <summary>
        /// copy the full path of the selected file
        /// </summary>
        [RelayCommand]
        private void CopyFilePath()
            => SelectedVFileItem.CopyFilePath();

        /// <summary>
        /// copy the folder path
        /// </summary>
        [RelayCommand]
        private void CopyFolderPath()
            => SelectedVFileItem.CopyFolderPath();

        /// <summary>
        /// open the containing folder
        /// </summary>
        [RelayCommand]
        private void OpenContainingFolder()
            => SelectedVFileItem.OpenContainingFolder();


        [RelayCommand]
        private void TrvCopyFilePath()
            => SelectedVTreeItem.CopyFilePath();

        [RelayCommand]
        private void TrvCopyFolderPath()
            => SelectedVTreeItem.CopyFolderPath();

        [RelayCommand]
        private void TrvOpenContainingFolder()
            => SelectedVTreeItem.OpenContainingFolder();

        #endregion
        #region ===== load and create file =====

        /// <summary>
        /// loads string from specific file
        /// </summary>
        /// <param name="fileToLoad"> file that contains the string to load </param>
        /// <returns> loaded string </returns>
        private static string LoadString(FileInfo fileToLoad)
        {
            if (!fileToLoad.Exists)
            { throw new ArgumentException("File Info does not exist"); }

            string res = new StreamReader(fileToLoad.FullName).ReadToEnd();
            return res;
        }

        /// <summary>
        /// checks if the desired file is already open
        /// </summary>
        /// <param name="fileToOpen"> file to open </param>
        private bool CheckFileAlreadyOpen(FileInfo fileToOpen)
        {
            bool isAlreadyOpen = false;

            // only when there is at least one file existing ...
            if (VEditTabs.Count > 0)
            {
                for (int i = 0; i < VEditTabs.Count; i++)
                {
                    FileInfo iFile = VEditTabs[i].CodeFileInfo;
                    if (fileToOpen.FullName == iFile.FullName)
                    {
                        Printer.Logging("Selected file already open ...");
                        VEditTabs[i].IsSelected = true;
                        isAlreadyOpen = true;
                        break;
                    }
                }
            }

            return isAlreadyOpen;
        }

        /// <summary>
        /// kernel method for creating a new VEditTab
        /// </summary>
        /// <param name="workDirectory"> working directory </param>
        /// <param name="name"> name of the tab </param>
        /// <param name="codeText"> content of the code in the tab </param>
        private void NewKernel(DirectoryInfo workDirectory, 
            string name, 
            string codeText)
        {
            // creates VEditTab
            REditTab et = new(host: RoslynHost, 
                workingDirectory: workDirectory,
                desireFileName: name,
                codeText: codeText);

            // settings
            et.Editor.FontFamily = VEditTabs.FontFamily;
            et.Editor.FontSize = VEditTabs.FontSize;
            et.IsSelected = true;
            VEditTabs.Add(et);

            // handling close tab button
            et.ClosableHeader.CloseButton.Click +=
                (o, e) => CloseFile();
        }

        /// <summary>
        /// creates a new VEditTab
        /// which is empty
        /// </summary>
        [RelayCommand]
        internal void NewEmpty()
            => NewKernel(workDirectory: new DirectoryInfo(Preference.WorkingDirectory),
                name: "",
                codeText: "");


        /// <summary>
        /// kernel of loading file
        /// </summary>
        /// <param name="fileToLoad"> the file to load/open </param>
        private void LoadKernel(FileInfo fileToLoad)
        {
            // checks if selected file already open
            if (CheckFileAlreadyOpen(fileToLoad)) { return; }

            // creates VEditTab
            NewKernel(workDirectory: fileToLoad.Directory ?? new(Preference.WorkingDirectory),
                name: fileToLoad.Name,
                codeText: LoadString(fileToLoad));
        }

        /// <summary>
        /// creates a new VEditTab
        /// by loading a certain file
        /// </summary>
        [RelayCommand]
        internal void OpenFile()
        {
            // opens file dialog
            if (FileOpener.ShowDialog() == false)
            {
                Printer.Warning("No file(s) selected ...", false);
                return;
            }

            // calls load kernel
            FileInfo codeTextFileInfo = new(FileOpener.FileName);
            LoadKernel(codeTextFileInfo);
        }

        /// <summary>
        /// creates a new VEditTab
        /// from the selected file
        /// </summary>
        [RelayCommand]
        internal void LoadSelectedFile()
        {
            VFileItem f = SelectedVFileItem;
            //VTreeItem f = SelectedVTreeItem;
            if (f.FullName == null || f.FullName == "")
            {
                Printer.Warning("No file(s) selected ...", false);
                return;
            }

            // calls load kernel
            FileInfo codeTextFileInfo = new(f.FullName); 
            if (!codeTextFileInfo.Extension.Equals(".txt") && !codeTextFileInfo.Extension.Equals(".cs"))
            {
                MessageBoxResult msgResult = MessageBox.Show("This file does not support.\n Do you still want to open it?",
                    msgBoxTitle, MessageBoxButton.YesNo);

                switch (msgResult)
                {
                    case MessageBoxResult.Yes:
                        {
                            LoadKernel(codeTextFileInfo);
                            break;
                        }
                    case MessageBoxResult.No:
                        break;
                }
            }
            else
            {
                LoadKernel(codeTextFileInfo);
            }
        }

        [RelayCommand]
        internal void TrvLoadSelectedFile() 
        {
            VTreeItem f = SelectedVTreeItem;
            if (f.FullName == null || f.FullName == "")
            {
                Printer.Warning("No file(s) selected ...", false);
                return;
            }

            // calls load kernel
            FileInfo codeTextFileInfo = new(f.FullName);
            if (!codeTextFileInfo.Extension.Equals(".txt") && !codeTextFileInfo.Extension.Equals(".cs"))
            {
                MessageBoxResult msgResult = MessageBox.Show("This file does not support.\n Do you still want to open it?",
                    msgBoxTitle, MessageBoxButton.YesNo);

                switch (msgResult)
                {
                    case MessageBoxResult.Yes:
                        {
                            LoadKernel(codeTextFileInfo);
                            break;
                        }
                    case MessageBoxResult.No:
                        break;
                }
            }
            else
            {
                LoadKernel(codeTextFileInfo);
            }
        }

        #endregion
        #region ===== save file =====

        /// <summary>
        /// kernel SaveAs method for a given REditTab
        /// </summary>
        /// <param name="et"> specific REditTab </param>
        private void SaveAsKernel(REditTab et)
        {
            // prepare FileSave
            FileSaver.FileName = et.CodeFileInfo.Name;
            if (FileSaver.ShowDialog() == false)
            {
                Printer.Logging("No file(s) selected ...", false);
                return;
            }
            // try to save
            et.Save(new FileInfo(FileSaver.FileName));

            // file tree updating ...
            if (Preference.WorkingDirectory == et.CodeFileInfo.DirectoryName) 
            {
                AppFiles.Add(new VFileItem(et.CodeFileInfo.FullName));
                AppFiles = new ObservableCollection<VFileItem>(AppFiles.OrderBy(x => x.FullName));
            }
        }

        /// <summary>
        /// saves the code file in the active tab
        /// as a new file
        /// </summary>
        [RelayCommand]
        internal void SaveFileAs()
        {
            if (!CheckTabAvailable()) { return; }
            // get current tab
            REditTab et = VEditTabs[SelectedTabIndex];
            SaveAsKernel(et);
        }

        /// <summary>
        /// kernel Save method for a given REditTab
        /// </summary>
        /// <param name="et"> specific REditTab </param>
        private void SaveKernel(REditTab et)
        {
            // try to save ...
            if (et.CodeFileInfo.Exists)
                et.Save();
            else
                SaveAsKernel(et);
        }

        /// <summary>
        /// saves the code file in the active tab 
        /// </summary>
        [RelayCommand]
        internal void SaveFile()
        {
            if (!CheckTabAvailable()) { return; }

            // get current tab
            REditTab et = VEditTabs[SelectedTabIndex];
            // SaveKernel
            SaveKernel(et);
        }

        /// <summary>
        /// saves the codes in all editors
        /// </summary>
        [RelayCommand]
        internal void SaveAllFiles()
        {
            if (!CheckTabAvailable()) { return; }

            // loop and save
            for (int i = 0; i < VEditTabs.Count; i++)
            {
                // get current tab
                REditTab et = VEditTabs[i];
                et.Focus();
                SaveKernel(et);
            }
        }

        #endregion
        #region ===== close and delete file =====

        /// <summary>
        /// kernel Close method for a given REditTab
        /// </summary>
        /// <param name="et"></param>
        private void CloseKernel(REditTab et)
        {
            if (et.Editor.IsModified)
            {
                MessageBoxResult msgResult = MessageBox.Show("Changes are not saved. Do you want to save the changes?",
                    msgBoxTitle, MessageBoxButton.YesNoCancel);

                switch (msgResult)
                {
                    case MessageBoxResult.Yes:
                        {
                            SaveFile();
                            VEditTabs.Remove(et);
                            break;
                        }
                    case MessageBoxResult.No:
                        VEditTabs.Remove(et);
                        break;
                    case MessageBoxResult.Cancel:
                        break;
                    default:
                        break;
                }
            }
            else
            {
                VEditTabs.Remove(et);
            }
        }

        /// <summary>
        /// closes the active tab
        /// </summary>
        [RelayCommand]
        internal void CloseFile()
        {
            if (!CheckTabAvailable()) { return; }

            // get the current tab
            REditTab et = VEditTabs[SelectedTabIndex];
            CloseKernel(et);
        }

        /// <summary>
        /// closes all tabs
        /// </summary>  
        [RelayCommand]
        private void CloseAllFiles()
        {
            if (!CheckTabAvailable()) { return; }

            // loop and save
            while(VEditTabs.Count > 0)
            {
                // get the last tab
                REditTab et = VEditTabs[VEditTabs.Count - 1];
                et.Focus();
                CloseKernel(et);
            }
        }

        /// <summary>
        /// gets selected file and delete
        /// </summary>
        [RelayCommand]
        private void DeleteFile()
        {
            // message box to confirm the action
            MessageBoxResult msgResult = 
                MessageBox.Show("Really want to delete selected file?", "msg", MessageBoxButton.YesNoCancel);
            // cases
            switch (msgResult)
            {
                case MessageBoxResult.Yes:
                    {
                        // gets selected VFile
                        VFileItem f = SelectedVFileItem;
                        // if open, try to close the tab first
                        FileInfo fileInfo = new (f.FullName);
                        if (CheckFileAlreadyOpen(fileInfo)) 
                        {
                            for(int i = 0; i < VEditTabs.Count; i++)
                            {
                                // get the last tab
                                REditTab et = VEditTabs[i];
                                if(et.CodeFileInfo.FullName == f.FullName)
                                {
                                    CloseKernel(et);
                                    break;
                                }
                            }
                        }
                        // delete the selected VFile
                        f.Delete();
                        AppFiles.Remove(f);
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

        [RelayCommand]
        private void TrvDeleteFile()
        {
            // message box to confirm the action
            MessageBoxResult msgResult =
                MessageBox.Show("Really want to delete selected file?", "msg", MessageBoxButton.YesNoCancel);
            // cases
            switch (msgResult)
            {
                case MessageBoxResult.Yes:
                    {
                        // gets selected VFile
                        VTreeItem f = SelectedVTreeItem;
                        // if open, try to close the tab first
                        FileInfo fileInfo = new(f.FullName);
                        if (CheckFileAlreadyOpen(fileInfo))
                        {
                            for (int i = 0; i < VEditTabs.Count; i++)
                            {
                                // get the last tab
                                REditTab et = VEditTabs[i];
                                if (et.CodeFileInfo.FullName == f.FullName)
                                {
                                    CloseKernel(et);
                                    break;
                                }
                            }
                        }
                        // delete the selected VFile
                        f.Delete();
                        AppTree.Items.Remove(f);
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

        #endregion
        #region ===== exit window =====

        /// <summary>
        /// checks whether the app can be closed safely
        /// i.e. are there still unsaved files?
        /// </summary>
        /// <returns> true: can exit safely; false: not safe to exit </returns>
        internal bool CanSafelyExitApp()
        {
            // check if all files are saved
            bool allSaved = true;
            for (int i = 0; i < VEditTabs.Count; i++)
            {
                // get current tab
                REditTab t = VEditTabs[i];
                // check if current content modified
                bool isModified = t.Editor.IsModified;
                if (isModified)
                    allSaved = false;
            }
            return allSaved;
        }

        /// <summary>
        /// exists window
        /// </summary>
        [RelayCommand]
        private void ExitApp()
        {
            if (CanSafelyExitApp())
                Environment.Exit(0);
            else
            {
                MessageBoxResult msgResult = MessageBox.Show("Changes are not saved. Do you want to save the changes?",
                    msgBoxTitle, MessageBoxButton.YesNoCancel);
                switch (msgResult)
                {
                    case MessageBoxResult.Yes:
                        {
                            SaveAllFiles();
                            break;
                        }
                    case MessageBoxResult.No:
                        {
                            Environment.Exit(0);
                            break;
                        }
                    case MessageBoxResult.Cancel:
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #endregion
        #region ------- code -------

        #region ===== search and replace =====

        /// <summary>
        /// find content in the code
        /// </summary>
        [RelayCommand]
        internal void Find()
        {
            if(!CheckTabAvailable()){ return; }

            // get the current tab
            REditTab et = VEditTabs[SelectedTabIndex];
            // show search panel
            et.ShowFindPanel();
        }

        /// <summary>
        /// replaces content in the code
        /// </summary>
        [RelayCommand]
        internal void Replace()
        {
            if(!CheckTabAvailable()) { return; }

            // get the current tab
            REditTab et = VEditTabs[SelectedTabIndex];
            // show replace panel
            et.ShowReplacePanel();
        }

        /// <summary>
        /// Method for file list search, case insensitive
        /// </summary>
        [RelayCommand]
        private void SearchFile()
        {
            //清空列表
            AppFiles.Clear();
            //重置列表
            InitiateVFiles();
            bool Capital = SearchModel.IsCapital;
            if (Capital)
            {
                //不区分大小写搜索
                AppFiles = new ObservableCollection<VFileItem>(AppFiles.Where(x => x.DisplayName.ToLower().Contains(SearchModel.SearchName.ToLower().Trim())));
            }
            else
            {
                //区分大小写搜索
                AppFiles = new ObservableCollection<VFileItem>(AppFiles.Where(x => x.DisplayName.Contains(SearchModel.SearchName.Trim())));
            }
        }

        [RelayCommand]
        public void SearchCapitalBtn()
        {
            if (SearchModel.IsCapital)
            {
                SearchModel.IsCapital = false;
            }
            else
            {
                SearchModel.IsCapital = true;
            }
            SearchFile();
        }

        #endregion
        #region ===== comment lines =====

        /// <summary>
        /// gets the active document info
        /// </summary>
        /// <returns> (REditTab, Document?) </returns>
        private (REditTab, Document?) GetDocumentInfo()
        {
            if (!CheckTabAvailable()) { return (null, null); }
            // get the current tab
            REditTab et = VEditTabs[SelectedTabIndex];
            // get the document
            Document? doc = RoslynHost.GetDocument(et.DocId);
            // return
            return (et, doc);
        }

        /// <summary>
        /// gets the actuve document and selection info
        /// </summary>
        /// <returns> (REditTab, Document, SourceText?, TextLine[]) </returns>
        private (REditTab, Document?, SourceText?, TextLine[]) GetDocSelectionInfo()
        {
            (REditTab et, Document? doc) = GetDocumentInfo();

            SourceText? sourceText = null;
            doc?.TryGetText(out sourceText); //= await doc.GetTextAsync().ConfigureAwait(false);
            // get all lines
            TextLineCollection allLines = sourceText.Lines;
            // define the selection
            TextSpan selection = new(start: et.Editor.SelectionStart, 
                length: et.Editor.SelectionLength);
            // find selected lines
            TextLine[] selectedLines = allLines
                .SkipWhile(x => !x.Span.IntersectsWith(selection))
                .TakeWhile(x => x.Span.IntersectsWith(selection)).ToArray();

            return (et, doc, sourceText, selectedLines);
        }


        /// <summary>
        /// comments the selected lines
        /// </summary>
        [RelayCommand]
        internal void CommentLines()
        {
            (REditTab et, Document? doc, SourceText? sourceText, 
                TextLine[] selectedLines) = GetDocSelectionInfo();

            // define changes
            List<TextChange> changes = new();
            foreach (TextLine line in selectedLines)
            {
                if (!string.IsNullOrWhiteSpace(sourceText?.GetSubText(line.Span).ToString()))
                {
                    changes.Add(new TextChange(new TextSpan(line.Start, 0), "//"));
                }
            }

            if (changes.Count == 0) { return; }

            Document? docUpdated = doc?.WithText(sourceText.WithChanges(changes));
            RoslynHost.UpdateDocument(docUpdated);
        }

        /// <summary>
        /// command: comment lines
        /// </summary>
        /*public ICommand CommentLinesCommand
            => new RelayCommand(CommentSelectedLines);*/


        /// <summary>
        /// uncomments the selected lines
        /// </summary>
        [RelayCommand]
        internal void UncommentLines()
        {
            (REditTab et, Document? doc, SourceText? sourceText,
                TextLine[] selectedLines) = GetDocSelectionInfo();

            // define changes
            List<TextChange> changes = new();
            foreach (TextLine line in selectedLines)
            {
                string text = sourceText.GetSubText(line.Span).ToString();
                if (text.TrimStart().StartsWith("//", StringComparison.Ordinal))
                {
                    changes.Add(new TextChange(new TextSpan(
                        line.Start + text.IndexOf("//", StringComparison.Ordinal),
                        "//".Length), string.Empty));
                }
            }

            if (changes.Count == 0) { return; }

            Document? docUpdated = doc?.WithText(sourceText.WithChanges(changes));
            RoslynHost.UpdateDocument(docUpdated);
        }

        /// <summary>
        /// command: uncomment lines
        /// </summary>
        /*public ICommand UncommentLinesCommand
            => new RelayCommand(UncommentSelectedLines);*/

        #endregion
        #region ===== format =====

        /// <summary>
        /// formats the codes automatically
        /// </summary>
        [RelayCommand]
        internal async void FormatCode()
        {
            if (!CheckTabAvailable()) { return; }

            // get the current tab
            REditTab et = VEditTabs[SelectedTabIndex];
            // get document from current tab
            Document? doc = RoslynHost.GetDocument(et.DocId);
            // update
            Document? docUpdated = await Microsoft.CodeAnalysis.Formatting.Formatter.FormatAsync(doc!).ConfigureAwait(false);
            RoslynHost.UpdateDocument(docUpdated);
        }

        /// <summary>
        /// command: format code
        /// </summary>
       /* public ICommand FormatCodeCommand 
            => new RelayCommand(FormatCode);*/

        #endregion
        #region ===== add class / method =====

        /// <summary>
        /// adds a class code block into the editor
        /// </summary>
        [RelayCommand]
        private void AddClass() => PrintNotImplementedInfo();

        /// <summary>
        /// adds a method code block into the editor
        /// </summary>
        [RelayCommand]
        private void AddMethod() => PrintNotImplementedInfo();

        #endregion
        #region ===== compile, diagnostic, run =====

        /// <summary>
        /// simple diagnostic for given script
        /// </summary>
        /// <param name="scr"> given script </param>
        /// <returns> diagnostic result </returns>
        private static bool DiagnosticSimple(Script<object> scr)
            => Coding.CheckError(scr, false).Item1;

        /// <summary>
        /// full diagnostic for given script
        /// </summary>
        /// <param name="scr"> given script </param>
        /// <returns> diagnostic result </returns>
        private static bool DiagnosticFull(Script<object> scr)
        {
            (bool hasError, string errorInfo) = Coding.CheckError(scr, true);
            if (hasError) { Printer.Write(errorInfo); }
            return hasError;
        }

        /// <summary>
        /// scripting for the code in a given EditorTab
        /// </summary>
        /// <param name="t"> given VEditTab </param>
        /// <returns> c# script </returns>
        private Script<object> Script(REditTab t)
            => Coding.MakeScript(t.Editor.Text, RoslynHost);

        /// <summary>
        /// diagnostic kernel for given REditTab
        /// </summary>
        /// <param name="et"> input REditTab </param>
        /// <param name="diagnosticFull"> whether perform full or simple diagnostic </param>
        private void DiagnosticKernel(REditTab et, bool diagnosticFull)
        {
            // script
            et.Script = Script(et);
            // diagnostic 
            et.HasCompileError = diagnosticFull ?
                DiagnosticFull(et.Script) : DiagnosticSimple(et.Script);
        }

        /// <summary>
        /// compiles the code in the active editor
        /// => Debug
        /// </summary>
        [RelayCommand]
        internal void CompileDiagnostic()
        {
            if (!CheckTabAvailable()) { return; }

            // logging
            Printer.Logging("VEMS compilation & diagnositc started ...", true, "[Logging]");
            // get the current VEditTab
            REditTab et = VEditTabs[SelectedTabIndex];
            // diagnostic full
            DiagnosticKernel(et, true);

            // result info
            if(et.HasCompileError)
                Printer.Error("Code diagnostic finished with error(s)", true, "[Error]");
            else
                Printer.Success("Code compilation finished successfully.", true, "[Success]");
        }

        /// <summary>
        /// compiles and runs the code in the active tab
        /// </summary>
        [RelayCommand]
        internal async Task Run()
        {
            if (!CheckTabAvailable()) { return; }
            
            // logging
            Printer.Logging("Code compilation & run started ...", true, "[Logging]");
            // get the current VEditTab
            REditTab et = VEditTabs[SelectedTabIndex];
            // diagnostic simple
            DiagnosticKernel(et, false);

            // run?
            if (et.HasCompileError)
                Printer.Error("Error(s) in the code detected.", true, "[Error]");
            else
            {
                if(et.Script != null)
                {
                    await Task.Run(() => et.Script.RunAsync());
                    Printer.Success("Code compilation & run finished.", true, "[Success]");
                }
            }
        }

        /// <summary>
        /// compiles and runs the code in the active tab
        /// in STA mode
        /// </summary>
        [Obsolete("Use CompileRun instead")]
        [RelayCommand]
        public void RunSTA()
        {
            // consistency check: file(s) available?
            if (VEditTabs.Count == 0)
                Printer.Warning("No available file(s) to run", false);
            else
            {
                Printer.Logging("Code compilation & run started ...", true, "[Logging]");
                // get the current VEditTab
                REditTab t = VEditTabs[SelectedTabIndex];
                // script
                t.Script = Script(t);
                // diagnostic simple
                t.HasCompileError = DiagnosticSimple(t.Script);
                // run?
                if (t.HasCompileError)
                    Printer.Error("Error(s) in the code detected.", true, "[Error}");
                else
                {
                    t.Script.RunAsync();
                    Printer.Success("Code compilation & run finished.", true, "[Success]");
                }
            }
        }

        #endregion

        #endregion
        #region ------- output -------

        #region ===== output text =====

        /// <summary>
        /// copies the console text to clipboard
        /// for further uses ...
        /// </summary>
        [RelayCommand]
        private void CopyConsoleText() 
            => Clipboard.SetText(VConsole.Content);

        /// <summary>
        /// clears the console output content
        /// </summary>
        [RelayCommand]
        internal void ClearConsole() => VConsole.Content = "";

        #endregion
        #region ===== output figure =====

        /// <summary>
        /// minimizes the selected VFrame
        /// </summary>
        [RelayCommand]
        private void MinimizeSelectedVFrame()
        {
            VFrame f = AppFrames[SelectedVFrameIndex];
            f.MinimizeWindow();
        }

        /// <summary>
        /// shows up the selected VFrame
        /// </summary>
        [RelayCommand]
        private void ShowUpSelectedVFrame()
        {
            VFrame f = AppFrames[SelectedVFrameIndex];
            f.ShowUpWindow();
        }

        /// <summary>
        /// closes the selected VFrame
        /// and delete from the list
        /// </summary>
        [RelayCommand]
        private void CloseSelectedVFrame()
        {
            VFrame f = AppFrames[SelectedVFrameIndex];
            f.Close();
        }

        /// <summary>
        /// checks if there is at least one VFrame available
        /// </summary>
        /// <returns> true or false </returns>
        private bool CheckVFrameAvailable()
        {
            if (AppFrames.Count <= 0)
            {
                Printer.Warning("No available VEMS|Frame(s) for the selected operation ...", false);
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// minimizes all the VFrame windows
        /// </summary>
        [RelayCommand]
        private void MinimizeAllVFrames()
        {
            if (!CheckVFrameAvailable()) { return; }
            for(int i = AppFrames.Count - 1; i >= 0; i--)
            {
                VFrame f = AppFrames[i];
                f.MinimizeWindow();
            }
        }

        /// <summary>
        /// closes all the VFrame windows
        /// </summary>
        [RelayCommand]
        private void CloseAllVFrames()
        {
            if (!CheckVFrameAvailable()) { return; }
            while(AppFrames.Count > 0)
            {
                VFrame f = AppFrames[AppFrames.Count - 1];
                f.Close();
            }
        }

        #endregion

        #endregion
        #region ------- parallel -------

        /// <summary>
        /// checks available IP addresses on this computer
        /// </summary>
        [RelayCommand]
        private void CheckIPInfo()
        {
            Ips = Parallel.Network.CheckIPs();
            Printer.Write($"On this computer, {Ips.Count} IPs are found: ");
            for (int i = 0; i < Ips.Count; i++)
                Printer.Write($" - IP address [{i}]: {Ips[i]}");
            if(Ip != null)
            {
                Printer.Write($"Currently selected IP: {Ip}");
                Printer.Write($"Currently selected Port: {Port}");
            }
        }

        /// <summary>
        /// enables parallel services by create a socket
        /// with selected IP and port
        /// </summary>
        [RelayCommand]
        private void EnableParallelService()
        {
            Host = Parallel.Network.CreateSocket(Ip.ToString(), Port);
            Printer.Logging($"Enabled parallel services with selected IP and Port ...");
        }

        /// <summary>
        /// lets the socket to start listening and accepting
        /// incoming connections
        /// </summary>
        [RelayCommand]
        private void SetAsWorker()
        {
            // starts ...
            Parallel.Network.StartService(Host);
            Printer.Logging($"Start to accept incoming connections ...");
        }


        [RelayCommand]
        private void StartAsNode()
        {
            IpTarget = IPAddress.Parse("192.168.8.100");
            PortTarget = 2024;
            IPEndPoint ipEndPoint = new(IpTarget, PortTarget);
            Socket node = new(ipEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);
            node.Connect(ipEndPoint);
        }


        [RelayCommand]
        private void HelloTo()
        {
            Host.Connect(IpTarget.ToString(), PortTarget);
            
        }

        // helper methods ...
        public void TestRun()
        {
            PrintAboutInfo();
            //PrintAboutInfoCommand.Execute(null);
        }


        #endregion
        #region ------- help -------

        #region ===== welcome =====

        /// <summary>
        /// prints about information
        /// </summary>
        [RelayCommand]
        private void PrintAboutInfo()
        {
            Printer.Logging($"VEMS stands for Virtual ElectroMagnetic Solutions", false, "[About VEMS]");
            Printer.Logging($"VEMS is developed by the Computational Photonics group, KLAMOS, CAS", false, "[About VEMS]");
        }

        #endregion
        #region ===== contact =====

        /// <summary>
        /// prints contact information
        /// </summary>
        [RelayCommand]
        private void PrintHelpInfo()
        {
            Printer.Logging("Feel free to contact zhangsite@ciomp.ac.cn", false, "[Help]");
        }

        #endregion
        #region ===== contact =====

        /// <summary>
        /// prints contact information
        /// </summary>
        [RelayCommand]
        private void PrintContactInfo()
        {
            Printer.Logging("Contact zhangsite@ciomp.ac.cn to learn more", false, "[Contact]");
        }

        #endregion
        #region ===== update =====

        /// <summary>
        /// opens download page
        /// </summary>
        [RelayCommand]
        private void OpenDownloadPageForVEMS()
        {
            string url = "http://159.226.165.79/published/vems-software/";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Printer.Warning($"Failed to open URL: {ex.Message}");
            }
        }

        #endregion
        #region ===== license =====

        /// <summary>
        /// prints license information
        /// </summary>
        [RelayCommand]
        private void PrintLicenseInfo()
        {
            DateTime d = ((App)Application.Current).ExpireDate;
            Printer.Logging($"Software license is valid for education only", false, "[License]");
            Printer.Logging($"License will expire on {d}", false, "[License]");
        }

        #endregion

        /// <summary>
        /// prints "not implemented" info in the console
        /// </summary>
        private void PrintNotImplementedInfo()
             => Printer.Warning("function not implemented ...", false);

        #endregion
        #region ------- font updating ------- 

        [RelayCommand]
        private void UpdateCodeEditorFontFamily()
            => VEditTabs.FontFamily = Preference.CodeEditorFontFamily;

        [RelayCommand]
        private void UpdateCodeEditorFontSize()
            => VEditTabs.FontSize = Preference.CodeEditorFontSize;

        [RelayCommand]
        private void UpdateConsoleFontFamily()
            => VConsole.FontFamily = Preference.ConsoleFontFamily;

        [RelayCommand]
        private void UpdateConsoleFontSize()
            => VConsole.FontSize = Preference.ConsoleFontSize;

        #endregion

        #region ------- test methods .......

        /*
        public void DeleteVFrame()
        {
            //FrameFiles.RemoveAt(0);
        }
        public void ShowSelectedVFrameIndex()
        {
            MessageBox.Show(SelectedVFrameIndex.ToString());
        }
        public void CheckVFrameInfo()
        {
            Printer.Write($"AppFrame.Count = {AppFrames.Count}");
        }
        public ICommand CheckVFrameInfoCommand
            => new RelayCommand(CheckVFrameInfo);

        public void LoadVFileTest()
        {
            MessageBox.Show(SelectedVFileItem.DisplayName);
        }

        public void ShowSelectedVFile()
        {
            Printer.Write(SelectedVFileItem.DisplayName);
        }
        public ICommand ShowSelectedFileCommand
            => new RelayCommand(ShowSelectedVFile);


        public void DisplayFontInfo()
        {
            MessageBox.Show("Console Font " + VConsole.FontFamily.ToString() +
                "\r\n + Size " + VConsole.FontSize.ToString());
        }

        public void AddTextToConsole() => VConsole.Content += "\r ... additional information added ...";
        public void EditorFontInfo()
        {
            MessageBox.Show("Editor FFamily = " + VEditTabs[0].Editor.FontFamily.ToString());
        }

        public void EditorFontSizeChange()
            => VEditTabs[0].Editor.FontSize = VEditTabs.FontSize; // EditorFont.SizeValue;

        public void FileInfoTest()
        {
            string fileName = SampleDirectory + @"\MySpace_v2.cs";
            FileInfo fileInfo = new(fileName);
            string d = fileInfo.Directory.FullName;
            string t = new StreamReader(fileInfo.FullName).ReadToEnd();

        }

        public void PrintSelectedTabIndex()
        {
            Console.WriteLine("Selected Tab Index = " + SelectedTabIndex.ToString());
        }

        public void PrintRoslynHostRefs()
        {
            var Refs = RoslynHost.DefaultReferences.ToList();
            for (int i = 0; i < RoslynHost.DefaultReferences.Length; i++)
            {
                var iRef = Refs[i];
                Console.WriteLine("[" + i.ToString() + "] = " + iRef);
            }
        }
        */

        #endregion

        #endregion

    }
}