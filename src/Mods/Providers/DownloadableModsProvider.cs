using Common.Enums;
using Common.Helpers;
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
        private ImmutableList<DownloadableMod>? _mods;
        private readonly SemaphoreSlim _semaphore = new(1);


        /// <summary>
        /// Get list of downloadable mods
        /// </summary>
        /// <param name="gameEnum">Game enum</param>
        /// <param name="modTypeEnum">Mod type enum</param>
        public ImmutableList<DownloadableMod> GetDownloadableMods(GameEnum gameEnum, ModTypeEnum modTypeEnum)
        {
            var result = _mods?.Where(x => x.Game == gameEnum && x.ModType == modTypeEnum);

            if (result is null)
            {
                return [];
            }

            return [.. result];
        }


        /// <summary>
        /// Get list of downloadable mods
        /// </summary>
        /// <param name="gameEnum">Game enum</param>
        /// <param name="modTypeEnums">List of mod type enums</param>
        public ImmutableList<DownloadableMod> GetDownloadableMods(GameEnum gameEnum, IEnumerable<ModTypeEnum> modTypeEnums)
        {
            var result = _mods?.Where(x => x.Game == gameEnum && modTypeEnums.Contains(x.ModType));

            if (result is null)
            {
                return [];
            }

            return [.. result];
        }


        /// <summary>
        /// Update cached list of downloadable mods from online repo
        /// </summary>
        public async Task<ImmutableList<DownloadableMod>> UpdateCachedListAsync()
        {
            _semaphore.Wait();

            if (_mods is null)
            {
                await UpdateCacheAsync().ConfigureAwait(false);
            }

            _semaphore.Release();

            _mods.ThrowIfNull();

            return _mods;
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

            _mods = [.. list];
        }
    }
}
