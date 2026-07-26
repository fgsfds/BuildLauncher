using Core.All.Serializable.Addon;

namespace Addons.Helpers;

public sealed record ExtractResult(
    string? UnpackedTo,
    IReadOnlyList<AddonManifestJsonModel>? Manifests
    );
