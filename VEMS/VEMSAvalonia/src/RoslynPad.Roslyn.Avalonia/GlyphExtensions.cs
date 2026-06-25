using RoslynPad.Roslyn.Completion;
using RoslynPad.Roslyn.Resources;
using Avalonia.Media;

namespace RoslynPad.Roslyn;

/// <summary>
/// 为Glyph（符号图标）提供扩展方法，用于转换为Avalonia的DrawingImage
/// </summary>
public static class GlyphExtensions
{
    /// <summary>
    /// 用于获取Glyph对应图像的Glyph服务实例
    /// </summary>
    public static IGlyphService GlyphService { get; set; } = new DefaultGlyphService();

    /// <summary>
    /// 将Glyph转换为Avalonia的DrawingImage（若存在对应图像）
    /// </summary>
    /// <param name="glyph">待转换的Glyph实例</param>
    /// <returns>转换后的DrawingImage，若无对应图像则返回null</returns>
    public static DrawingImage? ToImageSource(this Glyph glyph) => GlyphService.GetGlyphImage(glyph) as DrawingImage;

    /// <summary>
    /// 默认的Glyph服务实现，用于从内置Glyphs资源中获取对应图像
    /// </summary>
    private class DefaultGlyphService : IGlyphService
    {
        /// <summary>
        /// 内置Glyph图像资源集合
        /// </summary>
        private readonly Glyphs _glyphs = [];

        /// <summary>
        /// 获取指定Glyph对应的图像对象
        /// </summary>
        /// <param name="glyph">待获取图像的Glyph实例</param>
        /// <returns>若存在对应Drawing则返回DrawingImage，否则返回null</returns>
        public object? GetGlyphImage(Glyph glyph)
        {
            if (_glyphs.TryGetValue(glyph.ToString(), out var glyphImage) && glyphImage is Drawing drawing)
            {
                return new DrawingImage { Drawing = drawing };
            }

            return null;
        }
    }
}
