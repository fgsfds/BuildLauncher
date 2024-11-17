using System.Text;

namespace Common.Helpers;

public static class Extensions
{
    /// <summary>
    /// Convert long to readable size string
    /// </summary>
    /// <param name="size">File size</param>
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
    /// Convert timespan to readable string
    /// </summary>
    /// <param name="time">Timespan</param>
    public static string ToTimeString(this TimeSpan time)
    {
        if (time.TotalSeconds < 1)
        {
            return "never played";
        }

        StringBuilder sb = new();

        if (time.TotalMinutes < 1)
        {
            if (time.TotalSeconds > 2)
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
    /// Add new element or replace value of and existing element
    /// </summary>
    /// <param name="dict">Dictionary</param>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    public static void AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
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
}
