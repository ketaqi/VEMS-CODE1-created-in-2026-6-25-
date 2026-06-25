using System.Runtime.CompilerServices;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

#pragma warning disable IDE0060 // Remove unused parameter

namespace RoslynPad.Editor;

/// <summary>
/// Avalonia 控件通用扩展方法类
/// 提供控件树查找、窗口操作、样式处理、事件绑定等通用扩展
/// </summary>
internal static class AvaloniaExtensions
{
    /// <summary>
    /// 从当前控件向上查找指定类型的父级控件
    /// </summary>
    /// <typeparam name="T">目标父控件类型（继承自 Control）</typeparam>
    /// <param name="control">当前控件</param>
    /// <returns>找到的父控件实例，未找到则返回 null</returns>
    public static T? FindAncestorByType<T>(this Control control)
        where T : Control
    {
        Control? result = control;

        while (result != null && result is not T)
        {
            result = result.Parent as Control;
        }

        return result as T;
    }

    /// <summary>
    /// 获取当前控件所在的窗口实例
    /// </summary>
    /// <param name="c">当前控件</param>
    /// <returns>窗口实例，未找到则返回 null</returns>
    public static Window? GetWindow(this Control c) => c.FindAncestorByType<Window>();

    /// <summary>
    /// 获取控件的 UI 线程调度器
    /// </summary>
    /// <param name="o">当前控件</param>
    /// <returns>UI 线程的 Dispatcher 实例</returns>
    public static Dispatcher GetDispatcher(this Control o) => Dispatcher.UIThread;

    /// <summary>
    /// 获取控件的渲染尺寸（宽高）
    /// </summary>
    /// <param name="element">目标控件</param>
    /// <returns>控件的渲染尺寸（Size 结构体）</returns>
    public static Size GetRenderSize(this Control element) => element.Bounds.Size;

    /// <summary>
    /// 为控件绑定加载/卸载事件的统一处理方法
    /// 控件附加到视觉树时执行 action(true)，脱离时执行 action(false)
    /// </summary>
    /// <param name="element">目标控件</param>
    /// <param name="action">加载/卸载时的回调方法（参数：true=加载，false=卸载）</param>
    public static void HookupLoadedUnloadedAction(this Control element, Action<bool> action)
    {
        if (element.IsAttachedToVisualTree())
        {
            action(true);
        }

        element.AttachedToVisualTree += (o, e) => action(true);
        element.DetachedFromVisualTree += (o, e) => action(false);
    }

    /// <summary>
    /// 为窗口绑定位置变更事件
    /// </summary>
    /// <param name="topLevel">目标窗口</param>
    /// <param name="handler">位置变更事件的处理方法</param>
    public static void AttachLocationChanged(this Window topLevel, EventHandler<PixelPointEventArgs> handler)
    {
        topLevel.PositionChanged += handler;
    }

    /// <summary>
    /// 为窗口解绑位置变更事件
    /// </summary>
    /// <param name="topLevel">目标窗口</param>
    /// <param name="handler">要解绑的事件处理方法</param>
    public static void DetachLocationChanged(this Window topLevel, EventHandler<PixelPointEventArgs> handler)
    {
        topLevel.PositionChanged -= handler;
    }

    /// <summary>
    /// 将画刷转换为不可变（冻结）状态
    /// </summary>
    /// <param name="freezable">目标画刷</param>
    /// <returns>不可变的画刷实例</returns>
    public static IBrush AsFrozen(this IBrush freezable)
    {
        return freezable.ToImmutable();
    }

    /// <summary>
    /// 冻结画笔（空实现，适配跨平台逻辑）
    /// </summary>
    /// <param name="pen">目标画笔</param>
    public static void Freeze(this Pen pen)
    {
        // nop
    }

    /// <summary>
    /// 冻结几何图形（空实现，适配跨平台逻辑）
    /// </summary>
    /// <param name="geometry">目标几何图形</param>
    public static void Freeze(this Geometry geometry)
    {
        // nop
    }

    /// <summary>
    /// 向流几何上下文批量添加折线点
    /// </summary>
    /// <param name="context">流几何上下文</param>
    /// <param name="points">折线点集合</param>
    /// <param name="isStroked">是否描边（参数保留，适配接口）</param>
    /// <param name="isSmoothJoin">是否平滑连接（参数保留，适配接口）</param>
    public static void PolyLineTo(this StreamGeometryContext context, IList<Point> points, bool isStroked, bool isSmoothJoin)
    {
        foreach (var point in points)
        {
            context.LineTo(point);
        }
    }

    /// <summary>
    /// 设置模板控件的边框厚度（统一值）
    /// </summary>
    /// <param name="control">目标模板控件</param>
    /// <param name="thickness">边框厚度值</param>
    public static void SetBorderThickness(this TemplatedControl control, double thickness)
    {
        control.BorderThickness = new Thickness(thickness);
    }

    /// <summary>
    /// 关闭弹窗根窗口（适配 Hide 方法）
    /// </summary>
    /// <param name="window">弹窗根窗口实例</param>
    public static void Close(this PopupRoot window) => window.Hide();

    /// <summary>
    /// 判断键盘事件是否包含指定的修饰键
    /// </summary>
    /// <param name="args">键盘事件参数</param>
    /// <param name="modifier">要判断的修饰键</param>
    /// <returns>包含指定修饰键则返回 true，否则返回 false</returns>
    public static bool HasModifiers(this KeyEventArgs args, KeyModifiers modifier) =>
        (args.KeyModifiers & modifier) == modifier;

    /// <summary>
    /// 打开控件的工具提示
    /// </summary>
    /// <param name="toolTip">目标工具提示实例</param>
    /// <param name="control">关联的控件</param>
    public static void Open(this ToolTip toolTip, Control control) => ToolTip.SetIsOpen(control, true);

    /// <summary>
    /// 关闭控件的工具提示
    /// </summary>
    /// <param name="toolTip">目标工具提示实例</param>
    /// <param name="control">关联的控件</param>
    public static void Close(this ToolTip toolTip, Control control) => ToolTip.SetIsOpen(control, false);

    /// <summary>
    /// 设置控件工具提示的内容
    /// </summary>
    /// <param name="toolTip">目标工具提示实例</param>
    /// <param name="control">关联的控件</param>
    /// <param name="content">提示内容</param>
    public static void SetContent(this ToolTip toolTip, Control control, object content) => ToolTip.SetTip(control, content);

    /// <summary>
    /// 在指定控件位置打开浮出层
    /// </summary>
    /// <param name="flyout">目标浮出层实例</param>
    /// <param name="control">关联的控件</param>
    public static void Open(this FlyoutBase flyout, Control control) => flyout.ShowAt(control);

    /// <summary>
    /// 获取调度器的等待器（用于异步等待调度器上下文）
    /// </summary>
    /// <param name="dispatcher">目标调度器</param>
    /// <returns>调度器等待器实例</returns>
    public static DispatcherYieldAwaiter GetAwaiter(this Dispatcher dispatcher) => new(dispatcher, default);

    /// <summary>
    /// 调度器异步等待器结构体
    /// 用于等待调度器上下文的切换，确保代码在指定调度器线程执行
    /// </summary>
    /// <param name="dispatcher">目标调度器</param>
    /// <param name="priority">调度优先级</param>
    public readonly struct DispatcherYieldAwaiter(Dispatcher dispatcher, DispatcherPriority priority) : ICriticalNotifyCompletion
    {
        /// <summary>
        /// 判断是否已在调度器线程（无需等待）
        /// </summary>
        public bool IsCompleted => dispatcher.CheckAccess();

        /// <summary>
        /// 等待完成后验证调度器访问权限
        /// </summary>
        public void GetResult() => dispatcher.VerifyAccess();

        /// <summary>
        /// 注册完成后的回调方法（发布到调度器）
        /// </summary>
        /// <param name="continuation">回调方法</param>
        public void OnCompleted(Action continuation) => dispatcher.Post(continuation, priority);

        /// <summary>
        /// 非安全方式注册完成回调（同 OnCompleted）
        /// </summary>
        /// <param name="continuation">回调方法</param>
        public void UnsafeOnCompleted(Action continuation) => OnCompleted(continuation);
    }
}
