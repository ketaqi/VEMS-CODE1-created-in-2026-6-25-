namespace RoslynPad.Editor;

/// <summary>
/// 表示文档中的文本标记，用于高亮显示特定区域（如错误、警告等）。
/// </summary>
/// <remarks>
/// 继承自 <see cref="TextSegment"/>，表示文档中的一个连续区域。
/// 当标记的属性发生变化时，会自动触发重绘。
/// </remarks>
public sealed class TextMarker : TextSegment
{
    private readonly TextMarkerService _service;

    /// <summary>
    /// 初始化 <see cref="TextMarker"/> 类的新实例。
    /// </summary>
    /// <param name="service">文本标记服务。</param>
    /// <param name="startOffset">起始偏移量。</param>
    /// <param name="length">长度。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="service"/> 为 null 时抛出。</exception>
    public TextMarker(TextMarkerService service, int startOffset, int length)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        StartOffset = startOffset;
        Length = length;
    }

    /// <summary>
    /// 标记被删除时触发的事件。
    /// </summary>
    public event EventHandler? Deleted;

    /// <summary>
    /// 获取标记是否已被删除。
    /// </summary>
    public bool IsDeleted => !IsConnectedToCollection;

    /// <summary>
    /// 删除此标记。
    /// </summary>
    public void Delete()
    {
        _service.Remove(this);
    }

    /// <summary>
    /// 内部方法：标记被删除时调用。
    /// </summary>
    internal void OnDeleted()
    {
        Deleted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 请求重绘此标记区域。
    /// </summary>
    private void Redraw()
    {
        _service.Redraw(this);
    }

    private Color? _backgroundColor;

    /// <summary>
    /// 获取或设置背景颜色。
    /// </summary>
    public Color? BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (!EqualityComparer<Color?>.Default.Equals(_backgroundColor, value))
            {
                _backgroundColor = value;
                Redraw();
            }
        }
    }

    private Color? _foregroundColor;

    /// <summary>
    /// 获取或设置前景颜色。
    /// </summary>
    public Color? ForegroundColor
    {
        get => _foregroundColor;
        set
        {
            if (!EqualityComparer<Color?>.Default.Equals(_foregroundColor, value))
            {
                _foregroundColor = value;
                Redraw();
            }
        }
    }

    private FontWeight? _fontWeight;

    /// <summary>
    /// 获取或设置字体粗细。
    /// </summary>
    public FontWeight? FontWeight
    {
        get => _fontWeight;
        set
        {
            if (_fontWeight != value)
            {
                _fontWeight = value;
                Redraw();
            }
        }
    }

    private FontStyle? _fontStyle;

    /// <summary>
    /// 获取或设置字体样式。
    /// </summary>
    public FontStyle? FontStyle
    {
        get => _fontStyle;
        set
        {
            if (_fontStyle != value)
            {
                _fontStyle = value;
                Redraw();
            }
        }
    }

    /// <summary>
    /// 获取或设置标记的标签数据（可用于存储诊断信息等）。
    /// </summary>
    public object? Tag { get; set; }

    private Color _markerColor;

    /// <summary>
    /// 获取或设置标记的下划线颜色。
    /// </summary>
    public Color MarkerColor
    {
        get => _markerColor;
        set
        {
            if (!EqualityComparer<Color>.Default.Equals(_markerColor, value))
            {
                _markerColor = value;
                Redraw();
            }
        }
    }

    /// <summary>
    /// 获取或设置工具提示内容。
    /// </summary>
    public object? ToolTip { get; set; }
}
