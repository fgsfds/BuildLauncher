using Addons.Addons;
using Common;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;

namespace Addons.Helpers;

public static class AutoloadModsValidator
{
    /// <summary>
    /// Check if autoload mod works with current port and addons
    /// </summary>
    /// <param name="autoloadMod">Autoload mod</param>
    /// <param name="campaign">Campaign</param>
    /// <param name="mods">Autoload mods</param>
    /// <param name="features">Features supported by the port</param>
    public static bool ValidateAutoloadMod(AutoloadMod autoloadMod, IAddon campaign, Dictionary<AddonVersion, IAddon> mods, List<FeatureEnum> features)
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

        var areDependenciesPassed = CheckDependencies(autoloadMod, campaign, mods);

        if (!areDependenciesPassed)
        {
            return false;
        }

        var areIncompatiblesPassed = CheckIncompatibles(autoloadMod, campaign, mods);

        return areIncompatiblesPassed;
    }


    /// <summary>
    /// Check if addon has all required dependencies
    /// </summary>
    private static bool CheckDependencies(
        AutoloadMod autoloadMod,
        IAddon campaign,
        Dictionary<AddonVersion, IAddon> mods)
    {
        if (autoloadMod.DependentAddons is not null)
        {
            byte passedDependenciesCount = 0;

            foreach (var dependentAddon in autoloadMod.DependentAddons)
            {
                if (dependentAddon.Key.Equals(campaign.Id, StringComparison.InvariantCultureIgnoreCase) &&
                    (dependentAddon.Value is null || VersionComparer.Compare(campaign.Version, dependentAddon.Value)))
                {
                    passedDependenciesCount++;
                    continue;
                }

                foreach (var addon in mods)
                {
                    if (!dependentAddon.Key.Equals(addon.Key.Id, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    if (dependentAddon.Value is null)
                    {
                        passedDependenciesCount++;
                    }
                    else if (VersionComparer.Compare(addon.Key.Version, dependentAddon.Value))
                    {
                        passedDependenciesCount++;
                    }
                }
            }

            return autoloadMod.DependentAddons.Count == passedDependenciesCount;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Check if addon doesn't have any loaded incompatibles
    /// </summary>
    private static bool CheckIncompatibles(AutoloadMod autoloadMod, IAddon campaign, Dictionary<AddonVersion, IAddon> mods)
    {
        if (autoloadMod.IncompatibleAddons is null)
        {
            return true;
        }

        foreach (var incompatibleAddon in autoloadMod.IncompatibleAddons)
        {
            //What a fucking mess...
            //if campaign id equals addon id
            if (campaign.Id.Equals(incompatibleAddon.Key, StringComparison.InvariantCultureIgnoreCase) &&
                //AND either incompatible addon's version is null
                (incompatibleAddon.Value is null ||
                //OR campaign's version is null
                campaign.Version is null ||
                //OR addon's and campaigns's versions match
                VersionComparer.Compare(campaign.Version, incompatibleAddon.Value)
                ))
            {
                //the addon is incompatible
                return false;
            }

            foreach (var addon in mods)
            {
                if (incompatibleAddon.Key != addon.Key.Id)
                {
                    continue;
                }

                if (addon.Value is AutoloadMod aMod &&
                    !aMod.IsEnabled)
                {
                    continue;
                }

                if (incompatibleAddon.Value is null)
                {
                    return false;
                }
                else if (VersionComparer.Compare(addon.Key.Version, incompatibleAddon.Value))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
