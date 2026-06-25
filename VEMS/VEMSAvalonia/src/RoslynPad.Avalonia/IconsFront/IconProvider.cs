using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RoslynPad.IconsFront.Models;

namespace RoslynPad.IconsFront
{
    /// <summary>
    /// 图标提供程序管理器，负责注册和查找图标提供程序。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此类作为图标系统的中心入口点，管理多个 <see cref="IIconProvider"/> 实例。
    /// 根据图标标识的前缀自动路由到相应的提供程序。
    /// </para>
    /// <para>
    /// 使用 <see cref="Current"/> 单例访问全局图标提供程序实例。
    /// 在应用程序启动时使用 <see cref="Register"/> 方法注册图标提供程序。
    /// </para>
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code language="csharp">
    /// // 在应用程序启动时注册提供程序
    /// IconProvider.Current
    ///     .Register&lt;FontAwesomeIconProvider&gt;()
    ///     .Register(new CustomIconProvider());
    /// 
    /// // 获取图标
    /// var icon = IconProvider.Current.GetIcon("fa-solid fa-user");
    /// </code>
    /// </example>
    /// <seealso cref="IIconProvider"/>
    /// <seealso cref="IIconProviderContainer"/>
    public class IconProvider : IIconReader, IIconProviderContainer
    {
        /// <summary>
        /// 已注册的图标提供程序列表。
        /// </summary>
        private readonly List<IIconProvider> _iconProviders = new();

        /// <summary>
        /// 获取全局图标提供程序实例。
        /// </summary>
        /// <value>单例的 <see cref="IconProvider"/> 实例。</value>
        public static IconProvider Current { get; } = new IconProvider();

        /// <inheritdoc/>
        /// <exception cref="KeyNotFoundException">
        /// 当没有注册匹配前缀的图标提供程序时抛出。
        /// </exception>
        public IconModel GetIcon(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new IconModel(new ViewBoxModel(0, 0, 0, 0), new PathModel(string.Empty));
            }

            var provider = _iconProviders
                .FirstOrDefault(p => value.StartsWith(p.Prefix, StringComparison.OrdinalIgnoreCase));

            if (provider is null)
            {
                var msg = new StringBuilder()
                    .AppendLine(CultureInfo.InvariantCulture,
                        $"没有注册前缀匹配 \"{value}\" 的 {nameof(IIconProvider)}。")
                    .AppendLine(CultureInfo.InvariantCulture,
                        $"请在应用程序启动时使用 {nameof(IconProvider)}.{nameof(Current)}.{nameof(Register)} 注册至少一个 {nameof(IIconProvider)}。")
                    .ToString();
                throw new KeyNotFoundException(msg);
            }

            return provider.GetIcon(value);
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="iconProvider"/> 为 <see langword="null"/> 时抛出。
        /// </exception>
        /// <exception cref="ArgumentException">
        /// 当新提供程序的前缀与现有提供程序冲突时抛出。
        /// </exception>
        public IIconProviderContainer Register(IIconProvider iconProvider)
        {
            ArgumentNullException.ThrowIfNull(iconProvider);

            var conflicting = _iconProviders
                .FirstOrDefault(existing => IsPrefix(existing.Prefix, iconProvider.Prefix));

            if (conflicting != null)
            {
                throw new ArgumentException(
                    $"前缀 \"{iconProvider.Prefix}\" 与现有图标提供程序前缀 \"{conflicting.Prefix}\" 冲突。");
            }

            _iconProviders.Add(iconProvider);
            return this;
        }

        /// <inheritdoc/>
        public IIconProviderContainer Register<TIconProvider>()
            where TIconProvider : IIconProvider, new()
        {
            return Register(new TIconProvider());
        }

        /// <summary>
        /// 检查两个前缀是否存在冲突（一个是另一个的前缀）。
        /// </summary>
        /// <param name="existing">现有前缀。</param>
        /// <param name="adding">要添加的前缀。</param>
        /// <returns>若存在冲突返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        private static bool IsPrefix(string existing, string adding)
        {
            return existing.StartsWith(adding, StringComparison.OrdinalIgnoreCase)
                || adding.StartsWith(existing, StringComparison.OrdinalIgnoreCase);
        }
    }
}
