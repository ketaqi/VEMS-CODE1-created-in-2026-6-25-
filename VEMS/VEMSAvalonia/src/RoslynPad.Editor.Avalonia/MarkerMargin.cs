namespace RoslynPad.Editor;

/// <summary>
/// 标记边距控件，用于在编辑器边距中显示自定义标记（如图标、提示信息等）
/// </summary>
public class MarkerMargin : AbstractMargin
{
    /// <summary>
    /// 静态构造函数，注册LineNumber属性变更时的类处理程序
    /// </summary>
    static MarkerMargin()
    {
        LineNumberProperty.Changed.AddClassHandler<MarkerMargin>((o, e) => o.InvalidateArrange());
    }

    /// <summary>
    /// 初始化<see cref="MarkerMargin"/>类的新实例
    /// </summary>
    public MarkerMargin()
    {
        Marker = CreateMarker();
    }

    /// <summary>
    /// 标记被鼠标指针按下时触发的事件
    /// </summary>
    public event EventHandler? MarkerPointerDown;

    /// <summary>
    /// 创建标记控件（Image）并初始化相关事件和绑定
    /// </summary>
    /// <returns>初始化后的Image标记控件</returns>
    private Image CreateMarker()
    {
        var marker = new Image();
        // 处理标记的指针按下事件，标记为已处理并触发自定义事件
        marker.PointerPressed += (o, e) => { e.Handled = true; MarkerPointerDown?.Invoke(o, e); };
        // 绑定标记图片源到MarkerImage属性
        marker[~Image.SourceProperty] = this[~MarkerImageProperty];
        // 绑定标记提示文本到Message属性
        marker[~ToolTip.TipProperty] = this[~MessageProperty];
        VisualChildren.Add(marker);
        LogicalChildren.Add(marker);
        return marker;
    }

    /// <summary>
    /// 标识LineNumber依赖属性
    /// </summary>
    public static readonly StyledProperty<int?> LineNumberProperty =
        AvaloniaProperty.Register<MarkerMargin, int?>(nameof(LineNumber));

    /// <summary>
    /// 获取或设置标记关联的行号
    /// </summary>
    public int? LineNumber
    {
        get => GetValue(LineNumberProperty);
        set => SetValue(LineNumberProperty, value);
    }

    /// <summary>
    /// 标识Message依赖属性
    /// </summary>
    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<MarkerMargin, string>(nameof(Message));

    /// <summary>
    /// 获取或设置标记的提示信息
    /// </summary>
    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>
    /// 标识MarkerImage依赖属性
    /// </summary>
    public static readonly StyledProperty<IImage?> MarkerImageProperty =
        AvaloniaProperty.Register<MarkerMargin, IImage?>(nameof(MarkerImage));

    /// <summary>
    /// 获取或设置标记显示的图片
    /// </summary>
    public IImage? MarkerImage
    {
        get => GetValue(MarkerImageProperty);
        set => SetValue(MarkerImageProperty, value);
    }

    /// <summary>
    /// 获取标记控件实例
    /// </summary>
    public Control Marker { get; }

    /// <summary>
    /// 当TextView关联的控件发生变更时调用
    /// </summary>
    /// <param name="oldTextView">旧的TextView实例</param>
    /// <param name="newTextView">新的TextView实例</param>
    protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
    {
        if (oldTextView != null)
        {
            oldTextView.VisualLinesChanged -= TextViewVisualLinesChanged;
        }
        base.OnTextViewChanged(oldTextView, newTextView);
        if (newTextView != null)
        {
            newTextView.VisualLinesChanged += TextViewVisualLinesChanged;
        }

        InvalidateArrange();
    }

    /// <summary>
    /// 处理TextView视觉行变更事件，触发排列失效
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void TextViewVisualLinesChanged(object? sender, EventArgs e)
    {
        InvalidateArrange();
    }

    /// <summary>
    /// 测量控件所需大小（重写Avalonia布局方法）
    /// </summary>
    /// <param name="availableSize">可用大小</param>
    /// <returns>控件测量后的大小</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        Marker.Measure(availableSize);
        return new Size();
    }

    /// <summary>
    /// 排列控件子元素（重写Avalonia布局方法）
    /// </summary>
    /// <param name="finalSize">最终可用大小</param>
    /// <returns>控件排列后的大小</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var lineNumber = LineNumber;
        var textView = TextView;
        // 如果行号有效且能获取到对应的视觉行，则显示标记并定位
        if (lineNumber != null && textView?.GetVisualLine(lineNumber.Value) is VisualLine line)
        {
            Marker.IsVisible = true;
            var visualYPosition = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop);
            Marker.Arrange(new Rect(
                new Point(0, visualYPosition - textView.VerticalOffset),
                new Size(finalSize.Width, finalSize.Width)));
        }
        else
        {
            // 行号无效或无对应视觉行时隐藏标记
            Marker.IsVisible = false;
        }

        return finalSize;
    }
}
