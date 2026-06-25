using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace RoslynPad.Roslyn.Diagnostics;

/// <summary>
/// 诊断分析器服务接口，定义获取文档诊断数据的方法
/// </summary>
public interface IDiagnosticAnalyzerService
{
    /// <summary>
    /// 异步获取指定文档指定范围的诊断数据
    /// </summary>
    /// <param name="document">目标文本文档</param>
    /// <param name="range">文本范围，null表示整个文档</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>诊断数据数组</returns>
    Task<ImmutableArray<DiagnosticData>> GetDiagnosticsForSpanAsync(TextDocument document, TextSpan? range, CancellationToken cancellationToken);
}
