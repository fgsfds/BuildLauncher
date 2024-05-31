﻿using Common.Enums;
using Common.Helpers;

namespace Common.Interfaces
{
    public interface IInstalledAddonsProvider
    {
        event AddonChanged AddonsChangedEvent;

        /// <summary>
        /// Add addon to cache
        /// </summary>
        /// <param name="addonType">Addon type</param>
        /// <param name="pathToFile">Path to addon file</param>
        void AddAddon(AddonTypeEnum addonType, string pathToFile);

        /// <summary>
        /// Delete addon from cache and disk
        /// </summary>
        /// <param name="addon">Addon</param>
        void DeleteAddon(IAddon addon);

        /// <summary>
        /// Get installed addons
        /// </summary>
        /// <param name="addonType">Addon type</param>
        Dictionary<AddonVersion, IAddon> GetInstalledAddons(AddonTypeEnum addonType);

        /// <summary>
        /// Create cache of installed addons
        /// </summary>
        /// <param name="createNew">Clear current cache and create new</param>
        Task CreateCache(bool createNew);

        /// <summary>
        /// Disable addon
        /// </summary>
        /// <param name="id">Addon id</param>
        void DisableAddon(AddonVersion id);

        /// <summary>
        /// Enable addon
        /// </summary>
        /// <param name="id">Addon id</param>
        void EnableAddon(AddonVersion id);
    }
}