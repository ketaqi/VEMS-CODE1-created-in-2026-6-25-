namespace RoslynPad.IconsFront
{
    /// <summary>
    /// 定义图标的动画类型。
    /// </summary>
    /// <remarks>
    /// 这些动画效果模仿 Font Awesome 的 CSS 动画类。
    /// </remarks>
    public enum IconAnimation
    {
        /// <summary>
        /// 无动画。
        /// </summary>
        None,

        /// <summary>
        /// 连续旋转动画（平滑旋转）。
        /// </summary>
        /// <remarks>
        /// 类似于 Font Awesome 的 <c>fa-spin</c> 类，图标以恒定速度连续旋转。
        /// 适用于加载指示器等场景。
        /// </remarks>
        Spin,

        /// <summary>
        /// 脉冲旋转动画（8 步旋转）。
        /// </summary>
        /// <remarks>
        /// 类似于 Font Awesome 的 <c>fa-pulse</c> 类，图标以 8 步递进方式旋转。
        /// 适用于需要更明显视觉反馈的场景。
        /// </remarks>
        Pulse
    }
}
