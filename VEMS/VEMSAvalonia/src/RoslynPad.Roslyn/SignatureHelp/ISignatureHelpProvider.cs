using Microsoft.CodeAnalysis;

namespace RoslynPad.Roslyn.SignatureHelp;

/// <summary>
/// 签名帮助提供程序接口，定义签名帮助的核心行为契约
/// </summary>
public interface ISignatureHelpProvider
{
    /// <summary>
    /// 判断指定字符是否为触发签名帮助的字符
    /// </summary>
    /// <param name="ch">待判断的字符</param>
    /// <returns>是触发字符则返回true，否则返回false</returns>
    bool IsTriggerCharacter(char ch);

    /// <summary>
    /// 判断指定字符是否为重新触发签名帮助的字符
    /// </summary>
    /// <param name="ch">待判断的字符</param>
    /// <returns>是重新触发字符则返回true，否则返回false</returns>
    bool IsRetriggerCharacter(char ch);

    /// <summary>
    /// 异步获取指定位置的签名帮助项
    /// </summary>
    /// <param name="document">当前文档</param>
    /// <param name="position">获取签名帮助的位置（字符偏移量）</param>
    /// <param name="triggerInfo">签名帮助触发信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>匹配的签名帮助项，无匹配项时返回null</returns>
    Task<SignatureHelpItems?> GetItemsAsync(
        Document document,
        int position,
        SignatureHelpTriggerInfo triggerInfo,
        CancellationToken cancellationToken = default);
}
