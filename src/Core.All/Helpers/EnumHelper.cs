using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Core.All.Helpers;

/// <summary>
///     Provides helper methods for enum operations.
/// </summary>
public static class EnumHelper
{
    /// <summary>
    ///     Tries to parse a value into the specified enum type.
    /// </summary>
    /// <param name="value">Value to parse.</param>
    /// <param name="result">Parsed enum value, or default if parsing failed.</param>
    /// <typeparam name="T">Enum type.</typeparam>
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

    /// <summary>
    ///     Gets a unique hash value for an enum value by combining its type hash and value hash.
    /// </summary>
    /// <param name="e">Enum value.</param>
    public static long GetUniqueHash(this Enum e)
    {
        var a = e.GetType().GetHashCode();
        var b = e.GetHashCode();

        return a + b;
    }

    /// <summary>
    ///     Gets the description from a <see cref="DescriptionAttribute" /> on an enum value.
    /// </summary>
    /// <param name="value">Enum value.</param>
    /// <exception cref="InvalidDataException">Thrown when no description attribute is found.</exception>
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
