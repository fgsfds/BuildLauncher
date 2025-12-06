using System.Collections.Immutable;
using System.Text.Json;
using Addons.Addons;
using Common.All;
using Common.All.Enums;
using Common.All.Enums.Addons;
using Common.All.Helpers;
using Common.All.Interfaces;
using Common.All.Serializable.Addon;
using Common.Client.Cache;
using Common.Client.Helpers;
using Common.Client.Interfaces;
using CommunityToolkit.Diagnostics;
using Games.Games;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;

namespace Addons.Providers;

/// <summary>
/// Class that provides lists of installed mods
/// </summary>
public sealed class InstalledAddonsProvider
{
    private readonly BaseGame _game;
    private readonly IConfigProvider _config;
    private readonly ILogger _logger;
    private readonly ICacheAdder<Stream> _bitmapsCache;
    private readonly OriginalCampaignsProvider _originalCampaignsProvider;

    private readonly Dictionary<AddonId, BaseAddon> _campaignsCache = [];
    private readonly Dictionary<AddonId, BaseAddon> _mapsCache = [];
    private readonly Dictionary<AddonId, BaseAddon> _modsCache = [];

    private readonly SemaphoreSlim _semaphore = new(1);

    private volatile bool _isCacheUpdating;

    public event AddonChanged? AddonsChangedEvent;

    [Obsolete($"Don't create directly. Use {nameof(InstalledAddonsProviderFactory)}.")]
    public InstalledAddonsProvider(
        BaseGame game,
        IConfigProvider config,
        ILogger logger,
        [FromKeyedServices("Bitmaps")] ICacheAdder<Stream> bitmapsCache,
        OriginalCampaignsProvider originalCampaignsProvider
        )
    {
        _game = game;
        _config = config;
        _logger = logger;
        _bitmapsCache = bitmapsCache;
        _originalCampaignsProvider = originalCampaignsProvider;
    }


    /// <summary>
    /// Try to parse and copy addon into suitable folder
    /// </summary>
    /// <param name="pathToFile">Path to addon file</param>
    /// <returns>Copied successfully</returns>
    public async Task<bool> CopyAddonIntoFolder(string pathToFile)
    {
        if (!pathToFile.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
            !pathToFile.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError("File is not .zip or .map.");
            return false;
        }

        var addon = GetGameAndTypeFromFile(pathToFile);

        if (addon is null)
        {
            _logger.LogError("Can't get addon from the file.");
            return false;
        }

        if (addon.Item1 != _game.GameEnum)
        {
            _logger.LogError($"Addon is for the wrong game: {addon.Item1}.");
            return false;
        }

        string folderToPutFile;

        if (addon.Item2 is AddonTypeEnum.TC)
        {
            folderToPutFile = _game.CampaignsFolderPath;
        }
        else if (addon.Item2 is AddonTypeEnum.Map)
        {
            folderToPutFile = _game.MapsFolderPath;
        }
        else if (addon.Item2 is AddonTypeEnum.Mod)
        {
            folderToPutFile = _game.ModsFolderPath;
        }
        else
        {
            _logger.LogError($"Unknown addon type: {addon.Item2}.");
            return false;
        }

        var newPathToFile = Path.Combine(folderToPutFile, Path.GetFileName(pathToFile));

        File.Copy(pathToFile, newPathToFile, true);

        await AddAddonAsync(newPathToFile).ConfigureAwait(false);

        return true;
    }


    /// <summary>
    /// Create cache of installed addons.
    /// </summary>
    /// <param name="createNew">Clear current cache and create new.</param>
    /// <param name="addonType">Addon type.</param>
    public async Task CreateCache(bool createNew, AddonTypeEnum addonType)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        _isCacheUpdating = true;

        var cache = addonType switch
        {
            AddonTypeEnum.TC => _campaignsCache,
            AddonTypeEnum.Map => _mapsCache,
            AddonTypeEnum.Mod => _modsCache,
            _ => ThrowHelper.ThrowNotSupportedException<Dictionary<AddonId, BaseAddon>>(),
        };

        if (createNew)
        {
            cache.Clear();
        }

        try
        {
            if (_campaignsCache.Count == 0)
            {
                //campaigns
                List<string> filesTcs = [.. Directory.GetFiles(_game.CampaignsFolderPath, "*.zip")];

                var dirs = Directory.GetDirectories(_game.CampaignsFolderPath, "*", SearchOption.TopDirectoryOnly);

                foreach (var dir in dirs)
                {
                    var addonJsons = Directory.GetFiles(dir, "addon*.json");

                    if (addonJsons.Length > 0)
                    {
                        filesTcs.AddRange(addonJsons);
                    }
                }

                var tcs = await GetAddonsFromFilesAsync(filesTcs).ConfigureAwait(false);

                foreach (var wrongFile in tcs.Where(x => x.Value.Type is not AddonTypeEnum.TC))
                {
                    _logger.LogError($"File {wrongFile.Value.FileName} of type {wrongFile.Value.Type} is in the Campaigns folder");
                }

                _campaignsCache.AddRange(tcs);

                //grpinfo
                var grpInfoAddons = GrpInfoProvider.GetAddonsFromGrpInfo(_game.CampaignsFolderPath);

                if (grpInfoAddons?.Count > 0)
                {
                    foreach (var addon in grpInfoAddons)
                    {
                        var result = _campaignsCache.TryAdd(new(addon.AddonId.Id, null), addon);

                        if (!result)
                        {
                            _logger.LogError($"Failed to add {addon.FileName} to the campaigns list.");
                        }
                    }
                }
            }


            if (_mapsCache.Count == 0)
            {
                //maps
                var filesMaps = Directory.GetFiles(_game.MapsFolderPath).Where(static x => x.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || x.EndsWith(".map", StringComparison.OrdinalIgnoreCase));
                var maps = await GetAddonsFromFilesAsync(filesMaps).ConfigureAwait(false);

                foreach (var wrongFile in maps.Where(x => x.Value.Type is not AddonTypeEnum.Map))
                {
                    _logger.LogError($"File {wrongFile.Value.FileName} of type {wrongFile.Value.Type} is in the Maps folder");
                }

                _mapsCache.AddRange(maps);
            }


            if (_modsCache.Count == 0)
            {
                //mods
                var filesMods = Directory.GetFiles(_game.ModsFolderPath, "*.zip");
                var mods = await GetAddonsFromFilesAsync(filesMods).ConfigureAwait(false);

                foreach (var wrongFile in mods.Where(x => x.Value.Type is not AddonTypeEnum.Mod))
                {
                    _logger.LogError($"File {wrongFile.Value.FileName} of type {wrongFile.Value.Type} is in the Mods folder");
                }

                _modsCache.AddRange(mods);
            }


            //enabling/disabling addons
            foreach (var mod in _modsCache)
            {
                if (mod.Value is not AutoloadModEntity autoloadMod)
                {
                    _logger.LogError($"=== Error while enabling/disabling addon {mod.Key.Id}");
                    continue;
                }

                if (autoloadMod.IsEnabled)
                {
                    EnableAddon(mod.Key);
                }
                else if (!autoloadMod.IsEnabled)
                {
                    DisableAddon(mod.Key);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while creating installed addons cache ===");
        }
        finally
        {
            _isCacheUpdating = false;
            _ = _semaphore.Release();
            Guard.IsNotNull(_campaignsCache);
            Guard.IsNotNull(_mapsCache);
            Guard.IsNotNull(_modsCache);

            AddonsChangedEvent?.Invoke(_game.GameEnum, addonType);
        }
    }

    /// <summary>
    /// Add addon to cache
    /// </summary>
    /// <param name="pathToFile">Path to addon file</param>
    public async Task AddAddonAsync(string pathToFile)
    {
        Guard.IsNotNull(_campaignsCache);
        Guard.IsNotNull(_mapsCache);
        Guard.IsNotNull(_modsCache);

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
                _ => ThrowHelper.ThrowNotSupportedException<Dictionary<AddonId, BaseAddon>>(),
            };


            if (cache.TryGetValue(addon.AddonId, out _))
            {
                cache[addon.AddonId] = addon;
            }
            else
            {
                cache.Add(addon.AddonId, addon);
            }

            AddonsChangedEvent?.Invoke(_game.GameEnum, addon.Type);
        }
    }

    /// <summary>
    /// Delete addon from cache and disk
    /// </summary>
    /// <param name="addon">Addon</param>
    public void DeleteAddon(BaseAddon addon)
    {
        Guard.IsNotNull(_campaignsCache);
        Guard.IsNotNull(_mapsCache);
        Guard.IsNotNull(_modsCache);
        Guard.IsNotNull(addon.PathToFile);

        if (addon.IsUnpacked)
        {
            Directory.Delete(Path.GetDirectoryName(addon.PathToFile)!, true);
        }
        else
        {
            File.Delete(addon.PathToFile);
        }

        if (addon is LooseMapEntity lMap)
        {
            var files = Directory.GetFiles(Path.GetDirectoryName(addon.PathToFile)!, $"{Path.GetFileNameWithoutExtension(lMap.FileName)!}.*");

            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        if (addon.Type is AddonTypeEnum.TC)
        {
            _ = _campaignsCache.Remove(addon.AddonId);
        }
        else if (addon.Type is AddonTypeEnum.Map)
        {
            _ = _mapsCache.Remove(addon.AddonId);
        }
        else if (addon.Type is AddonTypeEnum.Mod)
        {
            _ = _modsCache.Remove(addon.AddonId);
        }

        AddonsChangedEvent?.Invoke(_game.GameEnum, addon.Type);
    }

    /// <summary>
    /// Enable addon
    /// </summary>
    /// <param name="id">Addon id</param>
    public void EnableAddon(AddonId addon)
    {
        var existing = _modsCache.FirstOrDefault(x => x.Key.Equals(addon));

        if (existing.Value is not AutoloadModEntity autoloadMod)
        {
            return;
        }

        if (autoloadMod.IsEnabled)
        {
            return;
        }

        autoloadMod.IsEnabled = true;

        if (autoloadMod.DependentAddons is not null)
        {
            foreach (var dep in autoloadMod.DependentAddons)
            {
                EnableAddon(new(dep.Key, dep.Value));
            }
        }

        if (autoloadMod.IncompatibleAddons is not null)
        {
            foreach (var inc in autoloadMod.IncompatibleAddons)
            {
                DisableAddon(new(inc.Key, inc.Value));
            }
        }

        var otherVersions = _modsCache
             .Where(x =>
                 x.Key.Id == addon.Id &&
                 !VersionComparer.Compare(x.Key.Version, addon.Version, "==") &&
                 !x.Value.FileName!.Equals(autoloadMod.FileName)
                 );

        foreach (var version in otherVersions)
        {
            DisableAddon(version.Key);
        }

        _config.ChangeModState(addon, true);
    }

    /// <summary>
    /// Disable addon
    /// </summary>
    /// <param name="id">Addon id</param>
    public void DisableAddon(AddonId addon)
    {
        var existing = _modsCache.FirstOrDefault(x => x.Key.Equals(addon));

        if (existing.Value is not AutoloadModEntity autoloadMod)
        {
            return;
        }

        if (!autoloadMod.IsEnabled)
        {
            return;
        }

        autoloadMod.IsEnabled = false;

        var deps = _modsCache.Where(x => x.Value.DependentAddons?.ContainsKey(autoloadMod.AddonId.Id) ?? false);

        foreach (var dep in deps)
        {
            DisableAddon(dep.Key);
        }

        _config.ChangeModState(addon, false);
    }

    /// <summary>
    /// Get list od installed addons of a type
    /// </summary>
    /// <param name="addonType">Addon type</param>
    public IReadOnlyDictionary<AddonId, BaseAddon> GetInstalledAddonsByType(AddonTypeEnum addonType)
    {
        return addonType switch
        {
            AddonTypeEnum.TC => GetInstalledCampaigns(),
            AddonTypeEnum.Map => GetInstalledMaps(),
            AddonTypeEnum.Mod => GetInstalledMods(),
            _ => ThrowHelper.ThrowNotSupportedException<Dictionary<AddonId, BaseAddon>>()
        };
    }

    private IReadOnlyDictionary<AddonId, BaseAddon> GetInstalledCampaigns()
    {
        var campaigns = _originalCampaignsProvider.GetOriginalCampaigns(_game);

        if (_isCacheUpdating)
        {
            return campaigns;
        }

        Guard.IsNotNull(_campaignsCache);

        if (_campaignsCache.Count == 0)
        {
            return campaigns;
        }

        if (_game.GameEnum is GameEnum.Wang)
        {
            //hack to make SW addons appear at the top of the list
            foreach (var customCamp in _campaignsCache
                .OrderByDescending(static x => x.Key.Id.Equals(nameof(WangAddonEnum.TwinDragon), StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(static x => x.Key.Id.Equals(nameof(WangAddonEnum.Wanton), StringComparison.OrdinalIgnoreCase))
                .ThenBy(static x => x.Value.Title))
            {
                campaigns.Add(customCamp.Key, customCamp.Value);
            }
        }
        else
        {
            foreach (var customCamp in _campaignsCache.OrderBy(static x => x.Value.Title))
            {
                campaigns.Add(customCamp.Key, customCamp.Value);
            }
        }

        return campaigns;
    }

    private IReadOnlyDictionary<AddonId, BaseAddon> GetInstalledMaps()
    {
        if (_isCacheUpdating)
        {
            return new Dictionary<AddonId, BaseAddon>();
        }

        Guard.IsNotNull(_mapsCache);

        return _mapsCache;
    }

    private IReadOnlyDictionary<AddonId, BaseAddon> GetInstalledMods()
    {
        if (_isCacheUpdating)
        {
            return new Dictionary<AddonId, BaseAddon>();
        }

        Guard.IsNotNull(_modsCache);

        return _modsCache;
    }

    /// <summary>
    /// Get addons from list of files
    /// </summary>
    /// <param name="files">Paths to addon files</param>
    private async Task<Dictionary<AddonId, BaseAddon>> GetAddonsFromFilesAsync(IEnumerable<string> files)
    {
        Dictionary<AddonId, BaseAddon> addedAddons = [];

        foreach (var file in files)
        {
            try
            {
                var newAddons = await GetAddonFromFileAsync(file).ConfigureAwait(false);

                if (newAddons is null or [])
                {
                    _logger.LogInformation($"Can't get addon from file {file}");
                    continue;
                }

                foreach (var newAddon in newAddons)
                {
                    try
                    {
                        if (newAddon is AutoloadModEntity &&
                            addedAddons.TryGetValue(newAddon.AddonId, out var existingMod))
                        {
                            if (existingMod.AddonId.Version is null &&
                                newAddon.AddonId.Version is not null)
                            {
                                //replacing with addon that have version
                                addedAddons[newAddon.AddonId] = newAddon;
                            }
                            else if (existingMod.AddonId.Version is not null &&
                                     newAddon.AddonId.Version is not null &&
                                     VersionComparer.Compare(newAddon.AddonId.Version, existingMod.AddonId.Version, ">"))
                            {
                                //replacing with addon that have higher version
                                addedAddons[newAddon.AddonId] = newAddon;
                            }
                        }
                        else
                        {
                            _ = addedAddons.TryAdd(newAddon.AddonId, newAddon);
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            catch (Exception)
            {
                continue;
            }
        }

        return addedAddons;
    }

    /// <summary>
    /// Get game enum and addon type enum from a file
    /// </summary>
    /// <param name="pathToFile">Path to file</param>
    private Tuple<GameEnum, AddonTypeEnum>? GetGameAndTypeFromFile(string pathToFile)
    {
        if (ArchiveFactory.IsArchive(pathToFile, out _))
        {
            using var archive = ArchiveFactory.Open(pathToFile);
            var manifestFile = archive.Entries.FirstOrDefault(static x => x.Key!.Equals("addon.json", StringComparison.OrdinalIgnoreCase));

            if (manifestFile is null)
            {
                return null;
            }

            using var stream = manifestFile.OpenEntryStream();

            var manifest = JsonSerializer.Deserialize(
                stream,
                AddonManifestContext.Default.AddonJsonModel
                );

            if (manifest is null)
            {
                return null;
            }

            var supportedGame = manifest.SupportedGame.Game;
            var type = manifest.AddonType;

            return new(supportedGame, type);
        }
        else if (pathToFile.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
        {
            return new(_game.GameEnum, AddonTypeEnum.Map);
        }

        return null;
    }

    /// <summary>
    /// Get addon from a file
    /// </summary>
    /// <param name="pathToFile">Path to addon file</param>
    private async Task<List<BaseAddon>?> GetAddonFromFileAsync(string pathToFile)
    {
        List<AddonCarcass> carcasses = [];

        if (pathToFile.EndsWith(".json"))
        {
            AddonCarcass carcass = new();

            carcass.IsUnpacked = true;

            var jsonText = File.ReadAllText(pathToFile);

            var manifest = JsonSerializer.Deserialize(
                jsonText,
                AddonManifestContext.Default.AddonJsonModel
                );

            if (manifest is null)
            {
                return null;
            }

            var addonDir = Path.GetDirectoryName(pathToFile)!;

            var gridFile = Directory.GetFiles(addonDir, "grid.*");
            var previewFile = Directory.GetFiles(addonDir, "preview.*");

            if (gridFile.Length > 0)
            {
                var crc = Crc32Helper.GetCrc32(gridFile[0]);
                await using var stream = File.OpenRead(gridFile[0]);
                _ = _bitmapsCache.TryAddGridToCache(crc, stream);
                carcass.GridImageHash = crc;
            }
            else
            {
                carcass.GridImageHash = null;
            }

            if (previewFile.Length > 0)
            {
                var crc = Crc32Helper.GetCrc32(previewFile[0]);
                await using var stream = File.OpenRead(previewFile[0]);
                _ = _bitmapsCache.TryAddPreviewToCache(crc, stream);
                carcass.PreviewImageHash = crc;
            }
            else
            {
                carcass.PreviewImageHash = null;
            }

            carcass.Type = manifest.AddonType;
            carcass.Id = manifest.Id;
            carcass.Title = manifest.Title;
            carcass.Author = manifest.Author;
            carcass.GridImageHash ??= carcass.PreviewImageHash;
            carcass.Version = manifest.Version;
            carcass.Description = manifest.Description;
            carcass.SupportedGame = manifest.SupportedGame.Game;
            carcass.GameVersion = manifest.SupportedGame.Version;
            carcass.GameCrc = manifest.SupportedGame.Crc;

            carcass.Rts = manifest.Rts;
            carcass.Ini = manifest.Ini;
            carcass.Rff = manifest.MainRff;
            carcass.Snd = manifest.SoundRff;

            carcass.StartMap = manifest.StartMap;

            carcass.RequiredFeatures = manifest.Dependencies?.RequiredFeatures?.Select(static x => x).ToImmutableArray();

            carcass.MainCon = manifest.MainCon;
            carcass.AddCons = manifest.AdditionalCons?.ToImmutableArray();

            carcass.MainDef = manifest.MainDef;
            carcass.AddDefs = manifest.AdditionalDefs?.ToImmutableArray();

            carcass.Dependencies = manifest.Dependencies?.Addons?.ToDictionary(static x => x.Id, static x => x.Version, StringComparer.OrdinalIgnoreCase);
            carcass.Incompatibles = manifest.Incompatibles?.Addons?.ToDictionary(static x => x.Id, static x => x.Version, StringComparer.OrdinalIgnoreCase);

            if (manifest.Executables is not null)
            {
                carcass.Executables = [];

                foreach (var osPortsPair in manifest.Executables)
                {
                    carcass.Executables.Add(osPortsPair.Key, []);

                    foreach (var x in osPortsPair.Value)
                    {
                        carcass.Executables[osPortsPair.Key].Add(x.Key, Path.Combine(Path.GetDirectoryName(pathToFile)!, x.Value));
                    }
                }
            }
            else
            {
                carcass.Executables = null;
            }

            if (manifest.Options is not null)
            {
                carcass.Options = [];

                foreach (var option in manifest.Options)
                {
                    carcass.Options.Add(option.OptionName, []);

                    if (option.Parameters is not null)
                    {
                        foreach (var param in option.Parameters)
                        {
                            carcass.Options[option.OptionName].Add(param.Key, param.Value);
                        }
                    }
                }
            }
            else
            {
                carcass.Options = null;
            }

            carcasses.Add(carcass);
        }
        else if (pathToFile.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
        {
            var bloodIni = pathToFile.Replace(".map", ".ini", StringComparison.OrdinalIgnoreCase);
            var iniExists = File.Exists(bloodIni);
            AddonId id = new(Path.GetFileName(pathToFile), null);

            var addon = new LooseMapEntity()
            {
                AddonId = id,
                Type = AddonTypeEnum.Map,
                Title = Path.GetFileName(pathToFile),
                SupportedGame = new(_game.GameEnum, null, null),
                PathToFile = pathToFile,
                StartMap = new MapFileJsonModel() { File = Path.GetFileName(pathToFile) },
                BloodIni = iniExists ? bloodIni : null,
                GridImageHash = null,
                Description = null,
                Author = null,
                MainDef = null,
                AdditionalDefs = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                RequiredFeatures = null,
                PreviewImageHash = null,
                IsUnpacked = false,
                Executables = null,
                Options = null,
                IsFavorite = _config.FavoriteAddons.Contains(id)
            };

            return [addon];
        }
        else if (ArchiveFactory.IsArchive(pathToFile, out _))
        {
            var unpackedTo = UnpackIfNeededAndGetAddonDto(pathToFile, out var manifests);

            if (manifests is null or [])
            {
                return null;
            }

            bool isUnpacked = false;
            long? gridImageHash;
            long? previewImageHash;

            if (unpackedTo is not null)
            {
                pathToFile = Path.Combine(unpackedTo, "addon.json");
                isUnpacked = true;

                var addonDir = Path.GetDirectoryName(pathToFile)!;

                var gridFile = Directory.GetFiles(addonDir, "grid.*");
                var previewFile = Directory.GetFiles(addonDir, "preview.*");

                if (gridFile.Length > 0)
                {
                    gridImageHash = Crc32Helper.GetCrc32(gridFile[0]);
                    await using var stream = File.OpenRead(gridFile[0]);
                    _ = _bitmapsCache.TryAddGridToCache(gridImageHash.Value, stream);
                }
                else
                {
                    gridImageHash = null;
                }

                if (previewFile.Length > 0)
                {
                    previewImageHash = Crc32Helper.GetCrc32(previewFile[0]);
                    await using var stream = File.OpenRead(previewFile[0]);
                    _ = _bitmapsCache.TryAddPreviewToCache(previewImageHash.Value, stream);
                }
                else
                {
                    previewImageHash = null;
                }
            }
            else
            {
                isUnpacked = false;
                using var archive = ArchiveFactory.Open(pathToFile);

                (gridImageHash, Stream? gridImageStream) = ImageHelper.GetCoverFromArchive(archive);
                (previewImageHash, Stream? previewImageStream) = ImageHelper.GetPreviewFromArchive(archive);

                if (gridImageHash is not null && gridImageStream is not null)
                {
                    _ = _bitmapsCache.TryAddGridToCache(gridImageHash.Value, gridImageStream);
                    gridImageStream.Dispose();
                }

                if (previewImageHash is not null && previewImageStream is not null)
                {
                    _ = _bitmapsCache.TryAddPreviewToCache(previewImageHash.Value, previewImageStream);
                    previewImageStream.Dispose();
                }

            }

            foreach (var manifest in manifests)
            {
                AddonCarcass carcass = new();

                carcass.IsUnpacked = isUnpacked;
                carcass.GridImageHash = gridImageHash ?? previewImageHash;
                carcass.PreviewImageHash = previewImageHash;

                carcass.Type = manifest.AddonType;
                carcass.Id = manifest.Id;
                carcass.Title = manifest.Title;
                carcass.Author = manifest.Author;
                carcass.Version = manifest.Version;
                carcass.Description = manifest.Description;
                carcass.SupportedGame = manifest.SupportedGame.Game;
                carcass.GameVersion = manifest.SupportedGame.Version;
                carcass.GameCrc = manifest.SupportedGame.Crc;

                carcass.Rts = manifest.Rts;
                carcass.Ini = manifest.Ini;
                carcass.Rff = manifest.MainRff;
                carcass.Snd = manifest.SoundRff;

                carcass.StartMap = manifest.StartMap;

                carcass.RequiredFeatures = manifest.Dependencies?.RequiredFeatures?.Select(static x => x).ToImmutableArray();

                carcass.MainCon = manifest.MainCon;
                carcass.AddCons = manifest.AdditionalCons?.ToImmutableArray();

                carcass.MainDef = manifest.MainDef;
                carcass.AddDefs = manifest.AdditionalDefs?.ToImmutableArray();

                carcass.Dependencies = manifest.Dependencies?.Addons?.ToDictionary(static x => x.Id, static x => x.Version, StringComparer.OrdinalIgnoreCase);
                carcass.Incompatibles = manifest.Incompatibles?.Addons?.ToDictionary(static x => x.Id, static x => x.Version, StringComparer.OrdinalIgnoreCase);

                if (manifest.Executables is not null)
                {
                    carcass.Executables = [];

                    foreach (var osPortsPair in manifest.Executables)
                    {
                        carcass.Executables.Add(osPortsPair.Key, []);

                        foreach (var x in osPortsPair.Value)
                        {
                            carcass.Executables[osPortsPair.Key].Add(x.Key, Path.Combine(Path.GetDirectoryName(pathToFile)!, x.Value));
                        }
                    }
                }
                else
                {
                    carcass.Executables = null;
                }

                if (manifest.Options is not null)
                {
                    carcass.Options = [];

                    foreach (var option in manifest.Options)
                    {
                        carcass.Options.Add(option.OptionName, []);

                        if (option.Parameters is not null)
                        {
                            foreach (var param in option.Parameters)
                            {
                                carcass.Options[option.OptionName].Add(param.Key, param.Value);
                            }
                        }
                    }
                }
                else
                {
                    carcass.Options = null;
                }
                
                carcasses.Add(carcass);
            }
        }
        else if (pathToFile.EndsWith(".grp", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        else
        {
            return null;
        }

        List<BaseAddon> addons = [];

        foreach (var carcass in carcasses)
        {
            if (carcass.Type is AddonTypeEnum.Mod)
            {
                var isEnabled = !_config.DisabledAutoloadMods.Contains(carcass.Id);

                if (carcass.MainDef is not null)
                {
                    ThrowHelper.ThrowArgumentException("Autoload mod can't have Main DEF");
                }

                AddonId id = new(carcass.Id, carcass.Version);

                var addon = new AutoloadModEntity()
                {
                    AddonId = id,
                    Type = AddonTypeEnum.Mod,
                    Title = carcass.Title,
                    GridImageHash = carcass.GridImageHash,
                    PreviewImageHash = carcass.PreviewImageHash,
                    Description = carcass.Description,
                    Author = carcass.Author,
                    IsEnabled = isEnabled,
                    PathToFile = pathToFile,
                    MainDef = null,
                    AdditionalDefs = carcass.AddDefs,
                    AdditionalCons = carcass.AddCons,
                    SupportedGame = new(carcass.SupportedGame, carcass.GameVersion, carcass.GameCrc),
                    DependentAddons = carcass.Dependencies,
                    IncompatibleAddons = carcass.Incompatibles,
                    StartMap = carcass.StartMap,
                    RequiredFeatures = carcass.RequiredFeatures,
                    IsUnpacked = carcass.IsUnpacked,
                    Executables = null,
                    Options = null,
                    IsFavorite = _config.FavoriteAddons.Contains(id)
                };

                addons.Add(addon);
            }
            else
            {
                if (_game.GameEnum
                    is GameEnum.Duke3D
                    or GameEnum.Fury
                    or GameEnum.Redneck
                    or GameEnum.NAM
                    or GameEnum.WW2GI)
                {
                    var addon = new DukeCampaignEntity()
                    {
                        AddonId = new(carcass.Id, carcass.Version),
                        Type = carcass.Type,
                        SupportedGame = new(carcass.SupportedGame, carcass.GameVersion, carcass.GameCrc),
                        Title = carcass.Title,
                        GridImageHash = carcass.GridImageHash,
                        PreviewImageHash = carcass.PreviewImageHash,
                        Description = carcass.Description,
                        Author = carcass.Author,
                        PathToFile = pathToFile,
                        DependentAddons = carcass.Dependencies,
                        IncompatibleAddons = carcass.Incompatibles,
                        StartMap = carcass.StartMap,
                        MainCon = carcass.MainCon,
                        AdditionalCons = carcass.AddCons,
                        MainDef = carcass.MainDef,
                        AdditionalDefs = carcass.AddDefs,
                        RTS = carcass.Rts,
                        RequiredFeatures = carcass.RequiredFeatures,
                        IsUnpacked = carcass.IsUnpacked,
                        Executables = carcass.Executables,
                        Options = carcass.Options,
                        IsFavorite = _config.FavoriteAddons.Contains(new(carcass.Id, carcass.Version))
                    };

                    addons.Add(addon);
                }
                else if (_game.GameEnum is GameEnum.Wang)
                {
                    var addon = new GenericCampaignEntity()
                    {
                        AddonId = new(carcass.Id, carcass.Version),
                        Type = carcass.Type,
                        SupportedGame = new(carcass.SupportedGame, carcass.GameVersion, carcass.GameCrc),
                        Title = carcass.Title,
                        GridImageHash = carcass.GridImageHash,
                        PreviewImageHash = carcass.PreviewImageHash,
                        Description = carcass.Description,
                        Author = carcass.Author,
                        PathToFile = pathToFile,
                        DependentAddons = carcass.Dependencies,
                        IncompatibleAddons = carcass.Incompatibles,
                        StartMap = carcass.StartMap,
                        MainDef = carcass.MainDef,
                        AdditionalDefs = carcass.AddDefs,
                        RequiredFeatures = carcass.RequiredFeatures,
                        IsUnpacked = carcass.IsUnpacked,
                        Executables = carcass.Executables,
                        Options = carcass.Options,
                        IsFavorite = _config.FavoriteAddons.Contains(new(carcass.Id, carcass.Version))
                    };

                    addons.Add(addon);
                }
                else if (_game.GameEnum is GameEnum.Blood)
                {
                    var addon = new BloodCampaignEntity()
                    {
                        AddonId = new(carcass.Id, carcass.Version),
                        Type = carcass.Type,
                        SupportedGame = new(carcass.SupportedGame, carcass.GameVersion, carcass.GameCrc),
                        Title = carcass.Title,
                        GridImageHash = carcass.GridImageHash,
                        PreviewImageHash = carcass.PreviewImageHash,
                        Description = carcass.Description,
                        Author = carcass.Author,
                        PathToFile = pathToFile,
                        DependentAddons = carcass.Dependencies,
                        IncompatibleAddons = carcass.Incompatibles,
                        StartMap = carcass.StartMap,
                        MainDef = carcass.MainDef,
                        AdditionalDefs = carcass.AddDefs,
                        INI = carcass.Ini,
                        RFF = carcass.Rff,
                        SND = carcass.Snd,
                        RequiredFeatures = carcass.RequiredFeatures,
                        IsUnpacked = carcass.IsUnpacked,
                        Executables = carcass.Executables,
                        Options = carcass.Options,
                        IsFavorite = _config.FavoriteAddons.Contains(new(carcass.Id, carcass.Version))
                    };

                    addons.Add(addon);
                }
                else if (_game.GameEnum is GameEnum.Slave)
                {
                    var addon = new GenericCampaignEntity()
                    {
                        AddonId = new(carcass.Id, carcass.Version),
                        Type = carcass.Type,
                        SupportedGame = new(carcass.SupportedGame, carcass.GameVersion, carcass.GameCrc),
                        Title = carcass.Title,
                        GridImageHash = carcass.GridImageHash,
                        PreviewImageHash = carcass.PreviewImageHash,
                        Description = carcass.Description,
                        Author = carcass.Author,
                        PathToFile = pathToFile,
                        DependentAddons = carcass.Dependencies,
                        IncompatibleAddons = carcass.Incompatibles,
                        StartMap = carcass.StartMap,
                        MainDef = carcass.MainDef,
                        AdditionalDefs = carcass.AddDefs,
                        RequiredFeatures = carcass.RequiredFeatures,
                        IsUnpacked = carcass.IsUnpacked,
                        Executables = carcass.Executables,
                        Options = carcass.Options,
                        IsFavorite = _config.FavoriteAddons.Contains(new(carcass.Id, carcass.Version))
                    };

                    addons.Add(addon);
                }
                else if (_game.GameEnum is GameEnum.Standalone)
                {
                    var addon = new StandaloneEntity()
                    {
                        AddonId = new(carcass.Id, carcass.Version),
                        Type = carcass.Type,
                        SupportedGame = new(carcass.SupportedGame, carcass.GameVersion, carcass.GameCrc),
                        Title = carcass.Title,
                        GridImageHash = carcass.GridImageHash,
                        PreviewImageHash = carcass.PreviewImageHash,
                        Description = carcass.Description,
                        Author = carcass.Author,
                        PathToFile = pathToFile,
                        DependentAddons = carcass.Dependencies,
                        IncompatibleAddons = carcass.Incompatibles,
                        StartMap = carcass.StartMap,
                        MainDef = carcass.MainDef,
                        AdditionalDefs = carcass.AddDefs,
                        RequiredFeatures = carcass.RequiredFeatures,
                        IsUnpacked = carcass.IsUnpacked,
                        Executables = carcass.Executables,
                        Options = carcass.Options,
                        IsFavorite = _config.FavoriteAddons.Contains(new(carcass.Id, carcass.Version))
                    };

                    addons.Add(addon);
                }
                else
                {
                    ThrowHelper.ThrowNotSupportedException();
                }
            }
        }

        return addons;
    }

    /// <summary>
    /// Unpack archive if needed and return path to folder
    /// </summary>
    /// <param name="pathToFile">Path to archive</param>
    /// <param name="addonDtos">AddonDto</param>
    private string? UnpackIfNeededAndGetAddonDto(string pathToFile, out List<AddonJsonModel>? addonDtos)
    {
        try
        {
            using var archive = ArchiveFactory.Open(pathToFile);

            string? unpackedTo = null;

            if (archive.Entries.Any(static x => x.Key!.Equals("addons.grpinfo", StringComparison.OrdinalIgnoreCase)))
            {
                //need to unpack archive with grpinfo
                unpackedTo = Unpack(pathToFile, archive);
                addonDtos = null;
                archive?.Dispose();
                File.Delete(pathToFile);

                return unpackedTo;
            }

            var addonJsonsInsideArchive = archive.Entries.Where(static x => x.Key!.StartsWith("addon") && x.Key!.EndsWith(".json"));

            if (addonJsonsInsideArchive?.Any() is not true)
            {
                addonDtos = null;
                return null;
            }

            using var addonJsonStream = addonJsonsInsideArchive.First().OpenEntryStream();

            var addonDto = JsonSerializer.Deserialize(
                addonJsonStream,
                AddonManifestContext.Default.AddonJsonModel
                )!;

            if (addonDto.MainRff is not null || addonDto.SoundRff is not null)
            {
                //need to unpack addons that contain custom RFF files
                unpackedTo = Unpack(pathToFile, archive);
            }
            else if (addonDto.Executables is not null)
            {
                //need to unpack addons with custom executables
                unpackedTo = Unpack(pathToFile, archive);
            }

            List<AddonJsonModel> result = [];

            if (unpackedTo is not null)
            {
                archive?.Dispose();
                File.Delete(pathToFile);

                var unpackedAddonJsons = Directory.GetFiles(unpackedTo, "addon*.json");

                foreach (var addonJson in unpackedAddonJsons)
                {
                    var text = File.ReadAllText(addonJson);

                    var addonDto2 = JsonSerializer.Deserialize(
                        text,
                        AddonManifestContext.Default.AddonJsonModel
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
                        AddonManifestContext.Default.AddonJsonModel
                        )!;

                    result.Add(addonDto2);
                }
            }

            addonDtos = result.Count > 0 ? result : null;
            return unpackedTo;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while unpacking archive ===");

            addonDtos = null;
            return null;
        }
    }

    /// <summary>
    /// Unpack archive and return path to folder
    /// </summary>
    /// <param name="pathToFile">Path to archive</param>
    /// <param name="archive">Archive</param>
    private string Unpack(string pathToFile, IArchive archive)
    {
        var fileFolder = Path.GetDirectoryName(pathToFile)!;
        var unpackTo = Path.Combine(fileFolder, Path.GetFileNameWithoutExtension(pathToFile));

        if (Directory.Exists(unpackTo))
        {
            Directory.Delete(unpackTo, true);
        }

        archive.ExtractToDirectory(unpackTo);

        return unpackTo;
    }
}

internal struct AddonCarcass
{
    public GameEnum SupportedGame { get; set; }
    public string Id { get; set; }
    public string Title { get; set; }
    public AddonTypeEnum Type { get; set; }
    public string Version { get; set; }
    public bool IsUnpacked { get; set; }
    public string? Author { get; set; }
    public string? Description { get; set; }
    public long? GridImageHash { get; set; }
    public long? PreviewImageHash { get; set; }
    public string? GameVersion { get; set; }
    public string? GameCrc { get; set; }
    public ImmutableArray<FeatureEnum>? RequiredFeatures { get; set; }
    public string? MainCon { get; set; }
    public ImmutableArray<string>? AddCons { get; set; }
    public string? MainDef { get; set; }
    public ImmutableArray<string>? AddDefs { get; set; }
    public string? Rts { get; set; }
    public string? Ini { get; set; }
    public string? Rff { get; set; }
    public string? Snd { get; set; }
    public Dictionary<string, string?>? Dependencies { get; set; }
    public Dictionary<string, string?>? Incompatibles { get; set; }
    public IStartMap? StartMap { get; set; }
    public Dictionary<OSEnum, Dictionary<PortEnum, string>>? Executables { get; set; }
    public Dictionary<string, Dictionary<string, OptionalParameterTypeEnum>>? Options { get; set; }
}
