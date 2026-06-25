#nullable enable
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Ribbon.Avalonia.Converters;

public sealed class IsNotNullConverter : IValueConverter
{
    /// <summary>
    /// 返回值是否非 null；当 parameter 为 "invert"（忽略大小写）时取反。
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var result = value is not null;
        if (parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase))
            result = !result;
        return result;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
