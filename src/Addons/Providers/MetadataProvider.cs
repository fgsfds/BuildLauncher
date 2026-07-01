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

/// <summary>
///     Checks for and applies remote metadata updates to locally installed addons.
/// </summary>
public sealed class MetadataProvider
{
    private readonly IApiInterface _apiInterface;

    /// <summary>
    ///     Semaphore to synchronize initialization.
    /// </summary>
    private readonly SemaphoreSlim _initSemaphore = new(1, 1);

    private readonly LocalFilesProvider _localFilesProvider;

    private readonly ILogger<MetadataProvider> _logger;

    /// <summary>
    ///     Cache of pending manifest updates keyed by file info.
    /// </summary>
    private readonly Dictionary<AddonFilePathWrapper, ParsedAddonFile> _updatesCache = [];

    /// <summary>
    ///     Lookup dictionary of metadata indexed by addon ID.
    /// </summary>
    private Dictionary<AddonId, AddonManifestJsonModel>? _metaDict;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MetadataProvider" /> class.
    /// </summary>
    /// <param name="localFilesProvider">Provider for local addon files.</param>
    /// <param name="apiInterface">API interface for fetching metadata.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    public MetadataProvider(
        LocalFilesProvider localFilesProvider,
        IApiInterface apiInterface,
        ILogger<MetadataProvider> logger
        )
    {
        _localFilesProvider = localFilesProvider;
        _apiInterface = apiInterface;
        _logger = logger;
    }

    /// <summary>
    ///     Gets whether the metadata dictionary has been loaded.
    /// </summary>
    [MemberNotNullWhen(true, nameof(_metaDict))]
    public bool IsInitialized => _metaDict is not null;

    /// <summary>Raised when metadata has been loaded from the API and the lookup dictionary is ready.</summary>
    public event EventHandler? MetadataInitializedEvent;

    /// <summary>Raised when a metadata update has been applied to an addon file.</summary>
    public event EventHandler<ParsedAddonFile>? MetadataUpdatedEvent;

    /// <summary>
    ///     Load metadata from the API and build an internal lookup dictionary.
    /// </summary>
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

    /// <summary>
    ///     Return true if a newer version of the manifest is available for <paramref name="addonId" />.
    /// </summary>
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

        if (!_localFilesProvider.TryGetCachedAddonFile(fileInfo, out var originalManifest))
        {
            return false;
        }

        var originalManifestStr = JsonSerializer.Serialize(originalManifest.Manifest, AddonManifestJsonContext.Default.AddonManifestJsonModel);
        var newManifestStr = JsonSerializer.Serialize(actualVersion, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        if (originalManifestStr.Equals(newManifestStr))
        {
            return false;
        }

        var newManifest = originalManifest with
        {
            Manifest = actualVersion
        };

        if (!_updatesCache.TryAdd(newManifest.FileInfo, newManifest))
        {
            _updatesCache[newManifest.FileInfo] = newManifest;
        }

        return true;
    }

    /// <summary>
    ///     Apply a pending manifest update by rewriting the addon file on disk.
    /// </summary>
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
                    var existing = archive.Entries.FirstOrDefault(x => x.Key.Equals(Path.GetFileName(update.FileInfo.FileName)));

                    if (existing is not null)
                    {
                        archive.RemoveEntry(existing);
                    }

                    using var ms = new MemoryStream();
                    await JsonSerializer.SerializeAsync(ms, update.Manifest, AddonManifestJsonContext.Default.AddonManifestJsonModel).ConfigureAwait(false);

                    archive.AddEntry(Path.GetFileName(update.FileInfo.FileName), ms);

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
        }
        catch (Exception ex)
        {
            return new(ResultEnum.Error, false, ex.ToString());
        }

        return new(ResultEnum.Success, false, string.Empty);
    }
}
