using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Common.Tools;
using Mods.Serializable;
using System.Collections.Immutable;
using System.Text.Json;

namespace Mods.Providers
{
    /// <summary>
    /// Class that provides lists of mods available to download
    /// </summary>
    public sealed class DownloadableModsProvider
    {
        private ImmutableList<DownloadableMod>? _cache;
        private readonly SemaphoreSlim _semaphore = new(1);

        private readonly ArchiveTools _archiveTools;
        private readonly InstalledModsProvider _installedModsProvider;

        /// <summary>
        /// Operation progress
        /// </summary>
        public Progress<float> Progress = new();

        public delegate void ModDownloaded(IGame game, ModTypeEnum modType);
        public event ModDownloaded NotifyModDownloaded;

        public DownloadableModsProvider(
            ArchiveTools archiveTools,
            InstalledModsProvider installedModsProvider
            )
        {
            _archiveTools = archiveTools;
            _installedModsProvider = installedModsProvider;
        }


        /// <summary>
        /// Update cached list of downloadable mods from online repo
        /// </summary>
        public async Task UpdateCachedListAsync()
        {
            _semaphore.Wait();

            if (_cache is null)
            {
                await UpdateCacheAsync().ConfigureAwait(false);
            }

            _semaphore.Release();

            _cache.ThrowIfNull();
        }


        /// <summary>
        /// Get list of downloadable mods
        /// </summary>
        /// <param name="gameEnum">Game enum</param>
        /// <param name="modTypeEnum">Mod type enum</param>
        public ImmutableList<DownloadableMod> GetDownloadableMods(IGame game, ModTypeEnum modTypeEnum)
        {
            var result = _cache?.Where(x => x.Game == game.GameEnum && x.ModType == modTypeEnum);

            if (result is null)
            {
                return [];
            }

            var installedMods = _installedModsProvider.GetMods(game, modTypeEnum);

            foreach (var downloadableMod in result)
            {
                if (installedMods.TryGetValue(downloadableMod.Guid, out var installedMod))
                {
                    downloadableMod.IsInstalled = true;

                    if (downloadableMod.Version > installedMod.Version)
                    {
                        downloadableMod.HasNewerVersion = true;
                    }
                }
                else
                {
                    downloadableMod.IsInstalled = false;
                }
            }

            return [.. result];
        }


        /// <summary>
        /// Download mod
        /// </summary>
        /// <param name="mod">Mod</param>
        /// <param name="game">Game</param>
        public async Task DownloadModAsync(DownloadableMod mod, IGame game)
        {
            var url = mod.DownloadUrl;
            var file = Path.GetFileName(url.ToString());
            string path;

            if (mod.ModType is ModTypeEnum.Campaign)
            {
                path = game.CampaignsFolderPath;
            }
            else if (mod.ModType is ModTypeEnum.Map)
            {
                path = game.MapsFolderPath;
            }
            else if (mod.ModType is ModTypeEnum.Autoload)
            {
                path = game.ModsFolderPath;
            }
            else
            {
                ThrowHelper.NotImplementedException(mod.ModType.ToString());
                return;
            }

            var pathToFile = Path.Combine(path, file);

            await _archiveTools.DownloadFileAsync(new(url), pathToFile).ConfigureAwait(false);

            Progress = _archiveTools.Progress;

            _installedModsProvider.AddMod(game, mod.ModType, pathToFile);

            NotifyModDownloaded?.Invoke(game, mod.ModType);
        }


        /// <summary>
        /// Download fixes xml from online repository
        /// </summary>
        /// <returns></returns>
        private async Task UpdateCacheAsync()
        {
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(10);
            await using var stream = await client.GetStreamAsync(Consts.Manifests).ConfigureAwait(false);

            using StreamReader file = new(stream);
            var fixesXml = file.ReadToEnd();

            var list = JsonSerializer.Deserialize(fixesXml, DownloadableModManifestsListContext.Default.ListDownloadableMod);

            _cache = [.. list];
        }
    }
}
