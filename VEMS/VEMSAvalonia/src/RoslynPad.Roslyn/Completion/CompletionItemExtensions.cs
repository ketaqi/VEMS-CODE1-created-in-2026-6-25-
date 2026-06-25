using Microsoft.CodeAnalysis.Completion;
using RoslynPad.Roslyn.CodeActions;

namespace RoslynPad.Roslyn.Completion;

/// <summary>
/// 为 <see cref="CompletionItem"/> 类型提供的扩展方法类
/// </summary>
public static class CompletionItemExtensions
{
    /// <summary>
    /// 从补全项中获取对应的图标标识
    /// </summary>
    /// <param name="completionItem">补全项实例</param>
    /// <returns>表示图标的 <see cref="Glyph"/> 枚举值</returns>
    public static Glyph GetGlyph(this CompletionItem completionItem)
    {
        return CodeActionExtensions.GetGlyph(completionItem.Tags);
    }

    /// <summary>
    /// 获取补全项的描述信息
    /// </summary>
    /// <param name="completionItem">补全项实例</param>
    /// <returns>包含描述信息的 <see cref="CompletionDescription"/> 实例</returns>
    public static CompletionDescription GetDescription(this CompletionItem completionItem)
    {
        return CommonCompletionItem.GetDescription(completionItem);
    }
}
