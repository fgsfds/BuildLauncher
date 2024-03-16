using Common.Enums;

namespace Common.Interfaces
{
    public delegate void ModInstalled(IGame game, ModTypeEnum modType);

    public interface IInstalledModsProvider
    {
        event ModInstalled NotifyModDeleted;

        /// <summary>
        /// Add mod to cache
        /// </summary>
        /// <param name="modTypeEnum">Mod type</param>
        /// <param name="pathToFile">Path to mod file</param>
        void AddMod(ModTypeEnum modTypeEnum, string pathToFile);

        /// <summary>
        /// Delete mod from cache and disk
        /// </summary>
        /// <param name="mod">Mod</param>
        void DeleteMod(IMod mod);

        /// <summary>
        /// Get installed mods
        /// </summary>
        /// <param name="modTypeEnum">Mod type</param>
        Dictionary<Guid, IMod> GetInstalledMods(ModTypeEnum modTypeEnum);

        /// <summary>
        /// Create cache of installed mods if it doesn't exist
        /// </summary>
        Task CreateCache();
    }
}