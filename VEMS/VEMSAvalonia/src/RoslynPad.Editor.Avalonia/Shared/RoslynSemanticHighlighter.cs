using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using RoslynPad.Roslyn;

namespace RoslynPad.Editor;

/// <summary>
/// Roslyn 语义高亮器，提供基于 Roslyn 分析的语法着色。
/// </summary>
/// <remarks>
/// 此类实现异步语义高亮：
/// <list type="bullet">
/// <item><description>使用节流（Throttle）避免频繁请求</description></item>
/// <item><description>缓存已高亮的行以提高性能</description></item>
/// <item><description>支持增量更新，避免闪烁</description></item>
/// </list>
/// </remarks>
internal sealed class RoslynSemanticHighlighter : IHighlighter
{
    private const int CacheSize = 512;
    private const int DelayInMs = 100;
    private readonly TextView _textView;
    private readonly IDocument _document;
    private readonly DocumentId _documentId;
    private readonly IRoslynHost _roslynHost;
    private readonly IClassificationHighlightColors _highlightColors;
    private readonly List<CachedLine>? _cachedLines;
    private readonly Subject<FrozenLine> _subject;
    private readonly List<(FrozenLine line, List<HighlightedSection> sections)> _changes;
    private readonly SynchronizationContext? _syncContext;

    private volatile bool _inHighlightingGroup;
    private int? _updatedLine;

    /// <summary>
    /// 初始化 <see cref="RoslynSemanticHighlighter"/> 类的新实例。
    /// </summary>
    /// <param name="textView">文本视图。</param>
    /// <param name="document">文档。</param>
    /// <param name="documentId">Roslyn 文档 ID。</param>
    /// <param name="roslynHost">Roslyn 宿主。</param>
    /// <param name="highlightColors">高亮颜色配置。</param>
    public RoslynSemanticHighlighter(TextView textView, IDocument document, DocumentId documentId, IRoslynHost roslynHost, IClassificationHighlightColors highlightColors)
    {
        _textView = textView ?? throw new ArgumentNullException(nameof(textView));
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _documentId = documentId;
        _roslynHost = roslynHost;
        _highlightColors = highlightColors;
        _subject = new Subject<FrozenLine>();
        _subject.GroupBy(c => c.LineNumber).Subscribe(SubscribeToLineGroup);

        if (document is TextDocument)
        {
            // 仅对实时编辑的文档使用缓存
            // 只读文档（如搜索结果）不需要缓存
            _cachedLines = [];
        }

        _changes = [];
        _syncContext = SynchronizationContext.Current;
    }

    /// <summary>
    /// 释放资源。
    /// </summary>
    public void Dispose()
    {
        _subject.Dispose();
    }

    /// <summary>
    /// 高亮状态变化事件。
    /// </summary>
    public event HighlightingStateChangedEventHandler? HighlightingStateChanged;

    /// <summary>
    /// 更新高亮区段。
    /// </summary>
    private void UpdateHighlightingSections(FrozenLine line, List<HighlightedSection> sections)
    {
        if (_inHighlightingGroup && line.LineNumber == _updatedLine)
        {
            lock (_changes)
            {
                _changes.Add((line, sections));
            }
            return;
        }

        _syncContext?.Post(o => UpdateHighlightingSectionsNoCheck(line, sections), null);
    }

    /// <summary>
    /// 无检查地更新高亮区段（在 UI 线程调用）。
    /// </summary>
    private void UpdateHighlightingSectionsNoCheck(FrozenLine line, List<HighlightedSection> sections)
    {
        if (!IsCurrentLine(line))
        {
            return;
        }

        var lineNumber = line.LineNumber;

        line.HighlightedLine.Sections.Clear();
        foreach (var section in sections)
        {
            line.HighlightedLine.Sections.Add(section);
        }

        if (_textView.GetVisualLine(line.LineNumber) != null)
        {
            HighlightingStateChanged?.Invoke(lineNumber, lineNumber);
        }
    }

    /// <summary>
    /// 检查行是否为当前有效行。
    /// </summary>
    private bool IsCurrentLine(FrozenLine line)
    {
        return !line.IsDeleted &&
               line.HighlightedLine.Document.Version.CompareAge(_document.Version) == 0 &&
               line.LineNumber <= _document.LineCount &&
               _document.GetLineByNumber(line.LineNumber) is var currentLine &&
               currentLine?.Length == line.Length;
    }

    /// <summary>
    /// 获取关联的文档。
    /// </summary>
    IDocument IHighlighter.Document => _document;

    /// <summary>
    /// 获取颜色栈（不支持，返回 null）。
    /// </summary>
    IEnumerable<HighlightingColor>? IHighlighter.GetColorStack(int lineNumber) => null;

    /// <summary>
    /// 更新高亮状态。
    /// </summary>
    /// <param name="lineNumber">行号。</param>
    public void UpdateHighlightingState(int lineNumber)
    {
        if (_inHighlightingGroup && _updatedLine == null)
        {
            _updatedLine = lineNumber;
        }
    }

    /// <summary>
    /// 高亮指定行。
    /// </summary>
    /// <param name="lineNumber">行号。</param>
    /// <returns>高亮后的行对象。</returns>
    public HighlightedLine HighlightLine(int lineNumber)
    {
        var documentLine = _document.GetLineByNumber(lineNumber);
        var newVersion = _document.Version;
        CachedLine? cachedLine = null;
        if (_cachedLines != null)
        {
            for (var i = 0; i < _cachedLines.Count; i++)
            {
                var line = _cachedLines[i];
                if (line.DocumentLine != documentLine) continue;
                if (newVersion == null || !newVersion.BelongsToSameDocumentAs(line.OldVersion))
                {
                    _cachedLines.RemoveAt(i);
                }
                else
                {
                    cachedLine = line;
                }
            }

            if (cachedLine != null && cachedLine.IsValid && newVersion?.CompareAge(cachedLine.OldVersion) == 0 &&
                cachedLine.DocumentLine.Length == documentLine.Length)
            {
                return cachedLine.HighlightedLine;
            }
        }

        var wasInHighlightingGroup = _inHighlightingGroup;
        if (!_inHighlightingGroup)
        {
            BeginHighlighting();
        }
        try
        {
            return DoHighlightLine(documentLine, cachedLine);
        }
        finally
        {
            if (!wasInHighlightingGroup)
            {
                EndHighlighting();
            }
        }
    }

    /// <summary>
    /// 执行行高亮。
    /// </summary>
    private HighlightedLine DoHighlightLine(IDocumentLine documentLine, CachedLine? previousCachedLine)
    {
        var line = new HighlightedLine(_document, documentLine);

        // 如果有之前缓存的数据，先使用它（因为请求是异步的）
        if (previousCachedLine != null && previousCachedLine.HighlightedLine is var previousHighlight &&
            previousHighlight.Sections.Count > 0)
        {
            var offsetShift = documentLine.Offset - previousCachedLine.Offset;

            foreach (var section in previousHighlight.Sections)
            {
                var offset = section.Offset + offsetShift;

                if (offset < documentLine.Offset)
                    continue;

                if (offset >= documentLine.EndOffset)
                    break;

                int length = Math.Min(section.Length, documentLine.EndOffset - offset);
                line.Sections.Add(new HighlightedSection
                {
                    Color = section.Color,
                    Offset = offset,
                    Length = length,
                });
            }
        }

        // 异步处理请求
        _subject.OnNext(new FrozenLine(line));

        CacheLine(line);
        return line;
    }

    /// <summary>
    /// 订阅行分组。
    /// </summary>
    private void SubscribeToLineGroup(IObservable<FrozenLine> observable)
    {
        var connectible = observable.Throttle(TimeSpan.FromMilliseconds(DelayInMs))
            .SelectMany(SubscribeToLine)
            .Replay();
        connectible.Connect();
    }

    /// <summary>
    /// 订阅单行高亮请求。
    /// </summary>
    private async Task<object?> SubscribeToLine(FrozenLine line)
    {
        var document = _roslynHost.GetDocument(_documentId);
        if (document == null)
            return null;

        IEnumerable<ClassifiedSpan> spans;
        try
        {
            spans = await GetClassifiedSpansAsync(document, line).ConfigureAwait(true);
        }
        catch (Exception)
        {
            return null;
        }

        // 重建区段
        var sections = new List<HighlightedSection>();
        foreach (var classifiedSpan in spans)
        {
            var textSpan = AdjustTextSpan(classifiedSpan, line);
            if (textSpan == null)
            {
                continue;
            }

            sections.Add(new HighlightedSection
            {
                Color = _highlightColors.GetBrush(classifiedSpan.ClassificationType),
                Offset = textSpan.Value.Start,
                Length = textSpan.Value.Length
            });
        }

        // 在 UI 线程更新
        UpdateHighlightingSections(line, sections);
        return null;
    }

    /// <summary>
    /// 调整文本范围以适应行边界。
    /// </summary>
    private static TextSpan? AdjustTextSpan(ClassifiedSpan classifiedSpan, FrozenLine line)
    {
        if (classifiedSpan.TextSpan.Start > line.EndOffset)
        {
            return null;
        }

        var result = TextSpan.FromBounds(
            Math.Max(classifiedSpan.TextSpan.Start, line.Offset),
            Math.Min(classifiedSpan.TextSpan.End, line.EndOffset));

        return result;
    }

    /// <summary>
    /// 缓存已高亮的行。
    /// </summary>
    private void CacheLine(HighlightedLine line)
    {
        if (_cachedLines != null && _document.Version != null)
        {
            _cachedLines.Add(new CachedLine(line, _document.Version));

            if (_cachedLines.Count > CacheSize)
            {
                _cachedLines.RemoveRange(0, CacheSize / 2);
            }
        }
    }

    /// <summary>
    /// 异步获取分类区段。
    /// </summary>
    private async Task<IEnumerable<ClassifiedSpan>> GetClassifiedSpansAsync(Document document, FrozenLine line)
    {
        if (!line.IsDeleted)
        {
            var text = await document.GetTextAsync().ConfigureAwait(false);
            if (text.Length >= line.Offset + line.TotalLength)
            {
                return await Classifier.GetClassifiedSpansAsync(document,
                        new TextSpan(line.Offset, line.TotalLength), CancellationToken.None)
                    .ConfigureAwait(false);
            }
        }

        return [];
    }

    /// <summary>
    /// 获取默认文本颜色。
    /// </summary>
    HighlightingColor IHighlighter.DefaultTextColor => _highlightColors.DefaultBrush;

    /// <summary>
    /// 开始高亮组。
    /// </summary>
    public void BeginHighlighting()
    {
        if (_inHighlightingGroup)
            throw new InvalidOperationException();
        _inHighlightingGroup = true;
    }

    /// <summary>
    /// 结束高亮组。
    /// </summary>
    public void EndHighlighting()
    {
        _inHighlightingGroup = false;
        _updatedLine = null;

        lock (_changes)
        {
            foreach (var change in _changes)
            {
                UpdateHighlightingSectionsNoCheck(change.line, change.sections);
            }

            _changes.Clear();
        }
    }

    /// <summary>
    /// 根据名称获取颜色（不支持，返回 null）。
    /// </summary>
    public HighlightingColor? GetNamedColor(string name) => null;

    /// <summary>
    /// 缓存的行数据。
    /// </summary>
    private class CachedLine
    {
        public readonly HighlightedLine HighlightedLine;
        public readonly ITextSourceVersion OldVersion;
        public readonly int Offset;
        public readonly bool IsValid;

        public IDocumentLine DocumentLine => HighlightedLine.DocumentLine;

        public CachedLine(HighlightedLine highlightedLine, ITextSourceVersion fileVersion)
        {
            HighlightedLine = highlightedLine ?? throw new ArgumentNullException(nameof(highlightedLine));
            OldVersion = fileVersion ?? throw new ArgumentNullException(nameof(fileVersion));
            IsValid = true;
            Offset = HighlightedLine.DocumentLine.Offset;
        }
    }

    /// <summary>
    /// 冻结的行数据（用于异步处理）。
    /// </summary>
    private class FrozenLine
    {
        public FrozenLine(HighlightedLine line)
        {
            HighlightedLine = line;

            var documentLine = line.DocumentLine;
            TotalLength = documentLine.TotalLength;
            DelimiterLength = documentLine.DelimiterLength;
            LineNumber = documentLine.LineNumber;
            IsDeleted = documentLine.IsDeleted;
            Offset = documentLine.Offset;
            Length = documentLine.Length;
            EndOffset = documentLine.EndOffset;
        }

        public HighlightedLine HighlightedLine { get; }
        public int TotalLength { get; }
        public int DelimiterLength { get; }
        public int LineNumber { get; }
        public bool IsDeleted { get; }
        public int Offset { get; }
        public int Length { get; }
        public int EndOffset { get; }
    }
}
