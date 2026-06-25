#nullable enable
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Ribbon.Avalonia.Converters;

public sealed class BoundsPointToAdjustedPointConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var p = ToPoint(value);
        if (p is null) return null;

        var (dx, dy) = ParseOffset(parameter);
        return new Point(p.Value.X + dx, p.Value.Y + dy);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var p = ToPoint(value);
        if (p is null) return null;

        var (dx, dy) = ParseOffset(parameter);
        return new Point(p.Value.X - dx, p.Value.Y - dy);
    }

    private static Point? ToPoint(object? value)
    {
        return value switch
        {
            Point pt => pt,
            PixelPoint ppt => new Point(ppt.X, ppt.Y),
            ValueTuple<double, double> t => new Point(t.Item1, t.Item2),
            _ => null
        };
    }

    private static (double dx, double dy) ParseOffset(object? parameter)
    {
        switch (parameter)
        {
            case null:
                return (0, 0);
            case string s when !string.IsNullOrWhiteSpace(s):
                {
                    // "dx,dy"
                    var parts = s.Split(',');
                    if (parts.Length >= 2 &&
                        double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var dx) &&
                        double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var dy))
                        return (dx, dy);
                    return (0, 0);
                }
            case Point pt:
                return (pt.X, pt.Y);
            case Vector v:
                return (v.X, v.Y);
            case (double dx, double dy):
                return (dx, dy);
            case Thickness th:
                return (th.Left, th.Top);
            default:
                return (0, 0);
        }
    }
}
