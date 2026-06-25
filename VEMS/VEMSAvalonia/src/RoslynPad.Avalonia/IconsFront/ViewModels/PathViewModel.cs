using Avalonia.Media;

namespace RoslynPad.IconsFront.ViewModels
{
    /// <summary>
    /// 表示图标路径的视图模型。
    /// </summary>
    /// <remarks>
    /// 此类封装了在画布上渲染图标路径所需的位置和几何数据。
    /// </remarks>
    internal class PathViewModel
    {
        /// <summary>
        /// 初始化 <see cref="PathViewModel"/> 类的新实例。
        /// </summary>
        /// <param name="left">路径左侧位置。</param>
        /// <param name="right">路径右侧位置。</param>
        /// <param name="data">路径的几何数据。</param>
        public PathViewModel(int left, int right, StreamGeometry data)
        {
            Left = left;
            Right = right;
            Data = data;
        }

        /// <summary>
        /// 获取路径的左侧位置。
        /// </summary>
        /// <value>左侧位置坐标。</value>
        public int Left { get; }

        /// <summary>
        /// 获取路径的右侧位置。
        /// </summary>
        /// <value>右侧位置坐标。</value>
        public int Right { get; }

        /// <summary>
        /// 获取路径的几何数据。
        /// </summary>
        /// <value>用于渲染的 <see cref="StreamGeometry"/> 实例。</value>
        public StreamGeometry Data { get; }
    }
}
