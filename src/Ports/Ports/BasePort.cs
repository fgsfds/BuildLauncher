using Common;
using Common.Client.Helpers;
using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
using Mods.Serializable.Addon;
using System.Reflection;
using System.Text;

namespace Ports.Ports;

/// <summary>
/// Base class for ports
/// </summary>
public abstract class BasePort
{
    /// <summary>
    /// Port enum
    /// </summary>
    public abstract PortEnum PortEnum { get; }

    /// <summary>
    /// Main executable
    /// </summary>
    public abstract string Exe { get; }

    /// <summary>
    /// Name of the port
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Games supported by the port
    /// </summary>
    public abstract List<GameEnum> SupportedGames { get; }

    /// <summary>
    /// Features supported by the port
    /// </summary>
    public abstract List<FeatureEnum> SupportedFeatures { get; }

    /// <summary>
    /// Currently installed version
    /// </summary>
    public abstract string? InstalledVersion { get; }

    /// <summary>
    /// Path to port install folder
    /// </summary>
    public virtual string PathToExecutableFolder => Path.Combine(ClientProperties.PortsFolderPath, PortFolderName);

    /// <summary>
    /// Is port installed
    /// </summary>
    public virtual bool IsInstalled => InstalledVersion is not null;

    /// <summary>
    /// Games versions supported by the port
    /// </summary>
    public virtual List<string> SupportedGamesVersions => [];

    /// <summary>
    /// Path to port exe
    /// </summary>
    public string FullPathToExe => Path.Combine(PathToExecutableFolder, Exe);


    /// <summary>
    /// Name of the config file
    /// </summary>
    protected abstract string ConfigFile { get; }

    /// <summary>
    /// Cmd parameter to add folder to search path
    /// </summary>
    protected abstract string AddDirectoryParam { get; }

    /// <summary>
    /// Cmd parameter to load additional GRP file
    /// </summary>
    protected abstract string AddGrpParam { get; }

    /// <summary>
    /// Cmd parameter to load additional file
    /// </summary>
    protected abstract string AddFileParam { get; }

    /// <summary>
    /// Cmd parameter to load additional Def file
    /// </summary>
    protected abstract string AddDefParam { get; }

    /// <summary>
    /// Cmd parameter to load additional Con file
    /// </summary>
    protected abstract string AddConParam { get; }

    /// <summary>
    /// Cmd parameter to load main Def file
    /// </summary>
    protected abstract string MainDefParam { get; }

    /// <summary>
    /// Cmd parameter to load main Con file
    /// </summary>
    protected abstract string MainConParam { get; }

    /// <summary>
    /// Cmd parameter for skill selection
    /// </summary>
    protected abstract string SkillParam { get; }

    /// <summary>
    /// Name of the folder that contains the port files
    /// By default is the same as <see cref="Name"/>
    /// </summary>
    private string PortFolderName => Name;

    /// <summary>
    /// Port's icon
    /// </summary>
    public Stream Icon => ImageHelper.FileNameToStream($"{Name}.png", Assembly.GetExecutingAssembly());


    /// <summary>
    /// Get command line parameters to start the game with selected campaign and autoload mods
    /// </summary>
    /// <param name="game"></param>
    /// <param name="addon"></param>
    /// <param name="mods"></param>
    /// <param name="skipIntro"></param>
    /// <param name="skipStartup"></param>
    /// <param name="skill"></param>
    /// <returns></returns>
    public string GetStartGameArgs(
        IGame game,
        IAddon addon,
        Dictionary<AddonVersion, IAddon> mods,
        bool skipIntro,
        bool skipStartup,
        byte? skill = null)
    {
        StringBuilder sb = new();

        BeforeStart(game, addon);

        if (skipIntro)
        {
            GetSkipIntroParameter(sb);
        }

        if (skipStartup)
        {
            GetSkipStartupParameter(sb);
        }

        GetAutoloadModsArgs(sb, game, addon, mods);

        GetStartCampaignArgs(sb, game, addon);

        if (skill is not null)
        {
            sb.Append($" {SkillParam}{skill}");
        }

        return sb.ToString();
    }


    /// <summary>
    /// Get startup args for manifested maps
    /// </summary>
    protected void GetMapArgs(StringBuilder sb, IGame game, IAddon camp)
    {
        //TODO e#m#
        if (camp.StartMap is MapFileDto mapFile)
        {
            sb.Append($@" {AddFileParam}""{Path.Combine(game.MapsFolderPath, camp.FileName!)}""");
            sb.Append($@" -map ""{mapFile.File}""");
        }
        else
        {
            ThrowHelper.NotImplementedException();
        }
    }


    /// <summary>
    /// Get startup args for loose maps
    /// </summary>
    protected void GetLooseMapArgs(StringBuilder sb, IGame game, IAddon camp)
    {
        camp.StartMap.ThrowIfNotType<MapFileDto>(out var mapFile);

        sb.Append($@" {AddDirectoryParam}""{game.MapsFolderPath}""");
        sb.Append($@" -map ""{mapFile.File}""");
    }


    /// <summary>
    /// Check if autoload mod works with current port and addon
    /// </summary>
    /// <param name="autoloadMod">Autoload mod</param>
    /// <param name="campaign">Campaign</param>
    protected bool ValidateAutoloadMod(AutoloadMod autoloadMod, IAddon campaign, Dictionary<AddonVersion, IAddon> addons)
    {
        if (!autoloadMod.IsEnabled)
        {
            //skipping disabled mods
            return false;
        }

        if (autoloadMod.SupportedGame.GameEnum != campaign.SupportedGame.GameEnum)
        {
            //skipping mod for different game
            return false;
        }

        if (autoloadMod.SupportedGame.GameVersion is not null &&
            !autoloadMod.SupportedGame.GameVersion.Equals(campaign.SupportedGame.GameVersion, StringComparison.InvariantCultureIgnoreCase))
        {
            return false;
        }

        if (autoloadMod.RequiredFeatures is not null &&
            autoloadMod.RequiredFeatures.Except(SupportedFeatures).Any())
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

                foreach (var addon in addons)
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

                foreach (var addon in addons)
                {
                    if (incompatibleAddon.Key != addon.Key.Id)
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


    protected void GetBloodArgs(StringBuilder sb, BloodGame game, IAddon addon)
    {
        if (addon is LooseMap lMap)
        {
            if (lMap.BloodIni is null)
            {
                sb.Append($@" -ini ""{Consts.BloodIni}""");
            }
            else
            {
                sb.Append($@" -ini ""{Path.GetFileName(lMap.BloodIni)}""");
            }

            GetLooseMapArgs(sb, game, addon);
            return;
        }
        
        if (addon is not BloodCampaign bCamp)
        {
            ThrowHelper.ArgumentException(nameof(addon));
            return;
        }

        if (bCamp.INI is not null)
        {
            sb.Append($@" -ini ""{bCamp.INI}""");
        }
        else if (bCamp.DependentAddons is not null && bCamp.DependentAddons.ContainsKey(nameof(BloodAddonEnum.BloodCP)))
        {
            sb.Append($@" -ini ""{Consts.CrypticIni}""");
        }


        if (bCamp.FileName is null)
        {
            return;
        }


        if (bCamp.RFF is not null)
        {
            sb.Append($@" -rff ""{bCamp.RFF}""");
        }


        if (bCamp.SND is not null)
        {
            sb.Append($@" -snd ""{bCamp.SND}""");
        }


        if (bCamp.Type is AddonTypeEnum.TC)
        {
            if (bCamp.FileName.Equals("addon.json"))
            {
                sb.Append($@" -game_dir ""{Path.GetDirectoryName(bCamp.PathToFile)}""");
            }
            else
            {
                sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, bCamp.FileName)}""");
            }
        }
        else if (bCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, game, bCamp);
        }
        else
        {
            ThrowHelper.NotImplementedException($"Mod type {bCamp.Type} is not supported");
            return;
        }
    }


    protected void GetSlaveArgs(StringBuilder sb, SlaveGame game, IAddon addon)
    {
        if (addon is LooseMap)
        {
            GetLooseMapArgs(sb, game, addon);
            return;
        }

        if (addon is not SlaveCampaign sCamp)
        {
            ThrowHelper.ArgumentException(nameof(addon));
            return;
        }

        if (sCamp.FileName is null)
        {
            return;
        }


        if (sCamp.Type is AddonTypeEnum.TC)
        {
            sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, sCamp.FileName)}""");
        }
        else if (sCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, game, sCamp);
        }
        else
        {
            ThrowHelper.NotImplementedException($"Mod type {sCamp.Type} is not supported");
            return;
        }
    }


    /// <summary>
    /// Method to perform before starting the port
    /// </summary>
    /// <param name="game">Game</param>
    /// <param name="campaign">Campaign</param>
    protected virtual void BeforeStart(IGame game, IAddon campaign) { }

    /// <summary>
    /// Get command line arguments to start custom map or campaign
    /// </summary>
    /// <param name="sb">String builder for parameters</param>
    /// <param name="game">Game</param>
    /// <param name="addon">Map/campaign</param>
    protected abstract void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon);

    /// <summary>
    /// Get command line arguments to load mods
    /// </summary>
    /// <param name="sb">String builder for parameters</param>
    /// <param name="game">Game</param>
    /// <param name="addon">Campaign\map</param>
    protected abstract void GetAutoloadModsArgs(StringBuilder sb, IGame game, IAddon addon, Dictionary<AddonVersion, IAddon> mods);

    /// <summary>
    /// Return command line parameter to skip intro
    /// </summary>
    /// <param name="sb">String builder for parameters</param>
    protected abstract void GetSkipIntroParameter(StringBuilder sb);

    /// <summary>
    /// Return command line parameter to skip startup window
    /// </summary>
    /// <param name="sb">String builder for parameters</param>
    protected abstract void GetSkipStartupParameter(StringBuilder sb);
}
