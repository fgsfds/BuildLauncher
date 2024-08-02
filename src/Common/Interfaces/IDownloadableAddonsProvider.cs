using Common.Enums;
using Common.Helpers;
using System.Collections.Immutable;

namespace Common.Interfaces
{
    public interface IDownloadableAddonsProvider
    {
        event AddonChanged AddonDownloadedEvent;

        /// <summary>
        /// Download progress
        /// </summary>
        Progress<float> Progress { get; }

        /// <summary>
        /// Download addon
        /// </summary>
        /// <param name="addon">Addon</param>
        Task DownloadAddonAsync(IDownloadableAddon addon);

        /// <summary>
        /// Download addon
        /// </summary>
        /// <param name="addonType">Addon type</param>
        ImmutableList<IDownloadableAddon> GetDownloadableAddons(AddonTypeEnum addonType);

        /// <summary>
        /// Create cache of downloadable addons
        /// </summary>
        Task CreateCacheAsync(bool createNew);
    }
}