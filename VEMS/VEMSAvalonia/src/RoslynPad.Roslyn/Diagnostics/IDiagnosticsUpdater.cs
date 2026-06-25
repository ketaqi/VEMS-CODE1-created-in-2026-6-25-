using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Host;

namespace RoslynPad.Roslyn.Diagnostics;

/// <summary>
/// 诊断更新器接口，定义诊断更新相关的属性和事件
/// </summary>
public interface IDiagnosticsUpdater : IWorkspaceService
{
    /// <summary>
    /// 获取或设置禁用的诊断ID集合
    /// </summary>
    ImmutableHashSet<string> DisabledDiagnostics { get; set; }

    /// <summary>
    /// 诊断数据变更事件，参数为<see cref="DiagnosticsChangedArgs"/>
    /// </summary>
    event Action<DiagnosticsChangedArgs>? DiagnosticsChanged;
}
