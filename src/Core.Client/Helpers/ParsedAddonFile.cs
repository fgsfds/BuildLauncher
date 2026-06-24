using Core.All.Enums;
using Core.All.Serializable.Addon;

namespace Core.Client.Helpers;

/// <summary>
/// Combines a parsed manifest with its file metadata and image hashes.
/// </summary>
public sealed record ParsedAddonFile
{
    /// <summary>
    /// File path wrapper that contains the addon folder or archive path and its main file name.
    /// </summary>
    public required AddonFilePathWrapper FileInfo { get; init; }

    /// <summary>
    /// The deserialized addon manifest, or null if not available.
    /// </summary>
    /// <value>The <see cref="AddonManifestJsonModel"/> instance containing addon metadata.</value>
    public required AddonManifestJsonModel? Manifest { get; init; }

    /// <summary>
    /// The game for which this addon file is intended or supported.
    /// </summary>
    public required GameEnum SupportedGame { get; init; }

    /// <summary>
    /// Gets or initializes the hash of the grid image associated with the addon file.
    /// </summary>
    public required long? GridHash { get; init; }

    /// <summary>
    /// Hash value for the preview image associated with the addon file.
    /// </summary>
    public required long? PreviewHash { get; init; }
}
