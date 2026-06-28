using System.Collections.Immutable;
using System.Security.Cryptography;
using Core.All;
using Core.All.Enums;
using Core.All.Helpers;
using Core.All.Serializable.Downloadable;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Core.Client.Tools;
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
    private readonly LocalFilesProvider _filesProvider;
    private readonly IApiInterface _apiInterface;
    private readonly InstalledAddonsProvider _installedAddonsProvider;
    private readonly ILogger<DownloadableAddonsProvider> _logger;

    private Dictionary<AddonTypeEnum, Dictionary<AddonId, DownloadableAddonJsonModel>>? _cache;

    private static readonly SemaphoreSlim _globalCacheSemaphore = new(1);

    //public event AddonChanged? AddonsChangedEvent;

    /// <summary>
    /// Download progress
    /// </summary>
    public Progress<float> Progress { get; } = new();


    [Obsolete($"Don't create directly. Use {nameof(DownloadableAddonsProviderFactory)}.")]
    public DownloadableAddonsProvider(
        BaseGame game,
        ArchiveTools archiveTools,
        FilesDownloader filesDownloader,
        LocalFilesProvider filesProvider,
        IApiInterface apiInterface,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        ILogger<DownloadableAddonsProvider> logger
        )
    {
        _game = game;
        _archiveTools = archiveTools;
        _filesDownloader = filesDownloader;
        _filesProvider = filesProvider;
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
            await _globalCacheSemaphore.WaitAsync().ConfigureAwait(false);

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
            _ = _globalCacheSemaphore.Release();
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
            var existingAddons = installedAddons
                .Where(x => x.AddonId.Id.Equals(downloadableAddon.Key.Id, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.AddonId)
                .ToList();

            downloadableAddon.Value.IsInstalled = true;

            if (existingAddons.Count == 0)
            {
                downloadableAddon.Value.IsInstalled = false;
                continue;
            }

            //Death Wish hack
            if (addonType is AddonTypeEnum.TC &&
                downloadableAddon.Key.Id.Contains("death-wish", StringComparison.OrdinalIgnoreCase) &&
                downloadableAddon.Key.Version?.StartsWith('1') is true)
            {
                downloadableAddon.Value.IsInstalled = existingAddons.Contains(downloadableAddon.Key);
            }
            else
            {
                foreach (var existingVersion in existingAddons.Select(static x => x.Version).Where(static x => x is not null))
                {
                    downloadableAddon.Value.IsUpdateAvailable = true;

                    if (!VersionComparer.Compare(downloadableAddon.Value.Version, existingVersion, ComparisonOperatorEnum.GreaterThan))
                    {
                        downloadableAddon.Value.IsUpdateAvailable = false;
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
                throw new NotSupportedException(addon.AddonType.ToString());
            }

            var pathToFile = Path.Combine(path, file);

            var isDownloaded = await _filesDownloader.DownloadFileAsync(url, pathToFile, cancellationToken).ConfigureAwait(false);

            if (!isDownloaded)
            {
                _logger.LogError($"Error while downloading {addon.Title}.");
                return false;
            }

            var doesHashMatch = await CheckFileHashAsync(pathToFile, addon.Sha256, cancellationToken).ConfigureAwait(false);

            if (!doesHashMatch)
            {
                if (File.Exists(pathToFile))
                {
                    File.Delete(pathToFile);
                }

                _logger.LogError($"File hash for {addon.Title} doesn't match.");
                return false;
            }

            _ = await _filesProvider.TryAddFileToCacheAsync(pathToFile, _game.GameEnum).ConfigureAwait(false);

            if (!ClientProperties.IsDeveloperMode)
            {
                var result = await _apiInterface.IncreaseNumberOfInstallsAsync(addon.Id).ConfigureAwait(false);

                if (result)
                {
                    addon.Installs++;
                }
            }

            //AddonsChangedEvent?.Invoke(_game.GameEnum, addon.AddonType);

            return true;
        }
        finally
        {
            _filesDownloader.ProgressChanged -= OnProgressChanged;
            _archiveTools.ProgressChanged -= OnProgressChanged;
        }
    }


    /// <summary>
    /// Check hash of the local file
    /// </summary>
    /// <param name="filePath">Full path to the file</param>
    /// <param name="expectedHash">Hash that the file's hash will be compared to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>true if check is passed</returns>
    private static async Task<bool> CheckFileHashAsync(
        string filePath,
        string expectedHash,
        CancellationToken cancellationToken
        )
    {
        await using var stream = File.OpenRead(filePath);
        var actualHash = await SHA256.HashDataAsync(stream, cancellationToken);
        var actualHashStr = Convert.ToHexString(actualHash);

        return expectedHash.Equals(actualHashStr, StringComparison.OrdinalIgnoreCase);
    }

    private void OnProgressChanged(object? sender, float e)
    {
        ((IProgress<float>)Progress).Report(e);
    }
}
