namespace RoslynPad.Roslyn.SignatureHelp;

/// <summary>
/// 表示签名帮助的触发信息结构体
/// </summary>
/// <param name="triggerReason">签名帮助的触发原因</param>
/// <param name="triggerCharacter">触发签名帮助的字符（可选）</param>
public readonly struct SignatureHelpTriggerInfo(SignatureHelpTriggerReason triggerReason, char? triggerCharacter = null)
{
    /// <summary>
    /// 获取对应的 Roslyn 原生签名帮助触发信息实例
    /// </summary>
    internal Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpTriggerInfo Inner { get; } = new Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpTriggerInfo(
            (Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpTriggerReason)triggerReason, triggerCharacter);

    /// <summary>
    /// 获取签名帮助的触发原因
    /// </summary>
    public SignatureHelpTriggerReason TriggerReason => (SignatureHelpTriggerReason)Inner.TriggerReason;

    /// <summary>
    /// 获取触发签名帮助的字符（如果有）
    /// </summary>
    public char? TriggerCharacter => Inner.TriggerCharacter;
}
