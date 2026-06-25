#nullable enable
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Ribbon.Avalonia.Converters;

public sealed class NotConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // null 按 false 处理，避免抛异常导致绑定中断
        var b = (value as bool?) ?? false;
        return !b;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
