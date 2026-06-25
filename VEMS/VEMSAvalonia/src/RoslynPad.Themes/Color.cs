using System.Globalization;

namespace RoslynPad.Themes;

/// <summary>
/// 表示一个颜色值，支持 HSLA 和 RGBA 色彩空间之间的转换。
/// </summary>
internal readonly struct Color
{
    private readonly HSLA _hsla;

    /// <summary>
    /// 使用 HSLA 值初始化 <see cref="Color"/> 实例。
    /// </summary>
    /// <param name="hsla">HSLA 颜色值。</param>
    public Color(HSLA hsla) => _hsla = hsla;

    /// <summary>
    /// 使用 RGBA 值初始化 <see cref="Color"/> 实例。
    /// </summary>
    /// <param name="rgba">RGBA 颜色值，会自动转换为 HSLA。</param>
    public Color(RGBA rgba) => _hsla = FromRGBA(rgba);

    /// <summary>
    /// 从十六进制颜色字符串创建 <see cref="Color"/> 实例。
    /// </summary>
    /// <param name="color">十六进制颜色字符串，支持格式：#RGB、#ARGB、#RRGGBB、#AARRGGBB。</param>
    /// <returns>对应的颜色实例。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当颜色字符串格式不正确时抛出。</exception>
    public static Color FromHex(string color) => color.Length switch
    {
        4 => new Color(new RGBA(ParseHex(color, new(1, 2)), ParseHex(color, new(2, 3)), ParseHex(color, new(3, 4)), 1.0)),
        5 => new Color(new RGBA(ParseHex(color, new(2, 3)), ParseHex(color, new(3, 4)), ParseHex(color, new(4, 5)), ParseHex(color, new(1, 2)) / 255.0)),
        7 => new Color(new RGBA(ParseHex(color, new(1, 3)), ParseHex(color, new(3, 5)), ParseHex(color, new(5, 7)), 1.0)),
        9 => new Color(new RGBA(ParseHex(color, new(3, 5)), ParseHex(color, new(5, 7)), ParseHex(color, new(7, 9)), ParseHex(color, new(1, 3)) / 255.0)),
        _ => throw new ArgumentOutOfRangeException(nameof(color))
    };

    /// <summary>
    /// 获取白色。
    /// </summary>
    public static Color White => new(new RGBA(255, 255, 255, 1.0));

    /// <summary>
    /// 获取黑色。
    /// </summary>
    public static Color Black => new(new RGBA(0, 0, 0, 1.0));

    /// <summary>
    /// 解析十六进制字符串的指定范围为整数。
    /// </summary>
    private static int ParseHex(string color, Range range) => int.Parse(color.AsSpan(range), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

    /// <summary>
    /// 返回颜色的十六进制字符串表示形式。
    /// </summary>
    public override string ToString() => ToRGBA(_hsla).ToString();

    /// <summary>
    /// 使颜色变亮。
    /// </summary>
    /// <param name="factor">亮度增加因子（0.0 到 1.0）。</param>
    /// <returns>变亮后的颜色。</returns>
    public Color Lighten(double factor)
    {
        return new Color(new HSLA(_hsla.H, _hsla.S, _hsla.L + _hsla.L * factor, _hsla.A));
    }

    /// <summary>
    /// 使颜色变暗。
    /// </summary>
    /// <param name="factor">亮度降低因子（0.0 到 1.0）。</param>
    /// <returns>变暗后的颜色。</returns>
    public Color Darken(double factor)
    {
        return new Color(new HSLA(_hsla.H, _hsla.S, _hsla.L - _hsla.L * factor, _hsla.A));
    }

    /// <summary>
    /// 调整颜色的透明度。
    /// </summary>
    /// <param name="factor">透明度因子（0.0 到 1.0）。</param>
    /// <returns>调整透明度后的颜色。</returns>
    public Color Transparent(double factor)
    {
        var rgba = ToRGBA(_hsla);
        return new Color(rgba with { A = rgba.A * factor });
    }

    /// <summary>
    /// 将 RGBA 颜色转换为 HSLA 颜色。
    /// </summary>
    private static HSLA FromRGBA(RGBA rgba)
    {
        var r = rgba.R / 255.0;
        var g = rgba.G / 255.0;
        var b = rgba.B / 255.0;
        var a = rgba.A;

        var max = Math.Max(Math.Max(r, g), b);
        var min = Math.Min(Math.Min(r, g), b);
        var h = 0.0;
        var s = 0.0;
        var l = (min + max) / 2.0;
        var chroma = max - min;

        if (chroma > 0)
        {
            s = Math.Min((l <= 0.5 ? chroma / (2.0 * l) : chroma / (2.0 - (2.0 * l))), 1);

            if (max == r)
            {
                h = (g - b) / chroma + (g < b ? 6 : 0);
            }
            else if (max == g)
            {
                h = (b - r) / chroma + 2;
            }
            else if (max == b)
            {
                h = (r - g) / chroma + 4;
            }

            h *= 60;
            h = Math.Round(h);
        }

        return new HSLA(h, s, l, a);
    }

    /// <summary>
    /// 将 HSLA 颜色转换为 RGBA 颜色。
    /// </summary>
    private static RGBA ToRGBA(HSLA hsla)
    {
        var h = hsla.H / 360.0;
        var s = hsla.S;
        var l = hsla.L;
        var a = hsla.A;
        double r, g, b;

        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            var p = 2 * l - q;
            r = Hue2Rgb(p, q, h + 1.0 / 3.0);
            g = Hue2Rgb(p, q, h);
            b = Hue2Rgb(p, q, h - 1.0 / 3.0);
        }

        return new RGBA((int)Math.Round(r * 255), (int)Math.Round(g * 255), (int)Math.Round(b * 255), a);

        static double Hue2Rgb(double p, double q, double t)
        {
            if (t < 0)
            {
                t += 1;
            }
            if (t > 1)
            {
                t -= 1;
            }
            if (t < 1.0 / 6.0)
            {
                return p + (q - p) * 6 * t;
            }
            if (t < 1.0 / 2.0)
            {
                return q;
            }
            if (t < 2.0 / 3.0)
            {
                return p + (q - p) * (2.0 / 3.0 - t) * 6;
            }
            return p;
        }
    }

    /// <summary>
    /// 表示 RGBA（红、绿、蓝、透明度）颜色值。
    /// </summary>
    /// <param name="R">红色分量（0-255）。</param>
    /// <param name="G">绿色分量（0-255）。</param>
    /// <param name="B">蓝色分量（0-255）。</param>
    /// <param name="A">透明度（0.0-1.0）。</param>
    public readonly record struct RGBA(int R, int G, int B, double A)
    {
        /// <summary>
        /// 返回颜色的十六进制字符串表示形式。
        /// </summary>
        public override string ToString() => A == 1 ? $"#{R:x2}{G:x2}{B:x2}" : $"#{(int)(A * 255):x2}{R:x2}{G:x2}{B:x2}";
    }

    /// <summary>
    /// 表示 HSLA（色相、饱和度、亮度、透明度）颜色值。
    /// </summary>
    /// <param name="H">色相（0-360）。</param>
    /// <param name="S">饱和度（0.0-1.0）。</param>
    /// <param name="L">亮度（0.0-1.0）。</param>
    /// <param name="A">透明度（0.0-1.0）。</param>
    public readonly record struct HSLA(double H, double S, double L, double A);
}
