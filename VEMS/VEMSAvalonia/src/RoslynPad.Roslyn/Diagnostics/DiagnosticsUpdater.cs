using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Shared.TestHooks;
using Microsoft.CodeAnalysis.Threading;

namespace RoslynPad.Roslyn.Diagnostics;

/// <summary>
/// 诊断信息更新器，负责监听工作区文档变化并更新诊断数据，提供诊断变更事件通知
/// </summary>
public class DiagnosticsUpdater : IDiagnosticsUpdater, IDisposable
{
    /// <summary>
    /// 工作区实例
    /// </summary>
    private readonly Workspace _workspace;

    /// <summary>
    /// 诊断分析器服务实例
    /// </summary>
    private readonly IDiagnosticAnalyzerService _diagnosticAnalyzerService;

    /// <summary>
    /// 线程安全锁对象，用于保护诊断数据的并发访问
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// 异步批处理工作队列，用于处理文档诊断更新请求
    /// </summary>
    private readonly AsyncBatchingWorkQueue<DocumentId> _workQueue;

    /// <summary>
    /// 取消令牌源，用于终止异步操作
    /// </summary>
    private readonly CancellationTokenSource _cts;

    /// <summary>
    /// 当前已缓存的诊断数据集合
    /// </summary>
    private HashSet<DiagnosticData> _currentDiagnostics;

    /// <summary>
    /// 获取或设置禁用的诊断ID集合
    /// </summary>
    public ImmutableHashSet<string> DisabledDiagnostics { get; set; } = [];

    /// <summary>
    /// DiagnosticsUpdater的MEF导出工厂类
    /// </summary>
    [ExportWorkspaceServiceFactory(typeof(IDiagnosticsUpdater))]
    [method: ImportingConstructor]
    internal class Factory(IDiagnosticAnalyzerService diagnosticAnalyzerService) : IWorkspaceServiceFactory
    {
        /// <summary>
        /// 创建诊断更新器服务实例
        /// </summary>
        /// <param name="workspaceServices">工作区服务容器</param>
        /// <returns>实现<see cref="IDiagnosticsUpdater"/>的服务实例</returns>
        public IWorkspaceService CreateService(HostWorkspaceServices workspaceServices)
        {
            return new DiagnosticsUpdater(workspaceServices.Workspace, diagnosticAnalyzerService);
        }
    }

    /// <summary>
    /// 初始化<see cref="DiagnosticsUpdater"/>实例
    /// </summary>
    /// <param name="workspace">关联的工作区实例</param>
    /// <param name="diagnosticAnalyzerService">诊断分析器服务实例</param>
    [ImportingConstructor]
    public DiagnosticsUpdater(Workspace workspace, IDiagnosticAnalyzerService diagnosticAnalyzerService)
    {
        workspace.DocumentOpened += OnDocumentOpened;
        workspace.DocumentActiveContextChanged += OnDocumentActiveContextChanged;
        workspace.WorkspaceChanged += OnWorkspaceChanged;
        foreach (var document in workspace.CurrentSolution.Projects.SelectMany(p => p.Documents))
        {
            ConnectDocument(document);
        }

        _workspace = workspace;
        _diagnosticAnalyzerService = diagnosticAnalyzerService;
        _currentDiagnostics = [];
        _cts = new CancellationTokenSource();

        _workQueue = new AsyncBatchingWorkQueue<DocumentId>(DelayTimeSpan.Short, ProcessWorkQueueAsync, new AsynchronousOperationListener(), _cts.Token);
    }

    /// <summary>
    /// 处理工作队列中的文档诊断更新请求
    /// </summary>
    /// <param name="documentIds">需要更新诊断的文档ID列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步操作的ValueTask</returns>
    private async ValueTask ProcessWorkQueueAsync(ImmutableSegmentedList<DocumentId> documentIds, CancellationToken cancellationToken)
    {
        foreach (var documentId in documentIds)
        {
            if (await _workspace.CurrentSolution.GetDocumentAsync(documentId, cancellationToken: cancellationToken).ConfigureAwait(false) is { } document)
            {
                await UpdateDiagnosticsAsync(document, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// 释放当前实例占用的资源
    /// </summary>
    public void Dispose()
    {
        _workspace.DocumentOpened -= OnDocumentOpened;
        _workspace.DocumentActiveContextChanged -= OnDocumentActiveContextChanged;
        _workspace.WorkspaceChanged -= OnWorkspaceChanged;
        _cts.Cancel();
    }

    /// <summary>
    /// 文档打开事件处理方法
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="args">文档事件参数</param>
    private void OnDocumentOpened(object? sender, DocumentEventArgs args) => ConnectDocument(args.Document);

    /// <summary>
    /// 文档活动上下文变更事件处理方法
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">文档活动上下文变更事件参数</param>
    private void OnDocumentActiveContextChanged(object? sender, DocumentActiveContextChangedEventArgs e) => _workQueue.AddWork(e.NewActiveContextDocumentId);

    /// <summary>
    /// 工作区变更事件处理方法
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">工作区变更事件参数</param>
    private void OnWorkspaceChanged(object? sender, WorkspaceChangeEventArgs e)
    {
        if (e.DocumentId is { } documentId)
        {
            _workQueue.AddWork(documentId);
        }
    }

    /// <summary>
    /// 关联文档并监听文本变更事件，触发诊断更新
    /// </summary>
    /// <param name="document">需要关联的文档实例</param>
    private void ConnectDocument(Document document)
    {
        if (document.TryGetText(out var text))
        {
            text.Container.TextChanged += (o, e) => _workQueue.AddWork(document.Id);
        }

        _workQueue.AddWork(document.Id);
    }

    /// <summary>
    /// 异步更新指定文档的诊断数据，并触发诊断变更事件
    /// </summary>
    /// <param name="document">需要更新诊断的文档</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步操作的Task</returns>
    private async Task UpdateDiagnosticsAsync(Document document, CancellationToken cancellationToken)
    {
        var diagnostics = await GetDiagnostics(document, cancellationToken).ConfigureAwait(false);

        lock (_lock)
        {
            var addedDiagnostics = diagnostics.Where(d => !_currentDiagnostics.Contains(d) && !DisabledDiagnostics.Contains(d.Id)).ToHashSet();
            _currentDiagnostics.ExceptWith(diagnostics);
            var removedDiagnostics = _currentDiagnostics;

            _currentDiagnostics = [];
            foreach (var diag in diagnostics)
            {
                _currentDiagnostics.Add(diag);
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (addedDiagnostics.Count > 0 || removedDiagnostics.Count > 0)
            {
                DiagnosticsChanged?.Invoke(new DiagnosticsChangedArgs(document.Id, addedDiagnostics, removedDiagnostics));
            }
        }
    }

    /// <summary>
    /// 异步获取指定文档的诊断数据
    /// </summary>
    /// <param name="document">目标文档</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>诊断数据数组，获取失败时返回空数组</returns>
    private async Task<ImmutableArray<DiagnosticData>> GetDiagnostics(Document document, CancellationToken cancellationToken)
    {
        try
        {
            return await _diagnosticAnalyzerService.GetDiagnosticsForSpanAsync(document, range: null, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// 诊断数据变更事件，当文档诊断数据新增/移除时触发
    /// </summary>
    public event Action<DiagnosticsChangedArgs>? DiagnosticsChanged;
}
