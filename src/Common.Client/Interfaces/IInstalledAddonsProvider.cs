using Common.All;
using Common.All.Enums;
using Common.All.Interfaces;

namespace Common.Client.Interfaces;

public interface IInstalledAddonsProvider
{
    /// <summary>
    /// Add addon to cache
    /// </summary>
    /// <param name="pathToFile">Path to addon file</param>
    Task AddAddonAsync(string pathToFile);

    /// <summary>
    /// Delete addon from cache and disk
    /// </summary>
    /// <param name="addon">Addon</param>
    void DeleteAddon(IAddon addon);

    /// <summary>
    /// Get list od installed addons of a type
    /// </summary>
    /// <param name="addonType">Addon type</param>
    IReadOnlyDictionary<AddonId, IAddon> GetInstalledAddonsByType(AddonTypeEnum addonType);

    /// <summary>
    /// Create cache of installed addons.
    /// </summary>
    /// <param name="createNew">Clear current cache and create new.</param>
    /// <param name="addonType">Addon type.</param>
    Task CreateCache(bool createNew, AddonTypeEnum addonType);

    /// <summary>
    /// Disable addon
    /// </summary>
    /// <param name="id">Addon id</param>
    void DisableAddon(AddonId id);

    /// <summary>
    /// Enable addon
    /// </summary>
    /// <param name="id">Addon id</param>
    void EnableAddon(AddonId id);
}