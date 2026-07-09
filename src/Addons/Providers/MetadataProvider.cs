using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Core.All;
using Core.All.Serializable.Addon;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;

namespace Addons.Providers;

public sealed class MetadataProvider
{
    private readonly IApiInterface _apiInterface;
    private readonly ILogger<MetadataProvider> _logger;

    private readonly SemaphoreSlim _initSemaphore = new(1, 1);

    private readonly Dictionary<AddonFilePathWrapper, ParsedAddonFile> _updatesCache = [];
    private Dictionary<AddonId, AddonManifestJsonModel>? _metaDict;

    public MetadataProvider(
        IApiInterface apiInterface,
        ILogger<MetadataProvider> logger
        )
    {
        _apiInterface = apiInterface;
        _logger = logger;
    }

    [MemberNotNullWhen(true, nameof(_metaDict))]
    public bool IsInitialized => _metaDict is not null;

    public event EventHandler? MetadataInitializedEvent;
    public event EventHandler<ParsedAddonFile>? MetadataUpdatedEvent;

    public async Task<bool> InitializeAsync()
    {
        if (IsInitialized)
        {
            return true;
        }

        try
        {
            await _initSemaphore.WaitAsync().ConfigureAwait(false);

            if (IsInitialized)
            {
                return true;
            }

            var metadata = await _apiInterface.GetMetadataAsync().ConfigureAwait(false);

            if (metadata is null)
            {
                return false;
            }

            _metaDict = metadata.ToDictionary(x => new AddonId(x.Id, x.Version));

            MetadataInitializedEvent?.Invoke(this, EventArgs.Empty);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to initialize metadata cache");

            return false;
        }
        finally
        {
            _initSemaphore.Release();
        }
    }

    public bool IsMetadataUpdateAvailable(AddonId addonId, AddonFilePathWrapper fileInfo)
    {
        if (_updatesCache.TryGetValue(fileInfo, out _))
        {
            return true;
        }

        if (_metaDict is null || !_metaDict.TryGetValue(addonId, out var actualVersion))
        {
            return false;
        }

        var originalManifest = ReadManifestFromDisk(fileInfo);

        if (originalManifest is null)
        {
            return false;
        }

        var originalManifestStr = JsonSerializer.Serialize(originalManifest, AddonManifestJsonContext.Default.AddonManifestJsonModel);
        var newManifestStr = JsonSerializer.Serialize(actualVersion, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        if (originalManifestStr.Equals(newManifestStr))
        {
            return false;
        }

        var newManifest = new ParsedAddonFile
        {
            FileInfo = fileInfo,
            Manifest = actualVersion,
            SupportedGame = originalManifest.SupportedGame.Game,
            GridHash = null,
            PreviewHash = null
        };

        if (!_updatesCache.TryAdd(newManifest.FileInfo, newManifest))
        {
            _updatesCache[newManifest.FileInfo] = newManifest;
        }

        return true;
    }

    public async Task<Result<bool>> UpdateMetadataAsync(AddonFilePathWrapper fileInfo)
    {
        try
        {
            if (!_updatesCache.TryGetValue(fileInfo, out var update))
            {
                return new(ResultEnum.Error, false, string.Empty);
            }

            if (fileInfo.IsZip)
            {
                var tempPath = fileInfo.PathToFile + ".temp";

                using (var archive = ZipArchive.OpenArchive(fileInfo.PathToFile))
                {
                    var existing = archive.Entries.FirstOrDefault(x => x.Key.Equals(update.FileInfo.ManifestFileName));

                    if (existing is not null)
                    {
                        archive.RemoveEntry(existing);
                    }

                    using var ms = new MemoryStream();
                    await JsonSerializer.SerializeAsync(ms, update.Manifest, AddonManifestJsonContext.Default.AddonManifestJsonModel).ConfigureAwait(false);

                    archive.AddEntry(update.FileInfo.ManifestFileName, ms);

                    archive.SaveTo(tempPath, new(CompressionType.None));
                }

                File.Delete(fileInfo.PathToFile);
                File.Move(tempPath, fileInfo.PathToFile);

                _updatesCache.Remove(fileInfo);

                MetadataUpdatedEvent?.Invoke(this, update);
            }
            else if (fileInfo.IsFolder)
            {
                var addonJson = JsonSerializer.Serialize(update.Manifest, AddonManifestJsonContext.Default.AddonManifestJsonModel);
                await File.WriteAllTextAsync(fileInfo.PathToFile, addonJson).ConfigureAwait(false);

                MetadataUpdatedEvent?.Invoke(this, update);
            }
            else
            {
                return new(ResultEnum.Error, false, $"Unsupported file type: {fileInfo}");
            }
        }
        catch (Exception ex)
        {
            return new(ResultEnum.Error, false, ex.ToString());
        }

        return new(ResultEnum.Success, false, string.Empty);
    }

    private static AddonManifestJsonModel? ReadManifestFromDisk(AddonFilePathWrapper fileInfo)
    {
        if (fileInfo.IsFolder)
        {
            if (!File.Exists(fileInfo.PathToFile))
            {
                return null;
            }

            using var stream = File.OpenRead(fileInfo.PathToFile);

            return JsonSerializer.Deserialize(stream, AddonManifestJsonContext.Default.AddonManifestJsonModel);
        }

        if (fileInfo.IsZip)
        {
            if (!File.Exists(fileInfo.PathToFile))
            {
                return null;
            }

            using var archive = ArchiveFactory.OpenArchive(fileInfo.PathToFile);
            var entry = archive.Entries.FirstOrDefault(x => x.Key?.Equals(fileInfo.ManifestFileName, StringComparison.OrdinalIgnoreCase) == true);

            if (entry is null)
            {
                return null;
            }

            using var stream = entry.OpenEntryStream();

            return JsonSerializer.Deserialize(stream, AddonManifestJsonContext.Default.AddonManifestJsonModel);
        }

        return null;
    }
}
