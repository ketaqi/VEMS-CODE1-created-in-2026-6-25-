using System;
using Avalonia.Reactive;

namespace RoslynPad.IconsFront
{
    /// <summary>
    /// 提供 <see cref="IObservable{T}"/> 的扩展方法。
    /// </summary>
    public static class ObservableExtensions
    {
        /// <summary>
        /// 订阅可观察序列，仅处理 OnNext 通知。
        /// </summary>
        /// <typeparam name="T">序列元素的类型。</typeparam>
        /// <param name="observable">要订阅的可观察序列。</param>
        /// <param name="onNext">处理每个元素的委托。</param>
        /// <returns>用于取消订阅的 <see cref="IDisposable"/> 对象。</returns>
        /// <remarks>
        /// 此扩展方法简化了仅需处理 OnNext 通知的订阅场景，
        /// 使用 <see cref="AnonymousObserver{T}"/> 包装委托。
        /// </remarks>
        public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<T> onNext)
            => observable.Subscribe(new AnonymousObserver<T>(onNext));
    }
}
