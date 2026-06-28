using System.Collections.Immutable;
using System.Text.Json;
using System.Threading.Channels;
using Addons.Addons;
using Core.All;
using Core.All.Enums;
using Core.All.Enums.Addons;
using Core.All.Helpers;
using Core.All.Interfaces;
using Core.All.Serializable.Addon;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Games.Games;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using StandaloneGame = Addons.Addons.StandaloneGame;

namespace Addons.Providers;

/// <summary>
/// Provides cached lists of installed campaigns, maps, and mods for a specific game.
/// </summary>
public sealed class InstalledAddonsProvider : IDisposable
{
    private readonly BaseGame _game;
    private readonly IConfigProvider _config;
    private readonly ILogger<InstalledAddonsProvider> _logger;
    private readonly OriginalCampaignsProvider _originalCampaignsProvider;
    private readonly MetadataProvider _metadataProvider;
    private readonly LocalFilesProvider _localFilesProvider;

    private readonly List<BaseAddon> _campaignsCache = [];
    private readonly List<BaseAddon> _mapsCache = [];
    private readonly List<BaseAddon> _modsCache = [];

    private readonly IChannelSubscriber<DiHelper.LocalFileEvent> _channelPublisher;
    private readonly ChannelReader<DiHelper.LocalFileEvent> _channelReader;
    private readonly CancellationTokenSource _channelCancellation = new();
    private readonly SemaphoreSlim _cacheUpdateSemaphore = new(1);

    public event AddonChanged? AddonsChangedEvent;

    [Obsolete($"Don't create directly. Use {nameof(InstalledAddonsProviderFactory)}.")]
    public InstalledAddonsProvider(
        BaseGame game,
        IConfigProvider config,
        OriginalCampaignsProvider originalCampaignsProvider,
        MetadataProvider metadataProvider,
        LocalFilesProvider localFilesProvider,
        IChannelSubscriber<DiHelper.LocalFileEvent> channelPublisher,
        ILogger<InstalledAddonsProvider> logger
        )
    {
        _game = game;
        _config = config;
        _logger = logger;
        _originalCampaignsProvider = originalCampaignsProvider;
        _metadataProvider = metadataProvider;
        _channelPublisher = channelPublisher;
        _localFilesProvider = localFilesProvider;

        _metadataProvider.MetadataUpdatedEvent += OnMetadataUpdated;
        _metadataProvider.MetadataInitializedEvent += OnMetadataInitialized;

        _channelReader = channelPublisher.Subscribe();

        Task.Run(async () =>
        {
            try
            {
                await foreach (var e in _channelReader.ReadAllAsync(_channelCancellation.Token))
                {
                    foreach (var parsedFile in e.Files)
                    {
                        await _cacheUpdateSemaphore.WaitAsync(_channelCancellation.Token);

                        try
                        {
                            if (parsedFile.SupportedGame != _game.GameEnum)
                            {
                                continue;
                            }

                            if (e.IsAdded)
                            {
                                var isUnpacked = await UnpackAndUpdateIfNeededAsync(parsedFile);

                                if (isUnpacked)
                                {
                                    break;
                                }

                                AddAddon(parsedFile);
                            }
                            else
                            {
                                DeleteAddon(parsedFile);
                            }
                        }
                        finally
                        {
                            _cacheUpdateSemaphore.Release();
                        }
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                //nothing to do
            }
        });
    }

    /// <summary>
    /// Build or refresh the internal caches for all addon types.
    /// </summary>
    /// <param name="createNew">If true, clear the cache for <paramref name="addonType"/> before rebuilding.</param>
    /// <param name="addonType">Addon type whose cache to optionally clear.</param>
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

            var files = await _localFilesProvider.GetCachedAddonFilesAsync().ConfigureAwait(false);

            switch (addonType)
            {
                case AddonTypeEnum.TC when cache.Count == 0:
                    _campaignsCache.Clear();
                    await FillCacheAsync(files, AddonTypeEnum.TC);
                    break;
                case AddonTypeEnum.Map when cache.Count == 0:
                    _mapsCache.Clear();
                    await FillCacheAsync(files, AddonTypeEnum.Map);
                    break;
                case AddonTypeEnum.Mod when cache.Count == 0:
                    _modsCache.Clear();
                    await FillCacheAsync(files, AddonTypeEnum.Mod);
                    break;
                case AddonTypeEnum.Official:
                default:
                    throw new ArgumentOutOfRangeException(nameof(addonType), addonType, null);
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
                        EnableAddon(mod.AddonId);
                    }
                    else if (!autoloadMod.IsEnabled)
                    {
                        DisableAddon(mod.AddonId);
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
            ArgumentNullException.ThrowIfNull(_campaignsCache);
            ArgumentNullException.ThrowIfNull(_mapsCache);
            ArgumentNullException.ThrowIfNull(_modsCache);

            AddonsChangedEvent?.Invoke(_game.GameEnum, addonType);
        }
    }

    private List<BaseAddon> GetCacheByAddonType(AddonTypeEnum addonType)
    {
        var cache = addonType switch
        {
            AddonTypeEnum.TC => _campaignsCache,
            AddonTypeEnum.Map => _mapsCache,
            AddonTypeEnum.Mod => _modsCache,
            _ => throw new NotSupportedException(),
        };

        return cache;
    }

    /// <summary>
    /// Scan parsed addon files and populate the cache for <paramref name="addonType"/>.
    /// </summary>
    private async Task FillCacheAsync(IReadOnlyList<ParsedAddonFile> parsedAddonFiles, AddonTypeEnum addonType)
    {
        foreach (var parsedAddonFile in parsedAddonFiles)
        {
            if (parsedAddonFile.Manifest is not null && parsedAddonFile.Manifest.SupportedGame.Game != _game.GameEnum)
            {
                continue;
            }

            var isUnpacked = await UnpackAndUpdateIfNeededAsync(parsedAddonFile);

            if (isUnpacked)
            {
                return;
            }

            var cache = GetCacheByAddonType(addonType);

            if (parsedAddonFile.FileInfo.IsGrpInfo)
            {
                if (addonType != AddonTypeEnum.TC)
                {
                    continue;
                }

                if (!GrpInfoProvider.TryGetAddonsFromGrpInfo(parsedAddonFile.FileInfo.PathToFile,  out var addons))
                {
                    continue;
                }

                cache.AddRange(addons);
            }
            else if (parsedAddonFile.FileInfo.IsMap)
            {
                if (addonType != AddonTypeEnum.Map)
                {
                    continue;
                }

                var addon = GetLooseMapFromFile(parsedAddonFile);

                if (addon is null)
                {
                    continue;
                }

                cache.Add(addon);
            }
            else
            {
                var addon = GetAddonFromFile(parsedAddonFile);

                if (addon is null || addon.Type != addonType)
                {
                    continue;
                }

                cache.Add(addon);
            }
        }
    }

    private BaseAddon? GetLooseMapFromFile(ParsedAddonFile parsedAddonFile)
    {
        if (!parsedAddonFile.FileInfo.IsMap)
        {
            return null;
        }

        var bloodIniName = parsedAddonFile.FileInfo.FileName.Replace(".map", ".ini", StringComparison.InvariantCultureIgnoreCase);
        var actualIni = Path.GetFileName(Directory.EnumerateFiles(parsedAddonFile.FileInfo.PathToFolder).FirstOrDefault(f => Path.GetFileName(f).Equals(bloodIniName, StringComparison.OrdinalIgnoreCase)));

        AddonId id = new(parsedAddonFile.FileInfo.FileName);

        return new LooseMap
        {
            AddonId = id,
            Type = AddonTypeEnum.Map,
            FileInfo = parsedAddonFile.FileInfo,
            Title = parsedAddonFile.FileInfo.FileName,
            SupportedGame = new(_game.GameEnum, null, null),
            StartMap = new MapFileJsonModel { File = parsedAddonFile.FileInfo.FileName },
            BloodIni = actualIni,
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
            IsFavorite = _config.FavoriteAddons.Contains(id),
            IsMetadataUpdateAvailable = _metadataProvider.IsMetadataUpdateAvailable(id, parsedAddonFile.FileInfo),
        };
    }

    /// <summary>
    /// Add a single addon from its parsed file into the appropriate cache.
    /// </summary>
    /// <param name="parsedAddonFile">Parsed addon file to add.</param>
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

            return;
        }

        if (parsedAddonFile.FileInfo.IsMap)
        {
            addon = GetLooseMapFromFile(parsedAddonFile);
        }
        else
        {
            addon = GetAddonFromFile(parsedAddonFile);
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
            AddonTypeEnum.Official => throw new NotSupportedException(),
            _ => throw new NotSupportedException(),
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

    /// <summary>
    /// Delete addon from disk and remove it from the cache.
    /// </summary>
    /// <param name="parsedAddonFile">Addon to delete.</param>
    public void DeleteAddon(ParsedAddonFile parsedAddonFile)
    {
        if (parsedAddonFile.Manifest is null)
        {
            throw new InvalidOperationException();
        }

        var cache = GetCacheByAddonType(parsedAddonFile.Manifest.AddonType);
        var addonToDelete = cache.FirstOrDefault(x => x.FileInfo is not null && x.FileInfo.Equals(parsedAddonFile.FileInfo));

        if (addonToDelete is not null)
        {
            DeleteAddon(addonToDelete);
        }
    }

    /// <summary>
    /// Delete addon from disk and remove it from the cache.
    /// </summary>
    /// <param name="addon">Addon to delete.</param>
    public void DeleteAddon(BaseAddon addon)
    {
        ArgumentNullException.ThrowIfNull(_campaignsCache);
        ArgumentNullException.ThrowIfNull(_mapsCache);
        ArgumentNullException.ThrowIfNull(_modsCache);
        ArgumentNullException.ThrowIfNull(addon.FileInfo);

        if (addon is LooseMap map)
        {
            File.Delete(map.FileInfo.PathToFile);

            var bloodIni = Path.Combine(addon.FileInfo.PathToFolder, map.BloodIni ?? string.Empty);
            if (map.BloodIni is not null &&
                File.Exists(bloodIni))
            {
                File.Delete(bloodIni);
            }
        }
        else if (addon.FileInfo.IsFolder)
        {
            Directory.Delete(addon.FileInfo.PathToFolder, true);
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

    /// <summary>
    /// Enable an autoload mod by id, cascading to dependencies and disabling incompatible mods.
    /// </summary>
    /// <param name="addon">Addon id to enable.</param>
    public void EnableAddon(AddonId addon)
    {
        var existing = _modsCache.FirstOrDefault(x => x.AddonId.Equals(addon));

        if (existing is not AutoloadMod autoloadMod)
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
                 x.AddonId.Id.Equals(addon.Id, StringComparison.OrdinalIgnoreCase) &&
                 !VersionComparer.Compare(x.AddonId.Version, addon.Version, ComparisonOperatorEnum.Equals) &&
                 (x.FileInfo is null || !x.FileInfo.Equals(autoloadMod.FileInfo))
                 );

        foreach (var version in otherVersions)
        {
            DisableAddon(version.AddonId);
        }

        _config.ChangeModState(addon, true);
    }

    /// <summary>
    /// Disable an autoload mod by id, cascading to dependant mods.
    /// </summary>
    /// <param name="addon">Addon id to disable.</param>
    public void DisableAddon(AddonId addon)
    {
        var existing = _modsCache.FirstOrDefault(x => x.AddonId.Equals(addon));

        if (existing is not AutoloadMod autoloadMod)
        {
            return;
        }

        if (!autoloadMod.IsEnabled)
        {
            return;
        }

        autoloadMod.IsEnabled = false;

        var deps = _modsCache.Where(x => x.DependentAddons?.ContainsKey(autoloadMod.AddonId.Id) ?? false);

        foreach (var dep in deps)
        {
            DisableAddon(dep.AddonId);
        }

        _config.ChangeModState(addon, false);
    }

    /// <summary>
    /// Get list of installed addons of a given type.
    /// </summary>
    /// <param name="addonType">Addon type</param>
    public IReadOnlyList<BaseAddon> GetInstalledAddonsByType(AddonTypeEnum addonType)
    {
        return addonType switch
        {
            AddonTypeEnum.TC => GetInstalledCampaigns(),
            AddonTypeEnum.Map => GetInstalledMaps(),
            AddonTypeEnum.Mod => GetInstalledMods(),
            _ => throw new NotSupportedException()
        };
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
                //hack to make SW addons appear at the top of the list
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

    private async Task<bool> UnpackAndUpdateIfNeededAsync(ParsedAddonFile parsedAddonFile)
    {
        if (!parsedAddonFile.FileInfo.IsZip)
        {
            return false;
        }

        var unpackedTo = UnpackIfNeeded(parsedAddonFile.FileInfo);

        if (unpackedTo is not null)
        {
            await _localFilesProvider.ReplacePathAsync(parsedAddonFile.FileInfo.PathToFile, unpackedTo);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Unpack an archive if it contains addon manifests or GRP info files.
    /// </summary>
    /// <param name="pathToFile">Path to archive.</param>
    /// <returns>Path to unpacked folder, or <see langword="null"/> if the archive was not unpacked.</returns>
    private string? UnpackIfNeeded(AddonFilePathWrapper pathToFile)
    {
        try
        {
            using var archive = ArchiveFactory.OpenArchive(pathToFile.PathToFile);

            string? unpackedTo = null;

            if (archive.Entries.Any(static x => x.Key!.Equals("addons.grpinfo", StringComparison.OrdinalIgnoreCase)))
            {
                //need to unpack archive with grpinfo
                unpackedTo = Unpack(pathToFile.PathToFile, archive);
                archive.Dispose();
                File.Delete(pathToFile.PathToFile);

                return unpackedTo;
            }

            var addonJsonsInsideArchive = archive.Entries
                .Where(static x => x.Key!.StartsWith("addon") && x.Key!.EndsWith(".json"))
                .ToList();

            if (addonJsonsInsideArchive.Count == 0)
            {
                return null;
            }

            using var addonJsonStream = addonJsonsInsideArchive[0].OpenEntryStream();

            var addonDto = JsonSerializer.Deserialize(
                addonJsonStream,
                AddonManifestJsonContext.Default.AddonManifestJsonModel
                );

            if (addonDto is null)
            {
                return null;
            }

            if (addonDto.MainRff is not null || addonDto.SoundRff is not null)
            {
                //need to unpack addons that contain custom RFF files
                unpackedTo = Unpack(pathToFile.PathToFile, archive);
            }
            else if (addonDto.Executables is not null)
            {
                //need to unpack addons with custom executables
                unpackedTo = Unpack(pathToFile.PathToFile, archive);
            }

            List<AddonManifestJsonModel> result = [];

            if (unpackedTo is not null)
            {
                archive.Dispose();
                File.Delete(pathToFile.PathToFile);

                var unpackedAddonJsons = Directory.GetFiles(unpackedTo, "addon*.json");

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

            return unpackedTo;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while unpacking archive ===");
            return null;
        }
    }

    /// <summary>
    /// Extract archive contents to a subfolder named after the archive.
    /// </summary>
    /// <param name="pathToFile">Path to archive.</param>
    /// <param name="archive">Archive to extract.</param>
    /// <returns>Path to the unpacked folder.</returns>
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

    /// <summary>
    /// Convert a parsed addon file into a domain addon object
    /// </summary>
    /// <param name="parsedAddonFile">Parsed addon file to convert.</param>
    internal BaseAddon? GetAddonFromFile(ParsedAddonFile parsedAddonFile)
    {
        if (parsedAddonFile.Manifest is null)
        {
            throw new InvalidOperationException($"{nameof(GetAddonFromFile)} requires a non-null manifest. File: {parsedAddonFile.FileInfo.PathToFile}");
        }

        AddonCarcass? carcass;
        BaseAddon? addon;

        if (parsedAddonFile.FileInfo.IsJson || parsedAddonFile.FileInfo.IsZip || parsedAddonFile.FileInfo.IsFolder)
        {
            carcass = GetCarcass(parsedAddonFile.Manifest, parsedAddonFile.FileInfo, parsedAddonFile.GridHash, parsedAddonFile.PreviewHash);
        }
        else if (parsedAddonFile.FileInfo.IsGrpInfo || parsedAddonFile.FileInfo.IsMap)
        {
            throw new NotSupportedException();
        }
        else
        {
            return null;
        }

        var carcassValue = carcass.Value;
        if (carcassValue.Type is AddonTypeEnum.Mod)
        {
            var isEnabled = !_config.DisabledAutoloadMods.Contains(carcassValue.Id);

            if (carcassValue.MainDef is not null)
            {
                throw new ArgumentException("Autoload mod can't have Main DEF");
            }

            AddonId id = new(carcassValue.Id, carcassValue.Version);
            addon = new AutoloadMod
            {
                AddonId = id,
                Type = AddonTypeEnum.Mod,
                Title = carcassValue.Title,
                GridImageHash = carcassValue.GridImageHash,
                PreviewImageHash = carcassValue.PreviewImageHash,
                Description = carcassValue.Description,
                Author = carcassValue.Author,
                ReleaseDate = carcassValue.ReleaseDate,
                IsEnabled = isEnabled,
                FileInfo = parsedAddonFile.FileInfo,
                MainDef = null,
                AdditionalDefs = carcassValue.AddDefs,
                AdditionalCons = carcassValue.AddCons,
                SupportedGame = new(carcassValue.SupportedGame, carcassValue.GameVersion, carcassValue.GameCrc),
                DependentAddons = carcassValue.Dependencies,
                IncompatibleAddons = carcassValue.Incompatibles,
                StartMap = carcassValue.StartMap,
                RequiredFeatures = carcassValue.RequiredFeatures,
                Executables = null,
                Options = null,
                IsFavorite = _config.FavoriteAddons.Contains(id),
                IsMetadataUpdateAvailable = _metadataProvider.IsMetadataUpdateAvailable(id, parsedAddonFile.FileInfo),
            };
        }
        else
        {
            addon = CreateCampaignAddon(carcassValue, parsedAddonFile.FileInfo);
        }

        return addon;
    }

    private BaseAddon CreateCampaignAddon(AddonCarcass carcass, AddonFilePathWrapper fileInfo)
    {
        AddonId id = new(carcass.Id, carcass.Version);
        var game = new GameInfo(carcass.SupportedGame, carcass.GameVersion, carcass.GameCrc);
        var isFavorite = _config.FavoriteAddons.Contains(id);
        var isUpdate = _metadataProvider.IsMetadataUpdateAvailable(id, fileInfo);

        return _game.GameEnum switch
        {
            GameEnum.Duke3D
                or GameEnum.Fury
                or GameEnum.Redneck
                or GameEnum.NAM
                or GameEnum.WW2GI =>
                new DukeCampaign
                {
                    AddonId = id,
                    Type = carcass.Type,
                    Title = carcass.Title,
                    GridImageHash = carcass.GridImageHash,
                    PreviewImageHash = carcass.PreviewImageHash,
                    Description = carcass.Description,
                    Author = carcass.Author,
                    ReleaseDate = carcass.ReleaseDate,
                    FileInfo = fileInfo,
                    DependentAddons = carcass.Dependencies,
                    IncompatibleAddons = carcass.Incompatibles,
                    StartMap = carcass.StartMap,
                    MainCon = carcass.MainCon,
                    AdditionalCons = carcass.AddCons,
                    MainDef = carcass.MainDef,
                    AdditionalDefs = carcass.AddDefs,
                    RTS = carcass.Rts,
                    RequiredFeatures = carcass.RequiredFeatures,
                    Executables = carcass.Executables,
                    Options = carcass.Options,
                    SupportedGame = game,
                    IsFavorite = isFavorite,
                    IsMetadataUpdateAvailable = isUpdate,
                },
            GameEnum.Wang or GameEnum.Slave =>
                new GenericCampaign
                {
                    AddonId = id,
                    Type = carcass.Type,
                    Title = carcass.Title,
                    GridImageHash = carcass.GridImageHash,
                    PreviewImageHash = carcass.PreviewImageHash,
                    Description = carcass.Description,
                    Author = carcass.Author,
                    ReleaseDate = carcass.ReleaseDate,
                    FileInfo = fileInfo,
                    DependentAddons = carcass.Dependencies,
                    IncompatibleAddons = carcass.Incompatibles,
                    StartMap = carcass.StartMap,
                    MainDef = carcass.MainDef,
                    AdditionalDefs = carcass.AddDefs,
                    RequiredFeatures = carcass.RequiredFeatures,
                    Executables = carcass.Executables,
                    Options = carcass.Options,
                    SupportedGame = game,
                    IsFavorite = isFavorite,
                    IsMetadataUpdateAvailable = isUpdate,
                },
            GameEnum.Blood =>
                new BloodCampaign
                {
                    AddonId = id,
                    Type = carcass.Type,
                    Title = carcass.Title,
                    GridImageHash = carcass.GridImageHash,
                    PreviewImageHash = carcass.PreviewImageHash,
                    Description = carcass.Description,
                    Author = carcass.Author,
                    ReleaseDate = carcass.ReleaseDate,
                    FileInfo = fileInfo,
                    DependentAddons = carcass.Dependencies,
                    IncompatibleAddons = carcass.Incompatibles,
                    StartMap = carcass.StartMap,
                    MainDef = carcass.MainDef,
                    AdditionalDefs = carcass.AddDefs,
                    INI = carcass.Ini,
                    RFF = carcass.Rff,
                    SND = carcass.Snd,
                    RequiredFeatures = carcass.RequiredFeatures,
                    Executables = carcass.Executables,
                    Options = carcass.Options,
                    SupportedGame = game,
                    IsFavorite = isFavorite,
                    IsMetadataUpdateAvailable = isUpdate,
                },
            GameEnum.Standalone =>
                new StandaloneGame
                {
                    AddonId = id,
                    Type = carcass.Type,
                    Title = carcass.Title,
                    GridImageHash = carcass.GridImageHash,
                    PreviewImageHash = carcass.PreviewImageHash,
                    Description = carcass.Description,
                    Author = carcass.Author,
                    ReleaseDate = carcass.ReleaseDate,
                    FileInfo = fileInfo,
                    DependentAddons = carcass.Dependencies,
                    IncompatibleAddons = carcass.Incompatibles,
                    StartMap = carcass.StartMap,
                    MainDef = carcass.MainDef,
                    AdditionalDefs = carcass.AddDefs,
                    RequiredFeatures = carcass.RequiredFeatures,
                    Executables = carcass.Executables,
                    Options = carcass.Options,
                    SupportedGame = game,
                    IsFavorite = isFavorite,
                    IsMetadataUpdateAvailable = isUpdate,
                },
            _ => throw new NotSupportedException(),
        };
    }

    /// <summary>
    /// Build an <see cref="AddonCarcass"/> from a manifest and file metadata.
    /// </summary>
    private static AddonCarcass GetCarcass(
        AddonManifestJsonModel manifest,
        AddonFilePathWrapper fileInfo,
        long? gridImageHash,
        long? previewImageHash)
    {
        AddonCarcass carcass = new()
        {
            GridImageHash = gridImageHash ?? previewImageHash,
            PreviewImageHash = previewImageHash,
            Type = manifest.AddonType,
            Id = manifest.Id,
            Title = manifest.Title,
            Author = manifest.Author,
            ReleaseDate = manifest.ReleaseDate,
            Version = manifest.Version,
            Description = manifest.Description,
            SupportedGame = manifest.SupportedGame.Game,
            GameVersion = manifest.SupportedGame.Version,
            GameCrc = manifest.SupportedGame.Crc,
            Rts = manifest.Rts,
            Ini = manifest.Ini,
            Rff = manifest.MainRff,
            Snd = manifest.SoundRff,
            StartMap = manifest.StartMap,
            RequiredFeatures = manifest.Dependencies?.RequiredFeatures?.ToImmutableArray(),
            MainCon = manifest.MainCon,
            AddCons = manifest.AdditionalCons?.ToImmutableArray(),
            MainDef = manifest.MainDef,
            AddDefs = manifest.AdditionalDefs?.ToImmutableArray(),
            Dependencies = manifest.Dependencies?.Addons?.ToDictionary(static x => x.Id, static x => x.Version, StringComparer.OrdinalIgnoreCase),
            Incompatibles = manifest.Incompatibles?.Addons?.ToDictionary(static x => x.Id, static x => x.Version, StringComparer.OrdinalIgnoreCase)
        };

        if (manifest.Executables is not null)
        {
            carcass.Executables = [];

            foreach (var osPortsPair in manifest.Executables)
            {
                carcass.Executables.Add(osPortsPair.Key, []);

                foreach (var x in osPortsPair.Value)
                {
                    carcass.Executables[osPortsPair.Key].Add(x.Key, Path.Combine(fileInfo.PathToFolder, x.Value));
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

        return carcass;
    }


    private void OnMetadataInitialized(object? sender, EventArgs e)
    {
        IEnumerable<BaseAddon> allAddons = [.. _campaignsCache, .. _mapsCache, .. _modsCache];

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


    /// <summary>
    /// Unsubscribe from events and release resources.
    /// </summary>
    public void Dispose()
    {
        _metadataProvider.MetadataUpdatedEvent -= OnMetadataUpdated;
        _metadataProvider.MetadataInitializedEvent -= OnMetadataInitialized;

        _channelCancellation.Cancel();
        _channelCancellation.Dispose();
        _cacheUpdateSemaphore.Dispose();
        _channelPublisher.Unsubscribe(_channelReader);
    }
}

/// <summary>
/// Intermediate representation built from an addon manifest before converting to a domain <see cref="BaseAddon"/>.
/// </summary>
internal struct AddonCarcass
{
    public GameEnum SupportedGame { get; init; }
    public string Id { get; init; }
    public string Title { get; init; }
    public AddonTypeEnum Type { get; init; }
    public string Version { get; init; }
    public string? Author { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public string? Description { get; init; }
    public long? GridImageHash { get; init; }
    public long? PreviewImageHash { get; init; }
    public string? GameVersion { get; init; }
    public string? GameCrc { get; init; }
    public ImmutableArray<FeatureEnum>? RequiredFeatures { get; init; }
    public string? MainCon { get; init; }
    public ImmutableArray<string>? AddCons { get; init; }
    public string? MainDef { get; init; }
    public ImmutableArray<string>? AddDefs { get; init; }
    public string? Rts { get; init; }
    public string? Ini { get; init; }
    public string? Rff { get; init; }
    public string? Snd { get; init; }
    public Dictionary<string, string?>? Dependencies { get; init; }
    public Dictionary<string, string?>? Incompatibles { get; init; }
    public IStartMap? StartMap { get; init; }

    public Dictionary<OSEnum, Dictionary<PortEnum, string>>? Executables { get; set; }
    public Dictionary<string, Dictionary<string, OptionalParameterTypeEnum>>? Options { get; set; }
}
