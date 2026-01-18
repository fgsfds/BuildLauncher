using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Common.All.Helpers;

public static class EnumHelper
{
    public static bool TryParse<T>(object? value, [NotNullWhen(true)] out T? result) where T : struct, Enum
    {
        result = default;

        if (value is not string valueStr)
        {
            return false;
        }

        if (!Enum.TryParse<T>(valueStr, true, out var parseResult))
        {
            return false;
        }

        result = parseResult;
        return true;
    }

    public static long GetUniqueHash(this Enum e)
    {
        var a = e.GetType().GetHashCode();
        var b = e.GetHashCode();

        return a + b;
    }

    public static string GetDescription(this Enum value)
    {
        var type = value.GetType();

        var field = type.GetField(value.ToString());

        if (field is null)
        {
            throw new InvalidDataException();
        }

        var attr = field
            .GetCustomAttributes(typeof(DescriptionAttribute), false)
            .Cast<DescriptionAttribute>()
            .FirstOrDefault();

        if (attr is null)
        {
            throw new InvalidDataException();
        }

        return attr.Description;
    }
}
