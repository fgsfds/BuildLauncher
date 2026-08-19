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
///     Provides functionality to manage and retrieve metadata associated with add-ons.
///     This class facilitates checking for metadata updates, initializing metadata storage,
///     and updating metadata as needed. It also provides events to notify about metadata
///     initialization and updates.
/// </summary>
public sealed class MetadataProvider : IDisposable
{
    /// <summary>
    ///     Maximum number of cached update entries to retain. Prevents unbounded growth.
    /// </summary>
    private const int MaxUpdatesCacheSize = 256;

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

    /// <summary>
    ///     Indicates whether the metadata has been successfully initialized.
    /// </summary>
    [MemberNotNullWhen(true, nameof(_metaDict))]
    public bool IsInitialized => _metaDict is not null;

    /// <summary>
    ///     Occurs when the metadata has been successfully initialized.
    /// </summary>
    public event EventHandler? MetadataInitializedEvent;

    /// <summary>
    ///     Occurs when the metadata associated with an addon file has been successfully updated.
    /// </summary>
    public event EventHandler<ParsedAddonFile>? MetadataUpdatedEvent;

    /// <summary>
    ///     Asynchronously initializes the metadata cache by retrieving metadata from the API interface.
    /// </summary>
    /// <returns>
    ///     A task representing the asynchronous operation. The task result is true if initialization succeeds, or false if it fails or metadata is unavailable.
    /// </returns>
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
    ///     Checks whether a metadata update is available for the given addon.
    /// </summary>
    /// <param name="addonId">
    ///     Addon identifier.
    /// </param>
    /// <param name="fileInfo">
    ///     Addon file path wrapper.
    /// </param>
    public bool IsMetadataUpdateAvailable(in AddonId addonId, AddonFilePathWrapper fileInfo)
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

        var originalManifestStr = JsonSerializer.Serialize(originalManifest, AddonManifestJsonContext.Default.AddonManifestJsonModel!);
        var newManifestStr = JsonSerializer.Serialize(actualVersion, AddonManifestJsonContext.Default.AddonManifestJsonModel!);

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

        if (_updatesCache.Count > MaxUpdatesCacheSize)
        {
            var oldestKey = _updatesCache.Keys.First();

            _ = _updatesCache.Remove(oldestKey);
        }

        return true;
    }

    /// <summary>
    ///     Asynchronously updates the metadata of the specified file. If the file is a zip archive,
    ///     it handles the update by extracting data, performing the necessary operations, and replacing
    ///     the file. If the file is a folder, it serializes and writes the metadata to the specified path.
    ///     An event is triggered upon successful metadata update.
    /// </summary>
    /// <param name="fileInfo">
    ///     An <see cref="AddonFilePathWrapper" /> object containing the file information, including
    ///     its path and whether it represents a zip archive or folder.
    /// </param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> representing the asynchronous operation. The task result contains
    ///     a <see cref="Result{T}" /> with a boolean indicating success or failure, as well as additional
    ///     result details.
    /// </returns>
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
                    var existing = archive.Entries.FirstOrDefault(x => x.Key?.Equals(update.FileInfo.ManifestFileName) is true);

                    if (existing is not null)
                    {
                        archive.RemoveEntry(existing);
                    }

                    using var ms = new MemoryStream();
                    await JsonSerializer.SerializeAsync(ms, update.Manifest, AddonManifestJsonContext.Default.AddonManifestJsonModel!).ConfigureAwait(false);

                    archive.AddEntry(update.FileInfo.ManifestFileName, ms);

                    archive.SaveTo(tempPath, new(CompressionType.None));
                }

                File.Move(tempPath, fileInfo.PathToFile, overwrite: true);

                _updatesCache.Remove(fileInfo);

                MetadataUpdatedEvent?.Invoke(this, update);
            }
            else if (fileInfo.IsFolder)
            {
                var addonJson = JsonSerializer.Serialize(update.Manifest, AddonManifestJsonContext.Default.AddonManifestJsonModel!);
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

    /// <inheritdoc />
    public void Dispose()
    {
        _initSemaphore.Dispose();
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

            return JsonSerializer.Deserialize(stream, AddonManifestJsonContext.Default.AddonManifestJsonModel!);
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

            return JsonSerializer.Deserialize(stream, AddonManifestJsonContext.Default.AddonManifestJsonModel!);
        }

        return null;
    }
}
