using System;

namespace RoslynPad.IconsFront
{
    /// <summary>
    /// 定义图标提供程序容器的接口。
    /// </summary>
    /// <remarks>
    /// 此接口提供注册 <see cref="IIconProvider"/> 的方法，
    /// 支持流畅 API 风格的链式调用。
    /// </remarks>
    public interface IIconProviderContainer
    {
        /// <summary>
        /// 注册指定的图标提供程序。
        /// </summary>
        /// <param name="iconProvider">要注册的图标提供程序实例。</param>
        /// <returns>当前容器实例，支持链式调用。</returns>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="iconProvider"/> 为 <see langword="null"/> 时抛出。
        /// </exception>
        /// <exception cref="ArgumentException">
        /// 当提供程序的前缀与现有提供程序冲突时抛出。
        /// </exception>
        IIconProviderContainer Register(IIconProvider iconProvider);

        /// <summary>
        /// 注册指定类型的图标提供程序。
        /// </summary>
        /// <typeparam name="TIconProvider">
        /// 图标提供程序类型，必须实现 <see cref="IIconProvider"/> 接口并具有无参构造函数。
        /// </typeparam>
        /// <returns>当前容器实例，支持链式调用。</returns>
        /// <exception cref="ArgumentException">
        /// 当提供程序的前缀与现有提供程序冲突时抛出。
        /// </exception>
        IIconProviderContainer Register<TIconProvider>()
            where TIconProvider : IIconProvider, new();
    }
}
