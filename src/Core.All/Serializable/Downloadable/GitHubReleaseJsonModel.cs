#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
using System.Text.Json.Serialization;

namespace Core.All.Serializable.Downloadable;

/// <summary>
///     Represents a GitHub release as returned by the GitHub API.
/// </summary>
public sealed class GitHubReleaseJsonModel
{
    /// <summary>
    ///     Gets or sets the release tag name.
    /// </summary>
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; }

    /// <summary>
    ///     Gets or sets whether this is a draft release.
    /// </summary>
    [JsonPropertyName("draft")]
    public bool IsDraft { get; set; }

    /// <summary>
    ///     Gets or sets whether this is a pre-release.
    /// </summary>
    [JsonPropertyName("prerelease")]
    public bool IsPrerelease { get; set; }

    /// <summary>
    ///     Gets or sets the release assets.
    /// </summary>
    [JsonPropertyName("assets")]
    public GitHubReleaseAsset[] Assets { get; set; }

    /// <summary>
    ///     Gets or sets the release body description.
    /// </summary>
    [JsonPropertyName("body")]
    public string Description { get; set; }
}


/// <summary>
///     Source generation context for <see cref="GitHubReleaseJsonModel" /> serialization.
/// </summary>
[JsonSourceGenerationOptions(RespectNullableAnnotations = true)]
[JsonSerializable(typeof(List<GitHubReleaseJsonModel>))]
public sealed partial class GitHubReleaseEntityContext : JsonSerializerContext;


/// <summary>
///     Represents an asset attached to a GitHub release.
/// </summary>
public sealed class GitHubReleaseAsset
{
    /// <summary>
    ///     Gets or sets the asset file name.
    /// </summary>
    [JsonPropertyName("name")]
    public string FileName { get; set; }

    /// <summary>
    ///     Gets or sets the browser download URL.
    /// </summary>
    [JsonPropertyName("browser_download_url")]
    public string DownloadUrl { get; set; }

    /// <summary>
    ///     Gets or sets the last updated date.
    /// </summary>
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedDate { get; set; }

    /// <summary>
    ///     Gets or sets the file digest hash.
    /// </summary>
    [JsonPropertyName("digest")]
    public string? Digest { get; set; }
}


/// <summary>
///     Source generation context for <see cref="GitHubReleaseAsset" /> serialization.
/// </summary>
[JsonSourceGenerationOptions(RespectNullableAnnotations = true)]
[JsonSerializable(typeof(List<GitHubReleaseAsset>))]
public sealed partial class GitHubReleaseAssetContext : JsonSerializerContext;
