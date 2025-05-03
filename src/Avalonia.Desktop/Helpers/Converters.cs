using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Common.Enums;
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
        return new BindingNotification(new NotImplementedException($"ConvertBack method for {nameof(ImagePathToBitmapConverter)} is not implemented."));
    }
}

public sealed class GameStringToEnumConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not GameEnum valueEnum)
        {
            return false;
        }

        if (parameter is not string paramStr)
        {
            throw new NotImplementedException();
        }

        if (!Enum.TryParse<GameEnum>(paramStr, out var gameEnum))
        {
            throw new NotImplementedException();
        }

        return valueEnum == gameEnum;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool valueBool)
        {
            return AvaloniaProperty.UnsetValue;
        }

        if (parameter is not string paramStr)
        {
            throw new NotImplementedException();
        }

        if (!valueBool)
        {
            return AvaloniaProperty.UnsetValue;
        }

        if (!Enum.TryParse<GameEnum>(paramStr, out var gameEnum))
        {
            throw new NotImplementedException();
        }

        return gameEnum;
    }
}
