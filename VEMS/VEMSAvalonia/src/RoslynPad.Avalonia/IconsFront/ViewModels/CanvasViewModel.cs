namespace RoslynPad.IconsFront.ViewModels
{
    /// <summary>
    /// 表示图标画布的视图模型。
    /// </summary>
    /// <remarks>
    /// 此类定义了图标渲染画布的尺寸，用于在视图中设置画布控件的大小。
    /// </remarks>
    internal class CanvasViewModel
    {
        /// <summary>
        /// 初始化 <see cref="CanvasViewModel"/> 类的新实例。
        /// </summary>
        /// <param name="width">画布宽度。</param>
        /// <param name="height">画布高度。</param>
        public CanvasViewModel(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// 获取画布的宽度。
        /// </summary>
        /// <value>画布宽度（像素）。</value>
        public int Width { get; }

        /// <summary>
        /// 获取画布的高度。
        /// </summary>
        /// <value>画布高度（像素）。</value>
        public int Height { get; }
    }
}
