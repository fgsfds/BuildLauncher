using System.Diagnostics;
using System.Text;
using Addons.Addons;
using Common.All.Enums;
using Common.All.Enums.Addons;
using Common.All.Enums.Versions;
using Common.All.Helpers;
using Common.All.Interfaces;
using CommunityToolkit.Diagnostics;
using Games.Games;

namespace Ports.Ports;

/// <summary>
/// Raze port
/// </summary>
public sealed class Raze : BasePort
{
    /// <inheritdoc/>
    public override PortEnum PortEnum => PortEnum.Raze;

    /// <inheritdoc/>
    protected override string WinExe => "raze.exe";

    /// <inheritdoc/>
    protected override string LinExe => "raze";

    /// <inheritdoc/>
    public override string Name => "Raze";

    /// <inheritdoc/>
    public override List<GameEnum> SupportedGames =>
        [
        GameEnum.Blood,
        GameEnum.Duke3D,
        GameEnum.Wang,
        GameEnum.Slave,
        GameEnum.Redneck,
        GameEnum.RidesAgain,
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
    public override string? InstalledVersion =>
        File.Exists(PortExeFilePath)
        ? FileVersionInfo.GetVersionInfo(PortExeFilePath).FileVersion
        : null;

    /// <inheritdoc/>
    public override bool IsSkillSelectionAvailable => false;


    /// <inheritdoc/>
    protected override string ConfigFile => "raze_portable.ini";

    /// <inheritdoc/>
    protected override string AddFileParam => "-file ";

    /// <inheritdoc/>
    protected override string AddDefParam => "-adddef ";

    /// <inheritdoc/>
    protected override string AddConParam => "-addcon ";

    /// <inheritdoc/>
    protected override string MainDefParam => "-def ";

    /// <inheritdoc/>
    protected override string MainConParam => "-con ";

    /// <inheritdoc/>
    protected override string AddDirectoryParam => AddFileParam;

    /// <inheritdoc/>
    protected override string AddGameDirParam => AddFileParam;

    /// <inheritdoc/>
    protected override string AddRffParam => AddFileParam;

    /// <inheritdoc/>
    protected override string AddSndParam => AddFileParam;

    /// <inheritdoc/>
    protected override string MainGrpParam => AddFileParam;

    /// <inheritdoc/>
    protected override string AddGrpParam => AddFileParam;

    /// <inheritdoc/>
    protected override string SkillParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    public override List<FeatureEnum> SupportedFeatures =>
        [
        FeatureEnum.TROR,
        FeatureEnum.Hightile,
        FeatureEnum.Models,
        FeatureEnum.Sloped_Sprites,
        FeatureEnum.Wall_Rotate_Cstat,
        FeatureEnum.SndInfo,
        FeatureEnum.TileFromTexture
        ];


    public Raze() : base()
    {
        try
        {
            MoveOldSavesFolder();
        }
        catch
        {
            Directory.Delete(Path.Combine(PortInstallFolderPath, "Save"));
        }
    }


    [Obsolete]
    private void MoveOldSavesFolder()
    {
        var oldSavesFolder = Path.Combine(PortInstallFolderPath, "Save");

        if (Directory.Exists(oldSavesFolder))
        {
            foreach (var entry in Directory.GetDirectories(oldSavesFolder))
            {
                var folderName = Path.GetFileName(entry);

                string gameFolder;

                if (folderName.StartsWith("blood", StringComparison.OrdinalIgnoreCase))
                {
                    gameFolder = "Blood";
                }
                else if (folderName.StartsWith("duke", StringComparison.OrdinalIgnoreCase))
                {
                    gameFolder = "Duke3D";
                }
                else if (folderName.StartsWith("slave", StringComparison.OrdinalIgnoreCase))
                {
                    gameFolder = "Slave";
                }
                else if (folderName.StartsWith("fury", StringComparison.OrdinalIgnoreCase))
                {
                    gameFolder = "Fury";
                }
                else if (folderName.StartsWith("wang", StringComparison.OrdinalIgnoreCase))
                {
                    gameFolder = "Wang";
                }
                else if (folderName.StartsWith("redneck", StringComparison.OrdinalIgnoreCase))
                {
                    gameFolder = "Redneck";
                }
                else if (folderName.StartsWith("ridesagain", StringComparison.OrdinalIgnoreCase))
                {
                    gameFolder = "Redneck";
                }
                else
                {
                    continue;
                }

                var gameSavesDir = Path.Combine(PortSavedGamesFolderPath, gameFolder);

                if (!Directory.Exists(gameSavesDir))
                {
                    _ = Directory.CreateDirectory(gameSavesDir);
                }

                Directory.Move(entry, Path.Combine(gameSavesDir, folderName));
            }

            Directory.Delete(oldSavesFolder, true);
        }
    }

    /// <inheritdoc/>
    protected override void GetSkipIntroParameter(StringBuilder sb) => sb.Append(" -quick");

    /// <inheritdoc/>
    protected override void GetSkipStartupParameter(StringBuilder sb) => sb.Append(" -nosetup");


    /// <inheritdoc/>
    public override void BeforeStart(BaseGame game, IAddon campaign)
    {
        var config = Path.Combine(PortInstallFolderPath, ConfigFile);

        if (!File.Exists(config))
        {
            //creating default config if it doesn't exist
            const string? DefaultConfig = """
                [GameSearch.Directories]
                Path=.

                [FileSearch.Directories]
                Path=.

                [SoundfontSearch.Directories]
                Path=$PROGDIR/soundfonts

                [GlobalSettings]
                gl_texture_filter=6
                snd_alresampler=Nearest
                gl_tonemap=5
                hw_useindexedcolortextures=true
                mus_extendedlookup=true
                snd_extendedlookup=true
                """;

            if (!Directory.Exists(Path.GetDirectoryName(config)))
            {
                _ = Directory.CreateDirectory(Path.GetDirectoryName(config)!);
            }

            File.WriteAllText(config, DefaultConfig);
        }

        AddGamePathsToConfig(game, campaign, game.GameInstallFolder!, config);

        RestoreRoute66Files(game);

        RestoreWtFiles(game);
    }

    /// <inheritdoc/>
    public override void AfterEnd(BaseGame game, IAddon campaign)
    {
        //nothing to do
    }

    /// <inheritdoc/>
    protected override void GetStartCampaignArgs(StringBuilder sb, BaseGame game, IAddon addon)
    {
        _ = sb.Append($@" -savedir ""{GetPathToAddonSavedGamesFolder(game.ShortName, addon.AddonId.Id)}""");

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
            GetNamWW2GIArgs(sb, nGame, addon);
        }
        else if (game is WW2GIGame gBaseGame)
        {
            GetNamWW2GIArgs(sb, gBaseGame, addon);
        }
        else if (game is BloodGame bGame)
        {
            GetBloodArgs(sb, bGame, addon);
        }
        else if (game is WangGame wGame)
        {
            GetWangArgs(sb, wGame, addon);
        }
        else if (game is SlaveGame sGame)
        {
            GetSlaveArgs(sb, sGame, addon);
        }
        else if (game is RedneckGame rGame)
        {
            GetRedneckArgs(sb, rGame, addon);
        }
        else
        {
            ThrowHelper.ThrowNotSupportedException($"Mod type {addon} for game {game} is not supported");
        }
    }

    private void GetDukeArgs(StringBuilder sb, DukeGame game, IAddon addon)
    {
        if (addon is LooseMapEntity)
        {
            GetLooseMapArgs(sb, game, addon);
            return;
        }


        Guard2.ThrowIfNotType<DukeCampaignEntity>(addon, out var dCamp);

        if (dCamp.SupportedGame.GameVersion is not null &&
            dCamp.SupportedGame.GameVersion.Equals(nameof(DukeVersionEnum.Duke3D_WT), StringComparison.OrdinalIgnoreCase))
        {
            var config = Path.Combine(PortInstallFolderPath, ConfigFile);

            AddGamePathsToConfig(game, addon, game.DukeWTInstallPath!, config);

            _ = sb.Append($" -addon {(byte)DukeAddonEnum.Base}");
        }
        else
        {
            var dukeAddon = (byte)DukeAddonEnum.Base;

            if (dCamp.DependentAddons is null)
            {
                dukeAddon = (byte)DukeAddonEnum.Base;
            }
            else if (dCamp.DependentAddons.ContainsKey(nameof(DukeAddonEnum.DukeDC)))
            {
                dukeAddon = (byte)DukeAddonEnum.DukeDC;
            }
            else if (dCamp.DependentAddons.ContainsKey(nameof(DukeAddonEnum.DukeNW)))
            {
                dukeAddon = (byte)DukeAddonEnum.DukeNW;
            }
            else if (dCamp.DependentAddons.ContainsKey(nameof(DukeAddonEnum.DukeVaca)))
            {
                dukeAddon = (byte)DukeAddonEnum.DukeVaca;
            }

            _ = sb.Append($" -addon {dukeAddon}");
        }


        if (dCamp.FileName is null)
        {
            return;
        }


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
            _ = sb.Append($@" {AddFileParam}""{dCamp.PathToFile}""");
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

    private void GetWangArgs(StringBuilder sb, WangGame game, IAddon addon)
    {
        if (addon is LooseMapEntity)
        {
            GetLooseMapArgs(sb, game, addon);
            return;
        }


        Guard2.ThrowIfNotType<GenericCampaignEntity>(addon, out var wCamp);

        //TODO downloaded addons support
        if (wCamp.DependentAddons is not null &&
            wCamp.DependentAddons.ContainsKey(nameof(WangAddonEnum.Wanton)))
        {
            _ = sb.Append($" {AddFileParam}WT.GRP");
        }
        else if (wCamp.DependentAddons is not null &&
            wCamp.DependentAddons.ContainsKey(nameof(WangAddonEnum.TwinDragon)))
        {
            _ = sb.Append($" {AddFileParam}TD.GRP");
        }


        if (wCamp.FileName is null)
        {
            return;
        }


        if (wCamp.Type is AddonTypeEnum.TC)
        {
            _ = sb.Append($@" {AddFileParam}""{wCamp.PathToFile}""");
        }
        else if (wCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, wCamp);
        }
        else
        {
            ThrowHelper.ThrowNotSupportedException($"Mod type {wCamp.Type} is not supported");
            return;
        }
    }

    private void GetRedneckArgs(StringBuilder sb, RedneckGame game, IAddon addon)
    {
        if (addon is LooseMapEntity)
        {
            GetLooseMapArgs(sb, game, addon);
            return;
        }


        Guard2.ThrowIfNotType<DukeCampaignEntity>(addon, out var rCamp);

        if (rCamp.DependentAddons is not null &&
            rCamp.DependentAddons.ContainsKey(nameof(RedneckAddonEnum.Route66)))
        {
            _ = sb.Append(" -route66");
            return;
        }

        if (rCamp.SupportedGame.GameEnum is GameEnum.RidesAgain)
        {
            var pathToConfig = Path.Combine(PortInstallFolderPath, ConfigFile);
            AddGamePathsToConfig(game, addon, game.AgainInstallPath!, pathToConfig);
        }


        if (rCamp.FileName is null)
        {
            return;
        }


        if (rCamp.MainCon is not null)
        {
            _ = sb.Append($@" {MainConParam}""{rCamp.MainCon}""");
        }

        if (rCamp.AdditionalCons?.Any() is true)
        {
            foreach (var con in rCamp.AdditionalCons)
            {
                _ = sb.Append($@" {AddConParam}""{con}""");
            }
        }


        if (rCamp.Type is AddonTypeEnum.TC)
        {
            _ = sb.Append($@" {AddFileParam}""{rCamp.PathToFile}""");
        }
        else if (rCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, rCamp);
        }
        else
        {
            ThrowHelper.ThrowNotSupportedException($"Mod type {rCamp.Type} is not supported");
        }
    }

    /// <summary>
    /// Add paths to game and mods folder to the config
    /// </summary>
    private static void AddGamePathsToConfig(BaseGame game, IAddon campaign, string gameInstallFolder, string config)
    {
        var contents = File.ReadAllLines(config);

        StringBuilder sb = new(contents.Length);

        for (var i = 0; i < contents.Length; i++)
        {
            if (contents[i].Equals("[GameSearch.Directories]"))
            {
                _ = sb.AppendLine(contents[i]);

                //game folder
                var path = gameInstallFolder.Replace('\\', '/');
                _ = sb.Append("Path=").AppendLine(path);

                //duke addons folders
                if (game is DukeGame dGame && !campaign.AddonId.Id.Equals(nameof(DukeVersionEnum.Duke3D_WT), StringComparison.OrdinalIgnoreCase))
                {
                    var addonPaths = dGame.AddonsPaths.Values.Distinct();

                    foreach (var folder in addonPaths)
                    {
                        if (folder.Equals(gameInstallFolder))
                        {
                            continue;
                        }

                        path = folder.Replace('\\', '/');
                        _ = sb.Append("Path=").AppendLine(path);
                    }
                }

                do
                {
                    i++;
                }
                while (!string.IsNullOrWhiteSpace(contents[i]));

                _ = sb.AppendLine();
                continue;
            }

            if (contents[i].Equals("[FileSearch.Directories]"))
            {
                _ = sb.AppendLine(contents[i]);

                //mods folder
                var path = game.ModsFolderPath.Replace('\\', '/');
                _ = sb.Append("Path=").AppendLine(path);

                //blood unpacked addons
                if (campaign is BloodCampaignEntity bCamp &&
                    bCamp.IsFolder)
                {
                    path = Path.GetDirectoryName(bCamp.PathToFile)!.Replace('\\', '/');
                    _ = sb.Append("Path=").AppendLine(path);
                }

                do
                {
                    i++;
                }
                while (!string.IsNullOrWhiteSpace(contents[i]));

                _ = sb.AppendLine();
                continue;
            }

            _ = sb.AppendLine(contents[i]);
        }

        var result = sb.ToString();
        File.WriteAllText(config, result);
    }
}
