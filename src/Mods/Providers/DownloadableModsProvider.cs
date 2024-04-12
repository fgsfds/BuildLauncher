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
    public sealed class DownloadableModsProvider : IDownloadableModsProvider
    {
        private readonly IGame _game;
        private readonly ArchiveTools _archiveTools;

        private static Dictionary<GameEnum, Dictionary<AddonTypeEnum, Dictionary<string, IDownloadableMod>>>? _cache;
        private static readonly SemaphoreSlim _semaphore = new(1);

        public event ModChanged ModDownloadedEvent;

        /// <inheritdoc/>
        public Progress<float> Progress { get; private set; }

        [Obsolete($"Don't create directly. Use {nameof(DownloadableModsProviderFactory)}.")]
        public DownloadableModsProvider(
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
            await using var stream = await client.GetStreamAsync(Consts.Manifests).ConfigureAwait(false);

            using StreamReader file = new(stream);
            var fixesXml = file.ReadToEnd();

            var list = JsonSerializer.Deserialize(fixesXml, DownloadableModManifestsListContext.Default.ListDownloadableAddonDto);

            if (list is null)
            {
                ThrowHelper.Exception();
            }

            _cache = [];

            foreach (var mod in list)
            {
                _cache.TryAdd(mod.Game, []);
                _cache[mod.Game].TryAdd(mod.ModType, []);
                _cache[mod.Game][mod.ModType].TryAdd(mod.Id, mod);
            }

            _semaphore.Release();
        }


        /// <inheritdoc/>
        public ImmutableList<IDownloadableMod> GetDownloadableMods(AddonTypeEnum modTypeEnum)
        {
            if (_cache is null || !_cache.TryGetValue(_game.GameEnum, out var downloadableMods))
            {
                return [];
            }

            if (downloadableMods is null || !downloadableMods.TryGetValue(modTypeEnum, out var modTypeCache))
            {
                return [];
            }

            var installedMods = _game.InstalledModsProvider.GetInstalledMods(modTypeEnum);

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
        public async Task DownloadModAsync(IDownloadableMod mod)
        {
            var url = mod.DownloadUrl;
            var file = Path.GetFileName(url.ToString());
            string path;

            if (mod.ModType is AddonTypeEnum.TC)
            {
                path = _game.CampaignsFolderPath;
            }
            else if (mod.ModType is AddonTypeEnum.Map)
            {
                path = _game.MapsFolderPath;
            }
            else if (mod.ModType is AddonTypeEnum.Mod)
            {
                path = _game.ModsFolderPath;
            }
            else
            {
                ThrowHelper.NotImplementedException(mod.ModType.ToString());
                return;
            }

            var pathToFile = Path.Combine(path, file);

            await _archiveTools.DownloadFileAsync(new(url), pathToFile).ConfigureAwait(false);

            _game.InstalledModsProvider.AddMod(mod.ModType, pathToFile);

            ModDownloadedEvent?.Invoke(_game, mod.ModType);
        }
    }
}
