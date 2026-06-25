using System.Diagnostics;

namespace RoslynPad.Editor;

/// <summary>
/// 文本标记服务，管理文档中的所有标记并负责渲染。
/// </summary>
/// <remarks>
/// 此类实现了三个接口：
/// <list type="bullet">
/// <item><description><see cref="DocumentColorizingTransformer"/>：用于着色文本</description></item>
/// <item><description><see cref="IBackgroundRenderer"/>：用于绘制背景和波浪线</description></item>
/// <item><description><see cref="ITextViewConnect"/>：用于连接到 TextView</description></item>
/// </list>
/// </remarks>
public sealed class TextMarkerService : DocumentColorizingTransformer, IBackgroundRenderer, ITextViewConnect
{
    #region Fields

    private readonly TextSegmentCollection<TextMarker>? _markers;
    private readonly TextDocument _document;
    private readonly List<TextView> _textViews;

    #endregion

    #region Constructors

    /// <summary>
    /// 初始化 <see cref="TextMarkerService"/> 类的新实例。
    /// </summary>
    /// <param name="editor">代码编辑器实例。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="editor"/> 为 null 时抛出。</exception>
    public TextMarkerService(CodeTextEditor editor)
    {
        ArgumentNullException.ThrowIfNull(editor);
        _document = editor.Document;
        _markers = new TextSegmentCollection<TextMarker>(_document);
        _textViews = [];
        editor.ToolTipRequest += EditorOnToolTipRequest;
    }

    /// <summary>
    /// 处理工具提示请求。
    /// </summary>
    private void EditorOnToolTipRequest(object? sender, ToolTipRequestEventArgs args)
    {
        var offset = _document.GetOffset(args.LogicalPosition);

        var markersAtOffset = GetMarkersAtOffset(offset);
        var markerWithToolTip = markersAtOffset.FirstOrDefault(marker => marker.ToolTip != null);
        if (markerWithToolTip != null && markerWithToolTip.ToolTip != null)
        {
            args.SetToolTip(markerWithToolTip.ToolTip);
        }
    }

    #endregion

    #region TextMarkerService

    /// <summary>
    /// 尝试在指定位置创建文本标记。
    /// </summary>
    /// <param name="startOffset">起始偏移量。</param>
    /// <param name="length">长度。</param>
    /// <returns>创建的标记，如果位置无效则返回 null。</returns>
    public TextMarker? TryCreate(int startOffset, int length)
    {
        if (_markers == null)
            throw new InvalidOperationException("Cannot create a marker when not attached to a document");

        var textLength = _document.TextLength;
        if (startOffset < 0 || startOffset > textLength) return null;
        if (length < 0 || startOffset + length > textLength) return null;

        var marker = new TextMarker(this, startOffset, length);
        _markers.Add(marker);
        return marker;
    }

    /// <summary>
    /// 获取指定偏移位置的所有标记。
    /// </summary>
    /// <param name="offset">偏移量。</param>
    /// <returns>包含该偏移位置的所有标记。</returns>
    public IEnumerable<TextMarker> GetMarkersAtOffset(int offset)
    {
        return _markers == null ? [] : _markers.FindSegmentsContaining(offset);
    }

    /// <summary>
    /// 获取所有文本标记。
    /// </summary>
    public IEnumerable<TextMarker> TextMarkers => _markers ?? Enumerable.Empty<TextMarker>();

    /// <summary>
    /// 清除所有标记。
    /// </summary>
    public void Clear() => _markers?.Clear();

    /// <summary>
    /// 移除满足条件的所有标记。
    /// </summary>
    /// <param name="predicate">过滤条件。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="predicate"/> 为 null 时抛出。</exception>
    public void RemoveAll(Predicate<TextMarker> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (_markers != null)
        {
            foreach (var m in _markers.ToArray())
            {
                if (predicate(m))
                    Remove(m);
            }
        }
    }

    /// <summary>
    /// 移除指定的标记。
    /// </summary>
    /// <param name="marker">要移除的标记。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="marker"/> 为 null 时抛出。</exception>
    public void Remove(TextMarker marker)
    {
        ArgumentNullException.ThrowIfNull(marker);
        var m = marker;
        if (_markers != null && _markers.Remove(m))
        {
            Redraw(m);
            m.OnDeleted();
        }
    }

    /// <summary>
    /// 请求重绘指定区域。
    /// </summary>
    /// <param name="segment">要重绘的区域。</param>
    internal void Redraw(ISegment segment)
    {
        foreach (var view in _textViews)
        {
            view.Redraw(segment);
        }
        RedrawRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 重绘请求事件。
    /// </summary>
    public event EventHandler? RedrawRequested;

    #endregion

    #region DocumentColorizingTransformer

    /// <summary>
    /// 着色指定行。
    /// </summary>
    /// <param name="line">要着色的行。</param>
    protected override void ColorizeLine(DocumentLine line)
    {
        if (_markers == null)
            return;
        var lineStart = line.Offset;
        var lineEnd = lineStart + line.Length;
        foreach (var marker in _markers.FindOverlappingSegments(lineStart, line.Length))
        {
            CommonBrush? foregroundBrush = null;
            if (marker.ForegroundColor != null)
            {
                foregroundBrush = new SolidColorBrush(marker.ForegroundColor.Value).AsFrozen();
            }
            ChangeLinePart(
                Math.Max(marker.StartOffset, lineStart),
                Math.Min(marker.EndOffset, lineEnd),
                element =>
                {
                    if (foregroundBrush != null)
                    {
                        element.TextRunProperties.SetForegroundBrush(foregroundBrush);
                    }
                    var tf = element.TextRunProperties.Typeface;
                    element.TextRunProperties.SetTypeface(new Typeface(
                        tf.FontFamily,
                        marker.FontStyle ?? tf.Style,
                        marker.FontWeight ?? tf.Weight,
                        tf.Stretch
                    ));
                }
            );
        }
    }

    #endregion

    #region IBackgroundRenderer

    /// <summary>
    /// 获取渲染层级。
    /// </summary>
    public KnownLayer Layer => KnownLayer.Selection;

    /// <summary>
    /// 绘制背景和波浪线标记。
    /// </summary>
    /// <param name="textView">文本视图。</param>
    /// <param name="drawingContext">绘图上下文。</param>
    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        ArgumentNullException.ThrowIfNull(textView);
        ArgumentNullException.ThrowIfNull(drawingContext);
        if (_markers == null || !textView.VisualLinesValid)
            return;
        var visualLines = textView.VisualLines;
        if (visualLines.Count == 0)
            return;
        var viewStart = visualLines.First().FirstDocumentLine.Offset;
        var viewEnd = visualLines.Last().LastDocumentLine.EndOffset;
        foreach (var marker in _markers.FindOverlappingSegments(viewStart, viewEnd - viewStart))
        {
            if (marker.BackgroundColor != null)
            {
                var geoBuilder = new BackgroundGeometryBuilder
                {
                    AlignToWholePixels = true,
                    CornerRadius = 3
                };
                geoBuilder.AddSegment(textView, marker);
                var geometry = geoBuilder.CreateGeometry();
                if (geometry != null)
                {
                    var color = marker.BackgroundColor.Value;
                    var brush = new SolidColorBrush(color).AsFrozen();
                    drawingContext.DrawGeometry(brush, null, geometry);
                }
            }
            foreach (var r in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
            {
                var startPoint = r.BottomLeft;
                var endPoint = r.BottomRight;

                var usedBrush = new SolidColorBrush(marker.MarkerColor).AsFrozen();
                var offset = 2.5;

                var count = Math.Max((int)((endPoint.X - startPoint.X) / offset) + 1, 4);

                var geometry = new StreamGeometry();

                using (var ctx = geometry.Open())
                {
                    ctx.BeginFigure(startPoint, false);
                    ctx.PolyLineTo(CreatePoints(startPoint, offset, count).ToArray(), true, false);
                }

                geometry.Freeze();

                var usedPen = new Pen(usedBrush, 1);
                usedPen.Freeze();
                drawingContext.DrawGeometry(Brushes.Transparent, usedPen, geometry);
            }
        }
    }

    /// <summary>
    /// 创建波浪线的点序列。
    /// </summary>
    private static IEnumerable<Point> CreatePoints(Point start, double offset, int count)
    {
        for (var i = 0; i < count; i++)
            yield return new Point(start.X + i * offset, start.Y - ((i + 1) % 2 == 0 ? offset : 0));
    }

    #endregion

    #region ITextViewConnect

    /// <summary>
    /// 添加到 TextView。
    /// </summary>
    void ITextViewConnect.AddToTextView(TextView textView)
    {
        if (textView != null && !_textViews.Contains(textView))
        {
            Debug.Assert(textView.Document == _document);
            _textViews.Add(textView);
        }
    }

    /// <summary>
    /// 从 TextView 移除。
    /// </summary>
    void ITextViewConnect.RemoveFromTextView(TextView textView)
    {
        if (textView != null)
        {
            Debug.Assert(textView.Document == _document);
            _textViews.Remove(textView);
        }
    }

    #endregion
}
