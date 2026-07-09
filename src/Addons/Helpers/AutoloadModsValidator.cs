using Addons.Addons;
using Core.All.Enums;
using Core.All.Helpers;

namespace Addons.Helpers;

/// <summary>
///     Validates whether autoload mods are compatible with the current campaign, port, and other mods.
/// </summary>
public static class AutoloadModsValidator
{
    /// <summary>
    ///     Check if autoload mod works with current port and addons
    /// </summary>
    /// <param name="autoloadMod">Autoload mod</param>
    /// <param name="campaign">Campaign</param>
    /// <param name="mods">Autoload mods</param>
    /// <param name="features">Features supported by the port</param>
    public static bool ValidateAutoloadMod(AutoloadMod autoloadMod, BaseAddon campaign, IReadOnlyList<BaseAddon> mods, List<FeatureEnum> features)
    {
        if (!autoloadMod.IsEnabled)
        {
            //skipping disabled mods
            return false;
        }

        if (autoloadMod.SupportedGame.GameEnum != campaign.SupportedGame.GameEnum &&
            !(autoloadMod.SupportedGame.GameEnum is GameEnum.Redneck && campaign.SupportedGame.GameEnum is GameEnum.RidesAgain))
        {
            //skipping mod for different game
            return false;
        }

        if (autoloadMod.SupportedGame.GameVersion?.Equals(campaign.SupportedGame.GameVersion, StringComparison.OrdinalIgnoreCase) is false)
        {
            //skipping mod for different game version
            return false;
        }

        if (autoloadMod.RequiredFeatures?.Except(features).Any() is true)
        {
            //skipping mod that requires unsupported features
            return false;
        }

        //check if campaign is incompatible with this or all addons
        if (campaign.IncompatibleAddons is not null)
        {
            foreach (var incompatibleAddon in campaign.IncompatibleAddons)
            {
                if (incompatibleAddon.Key.Equals("*"))
                {
                    return false;
                }

                if (!incompatibleAddon.Key.Equals(autoloadMod.AddonId.Id, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var areEqual = VersionComparer.Compare(incompatibleAddon.Value, autoloadMod.AddonId.Version);

                if (areEqual)
                {
                    return false;
                }
            }
        }

        var areDependenciesPassed = CheckDependencies(autoloadMod, campaign, mods);

        if (!areDependenciesPassed)
        {
            return false;
        }

        var areIncompatiblesPassed = CheckIncompatibles(autoloadMod, campaign, mods);

        return areIncompatiblesPassed;
    }


    /// <summary>
    ///     Checks whether the dependent addons of the autoload mod are satisfied by the campaign or other enabled mods.
    /// </summary>
    private static bool CheckDependencies(
        AutoloadMod autoloadMod,
        BaseAddon campaign,
        IReadOnlyList<BaseAddon> mods
        )
    {
        if (autoloadMod.DependentAddons is null)
        {
            return true;
        }

        byte passedDependenciesCount = 0;

        foreach (var dependentAddon in autoloadMod.DependentAddons)
        {
            if (campaign.AddonId.Id.Equals(dependentAddon.Key, StringComparison.OrdinalIgnoreCase) &&
                (dependentAddon.Value is null || VersionComparer.Compare(campaign.AddonId.Version, dependentAddon.Value)))
            {
                passedDependenciesCount++;

                continue;
            }

            if (campaign.DependentAddons is not null &&
                campaign.DependentAddons.TryGetValue(dependentAddon.Key, out var dependentAddonVersion) &&
                (dependentAddon.Value is null || VersionComparer.Compare(dependentAddonVersion, dependentAddon.Value)))
            {
                passedDependenciesCount++;

                continue;
            }

            foreach (var addon in mods)
            {
                if (!dependentAddon.Key.Equals(addon.AddonId.Id, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (dependentAddon.Value is null || VersionComparer.Compare(addon.AddonId.Version, dependentAddon.Value))
                {
                    passedDependenciesCount++;
                }
            }
        }

        return autoloadMod.DependentAddons.Count == passedDependenciesCount;
    }

    /// <summary>
    ///     Checks whether the autoload mod is incompatible with the campaign or any other enabled mods.
    /// </summary>
    private static bool CheckIncompatibles(
        AutoloadMod autoloadMod,
        BaseAddon campaign,
        IReadOnlyList<BaseAddon> mods
        )
    {
        if (autoloadMod.IncompatibleAddons is null)
        {
            return true;
        }

        var campaignIncompatibles = campaign.IncompatibleAddons?.ToDictionary() ?? [];
        campaignIncompatibles.TryAdd(campaign.AddonId.Id, campaign.AddonId.Version);

        foreach (var addon in mods.Where(x => x is AutoloadMod
                     {
                         IsEnabled: true
                     }
                     ))
        {
            campaignIncompatibles.TryAdd(addon.AddonId.Id, addon.AddonId.Version);
        }

        foreach (var a in campaignIncompatibles)
        {
            foreach (var b in autoloadMod.IncompatibleAddons)
            {
                if (!a.Key.Equals(b.Key, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var areEqual = VersionComparer.Compare(a.Value, b.Value);

                if (areEqual)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
