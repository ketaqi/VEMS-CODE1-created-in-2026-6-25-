using Microsoft.CodeAnalysis.Text;

namespace RoslynPad.Roslyn.SignatureHelp;

/// <summary>
/// 签名帮助项集合模型，封装一组签名帮助项及上下文信息
/// </summary>
public class SignatureHelpItems
{
    /// <summary>
    /// 获取签名帮助项列表
    /// </summary>
    public IList<SignatureHelpItem> Items { get; }

    /// <summary>
    /// 获取当前签名帮助适用的文本范围
    /// </summary>
    public TextSpan ApplicableSpan { get; }

    /// <summary>
    /// 获取语义分析得到的参数索引
    /// </summary>
	public int SemanticParameterIndex { get; }

    /// <summary>
    /// 获取语法分析得到的参数数量
    /// </summary>
	public int SyntacticArgumentCount { get; }

    /// <summary>
    /// 获取参数名称（可为null）
    /// </summary>
    public string? ArgumentName { get; }

    /// <summary>
    /// 获取或设置选中项的索引（可为null）
    /// </summary>
    public int? SelectedItemIndex { get; internal set; }

    /// <summary>
    /// 构造函数，从Roslyn原生签名帮助项集合初始化当前实例
    /// </summary>
    /// <param name="inner">Roslyn原生签名帮助项集合</param>
    internal SignatureHelpItems(Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpItems inner)
    {
        Items = inner.Items.Select(x => new SignatureHelpItem(x)).ToArray();
        ApplicableSpan = inner.ApplicableSpan;
        SemanticParameterIndex = inner.SemanticParameterIndex;
        SyntacticArgumentCount = inner.SyntacticArgumentCount;
        ArgumentName = inner.ArgumentName;
        SelectedItemIndex = inner.SelectedItemIndex;
    }
}
