using Common.Enums;
using System.Collections.Immutable;

namespace Common.Interfaces
{
    public delegate void ModDownloaded(IGame game, ModTypeEnum modType);

    public interface IDownloadableModsProvider
    {
        event ModDownloaded NotifyModDownloaded;

        /// <summary>
        /// Download progress
        /// </summary>
        Progress<float> Progress { get; }
        
        /// <summary>
        /// Download mod
        /// </summary>
        /// <param name="mod">Mod</param>
        /// <summary>
        /// Get list of downloadable mods
        /// </summary>
        /// <param name="modTypeEnum">Mod type enum</param>
        Task DownloadModAsync(IDownloadableMod mod);

        /// <summary>
        /// Download mod
        /// </summary>
        /// <param name="mod">Mod</param>
        ImmutableList<IDownloadableMod> GetDownloadableMods(ModTypeEnum modTypeEnum);

        /// <summary>
        /// Create cache of downloadable mods if it doesn't exist
        /// </summary>
        Task CreateCacheAsync();
    }
}