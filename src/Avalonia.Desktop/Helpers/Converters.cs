using System.Globalization;
using System.Text;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Desktop.Misc;
using Core.All.Enums;
using Core.All.Helpers;

namespace Avalonia.Desktop.Helpers;

/// <summary>
///     Converts Stream to Bitmap
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
            throw new NotSupportedException();
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
            throw new NotSupportedException();
        }

        return gameEnum;
    }
}


/// <summary>
///     Converts a string by replacing spaces with newline characters.
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
        throw new NotSupportedException();
    }
}


/// <summary>
///     Converts option parameters to a semicolon-separated list.
/// </summary>
public sealed class OptionParamsToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Dictionary<string, OptionalParameterTypeEnum> valueStr)
        {
            return string.Empty;
        }

        StringBuilder sb = new();

        foreach (var param in valueStr)
        {
            sb.Append(param.Key + ":" + param.Value.ToString() + "; ");
        }

        return sb.ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string valueStr)
        {
            return new Dictionary<string, OptionalParameterTypeEnum>();
        }

        Dictionary<string, OptionalParameterTypeEnum> result = [];

        var nameTypePair = valueStr.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var colonSeparatedPair in nameTypePair)
        {
            var fileNameExtensionPair = colonSeparatedPair.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (!Enum.TryParse<OptionalParameterTypeEnum>(fileNameExtensionPair[1], true, out var res))
            {
                throw new InvalidCastException();
            }

            result.Add(fileNameExtensionPair[0], res);
        }

        return result;
    }
}
