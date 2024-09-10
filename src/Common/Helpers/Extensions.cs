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
        if (size < 1024)
        {
            return $"{size}B";
        }
        else if (size < 1024 * 1024)
        {
            return $"{size / 1024}Kb";
        }
        else if (size < 1024 * 1024 * 1024)
        {
            return $"{size / 1024 / 1024}Mb";
        }

        return $"{size / 1024 / 1024 / 1024}Gb";
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
                sb.Append($" {time.Seconds} seconds");
            }
            else if (time.TotalSeconds >= 1)
            {
                sb.Append($" {time.Seconds} second");
            }

            return sb.ToString();
        }


        if (time.TotalHours >= 2)
        {
            sb.Append($" {(int)time.TotalHours} hours");
        }
        else if (time.TotalHours >= 1)
        {
            sb.Append($" {(int)time.TotalHours} hour");
        }

        if (time.Minutes >= 2)
        {
            sb.Append($" {time.Minutes} minutes");
        }
        else if (time.Minutes >= 1)
        {
            sb.Append($" {time.Minutes} minute");
        }

        return sb.Replace("  ", " ").ToString();
    }
}
