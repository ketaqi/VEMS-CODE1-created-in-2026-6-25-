using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using RoslynPad.Editor;
using RoslynPad.Roslyn;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;

namespace VEMS.WorkBench
{
    /// <summary>
    /// extended class VEditTab based on TabItem
    /// combined with RoslynEditor
    /// </summary>
    public class REditTab : ClosableTab
    {
        #region default fields

        private readonly string DefaultFontFamily = "Cascadia Code";
        private readonly double DefaultFontSize = 18;
        private readonly string DefaultHeader = "New VEMS Code";

        #endregion
        #region properties

        public RoslynHost Host { get; set; }
        public RoslynCodeEditor Editor { get; set; }
        public FileInfo CodeFileInfo { get; set; }
        public DocumentId DocId { get; set; }
        public Script<object>? Script { get; set; }
        public Compilation? Compilation { get; set; }
        public Diagnostic? Diagnostic { get; set; }
        public SearchReplacePanel FnRPanel { get; set; }
        public bool HasCompileError { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a new REditTab with specific host,
        /// working directory and source code text
        /// </summary>
        /// <param name="host"> roslyn host </param>
        /// <param name="workingDirectory"> working directory </param>
        /// <param name="desireFileName"> desire file name in working directory </param>
        /// <param name="codeText"> code text </param>
        public REditTab(RoslynHost host,
            DirectoryInfo workingDirectory,
            string desireFileName = "",
            string codeText = "")
        {
            // sets RoslynHost
            Host = host?? new RoslynHost();

            // creates RoslynCodeEditor
            Editor = new RoslynCodeEditor
            {
                FontFamily = new FontFamily(DefaultFontFamily),
                FontSize = DefaultFontSize,
            };
            DocId = Editor.Initialize(roslynHost: Host, 
                highlightColors: new ClassificationHighlightColors(),
                workingDirectory: workingDirectory.FullName, 
                documentText: codeText);
            Editor.IsModified = false;
            // install search and replace panel
            FnRPanel = SearchReplacePanel.Install(Editor);
            // sets up TabItem
            Content = Editor;
            if (desireFileName != null && desireFileName != "")
                Title = desireFileName;
            else
                Title = DefaultHeader; 
            // define file info
            string fileFullName = workingDirectory?.FullName +
                @"\" + this.Title;
            CodeFileInfo = new(fileFullName);
        }


        /// <summary>
        /// constructs a VEditTab
        /// with given RoslynHost and
        /// given working directory 
        /// and source code text
        /// </summary>
        /// <param name="host"> roslyn host </param>
        /// <param name="fontFamily"> font family of the editor </param>
        /// <param name="fontSize"> font size of the editor </param>
        /// <param name="workingDirectory"> working directory </param>
        /// <param name="codeText"> code text </param>
        [Obsolete]
        public REditTab(RoslynHost host,
            FontFamily fontFamily,
            double fontSize,
            string workingDirectory,
            string codeText)
        {
            Host = host;
            Editor = new RoslynCodeEditor
            {
                FontFamily = fontFamily,
                FontSize = fontSize,
            };
            DocId = Editor.Initialize(Host,
                new ClassificationHighlightColors(),
                workingDirectory,
                codeText);
            Editor.IsModified = false;
            base.Content = Editor;
            base.Header = "New VEMS Code";
            CodeFileInfo = new(Path.Combine(workingDirectory, Header.ToString()));
        }

        /// <summary>
        /// constructs a VEditTab
        /// with given RoslynHost and
        /// desire code file info
        /// and source code file info
        /// </summary>
        /// <param name="host"> roslyn host </param>
        /// <param name="fontFamily"> font family of the editor </param>
        /// <param name="fontSize"> font size of the editor </param>
        /// <param name="desireCodeFileInfo"> desire code file info </param>
        /// <param name="sourceCodeFileInfo"> source code file info </param>
        [Obsolete]
        public REditTab(RoslynHost host,
            FontFamily fontFamily,
            double fontSize,
            FileInfo desireCodeFileInfo,
            FileInfo sourceCodeFileInfo)
        {
            Host = host;
            Editor = new RoslynCodeEditor
            {
                FontFamily = fontFamily,
                FontSize = fontSize,
            };
            DocId = Editor.Initialize(Host,
                new ClassificationHighlightColors(),
                desireCodeFileInfo.Directory.FullName,
                new StreamReader(sourceCodeFileInfo.FullName).ReadToEnd());
            Editor.IsModified = false;
            base.Content = Editor;
            base.Header = desireCodeFileInfo.Name;
            CodeFileInfo = desireCodeFileInfo;
        }

        #endregion
        #region methods

        /// <summary>
        /// save the code file
        /// </summary>
        /// <param name="desireFileInfo"> desired saving FileInfo </param>
        public void Save(FileInfo? desireFileInfo = null)
        {
            // check and update CodeFileInfo
            if (desireFileInfo != null && desireFileInfo != CodeFileInfo)
                CodeFileInfo = desireFileInfo;
            // save
            Editor.Save(CodeFileInfo.FullName);
            // update
            Editor.IsModified = false;
            Title = CodeFileInfo.Name;
        }

        /// <summary>
        /// shows up the find panel
        /// </summary>
        public void ShowFindPanel()
        {
            FnRPanel.IsReplaceMode = false;
            FnRPanel.Open();
        }

        /// <summary>
        /// shows up the replace panel
        /// </summary>
        public void ShowReplacePanel()
        {
            FnRPanel.IsReplaceMode = true;
            FnRPanel.Open();
        }

        #endregion
    }

    // ...
    internal class REdit
    {
        private RoslynHost Host { get; set; }
        private RoslynCodeEditor Editor { get; set; }


        private REdit(RoslynHost host, string workDir, string code)
        {
            Host = host;
            // creates RoslynCodeEditor
            Editor = new();
            Editor.Initialize(Host, null, workDir, code);
        }

        private void Run()
        {
            Script scr = CSharpScript.Create(Editor.Text,
                ScriptOptions.Default.WithReferences(Host.DefaultReferences).WithImports(Host.DefaultImports));
            scr.RunAsync();
        }

    }

    /// <summary>
    /// list of REditTabs
    /// </summary>
    public class REditTabList : ObservableCollection<REditTab> 
    {
        private FontFamily fontFamily { get; set; }
        private double fontSize { get; set; }

        /// <summary>
        /// font family of all the REditorTabs
        /// </summary>
        public FontFamily FontFamily
        {
            get => fontFamily;
            set
            {
                fontFamily = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(FontFamily)));
                for (int i = 0; i < Count; i++)
                { this[i].Editor.FontFamily = FontFamily; }
            }
        }

        /// <summary>
        /// font size of all the REditorTabs
        /// </summary>
        public double FontSize
        {
            get => fontSize;
            set
            {
                fontSize = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(FontSize)));
                for (int i = 0; i < this.Count; i++)
                { this[i].Editor.FontSize = FontSize; }
            }
        }

        /// <summary>
        /// default constructor
        /// </summary>
        public REditTabList() 
        {
            fontFamily = new FontFamily(WorkBench.Defaults.CodeEditorFontFamily);
            fontSize = WorkBench.Defaults.CodeEditorFontSize;
        }
    }

    /// <summary>
    /// a tab item supporting a close button
    /// based on the CloseableHeader
    /// </summary>
    public class ClosableTab : TabItem
    {
        /// <summary>
        /// the closable header with a close button
        /// </summary>
        public CloseableHeader ClosableHeader { get; set; }

        /// <summary>
        /// expose HeaderLabel 
        /// </summary>
        public string Title
        {
            get => ClosableHeader.HeaderLabel.Content.ToString()?? ""; 
            set => ClosableHeader.HeaderLabel.Content = value; 
        }

        /// <summary>
        /// constructs a ClosableTab
        /// </summary>
        public ClosableTab()
        {
            ClosableHeader = new CloseableHeader();
            Header = ClosableHeader;
            // when CloseButton is clicked, 
            // the tab becomes selected/focused
            ClosableHeader.CloseButton.Click +=
                (o, e) => IsSelected = true;
        }

    }

}
