using System.Collections.Immutable;
using System.Security.Cryptography;
using Common.All;
using Common.All.Enums;
using Common.All.Helpers;
using Common.All.Serializable.Downloadable;
using Common.Client.Helpers;
using Common.Client.Interfaces;
using Common.Client.Tools;
using CommunityToolkit.Diagnostics;
using Games.Games;
using Microsoft.Extensions.Logging;

namespace Addons.Providers;

/// <summary>
/// Class that provides lists of addons available to download
/// </summary>
public sealed class DownloadableAddonsProvider
{
    private readonly BaseGame _game;
    private readonly ArchiveTools _archiveTools;
    private readonly FilesDownloader _filesDownloader;
    private readonly IApiInterface _apiInterface;
    private readonly InstalledAddonsProvider _installedAddonsProvider;
    private readonly ILogger _logger;

    private Dictionary<AddonTypeEnum, Dictionary<AddonId, DownloadableAddonJsonModel>>? _cache;

    private static readonly SemaphoreSlim _semaphore = new(1);

    public event AddonChanged? AddonDownloadedEvent;

    /// <summary>
    /// Download progress
    /// </summary>
    public Progress<float> Progress { get; } = new();


    [Obsolete($"Don't create directly. Use {nameof(DownloadableAddonsProviderFactory)}.")]
    public DownloadableAddonsProvider(
        BaseGame game,
        ArchiveTools archiveTools,
        FilesDownloader filesDownloader,
        IApiInterface apiInterface,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        ILogger logger
        )
    {
        _game = game;
        _archiveTools = archiveTools;
        _filesDownloader = filesDownloader;
        _apiInterface = apiInterface;
        _logger = logger;

        _installedAddonsProvider = installedAddonsProviderFactory.Get(_game);
    }

    /// <summary>
    /// Create downloadable addons cache
    /// </summary>
    /// <param name="createNew">Drop existing cache and create new</param>
    /// <returns>Is cache created successfully</returns>
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

    /// <summary>
    /// Get a list of downloadable addons
    /// </summary>
    /// <param name="addonType">Addon type</param>
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

    /// <summary>
    /// Download addon
    /// </summary>
    /// <param name="addon">Addon</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<bool> DownloadAddonAsync(
        DownloadableAddonJsonModel addon,
        CancellationToken cancellationToken
        )
    {
        try
        {
            _filesDownloader.ProgressChanged += OnProgressChanged;
            _archiveTools.ProgressChanged += OnProgressChanged;

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
                return false;
            }

            var pathToFile = Path.Combine(path, file);

            var isDownloaded = await _filesDownloader.DownloadFileAsync(url, pathToFile, cancellationToken).ConfigureAwait(false);

            if (!isDownloaded)
            {
                _logger.LogError($"Error while downloading {addon.Title}.");
                return false;
            }

            var doesMd5Match = await CheckFileMD5Async(pathToFile, addon.MD5, cancellationToken).ConfigureAwait(false);

            if (!doesMd5Match)
            {
                if (File.Exists(pathToFile))
                {
                    File.Delete(pathToFile);
                }

                _logger.LogError($"File hash for {addon.Title} doesn't match.");
                return false;
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

            AddonDownloadedEvent?.Invoke(_game.GameEnum, addon.AddonType);

            return true;
        }
        finally
        {
            _filesDownloader.ProgressChanged -= OnProgressChanged;
            _archiveTools.ProgressChanged -= OnProgressChanged;
        }
    }


    /// <summary>
    /// Check MD5 of the local file
    /// </summary>
    /// <param name="filePath">Full path to the file</param>
    /// <param name="fixMD5">MD5 that the file's hash will be compared to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>true if check is passed</returns>
    private static async Task<bool> CheckFileMD5Async(
        string filePath,
        string fixMD5,
        CancellationToken cancellationToken
        )
    {
        using var md5 = MD5.Create();

        await using var stream = File.OpenRead(filePath);

        var hash = await md5.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
        var hashStr = Convert.ToHexString(hash);

        return fixMD5.Equals(hashStr, StringComparison.OrdinalIgnoreCase);
    }

    private void OnProgressChanged(object? sender, float e)
    {
        ((IProgress<float>)Progress).Report(e);
    }
}
