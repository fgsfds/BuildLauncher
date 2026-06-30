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
/// Scans the addons folder for manifests, caches parsed results, and loads grid/preview images.
/// </summary>
public sealed class LocalFilesProvider
{
    private static readonly string _manifestNameBase = Path.GetFileNameWithoutExtension(CommonConstants.AddonManifestName);
    private static readonly string _manifestNameExt = Path.GetExtension(CommonConstants.AddonManifestName);

    private readonly InstalledGamesProvider _gamesProvider;
    private readonly ICacheAdder<Stream> _bitmapsCache;
    private readonly IChannelPublisher<DiHelper.LocalFileEvent> _channelPublisher;
    private readonly ILogger<LocalFilesProvider> _logger;
    private readonly SemaphoreSlim _cacheUpdateSemaphore = new(1, 1);

    private List<ParsedAddonFile>? _cachedDataDict;

    private static readonly EnumerationOptions RecursiveOptions = new()
    {
        MatchCasing = MatchCasing.CaseInsensitive,
        RecurseSubdirectories = true
    };

    private static readonly EnumerationOptions NonRecursiveOptions = new()
    {
        MatchCasing = MatchCasing.CaseInsensitive,
        RecurseSubdirectories = false
    };

    [MemberNotNullWhen(true, nameof(_cachedDataDict))]
    public bool IsInitialized => _cachedDataDict is not null;

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
    /// Scan all zip and manifest files in the addons folder and populate the cache.
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

            var manifestPattern = $"{_manifestNameBase}*{_manifestNameExt}";
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
    /// Try to retrieve a previously cached parsed addon file by its file descriptor.
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
    /// Updates all cached entries whose file path matches <paramref name="oldPathToFile"/> to point to <paramref name="newFolderPath"/>.
    /// </summary>
    /// <param name="oldPathToFile">The old file path to replace (typically a zip path).</param>
    /// <param name="newFolderPath">The new folder path.</param>
    /// <returns>The list of updated <see cref="AddonFilePathWrapper"/> entries.</returns>
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
                    x.FileInfo.PathToFile.Equals(oldPathToFile, StringComparison.InvariantCultureIgnoreCase) ||
                    x.FileInfo.PathToFolder.Equals(oldPathToFile, StringComparison.InvariantCultureIgnoreCase))
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

            await _channelPublisher.PublishAsync(new() { Files = [.. existingFiles], IsAdded = false});
            await _channelPublisher.PublishAsync(new() { Files = [.. updatedPaths], IsAdded = true});

            return updatedPaths;
        }
        finally
        {
            _cacheUpdateSemaphore.Release();
        }
    }

    /// <summary>
    /// Return cached list of parsed addon files, initializing the scanner first if needed.
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
    /// Add a single file to the cache by parsing it as a zip archive or manifest.
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
    /// Remove all cached entries whose physical file matches <paramref name="pathToFile"/>.
    /// Handles zips (multiple entries), folders (manifests), .map files and .grpinfo files.
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
                    x.FileInfo.PathToFile.Equals(pathToFile, StringComparison.InvariantCultureIgnoreCase) ||
                    x.FileInfo.PathToFolder.Equals(pathToFile, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            if (toRemove.Count == 0)
            {
                return false;
            }

            foreach (var file in toRemove)
            {
                _cachedDataDict.Remove(file);
            }

            await _channelPublisher.PublishAsync(new() { Files = [.. toRemove], IsAdded = false});
            return true;
        }
        finally
        {
            _cacheUpdateSemaphore.Release();
        }
    }

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
        else if (pathToFile.EndsWith(_manifestNameExt, StringComparison.OrdinalIgnoreCase) &&
            Path.GetFileName(pathToFile).StartsWith(_manifestNameBase, StringComparison.OrdinalIgnoreCase))
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
    /// Parse a single JSON manifest file and load its grid/preview images.
    /// </summary>
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
                gridFile = f;
            else if (name.Equals("preview", StringComparison.OrdinalIgnoreCase))
                previewFile = f;

            if (gridFile is not null && previewFile is not null)
                break;
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
    /// Parse manifests inside a zip archive and load any embedded grid/preview images.
    /// </summary>
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
            && x.Key.EndsWith(Path.GetExtension(CommonConstants.AddonManifestName))
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
