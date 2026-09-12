using System.Text.Json;
using Core.All.Serializable.Addon;
using Core.Client.Helpers;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;

namespace Addons.Helpers;

/// <summary>
///     Provides functionality for extracting and processing addon files from archived packages.
///     Specifically designed to handle archives containing addon-related metadata and resources.
/// </summary>
public sealed class ArchivedAddonExtractor
{
    private readonly ILogger _logger;

    public ArchivedAddonExtractor(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Tries to extract the contents of a ZIP file if the specified file path points to a valid ZIP file.
    ///     If the file is not a ZIP file, the method returns null.
    /// </summary>
    /// <param name="pathToFile">
    ///     The file path to the archive to be extracted.
    /// </param>
    /// <returns>
    ///     An <see cref="ExtractResult" /> representing the extraction results, including the directory where the contents
    ///     were unpacked and the list of extracted addon manifests, or null if the file is not a ZIP archive.
    /// </returns>
    public async Task<ExtractResult?> TryExtractIfNeededAsync(string pathToFile)
    {
        if (!pathToFile.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string? unpackedTo = null;
        IArchive? archive = null;

        try
        {
            archive = ArchiveFactory.Open(pathToFile);

            if (archive.Entries.Any(static x => x.Key?.Equals("addons.grpinfo", StringComparison.OrdinalIgnoreCase) == true))
            {
                unpackedTo = Unpack(pathToFile, archive);

                return new ExtractResult(unpackedTo, null);
            }

            var addonJsonsInsideArchive = archive.Entries
                                                 .Where(static x => x.Key?.StartsWith("addon", StringComparison.OrdinalIgnoreCase) == true &&
                                                                    x.Key.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                                                      )
                                                 .ToList();

            if (addonJsonsInsideArchive.Count == 0)
            {
                return null;
            }

            var addonJsonStream = await addonJsonsInsideArchive[0].OpenEntryStreamAsync().ConfigureAwait(false);
            await using var addonJsonStreamScope = addonJsonStream.ConfigureAwait(false);

            var addonDto = await JsonSerializer.DeserializeAsync(
                addonJsonStream,
                AddonManifestJsonContext.Default.AddonManifestJsonModel!
                ).ConfigureAwait(false);

            if (addonDto is null)
            {
                return null;
            }

            if (addonDto.MainRff is not null || addonDto.SoundRff is not null)
            {
                unpackedTo = Unpack(pathToFile, archive);
            }
            else if (addonDto.Executables is not null)
            {
                unpackedTo = Unpack(pathToFile, archive);
            }

            List<AddonManifestJsonModel> manifests = new(3);

            if (unpackedTo is not null)
            {
                var unpackedAddonJsons = Directory.GetFiles(unpackedTo, "addon*.json");

                foreach (var addonJson in unpackedAddonJsons)
                {
                    var text = File.OpenRead(addonJson);
                    await using var textScope = text.ConfigureAwait(false);

                    var addonDto2 = await JsonSerializer.DeserializeAsync(
                        text,
                        AddonManifestJsonContext.Default.AddonManifestJsonModel!
                        ).ConfigureAwait(false);

                    if (addonDto2 is null)
                    {
                        _logger.LogError("Error while deserializing {FileName}.", addonJson);

                        return null;
                    }

                    manifests.Add(addonDto2);
                }
            }
            else
            {
                foreach (var addonJson in addonJsonsInsideArchive)
                {
                    var addonJsonStream2 = await addonJson.OpenEntryStreamAsync().ConfigureAwait(false);
                    await using var addonJsonStream2Scope = addonJsonStream2.ConfigureAwait(false);

                    var addonDto2 = await JsonSerializer.DeserializeAsync(
                        addonJsonStream2,
                        AddonManifestJsonContext.Default.AddonManifestJsonModel!
                        ).ConfigureAwait(false);

                    if (addonDto2 is null)
                    {
                        _logger.LogError("Error while deserializing {FileName}.", addonJson.Key);

                        return null;
                    }

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
        finally
        {
            archive?.Dispose();

            if (unpackedTo is not null)
            {
                File.Delete(pathToFile);
            }
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
