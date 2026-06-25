using System.Runtime.CompilerServices;

namespace RoslynPad.Build;

/// <summary>
/// 提供切换到线程池执行的 awaitable，不捕获同步上下文。
/// </summary>
public readonly struct NoContextYieldAwaitable
{
    /// <summary>
    /// 获取 awaiter。
    /// </summary>
    /// <returns>awaiter 实例。</returns>
    public NoContextYieldAwaiter GetAwaiter() => new();

    /// <summary>
    /// 线程池 awaiter 实现。
    /// </summary>
    public readonly struct NoContextYieldAwaiter : ICriticalNotifyCompletion
    {
        /// <summary>
        /// 获取是否已在线程池线程上。
        /// </summary>
        public bool IsCompleted => Thread.CurrentThread.IsThreadPoolThread;

        /// <summary>
        /// 注册完成回调。
        /// </summary>
        /// <param name="continuation">回调委托。</param>
        public void OnCompleted(Action continuation) => QueueContinuation(continuation, flowContext: false);

        /// <summary>
        /// 不安全地注册完成回调。
        /// </summary>
        /// <param name="continuation">回调委托。</param>
        public void UnsafeOnCompleted(Action continuation) => QueueContinuation(continuation, flowContext: false);

        private static void QueueContinuation(Action continuation, bool flowContext)
        {
            ArgumentNullException.ThrowIfNull(continuation);

            if (flowContext)
            {
                ThreadPool.QueueUserWorkItem(s_waitCallbackRunAction, continuation);
            }
            else
            {
                ThreadPool.UnsafeQueueUserWorkItem(s_waitCallbackRunAction, continuation);
            }
        }

        private static readonly WaitCallback s_waitCallbackRunAction = RunAction!;

        private static void RunAction(object state) => ((Action)state)();

        /// <summary>
        /// 获取结果（无操作）。
        /// </summary>
        public void GetResult() { }
    }
}
