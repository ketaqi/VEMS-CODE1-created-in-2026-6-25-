using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Controls.Shapes;
using OpticalChainSimulator.ViewModels;

namespace OpticalChainSimulator.Behaviors;

/// <summary>
/// ListBox项拖拽放置处理器
/// 继承自DropHandlerBase，实现ListBox内光学元件项的拖拽移动核心逻辑：
/// 1. 拖拽过程中显示橙色插入线指示放置位置；
/// 2. 精准计算鼠标位置对应的项插入索引；
/// 3. 执行项移动操作并同步到主窗口视图模型；
/// 4. 自动管理插入线UI的创建、更新、迁移和清理。
/// </summary>
public class ItemsListBoxDropHandler : DropHandlerBase
{
    /// <summary>
    /// 当前拖拽操作的效果（如移动、复制等），供外部行为读取判断操作类型
    /// </summary>
    public DragDropEffects DragDropEffects { get; private set; }

    /// <summary>
    /// 插入位置指示线UI元素（单例模式创建，避免重复实例化导致UI冗余）
    /// </summary>
    private Rectangle? _indicator;

    /// <summary>
    /// 当前承载插入线的Canvas容器，用于管理插入线的UI挂载位置
    /// </summary>
    private Canvas? _currentCanvas;

    /// <summary>
    /// 当前关联的ListBox控件，用于定位插入线和计算插入索引
    /// </summary>
    private ListBox? _currentListBox;

    /// <summary>
    /// 确保插入线UI元素已创建并挂载到指定Canvas容器
    /// 采用单例管理插入线，切换Canvas时自动将插入线从旧容器迁移到新容器
    /// </summary>
    /// <param name="canvas">承载插入线的目标Canvas容器</param>
    private void EnsureIndicator(Canvas canvas)
    {
        // 单例创建插入线：仅在首次调用时初始化UI属性
        if (_indicator == null)
        {
            _indicator = new Rectangle
            {
                Height = 2,               // 插入线高度
                Fill = Brushes.OrangeRed, // 插入线颜色（橙红色）
                Opacity = 0.95,           // 插入线透明度
                RadiusX = 1,              // 圆角X半径
                RadiusY = 1,              // 圆角Y半径
                [Canvas.ZIndexProperty] = 1000 // 置顶显示，避免被其他控件遮挡
            };
        }

        // 切换Canvas容器时迁移插入线
        if (_currentCanvas != canvas)
        {
            // 从旧Canvas移除插入线（避免内存泄漏和UI残留）
            if (_currentCanvas != null && _indicator.Parent == _currentCanvas)
            {
                _currentCanvas.Children.Remove(_indicator);
            }

            // 更新当前Canvas引用，并将插入线添加到新Canvas
            _currentCanvas = canvas;
            if (canvas != null && !_currentCanvas.Children.Contains(_indicator))
                _currentCanvas.Children.Add(_indicator);
        }
    }

    /// <summary>
    /// 移除并清理插入线UI元素
    /// 从当前Canvas中移除插入线，重置关联的Canvas和ListBox引用，避免UI残留
    /// </summary>
    private void RemoveIndicator()
    {
        // 安全移除插入线，防止空引用异常
        if (_indicator != null && _currentCanvas != null)
        {
            _currentCanvas.Children.Remove(_indicator);
        }
        // 重置容器引用，释放资源
        _currentCanvas = null;
        _currentListBox = null;
    }

    /// <summary>
    /// 根据鼠标位置计算ListBox中项的插入索引
    /// 核心规则：
    /// - 鼠标在项上半部分 → 插入到该项之前；
    /// - 鼠标在所有项下方 → 插入到末尾；
    /// - ListBox无项 → 插入到0位置。
    /// </summary>
    /// <param name="listBox">目标ListBox控件</param>
    /// <param name="position">鼠标相对于ListBox的坐标（用于定位插入位置）</param>
    /// <returns>计算得到的插入索引；ListBox无项集合时返回-1</returns>
    private int DetermineInsertIndex(ListBox listBox, Point position)
    {
        // 获取ListBox的项集合，为空则返回-1
        var items = listBox.Items;
        if (items == null)
            return -1;

        var count = items.Count;
        // 无项时插入索引为0
        if (count == 0)
            return 0;

        // 遍历所有项容器，计算鼠标相对位置
        for (int i = 0; i < count; i++)
        {
            // 获取索引对应的项容器控件
            var container = listBox.ItemContainerGenerator.ContainerFromIndex(i) as Control;
            if (container == null)
                continue;

            // 转换容器坐标到ListBox坐标系（确保位置计算准确）
            var transform = container.TransformToVisual(listBox);
            Point topLeft;
            if (transform.HasValue)
                topLeft = transform.Value.Transform(new Point(0, 0));
            else
                topLeft = new Point(0, 0);

            // 构建项容器的矩形区域
            var rect = new Rect(topLeft, container.Bounds.Size);

            // 计算项容器的垂直中线，判断鼠标位置
            double midY = rect.Top + rect.Height / 2.0;
            if (position.Y < midY)
            {
                // 鼠标在上半部分，插入到当前项之前
                return i;
            }
            // 鼠标在下半部分，继续遍历下一项
        }

        // 鼠标在所有项下方，插入到末尾
        return count;
    }

    /// <summary>
    /// 在指定鼠标位置显示插入线UI
    /// 自动查找名为InsertionCanvas的Canvas容器，计算插入线位置并更新其布局；
    /// 任何异常时自动清理插入线，避免UI残留或崩溃。
    /// </summary>
    /// <param name="listBox">目标ListBox控件</param>
    /// <param name="position">鼠标相对于ListBox的坐标</param>
    private void ShowInsertionAt(ListBox listBox, Point position)
    {
        try
        {
            // 第一步：查找承载插入线的Canvas容器（优先从Window级查找）
            Canvas? canvas = null;
            var visualRoot = listBox.GetVisualRoot();
            if (visualRoot is Window win)
            {
                // 查找Window中命名为InsertionCanvas的Canvas（推荐方式）
                canvas = win.FindControl<Canvas>("InsertionCanvas");
            }

            // 兜底策略：从ListBox所在命名作用域查找Canvas
            if (canvas == null)
            {
                try
                {
                    canvas = listBox.FindControl<Canvas>("InsertionCanvas");
                }
                catch
                {
                    // 忽略查找异常，保持canvas为null
                    canvas = null;
                }
            }

            // 未找到Canvas则清理插入线并返回
            if (canvas == null)
            {
                RemoveIndicator();
                return;
            }

            // 第二步：计算插入索引，无效则清理插入线
            int insertIndex = DetermineInsertIndex(listBox, position);
            if (insertIndex < 0)
            {
                RemoveIndicator();
                return;
            }

            // 第三步：确保插入线已创建并挂载到Canvas
            EnsureIndicator(canvas);
            _currentListBox = listBox;

            // 第四步：计算插入线在Canvas中的Y坐标
            double y;
            if (insertIndex >= listBox.Items.Count)
            {
                // 插入到末尾：定位到最后一个项的底部
                if (listBox.Items.Count == 0)
                {
                    y = 0;
                }
                else
                {
                    var lastContainer = listBox.ItemContainerGenerator.ContainerFromIndex(listBox.Items.Count - 1) as Control;
                    if (lastContainer == null)
                    {
                        y = listBox.Bounds.Height;
                    }
                    else
                    {
                        // 转换最后一个项的坐标到ListBox坐标系
                        var t = lastContainer.TransformToVisual(listBox);
                        Point pt;
                        if (t.HasValue)
                            pt = t.Value.Transform(new Point(0, lastContainer.Bounds.Height));
                        else
                            pt = new Point(0, lastContainer.Bounds.Height);
                        y = pt.Y;
                    }
                }
            }
            else
            {
                // 插入到指定索引位置：定位到目标项的顶部
                var targetContainer = listBox.ItemContainerGenerator.ContainerFromIndex(insertIndex) as Control;
                if (targetContainer == null)
                {
                    y = 0;
                }
                else
                {
                    // 转换目标项的坐标到ListBox坐标系
                    var t = targetContainer.TransformToVisual(listBox);
                    Point pt;
                    if (t.HasValue)
                        pt = t.Value.Transform(new Point(0, 0));
                    else
                        pt = new Point(0, 0);
                    y = pt.Y;
                }
            }

            // 第五步：转换ListBox坐标到Canvas坐标系（Canvas与ListBox位置对齐）
            var listToCanvas = listBox.TransformToVisual(canvas);
            Point canvasPoint;
            if (listToCanvas.HasValue)
                canvasPoint = listToCanvas.Value.Transform(new Point(0, y));
            else
                canvasPoint = new Point(0, y);
            double canvasY = canvasPoint.Y;

            // 钳位处理：确保插入线不超出Canvas可视区域
            canvasY = Math.Max(0, Math.Min(canvas.Bounds.Height - (_indicator?.Height ?? 2), canvasY));

            // 第六步：在UI线程更新插入线布局（避免跨线程操作异常）
            double left = 4; // 插入线左侧内边距
            double width = Math.Max(8, canvas.Bounds.Width - left * 2); // 插入线宽度（留右侧内边距）
            Dispatcher.UIThread.Post(() =>
            {
                // 安全检查：避免插入线/Canvas已被清理
                if (_indicator == null || _currentCanvas == null)
                    return;

                // 更新插入线的尺寸和位置
                _indicator.Width = width;
                Canvas.SetLeft(_indicator, left);
                Canvas.SetTop(_indicator, canvasY - (_indicator.Height / 2.0)); // 垂直居中对齐插入位置
            }, DispatcherPriority.Background);
        }
        catch
        {
            // 任何异常时强制清理插入线，防止UI卡死
            RemoveIndicator();
        }
    }

    /// <summary>
    /// 拖拽操作的内部验证与执行逻辑
    /// - 验证阶段（execute=false）：仅更新插入线显示，不执行实际移动；
    /// - 执行阶段（execute=true）：验证通过后执行项移动，同步到VM并清理状态。
    /// </summary>
    /// <param name="listBox">目标ListBox控件</param>
    /// <param name="e">拖拽事件参数（包含鼠标位置、拖拽效果等）</param>
    /// <param name="sourceContext">拖拽源上下文（应为OpticalElementViewModel）</param>
    /// <param name="vm">主窗口视图模型（承载光学元件集合，处理项移动）</param>
    /// <param name="execute">是否执行实际移动操作（true=执行，false=仅验证）</param>
    /// <returns>验证/执行成功返回true，失败返回false</returns>
    private bool ValidateInternal(ListBox listBox, DragEventArgs e, object? sourceContext, MainWindowViewModel? vm, bool execute)
    {
        // 验证1：拖拽源必须是OpticalElementViewModel，且VM不能为空
        if (sourceContext is not OpticalElementViewModel sourceItem || vm == null)
            return false;

        // 验证2：拖拽源必须存在于VM的元件集合中
        var items = vm.Elements;
        var sourceIndex = items.IndexOf(sourceItem);
        if (sourceIndex < 0)
            return false;

        // 验证3：鼠标位置必须有效（非NaN）
        var pos = e.GetPosition(listBox);
        if (double.IsNaN(pos.X) || double.IsNaN(pos.Y))
            return false;

        // 验证4：插入索引必须有效（0 ~ 项总数）
        int insertIndex = DetermineInsertIndex(listBox, pos);
        if (insertIndex < 0 || insertIndex > items.Count)
            return false;

        // 仅验证阶段：更新插入线显示，直接返回成功
        if (!execute)
        {
            ShowInsertionAt(listBox, pos);
            return true;
        }

        // 执行阶段：处理项移动逻辑
        // 1. 记录当前拖拽效果（供外部行为判断）
        DragDropEffects = e.DragEffects;
        // 2. 调用VM的MoveElement方法，处理项移动（VM内部处理索引偏移）
        vm.MoveElement(sourceIndex, insertIndex);

        // 3. 延迟清理拖拽效果（允许外部监听器读取后再重置）
        Dispatcher.UIThread.Post(() =>
        {
            DragDropEffects = DragDropEffects.None;
        }, DispatcherPriority.Background);

        // 4. 清理插入线UI
        RemoveIndicator();

        return true;
    }

    /// <summary>
    /// 重写基类验证方法，处理拖拽过程中的实时验证逻辑
    /// 验证通过后更新插入线显示，验证失败则立即清理插入线
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">拖拽事件参数</param>
    /// <param name="sourceContext">拖拽源上下文（应为OpticalElementViewModel）</param>
    /// <param name="targetContext">拖拽目标上下文（应为MainWindowViewModel）</param>
    /// <param name="state">拖拽状态（未使用）</param>
    /// <returns>验证通过返回true，否则返回false</returns>
    public override bool Validate(
        object? sender,
        DragEventArgs e,
        object? sourceContext,
        object? targetContext,
        object? state)
    {
        // 基础验证：事件源必须是Control，发送者必须是ListBox
        if (!(e.Source is Control) || sender is not ListBox listBox)
        {
            RemoveIndicator();
            return false;
        }

        // 转换目标上下文为主窗口VM，执行内部验证（仅验证，不执行移动）
        var vm = targetContext as MainWindowViewModel;
        return ValidateInternal(listBox, e, sourceContext, vm, false);
    }

    /// <summary>
    /// 重写基类执行方法，处理拖拽完成后的项移动逻辑
    /// 执行成功后同步更新VM的元件顺序，清理插入线和拖拽状态；失败则清理UI残留
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">拖拽事件参数</param>
    /// <param name="sourceContext">拖拽源上下文（应为OpticalElementViewModel）</param>
    /// <param name="targetContext">拖拽目标上下文（应为MainWindowViewModel）</param>
    /// <param name="state">拖拽状态（未使用）</param>
    /// <returns>执行成功返回true，否则返回false</returns>
    public override bool Execute(
        object? sender,
        DragEventArgs e,
        object? sourceContext,
        object? targetContext,
        object? state)
    {
        // 基础验证：事件源必须是Control，发送者必须是ListBox
        if (!(e.Source is Control) || sender is not ListBox listBox)
        {
            RemoveIndicator();
            return false;
        }

        // 转换目标上下文为主窗口VM，执行内部验证（执行实际移动操作）
        var vm = targetContext as MainWindowViewModel;
        return ValidateInternal(listBox, e, sourceContext, vm, true);
    }
}