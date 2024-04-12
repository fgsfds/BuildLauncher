﻿using Common.Enums;
using Common.Helpers;

namespace Common.Interfaces
{
    public interface IInstalledModsProvider
    {
        event ModChanged ModDeletedEvent;

        /// <summary>
        /// Add mod to cache
        /// </summary>
        /// <param name="modTypeEnum">Mod type</param>
        /// <param name="pathToFile">Path to mod file</param>
        void AddMod(AddonTypeEnum modTypeEnum, string pathToFile);

        /// <summary>
        /// Delete mod from cache and disk
        /// </summary>
        /// <param name="mod">Mod</param>
        void DeleteMod(IAddon mod);

        /// <summary>
        /// Get installed mods
        /// </summary>
        /// <param name="modTypeEnum">Mod type</param>
        Dictionary<string, IAddon> GetInstalledMods(AddonTypeEnum modTypeEnum);

        /// <summary>
        /// Create cache of installed mods if it doesn't exist
        /// </summary>
        /// <param name="createNew">Clear current cache and create new</param>
        Task CreateCache(bool createNew);
    }
}