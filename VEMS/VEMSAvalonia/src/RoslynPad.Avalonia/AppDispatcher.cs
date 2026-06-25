using System;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using RoslynPad.UI;
using Avalonia.Threading;

namespace RoslynPad;

/// <summary>
/// 应用级调度器适配器：
/// 将应用内部抽象 <see cref="IAppDispatcher"/> 映射到 Avalonia 的 <see cref="Dispatcher.UIThread"/>，
/// 以统一在 UI 线程上调度执行委托（支持优先级与取消令牌）。
/// </summary>
/// <remarks>
/// 设计要点：
/// <list type="bullet">
/// <item><description><see cref="InvokeAsync(Action, AppDispatcherPriority, CancellationToken)"/>：无返回值的“发起即忘”调用，内部仍以任务形式调度，但不等待结果。</description></item>
/// <item><description><see cref="InvokeTaskAsync(Action, AppDispatcherPriority, CancellationToken)"/>：返回 <see cref="Task"/> 以便调用方可继续 <c>await</c> 或组合任务。</description></item>
/// <item><description>优先级通过 <see cref="ConvertPriority(AppDispatcherPriority)"/> 进行枚举映射，保持跨平台一致性。</description></item>
/// </list>
/// 线程模型：始终封送到 Avalonia UI 线程执行；请避免在该线程中进行重计算或阻塞操作。
[Export(typeof(IAppDispatcher))]
public class AppDispatcher : IAppDispatcher
{
    /// <summary>
    /// 在 UI 线程异步调度执行一个操作（不关心完成时机）。
    /// </summary>
    /// <param name="action">要执行的委托。</param>
    /// <param name="priority">应用层调度优先级。</param>
    /// <param name="cancellationToken">取消令牌：在操作排队期间可取消。</param>
    /// <remarks>
    /// 内部使用 <see cref="InternalInvoke(Action, AppDispatcherPriority, CancellationToken)"/> 发起调度，
    /// 但不对返回任务进行等待（“发起即忘”模式）。
    /// </remarks>
    public void InvokeAsync(Action action, AppDispatcherPriority priority = AppDispatcherPriority.Normal,
        CancellationToken cancellationToken = new CancellationToken())
    {
        _ = InternalInvoke(action, priority, cancellationToken);
    }

    /// <summary>
    /// 在 UI 线程异步调度执行一个操作，并返回可等待的任务。
    /// </summary>
    /// <param name="action">要执行的委托。</param>
    /// <param name="priority">应用层调度优先级。</param>
    /// <param name="cancellationToken">取消令牌：在操作排队期间可取消。</param>
    /// <returns>表示调度与执行完成的 <see cref="Task"/>。</returns>
    /// <remarks>
    /// 适用于需要串联/等待 UI 线程操作完成的场景。
    /// </remarks>
    public Task InvokeTaskAsync(Action action, AppDispatcherPriority priority = AppDispatcherPriority.Normal,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return InternalInvoke(action, priority, cancellationToken);
    }

    /// <summary>
    /// 内部统一调度入口：将应用层优先级转换为 Avalonia 优先级并封送到 UI 线程。
    /// </summary>
    private Task InternalInvoke(Action action, AppDispatcherPriority priority, CancellationToken cancellationToken)
    {
        // Dispatcher.UIThread.InvokeAsync 返回 DispatcherOperation；通过 GetTask() 获取可等待的 Task
        return Dispatcher.UIThread.InvokeAsync(action, ConvertPriority(priority), cancellationToken).GetTask(); //
    }

    /// <summary>
    /// 应用层优先级到 Avalonia <see cref="DispatcherPriority"/> 的映射。
    /// </summary>
    /// <param name="priority">应用层优先级枚举。</param>
    /// <returns>Avalonia 对应的 <see cref="DispatcherPriority"/>。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当收到未定义的枚举值时抛出。</exception>
    private DispatcherPriority ConvertPriority(AppDispatcherPriority priority) => priority switch
    {
        AppDispatcherPriority.Normal => DispatcherPriority.Normal,
        AppDispatcherPriority.High => DispatcherPriority.Send,
        AppDispatcherPriority.Low => DispatcherPriority.Background,
        _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null),
    }; //
}
