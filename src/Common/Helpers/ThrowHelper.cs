using CommunityToolkit.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Common.Helpers;

public static class Guard2
{
    /// <summary>
    /// Throw <see cref="ArgumentException"/> if <paramref name="obj"/> is not of type <typeparamref name="T"/>
    /// otherwise, return <paramref name="obj"/> cast to <typeparamref name="T"/>
    /// </summary>
    public static void ThrowIfNotType<T>([NotNull] this object? obj, out T ret, [CallerArgumentExpression(nameof(obj))] string? name = null)
    {
        if (obj is not T objT)
        {
            ret = default!;
            ThrowHelper.ThrowFormatException(name);
            return;
        }

        ret = objT;
    }
}
