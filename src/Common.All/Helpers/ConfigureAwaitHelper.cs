using System.Runtime.CompilerServices;

namespace Common.All.Helpers;

public static class ConfigureAwaitHelper
{
    public static ConfiguredValueTaskAwaitable<T> Caf<T>(
        this ValueTask<T> task
        )
    {
        return task.ConfigureAwait(false);
    }

    public static ConfiguredValueTaskAwaitable Caf(
        this ValueTask task
        )
    {
        return task.ConfigureAwait(false);
    }

    public static ConfiguredTaskAwaitable<T> Caf<T>(
        this Task<T> task
        )
    {
        return task.ConfigureAwait(false);
    }

    public static ConfiguredTaskAwaitable Caf(
        this Task task
        )
    {
        return task.ConfigureAwait(false);
    }
}
