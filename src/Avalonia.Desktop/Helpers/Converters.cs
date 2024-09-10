using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Common.Helpers;
using System.Globalization;

namespace Avalonia.Desktop.Helpers;

/// <summary>
/// Converts Stream to Bitmap
/// </summary>
public sealed class ImagePathToBitmapConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        value.ThrowIfNotType<Stream>(out var stream);

        stream.Position = 0;

        return new Bitmap(stream);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new BindingNotification(new NotImplementedException("ConvertBack method for ImagePathToBitmapConverter is not implemented."));
    }
}
