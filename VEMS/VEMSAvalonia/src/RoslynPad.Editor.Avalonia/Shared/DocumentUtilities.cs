namespace RoslynPad.Editor;

/// <summary>
/// 文档处理相关的工具类
/// </summary>
internal static class DocumentUtilities
{
    /// <summary>
    /// 在文本源中查找指定偏移量前的单词起始位置
    /// </summary>
    /// <param name="textSource">文本源实例</param>
    /// <param name="offset">起始查找的偏移量</param>
    /// <returns>找到的单词起始位置偏移量</returns>
    public static int FindPreviousWordStart(this ITextSource textSource, int offset)
    {
        return TextUtilities.GetNextCaretPosition(textSource, offset, LogicalDirection.Backward, CaretPositioningMode.WordStart);
    }
}
