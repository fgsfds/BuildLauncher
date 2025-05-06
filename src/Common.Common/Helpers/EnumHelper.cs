using System.Diagnostics.CodeAnalysis;

namespace Common.Common.Helpers;

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
}
