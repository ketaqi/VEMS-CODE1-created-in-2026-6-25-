using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using RoslynPad.Build;

namespace RoslynPad.UI;

public sealed class BackgroundRunInfo
{
    public required string Id { get; init; }
    public required string DocumentName { get; init; }
    public required string ScriptPath { get; init; }
    public required string LogPath { get; init; }
    public required DateTime StartTime { get; init; }

    public CancellationTokenSource Cancellation { get; init; } = new();

    public Task? Task { get; set; }
    public bool IsCompleted { get; private set; }
    public bool IsFaulted { get; private set; }
    public Exception? Error { get; private set; }

    // 运行时信息（新增）
    internal ExecutionHost? Host { get; set; }
    internal IProcessJob? Job { get; set; }
    public int? ProcessId { get; internal set; }
    public string Display => ProcessId is int pid ? $"PID {pid}" : "(starting)";

    internal void MarkCompleted(Exception? error)
    {
        IsCompleted = true;
        IsFaulted = error is not null;
        Error = error;
    }
}

public sealed class BackgroundRunManager
{
    private static readonly Lazy<BackgroundRunManager> _lazy = new(() => new BackgroundRunManager());
    public static BackgroundRunManager Instance => _lazy.Value;

    private BackgroundRunManager() { }

    public ObservableCollection<BackgroundRunInfo> Runs { get; } = new();

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

    public void MarkCompleted(BackgroundRunInfo info, Exception? error = null) => info.MarkCompleted(error);

    public IEnumerable<BackgroundRunInfo> ActiveRunsForDocument(string documentName) =>
        Runs.Where(r => !r.IsCompleted && r.DocumentName == documentName);

    public IEnumerable<BackgroundRunInfo> ActiveRunsAll() =>
        Runs.Where(r => !r.IsCompleted);

    public BackgroundRunInfo? FindByPid(int pid) =>
        Runs.FirstOrDefault(r => !r.IsCompleted && r.ProcessId == pid);

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

    public Task CancelAllForDocumentAsync(string documentName, ILogger? logger = null) =>
        Task.WhenAll(ActiveRunsForDocument(documentName).Select(r => CancelAsync(r, logger)));

    public Task CancelAllAsync(ILogger? logger = null) =>
        Task.WhenAll(ActiveRunsAll().Select(r => CancelAsync(r, logger)));

    public Task CancelByPidAsync(int pid, ILogger? logger = null)
    {
        var run = FindByPid(pid);
        return run != null ? CancelAsync(run, logger) : Task.CompletedTask;
    }
}
