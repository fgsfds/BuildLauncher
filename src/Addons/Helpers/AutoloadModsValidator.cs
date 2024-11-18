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

        if (autoloadMod.DependentAddons is not null)
        {
            foreach (var dependantAddon in autoloadMod.DependentAddons)
            {
                if (campaign.Id.Equals(dependantAddon.Key, StringComparison.InvariantCultureIgnoreCase) &&
                    (dependantAddon.Value is null || VersionComparer.Compare(campaign.Version, dependantAddon.Value)))
                {
                    return true;
                }

                foreach (var addon in mods)
                {
                    if (!dependantAddon.Key.Equals(addon.Key.Id, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    else if (dependantAddon.Value is null)
                    {
                        return true;
                    }
                    else if (VersionComparer.Compare(addon.Key.Version, dependantAddon.Value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        if (autoloadMod.IncompatibleAddons is not null)
        {
            foreach (var incompatibleAddon in autoloadMod.IncompatibleAddons)
            {
                //What a fucking mess...
                //if campaign id equals addon id
                if (campaign.Id.Equals(incompatibleAddon.Key, StringComparison.InvariantCultureIgnoreCase) &&
                    //AND either both campaign's and addon's versions are null
                    ((incompatibleAddon.Value is null && campaign.Version is null) ||
                    //OR addon's version is not null and does match the comparer
                    (incompatibleAddon.Value is not null && VersionComparer.Compare(campaign.Version, incompatibleAddon.Value))))
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
                    else if (incompatibleAddon.Value is null)
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

        return true;
    }
}
