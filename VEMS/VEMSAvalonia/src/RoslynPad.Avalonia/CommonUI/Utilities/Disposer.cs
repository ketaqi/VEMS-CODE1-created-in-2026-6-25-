namespace RoslynPad.UI.Utilities;

/// <summary>
/// 一次性资源包装器：将任意清理操作封装为 <see cref="IDisposable"/> 对象。
/// </summary>
/// <remarks>
/// <para>
/// 此类提供了一种便捷的方式，将回调函数包装为 <see cref="IDisposable"/>，
/// 使得任何清理逻辑都可以通过 <c>using</c> 语句或显式调用 <see cref="Dispose"/> 来执行。
/// </para>
/// <para>
/// 典型使用场景：
/// <list type="bullet">
///   <item><description>实现 <see cref="IObservable{T}.Subscribe"/> 的返回值，用于取消订阅</description></item>
///   <item><description>将事件解绑逻辑封装为可释放对象</description></item>
///   <item><description>在 <c>using</c> 块中执行临时状态的恢复</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 用于订阅取消
/// public IDisposable Subscribe(IObserver&lt;T&gt; observer)
/// {
///     _observers.Add(observer);
///     return new Disposer(() => _observers.Remove(observer));
/// }
/// 
/// // 用于临时状态恢复
/// var oldValue = SomeProperty;
/// SomeProperty = newValue;
/// using var restore = new Disposer(() => SomeProperty = oldValue);
/// // ... 执行操作 ...
/// // 离开作用域时自动恢复 SomeProperty
/// 
/// // 用于事件解绑
/// button.Click += handler;
/// var unsubscribe = new Disposer(() => button.Click -= handler);
/// // 稍后调用 unsubscribe.Dispose() 解绑事件
/// </code>
/// </example>
/// <param name="onDispose">
/// 在调用 <see cref="Dispose"/> 时执行的清理回调。
/// 此回调仅执行一次（如果多次调用 <see cref="Dispose"/>，每次都会执行）。
/// </param>
internal sealed class Disposer(Action onDispose) : IDisposable
{
    /// <summary>
    /// 要在释放时执行的清理操作。
    /// </summary>
    private readonly Action _onDispose = onDispose;

    /// <summary>
    /// 执行清理操作。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 调用此方法将执行构造时传入的清理回调。
    /// </para>
    /// <para>
    /// 注意：此实现不是线程安全的，也不保证幂等性。
    /// 如果需要确保只执行一次，调用方应自行管理。
    /// </para>
    /// </remarks>
    public void Dispose() => _onDispose?.Invoke();
}
