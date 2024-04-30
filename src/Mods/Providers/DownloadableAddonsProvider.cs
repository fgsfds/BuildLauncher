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
    /// Class that provides lists of mods available to download
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

                var list = JsonSerializer.Deserialize(fixesXml, DownloadableModManifestsListContext.Default.ListDownloadableAddonDto);

                list.ThrowIfNull();

                _cache = [];

                foreach (var mod in list)
                {
                    //hack for RR
                    if (mod.Game is GameEnum.RedneckRA)
                    {
                        mod.Game = GameEnum.Redneck;
                    }

                    _cache.TryAdd(mod.Game, []);
                    _cache[mod.Game].TryAdd(mod.AddonType, []);
                    _cache[mod.Game][mod.AddonType].TryAdd(mod.Id, mod);
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
        public ImmutableList<IDownloadableAddon> GetDownloadableAddons(AddonTypeEnum modTypeEnum)
        {
            if (_cache is null || !_cache.TryGetValue(_game.GameEnum, out var downloadableMods))
            {
                return [];
            }

            if (downloadableMods is null || !downloadableMods.TryGetValue(modTypeEnum, out var modTypeCache))
            {
                return [];
            }

            var installedMods = _game.InstalledAddonsProvider.GetInstalledAddon(modTypeEnum);

            foreach (var downloadableMod in modTypeCache)
            {
                if (installedMods.TryGetValue(downloadableMod.Key, out var installedMod))
                {
                    downloadableMod.Value.IsInstalled = true;

                    if (VersionComparer.Compare(downloadableMod.Value.Version, installedMod.Version, ">"))
                    {
                        downloadableMod.Value.HasNewerVersion = true;
                    }
                }
                else
                {
                    downloadableMod.Value.IsInstalled = false;
                }
            }

            return [.. modTypeCache.Values];
        }


        /// <inheritdoc/>
        public async Task DownloadAddonAsync(IDownloadableAddon mod)
        {
            var url = mod.DownloadUrl;
            var file = Path.GetFileName(url.ToString());
            string path;

            if (mod.AddonType is AddonTypeEnum.TC)
            {
                path = _game.CampaignsFolderPath;
            }
            else if (mod.AddonType is AddonTypeEnum.Map)
            {
                path = _game.MapsFolderPath;
            }
            else if (mod.AddonType is AddonTypeEnum.Mod)
            {
                path = _game.ModsFolderPath;
            }
            else
            {
                ThrowHelper.NotImplementedException(mod.AddonType.ToString());
                return;
            }

            var pathToFile = Path.Combine(path, file);

            await _archiveTools.DownloadFileAsync(new(url), pathToFile).ConfigureAwait(false);

            _game.InstalledAddonsProvider.AddAddon(mod.AddonType, pathToFile);

            AddonDownloadedEvent?.Invoke(_game, mod.AddonType);
        }
    }
}
