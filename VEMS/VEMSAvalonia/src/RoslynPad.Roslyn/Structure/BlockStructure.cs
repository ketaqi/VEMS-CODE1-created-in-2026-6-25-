using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace RoslynPad.Roslyn.Structure;

/// <summary>
/// 表示整个文档的代码块结构，包含多个块跨度的集合
/// </summary>
public sealed class BlockStructure
{
    /// <summary>
    /// 从Roslyn原生<see cref="Microsoft.CodeAnalysis.Structure.BlockStructure"/>初始化<see cref="BlockStructure"/>实例
    /// </summary>
    /// <param name="inner">Roslyn原生的块结构实例</param>
    internal BlockStructure(Microsoft.CodeAnalysis.Structure.BlockStructure inner)
    {
        Spans = inner.Spans.SelectAsArray(span => new BlockSpan(span));
    }

    /// <summary>
    /// 获取文档中所有代码块的跨度集合
    /// </summary>
    public ImmutableArray<BlockSpan> Spans { get; }
}
