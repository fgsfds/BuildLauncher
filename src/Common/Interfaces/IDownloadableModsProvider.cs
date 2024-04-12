using Common.Enums;
using Common.Helpers;
using System.Collections.Immutable;

namespace Common.Interfaces
{
    public interface IDownloadableModsProvider
    {
        event ModChanged ModDownloadedEvent;

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
        ImmutableList<IDownloadableMod> GetDownloadableMods(AddonTypeEnum modTypeEnum);

        /// <summary>
        /// Create cache of downloadable mods if it doesn't exist
        /// </summary>
        Task CreateCacheAsync();
    }
}