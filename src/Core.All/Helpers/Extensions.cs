using System.Text;

namespace Core.All.Helpers;

/// <summary>
///     Provides extension methods for common types.
/// </summary>
public static class Extensions
{
    /// <summary>
    ///     Converts a file size in bytes to a human-readable string.
    /// </summary>
    /// <param name="size">File size in bytes.</param>
    public static string ToSizeString(this long size)
    {
        if (size < 1000)
        {
            return size.ToString("0") + "B";
        }
        else if (size < 1000 * 1000)
        {
            return (size / 1000).ToString("0") + "KB";
        }
        else if (size < 1000 * 1000 * 1000)
        {
            return (size * 1e-6).ToString("0") + "MB";
        }

        return (size * 1e-9).ToString("0.##") + "GB";
    }

    /// <summary>
    ///     Converts a time span to a human-readable string.
    /// </summary>
    /// <param name="time">Time span.</param>
    public static string ToTimeString(this TimeSpan time)
    {
        if (time.TotalSeconds < 1)
        {
            return "never played";
        }

        StringBuilder sb = new();

        if (time.TotalMinutes < 1)
        {
            if (time.TotalSeconds >= 2)
            {
                _ = sb.Append($" {time.Seconds} seconds");
            }
            else if (time.TotalSeconds >= 1)
            {
                _ = sb.Append($" {time.Seconds} second");
            }

            return sb.ToString();
        }


        if (time.TotalHours >= 2)
        {
            _ = sb.Append($" {(int)time.TotalHours} hours");
        }
        else if (time.TotalHours >= 1)
        {
            _ = sb.Append($" {(int)time.TotalHours} hour");
        }

        if (time.Minutes >= 2)
        {
            _ = sb.Append($" {time.Minutes} minutes");
        }
        else if (time.Minutes >= 1)
        {
            _ = sb.Append($" {time.Minutes} minute");
        }

        return sb.Replace("  ", " ").ToString();
    }

    /// <summary>
    ///     Adds a new element or replaces the value of an existing element in the dictionary.
    /// </summary>
    /// <param name="dict">Dictionary.</param>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    public static void AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value) where TKey : notnull
    {
        if (dict.ContainsKey(key))
        {
            dict[key] = value;
        }
        else
        {
            dict.Add(key, value);
        }
    }

    /// <summary>
    ///     Adds all elements from the source dictionary to the target, overwriting existing keys.
    /// </summary>
    /// <param name="target">Target dictionary.</param>
    /// <param name="source">Source dictionary.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> target, IDictionary<TKey, TValue> source)
    {
        foreach (var kv in source)
        {
            target[kv.Key] = kv.Value; // Overwrites existing keys
        }
    }

    /// <summary>
    ///     Adds all key-value pairs from the source to the target, overwriting existing keys.
    /// </summary>
    /// <param name="target">Target dictionary.</param>
    /// <param name="source">Source key-value pairs.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> target, IEnumerable<KeyValuePair<TKey, TValue>> source)
    {
        foreach (var kv in source)
        {
            target[kv.Key] = kv.Value; // Overwrites existing keys
        }
    }
}
