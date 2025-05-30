﻿using System.Text;
using System.Text.Json.Serialization;
using Common.Enums;
using Common.Helpers;
using Common.Serializable;

namespace Common.Common.Serializable.Downloadable;

public sealed class DownloadableAddonJsonModel
{
    [JsonPropertyName("Id")]
    public required string Id { get; set; }

    [JsonPropertyName("AddonType")]
    public required AddonTypeEnum AddonType { get; set; }

    [JsonPropertyName("Game")]
    [JsonConverter(typeof(GameEnumJsonConverter))]
    public required GameEnum Game { get; set; }

    [JsonPropertyName("DownloadUrl")]
    public required Uri DownloadUrl { get; set; }

    [JsonPropertyName("Title")]
    public required string Title { get; set; }

    [JsonPropertyName("Version")]
    public required string Version { get; set; }

    [JsonPropertyName("FileSize")]
    public required long FileSize { get; set; }

    [JsonIgnore]
    [JsonPropertyName("IsDisabled")]
    public bool IsDisabled { get; set; }

    [JsonPropertyName("Dependencies")]
    public List<string>? Dependencies { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("Author")]
    public string? Author { get; set; }

    [JsonPropertyName("Installs")]
    public int? Installs { get; set; }

    [JsonPropertyName("Rating")]
    public decimal? Rating { get; set; }

    [JsonPropertyName("UpdateDate")]
    public DateTime UpdateDate { get; set; }


    [JsonIgnore]
    public bool IsInstalled { get; set; }

    [JsonIgnore]
    public bool HasNewerVersion { get; set; }

    [JsonIgnore]
    public string RatingStr
    {
        get
        {
            if (!Rating.HasValue)
            {
                return string.Empty;
            }

            if (Rating == 0)
            {
                return "-";
            }

            return Rating.Value.ToString("0.##");
        }
    }

    [JsonIgnore]
    public string Status
    {
        get
        {
            if (HasNewerVersion)
            {
                return "Update available";
            }
            if (IsInstalled)
            {
                return "Installed";
            }

            return string.Empty;
        }
    }

    [JsonIgnore]
    public string FileSizeString => FileSize.ToSizeString();

    [JsonIgnore]
    public string UpdateDateString
    {
        get
        {
            var now = DateTime.UtcNow;
            var span = now - UpdateDate;

            if (span.TotalDays < 1)
            {
                return "Today";
            }
            else if (span.TotalDays < 2)
            {
                return "Yesterday";
            }
            else
            {
                return $"{(int)span.TotalDays} days ago";
            }
        }
    }


    public string ToMarkdownString()
    {
        StringBuilder description = new($"## {Title}");

        _ = description.Append($"{Environment.NewLine}{Environment.NewLine}#### v{Version}");

        if (Author is not null)
        {
            _ = description.Append($"{Environment.NewLine}{Environment.NewLine}*by {Author}*");
        }

        if (Description is not null)
        {
            var lines = Description.Split("\n");

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("http"))
                {
                    var line = lines[i].Trim();
                    lines[i] = $"[{line}]({line})";
                }
            }

            _ = description.Append(Environment.NewLine + Environment.NewLine).AppendJoin(Environment.NewLine + Environment.NewLine, lines);
        }

        if (Dependencies is not null)
        {
            _ = description.Append($"{Environment.NewLine}{Environment.NewLine}#### Requires:{Environment.NewLine}").AppendJoin(Environment.NewLine + Environment.NewLine, Dependencies);
        }

        return description.ToString();
    }
}


[JsonSourceGenerationOptions(
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    RespectNullableAnnotations = true,
    Converters = [
        //typeof(JsonStringEnumConverter<GameEnum>),
        typeof(GameEnumJsonConverter),
        typeof(JsonStringEnumConverter<AddonTypeEnum>)
        ]
)]
[JsonSerializable(typeof(Dictionary<GameEnum, List<DownloadableAddonJsonModel>>))]
public sealed partial class DownloadableAddonJsonModelDictionaryContext : JsonSerializerContext;
