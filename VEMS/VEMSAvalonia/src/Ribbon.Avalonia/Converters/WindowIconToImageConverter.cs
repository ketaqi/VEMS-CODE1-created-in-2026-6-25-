#nullable enable
using System;
using System.Globalization;
using System.IO;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace Ribbon.Avalonia.Converters;

public sealed class WindowIconToImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not WindowIcon icon)
            return null;

        try
        {
            using var ms = new MemoryStream();
            icon.Save(ms);
            ms.Position = 0;
            // Bitmap(Stream) 会读取流内容；这里创建后即可释放 ms
            return new Bitmap(ms);
        }
        catch
        {
            // 任何格式/流错误都返回 null，避免在绑定管线抛异常
            return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
