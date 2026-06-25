using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Threading;

namespace RoslynPad.Roslyn.SignatureHelp;

/// <summary>
/// 表示签名帮助的参数信息封装类
/// </summary>
public class SignatureHelpParameter
{
    /// <summary>
    /// 获取参数名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 获取用于生成参数文档的工厂方法，该方法接收取消令牌并返回标记文本集合
    /// </summary>
    public Func<CancellationToken, IEnumerable<TaggedText>> DocumentationFactory { get; }

    /// <summary>
    /// 获取参数前缀的显示文本片段集合
    /// </summary>
    public IList<TaggedText> PrefixDisplayParts { get; }

    /// <summary>
    /// 获取参数后缀的显示文本片段集合
    /// </summary>
    public IList<TaggedText> SuffixDisplayParts { get; }

    /// <summary>
    /// 获取参数的显示文本片段集合
    /// </summary>
    public IList<TaggedText> DisplayParts { get; }

    /// <summary>
    /// 获取一个值，该值指示参数是否为可选参数
    /// </summary>
    public bool IsOptional { get; }

    /// <summary>
    /// 获取参数被选中时的显示文本片段集合
    /// </summary>
    public IList<TaggedText> SelectedDisplayParts { get; }

    /// <summary>
    /// 使用 Roslyn 原生的签名帮助参数实例初始化 <see cref="SignatureHelpParameter"/> 类的新实例
    /// </summary>
    /// <param name="inner">Roslyn 原生的 <see cref="Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpParameter"/> 实例</param>
    internal SignatureHelpParameter(Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpParameter inner)
    {
        Name = inner.Name;
        DocumentationFactory = inner.DocumentationFactory;
        PrefixDisplayParts = inner.PrefixDisplayParts;
        SuffixDisplayParts = inner.SuffixDisplayParts;
        DisplayParts = inner.DisplayParts;
        IsOptional = inner.IsOptional;
        SelectedDisplayParts = inner.SelectedDisplayParts;
    }
}
