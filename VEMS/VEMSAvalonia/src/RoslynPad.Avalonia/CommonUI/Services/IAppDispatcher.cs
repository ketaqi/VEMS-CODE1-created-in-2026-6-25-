namespace RoslynPad.UI;

/// <summary>
/// 应用程序调度器接口：提供跨线程 UI 调度的统一抽象。
/// </summary>
/// <remarks>
/// <para>
/// 此接口用于将操作封送到 UI 线程执行，确保线程安全的 UI 更新。
/// 在 Avalonia 中，具体实现通常委托给 <c>Dispatcher.UIThread</c>。
/// </para>
/// <para>
/// 典型使用场景：
/// <list type="bullet">
///   <item><description>后台任务完成后更新 UI</description></item>
///   <item><description>异步操作中修改绑定属性</description></item>
///   <item><description>从非 UI 线程触发 UI 事件</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 在后台线程中更新 UI
/// await Task.Run(() =>
/// {
///     // 执行耗时操作...
///     
///     // 封送回 UI 线程更新界面
///     _dispatcher.InvokeAsync(() =>
///     {
///         StatusText = "操作完成";
///     });
/// });
/// </code>
/// </example>
public interface IAppDispatcher
{
    /// <summary>
    /// 异步调度一个操作到 UI 线程执行。
    /// </summary>
    /// <param name="action">要在 UI 线程执行的操作。</param>
    /// <param name="priority">调度优先级，默认为 <see cref="AppDispatcherPriority.Normal"/>。</param>
    /// <param name="cancellationToken">用于取消操作的令牌。</param>
    /// <remarks>
    /// 此方法立即返回，不等待操作完成。如需等待操作完成，请使用 <see cref="InvokeTaskAsync"/>。
    /// </remarks>
    /// <example>
    /// <code>
    /// _dispatcher.InvokeAsync(() => 
    /// {
    ///     ProgressValue = 100;
    ///     IsLoading = false;
    /// }, AppDispatcherPriority.High);
    /// </code>
    /// </example>
    void InvokeAsync(Action action, AppDispatcherPriority priority = AppDispatcherPriority.Normal, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步调度一个操作到 UI 线程执行，并等待其完成。
    /// </summary>
    /// <param name="action">要在 UI 线程执行的操作。</param>
    /// <param name="priority">调度优先级，默认为 <see cref="AppDispatcherPriority.Normal"/>。</param>
    /// <param name="cancellationToken">用于取消操作的令牌。</param>
    /// <returns>表示异步操作的任务，任务完成时表示操作已在 UI 线程执行完毕。</returns>
    /// <example>
    /// <code>
    /// await _dispatcher.InvokeTaskAsync(() =>
    /// {
    ///     // 更新多个 UI 属性
    ///     Items.Clear();
    ///     Items.AddRange(newItems);
    /// });
    /// </code>
    /// </example>
    Task InvokeTaskAsync(Action action, AppDispatcherPriority priority = AppDispatcherPriority.Normal, CancellationToken cancellationToken = default);
}

/// <summary>
/// 调度器优先级枚举：定义操作在 UI 线程上的执行优先级。
/// </summary>
/// <remarks>
/// 优先级影响操作在调度队列中的排序，高优先级操作会更快被执行。
/// </remarks>
public enum AppDispatcherPriority
{
    /// <summary>
    /// 普通优先级：大多数 UI 更新操作使用此优先级。
    /// </summary>
    Normal,

    /// <summary>
    /// 高优先级：需要立即响应的操作，如用户交互反馈。
    /// </summary>
    High,

    /// <summary>
    /// 低优先级：可延迟执行的操作，如后台数据加载后的 UI 更新。
    /// </summary>
    Low
}
