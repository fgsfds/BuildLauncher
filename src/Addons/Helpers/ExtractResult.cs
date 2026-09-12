using Core.All.Serializable.Addon;

namespace Addons.Helpers;

/// <summary>
///     Represents the result of extracting an addon archive.
/// </summary>
/// <param name="UnpackedTo">
///     The directory the archive was unpacked to, if any.
/// </param>
/// <param name="Manifests">
///     The manifests discovered in the archive, if any.
/// </param>
public sealed record ExtractResult(
    string? UnpackedTo,
    IReadOnlyList<AddonManifestJsonModel>? Manifests
    );
