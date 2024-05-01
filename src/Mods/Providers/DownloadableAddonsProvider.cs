using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Common.Tools;
using Mods.Helpers;
using Mods.Serializable;
using System.Collections.Immutable;
using System.Text.Json;

namespace Mods.Providers
{
    /// <summary>
    /// Class that provides lists of addons available to download
    /// </summary>
    public sealed class DownloadableAddonsProvider : IDownloadableAddonsProvider
    {
        private readonly IGame _game;
        private readonly ArchiveTools _archiveTools;

        private static Dictionary<GameEnum, Dictionary<AddonTypeEnum, Dictionary<string, IDownloadableAddon>>>? _cache;
        private static readonly SemaphoreSlim _semaphore = new(1);

        public event AddonChanged AddonDownloadedEvent;

        /// <inheritdoc/>
        public Progress<float> Progress { get; private set; }

        [Obsolete($"Don't create directly. Use {nameof(DownloadableAddonsProviderFactory)}.")]
        public DownloadableAddonsProvider(
            IGame game,
            ArchiveTools archiveTools
            )
        {
            _game = game;
            _archiveTools = archiveTools;

            Progress = _archiveTools.Progress;
        }


        /// <inheritdoc/>
        public async Task CreateCacheAsync()
        {
            _semaphore.Wait();

            if (_cache is not null)
            {
                _semaphore.Release();
                return;
            }

            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                await using var stream = await client.GetStreamAsync(Consts.Manifests).ConfigureAwait(false);

                using StreamReader file = new(stream);
                var fixesXml = file.ReadToEnd();

                var addons = JsonSerializer.Deserialize(fixesXml, DownloadableModManifestsListContext.Default.ListDownloadableAddonDto);

                addons.ThrowIfNull();

                _cache = [];

                foreach (var addon in addons)
                {
                    //hack for RR
                    if (addon.Game is GameEnum.RedneckRA)
                    {
                        addon.Game = GameEnum.Redneck;
                    }

                    _cache.TryAdd(addon.Game, []);
                    _cache[addon.Game].TryAdd(addon.AddonType, []);
                    _cache[addon.Game][addon.AddonType].TryAdd(addon.Id, addon);
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                _semaphore.Release();
            }
        }


        /// <inheritdoc/>
        public ImmutableList<IDownloadableAddon> GetDownloadableAddons(AddonTypeEnum addonType)
        {
            if (_cache is null || !_cache.TryGetValue(_game.GameEnum, out var downloadableAddons))
            {
                return [];
            }

            if (downloadableAddons is null || !downloadableAddons.TryGetValue(addonType, out var addonTypeCache))
            {
                return [];
            }

            var installedAddons = _game.InstalledAddonsProvider.GetInstalledAddon(addonType);

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
            var file = Path.GetFileName(url);
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

            await _archiveTools.DownloadFileAsync(new(url), pathToFile).ConfigureAwait(false);

            _game.InstalledAddonsProvider.AddAddon(addon.AddonType, pathToFile);

            AddonDownloadedEvent?.Invoke(_game, addon.AddonType);
        }
    }
}
