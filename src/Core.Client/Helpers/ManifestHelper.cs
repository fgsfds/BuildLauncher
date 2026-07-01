using System.Text.Json;
using Core.All;
using Core.All.Serializable.Addon;
using SharpCompress.Archives.Zip;

namespace Core.Client.Helpers;

/// <summary>
///     Provides helper methods for reading and parsing addon manifests from archives.
/// </summary>
public static class ManifestHelper
{
    /// <summary>
    ///     Reads and deserializes the addon manifest from a zip archive.
    /// </summary>
    /// <param name="pathToFile">Absolute path to the zip archive.</param>
    /// <returns>A result containing the deserialized manifest, or an error if not found.</returns>
    public static async Task<Result<AddonManifestJsonModel?>> GetMainManifestAsync(string pathToFile)
    {
        using var archive = ZipArchive.OpenArchive(pathToFile);
        var addonJson = archive.Entries.FirstOrDefault(static x => x.Key?.Equals("addon.json", StringComparison.OrdinalIgnoreCase) == true);

        if (addonJson is null)
        {
            return new(ResultEnum.NotFound, null, "Can't find addon info in the provided archive.");
        }

        using var stream = await addonJson.OpenEntryStreamAsync().ConfigureAwait(false);
        var manifest = await JsonSerializer.DeserializeAsync(stream, AddonManifestJsonContext.Default.AddonManifestJsonModel).ConfigureAwait(false);

        return manifest is null
            ? new(ResultEnum.Error, null, "Error while deserializing addon.json.")
            : new(ResultEnum.Success, manifest, string.Empty);
    }
}
