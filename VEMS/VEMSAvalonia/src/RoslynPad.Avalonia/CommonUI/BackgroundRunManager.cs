using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using RoslynPad.Build;

namespace RoslynPad.UI;

/// <summary>
/// 后台运行信息：表示一个后台执行的脚本任务的完整状态。
/// </summary>
/// <remarks>
/// <para>
/// 此类封装了后台脚本运行的所有相关信息，包括：
/// <list type="bullet">
///   <item><description>唯一标识符和文档信息</description></item>
///   <item><description>运行状态（进行中/已完成/已失败）</description></item>
///   <item><description>进程信息和取消机制</description></item>
///   <item><description>日志文件路径</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class BackgroundRunInfo
{
    /// <summary>获取运行任务的唯一标识符。</summary>
    public required string Id { get; init; }

    /// <summary>获取关联的文档名称。</summary>
    public required string DocumentName { get; init; }

    /// <summary>获取脚本文件的完整路径。</summary>
    public required string ScriptPath { get; init; }

    /// <summary>获取日志文件的完整路径。</summary>
    public required string LogPath { get; init; }

    /// <summary>获取任务启动时间。</summary>
    public required DateTime StartTime { get; init; }

    /// <summary>获取用于取消任务的取消令牌源。</summary>
    public CancellationTokenSource Cancellation { get; init; } = new();

    /// <summary>获取或设置表示运行任务的 <see cref="Task"/> 对象。</summary>
    public Task? Task { get; set; }

    /// <summary>获取任务是否已完成。</summary>
    public bool IsCompleted { get; private set; }

    /// <summary>获取任务是否因错误而失败。</summary>
    public bool IsFaulted { get; private set; }

    /// <summary>获取任务失败时的异常（如果有）。</summary>
    public Exception? Error { get; private set; }

    /// <summary>获取或设置执行宿主（内部使用）。</summary>
    internal ExecutionHost? Host { get; set; }

    /// <summary>获取或设置进程作业对象（内部使用）。</summary>
    internal IProcessJob? Job { get; set; }

    /// <summary>获取或设置运行进程的 PID。</summary>
    public int? ProcessId { get; internal set; }

    /// <summary>获取任务的显示文本（PID 或启动中状态）。</summary>
    public string Display => ProcessId is int pid ? $"PID {pid}" : "(starting)";

    /// <summary>
    /// 标记任务为已完成状态。
    /// </summary>
    /// <param name="error">如果任务失败，传入异常对象；成功则为 <c>null</c>。</param>
    internal void MarkCompleted(Exception? error)
    {
        IsCompleted = true;
        IsFaulted = error is not null;
        Error = error;
    }
}

/// <summary>
/// 后台运行管理器：管理所有后台执行的脚本任务。
/// </summary>
/// <remarks>
/// <para>
/// 此类使用单例模式，提供对后台运行任务的集中管理：
/// <list type="bullet">
///   <item><description>注册和跟踪后台运行任务</description></item>
///   <item><description>按文档名称或进程 ID 查询任务</description></item>
///   <item><description>取消单个或批量任务</description></item>
/// </list>
/// </para>
/// <para>
/// 取消机制：
/// <list type="number">
///   <item><description>首先尝试通过 <see cref="CancellationToken"/> 协作取消</description></item>
///   <item><description>然后调用 <see cref="ExecutionHost.TerminateAsync"/> 终止宿主</description></item>
///   <item><description>最后通过 <see cref="IProcessJob"/> 清理进程树（仅 Windows）</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 注册后台任务
/// var info = BackgroundRunManager.Instance.Register("Script.csx", scriptPath, logPath);
/// 
/// // 查询活动任务
/// var activeTasks = BackgroundRunManager.Instance.ActiveRunsForDocument("Script.csx");
/// 
/// // 取消特定任务
/// await BackgroundRunManager.Instance.CancelAsync(info, logger);
/// 
/// // 取消所有任务
/// await BackgroundRunManager.Instance.CancelAllAsync(logger);
/// </code>
/// </example>
public sealed class BackgroundRunManager
{
    /// <summary>延迟初始化的单例实例。</summary>
    private static readonly Lazy<BackgroundRunManager> _lazy = new(() => new BackgroundRunManager());

    /// <summary>获取 <see cref="BackgroundRunManager"/> 的单例实例。</summary>
    public static BackgroundRunManager Instance => _lazy.Value;

    /// <summary>私有构造函数，强制使用单例。</summary>
    private BackgroundRunManager() { }

    /// <summary>获取所有后台运行任务的可观察集合。</summary>
    /// <value>
    /// 包含所有（包括已完成的）后台运行信息的集合，可用于 UI 绑定。
    /// </value>
    public ObservableCollection<BackgroundRunInfo> Runs { get; } = new();

    /// <summary>
    /// 注册一个新的后台运行任务。
    /// </summary>
    /// <param name="documentName">文档名称。</param>
    /// <param name="scriptPath">脚本文件路径。</param>
    /// <param name="logPath">日志文件路径。</param>
    /// <returns>新创建的 <see cref="BackgroundRunInfo"/> 实例。</returns>
    public BackgroundRunInfo Register(string documentName, string scriptPath, string logPath)
    {
        var info = new BackgroundRunInfo
        {
            Id = Guid.NewGuid().ToString("n"),
            DocumentName = documentName,
            ScriptPath = scriptPath,
            LogPath = logPath,
            StartTime = DateTime.Now
        };
        Runs.Add(info);
        return info;
    }

    /// <summary>
    /// 将指定任务标记为已完成。
    /// </summary>
    /// <param name="info">任务信息。</param>
    /// <param name="error">如果任务失败，传入异常对象；成功则为 <c>null</c>。</param>
    public void MarkCompleted(BackgroundRunInfo info, Exception? error = null) => info.MarkCompleted(error);

    /// <summary>
    /// 获取指定文档的所有活动运行任务。
    /// </summary>
    /// <param name="documentName">文档名称。</param>
    /// <returns>未完成的运行任务枚举。</returns>
    public IEnumerable<BackgroundRunInfo> ActiveRunsForDocument(string documentName) =>
        Runs.Where(r => !r.IsCompleted && r.DocumentName == documentName);

    /// <summary>
    /// 获取所有活动运行任务。
    /// </summary>
    /// <returns>所有未完成的运行任务枚举。</returns>
    public IEnumerable<BackgroundRunInfo> ActiveRunsAll() =>
        Runs.Where(r => !r.IsCompleted);

    /// <summary>
    /// 通过进程 ID 查找运行任务。
    /// </summary>
    /// <param name="pid">进程 ID。</param>
    /// <returns>匹配的运行信息；如果未找到返回 <c>null</c>。</returns>
    public BackgroundRunInfo? FindByPid(int pid) =>
        Runs.FirstOrDefault(r => !r.IsCompleted && r.ProcessId == pid);

    /// <summary>
    /// 异步取消指定的后台任务。
    /// </summary>
    /// <param name="info">要取消的任务信息。</param>
    /// <param name="logger">可选的日志记录器。</param>
    /// <returns>表示异步操作的任务。</returns>
    public async Task CancelAsync(BackgroundRunInfo info, ILogger? logger = null)
    {
        try
        {
            info.Cancellation.Cancel();

            if (info.Host != null)
            {
                try { await info.Host.TerminateAsync().ConfigureAwait(false); }
                catch (Exception ex) { logger?.LogWarning(ex, "Terminate host failed"); }
            }
        }
        finally
        {
            // 兜底：Windows 下关闭 Job 杀整树；非 Windows no-op
            try { info.Job?.Dispose(); } catch { }
        }
    }

    /// <summary>
    /// 异步取消指定文档的所有活动任务。
    /// </summary>
    /// <param name="documentName">文档名称。</param>
    /// <param name="logger">可选的日志记录器。</param>
    /// <returns>表示异步操作的任务。</returns>
    public Task CancelAllForDocumentAsync(string documentName, ILogger? logger = null) =>
        Task.WhenAll(ActiveRunsForDocument(documentName).Select(r => CancelAsync(r, logger)));

    /// <summary>
    /// 异步取消所有活动任务。
    /// </summary>
    /// <param name="logger">可选的日志记录器。</param>
    /// <returns>表示异步操作的任务。</returns>
    public Task CancelAllAsync(ILogger? logger = null) =>
        Task.WhenAll(ActiveRunsAll().Select(r => CancelAsync(r, logger)));

    /// <summary>
    /// 通过进程 ID 异步取消任务。
    /// </summary>
    /// <param name="pid">进程 ID。</param>
    /// <param name="logger">可选的日志记录器。</param>
    /// <returns>表示异步操作的任务。</returns>
    public Task CancelByPidAsync(int pid, ILogger? logger = null)
    {
        var run = FindByPid(pid);
        return run != null ? CancelAsync(run, logger) : Task.CompletedTask;
    }
}
