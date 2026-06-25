using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RoslynPad.Roslyn.SignatureHelp;

/// <summary>
/// 为 <see cref="ISignatureHelpProvider"/> 接口提供扩展方法的静态类
/// </summary>
public static class SignatureHelpProviderExtensions
{
    /// <summary>
    /// 异步判断指定位置的字符是否是签名帮助的触发字符
    /// </summary>
    /// <param name="provider">签名帮助提供程序实例，不能为 null</param>
    /// <param name="document">包含要检查位置的文档实例</param>
    /// <param name="position">要检查的字符位置（基于文档文本的偏移量）</param>
    /// <returns>如果指定位置的字符是触发字符则返回 true，否则返回 false</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="provider"/> 为 null 时抛出</exception>
    public static async Task<bool> IsTriggerCharacter(this ISignatureHelpProvider provider, Document document, int position)
    {
        ArgumentNullException.ThrowIfNull(provider);
        var text = await document.GetTextAsync().ConfigureAwait(false);
        var character = text.GetSubText(new TextSpan(position, 1))[0];
        return provider.IsTriggerCharacter(character);
    }
}
