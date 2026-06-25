using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using TextChange = Microsoft.CodeAnalysis.Text.TextChange;
using TextChangeEventArgs = Microsoft.CodeAnalysis.Text.TextChangeEventArgs;

namespace RoslynPad.Editor;

/// <summary>
/// 适配AvalonEdit编辑器的SourceText容器实现，用于桥接Roslyn的SourceText与AvalonEdit的文本文档
/// </summary>
public sealed class AvalonEditTextContainer : SourceTextContainer, IDisposable
{
    /// <summary>
    /// 当前的SourceText实例
    /// </summary>
    private SourceText _currentText;

    /// <summary>
    /// 标记是否正在更新文本，用于防止递归更新
    /// </summary>
    private bool _updatding;

    /// <summary>
    /// 获取关联的AvalonEdit文本文档
    /// </summary>
    public TextDocument Document { get; }

    /// <summary>
    /// 获取或设置关联的文本编辑器实例
    /// <para>若设置，文本更新时会同步更新<see cref="TextEditor.CaretOffset"/>（光标位置）</para>
    /// </summary>
    public TextEditor? Editor { get; set; }

    /// <summary>
    /// 获取当前的SourceText实例
    /// </summary>
    public override SourceText CurrentText => _currentText;

    /// <summary>
    /// 初始化<AvalonEditTextContainer>实例
    /// </summary>
    /// <param name="document">关联的AvalonEdit文本文档，不能为空</param>
    /// <exception cref="ArgumentNullException">当document为null时抛出</exception>
    public AvalonEditTextContainer(TextDocument document)
    {
        Document = document ?? throw new ArgumentNullException(nameof(document));
        _currentText = new AvalonEditSourceText(this, Document.Text);

        Document.Changed += DocumentOnChanged;
    }

    /// <summary>
    /// 释放资源，取消文档变更事件订阅
    /// </summary>
    public void Dispose()
    {
        Document.Changed -= DocumentOnChanged;
    }

    /// <summary>
    /// 处理文档文本变更事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">文档变更事件参数</param>
    private void DocumentOnChanged(object? sender, DocumentChangeEventArgs e)
    {
        if (_updatding) return;

        var oldText = _currentText;

        var textSpan = new TextSpan(e.Offset, e.RemovalLength);
        var textChangeRange = new TextChangeRange(textSpan, e.InsertionLength);
        _currentText = _currentText.WithChanges(new TextChange(textSpan, e.InsertedText?.Text ?? string.Empty));

        TextChanged?.Invoke(this, new TextChangeEventArgs(oldText, _currentText, textChangeRange));
    }

    /// <summary>
    /// 文本变更事件，当容器内文本发生变化时触发
    /// </summary>
    public override event EventHandler<TextChangeEventArgs>? TextChanged;

    /// <summary>
    /// 更新容器内的文本，并同步调整编辑器光标位置
    /// </summary>
    /// <param name="newText">新的SourceText实例</param>
    public void UpdateText(SourceText newText)
    {
        _updatding = true;
        Document.BeginUpdate();
        var editor = Editor;
        var caretOffset = editor?.CaretOffset ?? 0;
        var documentOffset = 0;
        try
        {
            var changes = newText.GetTextChanges(_currentText);

            foreach (var change in changes)
            {
                var newTextChange = change.NewText ?? string.Empty;
                Document.Replace(change.Span.Start + documentOffset, change.Span.Length, new StringTextSource(newTextChange));

                var changeOffset = newTextChange.Length - change.Span.Length;
                if (caretOffset >= change.Span.Start + documentOffset + change.Span.Length)
                {
                    // 光标在变更文本后方时，根据文本长度差调整光标位置
                    caretOffset += changeOffset;
                }
                else if (caretOffset >= change.Span.Start + documentOffset)
                {
                    // 光标在变更文本内时，若变更后光标超出替换文本范围，将光标移至替换文本起始位置
                    if (caretOffset >= change.Span.Start + documentOffset + newTextChange.Length)
                    {
                        caretOffset = change.Span.Start + documentOffset;
                    }
                }

                documentOffset += changeOffset;
            }

            _currentText = newText;
        }
        finally
        {
            _updatding = false;
            Document.EndUpdate();

            // 确保光标位置在合法范围内
            if (caretOffset < 0)
            {
                caretOffset = 0;
            }

            if (caretOffset > newText.Length)
            {
                caretOffset = newText.Length;
            }

            // 同步更新编辑器光标位置
            if (editor != null)
            {
                editor.CaretOffset = caretOffset;
            }
        }
    }

    /// <summary>
    /// 适配AvalonEdit的SourceText包装类，桥接原生SourceText与AvalonEditTextContainer
    /// </summary>
    private class AvalonEditSourceText : SourceText
    {
        /// <summary>
        /// 关联的AvalonEditTextContainer容器
        /// </summary>
        private readonly AvalonEditTextContainer _container;

        /// <summary>
        /// 底层的原生SourceText实例
        /// </summary>
        private readonly SourceText _sourceText;

        /// <summary>
        /// 从字符串初始化<AvalonEditSourceText>实例
        /// </summary>
        /// <param name="container">关联的容器实例</param>
        /// <param name="text">初始化的文本内容</param>
        public AvalonEditSourceText(AvalonEditTextContainer container, string text) : this(container, From(text))
        {
        }

        /// <summary>
        /// 从原生SourceText初始化<AvalonEditSourceText>实例
        /// </summary>
        /// <param name="container">关联的容器实例</param>
        /// <param name="sourceText">原生SourceText实例</param>
        private AvalonEditSourceText(AvalonEditTextContainer container, SourceText sourceText)
        {
            _container = container;
            _sourceText = sourceText;
        }

        /// <summary>
        /// 将文本内容复制到字符数组
        /// </summary>
        /// <param name="sourceIndex">源文本起始索引</param>
        /// <param name="destination">目标字符数组</param>
        /// <param name="destinationIndex">目标数组起始索引</param>
        /// <param name="count">复制的字符数量</param>
        public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count) =>
            _sourceText.CopyTo(sourceIndex, destination, destinationIndex, count);

        /// <summary>
        /// 获取文本的编码格式
        /// </summary>
        public override Encoding? Encoding => _sourceText.Encoding;

        /// <summary>
        /// 获取文本长度
        /// </summary>
        public override int Length => _sourceText.Length;

        /// <summary>
        /// 获取指定位置的字符
        /// </summary>
        /// <param name="position">字符位置索引</param>
        /// <returns>指定位置的字符</returns>
        public override char this[int position] => _sourceText[position];

        /// <summary>
        /// 获取指定文本范围的子文本
        /// </summary>
        /// <param name="span">文本范围</param>
        /// <returns>包装后的子文本实例</returns>
        public override SourceText GetSubText(TextSpan span) => new AvalonEditSourceText(_container, _sourceText.GetSubText(span));

        /// <summary>
        /// 将指定文本范围写入TextWriter
        /// </summary>
        /// <param name="writer">目标TextWriter</param>
        /// <param name="span">文本范围</param>
        /// <param name="cancellationToken">取消令牌</param>
        public override void Write(TextWriter writer, TextSpan span, CancellationToken cancellationToken = new CancellationToken()) =>
            _sourceText.Write(writer, span, cancellationToken);

        /// <summary>
        /// 将文本转换为字符串
        /// </summary>
        /// <returns>完整的文本字符串</returns>
        public override string ToString() => _sourceText.ToString();

        /// <summary>
        /// 将指定范围的文本转换为字符串
        /// </summary>
        /// <param name="span">文本范围</param>
        /// <returns>指定范围的文本字符串</returns>
        public override string ToString(TextSpan span) => _sourceText.ToString(span);

        /// <summary>
        /// 获取与旧文本相比的变更范围列表
        /// </summary>
        /// <param name="oldText">旧文本实例</param>
        /// <returns>变更范围列表</returns>
        public override IReadOnlyList<TextChangeRange> GetChangeRanges(SourceText oldText)
            => _sourceText.GetChangeRanges(GetInnerSourceText(oldText));

        /// <summary>
        /// 获取与旧文本相比的文本变更列表
        /// </summary>
        /// <param name="oldText">旧文本实例</param>
        /// <returns>文本变更列表</returns>
        public override IReadOnlyList<TextChange> GetTextChanges(SourceText oldText) => _sourceText.GetTextChanges(GetInnerSourceText(oldText));

        /// <summary>
        /// 获取文本行集合
        /// </summary>
        /// <returns>文本行集合</returns>
        protected override TextLineCollection GetLinesCore() => _sourceText.Lines;

        /// <summary>
        /// 比较文本内容是否与另一SourceText相等
        /// </summary>
        /// <param name="other">待比较的SourceText实例</param>
        /// <returns>内容是否相等</returns>
        protected override bool ContentEqualsImpl(SourceText other) => _sourceText.ContentEquals(GetInnerSourceText(other));

        /// <summary>
        /// 获取关联的SourceText容器
        /// </summary>
        public override SourceTextContainer Container => _container ?? _sourceText.Container;

        /// <summary>
        /// 判断当前实例是否与另一对象相等
        /// </summary>
        /// <param name="obj">待比较的对象</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object? obj) => _sourceText.Equals(obj);

        /// <summary>
        /// 获取哈希码
        /// </summary>
        /// <returns>哈希码值</returns>
        public override int GetHashCode() => _sourceText.GetHashCode();

        /// <summary>
        /// 应用文本变更并返回新的SourceText实例
        /// </summary>
        /// <param name="changes">文本变更列表</param>
        /// <returns>应用变更后的新实例</returns>
        public override SourceText WithChanges(IEnumerable<TextChange> changes) =>
            new AvalonEditSourceText(_container, _sourceText.WithChanges(changes));

        /// <summary>
        /// 从包装类中提取原生SourceText实例
        /// </summary>
        /// <param name="oldText">包装后的SourceText实例</param>
        /// <returns>原生SourceText实例</returns>
        private static SourceText GetInnerSourceText(SourceText oldText) =>
            (oldText as AvalonEditSourceText)?._sourceText ?? oldText;
    }
}
