using System.Collections.ObjectModel;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using Avalonia.Media;
using RoslynPad.Models;

namespace RoslynPad.ViewModels;

/// <summary>
/// 快捷键配置视图模型：管理所有可配置的键盘快捷键。
/// </summary>
/// <remarks>
/// 职责：
/// <list type="bullet">
///   <item>存储和管理所有菜单/功能的快捷键配置</item>
///   <item>提供快捷键的加载、保存、导入、导出功能</item>
///   <item>生成带快捷键的菜单提示文本（Tip 属性）</item>
/// </list>
/// </remarks>
public class ShortcutSettingsViewModel : NotificationObject
{
    private readonly LocalizationManager _localized;
    private readonly Action? _onShortcutChanged;

    /// <summary>
    /// 初始化快捷键配置视图模型。
    /// </summary>
    /// <param name="localized">本地化管理器，用于生成带翻译的提示文本。</param>
    /// <param name="onShortcutChanged">快捷键变更时的回调（可选）。</param>
    public ShortcutSettingsViewModel(LocalizationManager localized, Action? onShortcutChanged = null)
    {
        _localized = localized;
        _onShortcutChanged = onShortcutChanged;
    }

    #region JSON 序列化选项

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    #endregion

    #region 文件菜单快捷键

    private string _fileMenu = "Ctrl+F";
    public string FileMenu
    {
        get => _fileMenu;
        set => SetShortcut(ref _fileMenu, value, nameof(FileMenu), nameof(FileMenuTip));
    }
    public string FileMenuTip => $"{_localized["MainWindow.MenuItem1"]} ({FileMenu})";

    private string _newFileShortcut = "Ctrl+N";
    public string NewFileShortcut
    {
        get => _newFileShortcut;
        set => SetShortcut(ref _newFileShortcut, value, nameof(NewFileShortcut), nameof(NewFileTip));
    }
    public string NewFileTip => $"{_localized["MainWindow.MenuItem111"]} ({NewFileShortcut})";

    private string _openFileShortcut = "Ctrl+O";
    public string OpenFileShortcut
    {
        get => _openFileShortcut;
        set => SetShortcut(ref _openFileShortcut, value, nameof(OpenFileShortcut), nameof(OpenFileTip));
    }
    public string OpenFileTip => $"{_localized["MainWindow.MenuItem112"]} ({OpenFileShortcut})";

    private string _openFolderShortcut = "Ctrl+Shift+O";
    public string OpenFolderShortcut
    {
        get => _openFolderShortcut;
        set => SetShortcut(ref _openFolderShortcut, value, nameof(OpenFolderShortcut), nameof(OpenFolderTip));
    }
    public string OpenFolderTip => $"{_localized["MainWindow.MenuItem121"]} ({OpenFolderShortcut})";

    private string _defaultFolderShortcut = "Ctrl+D";
    public string DefaultFolderShortcut
    {
        get => _defaultFolderShortcut;
        set => SetShortcut(ref _defaultFolderShortcut, value, nameof(DefaultFolderShortcut), nameof(DefaultFolderTip));
    }
    public string DefaultFolderTip => $"{_localized["MainWindow.MenuItem122"]} ({DefaultFolderShortcut})";

    private string _saveFileShortcut = "Ctrl+S";
    public string SaveFileShortcut
    {
        get => _saveFileShortcut;
        set => SetShortcut(ref _saveFileShortcut, value, nameof(SaveFileShortcut), nameof(SaveFileTip));
    }
    public string SaveFileTip => $"{_localized["MainWindow.MenuItem131"]} ({SaveFileShortcut})";

    private string _saveAsShortcut = "Ctrl+Shift+S";
    public string SaveAsShortcut
    {
        get => _saveAsShortcut;
        set => SetShortcut(ref _saveAsShortcut, value, nameof(SaveAsShortcut), nameof(SaveAsTip));
    }
    public string SaveAsTip => $"{_localized["MainWindow.MenuItem132"]} ({SaveAsShortcut})";

    private string _saveAllShortcut = "Ctrl+Alt+S";
    public string SaveAllShortcut
    {
        get => _saveAllShortcut;
        set => SetShortcut(ref _saveAllShortcut, value, nameof(SaveAllShortcut), nameof(SaveAllTip));
    }
    public string SaveAllTip => $"{_localized["MainWindow.MenuItem133"]} ({SaveAllShortcut})";

    private string _closeTabShortcut = "Ctrl+W";
    public string CloseTabShortcut
    {
        get => _closeTabShortcut;
        set => SetShortcut(ref _closeTabShortcut, value, nameof(CloseTabShortcut), nameof(CloseTabTip));
    }
    public string CloseTabTip => $"{_localized["MainWindow.MenuItem141"]} ({CloseTabShortcut})";

    private string _closeAllShortcut = "Ctrl+Shift+W";
    public string CloseAllShortcut
    {
        get => _closeAllShortcut;
        set => SetShortcut(ref _closeAllShortcut, value, nameof(CloseAllShortcut), nameof(CloseAllTip));
    }
    public string CloseAllTip => $"{_localized["MainWindow.MenuItem142"]} ({CloseAllShortcut})";

    private string _exitAppShortcut = "Alt+F4";
    public string ExitAppShortcut
    {
        get => _exitAppShortcut;
        set => SetShortcut(ref _exitAppShortcut, value, nameof(ExitAppShortcut), nameof(ExitAppTip));
    }
    public string ExitAppTip => $"{_localized["MainWindow.MenuItem143"]} ({ExitAppShortcut})";

    #endregion

    #region 代码菜单快捷键

    private string _codeMenu = "Ctrl+E";
    public string CodeMenu
    {
        get => _codeMenu;
        set => SetShortcut(ref _codeMenu, value, nameof(CodeMenu), nameof(CodeMenuTip));
    }
    public string CodeMenuTip => $"{_localized["MainWindow.MenuItem2"]} ({CodeMenu})";

    private string _searchTextShortcut = "Ctrl+F";
    public string SearchTextShortcut
    {
        get => _searchTextShortcut;
        set => SetShortcut(ref _searchTextShortcut, value, nameof(SearchTextShortcut), nameof(SearchTextTip));
    }
    public string SearchTextTip => $"{_localized["MainWindow.MenuItem211"]} ({SearchTextShortcut})";

    private string _replaceTextShortcut = "Ctrl+H";
    public string ReplaceTextShortcut
    {
        get => _replaceTextShortcut;
        set => SetShortcut(ref _replaceTextShortcut, value, nameof(ReplaceTextShortcut), nameof(ReplaceTextTip));
    }
    public string ReplaceTextTip => $"{_localized["MainWindow.MenuItem212"]} ({ReplaceTextShortcut})";

    private string _formatCodeShortcut = "Ctrl+D";
    public string FormatCodeShortcut
    {
        get => _formatCodeShortcut;
        set => SetShortcut(ref _formatCodeShortcut, value, nameof(FormatCodeShortcut), nameof(FormatCodeTip));
    }
    public string FormatCodeTip => $"{_localized["MainWindow.MenuItem221"]} ({FormatCodeShortcut})";

    private string _commentSelectionShortcut = "Ctrl+C";
    public string CommentSelectionShortcut
    {
        get => _commentSelectionShortcut;
        set => SetShortcut(ref _commentSelectionShortcut, value, nameof(CommentSelectionShortcut), nameof(CommentSelectionTip));
    }
    public string CommentSelectionTip => $"{_localized["MainWindow.MenuItem231"]} ({CommentSelectionShortcut})";

    private string _uncommentSelectionShortcut = "Ctrl+U";
    public string UncommentSelectionShortcut
    {
        get => _uncommentSelectionShortcut;
        set => SetShortcut(ref _uncommentSelectionShortcut, value, nameof(UncommentSelectionShortcut), nameof(UncommentSelectionTip));
    }
    public string UncommentSelectionTip => $"{_localized["MainWindow.MenuItem232"]} ({UncommentSelectionShortcut})";

    private string _debugCodeShortcut = "F5";
    public string DebugCodeShortcut
    {
        get => _debugCodeShortcut;
        set => SetShortcut(ref _debugCodeShortcut, value, nameof(DebugCodeShortcut), nameof(DebugCodeTip));
    }
    public string DebugCodeTip => $"{_localized["MainWindow.MenuItem241"]} ({DebugCodeShortcut})";

    private string _runCodeShortcut = "Ctrl+F5";
    public string RunCodeShortcut
    {
        get => _runCodeShortcut;
        set => SetShortcut(ref _runCodeShortcut, value, nameof(RunCodeShortcut), nameof(RunCodeTip));
    }
    public string RunCodeTip => $"{_localized["MainWindow.MenuItem242"]} ({RunCodeShortcut})";

    private string _addClassShortcut = "Ctrl+Shift+A";
    public string AddClassShortcut
    {
        get => _addClassShortcut;
        set => SetShortcut(ref _addClassShortcut, value, nameof(AddClassShortcut));
    }

    private string _addMethodShortcut = "Ctrl+Shift+M";
    public string AddMethodShortcut
    {
        get => _addMethodShortcut;
        set => SetShortcut(ref _addMethodShortcut, value, nameof(AddMethodShortcut));
    }

    #endregion

    #region 输出菜单快捷键

    private string _outputMenu = "Ctrl+O";
    public string OutputMenu
    {
        get => _outputMenu;
        set => SetShortcut(ref _outputMenu, value, nameof(OutputMenu), nameof(OutputMenuTip));
    }
    public string OutputMenuTip => $"{_localized["MainWindow.MenuItem3"]} ({OutputMenu})";

    private string _copyMessageShortcut = "Ctrl+C";
    public string CopyMessageShortcut
    {
        get => _copyMessageShortcut;
        set => SetShortcut(ref _copyMessageShortcut, value, nameof(CopyMessageShortcut), nameof(CopyMessageTip));
    }
    public string CopyMessageTip => $"{_localized["MainWindow.MenuItem311"]} ({CopyMessageShortcut})";

    private string _clearMessageShortcut = "Ctrl+L";
    public string ClearMessageShortcut
    {
        get => _clearMessageShortcut;
        set => SetShortcut(ref _clearMessageShortcut, value, nameof(ClearMessageShortcut), nameof(ClearMessageTip));
    }
    public string ClearMessageTip => $"{_localized["MainWindow.MenuItem312"]} ({ClearMessageShortcut})";

    private string _allFiguresMinimizedShortcut = "Ctrl+M";
    public string AllFiguresMinimizedShortcut
    {
        get => _allFiguresMinimizedShortcut;
        set => SetShortcut(ref _allFiguresMinimizedShortcut, value, nameof(AllFiguresMinimizedShortcut), nameof(AllFiguresMinimizedTip));
    }
    public string AllFiguresMinimizedTip => $"{_localized["MainWindow.MenuItem321"]} ({AllFiguresMinimizedShortcut})";

    private string _closeAllFiguresShortcut = "Ctrl+Shift+M";
    public string CloseAllFiguresShortcut
    {
        get => _closeAllFiguresShortcut;
        set => SetShortcut(ref _closeAllFiguresShortcut, value, nameof(CloseAllFiguresShortcut), nameof(CloseAllFiguresTip));
    }
    public string CloseAllFiguresTip => $"{_localized["MainWindow.MenuItem322"]} ({CloseAllFiguresShortcut})";

    #endregion

    #region 帮助菜单快捷键

    private string _helpMenu = "F1";
    public string HelpMenu
    {
        get => _helpMenu;
        set => SetShortcut(ref _helpMenu, value, nameof(HelpMenu), nameof(HelpMenuTip));
    }
    public string HelpMenuTip => $"{_localized["MainWindow.MenuItem4"]} ({HelpMenu})";

    private string _getStartedShortcut = "Ctrl+G";
    public string GetStartedShortcut
    {
        get => _getStartedShortcut;
        set => SetShortcut(ref _getStartedShortcut, value, nameof(GetStartedShortcut), nameof(GetStartedTip));
    }
    public string GetStartedTip => $"{_localized["MainWindow.MenuItem411"]} ({GetStartedShortcut})";

    private string _vemsDocsShortcut = "Ctrl+Alt+G";
    public string VEMSDocsShortcut
    {
        get => _vemsDocsShortcut;
        set => SetShortcut(ref _vemsDocsShortcut, value, nameof(VEMSDocsShortcut), nameof(VEMSDocsTip));
    }
    public string VEMSDocsTip => $"{_localized["MainWindow.MenuItem412"]} ({VEMSDocsShortcut})";

    private string _linksShortcut = "Ctrl+Shift+G";
    public string LinksShortcut
    {
        get => _linksShortcut;
        set => SetShortcut(ref _linksShortcut, value, nameof(LinksShortcut), nameof(LinksTip));
    }
    public string LinksTip => $"{_localized["MainWindow.MenuItem413"]} ({LinksShortcut})";

    private string _helpShortcut = "Ctrl+Shift+H";
    public string HelpShortcut
    {
        get => _helpShortcut;
        set => SetShortcut(ref _helpShortcut, value, nameof(HelpShortcut), nameof(HelpTip));
    }
    public string HelpTip => $"{_localized["MainWindow.MenuItem414"]} ({HelpShortcut})";

    private string _licenseInfoShortcut = "Ctrl+I";
    public string LicenseInfoShortcut
    {
        get => _licenseInfoShortcut;
        set => SetShortcut(ref _licenseInfoShortcut, value, nameof(LicenseInfoShortcut), nameof(LicenseInfoTip));
    }
    public string LicenseInfoTip => $"{_localized["MainWindow.MenuItem421"]} ({LicenseInfoShortcut})";

    private string _updateAppShortcut = "Ctrl+U";
    public string UpdateAppShortcut
    {
        get => _updateAppShortcut;
        set => SetShortcut(ref _updateAppShortcut, value, nameof(UpdateAppShortcut), nameof(UpdateAppTip));
    }
    public string UpdateAppTip => $"{_localized["MainWindow.MenuItem422"]} ({UpdateAppShortcut})";

    private string _aboutVEMSShortcut = "Ctrl+A";
    public string AboutVEMSShortcut
    {
        get => _aboutVEMSShortcut;
        set => SetShortcut(ref _aboutVEMSShortcut, value, nameof(AboutVEMSShortcut), nameof(AboutVEMSTip));
    }
    public string AboutVEMSTip => $"{_localized["MainWindow.MenuItem431"]} ({AboutVEMSShortcut})";

    private string _contactUSShortcut = "Ctrl+Shift+A";
    public string ContactUSShortcut
    {
        get => _contactUSShortcut;
        set => SetShortcut(ref _contactUSShortcut, value, nameof(ContactUSShortcut), nameof(ContactUSTip));
    }
    public string ContactUSTip => $"{_localized["MainWindow.MenuItem432"]} ({ContactUSShortcut})";

    private string _joinShortcut = "Ctrl+J";
    public string JoinShortcut
    {
        get => _joinShortcut;
        set => SetShortcut(ref _joinShortcut, value, nameof(JoinShortcut), nameof(JoinShortcutTip));
    }
    public string JoinShortcutTip => $"{_localized["MainWindow.MenuItem433"]} ({JoinShortcut})";

    #endregion

    #region 左侧活动栏快捷键

    private string _openExplorerShortcut = "Ctrl+Alt+E";
    public string OpenExplorerShortcut
    {
        get => _openExplorerShortcut;
        set => SetShortcut(ref _openExplorerShortcut, value, nameof(OpenExplorerShortcut), nameof(OpenExplorerTip));
    }
    public string OpenExplorerTip => $"{_localized["MainWindow.leftMenuItem1"]} ({OpenExplorerShortcut})";

    private string _openSearchShortcut = "Ctrl+Alt+F";
    public string OpenSearchShortcut
    {
        get => _openSearchShortcut;
        set => SetShortcut(ref _openSearchShortcut, value, nameof(OpenSearchShortcut), nameof(OpenSearchTip));
    }
    public string OpenSearchTip => $"{_localized["MainWindow.leftMenuItem5"]} ({OpenSearchShortcut})";

    private string _openSourceControlShortcut = "Ctrl+Alt+S";
    public string OpenSourceControlShortcut
    {
        get => _openSourceControlShortcut;
        set => SetShortcut(ref _openSourceControlShortcut, value, nameof(OpenSourceControlShortcut), nameof(OpenSourceControlTip));
    }
    public string OpenSourceControlTip => $"{_localized["MainWindow.leftMenuItem6"]} ({OpenSourceControlShortcut})";

    private string _openRunDebugShortcut = "Ctrl+Alt+R";
    public string OpenRunDebugShortcut
    {
        get => _openRunDebugShortcut;
        set => SetShortcut(ref _openRunDebugShortcut, value, nameof(OpenRunDebugShortcut), nameof(OpenRunDebugTip));
    }
    public string OpenRunDebugTip => $"{_localized["MainWindow.leftMenuItem7"]} ({OpenRunDebugShortcut})";

    private string _openUserPreferencesShortcut = "Ctrl+Alt+U";
    public string OpenUserPreferencesShortcut
    {
        get => _openUserPreferencesShortcut;
        set => SetShortcut(ref _openUserPreferencesShortcut, value, nameof(OpenUserPreferencesShortcut), nameof(OpenUserPreferencesTip));
    }
    public string OpenUserPreferencesTip => $"{_localized["MainWindow.leftMenuItem2"]} ({OpenUserPreferencesShortcut})";

    private string _openUserPreformanceShortcut = "Ctrl+Alt+P";
    public string OpenUserPreformanceShortcut
    {
        get => _openUserPreformanceShortcut;
        set => SetShortcut(ref _openUserPreformanceShortcut, value, nameof(OpenUserPreformanceShortcut), nameof(OpenUserPreformanceTip));
    }
    public string OpenUserPreformanceTip => $"{_localized["MainWindow.leftMenuItem3"]} ({OpenUserPreformanceShortcut})";

    private string _openThemeShortcut = "Ctrl+Alt+T";
    public string OpenThemeShortcut
    {
        get => _openThemeShortcut;
        set => SetShortcut(ref _openThemeShortcut, value, nameof(OpenThemeShortcut), nameof(OpenThemeTip));
    }
    public string OpenThemeTip => $"{_localized["MainWindow.leftMenuItem7"]} ({OpenThemeShortcut})";

    private string _openLanguageShortcut = "Ctrl+Alt+L";
    public string OpenLanguageShortcut
    {
        get => _openLanguageShortcut;
        set => SetShortcut(ref _openLanguageShortcut, value, nameof(OpenLanguageShortcut), nameof(OpenLanguageTip));
    }
    public string OpenLanguageTip => $"{_localized["MainWindow.leftMenuItem7"]} ({OpenLanguageShortcut})";

    #endregion

    #region 通用快捷键设置方法

    /// <summary>
    /// 设置快捷键并触发相关通知。
    /// </summary>
    private void SetShortcut(ref string field, string value, string propertyName, string? tipPropertyName = null)
    {
        if (field == value) return;
        field = value;
        OnPropertyChanged(propertyName);
        if (tipPropertyName != null)
            OnPropertyChanged(tipPropertyName);
        _onShortcutChanged?.Invoke();
    }

    #endregion

    #region 保存/加载

    private const string ShortcutsFileName = "user_shortcuts.json";

    /// <summary>
    /// 将当前快捷键配置保存到文件。
    /// </summary>
    public void Save()
    {
        //var data = CreateShortcutData();
        //var json = JsonSerializer.Serialize(data, s_jsonOptions);
        //File.WriteAllText(ShortcutsFileName, json);
    }

    /// <summary>
    /// 从文件加载快捷键配置。
    /// </summary>
    public void Load()
    {
        if (!File.Exists(ShortcutsFileName)) return;

        try
        {
            var json = File.ReadAllText(ShortcutsFileName);
            var data = JsonSerializer.Deserialize<ShortcutData>(json);
            if (data != null)
                ApplyShortcutData(data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ShortcutSettings.Load] Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出快捷键配置到指定路径。
    /// </summary>
    public void Export(string path)
    {
        //var data = CreateShortcutData();
        //var json = JsonSerializer.Serialize(data, s_jsonOptions);
        //File.WriteAllText(path, json);
    }

    /// <summary>
    /// 从指定路径导入快捷键配置。
    /// </summary>
    public void Import(string path)
    {
        if (!File.Exists(path)) return;

        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<ShortcutData>(json);
        if (data != null)
        {
            ApplyShortcutData(data);
            Save();
        }
    }

    //private ShortcutData CreateShortcutData()
    //{
    //    return new ShortcutData
    //    {
    //        FileMenu = FileMenu,
    //        NewFileShortcut = NewFileShortcut,
    //        OpenFileShortcut = OpenFileShortcut,
    //        OpenFolderShortcut = OpenFolderShortcut,
    //        DefaultFolderShortcut = DefaultFolderShortcut,
    //        SaveFileShortcut = SaveFileShortcut,
    //        SaveAsShortcut = SaveAsShortcut,
    //        SaveAllShortcut = SaveAllShortcut,
    //        CloseTabShortcut = CloseTabShortcut,
    //        CloseAllShortcut = CloseAllShortcut,
    //        ExitAppShortcut = ExitAppShortcut,
    //        CodeMenu = CodeMenu,
    //        SearchTextShortcut = SearchTextShortcut,
    //        ReplaceTextShortcut = ReplaceTextShortcut,
    //        FormatCodeShortcut = FormatCodeShortcut,
    //        CommentSelectionShortcut = CommentSelectionShortcut,
    //        UncommentSelectionShortcut = UncommentSelectionShortcut,
    //        DebugCodeShortcut = DebugCodeShortcut,
    //        RunCodeShortcut = RunCodeShortcut,
    //        AddClassShortcut = AddClassShortcut,
    //        AddMethodShortcut = AddMethodShortcut,
    //        OutputMenu = OutputMenu,
    //        CopyMessageShortcut = CopyMessageShortcut,
    //        ClearMessageShortcut = ClearMessageShortcut,
    //        AllFiguresMinimizedShortcut = AllFiguresMinimizedShortcut,
    //        CloseAllFiguresShortcut = CloseAllFiguresShortcut,
    //        HelpMenu = HelpMenu,
    //        GetStartedShortcut = GetStartedShortcut,
    //        VEMSDocsShortcut = VEMSDocsShortcut,
    //        LinksShortcut = LinksShortcut,
    //        HelpShortcut = HelpShortcut,
    //        LicenseInfoShortcut = LicenseInfoShortcut,
    //        UpdateAppShortcut = UpdateAppShortcut,
    //        AboutVEMSShortcut = AboutVEMSShortcut,
    //        ContactUSShortcut = ContactUSShortcut,
    //        JoinShortcut = JoinShortcut,
    //        OpenExplorerShortcut = OpenExplorerShortcut,
    //        OpenSearchShortcut = OpenSearchShortcut,
    //        OpenSourceControlShortcut = OpenSourceControlShortcut,
    //        OpenRunDebugShortcut = OpenRunDebugShortcut,
    //        OpenUserPreferencesShortcut = OpenUserPreferencesShortcut,
    //        OpenUserPreformanceShortcut = OpenUserPreformanceShortcut,
    //        OpenThemeShortcut = OpenThemeShortcut,
    //        OpenLanguageShortcut = OpenLanguageShortcut,
    //    };
    //}

    private void ApplyShortcutData(ShortcutData data)
    {
        FileMenu = data.FileMenu;
        NewFileShortcut = data.NewFileShortcut;
        OpenFileShortcut = data.OpenFileShortcut;
        OpenFolderShortcut = data.OpenFolderShortcut;
        DefaultFolderShortcut = data.DefaultFolderShortcut;
        SaveFileShortcut = data.SaveFileShortcut;
        SaveAsShortcut = data.SaveAsShortcut;
        SaveAllShortcut = data.SaveAllShortcut;
        CloseTabShortcut = data.CloseTabShortcut;
        CloseAllShortcut = data.CloseAllShortcut;
        ExitAppShortcut = data.ExitAppShortcut;
        CodeMenu = data.CodeMenu;
        SearchTextShortcut = data.SearchTextShortcut;
        ReplaceTextShortcut = data.ReplaceTextShortcut;
        FormatCodeShortcut = data.FormatCodeShortcut;
        CommentSelectionShortcut = data.CommentSelectionShortcut;
        UncommentSelectionShortcut = data.UncommentSelectionShortcut;
        DebugCodeShortcut = data.DebugCodeShortcut;
        RunCodeShortcut = data.RunCodeShortcut;
        AddClassShortcut = data.AddClassShortcut;
        AddMethodShortcut = data.AddMethodShortcut;
        OutputMenu = data.OutputMenu;
        CopyMessageShortcut = data.CopyMessageShortcut;
        ClearMessageShortcut = data.ClearMessageShortcut;
        AllFiguresMinimizedShortcut = data.AllFiguresMinimizedShortcut;
        CloseAllFiguresShortcut = data.CloseAllFiguresShortcut;
        HelpMenu = data.HelpMenu;
        GetStartedShortcut = data.GetStartedShortcut;
        VEMSDocsShortcut = data.VEMSDocsShortcut;
        LinksShortcut = data.LinksShortcut;
        HelpShortcut = data.HelpShortcut;
        LicenseInfoShortcut = data.LicenseInfoShortcut;
        UpdateAppShortcut = data.UpdateAppShortcut;
        AboutVEMSShortcut = data.AboutVEMSShortcut;
        ContactUSShortcut = data.ContactUSShortcut;
        JoinShortcut = data.JoinShortcut;
        OpenExplorerShortcut = data.OpenExplorerShortcut;
        OpenSearchShortcut = data.OpenSearchShortcut;
        OpenSourceControlShortcut = data.OpenSourceControlShortcut;
        OpenRunDebugShortcut = data.OpenRunDebugShortcut;
        OpenUserPreferencesShortcut = data.OpenUserPreferencesShortcut;
        OpenUserPreformanceShortcut = data.OpenUserPreformanceShortcut;
        OpenThemeShortcut = data.OpenThemeShortcut;
        OpenLanguageShortcut = data.OpenLanguageShortcut;
    }

    /// <summary>
    /// 刷新所有 Tip 属性通知（语言切换后调用）。
    /// </summary>
    public void RefreshAllTips()
    {
        OnPropertyChanged(nameof(FileMenuTip));
        OnPropertyChanged(nameof(NewFileTip));
        OnPropertyChanged(nameof(OpenFileTip));
        OnPropertyChanged(nameof(OpenFolderTip));
        OnPropertyChanged(nameof(DefaultFolderTip));
        OnPropertyChanged(nameof(SaveFileTip));
        OnPropertyChanged(nameof(SaveAsTip));
        OnPropertyChanged(nameof(SaveAllTip));
        OnPropertyChanged(nameof(CloseTabTip));
        OnPropertyChanged(nameof(CloseAllTip));
        OnPropertyChanged(nameof(ExitAppTip));
        OnPropertyChanged(nameof(CodeMenuTip));
        OnPropertyChanged(nameof(SearchTextTip));
        OnPropertyChanged(nameof(ReplaceTextTip));
        OnPropertyChanged(nameof(FormatCodeTip));
        OnPropertyChanged(nameof(CommentSelectionTip));
        OnPropertyChanged(nameof(UncommentSelectionTip));
        OnPropertyChanged(nameof(DebugCodeTip));
        OnPropertyChanged(nameof(RunCodeTip));
        OnPropertyChanged(nameof(OutputMenuTip));
        OnPropertyChanged(nameof(CopyMessageTip));
        OnPropertyChanged(nameof(ClearMessageTip));
        OnPropertyChanged(nameof(AllFiguresMinimizedTip));
        OnPropertyChanged(nameof(CloseAllFiguresTip));
        OnPropertyChanged(nameof(HelpMenuTip));
        OnPropertyChanged(nameof(GetStartedTip));
        OnPropertyChanged(nameof(VEMSDocsTip));
        OnPropertyChanged(nameof(LinksTip));
        OnPropertyChanged(nameof(HelpTip));
        OnPropertyChanged(nameof(LicenseInfoTip));
        OnPropertyChanged(nameof(UpdateAppTip));
        OnPropertyChanged(nameof(AboutVEMSTip));
        OnPropertyChanged(nameof(ContactUSTip));
        OnPropertyChanged(nameof(JoinShortcutTip));
        OnPropertyChanged(nameof(OpenExplorerTip));
        OnPropertyChanged(nameof(OpenSearchTip));
        OnPropertyChanged(nameof(OpenSourceControlTip));
        OnPropertyChanged(nameof(OpenRunDebugTip));
        OnPropertyChanged(nameof(OpenUserPreferencesTip));
        OnPropertyChanged(nameof(OpenUserPreformanceTip));
        OnPropertyChanged(nameof(OpenThemeTip));
        OnPropertyChanged(nameof(OpenLanguageTip));
    }

    #endregion
}
