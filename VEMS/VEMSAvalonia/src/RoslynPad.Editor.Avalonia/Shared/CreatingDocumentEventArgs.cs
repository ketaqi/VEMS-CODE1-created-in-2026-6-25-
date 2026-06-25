using Microsoft.CodeAnalysis;

namespace RoslynPad.Editor;

/// <summary>
/// 文档创建事件的参数类
/// </summary>
public class CreatingDocumentEventArgs : RoutedEventArgs
{
    /// <summary>
    /// 初始化 <see cref="CreatingDocumentEventArgs"/> 类的新实例
    /// </summary>
    /// <param name="textContainer">关联的文本容器实例</param>
    public CreatingDocumentEventArgs(AvalonEditTextContainer textContainer)
    {
        TextContainer = textContainer;
        RoutedEvent = RoslynCodeEditor.CreatingDocumentEvent;
    }

    /// <summary>
    /// 获取关联的文本容器
    /// </summary>
    public AvalonEditTextContainer TextContainer { get; }

    /// <summary>
    /// 获取或设置文档标识ID
    /// </summary>
    public DocumentId? DocumentId { get; set; }
}
