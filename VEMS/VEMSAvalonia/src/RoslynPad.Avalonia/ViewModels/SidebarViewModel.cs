using System.Windows.Input;
using RoslynPad.UI;
using RoslynPad.ViewModels;

namespace RoslynPad.ViewModels;

/// <summary>
/// 侧边栏视图模型：管理左侧活动栏与功能区的显示状态。
/// </summary>
public class SidebarViewModel : NotificationObject
{
    private readonly ICommandProvider _commands;

    public SidebarViewModel(ICommandProvider commands)
    {
        _commands = commands;

        // 初始化显示命令
        ShowDocumentTreeViewCommand = commands.Create(() => SetActiveView(ActiveSidebarView.DocumentTree));
        ShowUserPreferencesViewCommand = commands.Create(() => SetActiveView(ActiveSidebarView.UserPreferences));
        ShowPerformanceSettingsViewCommand = commands.Create(() => SetActiveView(ActiveSidebarView.PerformanceSettings));
        ShowDLLExpandViewCommand = commands.Create(() => SetActiveView(ActiveSidebarView.DLLExpand));
        ShowDocumentTreeView4Command = commands.Create(() => SetActiveView(ActiveSidebarView.DocumentTree4));
        ShowRunAndDebugViewCommand = commands.Create(() => SetActiveView(ActiveSidebarView.RunAndDebug));
    }

    #region 活动视图枚举

    public enum ActiveSidebarView
    {
        DocumentTree,
        UserPreferences,
        PerformanceSettings,
        DLLExpand,
        DocumentTree4,
        RunAndDebug
    }

    #endregion

    #region 可见性属性

    private bool _isDocumentTreeViewVisible = true;
    public bool IsDocumentTreeViewVisible
    {
        get => _isDocumentTreeViewVisible;
        set => SetProperty(ref _isDocumentTreeViewVisible, value);
    }

    private bool _isUserPreferencesViewVisible;
    public bool IsUserPreferencesViewVisible
    {
        get => _isUserPreferencesViewVisible;
        set => SetProperty(ref _isUserPreferencesViewVisible, value);
    }

    private bool _isPerformanceSettingsViewVisible;
    public bool IsPerformanceSettingsViewVisible
    {
        get => _isPerformanceSettingsViewVisible;
        set => SetProperty(ref _isPerformanceSettingsViewVisible, value);
    }

    private bool _isDLLExpandViewVisible;
    public bool IsDLLExpandViewVisible
    {
        get => _isDLLExpandViewVisible;
        set => SetProperty(ref _isDLLExpandViewVisible, value);
    }

    private bool _isDocumentTreeView4Visible;
    public bool IsDocumentTreeView4Visible
    {
        get => _isDocumentTreeView4Visible;
        set => SetProperty(ref _isDocumentTreeView4Visible, value);
    }

    private bool _isRunAndDebugViewVisible;
    public bool IsRunAndDebugViewVisible
    {
        get => _isRunAndDebugViewVisible;
        set => SetProperty(ref _isRunAndDebugViewVisible, value);
    }

    #endregion

    #region 侧边栏尺寸属性

    private bool _isSidebarOpen = true;
    public bool IsSidebarOpen
    {
        get => _isSidebarOpen;
        set
        {
            if (SetProperty(ref _isSidebarOpen, value))
                OnPropertyChanged(nameof(SidebarOpenLength));
        }
    }

    private double _sidebarWidth = 350;
    public double SidebarWidth
    {
        get => _sidebarWidth;
        set
        {
            var clamped = Math.Clamp(value, 180, 800);
            if (SetProperty(ref _sidebarWidth, clamped))
                OnPropertyChanged(nameof(SidebarOpenLength));
        }
    }

    public double SidebarOpenLength => 48 + SidebarWidth;

    #endregion

    #region 切换命令

    public ICommand ShowDocumentTreeViewCommand { get; }
    public ICommand ShowUserPreferencesViewCommand { get; }
    public ICommand ShowPerformanceSettingsViewCommand { get; }
    public ICommand ShowDLLExpandViewCommand { get; }
    public ICommand ShowDocumentTreeView4Command { get; }
    public ICommand ShowRunAndDebugViewCommand { get; }

    #endregion

    #region 辅助方法

    private void SetActiveView(ActiveSidebarView view)
    {
        IsDocumentTreeViewVisible = view == ActiveSidebarView.DocumentTree;
        IsUserPreferencesViewVisible = view == ActiveSidebarView.UserPreferences;
        IsPerformanceSettingsViewVisible = view == ActiveSidebarView.PerformanceSettings;
        IsDLLExpandViewVisible = view == ActiveSidebarView.DLLExpand;
        IsDocumentTreeView4Visible = view == ActiveSidebarView.DocumentTree4;
        IsRunAndDebugViewVisible = view == ActiveSidebarView.RunAndDebug;
    }

    #endregion
}
