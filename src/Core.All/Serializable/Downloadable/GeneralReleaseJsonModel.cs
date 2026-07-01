using System.Text.Json.Serialization;
using Core.All.Enums;

namespace Core.All.Serializable.Downloadable;

/// <summary>
///     Represents a release for a specific operating system.
/// </summary>
public sealed class GeneralReleaseJsonModel
{
    /// <summary>
    ///     Gets the supported operating system.
    /// </summary>
    public required OSEnum SupportedOS { get; init; }

    /// <summary>
    ///     Gets the release version.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    ///     Gets the release description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    ///     Gets the download URL.
    /// </summary>
    public required Uri? DownloadUrl { get; init; }

    /// <summary>
    ///     Gets the file hash.
    /// </summary>
    public required string? Hash { get; init; }
}


/// <summary>
///     Source generation context for <see cref="GeneralReleaseJsonModel" /> serialization.
/// </summary>
[JsonSourceGenerationOptions(
    Converters = [typeof(JsonStringEnumConverter<OSEnum>)]
    )]
[JsonSerializable(typeof(List<GeneralReleaseJsonModel>))]
public sealed partial class GeneralReleaseJsonModelContext : JsonSerializerContext;
