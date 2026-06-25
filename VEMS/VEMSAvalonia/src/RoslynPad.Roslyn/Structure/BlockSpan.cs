using Microsoft.CodeAnalysis.Text;

#pragma warning disable CA1716
namespace RoslynPad.Roslyn.Structure;

/// <summary>
/// 表示代码块的跨度信息，包含文本范围和横幅文本
/// </summary>
public readonly struct BlockSpan
{
    /// <summary>
    /// 从Roslyn原生<see cref="Microsoft.CodeAnalysis.Structure.BlockSpan"/>初始化<see cref="BlockSpan"/>实例
    /// </summary>
    /// <param name="inner">Roslyn原生的块跨度实例</param>
    internal BlockSpan(Microsoft.CodeAnalysis.Structure.BlockSpan inner)
    {
        TextSpan = inner.TextSpan;
        BannerText = inner.BannerText;
    }

    /// <summary>
    /// 获取代码块的文本范围
    /// </summary>
    public TextSpan TextSpan { get; }

    /// <summary>
    /// 获取代码块的横幅文本（用于折叠/展开显示的描述文本）
    /// </summary>
    public string BannerText { get; }
}
