// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

namespace RoslynPad.Editor;

/// <summary>
/// 文本标记提示框提供器，处理编辑器中标记（Marker）的提示框展示逻辑
/// </summary>
/// <param name="textMarkerService">文本标记服务，用于管理和获取文本标记</param>
/// <param name="editor">文本编辑器实例，关联当前操作的编辑器</param>
internal sealed class TextMarkerToolTipProvider(TextMarkerService textMarkerService, TextEditor editor)
{
    private readonly TextMarkerService _textMarkerService = textMarkerService;
    private readonly TextEditor _editor = editor;

    /// <summary>
    /// 处理提示框请求事件，根据鼠标位置展示对应的标记提示信息
    /// </summary>
    /// <param name="args">提示框请求事件参数，包含请求位置、文档上下文等信息</param>
    public void HandleToolTipRequest(ToolTipRequestEventArgs args)
    {
        if (!args.InDocument) return;
        var offset = _editor.Document.GetOffset(args.LogicalPosition);

        //FoldingManager foldings = _editor.GetService(typeof(FoldingManager)) as FoldingManager;
        //if (foldings != null)
        //{
        //    var foldingsAtOffset = foldings.GetFoldingsAt(offset);
        //    FoldingSection collapsedSection = foldingsAtOffset.FirstOrDefault(section => section.IsFolded);

        //    if (collapsedSection != null)
        //    {
        //        args.SetToolTip(GetTooltipTextForCollapsedSection(args, collapsedSection));
        //    }
        //}

        var markersAtOffset = _textMarkerService.GetMarkersAtOffset(offset);
        var markerWithToolTip = markersAtOffset.FirstOrDefault(marker => marker.ToolTip != null);
        if (markerWithToolTip != null && markerWithToolTip.ToolTip != null)
        {
            args.SetToolTip(markerWithToolTip.ToolTip);
        }
    }

    ///// <summary>
    ///// 获取折叠区域的提示文本（已注释，保留原有逻辑）
    ///// </summary>
    ///// <param name="args">提示框请求事件参数</param>
    ///// <param name="foldingSection">折叠区域对象</param>
    ///// <returns>格式化后的折叠区域提示文本</returns>
    //string GetTooltipTextForCollapsedSection(ToolTipRequestEventArgs args, FoldingSection foldingSection)
    //{
    //    return ToolTipUtils.GetAlignedText(_editor.Document, foldingSection.StartOffset, foldingSection.EndOffset);
    //}
}

/// <summary>
/// 提示框请求事件参数类，封装提示框请求的相关上下文信息
/// </summary>
public sealed class ToolTipRequestEventArgs : RoutedEventArgs
{
    /// <summary>
    /// 初始化<see cref="ToolTipRequestEventArgs"/>类的新实例
    /// </summary>
    public ToolTipRequestEventArgs()
    {
        RoutedEvent = CodeTextEditor.ToolTipRequestEvent;
    }

    /// <summary>
    /// 获取或设置一个值，指示请求是否发生在文档内容范围内
    /// </summary>
    public bool InDocument { get; set; }

    /// <summary>
    /// 获取或设置请求的逻辑位置（文档中的行列位置）
    /// </summary>
    public TextLocation LogicalPosition { get; set; }

    /// <summary>
    /// 获取或设置请求的字符偏移位置
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// 获取或设置要展示的提示框内容
    /// </summary>
    public object? ContentToShow { get; set; }

    /// <summary>
    /// 设置提示框要展示的内容，并标记事件为已处理
    /// </summary>
    /// <param name="content">提示框内容（不能为空）</param>
    /// <exception cref="ArgumentNullException">当content为null时抛出</exception>
    public void SetToolTip(object content)
    {
        Handled = true;
        ContentToShow = content ?? throw new ArgumentNullException(nameof(content));
    }
}
