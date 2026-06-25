using Microsoft.CodeAnalysis;

namespace RoslynPad.Roslyn;

/// <summary>
/// <see cref="ISymbol"/> 接口的扩展方法类
/// </summary>
public static class ISymbolExtensions
{
    /// <summary>
    /// 获取符号对应的补全图标（Glyph）
    /// </summary>
    /// <param name="symbol">目标符号</param>
    /// <returns>符号对应的补全图标枚举值</returns>
    public static Completion.Glyph GetGlyph(this ISymbol symbol)
    {
        return (Completion.Glyph)Microsoft.CodeAnalysis.Shared.Extensions.ISymbolExtensions2.GetGlyph(symbol);
    }
}
