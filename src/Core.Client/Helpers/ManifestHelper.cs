using System.Text.Json;
using Core.All;
using Core.All.Serializable.Addon;
using SharpCompress.Archives.Zip;

namespace Core.Client.Helpers;

public static class ManifestHelper
{
    public static async Task<Result<AddonManifestJsonModel?>> GetMainManifestAsync(string pathToFile)
    {
        using var archive = ZipArchive.OpenArchive(pathToFile);
        var addonJson = archive.Entries.FirstOrDefault(static x => x.Key!.Equals("addon.json", StringComparison.OrdinalIgnoreCase));

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
