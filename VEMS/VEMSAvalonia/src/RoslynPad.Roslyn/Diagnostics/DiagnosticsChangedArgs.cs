using Microsoft.CodeAnalysis;

namespace RoslynPad.Roslyn.Diagnostics;

/// <summary>
/// 诊断信息变更事件参数类
/// </summary>
/// <param name="DocumentId">发生诊断变更的文档ID</param>
/// <param name="AddedDiagnostics">新增的诊断数据集合</param>
/// <param name="RemovedDiagnostics">移除的诊断数据集合</param>
public record DiagnosticsChangedArgs(
    DocumentId DocumentId,
    IReadOnlySet<DiagnosticData> AddedDiagnostics,
    IReadOnlySet<DiagnosticData> RemovedDiagnostics);
