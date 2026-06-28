using Addons.Addons;
using Core.All;
using Core.All.Helpers;
using Core.Client.Interfaces;

namespace Addons.Helpers;

/// <summary>
/// Activates and deactivates autoload mods, cascading to dependencies and incompatible mods.
/// </summary>
internal sealed class AddonActivator
{
    private readonly IConfigProvider _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddonActivator"/> class.
    /// </summary>
    /// <param name="config">Configuration provider used to persist mod enabled/disabled state.</param>
    public AddonActivator(IConfigProvider config)
    {
        _config = config;
    }

    /// <summary>
    /// Enable an autoload mod, recursively enabling its dependencies and disabling incompatible mods.
    /// Disables other versions of the same mod id.
    /// </summary>
    /// <param name="addon">Addon id to enable.</param>
    /// <param name="modsCache">The current list of installed mods to search within.</param>
    public void EnableAddon(AddonId addon, List<BaseAddon> modsCache)
    {
        var existing = modsCache.FirstOrDefault(x => x.AddonId.Equals(addon));

        if (existing is not AutoloadMod autoloadMod)
        {
            return;
        }

        if (autoloadMod.IsEnabled)
        {
            return;
        }

        autoloadMod.IsEnabled = true;

        if (autoloadMod.DependentAddons is not null)
        {
            foreach (var dep in autoloadMod.DependentAddons)
            {
                EnableAddon(new(dep.Key, dep.Value), modsCache);
            }
        }

        if (autoloadMod.IncompatibleAddons is not null)
        {
            foreach (var inc in autoloadMod.IncompatibleAddons)
            {
                DisableAddon(new(inc.Key, inc.Value), modsCache);
            }
        }

        var otherVersions = modsCache
             .Where(x =>
                 x.AddonId.Id.Equals(addon.Id, StringComparison.OrdinalIgnoreCase) &&
                 !VersionComparer.Compare(x.AddonId.Version, addon.Version, ComparisonOperatorEnum.Equals) &&
                 (x.FileInfo is null || !x.FileInfo.Equals(autoloadMod.FileInfo))
                 );

        foreach (var version in otherVersions)
        {
            DisableAddon(version.AddonId, modsCache);
        }

        _config.ChangeModState(addon, true);
    }

    /// <summary>
    /// Disable an autoload mod, recursively disabling any mods that depend on it.
    /// </summary>
    /// <param name="addon">Addon id to disable.</param>
    /// <param name="modsCache">The current list of installed mods to search within.</param>
    public void DisableAddon(AddonId addon, List<BaseAddon> modsCache)
    {
        var existing = modsCache.FirstOrDefault(x => x.AddonId.Equals(addon));

        if (existing is not AutoloadMod autoloadMod)
        {
            return;
        }

        if (!autoloadMod.IsEnabled)
        {
            return;
        }

        autoloadMod.IsEnabled = false;

        var deps = modsCache.Where(x => x.DependentAddons?.ContainsKey(autoloadMod.AddonId.Id) ?? false);

        foreach (var dep in deps)
        {
            DisableAddon(dep.AddonId, modsCache);
        }

        _config.ChangeModState(addon, false);
    }
}
