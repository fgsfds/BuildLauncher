using System.Text;
using System.Text.Json.Serialization;
using Core.All.Enums;
using Core.All.Helpers;

namespace Core.All.Serializable.Downloadable;

/// <summary>
///     Represents a downloadable addon as stored in the addon database.
/// </summary>
public sealed class DownloadableAddonJsonModel
{
    /// <summary>
    ///     Gets or sets the addon identifier.
    /// </summary>
    [JsonPropertyName("Id")]
    public required string Id { get; set; }

    /// <summary>
    ///     Gets or sets the addon type.
    /// </summary>
    [JsonPropertyName("AddonType")]
    public required AddonTypeEnum AddonType { get; set; }

    /// <summary>
    ///     Gets or sets the game this addon is for.
    /// </summary>
    [JsonPropertyName("Game")]
    [JsonConverter(typeof(GameEnumJsonConverter))]
    public required GameEnum Game { get; set; }

    /// <summary>
    ///     Gets or sets the download URL.
    /// </summary>
    [JsonPropertyName("DownloadUrl")]
    public required Uri DownloadUrl { get; set; }

    /// <summary>
    ///     Gets or sets the addon title.
    /// </summary>
    [JsonPropertyName("Title")]
    public required string Title { get; set; }

    /// <summary>
    ///     Gets or sets the addon version.
    /// </summary>
    [JsonPropertyName("Version")]
    public required string Version { get; set; }

    /// <summary>
    ///     Gets or sets the file size in bytes.
    /// </summary>
    [JsonPropertyName("FileSize")]
    public required long FileSize { get; set; }

    /// <summary>
    ///     Gets or sets whether the addon is disabled.
    /// </summary>
    [JsonIgnore]
    [JsonPropertyName("IsDisabled")]
    public bool IsDisabled { get; set; }

    /// <summary>
    ///     Gets or sets the list of dependency addon identifiers.
    /// </summary>
    [JsonPropertyName("Dependencies")]
    public List<string>? Dependencies { get; set; }

    /// <summary>
    ///     Gets or sets the addon description.
    /// </summary>
    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the addon author.
    /// </summary>
    [JsonPropertyName("Author")]
    public string? Author { get; set; }

    /// <summary>
    ///     Gets or sets the number of installs.
    /// </summary>
    [JsonPropertyName("Installs")]
    public int? Installs { get; set; }

    /// <summary>
    ///     Gets or sets the addon rating.
    /// </summary>
    [JsonPropertyName("Rating")]
    public decimal? Rating { get; set; }

    /// <summary>
    ///     Gets or sets the last update date.
    /// </summary>
    [JsonPropertyName("UpdateDate")]
    public DateTime UpdateDate { get; set; }

    /// <summary>
    ///     Gets or sets the MD5 hash.
    /// </summary>
    [JsonPropertyName("MD5")]
    [Obsolete]
    public string MD5 { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the SHA-256 hash.
    /// </summary>
    [JsonPropertyName("Sha256")]
    public string Sha256 { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets whether the addon is installed.
    /// </summary>
    [JsonIgnore]
    public bool IsInstalled { get; set; }

    /// <summary>
    ///     Gets or sets whether an update is available.
    /// </summary>
    [JsonIgnore]
    public bool IsUpdateAvailable { get; set; }

    /// <summary>
    ///     Gets the formatted rating string.
    /// </summary>
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

    /// <summary>
    ///     Gets the installation status text.
    /// </summary>
    [JsonIgnore]
    public string Status
    {
        get
        {
            if (IsUpdateAvailable)
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

    /// <summary>
    ///     Gets the file size as a human-readable string.
    /// </summary>
    [JsonIgnore]
    public string FileSizeString => FileSize.ToSizeString();

    /// <summary>
    ///     Gets the update date as a relative time string.
    /// </summary>
    [JsonIgnore]
    public string UpdateDateString
    {
        get
        {
            var now = DateTime.UtcNow;
            var span = now - UpdateDate;

            return span.TotalDays switch
            {
                < 1 => "Today",
                < 2 => "Yesterday",
                _ => $"{(int)span.TotalDays} days ago"
            };
        }
    }

    /// <summary>
    ///     Converts the addon information to a Markdown-formatted string.
    /// </summary>
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


/// <summary>
///     Source generation context for <see cref="DownloadableAddonJsonModel" /> dictionary serialization.
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    RespectNullableAnnotations = true,
    Converters =
    [
        //typeof(JsonStringEnumConverter<GameEnum>),
        typeof(GameEnumJsonConverter),
        typeof(JsonStringEnumConverter<AddonTypeEnum>)
    ]
    )]
[JsonSerializable(typeof(Dictionary<GameEnum, List<DownloadableAddonJsonModel>>))]
public sealed partial class DownloadableAddonJsonModelDictionaryContext : JsonSerializerContext;
