using Common;
using Common.Enums;
using Common.Enums.Addons;
using Common.Enums.Versions;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
using System.Text;

namespace Ports.Ports.EDuke32;

/// <summary>
/// EDuke32 port
/// </summary>
public class EDuke32 : BasePort
{
    /// <inheritdoc/>
    public override PortEnum PortEnum => PortEnum.EDuke32;

    /// <inheritdoc/>
    public override string Exe => "eduke32.exe";

    /// <inheritdoc/>
    public override string Name => "EDuke32";

    /// <inheritdoc/>
    protected override string ConfigFile => "eduke32.cfg";

    /// <inheritdoc/>
    protected override string AddGrpParam => "-grp ";

    /// <inheritdoc/>
    protected override string AddDirectoryParam => "-j ";

    /// <inheritdoc/>
    protected override string AddFileParam => "-g ";

    /// <inheritdoc/>
    protected override string AddDefParam => "-mh ";

    /// <inheritdoc/>
    protected override string AddConParam => "-mx ";

    /// <inheritdoc/>
    protected override string MainDefParam => "-h ";

    /// <inheritdoc/>
    protected override string MainConParam => "-x ";

    /// <inheritdoc/>
    protected override string SkillParam => "-s";

    /// <inheritdoc/>
    protected override string AddGameDirParam => "-game_dir ";

    /// <inheritdoc/>
    protected override string AddRffParam => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override string AddSndParam => throw new NotImplementedException();

    /// <inheritdoc/>
    public override List<GameEnum> SupportedGames =>
        [
        GameEnum.Duke3D,
        GameEnum.NAM,
        GameEnum.WWIIGI
        ];

    /// <inheritdoc/>
    public override List<string> SupportedGamesVersions =>
        [
        nameof(DukeVersionEnum.Duke3D_13D),
        nameof(DukeVersionEnum.Duke3D_Atomic),
        nameof(DukeVersionEnum.Duke3D_WT)
        ];

    /// <inheritdoc/>
    public override List<FeatureEnum> SupportedFeatures => 
        [
        FeatureEnum.EDuke32_CON,
        FeatureEnum.Dynamic_Lighting,
        FeatureEnum.Hightile,
        FeatureEnum.Models,
        FeatureEnum.Sloped_Sprites,
        FeatureEnum.TROR,
        FeatureEnum.Wall_Rotate_Cstat
        ];

    /// <inheritdoc/>
    public override string? InstalledVersion
    {
        get
        {
            var versionFile = Path.Combine(PortInstallFolderPath, "version");

            if (!File.Exists(versionFile))
            {
                return null;
            }

            return File.ReadAllText(versionFile);
        }
    }

    /// <inheritdoc/>
    protected override void GetSkipIntroParameter(StringBuilder sb) => sb.Append(" -quick");

    /// <inheritdoc/>
    protected override void GetSkipStartupParameter(StringBuilder sb) => sb.Append(" -nosetup");


    /// <inheritdoc/>
    protected override void BeforeStart(IGame game, IAddon campaign)
    {
        MoveSaveFiles(game, campaign);

        FixGrpInConfig();
    }

    protected void MoveSaveFiles(IGame game, IAddon campaign)
    {
        var saveFolder = GetPathToAddonSavedGamesFolder(game.ShortName, campaign.Id);

        if (Directory.Exists(saveFolder))
        {
            var saves = Directory.GetFiles(saveFolder);

            foreach (var save in saves)
            {
                string destFileName;

                if (campaign.IsFolder)
                {
                    destFileName = Path.Combine(Path.GetDirectoryName(campaign.PathToFile)!, Path.GetFileName(save)!);
                }
                else
                {
                    destFileName = Path.Combine(PortInstallFolderPath, Path.GetFileName(save)!);
                }

                File.Move(save, destFileName, true);
            }
        }
    }

    /// <inheritdoc/>
    public override void AfterEnd(IGame game, IAddon campaign)
    {
        var saveFolder = GetPathToAddonSavedGamesFolder(game.ShortName, campaign.Id);

        if (!Directory.Exists(saveFolder))
        {
            _ = Directory.CreateDirectory(saveFolder);
        }

        string path;

        if (campaign.IsFolder)
        {
            path = Path.GetDirectoryName(campaign.PathToFile)!;
        }
        else
        {
            path = PortInstallFolderPath;
        }

        var saves = from file in Directory.GetFiles(path)
                from ext in SaveFileExtensions
                where file.EndsWith(ext)
                select file;

        if (!Directory.Exists(saveFolder))
        {
            _ = Directory.CreateDirectory(saveFolder);
        }

        foreach (var save in saves)
        {
            var destFileName = Path.Combine(saveFolder, Path.GetFileName(save)!);
            File.Move(save, destFileName, true);
        }
    }

    /// <inheritdoc/>
    protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon)
    {
        _ = sb.Append(" -usecwd"); //don't search for steam/gog installs
        _ = sb.Append(" -cachesize 262144"); //set cache to 256MiB

        if (addon.MainDef is not null)
        {
            _ = sb.Append($@" {MainDefParam}""{addon.MainDef}""");
        }
        else
        {
            //overriding default def so gamename.def files are ignored
            _ = sb.Append($@" {MainDefParam}""a""");
        }

        if (addon.AdditionalDefs is not null)
        {
            foreach (var def in addon.AdditionalDefs)
            {
                _ = sb.Append($@" {AddDefParam}""{def}""");
            }
        }


        if (game is DukeGame dGame)
        {
            GetDukeArgs(sb, dGame, addon);
        }
        else
        {
            ThrowHelper.NotImplementedException($"Mod type {addon.Type} for game {game} is not supported");
        }
    }


    /// <summary>
    /// Get startup agrs for Duke
    /// </summary>
    /// <param name="sb">StringBuilder</param>
    /// <param name="game">DukeGame</param>
    /// <param name="addon">DukeCampaign</param>
    protected void GetDukeArgs(StringBuilder sb, DukeGame game, IAddon addon)
    {
        if (addon.SupportedGame.GameEnum is GameEnum.Duke64)
        {
            _ = sb.Append(@$" {AddDirectoryParam}""{Path.GetDirectoryName(game.Duke64RomPath)}"" -gamegrp ""{Path.GetFileName(game.Duke64RomPath)}""");
            return;
        }

        if (addon.SupportedGame.GameVersion is not null &&
            addon.SupportedGame.GameVersion.Equals(nameof(DukeVersionEnum.Duke3D_WT), StringComparison.InvariantCultureIgnoreCase))
        {
            _ = sb.Append($@" {AddDirectoryParam}""{game.DukeWTInstallPath}"" -addon {(byte)DukeAddonEnum.Base} {AddDirectoryParam}""{Path.Combine(game.SpecialFolderPath, Consts.WTStopgap)}"" -gamegrp e32wt.grp");
        }
        else
        {
            _ = sb.Append($@" {AddDirectoryParam}""{game.GameInstallFolder}""");

            if (addon.DependentAddons is not null)
            {
                //DUKE IT OUT IN DC
                if (addon.DependentAddons.ContainsKey(nameof(DukeAddonEnum.DukeDC)))
                {
                    var addonPath = game.AddonsPaths[DukeAddonEnum.DukeDC];

                    if (!addonPath.Equals(game.GameInstallFolder))
                    {
                        _ = sb.Append($@" {AddDirectoryParam}""{addonPath}""");
                    }

                    _ = sb.Append($@" {AddGrpParam}DUKEDC.GRP");

                    if (File.Exists(Path.Combine(addonPath, "DUKEDC.CON")))
                    {
                        _ = sb.Append($@" {MainConParam}DUKEDC.CON");
                    }
                }
                //NUCLEAR WINTER
                else if (addon.DependentAddons.ContainsKey(nameof(DukeAddonEnum.DukeNW)))
                {
                    var addonPath = game.AddonsPaths[DukeAddonEnum.DukeNW];

                    if (!addonPath.Equals(game.GameInstallFolder))
                    {
                        _ = sb.Append($@" {AddDirectoryParam}""{addonPath}""");
                    }

                    _ = sb.Append($@" {AddGrpParam}NWINTER.GRP {MainConParam}NWINTER.CON");
                }
                //CARIBBEAN
                else if (addon.DependentAddons.ContainsKey(nameof(DukeAddonEnum.DukeVaca)))
                {
                    var addonPath = game.AddonsPaths[DukeAddonEnum.DukeVaca];

                    if (!addonPath.Equals(game.GameInstallFolder))
                    {
                        _ = sb.Append($@" {AddDirectoryParam}""{addonPath}""");
                    }

                    _ = sb.Append($@" {AddGrpParam}VACATION.GRP");

                    if (File.Exists(Path.Combine(addonPath, "VACATION.CON")))
                    {
                        _ = sb.Append($@" {MainConParam}VACATION.CON");
                    }
                }
            }
        }


        if (addon.FileName is null)
        {
            return;
        }

        if (addon is LooseMap)
        {
            GetLooseMapArgs(sb, game, addon);
            return;
        }

        if (addon is not DukeCampaign dCamp)
        {
            ThrowHelper.ArgumentException(nameof(addon));
            return;
        }


        if (dCamp.MainCon is not null)
        {
            _ = sb.Append($@" {MainConParam}""{dCamp.MainCon}""");
        }

        if (dCamp.AdditionalCons?.Count > 0)
        {
            foreach (var con in dCamp.AdditionalCons)
            {
                _ = sb.Append($@" {AddConParam}""{con}""");
            }
        }


        if (dCamp.Type is AddonTypeEnum.TC)
        {
            _ = sb.Append($@" {AddFileParam}""{dCamp.PathToFile}""");
            //_ = sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, dCamp.FileName!)}""");
        }
        else if (dCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, game, dCamp);
        }
        else
        {
            ThrowHelper.NotImplementedException($"Mod type {dCamp.Type} is not supported");
            return;
        }
    }


    /// <inheritdoc/>
    protected override void GetAutoloadModsArgs(StringBuilder sb, IGame game, IAddon addon, Dictionary<AddonVersion, IAddon> mods)
    {
        if (mods.Count == 0)
        {
            return;
        }

        var enabledModsCount = 0;

        foreach (var mod in mods)
        {
            if (mod.Value is not AutoloadMod aMod)
            {
                continue;
            }

            if (!ValidateAutoloadMod(aMod, addon, mods))
            {
                continue;
            }

            _ = sb.Append($@" {AddFileParam}""{aMod.FileName}""");

            if (aMod.AdditionalDefs is not null)
            {
                foreach (var def in aMod.AdditionalDefs)
                {
                    _ = sb.Append($@" {AddDefParam}""{def}""");
                }
            }

            if (aMod.AdditionalCons is not null)
            {
                foreach (var con in aMod.AdditionalCons)
                {
                    _ = sb.Append($@" {AddConParam}""{con}""");
                }
            }

            enabledModsCount++;
        }

        if (enabledModsCount > 0)
        {
            _ = sb.Append($@" {AddDirectoryParam}""{game.ModsFolderPath}""");
        }
    }


    /// <summary>
    /// Remove GRP files from the config
    /// </summary>
    protected void FixGrpInConfig()
    {
        var config = Path.Combine(PortInstallFolderPath, ConfigFile);

        if (!File.Exists(config))
        {
            return;
        }

        var contents = File.ReadAllLines(config);

        for (var i = 0; i < contents.Length; i++)
        {
            if (contents[i].StartsWith("SelectedGRP"))
            {
                contents[i] = @"SelectedGRP = """"";
                break;
            }
        }

        File.WriteAllLines(config, contents);
    }
}
