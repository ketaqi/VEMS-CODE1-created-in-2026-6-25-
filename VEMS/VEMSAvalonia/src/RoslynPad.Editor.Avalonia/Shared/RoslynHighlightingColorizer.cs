using Microsoft.CodeAnalysis;
using RoslynPad.Roslyn;

namespace RoslynPad.Editor;

/// <summary>
/// 基于Roslyn的语法高亮着色器，用于为编辑器提供语义高亮能力
/// </summary>
/// <param name="documentId">文档唯一标识</param>
/// <param name="roslynHost">Roslyn宿主服务，提供编译、语法分析等能力</param>
/// <param name="highlightColors">高亮颜色配置，定义各类语法元素的显示颜色</param>
public sealed class RoslynHighlightingColorizer(DocumentId documentId, IRoslynHost roslynHost, IClassificationHighlightColors highlightColors) : HighlightingColorizer
{
    private readonly DocumentId _documentId = documentId;
    private readonly IRoslynHost _roslynHost = roslynHost;
    private readonly IClassificationHighlightColors _highlightColors = highlightColors;

    /// <summary>
    /// 创建适用于指定文本视图和文档的Roslyn语义高亮器
    /// </summary>
    /// <param name="textView">文本视图对象，对应编辑器的可视化区域</param>
    /// <param name="document">文本文档对象，包含文档内容和结构信息</param>
    /// <returns>实现语义高亮的<see cref="IHighlighter"/>实例</returns>
    protected override IHighlighter CreateHighlighter(TextView textView, TextDocument document) =>
        new RoslynSemanticHighlighter(textView, document, _documentId, _roslynHost, _highlightColors);
}
