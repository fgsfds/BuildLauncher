using System.Text.Json;
using Addons.Addons;
using Addons.Helpers;
using Core.All;
using Core.All.Enums;
using Core.All.Enums.Addons;
using Core.All.Serializable.Addon;
using Core.Client.Cache;
using Core.Client.Enums;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Games.Games;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;

namespace Addons.Providers;

public sealed class InstalledAddonsProvider : IDisposable
{
    public delegate void AddonChanged(GameEnum gameEnum, AddonTypeEnum? addonType);

    internal readonly SemaphoreSlim _cacheUpdateSemaphore = new(1);

    private readonly BaseGame _game;

    private readonly List<BaseAddon> _campaignsCache = [];
    private readonly List<BaseAddon> _mapsCache = [];
    private readonly List<BaseAddon> _modsCache = [];

    private readonly AddonActivator _addonActivator;
    private readonly AddonFactory _addonFactory;
    private readonly ArchivedAddonExtractor _archivedAddonExtractor;
    private readonly ICacheAdder<Stream> _bitmapsCache;
    private readonly MetadataProvider _metadataProvider;
    private readonly OriginalCampaignsProvider _originalCampaignsProvider;
    private readonly ILogger<InstalledAddonsProvider> _logger;


    [Obsolete($"Don't create directly. Use {nameof(InstalledAddonsProviderFactory)}.")]
    public InstalledAddonsProvider(
        BaseGame game,
        IConfigProvider config,
        [FromKeyedServices(KeyedServicesEnum.Bitmaps)] ICacheAdder<Stream> bitmapsCache,
        OriginalCampaignsProvider originalCampaignsProvider,
        MetadataProvider metadataProvider,
        ArchivedAddonExtractor archivedAddonExtractor,
        ILogger<InstalledAddonsProvider> logger
        )
    {
        _game = game;
        _bitmapsCache = bitmapsCache;
        _originalCampaignsProvider = originalCampaignsProvider;
        _metadataProvider = metadataProvider;
        _archivedAddonExtractor = archivedAddonExtractor;
        _logger = logger;

        _addonFactory = new AddonFactory(game, config, metadataProvider);
        _addonActivator = new AddonActivator(config);

        _metadataProvider.MetadataUpdatedEvent += OnMetadataUpdated;
        _metadataProvider.MetadataInitializedEvent += OnMetadataInitialized;
    }

    public void Dispose()
    {
        _metadataProvider.MetadataUpdatedEvent -= OnMetadataUpdated;
        _metadataProvider.MetadataInitializedEvent -= OnMetadataInitialized;

        _cacheUpdateSemaphore.Dispose();
    }

    public event AddonChanged? AddonsChangedEvent;

    public async Task CreateCacheAsync(bool createNew, AddonTypeEnum addonType)
    {
        try
        {
            await _cacheUpdateSemaphore.WaitAsync().ConfigureAwait(false);

            var cache = GetCacheByAddonType(addonType);

            if (createNew)
            {
                cache.Clear();
            }

            switch (addonType)
            {
                case AddonTypeEnum.TC when cache.Count == 0:
                    _campaignsCache.Clear();
                    await ScanCampaignsFolderAsync();

                    break;
                case AddonTypeEnum.Map when cache.Count == 0:
                    _mapsCache.Clear();
                    await ScanMapsFolderAsync();

                    break;
                case AddonTypeEnum.Mod when cache.Count == 0:
                    _modsCache.Clear();
                    await ScanModsFolderAsync();

                    break;
                case AddonTypeEnum.Official:
                default:
                    throw new ArgumentOutOfRangeException(nameof(addonType), addonType, $"Unsupported addon type: {addonType}.");
            }

            if (addonType is AddonTypeEnum.Mod)
            {
                foreach (var mod in _modsCache)
                {
                    if (mod is not AutoloadMod autoloadMod)
                    {
                        _logger.LogError($"=== Error while enabling/disabling addon {mod.AddonId.Id}");

                        continue;
                    }

                    if (autoloadMod.IsEnabled)
                    {
                        _addonActivator.EnableAddon(mod.AddonId, _modsCache);
                    }
                    else if (!autoloadMod.IsEnabled)
                    {
                        _addonActivator.DisableAddon(mod.AddonId, _modsCache);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while creating installed addons cache ===");
        }
        finally
        {
            _ = _cacheUpdateSemaphore.Release();
        }

        AddonsChangedEvent?.Invoke(_game.GameEnum, addonType);
    }

    private async Task ScanCampaignsFolderAsync()
    {
        var campaignsPath = _game.CampaignsFolderPath;

        Ensure.DirectoryExists(campaignsPath);

        var files = new List<string>();
        files.AddRange(Directory.GetFiles(campaignsPath, "*.zip"));

        var dirs = Directory.GetDirectories(campaignsPath, "*", SearchOption.TopDirectoryOnly);

        foreach (var dir in dirs)
        {
            var addonJsons = Directory.GetFiles(dir, "addon*.json");

            if (addonJsons.Length > 0)
            {
                files.AddRange(addonJsons);
            }
        }

        foreach (var file in files)
        {
            var addons = await GetAddonFromFileAsync(file).ConfigureAwait(false);

            if (addons is null)
            {
                continue;
            }

            foreach (var addon in addons)
            {
                if (addon.Type is not AddonTypeEnum.TC)
                {
                    _logger.LogError($"File {addon.FileInfo?.FileName} of type {addon.Type} is in the Campaigns folder");

                    continue;
                }

                _campaignsCache.Add(addon);
            }
        }

        var grpInfos = Directory.GetFiles(campaignsPath, "*.grpinfo", SearchOption.AllDirectories);

        foreach (var grpInfo in grpInfos)
        {
            if (GrpInfoProvider.TryGetAddonsFromGrpInfo(grpInfo, out var grpInfoAddons))
            {
                _campaignsCache.AddRange(grpInfoAddons);
            }
        }
    }

    private async Task ScanMapsFolderAsync()
    {
        var mapsPath = _game.MapsFolderPath;

        if (!Directory.Exists(mapsPath))
        {
            return;
        }

        var files = Directory.GetFiles(mapsPath)
                             .Where(static x => x.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
                                                x.EndsWith(".map", StringComparison.OrdinalIgnoreCase)
                                  );

        foreach (var file in files)
        {
            var addons = await GetAddonFromFileAsync(file).ConfigureAwait(false);

            if (addons is null)
            {
                continue;
            }

            foreach (var addon in addons)
            {
                if (addon.Type is not AddonTypeEnum.Map)
                {
                    _logger.LogError($"File {addon.FileInfo?.FileName} of type {addon.Type} is in the Maps folder");

                    continue;
                }

                _mapsCache.Add(addon);
            }
        }
    }

    private async Task ScanModsFolderAsync()
    {
        var modsPath = _game.ModsFolderPath;

        if (!Directory.Exists(modsPath))
        {
            return;
        }

        var files = Directory.GetFiles(modsPath, "*.zip");

        foreach (var file in files)
        {
            var addons = await GetAddonFromFileAsync(file).ConfigureAwait(false);

            if (addons is null)
            {
                continue;
            }

            foreach (var addon in addons)
            {
                if (addon.Type is not AddonTypeEnum.Mod)
                {
                    _logger.LogError($"File {addon.FileInfo?.FileName} of type {addon.Type} is in the Mods folder");

                    continue;
                }

                _modsCache.Add(addon);
            }
        }
    }

    public async Task AddAddonAsync(string pathToFile)
    {
        ArgumentNullException.ThrowIfNull(_campaignsCache);
        ArgumentNullException.ThrowIfNull(_mapsCache);
        ArgumentNullException.ThrowIfNull(_modsCache);

        var addons = await GetAddonFromFileAsync(pathToFile).ConfigureAwait(false);

        if (addons is null or [])
        {
            return;
        }

        foreach (var addon in addons)
        {
            var cache = addon.Type switch
            {
                AddonTypeEnum.TC => _campaignsCache,
                AddonTypeEnum.Map => _mapsCache,
                AddonTypeEnum.Mod => _modsCache,
                AddonTypeEnum.Official => throw new NotSupportedException($"Official addons cannot be added to cache."),
                _ => throw new NotSupportedException($"Unsupported addon type: {addon.Type}.")
            };

            var existing = cache.FindIndex(x => x.AddonId == addon.AddonId);

            if (existing >= 0)
            {
                cache[existing] = addon;
            }
            else
            {
                cache.Add(addon);
            }

            AddonsChangedEvent?.Invoke(_game.GameEnum, addon.Type);
        }
    }

    public void AddAddon(ParsedAddonFile parsedAddonFile)
    {
        ArgumentNullException.ThrowIfNull(_campaignsCache);
        ArgumentNullException.ThrowIfNull(_mapsCache);
        ArgumentNullException.ThrowIfNull(_modsCache);

        BaseAddon? addon;

        if (parsedAddonFile.FileInfo.IsGrpInfo)
        {
            if (GrpInfoProvider.TryGetAddonsFromGrpInfo(parsedAddonFile.FileInfo.PathToFile, out var addons))
            {
                _campaignsCache.AddRange(addons);
            }

            AddonsChangedEvent?.Invoke(_game.GameEnum, AddonTypeEnum.TC);

            return;
        }

        if (parsedAddonFile.FileInfo.IsMap)
        {
            addon = _addonFactory.GetLooseMapFromFile(parsedAddonFile);
        }
        else if (parsedAddonFile.Manifest is not null)
        {
            addon = _addonFactory.GetAddonFromFile(parsedAddonFile);
        }
        else
        {
            return;
        }

        if (addon is null)
        {
            return;
        }

        var cache = addon.Type switch
        {
            AddonTypeEnum.TC => _campaignsCache,
            AddonTypeEnum.Map => _mapsCache,
            AddonTypeEnum.Mod => _modsCache,
            AddonTypeEnum.Official => throw new NotSupportedException($"Official addons cannot be updated in cache."),
            _ => throw new NotSupportedException($"Unsupported addon type: {addon.Type}.")
        };

        var existing = cache.FindIndex(x => x.AddonId == addon.AddonId);

        if (existing >= 0)
        {
            cache[existing] = addon;
        }
        else
        {
            cache.Add(addon);
        }

        AddonsChangedEvent?.Invoke(_game.GameEnum, addon.Type);
    }

    public void DeleteAddon(ParsedAddonFile parsedAddonFile)
    {
        if (parsedAddonFile.Manifest is null)
        {
            throw new InvalidOperationException("Cannot delete addon without a manifest");
        }

        var cache = GetCacheByAddonType(parsedAddonFile.Manifest.AddonType);
        var addonToDelete = cache.FirstOrDefault(x => x.FileInfo is not null && x.FileInfo.Equals(parsedAddonFile.FileInfo));

        if (addonToDelete is not null)
        {
            DeleteAddon(addonToDelete);
        }
    }

    public void DeleteAddon(BaseAddon addon)
    {
        ArgumentNullException.ThrowIfNull(_campaignsCache);
        ArgumentNullException.ThrowIfNull(_mapsCache);
        ArgumentNullException.ThrowIfNull(_modsCache);
        ArgumentNullException.ThrowIfNull(addon.FileInfo);

        if (addon is LooseMap map)
        {
            File.Delete(addon.FileInfo.PathToFile);

            var bloodIni = Path.Combine(addon.FileInfo.PathToFolder, map.BloodIni ?? string.Empty);

            if (map.BloodIni is not null &&
                File.Exists(bloodIni))
            {
                File.Delete(bloodIni);
            }
        }
        else if (addon.FileInfo.IsFolder)
        {
            var pathToFolder = addon.FileInfo.PathToFolder;
            var grpInfoFile = Directory.GetFiles(pathToFolder, "*.grpinfo").FirstOrDefault();

            if (grpInfoFile is not null)
            {
                var pathToFile = addon.FileInfo.PathToFile;

                if (File.Exists(pathToFile))
                {
                    File.Delete(pathToFile);
                }

                var remainingGrps = Directory.GetFiles(pathToFolder, "*.grp");

                if (remainingGrps.Length == 0)
                {
                    Directory.Delete(pathToFolder, recursive: true);
                }
            }
            else
            {
                Directory.Delete(pathToFolder, true);
            }
        }
        else
        {
            File.Delete(addon.FileInfo.PathToFile);
        }

        if (addon.Type is AddonTypeEnum.TC)
        {
            _ = _campaignsCache.Remove(addon);
        }
        else if (addon.Type is AddonTypeEnum.Map)
        {
            _ = _mapsCache.Remove(addon);
        }
        else if (addon.Type is AddonTypeEnum.Mod)
        {
            _ = _modsCache.Remove(addon);
        }

        AddonsChangedEvent?.Invoke(_game.GameEnum, addon.Type);
    }

    public void EnableAddon(AddonId addon)
    {
        _addonActivator.EnableAddon(addon, _modsCache);
    }

    public void DisableAddon(AddonId addon)
    {
        _addonActivator.DisableAddon(addon, _modsCache);
    }

    public IReadOnlyList<BaseAddon> GetInstalledAddonsByType(AddonTypeEnum addonType)
    {
        return addonType switch
        {
            AddonTypeEnum.TC => GetInstalledCampaigns(),
            AddonTypeEnum.Map => GetInstalledMaps(),
            AddonTypeEnum.Mod => GetInstalledMods(),
            _ => throw new NotSupportedException($"Unsupported addon type: {addonType}.")
        };
    }

    internal BaseAddon? GetAddonFromFile(ParsedAddonFile parsedAddonFile)
    {
        return _addonFactory.GetAddonFromFile(parsedAddonFile);
    }

    private List<BaseAddon> GetCacheByAddonType(AddonTypeEnum addonType)
    {
        var cache = addonType switch
        {
            AddonTypeEnum.TC => _campaignsCache,
            AddonTypeEnum.Map => _mapsCache,
            AddonTypeEnum.Mod => _modsCache,
            _ => throw new NotSupportedException($"Unsupported addon type: {addonType}.")
        };

        return cache;
    }

    private IReadOnlyList<BaseAddon> GetInstalledCampaigns()
    {
        var campaigns = _originalCampaignsProvider.GetOriginalCampaigns(_game);

        if (!_cacheUpdateSemaphore.Wait(1))
        {
            return [.. campaigns.Values];
        }

        try
        {
            ArgumentNullException.ThrowIfNull(_campaignsCache);

            if (_campaignsCache.Count == 0)
            {
                return [.. campaigns.Values];
            }

            if (_game.GameEnum is GameEnum.Wang)
            {
                foreach (var customCamp in _campaignsCache
                                          .OrderByDescending(static x => x.AddonId.Id.Equals(nameof(WangAddonEnum.TwinDragon), StringComparison.OrdinalIgnoreCase))
                                          .ThenByDescending(static x => x.AddonId.Id.Equals(nameof(WangAddonEnum.Wanton), StringComparison.OrdinalIgnoreCase))
                                          .ThenBy(static x => x.Title))
                {
                    campaigns[customCamp.AddonId] = customCamp;
                }
            }
            else
            {
                foreach (var customCamp in _campaignsCache.OrderBy(static x => x.Title))
                {
                    campaigns[customCamp.AddonId] = customCamp;
                }
            }

            return [.. campaigns.Values];
        }
        finally
        {
            _cacheUpdateSemaphore.Release();
        }
    }

    private IReadOnlyList<BaseAddon> GetInstalledMaps()
    {
        if (!_cacheUpdateSemaphore.Wait(1))
        {
            return [];
        }

        try
        {
            ArgumentNullException.ThrowIfNull(_mapsCache);

            return [.. _mapsCache];
        }
        finally
        {
            _cacheUpdateSemaphore.Release();
        }
    }

    private IReadOnlyList<BaseAddon> GetInstalledMods()
    {
        if (!_cacheUpdateSemaphore.Wait(1))
        {
            return [];
        }

        try
        {
            ArgumentNullException.ThrowIfNull(_modsCache);

            return [.. _modsCache];
        }
        finally
        {
            _cacheUpdateSemaphore.Release();
        }
    }

    private async Task<List<BaseAddon>?> GetAddonFromFileAsync(string pathToFile)
    {
        if (pathToFile.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return await GetAddonFromJsonFileAsync(pathToFile).ConfigureAwait(false);
        }

        if (pathToFile.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
        {
            return [GetLooseMapFromPath(pathToFile, _game.GameEnum)];
        }

        if (pathToFile.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return await GetAddonsFromArchiveAsync(pathToFile).ConfigureAwait(false);
        }

        return null;
    }

    private async Task<List<BaseAddon>> GetAddonFromJsonFileAsync(string pathToFile)
    {
        await using var jsonStream = File.OpenRead(pathToFile);
        var manifest = await JsonSerializer.DeserializeAsync(jsonStream, AddonManifestJsonContext.Default.AddonManifestJsonModel).ConfigureAwait(false);

        if (manifest is null)
        {
            return [];
        }

        var addonDir = Path.GetDirectoryName(pathToFile)!;
        var gridFile = Directory.GetFiles(addonDir, "grid.*").FirstOrDefault();
        var previewFile = Directory.GetFiles(addonDir, "preview.*").FirstOrDefault();

        long? gridImageHash = null;

        if (gridFile is not null)
        {
            gridImageHash = Crc32Helper.GetCrc32(gridFile);
            await using var stream = File.OpenRead(gridFile);
            _ = _bitmapsCache.TryAddGridToCache(gridImageHash.Value, stream);
        }

        long? previewImageHash = null;

        if (previewFile is not null)
        {
            previewImageHash = Crc32Helper.GetCrc32(previewFile);
            await using var stream = File.OpenRead(previewFile);
            _ = _bitmapsCache.TryAddPreviewToCache(previewImageHash.Value, stream);
        }

        var parsedFile = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(addonDir, Path.GetFileName(pathToFile)),
            SupportedGame = manifest.SupportedGame.Game,
            Manifest = manifest,
            GridHash = gridImageHash,
            PreviewHash = previewImageHash
        };

        var addon = _addonFactory.GetAddonFromFile(parsedFile);

        return addon is not null ? [addon] : [];
    }

    private static LooseMap GetLooseMapFromPath(string pathToFile, GameEnum gameEnum)
    {
        var bloodIni = pathToFile.Replace(".map", ".ini", StringComparison.OrdinalIgnoreCase);
        var addonDir = Path.GetDirectoryName(pathToFile)!;

        return new LooseMap
        {
            AddonId = new(Path.GetFileName(pathToFile)),
            Type = AddonTypeEnum.Map,
            Title = Path.GetFileName(pathToFile),
            SupportedGame = new GameInfo(gameEnum),
            FileInfo = new AddonFilePathWrapper(addonDir, Path.GetFileName(pathToFile)),
            StartMap = new MapFileJsonModel
            {
                File = Path.GetFileName(pathToFile)
            },
            BloodIni = File.Exists(bloodIni) ? bloodIni : null,
            GridImageHash = null,
            Description = null,
            Author = null,
            ReleaseDate = null,
            MainDef = null,
            AdditionalDefs = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            RequiredFeatures = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null,
            IsFavorite = false,
            IsMetadataUpdateAvailable = false
        };
    }

    private async Task<List<BaseAddon>?> GetAddonsFromArchiveAsync(string pathToFile)
    {
        var extractResult = await _archivedAddonExtractor.TryExtractIfNeededAsync(pathToFile).ConfigureAwait(false);

        if (extractResult is null)
        {
            return null;
        }

        if (extractResult.Manifests is null or [])
        {
            return null;
        }

        string effectivePath;
        bool isUnpacked;

        if (extractResult.UnpackedTo is not null)
        {
            effectivePath = Path.Combine(extractResult.UnpackedTo, "addon.json");
            isUnpacked = true;
        }
        else
        {
            effectivePath = pathToFile;
            isUnpacked = false;
        }

        long? gridImageHash;
        long? previewImageHash;

        if (isUnpacked)
        {
            var addonDir = Path.GetDirectoryName(effectivePath)!;
            var gridFile = Directory.GetFiles(addonDir, "grid.*").FirstOrDefault();
            var previewFile = Directory.GetFiles(addonDir, "preview.*").FirstOrDefault();

            if (gridFile is not null)
            {
                gridImageHash = Crc32Helper.GetCrc32(gridFile);
                await using var stream = File.OpenRead(gridFile);
                _ = _bitmapsCache.TryAddGridToCache(gridImageHash.Value, stream);
            }
            else
            {
                gridImageHash = null;
            }

            if (previewFile is not null)
            {
                previewImageHash = Crc32Helper.GetCrc32(previewFile);
                await using var stream = File.OpenRead(previewFile);
                _ = _bitmapsCache.TryAddPreviewToCache(previewImageHash.Value, stream);
            }
            else
            {
                previewImageHash = null;
            }
        }
        else
        {
            using var archive = ArchiveFactory.OpenArchive(pathToFile);
            await using var cover = ImageHelper.GetCoverFromArchive(archive);
            await using var preview = ImageHelper.GetPreviewFromArchive(archive);

            gridImageHash = cover?.Crc;
            previewImageHash = preview?.Crc;

            if (cover.HasValue)
            {
                _ = _bitmapsCache.TryAddGridToCache(cover.Value.Crc, cover.Value.Stream);
            }

            if (preview.HasValue)
            {
                _ = _bitmapsCache.TryAddPreviewToCache(preview.Value.Crc, preview.Value.Stream);
            }
        }

        List<BaseAddon> addons = [];

        var isZip = !isUnpacked;

        foreach (var manifest in extractResult.Manifests)
        {
            var fileInfo = isUnpacked
                ? new AddonFilePathWrapper(Path.GetDirectoryName(effectivePath)!, Path.GetFileName(effectivePath))
                : new AddonFilePathWrapper(pathToFile, $"addon{GetManifestSuffix(manifest, extractResult.Manifests)}.json");

            var parsedFile = new ParsedAddonFile
            {
                FileInfo = fileInfo,
                SupportedGame = manifest.SupportedGame.Game,
                Manifest = manifest,
                GridHash = gridImageHash,
                PreviewHash = previewImageHash
            };

            var addon = _addonFactory.GetAddonFromFile(parsedFile);

            if (addon is not null)
            {
                addons.Add(addon);
            }
        }

        return addons;
    }

    private static string GetManifestSuffix(AddonManifestJsonModel manifest, IReadOnlyList<AddonManifestJsonModel> allManifests)
    {
        if (allManifests.Count <= 1)
        {
            return string.Empty;
        }

        for (var i = 0; i < allManifests.Count; i++)
        {
            if (allManifests[i] == manifest)
            {
                return (i + 1).ToString();
            }
        }

        return string.Empty;
    }

    private void OnMetadataInitialized(object? sender, EventArgs e)
    {
        IEnumerable<BaseAddon> allAddons =
        [
            .. _campaignsCache,
            .. _mapsCache,
            .. _modsCache
        ];

        foreach (var camp in allAddons)
        {
            if (camp.FileInfo is null)
            {
                continue;
            }

            camp.IsMetadataUpdateAvailable = _metadataProvider.IsMetadataUpdateAvailable(camp.AddonId, camp.FileInfo);
        }
    }

    private void OnMetadataUpdated(object? sender, ParsedAddonFile e)
    {
        try
        {
            if (_game.GameEnum != e.Manifest.SupportedGame.Game)
            {
                return;
            }

            var cache = GetCacheByAddonType(e.Manifest.AddonType);

            var oldVersion = cache.FirstOrDefault(x => x.AddonId == e.Manifest.AddonId);

            if (oldVersion is null)
            {
                throw new InvalidOperationException($"Metadata update target not found in cache: {e.Manifest.AddonId}");
            }

            cache.Remove(oldVersion);
            AddAddon(e);

            AddonsChangedEvent?.Invoke(_game.GameEnum, e.Manifest.AddonType);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error while updating metadata.");
        }
    }
}
