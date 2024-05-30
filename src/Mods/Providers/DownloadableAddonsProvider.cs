using ClientCommon.API;
using ClientCommon.Helpers;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Common.Tools;
using System.Collections.Immutable;

namespace Mods.Providers
{
    /// <summary>
    /// Class that provides lists of addons available to download
    /// </summary>
    public sealed class DownloadableAddonsProvider : IDownloadableAddonsProvider
    {
        private readonly IGame _game;
        private readonly ArchiveTools _archiveTools;
        private readonly ApiInterface _apiInterface;

        private Dictionary<AddonTypeEnum, Dictionary<string, IDownloadableAddon>>? _cache;
        private readonly SemaphoreSlim _semaphore = new(1);

        public event AddonChanged AddonDownloadedEvent;

        /// <inheritdoc/>
        public Progress<float> Progress { get; private set; }

        [Obsolete($"Don't create directly. Use {nameof(DownloadableAddonsProviderFactory)}.")]
        public DownloadableAddonsProvider(
            IGame game,
            ArchiveTools archiveTools,
            ApiInterface apiInterface
            )
        {
            _game = game;
            _archiveTools = archiveTools;
            _apiInterface = apiInterface;

            Progress = _archiveTools.Progress;
        }


        /// <inheritdoc/>
        public async Task CreateCacheAsync()
        {
            await _semaphore.WaitAsync();

            if (_cache is not null)
            {
                _semaphore.Release();
                return;
            }

            var addons = await _apiInterface.GetAddonsAsync(_game.GameEnum).ConfigureAwait(false);

            if (addons is null ||  addons.Count == 0)
            {
                _semaphore.Release();
                return;
            }

            _cache = [];

            foreach (var addon in addons)
            {
                _cache.TryAdd(addon.AddonType, []);
                _cache[addon.AddonType].TryAdd(addon.Id, addon);
            }

            _semaphore.Release();
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

            var installedAddons = _game.InstalledAddonsProvider.GetInstalledAddons(addonType);

            foreach (var downloadableAddon in addonTypeCache)
            {
                if (installedAddons.TryGetValue(downloadableAddon.Key, out var installedAddon))
                {
                    downloadableAddon.Value.IsInstalled = true;

                    if (VersionComparer.Compare(downloadableAddon.Value.Version, installedAddon.Version, ">"))
                    {
                        downloadableAddon.Value.HasNewerVersion = true;
                    }
                }
                else
                {
                    downloadableAddon.Value.IsInstalled = false;
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
                ThrowHelper.NotImplementedException(addon.AddonType.ToString());
                return;
            }

            var pathToFile = Path.Combine(path, file);

            await _archiveTools.DownloadFileAsync(url, pathToFile).ConfigureAwait(false);

            _game.InstalledAddonsProvider.AddAddon(addon.AddonType, pathToFile);

            if (!ClientProperties.IsDevMode)
            {
                var result = await _apiInterface.IncreaseNumberOfInstallsAsync(addon.Id);

                if (result)
                {
                    addon.Installs++;
                }
            }

            AddonDownloadedEvent?.Invoke(_game, addon.AddonType);
        }
    }
}
