using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Core.All;
using Core.All.Enums;
using Core.All.Helpers;
using Core.All.Serializable.Addon;
using Core.Client.Cache;
using Core.Client.Enums;
using Core.Client.Helpers;
using Games.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;

namespace Addons.Providers;

/// <summary>
///     Scans the addons folder for manifests, caches parsed results, and loads grid/preview images.
/// </summary>
public sealed class LocalFilesProvider
{
    /// <summary>
    ///     Base file name of the addon manifest (without extension).
    /// </summary>
    private static readonly string ManifestNameBase = Path.GetFileNameWithoutExtension(CommonConstants.AddonManifestName);

    /// <summary>
    ///     Extension of the addon manifest file.
    /// </summary>
    private static readonly string ManifestNameExt = Path.GetExtension(CommonConstants.AddonManifestName);

    /// <summary>
    ///     Enumeration options for recursive directory searches.
    /// </summary>
    private static readonly EnumerationOptions RecursiveOptions = new()
    {
        MatchCasing = MatchCasing.CaseInsensitive,
        RecurseSubdirectories = true
    };

    /// <summary>
    ///     Enumeration options for non-recursive directory searches.
    /// </summary>
    private static readonly EnumerationOptions NonRecursiveOptions = new()
    {
        MatchCasing = MatchCasing.CaseInsensitive,
        RecurseSubdirectories = false
    };

    private readonly ICacheAdder<Stream> _bitmapsCache;

    /// <summary>
    ///     Semaphore to synchronize cache update operations.
    /// </summary>
    private readonly SemaphoreSlim _cacheUpdateSemaphore = new(1, 1);

    private readonly IChannelPublisher<DiHelper.LocalFileEvent> _channelPublisher;

    private readonly InstalledGamesProvider _gamesProvider;

    private readonly ILogger<LocalFilesProvider> _logger;

    /// <summary>
    ///     Internal cache of parsed addon files.
    /// </summary>
    private List<ParsedAddonFile>? _cachedDataDict;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LocalFilesProvider" /> class.
    /// </summary>
    /// <param name="gamesProvider">Provider for installed games.</param>
    /// <param name="bitmapsCache">Cache for bitmap images (grid/preview).</param>
    /// <param name="channelPublisher">Channel publisher for local file change events.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    public LocalFilesProvider(
        InstalledGamesProvider gamesProvider,
        [FromKeyedServices(KeyedServicesEnum.Bitmaps)] ICacheAdder<Stream> bitmapsCache,
        [FromKeyedServices(KeyedServicesEnum.LocalFilesChannel)] IChannelPublisher<DiHelper.LocalFileEvent> channelPublisher,
        ILogger<LocalFilesProvider> logger
        )
    {
        _gamesProvider = gamesProvider;
        _bitmapsCache = bitmapsCache;
        _channelPublisher = channelPublisher;
        _logger = logger;
    }

    /// <summary>
    ///     Gets whether the cache has been initialized.
    /// </summary>
    [MemberNotNullWhen(true, nameof(_cachedDataDict))]
    public bool IsInitialized => _cachedDataDict is not null;

    /// <summary>
    ///     Scan all zip and manifest files in the addons folder and populate the cache.
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        if (IsInitialized)
        {
            return true;
        }

        try
        {
            await _cacheUpdateSemaphore.WaitAsync().ConfigureAwait(false);

            if (IsInitialized)
            {
                return true;
            }

            if (!Directory.Exists(ClientProperties.AddonsFolderPath))
            {
                _cachedDataDict = [];

                return true;
            }

            var results = new List<ParsedAddonFile>();

            foreach (var zip in Directory.EnumerateFiles(ClientProperties.AddonsFolderPath, "*.zip", RecursiveOptions))
            {
                var result = await ProcessArchiveAsync(zip);

                if (result is not null)
                {
                    results.AddRange(result);
                }
            }

            var manifestPattern = $"{ManifestNameBase}*{ManifestNameExt}";

            foreach (var manifest in Directory.EnumerateFiles(ClientProperties.AddonsFolderPath, manifestPattern, RecursiveOptions))
            {
                var result = await ProcessManifestFileAsync(manifest);

                if (result is not null)
                {
                    results.Add(result);
                }
            }

            foreach (var grpinfo in Directory.EnumerateFiles(ClientProperties.AddonsFolderPath, "*.grpinfo", RecursiveOptions))
            {
                results.Add(CreateSimpleEntry(grpinfo, GameEnum.Duke3D));
            }

            foreach (var game in _gamesProvider.GetGames())
            {
                if (!Directory.Exists(game.MapsFolderPath))
                {
                    continue;
                }

                foreach (var map in Directory.EnumerateFiles(game.MapsFolderPath, "*.map", NonRecursiveOptions))
                {
                    results.Add(CreateSimpleEntry(map, game.GameEnum));
                }
            }

            _cachedDataDict = results;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to initialize addon cache");

            return false;
        }
        finally
        {
            _cacheUpdateSemaphore.Release();
        }
    }

    /// <summary>
    ///     Try to retrieve a previously cached parsed addon file by its file descriptor.
    /// </summary>
    public bool TryGetCachedAddonFile(AddonFilePathWrapper fileInfo, [NotNullWhen(true)] out ParsedAddonFile? file)
    {
        if (_cachedDataDict is null)
        {
            file = null;

            return false;
        }

        file = _cachedDataDict.FirstOrDefault(x => x.FileInfo.Equals(fileInfo));

        return file is not null;
    }

    /// <summary>
    ///     Updates all cached entries whose file path matches <paramref name="oldPathToFile" /> to point to
    ///     <paramref name="newFolderPath" />.
    /// </summary>
    /// <param name="oldPathToFile">The old file path to replace (typically a zip path).</param>
    /// <param name="newFolderPath">The new folder path.</param>
    /// <returns>The list of updated <see cref="AddonFilePathWrapper" /> entries.</returns>
    public async Task<IReadOnlyList<ParsedAddonFile>> ReplacePathAsync(string oldPathToFile, string newFolderPath)
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("Cache is not initialized.");
        }

        try
        {
            await _cacheUpdateSemaphore.WaitAsync();

            var existingFiles = _cachedDataDict.Where(x =>
                                                          x.FileInfo.PathToFile.Equals(oldPathToFile, StringComparison.OrdinalIgnoreCase) ||
                                                          x.FileInfo.PathToFolder.Equals(oldPathToFile, StringComparison.OrdinalIgnoreCase))
                                               .ToList();

            var updatedPaths = new List<ParsedAddonFile>(existingFiles.Count);

            foreach (var file in existingFiles)
            {
                var newFileInfo = file.FileInfo.WithChangedFolder(newFolderPath);

                var newFile = file with
                {
                    FileInfo = newFileInfo
                };

                _cachedDataDict.Remove(file);
                _cachedDataDict.Add(newFile);

                updatedPaths.Add(newFile);
            }

            await _channelPublisher.PublishAsync(new()
            {
                Files = [.. existingFiles],
                IsAdded = false
            });

            await _channelPublisher.PublishAsync(new()
            {
                Files = [.. updatedPaths],
                IsAdded = true
            });

            return updatedPaths;
        }
        finally
        {
            _cacheUpdateSemaphore.Release();
        }
    }

    /// <summary>
    ///     Return cached list of parsed addon files, initializing the scanner first if needed.
    /// </summary>
    public async Task<IReadOnlyList<ParsedAddonFile>> GetCachedAddonFilesAsync()
    {
        if (!IsInitialized)
        {
            await InitializeAsync().ConfigureAwait(false);
        }

        if (_cachedDataDict is null)
        {
            throw new InvalidOperationException("Initialization failed, cache is null.");
        }

        return [.. _cachedDataDict];
    }

    /// <summary>
    ///     Add a single file to the cache by parsing it as a zip archive or manifest.
    /// </summary>
    public async Task<List<ParsedAddonFile>?> TryAddFileToCacheAsync(string pathToFile, GameEnum? gameEnum)
    {
        try
        {
            await _cacheUpdateSemaphore.WaitAsync();

            var results = await ProcessFileAsync(pathToFile, gameEnum);

            if (results is not null && _cachedDataDict is not null)
            {
                _cachedDataDict.AddRange(results);

                await _channelPublisher.PublishAsync(new()
                {
                    Files = [.. results],
                    IsAdded = true
                });
            }

            return results;
        }
        finally
        {
            _cacheUpdateSemaphore.Release();
        }
    }

    /// <summary>
    ///     Remove all cached entries whose physical file matches <paramref name="pathToFile" />.
    ///     Handles zips (multiple entries), folders (manifests), .map files and .grpinfo files.
    /// </summary>
    public async Task<bool> TryRemoveFileFromCacheAsync(string pathToFile)
    {
        if (!IsInitialized)
        {
            return false;
        }

        try
        {
            await _cacheUpdateSemaphore.WaitAsync();

            var toRemove = _cachedDataDict
                          .Where(x =>
                                     x.FileInfo.PathToFile.Equals(pathToFile, StringComparison.OrdinalIgnoreCase) ||
                                     x.FileInfo.PathToFolder.Equals(pathToFile, StringComparison.OrdinalIgnoreCase))
                          .ToList();

            if (toRemove.Count == 0)
            {
                return false;
            }

            foreach (var file in toRemove)
            {
                _cachedDataDict.Remove(file);
            }

            await _channelPublisher.PublishAsync(new()
            {
                Files = [.. toRemove],
                IsAdded = false
            });

            return true;
        }
        finally
        {
            _cacheUpdateSemaphore.Release();
        }
    }

    /// <summary>
    ///     Creates a simple parsed addon file entry for a file path without a manifest.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <param name="gameEnum">Associated game enum.</param>
    private static ParsedAddonFile CreateSimpleEntry(string path, GameEnum gameEnum)
    {
        return new ParsedAddonFile
        {
            FileInfo = new(Path.GetDirectoryName(path), Path.GetFileName(path)),
            SupportedGame = gameEnum,
            Manifest = null,
            GridHash = null,
            PreviewHash = null
        };
    }

    /// <summary>
    ///     Routes a file to the appropriate parsing method based on its extension.
    /// </summary>
    /// <param name="pathToFile">Path to the file to process.</param>
    /// <param name="gameEnum">Associated game enum for .map files.</param>
    private async Task<List<ParsedAddonFile>?> ProcessFileAsync(string pathToFile, GameEnum? gameEnum)
    {
        if (pathToFile.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            var result = await ProcessArchiveAsync(pathToFile);

            if (result is not null)
            {
                return [..result];
            }
        }
        else if (pathToFile.EndsWith(ManifestNameExt, StringComparison.OrdinalIgnoreCase) &&
                 Path.GetFileName(pathToFile).StartsWith(ManifestNameBase, StringComparison.OrdinalIgnoreCase))
        {
            var result = await ProcessManifestFileAsync(pathToFile);

            if (result is not null)
            {
                return [result];
            }
        }
        else if (pathToFile.EndsWith(".grpinfo", StringComparison.OrdinalIgnoreCase))
        {
            return [CreateSimpleEntry(pathToFile, GameEnum.Duke3D)];
        }
        else if (pathToFile.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
        {
            return [CreateSimpleEntry(pathToFile, gameEnum ?? throw new InvalidOperationException("Game enum must be provided for .map files"))];
        }

        return null;
    }

    /// <summary>
    ///     Reads and parses a manifest JSON file from disk, caches associated grid and preview images, and returns a parsed addon file entry.
    /// </summary>
    /// <param name="file">Path to the manifest file.</param>
    private async Task<ParsedAddonFile?> ProcessManifestFileAsync(string file)
    {
        var folderPath = Path.GetDirectoryName(file);

        if (folderPath is null)
        {
            return null;
        }

        await using var stream = File.OpenRead(file);
        var manifest = await JsonSerializer.DeserializeAsync(stream, AddonManifestJsonContext.Default.AddonManifestJsonModel).ConfigureAwait(false);

        string? gridFile = null;
        string? previewFile = null;

        foreach (var f in Directory.EnumerateFiles(folderPath))
        {
            var name = Path.GetFileNameWithoutExtension(f.AsSpan());

            if (name.Equals("grid", StringComparison.OrdinalIgnoreCase))
            {
                gridFile = f;
            }
            else if (name.Equals("preview", StringComparison.OrdinalIgnoreCase))
            {
                previewFile = f;
            }

            if (gridFile is not null && previewFile is not null)
            {
                break;
            }
        }

        long? gridHash = null;
        long? previewHash = null;

        if (gridFile is not null)
        {
            await using var cover = File.OpenRead(gridFile);
            gridHash = Crc32Helper.GetCrc32(gridFile);
            _bitmapsCache.TryAddGridToCache(gridHash.Value, cover);
        }

        if (previewFile is not null)
        {
            await using var previewStream = File.OpenRead(previewFile);
            previewHash = Crc32Helper.GetCrc32(previewFile);
            _bitmapsCache.TryAddPreviewToCache(previewHash.Value, previewStream);
        }

        if (manifest is not null)
        {
            return new ParsedAddonFile
            {
                FileInfo = new(Path.GetDirectoryName(file), Path.GetFileName(file)),
                SupportedGame = manifest.SupportedGame.Game,
                Manifest = manifest,
                GridHash = gridHash,
                PreviewHash = previewHash
            };
        }

        return null;
    }

    /// <summary>
    ///     Opens a zip archive, extracts manifests and GRP info entries, caches grid and preview images, and returns the list of parsed addon files.
    /// </summary>
    /// <param name="file">Path to the archive file.</param>
    private async Task<List<ParsedAddonFile>?> ProcessArchiveAsync(string file)
    {
        List<ParsedAddonFile>? results = null;
        using var archive = ArchiveFactory.OpenArchive(file);

        await using var cover = ImageHelper.GetCoverFromArchive(archive);
        await using var preview = ImageHelper.GetPreviewFromArchive(archive);

        var gridHash = cover?.Crc;
        var previewHash = preview?.Crc;

        if (cover.HasValue)
        {
            _bitmapsCache.TryAddGridToCache(cover.Value.Crc, cover.Value.Stream);
        }

        if (preview.HasValue)
        {
            _bitmapsCache.TryAddPreviewToCache(preview.Value.Crc, preview.Value.Stream);
        }

        var manifests = archive.Entries.Where(x =>
                                                  x.Key?.Contains(Path.GetFileNameWithoutExtension(CommonConstants.AddonManifestName), StringComparison.OrdinalIgnoreCase) == true
                                               && x.Key.EndsWith(Path.GetExtension(CommonConstants.AddonManifestName), StringComparison.OrdinalIgnoreCase)
            );

        var grpInfos = archive.Entries.Where(x =>
                                                 x.Key?.EndsWith(".grpinfo") == true
            );

        foreach (var manifestEntry in manifests)
        {
            await using var stream = await manifestEntry.OpenEntryStreamAsync().ConfigureAwait(false);
            var manifest = await JsonSerializer.DeserializeAsync(stream, AddonManifestJsonContext.Default.AddonManifestJsonModel).ConfigureAwait(false);

            if (manifest is null)
            {
                continue;
            }

            results ??= [];

            results.Add(new ParsedAddonFile
            {
                FileInfo = new(file, manifestEntry.Key),
                SupportedGame = manifest.SupportedGame.Game,
                Manifest = manifest,
                GridHash = gridHash,
                PreviewHash = previewHash
            });
        }

        foreach (var grpInfo in grpInfos)
        {
            results ??= [];

            results.Add(new ParsedAddonFile
            {
                FileInfo = new(file, grpInfo.Key),
                SupportedGame = GameEnum.Duke3D,
                Manifest = null,
                GridHash = null,
                PreviewHash = null
            });
        }

        return results;
    }
}
