using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace RoslynPad.Roslyn.SignatureHelp;

/// <summary>
/// 签名帮助项模型，封装单个签名帮助的核心信息
/// </summary>
public class SignatureHelpItem
{
    /// <summary>
    /// 获取一个值，指示当前签名帮助项是否为可变参数（支持任意数量参数）
    /// </summary>
    public bool IsVariadic { get; }

    /// <summary>
    /// 获取签名前缀显示文本片段集合
    /// </summary>
    public ImmutableArray<TaggedText> PrefixDisplayParts { get; }

    /// <summary>
    /// 获取签名后缀显示文本片段集合
    /// </summary>
    public ImmutableArray<TaggedText> SuffixDisplayParts { get; }

    /// <summary>
    /// 获取参数分隔符显示文本片段集合
    /// </summary>
    public ImmutableArray<TaggedText> SeparatorDisplayParts { get; }

    /// <summary>
    /// 获取签名参数集合
    /// </summary>
    public ImmutableArray<SignatureHelpParameter> Parameters { get; }

    /// <summary>
    /// 获取签名描述文本片段集合
    /// </summary>
    public ImmutableArray<TaggedText> DescriptionParts { get; }

    /// <summary>
    /// 获取文档注释工厂方法，用于异步获取签名的文档注释
    /// </summary>
    public Func<CancellationToken, IEnumerable<TaggedText>> DocumentationFactory { get; }

    /// <summary>
    /// 构造函数，从Roslyn原生签名帮助项初始化当前实例
    /// </summary>
    /// <param name="inner">Roslyn原生签名帮助项</param>
    internal SignatureHelpItem(Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpItem inner)
    {
        IsVariadic = inner.IsVariadic;
        PrefixDisplayParts = inner.PrefixDisplayParts;
        SuffixDisplayParts = inner.SuffixDisplayParts;
        SeparatorDisplayParts = inner.SeparatorDisplayParts;
        Parameters = ImmutableArray.CreateRange(inner.Parameters.Select(source => new SignatureHelpParameter(source)));
        DescriptionParts = inner.DescriptionParts;
        IsVariadic = inner.IsVariadic;
        DocumentationFactory = inner.DocumentationFactory;
    }
}
