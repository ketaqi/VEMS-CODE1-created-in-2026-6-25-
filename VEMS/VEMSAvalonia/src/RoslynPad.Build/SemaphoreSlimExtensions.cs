namespace RoslynPad.Build;

/// <summary>
/// 提供 <see cref="SemaphoreSlim"/> 的扩展方法，支持 using 模式自动释放。
/// </summary>
public static class SemaphoreSlimExtensions
{
    /// <summary>
    /// 同步等待信号量并返回可释放的包装器。
    /// </summary>
    /// <param name="semaphore">信号量实例。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>可释放的信号量包装器，释放时自动调用 Release。</returns>
    /// <example>
    /// <code>
    /// using var _ = semaphore.DisposableWait();
    /// // 临界区代码
    /// </code>
    /// </example>
    public static SemaphoreDisposer DisposableWait(this SemaphoreSlim semaphore, CancellationToken cancellationToken = default)
    {
        semaphore.Wait(cancellationToken);
        return new SemaphoreDisposer(semaphore);
    }

    /// <summary>
    /// 异步等待信号量并返回可释放的包装器。
    /// </summary>
    /// <param name="semaphore">信号量实例。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>可释放的信号量包装器，释放时自动调用 Release。</returns>
    /// <example>
    /// <code>
    /// using var _ = await semaphore.DisposableWaitAsync();
    /// // 临界区代码
    /// </code>
    /// </example>
    public static async ValueTask<SemaphoreDisposer> DisposableWaitAsync(this SemaphoreSlim semaphore, CancellationToken cancellationToken = default)
    {
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        return new SemaphoreDisposer(semaphore);
    }

    /// <summary>
    /// 信号量的可释放包装器，实现 RAII 模式自动释放信号量。
    /// </summary>
    /// <param name="semaphore">被包装的信号量。</param>
    public readonly struct SemaphoreDisposer(SemaphoreSlim semaphore) : IDisposable
    {
        private readonly SemaphoreSlim _semaphore = semaphore;

        /// <summary>
        /// 释放信号量。
        /// </summary>
        public void Dispose() => _semaphore.Release();
    }
}
