using System.Composition;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RoslynPad.Roslyn.Diagnostics;

/// <summary>
/// 空实现的诊断刷新器，用于无实际刷新逻辑的场景
/// </summary>
[Export(typeof(IDiagnosticsRefresher))]
internal class NullDiagnosticsRefresher : IDiagnosticsRefresher
{
    /// <summary>
    /// 获取全局状态版本号
    /// </summary>
    public int GlobalStateVersion { get; }

    /// <summary>
    /// 工作区刷新请求事件
    /// </summary>
    public event Action? WorkspaceRefreshRequested;

    /// <summary>
    /// 发起工作区刷新请求
    /// </summary>
    public void RequestWorkspaceRefresh()
    {
        WorkspaceRefreshRequested?.Invoke();
    }
}
