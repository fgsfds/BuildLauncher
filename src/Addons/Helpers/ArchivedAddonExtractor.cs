using System.Text.Json;
using Addons.Providers;
using Core.All.Serializable.Addon;
using Core.Client.Helpers;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;

namespace Addons.Helpers;

/// <summary>
/// Inspects and conditionally extracts addon archives when they contain GRP info, RFF files, or custom executables.
/// </summary>
internal sealed class ArchivedAddonExtractor
{
    private readonly LocalFilesProvider _localFilesProvider;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArchivedAddonExtractor"/> class.
    /// </summary>
    /// <param name="localFilesProvider">Provider used to replace file paths after extraction.</param>
    /// <param name="logger">Logger for extraction errors.</param>
    public ArchivedAddonExtractor(LocalFilesProvider localFilesProvider, ILogger logger)
    {
        _localFilesProvider = localFilesProvider;
        _logger = logger;
    }

    /// <summary>
    /// If the parsed addon file is a ZIP archive, unpack it when necessary and update the local files provider.
    /// </summary>
    /// <param name="parsedAddonFile">Parsed addon file to check and optionally unpack.</param>
    /// <returns><see langword="true"/> if the file was unpacked and the path was updated; otherwise <see langword="false"/>.</returns>
    public async Task<bool> UnpackAndUpdateIfNeededAsync(ParsedAddonFile parsedAddonFile)
    {
        if (!parsedAddonFile.FileInfo.IsZip)
        {
            return false;
        }

        string? unpackedTo;

        try
        {
            using var archive = ArchiveFactory.OpenArchive(parsedAddonFile.FileInfo.PathToFile);

            string? unpackedTo1 = null;

            if (archive.Entries.Any(static x => x.Key!.Equals("addons.grpinfo", StringComparison.OrdinalIgnoreCase)))
            {
                unpackedTo1 = Unpack(parsedAddonFile.FileInfo.PathToFile, archive);
                archive.Dispose();
                File.Delete(parsedAddonFile.FileInfo.PathToFile);

                unpackedTo = unpackedTo1;
            }
            else
            {
                var addonJsonsInsideArchive = archive.Entries
                    .Where(static x => x.Key!.StartsWith("addon") && x.Key!.EndsWith(".json"))
                    .ToList();

                if (addonJsonsInsideArchive.Count == 0)
                {
                    unpackedTo = null;
                }
                else
                {
                    using var addonJsonStream = addonJsonsInsideArchive[0].OpenEntryStream();

                    var addonDto = JsonSerializer.Deserialize(
                        addonJsonStream,
                        AddonManifestJsonContext.Default.AddonManifestJsonModel
                        );

                    if (addonDto is null)
                    {
                        unpackedTo = null;
                    }
                    else
                    {
                        if (addonDto.MainRff is not null || addonDto.SoundRff is not null)
                        {
                            unpackedTo1 = Unpack(parsedAddonFile.FileInfo.PathToFile, archive);
                        }
                        else if (addonDto.Executables is not null)
                        {
                            unpackedTo1 = Unpack(parsedAddonFile.FileInfo.PathToFile, archive);
                        }

                        List<AddonManifestJsonModel> result = [];

                        if (unpackedTo1 is not null)
                        {
                            archive.Dispose();
                            File.Delete(parsedAddonFile.FileInfo.PathToFile);

                            var unpackedAddonJsons = Directory.GetFiles(unpackedTo1, "addon*.json");

                            foreach (var addonJson in unpackedAddonJsons)
                            {
                                using var text = File.OpenRead(addonJson);

                                var addonDto2 = JsonSerializer.Deserialize(
                                    text,
                                    AddonManifestJsonContext.Default.AddonManifestJsonModel
                                    )!;

                                result.Add(addonDto2);
                            }
                        }
                        else
                        {
                            foreach (var addonJson in addonJsonsInsideArchive)
                            {
                                using var addonJsonStream2 = addonJson.OpenEntryStream();

                                var addonDto2 = JsonSerializer.Deserialize(
                                    addonJsonStream2,
                                    AddonManifestJsonContext.Default.AddonManifestJsonModel
                                    )!;

                                result.Add(addonDto2);
                            }
                        }

                        unpackedTo = unpackedTo1;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while unpacking archive ===");
            unpackedTo = null;
        }

        if (unpackedTo is not null)
        {
            await _localFilesProvider.ReplacePathAsync(parsedAddonFile.FileInfo.PathToFile, unpackedTo);
            return true;
        }

        return false;
    }

    private static string Unpack(string pathToFile, IArchive archive)
    {
        var fileFolder = Path.GetDirectoryName(pathToFile)!;
        var unpackTo = Path.Combine(fileFolder, Path.GetFileNameWithoutExtension(pathToFile));

        if (Directory.Exists(unpackTo))
        {
            Directory.Delete(unpackTo, true);
        }

        if (!Directory.Exists(unpackTo))
        {
            Directory.CreateDirectory(unpackTo);
        }

        archive.WriteToDirectory(unpackTo);

        return unpackTo;
    }
}
