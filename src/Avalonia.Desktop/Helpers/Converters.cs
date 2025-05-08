using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Common.Common.Helpers;
using Common.Enums;
using Common.Helpers;
using System.Globalization;

namespace Avalonia.Desktop.Helpers;

/// <summary>
/// Converts Stream to Bitmap
/// </summary>
public sealed class StreamToBitmapConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        value.ThrowIfNotType<Stream>(out var stream);

        stream.Position = 0;

        return Bitmap.DecodeToHeight(stream, (int)DesktopConsts.GridImageHeight, BitmapInterpolationMode.HighQuality);

    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new BindingNotification(new NotImplementedException($"ConvertBack method for {nameof(StreamToBitmapConverter)} is not implemented."));
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

        if (!EnumHelper.TryParse<GameEnum>(parameter, out var gameEnum))
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

        if (!valueBool)
        {
            return AvaloniaProperty.UnsetValue;
        }

        if (!EnumHelper.TryParse<GameEnum>(parameter, out var gameEnum))
        {
            throw new NotImplementedException();
        }

        return gameEnum;
    }
}
