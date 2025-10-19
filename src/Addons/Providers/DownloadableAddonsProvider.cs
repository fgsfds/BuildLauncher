using System.Collections.Immutable;
using Common.All;
using Common.All.Enums;
using Common.All.Helpers;
using Common.All.Interfaces;
using Common.All.Serializable.Downloadable;
using Common.Client.Helpers;
using Common.Client.Interfaces;
using Common.Client.Tools;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Addons.Providers;

/// <summary>
/// Class that provides lists of addons available to download
/// </summary>
public sealed class DownloadableAddonsProvider : IDownloadableAddonsProvider
{
    private readonly IGame _game;
    private readonly ArchiveTools _archiveTools;
    private readonly IApiInterface _apiInterface;
    private readonly InstalledAddonsProvider _installedAddonsProvider;
    private readonly ILogger _logger;

    private Dictionary<AddonTypeEnum, Dictionary<AddonId, DownloadableAddonJsonModel>>? _cache;

    private static readonly SemaphoreSlim _semaphore = new(1);

    /// <inheritdoc/>
    public event AddonChanged? AddonDownloadedEvent;

    /// <inheritdoc/>
    public Progress<float> Progress { get; }


    [Obsolete($"Don't create directly. Use {nameof(DownloadableAddonsProviderFactory)}.")]
    public DownloadableAddonsProvider(
        IGame game,
        ArchiveTools archiveTools,
        IApiInterface apiInterface,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        ILogger logger
        )
    {
        _game = game;
        _archiveTools = archiveTools;
        _apiInterface = apiInterface;
        _logger = logger;

        _installedAddonsProvider = installedAddonsProviderFactory.GetSingleton(_game);

        Progress = _archiveTools.Progress;
    }

    /// <inheritdoc/>
    public async Task<bool> CreateCacheAsync(bool createNew)
    {
        try
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);

            if (_cache is not null && !createNew)
            {
                return true;
            }

            var addons = await _apiInterface.GetAddonsAsync(_game.GameEnum).ConfigureAwait(false);

            if (addons is null)
            {
                return false;
            }

            if (addons.Count == 0)
            {
                return true;
            }

            _cache = [];

            addons = [.. addons.Where(a => !a.IsDisabled)
            .OrderBy(a => a.Title)
            .ThenBy(a => a.Version)];

            foreach (var addon in addons)
            {
                _ = _cache.TryAdd(addon.AddonType, []);
                _ = _cache[addon.AddonType].TryAdd(new(addon.Id, addon.Version), addon);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"=== Error while creating downloadable cache for {_game.GameEnum} ===");
            return false;
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }


    /// <inheritdoc/>
    public ImmutableList<DownloadableAddonJsonModel> GetDownloadableAddons(AddonTypeEnum addonType)
    {
        if (_cache is null)
        {
            return [];
        }

        if (!_cache.TryGetValue(addonType, out var addonTypeCache))
        {
            return [];
        }

        var installedAddons = _installedAddonsProvider.GetInstalledAddonsByType(addonType);

        foreach (var downloadableAddon in addonTypeCache)
        {
            var existingAddons = installedAddons.Where(x => x.Key.Id == downloadableAddon.Key.Id).Select(x => x.Key);

            downloadableAddon.Value.IsInstalled = true;

            if (!existingAddons.Any())
            {
                downloadableAddon.Value.IsInstalled = false;
                continue;
            }

            //Death Wish hack
            if (addonType is AddonTypeEnum.TC &&
                downloadableAddon.Key.Id.Contains("death-wish", StringComparison.OrdinalIgnoreCase) &&
                downloadableAddon.Key.Version!.StartsWith('1'))
            {
                downloadableAddon.Value.IsInstalled = existingAddons.Contains(downloadableAddon.Key);
            }
            else
            {
                foreach (var existingVersion in existingAddons.Select(static x => x.Version).Where(static x => x is not null))
                {
                    downloadableAddon.Value.HasNewerVersion = true;

                    if (VersionComparer.Compare(downloadableAddon.Value.Version, existingVersion, "<="))
                    {
                        downloadableAddon.Value.HasNewerVersion = false;
                        break;
                    }
                }
            }
        }

        return [.. addonTypeCache.Values];
    }


    /// <inheritdoc/>
    public async Task DownloadAddonAsync(
        DownloadableAddonJsonModel addon,
        CancellationToken cancellationToken
        )
    {
        var url = addon.DownloadUrl;
        var file = Path.GetFileName(url.ToString());
        string path;

        if (addon.AddonType is AddonTypeEnum.TC)
        {
            path = _game.CampaignsFolderPath;
        }
        else if (addon.AddonType is AddonTypeEnum.Map)
        {
            path = _game.MapsFolderPath;
        }
        else if (addon.AddonType is AddonTypeEnum.Mod)
        {
            path = _game.ModsFolderPath;
        }
        else
        {
            ThrowHelper.ThrowNotSupportedException(addon.AddonType.ToString());
            return;
        }

        var pathToFile = Path.Combine(path, file);

        var isDownloaded = await _archiveTools.DownloadFileAsync(url, pathToFile, cancellationToken).ConfigureAwait(false);

        if (!isDownloaded)
        {
            return;
        }

        await _installedAddonsProvider.AddAddonAsync(pathToFile).ConfigureAwait(false);

        if (!ClientProperties.IsDeveloperMode)
        {
            var result = await _apiInterface.IncreaseNumberOfInstallsAsync(addon.Id).ConfigureAwait(false);

            if (result)
            {
                addon.Installs++;
            }
        }

        AddonDownloadedEvent?.Invoke(_game, addon.AddonType);
    }
}
