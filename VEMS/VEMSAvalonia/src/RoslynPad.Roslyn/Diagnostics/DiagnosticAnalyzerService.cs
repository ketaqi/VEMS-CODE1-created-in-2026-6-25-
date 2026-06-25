using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace RoslynPad.Roslyn.Diagnostics;

/// <summary>
/// 诊断分析器服务的包装类，用于适配Roslyn原生诊断分析器服务并转换诊断数据格式
/// </summary>
[Export(typeof(IDiagnosticAnalyzerService)), Shared]
[method: ImportingConstructor]
internal sealed class DiagnosticAnalyzerService(Microsoft.CodeAnalysis.Diagnostics.IDiagnosticAnalyzerService inner) : IDiagnosticAnalyzerService
{
    /// <summary>
    /// 异步获取指定文档中指定文本范围的诊断数据
    /// </summary>
    /// <param name="document">要分析的文本文档</param>
    /// <param name="range">要分析的文本范围，若为null则分析整个文档</param>
    /// <param name="cancellationToken">取消令牌，用于取消异步操作</param>
    /// <returns>包含诊断数据的不可变数组</returns>
    public async Task<ImmutableArray<DiagnosticData>> GetDiagnosticsForSpanAsync(TextDocument document, TextSpan? range, CancellationToken cancellationToken)
    {
        var diagnostics = await inner.GetDiagnosticsForSpanAsync(document, range, DiagnosticKind.All, cancellationToken).ConfigureAwait(false);

        return ConvertDiagnostics(diagnostics);
    }

    /// <summary>
    /// 将Roslyn原生诊断数据数组转换为封装后的诊断数据数组
    /// </summary>
    /// <param name="diagnostics">Roslyn原生诊断数据数组</param>
    /// <returns>封装后的诊断数据不可变数组</returns>
    private static ImmutableArray<DiagnosticData> ConvertDiagnostics(ImmutableArray<Microsoft.CodeAnalysis.Diagnostics.DiagnosticData> diagnostics) =>
        diagnostics.SelectAsArray(d => new DiagnosticData(d));
}
