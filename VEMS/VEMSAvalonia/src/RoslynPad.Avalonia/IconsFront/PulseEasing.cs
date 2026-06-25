using System.Collections.Generic;
using System.Linq;
using Avalonia.Animation.Easings;

namespace RoslynPad.IconsFront
{
    /// <summary>
    /// 脉冲动画缓动函数，模拟 Font Awesome 的 fa-pulse 效果。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此缓动函数将动画进度分为 8 个离散步骤，
    /// 创建类似于老式时钟秒针移动的效果。
    /// </para>
    /// <para>
    /// 与平滑的 <see cref="IconAnimation. Spin"/> 动画不同，
    /// 脉冲动画在视觉上更加明显和引人注目。
    /// </para>
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code language="csharp">
    /// var animation = new Animation
    /// {
    ///     Easing = new PulseEasing(),
    ///     Duration = TimeSpan.FromSeconds(1),
    ///     // ... 
    /// };
    /// </code>
    /// </example>
    public class PulseEasing : Easing
    {
        /// <summary>
        /// 动画步骤数量。
        /// </summary>
        private const int Steps = 8;

        /// <summary>
        /// 预计算的步骤阈值数组。
        /// </summary>
        private static readonly IEnumerable<double> _steps = Enumerable
            .Range(0, Steps + 1)
            .Select(index => 1.0 / Steps * index)
            .ToArray();

        /// <summary>
        /// 计算给定进度值的缓动输出。
        /// </summary>
        /// <param name="progress">动画进度，范围 0.0 到 1.0。</param>
        /// <returns>
        /// 最接近且不超过 <paramref name="progress"/> 的步骤值。
        /// 例如，进度 0.15 返回 0.125（1/8），进度 0.30 返回 0.25（2/8）。
        /// </returns>
        public override double Ease(double progress)
        {
            return _steps.Last(step => step <= progress);
        }
    }
}
