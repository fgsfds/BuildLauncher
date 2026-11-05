using System.Text.Json;
using Core.All.Enums;
using Core.All.Serializable.Addon;
using Core.Client.Helpers;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace Addons.Helpers;

/// <summary>
///     Provides functionality for extracting and processing addon files from archived packages.
///     Specifically designed to handle archives containing addon-related metadata and resources.
/// </summary>
public sealed class ArchivedAddonExtractor
{
    private readonly ILogger _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ArchivedAddonExtractor" /> class.
    /// </summary>
    /// <param name="logger">
    ///     The logger.
    /// </param>
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

        try
        {
            using var archive = ArchiveFactory.Open(pathToFile);
            var hasGrpInfo = archive.Entries.Any(static entry => entry.Key?.Equals("addons.grpinfo", StringComparison.OrdinalIgnoreCase) == true);
            var manifestEntries = archive.Entries.Where(static entry =>
                !entry.IsDirectory && entry.Key is not null &&
                !entry.Key.Contains('/') && !entry.Key.Contains('\\') &&
                entry.Key.StartsWith("addon", StringComparison.OrdinalIgnoreCase) &&
                entry.Key.EndsWith(".json", StringComparison.OrdinalIgnoreCase)).ToArray();

            if (!hasGrpInfo && manifestEntries.Length == 0)
            {
                return null;
            }

            var manifests = new List<AddonManifestJsonModel>();
            foreach (var entry in manifestEntries)
            {
                var stream = await entry.OpenEntryStreamAsync().ConfigureAwait(false);
                await using var streamScope = stream.ConfigureAwait(false);
                var manifest = await JsonSerializer.DeserializeAsync(stream, AddonManifestJsonContext.Default.AddonManifestJsonModel).ConfigureAwait(false);
                if (manifest is null || manifest.SupportedGame is null ||
                    string.IsNullOrWhiteSpace(manifest.Id) || string.IsNullOrWhiteSpace(manifest.Title) ||
                    !Enum.IsDefined(manifest.SupportedGame.Game) ||
                    manifest.AddonType is not (AddonTypeEnum.TC or AddonTypeEnum.Map or AddonTypeEnum.Mod))
                {
                    throw new InvalidDataException($"Invalid addon manifest: {entry.Key}");
                }

                manifests.Add(manifest);
            }

            var needsExtraction = hasGrpInfo || manifests.Any(static manifest =>
                manifest.MainRff is not null || manifest.SoundRff is not null || manifest.Executables is not null);
            if (!needsExtraction)
            {
                return new ExtractResult(null, [.. manifests]);
            }

            var destination = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(pathToFile))!, Path.GetFileNameWithoutExtension(pathToFile));
            using var replacement = new DirectoryReplacement(destination, _logger);
            archive.WriteToDirectory(replacement.StagingPath, new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
            archive.Dispose();
            replacement.Commit();
            try
            {
                File.Delete(pathToFile);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Addon installed, but its source archive could not be removed: {Path}.", pathToFile);
            }

            return new ExtractResult(destination, hasGrpInfo ? null : [.. manifests]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while preparing addon archive {Path}; the source archive was retained.", pathToFile);
            return null;
        }
    }
}
