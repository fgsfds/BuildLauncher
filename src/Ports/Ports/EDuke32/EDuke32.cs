using Addons.Addons;
using Common.Client.Helpers;
using Common.Enums;
using Common.Enums.Addons;
using Common.Enums.Versions;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Diagnostics;
using Games.Games;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using System.Reflection;
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
    protected override string WinExe => "eduke32.exe";

    /// <inheritdoc/>
    protected override string LinExe => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    public override string Name => "EDuke32";

    /// <inheritdoc/>
    protected override string ConfigFile => "eduke32.cfg";

    /// <inheritdoc/>
    protected override string MainGrpParam => "-gamegrp ";

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
    protected override string AddRffParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string AddSndParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    public override List<GameEnum> SupportedGames =>
        [
        GameEnum.Duke3D,
        GameEnum.NAM,
        GameEnum.WW2GI
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


    public EDuke32()
    {
        CreateWTStopgapFolder();
    }


    /// <summary>
    /// Create folder with files required for World Tour to work with EDuke32
    /// </summary>
    private void CreateWTStopgapFolder()
    {
        if (PortEnum is not PortEnum.EDuke32)
        {
            return;
        }

        var stopgapFolder = Path.Combine(PortInstallFolderPath, ClientConsts.WTStopgap);

        if (Directory.Exists(stopgapFolder))
        {
            return;
        }

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Ports.Assets.WTStopgap.zip");

        Guard.IsNotNull(stream);

        using var archive = ZipArchive.Open(stream);

        archive.ExtractToDirectory(stopgapFolder);
    }


    /// <inheritdoc/>
    protected override void GetSkipIntroParameter(StringBuilder sb) => sb.Append(" -quick");

    /// <inheritdoc/>
    protected override void GetSkipStartupParameter(StringBuilder sb) => sb.Append(" -nosetup");


    /// <inheritdoc/>
    public override void BeforeStart(IGame game, IAddon campaign)
    {
        MoveSaveFiles(game, campaign);

        FixGrpInConfig();

        FixWtFiles(game, campaign);
    }

    protected void MoveSaveFiles(IGame game, IAddon campaign)
    {
        var saveFolder = GetPathToAddonSavedGamesFolder(game.ShortName, campaign.Id);

        if (!Directory.Exists(saveFolder))
        {
            return;
        }

        var saves = Directory.GetFiles(saveFolder);

        string firstPart = campaign.IsFolder ? Path.GetDirectoryName(campaign.PathToFile)! : PortInstallFolderPath;

        foreach (var save in saves)
        {
            var destFileName = Path.Combine(firstPart, Path.GetFileName(save)!);
            File.Move(save, destFileName, true);
        }
    }

    /// <inheritdoc/>
    public override void AfterEnd(IGame game, IAddon campaign)
    {
        //copying saved games into separate folder
        var saveFolder = GetPathToAddonSavedGamesFolder(game.ShortName, campaign.Id);

        string path;

        if (campaign.IsFolder)
        {
            path = Path.GetDirectoryName(campaign.PathToFile)!;
        }
        else
        {
            path = PortInstallFolderPath;
        }

        var files = from file in Directory.GetFiles(path)
                    from ext in SaveFileExtensions
                    where file.EndsWith(ext)
                    select file;

        if (!Directory.Exists(saveFolder))
        {
            _ = Directory.CreateDirectory(saveFolder);
        }

        foreach (var file in files)
        {
            var destFileName = Path.Combine(saveFolder, Path.GetFileName(file)!);
            File.Move(file, destFileName, true);
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
        else if (game is NamGame nGame)
        {
            _ = sb.Append($@" {AddDirectoryParam}""{game.GameInstallFolder}""");

            GetNamWW2GIArgs(sb, nGame, addon);
        }
        else if (game is WW2GIGame giGame)
        {
            _ = sb.Append($@" {AddDirectoryParam}""{game.GameInstallFolder}""");

            GetNamWW2GIArgs(sb, giGame, addon);
        }
        else
        {
            ThrowHelper.ThrowNotSupportedException($"Mod type {addon.Type} for game {game} is not supported");
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
            _ = sb.Append(@$" {AddDirectoryParam}""{Path.GetDirectoryName(game.Duke64RomPath)}"" {MainGrpParam}""{Path.GetFileName(game.Duke64RomPath)}""");
            return;
        }

        if (addon.SupportedGame.GameVersion?.Equals(nameof(DukeVersionEnum.Duke3D_WT), StringComparison.InvariantCultureIgnoreCase) == true)
        {
            _ = sb.Append($@" {AddDirectoryParam}""{game.DukeWTInstallPath}"" -addon {(byte)DukeAddonEnum.Base} {AddDirectoryParam}""{Path.Combine(PortInstallFolderPath, ClientConsts.WTStopgap)}"" {MainGrpParam}e32wt.grp {AddDefParam}e32wt.def");
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

                    _ = sb.Append($" {AddGrpParam}DUKEDC.GRP");

                    if (File.Exists(Path.Combine(addonPath, "DUKEDC.CON")))
                    {
                        _ = sb.Append($" {MainConParam}DUKEDC.CON");
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

                    _ = sb.Append($" {AddGrpParam}NWINTER.GRP {MainConParam}NWINTER.CON");
                }
                //CARIBBEAN
                else if (addon.DependentAddons.ContainsKey(nameof(DukeAddonEnum.DukeVaca)))
                {
                    var addonPath = game.AddonsPaths[DukeAddonEnum.DukeVaca];

                    if (!addonPath.Equals(game.GameInstallFolder))
                    {
                        _ = sb.Append($@" {AddDirectoryParam}""{addonPath}""");
                    }

                    _ = sb.Append($" {AddGrpParam}VACATION.GRP");

                    if (File.Exists(Path.Combine(addonPath, "VACATION.CON")))
                    {
                        _ = sb.Append($" {MainConParam}VACATION.CON");
                    }
                }
            }
        }

        if (addon.FileName is null)
        {
            return;
        }

        if (addon is LooseMapEntity)
        {
            GetLooseMapArgs(sb, game, addon);
            return;
        }


        addon.ThrowIfNotType(out DukeCampaignEntity dCamp);


        if (dCamp.MainCon is not null)
        {
            _ = sb.Append($@" {MainConParam}""{dCamp.MainCon}""");
        }

        if (dCamp.AdditionalCons?.Any() is true)
        {
            foreach (var con in dCamp.AdditionalCons)
            {
                _ = sb.Append($@" {AddConParam}""{con}""");
            }
        }


        if (dCamp.Type is AddonTypeEnum.TC)
        {
            if (dCamp.Executables is not null)
            {
                //don't add addon dir if the port is overridden
            }
            else
            {
                _ = sb.Append($@" {AddFileParam}""{dCamp.PathToFile}""");
            }
        }
        else if (dCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, dCamp);
        }
        else
        {
            ThrowHelper.ThrowNotSupportedException($"Mod type {dCamp.Type} is not supported");
            return;
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

    /// <summary>
    /// Rename WT's ART files if custom campaign is launched
    /// </summary>
    protected void FixWtFiles(IGame game, IAddon campaign)
    {
        if (game is not DukeGame)
        {
            return;
        }

        Guard.IsNotNull(game.GameInstallFolder);

        var art1 = Path.Combine(game.GameInstallFolder, "TILES009.ART");
        var art1r = Path.Combine(game.GameInstallFolder, "TILES009._ART");

        var art2 = Path.Combine(game.GameInstallFolder, "TILES020.ART");
        var art2r = Path.Combine(game.GameInstallFolder, "TILES020._ART");

        var art3 = Path.Combine(game.GameInstallFolder, "TILES021.ART");
        var art3r = Path.Combine(game.GameInstallFolder, "TILES021._ART");

        var art4 = Path.Combine(game.GameInstallFolder, "TILES022.ART");
        var art4r = Path.Combine(game.GameInstallFolder, "TILES022._ART");


        if (campaign.Id.Equals(nameof(DukeVersionEnum.Duke3D_WT), StringComparison.OrdinalIgnoreCase))
        {
            RestoreWtFiles(game);
        }
        else
        {
            if (File.Exists(art1))
            {
                File.Move(art1, art1r, true);
            }

            if (File.Exists(art2))
            {
                File.Move(art2, art2r, true);
            }

            if (File.Exists(art3))
            {
                File.Move(art3, art3r, true);
            }

            if (File.Exists(art4))
            {
                File.Move(art4, art4r, true);
            }
        }
    }
}
