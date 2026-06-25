using RoslynPad.Roslyn.Completion;

namespace RoslynPad.Roslyn;

/// <summary>
/// 图标服务接口，用于获取指定图标枚举对应的图标图像
/// </summary>
public interface IGlyphService
{
    /// <summary>
    /// 获取指定图标对应的图像对象
    /// </summary>
    /// <param name="glyph">图标枚举值</param>
    /// <returns>图标对应的图像对象，无对应图像则返回 null</returns>
    object? GetGlyphImage(Glyph glyph);
}
