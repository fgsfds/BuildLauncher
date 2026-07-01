using System.Threading.Channels;
using Addons.Addons;
using Addons.Helpers;
using Core.All;
using Core.All.Enums;
using Core.All.Enums.Addons;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Games.Games;
using Microsoft.Extensions.Logging;
using DiHelper = Core.Client.Helpers.DiHelper;

namespace Addons.Providers;

/// <summary>
///     Provides cached lists of installed campaigns, maps, and mods for a specific game.
/// </summary>
public sealed class InstalledAddonsProvider : IDisposable
{
    /// <summary>
    ///     Represents the method that handles addon change events.
    /// </summary>
    /// <param name="gameEnum">The game that was changed.</param>
    /// <param name="addonType">Optional addon type that was changed.</param>
    public delegate void AddonChanged(GameEnum gameEnum, AddonTypeEnum? addonType);


    /// <summary>
    ///     Handles addon activation and deactivation logic.
    /// </summary>
    private readonly AddonActivator _addonActivator;

    /// <summary>
    ///     Factory for creating addon domain objects from parsed files.
    /// </summary>
    private readonly AddonFactory _addonFactory;

    /// <summary>
    ///     Extracts archived addons when needed.
    /// </summary>
    private readonly ArchivedAddonExtractor _archivedAddonExtractor;

    /// <summary>
    ///     Semaphore to synchronize cache update operations.
    /// </summary>
    private readonly SemaphoreSlim _cacheUpdateSemaphore = new(1);

    /// <summary>
    ///     Cache of installed campaigns.
    /// </summary>
    private readonly List<BaseAddon> _campaignsCache = [];

    /// <summary>
    ///     Cancellation source for the background channel reader loop.
    /// </summary>
    private readonly CancellationTokenSource _channelCancellation = new();

    private readonly IChannelSubscriber<DiHelper.LocalFileEvent> _channelPublisher;

    /// <summary>
    ///     Reader for the local file event channel.
    /// </summary>
    private readonly ChannelReader<DiHelper.LocalFileEvent> _channelReader;

    private readonly BaseGame _game;

    private readonly LocalFilesProvider _localFilesProvider;

    private readonly ILogger<InstalledAddonsProvider> _logger;

    /// <summary>
    ///     Cache of installed maps.
    /// </summary>
    private readonly List<BaseAddon> _mapsCache = [];

    private readonly MetadataProvider _metadataProvider;

    /// <summary>
    ///     Cache of installed mods.
    /// </summary>
    private readonly List<BaseAddon> _modsCache = [];

    private readonly OriginalCampaignsProvider _originalCampaignsProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstalledAddonsProvider" /> class.
    /// </summary>
    /// <param name="game">The game this provider manages addons for.</param>
    /// <param name="config">Configuration provider for addon state (favourites, disabled mods).</param>
    /// <param name="originalCampaignsProvider">Provides original/official campaigns for the game.</param>
    /// <param name="metadataProvider">Provides remote metadata update checks.</param>
    /// <param name="localFilesProvider">Scans and caches parsed addon files on disk.</param>
    /// <param name="channelPublisher">Channel that publishes local file add/remove events.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
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
        _logger = logger;
        _originalCampaignsProvider = originalCampaignsProvider;
        _metadataProvider = metadataProvider;
        _channelPublisher = channelPublisher;
        _localFilesProvider = localFilesProvider;

        _addonFactory = new AddonFactory(game, config, metadataProvider);
        _archivedAddonExtractor = new ArchivedAddonExtractor(localFilesProvider, logger);
        _addonActivator = new AddonActivator(config);

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
                                var isUnpacked = await _archivedAddonExtractor.UnpackAndUpdateIfNeededAsync(parsedFile);

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
    ///     Unsubscribe from events and release resources.
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

    /// <summary>
    ///     Raised when one or more addons are added, removed, or their cache is rebuilt.
    /// </summary>
    public event AddonChanged? AddonsChangedEvent;

    /// <summary>
    ///     Build or refresh the internal caches for all addon types.
    /// </summary>
    /// <param name="createNew">If true, clear the cache for <paramref name="addonType" /> before rebuilding.</param>
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
            ArgumentNullException.ThrowIfNull(_campaignsCache);
            ArgumentNullException.ThrowIfNull(_mapsCache);
            ArgumentNullException.ThrowIfNull(_modsCache);

            AddonsChangedEvent?.Invoke(_game.GameEnum, addonType);
        }
    }

    /// <summary>
    ///     Returns the internal cache list for the specified addon type.
    /// </summary>
    /// <param name="addonType">Addon type to look up.</param>
    /// <returns>The cache list corresponding to the addon type.</returns>
    private List<BaseAddon> GetCacheByAddonType(AddonTypeEnum addonType)
    {
        var cache = addonType switch
        {
            AddonTypeEnum.TC => _campaignsCache,
            AddonTypeEnum.Map => _mapsCache,
            AddonTypeEnum.Mod => _modsCache,
            _ => throw new NotSupportedException()
        };

        return cache;
    }

    /// <summary>
    ///     Fills the cache for the specified addon type by processing all parsed addon files.
    /// </summary>
    /// <param name="parsedAddonFiles">Parsed addon files to process.</param>
    /// <param name="addonType">Addon type to cache.</param>
    private async Task FillCacheAsync(IReadOnlyList<ParsedAddonFile> parsedAddonFiles, AddonTypeEnum addonType)
    {
        foreach (var parsedAddonFile in parsedAddonFiles)
        {
            if (parsedAddonFile.Manifest is not null && parsedAddonFile.Manifest.SupportedGame.Game != _game.GameEnum)
            {
                continue;
            }

            var isUnpacked = await _archivedAddonExtractor.UnpackAndUpdateIfNeededAsync(parsedAddonFile);

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

                if (!GrpInfoProvider.TryGetAddonsFromGrpInfo(parsedAddonFile.FileInfo.PathToFile, out var addons))
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

                var addon = _addonFactory.GetLooseMapFromFile(parsedAddonFile);

                if (addon is null)
                {
                    continue;
                }

                cache.Add(addon);
            }
            else
            {
                var addon = _addonFactory.GetAddonFromFile(parsedAddonFile);

                if (addon is null || addon.Type != addonType)
                {
                    continue;
                }

                cache.Add(addon);
            }
        }
    }

    /// <summary>
    ///     Add a single addon from its parsed file into the appropriate cache.
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
            addon = _addonFactory.GetLooseMapFromFile(parsedAddonFile);
        }
        else
        {
            addon = _addonFactory.GetAddonFromFile(parsedAddonFile);
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
            _ => throw new NotSupportedException()
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
    ///     Delete addon from disk and remove it from the cache.
    /// </summary>
    /// <param name="parsedAddonFile">Addon to delete.</param>
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

    /// <summary>
    ///     Delete addon from disk and remove it from the cache.
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
    ///     Enable an autoload mod by id, cascading to dependencies and disabling incompatible mods.
    /// </summary>
    /// <param name="addon">Addon id to enable.</param>
    public void EnableAddon(AddonId addon)
    {
        _addonActivator.EnableAddon(addon, _modsCache);
    }

    /// <summary>
    ///     Disable an autoload mod by id, cascading to dependant mods.
    /// </summary>
    /// <param name="addon">Addon id to disable.</param>
    public void DisableAddon(AddonId addon)
    {
        _addonActivator.DisableAddon(addon, _modsCache);
    }

    /// <summary>
    ///     Get list of installed addons of a given type.
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


    /// <summary>
    ///     Returns the list of installed campaigns, including original campaigns and custom campaigns from the cache.
    /// </summary>
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

    /// <summary>
    ///     Returns the list of installed loose map files from the cache.
    /// </summary>
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

    /// <summary>
    ///     Returns the list of installed mods from the cache.
    /// </summary>
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

    /// <summary>
    ///     Convert a parsed addon file into a domain addon object
    /// </summary>
    /// <param name="parsedAddonFile">Parsed addon file to convert.</param>
    /// <summary>
    ///     Convert a parsed addon file into a domain addon object.
    /// </summary>
    /// <param name="parsedAddonFile">Parsed addon file to convert.</param>
    internal BaseAddon? GetAddonFromFile(ParsedAddonFile parsedAddonFile)
    {
        return _addonFactory.GetAddonFromFile(parsedAddonFile);
    }

    /// <summary>
    ///     Handles the MetadataInitialized event by checking each cached addon for available metadata updates.
    /// </summary>
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

    /// <summary>
    ///     Handles the MetadataUpdated event by replacing the old cached addon with the updated version.
    /// </summary>
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
