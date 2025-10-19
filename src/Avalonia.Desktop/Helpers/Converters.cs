using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Desktop.Misc;
using Common.All.Enums;
using Common.All.Helpers;
using CommunityToolkit.Diagnostics;

namespace Avalonia.Desktop.Helpers;

/// <summary>
/// Converts Stream to Bitmap
/// </summary>
public sealed class CachedHashToBitmapConverter : IValueConverter
{
    private readonly BitmapsCache _bitmapsCache;

    public CachedHashToBitmapConverter(BitmapsCache bitmapsCache)
    {
        _bitmapsCache = bitmapsCache;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        if (value is not long valueStr)
        {
            return null;
        }

        var bitmap = _bitmapsCache.GetFromCache(valueStr);

        return bitmap;

    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new BindingNotification(new NotImplementedException($"ConvertBack method for {nameof(CachedHashToBitmapConverter)} is not implemented."));
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
            ThrowHelper.ThrowNotSupportedException();
            return null;
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
            ThrowHelper.ThrowNotSupportedException();
            return null;
        }

        return gameEnum;
    }
}

/// <summary>
/// Converts a string by replacing spaces with newline characters.
/// </summary>
public sealed class StringToWrappedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string valueStr)
        {
            return false;
        }

        return valueStr.Replace(" ", Environment.NewLine);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return ThrowHelper.ThrowNotSupportedException<object>();
    }
}
