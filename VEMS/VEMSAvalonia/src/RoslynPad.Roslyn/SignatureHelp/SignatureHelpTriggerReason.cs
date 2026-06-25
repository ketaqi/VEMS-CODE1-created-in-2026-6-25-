namespace RoslynPad.Roslyn.SignatureHelp;

/// <summary>
/// 定义签名帮助的触发原因枚举
/// </summary>
public enum SignatureHelpTriggerReason
{
    /// <summary>
    /// 通过调用签名帮助命令触发
    /// </summary>
    InvokeSignatureHelpCommand,

    /// <summary>
    /// 通过输入字符命令触发
    /// </summary>
    TypeCharCommand,

    /// <summary>
    /// 通过重新触发命令触发
    /// </summary>
    RetriggerCommand
}
