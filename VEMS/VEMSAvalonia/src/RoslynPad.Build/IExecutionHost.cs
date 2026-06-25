using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RoslynPad.Build;

/// <summary>
/// 定义代码执行宿主的接口。
/// </summary>
internal interface IExecutionHost
{
    /// <summary>
    /// 获取或设置执行平台。
    /// </summary>
    ExecutionPlatform Platform { get; set; }

    /// <summary>
    /// 获取或设置文档名称。
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// 获取或设置 dotnet 可执行文件路径。
    /// </summary>
    string DotNetExecutable { get; set; }

    /// <summary>
    /// 获取元数据引用集合。
    /// </summary>
    ImmutableArray<MetadataReference> MetadataReferences { get; }

    /// <summary>
    /// 获取分析器引用集合。
    /// </summary>
    ImmutableArray<AnalyzerFileReference> Analyzers { get; }

    /// <summary>
    /// 获取或设置关联的文档 ID。
    /// </summary>
    DocumentId? DocumentId { get; set; }

    /// <summary>
    /// 编译错误事件。
    /// </summary>
    event Action<IList<CompilationErrorResultObject>>? CompilationErrors;

    /// <summary>
    /// 反汇编完成事件。
    /// </summary>
    event Action<string>? Disassembled;

    /// <summary>
    /// 输出结果事件。
    /// </summary>
    event Action<ResultObject>? Dumped;

    /// <summary>
    /// 运行时错误事件。
    /// </summary>
    event Action<ExceptionResultObject>? Error;

    /// <summary>
    /// 请求读取输入事件。
    /// </summary>
    event Action? ReadInput;

    /// <summary>
    /// 还原开始事件。
    /// </summary>
    event Action? RestoreStarted;

    /// <summary>
    /// 还原完成事件。
    /// </summary>
    event Action<RestoreResult>? RestoreCompleted;

    /// <summary>
    /// 进度变化事件。
    /// </summary>
    event Action<ProgressResultObject>? ProgressChanged;

    /// <summary>
    /// 清除还原缓存。
    /// </summary>
    void ClearRestoreCache();

    /// <summary>
    /// 异步更新引用。
    /// </summary>
    /// <param name="alwaysRestore">是否始终执行还原。</param>
    Task UpdateReferencesAsync(bool alwaysRestore);

    /// <summary>
    /// 异步发送输入到运行中的进程。
    /// </summary>
    /// <param name="input">输入内容。</param>
    Task SendInputAsync(string input);

    /// <summary>
    /// 异步执行代码。
    /// </summary>
    /// <param name="path">代码文件路径。</param>
    /// <param name="disassemble">是否反汇编。</param>
    /// <param name="optimizationLevel">优化级别。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    Task ExecuteAsync(string path, bool disassemble, OptimizationLevel? optimizationLevel, CancellationToken cancellationToken);

    /// <summary>
    /// 异步终止执行。
    /// </summary>
    Task TerminateAsync();
}
