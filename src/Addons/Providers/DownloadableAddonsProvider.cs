using Common;
using Common.Client.Api;
using Common.Client.Helpers;
using Common.Client.Interfaces;
using Common.Client.Tools;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Diagnostics;
using System.Collections.Immutable;

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

    private Dictionary<AddonTypeEnum, Dictionary<AddonVersion, IDownloadableAddon>>? _cache;
    private static readonly SemaphoreSlim _semaphore = new(1);

    public event AddonChanged AddonDownloadedEvent;

    /// <inheritdoc/>
    public Progress<float> Progress { get; private set; }

    [Obsolete($"Don't create directly. Use {nameof(DownloadableAddonsProviderFactory)}.")]
    public DownloadableAddonsProvider(
        IGame game,
        ArchiveTools archiveTools,
        IApiInterface apiInterface,
        InstalledAddonsProviderFactory installedAddonsProviderFactory
        )
    {
        _game = game;
        _archiveTools = archiveTools;
        _apiInterface = apiInterface;
        _installedAddonsProvider = installedAddonsProviderFactory.GetSingleton(_game);

        Progress = _archiveTools.Progress;
    }


    /// <inheritdoc/>
    public async Task CreateCacheAsync(bool createNew)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        if (_cache is not null && !createNew)
        {
            _ = _semaphore.Release();
            return;
        }

        var addons = await _apiInterface.GetAddonsAsync(_game.GameEnum).ConfigureAwait(false);

        if (addons is null || addons.Count == 0)
        {
            _ = _semaphore.Release();
            return;
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

        _ = _semaphore.Release();
    }


    /// <inheritdoc/>
    public ImmutableList<IDownloadableAddon> GetDownloadableAddons(AddonTypeEnum addonType)
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
            var existinsAddons = installedAddons.Where(x => x.Key.Id == downloadableAddon.Key.Id).Select(x => x.Key);

            downloadableAddon.Value.IsInstalled = true;

            if (!existinsAddons.Any())
            {
                downloadableAddon.Value.IsInstalled = false;
                continue;
            }

            //Death Wish hack
            if (addonType is AddonTypeEnum.TC &&
                downloadableAddon.Key.Id.Contains("death-wish", StringComparison.InvariantCultureIgnoreCase) &&
                downloadableAddon.Key.Version!.StartsWith('1'))
            {
                if (existinsAddons.Contains(downloadableAddon.Key))
                {
                    downloadableAddon.Value.IsInstalled = true;
                }
                else
                {
                    downloadableAddon.Value.IsInstalled = false;
                }

                continue;
            }
            else
            {
                foreach (var existingVersion in existinsAddons.Select(static x => x.Version).Where(static x => x is not null))
                {
                    downloadableAddon.Value.HasNewerVersion = true;

                    if (VersionComparer.Compare(downloadableAddon.Value.Version, existingVersion!, "<="))
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
    public async Task DownloadAddonAsync(IDownloadableAddon addon)
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

        await _archiveTools.DownloadFileAsync(url, pathToFile).ConfigureAwait(false);

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
