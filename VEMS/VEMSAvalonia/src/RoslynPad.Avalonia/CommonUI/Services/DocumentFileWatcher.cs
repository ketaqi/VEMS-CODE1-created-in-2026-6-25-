using System.Composition;
using RoslynPad.UI.Utilities;

namespace RoslynPad.UI;

/// <summary>
/// 文档文件变更类型枚举：表示文件系统中发生的变更操作类型。
/// </summary>
public enum DocumentFileChangeType
{
    /// <summary>
    /// 文件或目录被创建。
    /// </summary>
    Created,

    /// <summary>
    /// 文件或目录被删除。
    /// </summary>
    Deleted,

    /// <summary>
    /// 文件或目录被重命名。
    /// </summary>
    Renamed
}

/// <summary>
/// 文档文件变更事件数据：封装文件变更的详细信息。
/// </summary>
/// <remarks>
/// <para>
/// 此记录类型包含文件变更的类型、原路径，以及重命名时的新路径。
/// </para>
/// </remarks>
/// <param name="type">变更类型。</param>
/// <param name="path">变更的文件或目录路径（重命名时为原路径）。</param>
/// <param name="newPath">重命名操作的新路径；其他操作为 <c>null</c>。</param>
/// <example>
/// <code>
/// // 创建事件
/// var created = new DocumentFileChanged(DocumentFileChangeType.Created, "/path/to/file.cs");
/// 
/// // 重命名事件
/// var renamed = new DocumentFileChanged(DocumentFileChangeType.Renamed, "/old/path.cs", "/new/path.cs");
/// </code>
/// </example>
public class DocumentFileChanged(DocumentFileChangeType type, string path, string? newPath = null)
{
    /// <summary>
    /// 获取变更类型。
    /// </summary>
    public DocumentFileChangeType Type { get; } = type;

    /// <summary>
    /// 获取变更的文件或目录路径。
    /// </summary>
    /// <remarks>
    /// 对于重命名操作，此属性返回原路径。
    /// </remarks>
    public string Path { get; } = path;

    /// <summary>
    /// 获取重命名操作的新路径。
    /// </summary>
    /// <value>
    /// 重命名时为新路径；创建或删除操作时为 <c>null</c>。
    /// </value>
    public string? NewPath { get; } = newPath;
}

/// <summary>
/// 文档文件监视器：监控文档目录的文件系统变更并通知订阅者。
/// </summary>
/// <remarks>
/// <para>
/// 此类封装 <see cref="FileSystemWatcher"/>，提供响应式（Observable）的文件变更通知机制。
/// 支持监控文件的创建、删除和重命名操作，并自动包含子目录。
/// </para>
/// <para>
/// 设计特点：
/// <list type="bullet">
///   <item><description>实现 <see cref="IObservable{T}"/> 接口，支持响应式订阅模式</description></item>
///   <item><description>变更通知自动封送到 UI 线程（通过 <see cref="IAppDispatcher"/>）</description></item>
///   <item><description>更换监控路径时自动清理旧的订阅者</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 订阅文件变更
/// var watcher = serviceProvider.GetRequiredService&lt;DocumentFileWatcher&gt;();
/// watcher.Path = "/path/to/documents";
/// 
/// var subscription = watcher.Subscribe(Observer.Create&lt;DocumentFileChanged&gt;(change =>
/// {
///     Console.WriteLine($"{change.Type}: {change.Path}");
///     if (change.NewPath != null)
///         Console.WriteLine($"  -> {change.NewPath}");
/// }));
/// 
/// // 取消订阅
/// subscription.Dispose();
/// </code>
/// </example>
[Export]
public class DocumentFileWatcher : IDisposable, IObservable<DocumentFileChanged>
{
    /// <summary>
    /// UI 线程调度器，用于将变更通知封送到 UI 线程。
    /// </summary>
    private readonly IAppDispatcher _appDispatcher;

    /// <summary>
    /// 底层的文件系统监视器。
    /// </summary>
    private readonly FileSystemWatcher _fileSystemWatcher;

    /// <summary>
    /// 当前活动的观察者列表。
    /// </summary>
    private readonly List<IObserver<DocumentFileChanged>> _observers;

    /// <summary>
    /// 初始化 <see cref="DocumentFileWatcher"/> 类的新实例。
    /// </summary>
    /// <param name="appDispatcher">UI 线程调度器，用于封送变更通知。</param>
    /// <remarks>
    /// 此构造函数由 MEF 容器通过依赖注入调用。
    /// 初始化时会配置 <see cref="FileSystemWatcher"/> 以监控子目录中的变更。
    /// </remarks>
    [ImportingConstructor]
    public DocumentFileWatcher(IAppDispatcher appDispatcher)
    {
        _appDispatcher = appDispatcher;
        _observers = [];
        _fileSystemWatcher = new FileSystemWatcher();
        _fileSystemWatcher.Created += OnChanged;
        _fileSystemWatcher.Renamed += OnRenamed;
        _fileSystemWatcher.Deleted += OnChanged;
        _fileSystemWatcher.IncludeSubdirectories = true;
    }

    /// <summary>
    /// 获取或设置要监控的目录路径。
    /// </summary>
    /// <value>
    /// 当前监控的目录路径。
    /// </value>
    /// <remarks>
    /// <para>
    /// 设置新路径时：
    /// <list type="bullet">
    ///   <item><description>如果目录存在，启用文件监控</description></item>
    ///   <item><description>如果目录不存在，禁用文件监控</description></item>
    ///   <item><description>清除所有现有订阅者（因为根目录已更改）</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public string Path
    {
        get => _fileSystemWatcher.Path;
        set
        {
            var exists = Directory.Exists(value);
            if (exists)
            {
                _fileSystemWatcher.Path = value;
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
            else
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
            }

            _observers.Clear(); // Most likely root has changed
        }
    }

    /// <summary>
    /// 处理文件创建或删除事件。
    /// </summary>
    /// <param name="sender">事件源。</param>
    /// <param name="e">包含变更信息的事件参数。</param>
    private void OnChanged(object? sender, FileSystemEventArgs e)
    {
        Publish(new DocumentFileChanged(ToDocumentFileChangeType(e.ChangeType), e.FullPath));
    }

    /// <summary>
    /// 将 <see cref="WatcherChangeTypes"/> 转换为 <see cref="DocumentFileChangeType"/>。
    /// </summary>
    /// <param name="changeType">文件系统监视器的变更类型。</param>
    /// <returns>对应的文档文件变更类型。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当遇到不支持的变更类型时抛出。</exception>
    private DocumentFileChangeType ToDocumentFileChangeType(WatcherChangeTypes changeType)
    {
        return changeType switch
        {
            WatcherChangeTypes.Created => DocumentFileChangeType.Created,
            WatcherChangeTypes.Deleted => DocumentFileChangeType.Deleted,
            WatcherChangeTypes.Renamed => DocumentFileChangeType.Renamed,
            _ => throw new ArgumentOutOfRangeException(nameof(changeType), changeType, null),
        };
    }

    /// <summary>
    /// 处理文件重命名事件。
    /// </summary>
    /// <param name="sender">事件源。</param>
    /// <param name="e">包含重命名信息的事件参数。</param>
    private void OnRenamed(object? sender, RenamedEventArgs e)
    {
        Publish(new DocumentFileChanged(ToDocumentFileChangeType(e.ChangeType), e.OldFullPath, e.FullPath));
    }

    /// <summary>
    /// 向所有订阅者发布文件变更通知。
    /// </summary>
    /// <param name="documentFileChanged">要发布的变更事件。</param>
    /// <remarks>
    /// 通知会通过 <see cref="IAppDispatcher"/> 封送到 UI 线程执行，
    /// 确保订阅者可以安全地更新 UI。
    /// </remarks>
    private void Publish(DocumentFileChanged documentFileChanged)
    {
        foreach (var observer in _observers.ToArray())
        {
            _appDispatcher.InvokeAsync(() => observer.OnNext(documentFileChanged));
        }
    }

    /// <summary>
    /// 释放文件监视器资源。
    /// </summary>
    public void Dispose()
    {
        _fileSystemWatcher?.Dispose();
    }

    /// <summary>
    /// 订阅文件变更通知。
    /// </summary>
    /// <param name="observer">接收变更通知的观察者。</param>
    /// <returns>
    /// 用于取消订阅的 <see cref="IDisposable"/> 对象。
    /// 调用其 <see cref="IDisposable.Dispose"/> 方法将移除订阅。
    /// </returns>
    /// <example>
    /// <code>
    /// var subscription = watcher.Subscribe(Observer.Create&lt;DocumentFileChanged&gt;(
    ///     change => Console.WriteLine($"Changed: {change.Path}")
    /// ));
    /// 
    /// // 稍后取消订阅
    /// subscription.Dispose();
    /// </code>
    /// </example>
    public IDisposable Subscribe(IObserver<DocumentFileChanged> observer)
    {
        if (!_observers.Contains(observer))
            _observers.Add(observer);

        return new Disposer(() => _observers.Remove(observer));
    }
}
