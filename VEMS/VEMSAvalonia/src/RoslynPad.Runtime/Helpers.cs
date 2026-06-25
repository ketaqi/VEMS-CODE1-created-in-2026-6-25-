using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RoslynPad.Runtime;

/// <summary>
/// RoslynPad 运行时辅助方法。
/// </summary>
public static class Helpers
{
    internal static event Action<double?>? Progress;

    private static readonly Lazy<Task<SynchronizationContext>> s_dispatcherTask = new(CreateWpfDispatcherAsync);

    /// <summary>
    /// 创建一个运行 WPF Dispatcher 的新线程，并返回该 Dispatcher 的 <see cref="SynchronizationContext"/>。
    /// </summary>
    /// <returns>WPF Dispatcher 的同步上下文。</returns>
    /// <exception cref="PlatformNotSupportedException">在非 Windows 平台上调用时抛出。</exception>
    public static async Task<SynchronizationContext> CreateWpfDispatcherAsync()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException($"{nameof(CreateWpfDispatcherAsync)} is supported only on Windows");
        }

        var windowsBaseAssembly = Assembly.Load("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        var dispatcherType = windowsBaseAssembly.GetType("System.Windows.Threading.Dispatcher", throwOnError: true) ?? throw new InvalidOperationException();
        var dispatcherSyncContextCtor = windowsBaseAssembly.GetType("System.Windows.Threading.DispatcherSynchronizationContext", throwOnError: true)!
            .GetConstructors().FirstOrDefault(c => c.GetParameters() is var p && p.Length == 1 && p[0].ParameterType.Name == "Dispatcher") ?? throw new InvalidOperationException();
        var runMethod = dispatcherType.GetMethod("Run", [])?.CreateDelegate(typeof(Action)) as Action ?? throw new InvalidOperationException();
        var currentDispatcherProperty = dispatcherType.GetProperty("CurrentDispatcher") ?? throw new InvalidOperationException();

        var tcs = new TaskCompletionSource<object>();

        var thread = new Thread(() =>
        {
            var dispatcher = currentDispatcherProperty.GetValue(null) ?? throw new InvalidOperationException();
            tcs.SetResult(dispatcher);
            runMethod();
        });

        thread.SetApartmentState(ApartmentState.STA);

        thread.IsBackground = true;
        thread.Start();

        var dispatcher = await tcs.Task.ConfigureAwait(false);

        return (SynchronizationContext)dispatcherSyncContextCtor.Invoke([dispatcher]);
    }

    /// <summary>
    /// await 此方法以切换到默认的 WPF Dispatcher 线程。
    /// Dispatcher 会创建一个消息泵，可与大多数 Windows UI 框架配合使用（如 WPF、Windows Forms）。
    /// 使用 <see cref="CreateWpfDispatcherAsync"/> 懒惰创建 Dispatcher。
    /// </summary>
    /// <returns>可 await 的同步上下文。</returns>
    public static SynchronizationContextAwaitable RunWpfAsync() => new(s_dispatcherTask.Value);

    /// <summary>
    /// 向 UI 报告进度。
    /// </summary>
    /// <param name="progress">进度值（0.0 到 1.0），或 null 以隐藏进度报告。</param>
    public static void ReportProgress(double? progress)
    {
        if (progress.HasValue)
        {
            if (progress.Value < 0.0)
            {
                progress = 0.0;
            }
            else if (progress.Value > 1.0)
            {
                progress = 1.0;
            }
        }

        Progress?.Invoke(progress);
    }

    /// <summary>
    /// 同步上下文的可 await 包装器。
    /// </summary>
    /// <param name="task">同步上下文任务。</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct SynchronizationContextAwaitable(Task<SynchronizationContext> task)
    {
        private readonly Task<SynchronizationContext> _task = task;

        /// <summary>
        /// 获取 awaiter。
        /// </summary>
        public SynchronizationContextAwaiter GetAwaiter() => new(_task);
    }

    /// <summary>
    /// 同步上下文的 awaiter 实现。
    /// </summary>
    /// <param name="task">同步上下文任务。</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct SynchronizationContextAwaiter(Task<SynchronizationContext> task) : INotifyCompletion
    {
        private static readonly SendOrPostCallback s_postCallback = state => ((Action)state!)();

        private readonly Task<SynchronizationContext> _task = task;

        /// <summary>
        /// 获取是否已完成（始终返回 false 以强制异步继续）。
        /// </summary>
        public bool IsCompleted => false;

        /// <summary>
        /// 注册完成回调，将在同步上下文上执行。
        /// </summary>
        /// <param name="continuation">回调委托。</param>
        public void OnCompleted(Action continuation)
        {
            if (_task.Status == TaskStatus.RanToCompletion)
            {
                _task.Result.Post(s_postCallback, continuation);
                return;
            }

            _task.ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    t.Result.Post(s_postCallback, continuation);
                }
                else
                {
                    // GetResult will throw
                    continuation();
                }
            });
        }

        /// <summary>
        /// 获取结果。
        /// </summary>
        public void GetResult() => _task.GetAwaiter().GetResult();
    }
}
