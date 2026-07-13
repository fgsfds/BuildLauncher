using System.Text.Json;
using Core.All.Serializable.Addon;
using Core.Client.Helpers;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;

namespace Addons.Helpers;

public sealed class ArchivedAddonExtractor
{
    private readonly ILogger _logger;

    public ArchivedAddonExtractor(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<ExtractResult?> TryExtractIfNeededAsync(string pathToFile)
    {
        if (!pathToFile.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        try
        {
            using var archive = ArchiveFactory.OpenArchive(pathToFile);

            if (archive.Entries.Any(static x => x.Key?.Equals("addons.grpinfo", StringComparison.OrdinalIgnoreCase) == true))
            {
                var grpInfoUnpackedTo = Unpack(pathToFile, archive);
                archive.Dispose();
                File.Delete(pathToFile);

                return new ExtractResult(grpInfoUnpackedTo, null);
            }

            var addonJsonsInsideArchive = archive.Entries
                                                 .Where(static x => x.Key?.StartsWith("addon") == true && x.Key.EndsWith(".json"))
                                                 .ToList();

            if (addonJsonsInsideArchive.Count == 0)
            {
                return null;
            }

            using var addonJsonStream = addonJsonsInsideArchive[0].OpenEntryStream();

            var addonDto = JsonSerializer.Deserialize(
                addonJsonStream,
                AddonManifestJsonContext.Default.AddonManifestJsonModel
                );

            if (addonDto is null)
            {
                return null;
            }

            string? unpackedTo = null;

            if (addonDto.MainRff is not null || addonDto.SoundRff is not null)
            {
                unpackedTo = Unpack(pathToFile, archive);
                archive.Dispose();
                File.Delete(pathToFile);
            }
            else if (addonDto.Executables is not null)
            {
                unpackedTo = Unpack(pathToFile, archive);
                archive.Dispose();
                File.Delete(pathToFile);
            }

            List<AddonManifestJsonModel> manifests = [];

            if (unpackedTo is not null)
            {
                var unpackedAddonJsons = Directory.GetFiles(unpackedTo, "addon*.json");

                foreach (var addonJson in unpackedAddonJsons)
                {
                    await using var text = File.OpenRead(addonJson);

                    var addonDto2 = JsonSerializer.Deserialize(
                        text,
                        AddonManifestJsonContext.Default.AddonManifestJsonModel
                        )!;

                    manifests.Add(addonDto2);
                }
            }
            else
            {
                foreach (var addonJson in addonJsonsInsideArchive)
                {
                    using var addonJsonStream2 = await addonJson.OpenEntryStreamAsync();

                    var addonDto2 = JsonSerializer.Deserialize(
                        addonJsonStream2,
                        AddonManifestJsonContext.Default.AddonManifestJsonModel
                        )!;

                    manifests.Add(addonDto2);
                }
            }

            return new ExtractResult(unpackedTo, [.. manifests]);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while unpacking archive ===");

            return null;
        }
    }

    private static string Unpack(string pathToFile, IArchive archive)
    {
        var fileFolder = Path.GetDirectoryName(pathToFile) ?? throw new InvalidOperationException($"Could not determine directory for {pathToFile}");
        var unpackTo = Path.Combine(fileFolder, Path.GetFileNameWithoutExtension(pathToFile));

        if (Directory.Exists(unpackTo))
        {
            Directory.Delete(unpackTo, true);
        }

        Ensure.DirectoryExists(unpackTo);

        archive.WriteToDirectory(unpackTo);

        return unpackTo;
    }
}


public sealed record ExtractResult(
    string? UnpackedTo,
    IReadOnlyList<AddonManifestJsonModel>? Manifests
    );
