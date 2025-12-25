using Addons.Addons;
using Common.All;
using Common.All.Enums;
using Common.All.Helpers;

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
    public static bool ValidateAutoloadMod(AutoloadMod autoloadMod, BaseAddon campaign, IEnumerable<KeyValuePair<AddonId, BaseAddon>> mods, List<FeatureEnum> features)
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

        //check if campaign is incomatible with this or all addons
        if (campaign.IncompatibleAddons is not null)
        {
            foreach (var incompatibleAddon in campaign.IncompatibleAddons)
            {
                if (incompatibleAddon.Key.Equals("*"))
                {
                    return false;
                }

                if (incompatibleAddon.Key != autoloadMod.AddonId.Id)
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
    /// Check if addon has all required dependencies
    /// </summary>
    private static bool CheckDependencies(
        AutoloadMod autoloadMod,
        BaseAddon campaign,
        IEnumerable<KeyValuePair<AddonId, BaseAddon>> mods)
    {
        if (autoloadMod.DependentAddons is not null)
        {
            byte passedDependenciesCount = 0;

            foreach (var dependentAddon in autoloadMod.DependentAddons)
            {
                if (campaign.AddonId.Id.Equals(dependentAddon.Key, StringComparison.OrdinalIgnoreCase) &&
                    (dependentAddon.Value is null || VersionComparer.Compare(campaign.AddonId.Version, dependentAddon.Value)))
                {
                    passedDependenciesCount++;
                    continue;
                }

                if (campaign.DependentAddons?.TryGetValue(dependentAddon.Key, out var dependentAddonVersion) ?? false &&
                    (dependentAddon.Value is null || VersionComparer.Compare(dependentAddonVersion, dependentAddon.Value)))
                {
                    passedDependenciesCount++;
                    continue;
                }

                foreach (var addon in mods)
                {
                    if (!dependentAddon.Key.Equals(addon.Key.Id, StringComparison.OrdinalIgnoreCase))
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
    /// Checks if mod is incompatible with other enabled addons or campaign
    /// </summary>
    private static bool CheckIncompatibles(
        AutoloadMod autoloadMod,
        BaseAddon campaign,
        IEnumerable<KeyValuePair<AddonId, BaseAddon>> mods
        )
    {
        var campaignIncompatibles = campaign.IncompatibleAddons?.ToDictionary() ?? [];
        campaignIncompatibles.Add(campaign.AddonId.Id, campaign.AddonId.Version);
        campaignIncompatibles.AddRange(mods.Where(x => x.Value is AutoloadMod { IsEnabled: true }).ToDictionary(x => x.Key.Id, x => x.Key.Version));

        if (autoloadMod.IncompatibleAddons is not null)
        {
            foreach (var a in campaignIncompatibles)
            {
                foreach (var b in autoloadMod.IncompatibleAddons)
                {
                    if (a.Key != b.Key)
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
        }

        return true;
    }
}
