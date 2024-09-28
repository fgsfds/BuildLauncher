using Common;
using Common.Enums;
using Common.Enums.Addons;
using Common.Enums.Versions;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
using System.Diagnostics;
using System.Text;

namespace Ports.Ports;

/// <summary>
/// Raze port
/// </summary>
public sealed class Raze : BasePort
{
    /// <inheritdoc/>
    public override PortEnum PortEnum => PortEnum.Raze;

    /// <inheritdoc/>
    public override string Exe => "raze.exe";

    /// <inheritdoc/>
    public override string Name => "Raze";

    /// <inheritdoc/>
    public override List<GameEnum> SupportedGames =>
        [
        GameEnum.Blood,
        GameEnum.Duke3D,
        GameEnum.ShadowWarrior,
        GameEnum.Exhumed,
        GameEnum.Redneck,
        GameEnum.RidesAgain,
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
    public override string? InstalledVersion =>
        File.Exists(PortExeFolderPath)
        ? FileVersionInfo.GetVersionInfo(PortExeFolderPath).FileVersion
        : null;


    /// <inheritdoc/>
    protected override string ConfigFile => "raze_portable.ini";

    /// <inheritdoc/>
    protected override string AddDirectoryParam => "-file ";

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
    protected override string AddGameDirParam => "-file ";

    /// <inheritdoc/>
    protected override string AddRffParam => "-file ";

    /// <inheritdoc/>
    protected override string AddSndParam => "-file ";

    /// <inheritdoc/>
    protected override string AddGrpParam => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override string SkillParam => throw new NotImplementedException();

    /// <inheritdoc/>
    public override List<FeatureEnum> SupportedFeatures =>
        [
        FeatureEnum.TROR,
        FeatureEnum.Hightile,
        FeatureEnum.Models,
        FeatureEnum.Sloped_Sprites,
        FeatureEnum.Wall_Rotate_Cstat
        ];


    public Raze() : base()
    {
        try
        {
            MoveOldSavesFolder();
        }
        catch
        {
            Directory.Delete(Path.Combine(PortExecutableFolderPath, "Save"));
        }
    }


    [Obsolete]
    private void MoveOldSavesFolder()
    {
        var oldSavesFolder = Path.Combine(PortExecutableFolderPath, "Save");

        if (Directory.Exists(oldSavesFolder))
        {
            foreach (var entry in Directory.GetDirectories(oldSavesFolder))
            {
                var folderName = Path.GetFileName(entry);

                string gameFolder;

                if (folderName.StartsWith("blood", StringComparison.InvariantCultureIgnoreCase))
                {
                    gameFolder = "Blood";
                }
                else if (folderName.StartsWith("duke", StringComparison.InvariantCultureIgnoreCase))
                {
                    gameFolder = "Duke3D";
                }
                else if (folderName.StartsWith("slave", StringComparison.InvariantCultureIgnoreCase))
                {
                    gameFolder = "Slave";
                }
                else if (folderName.StartsWith("fury", StringComparison.InvariantCultureIgnoreCase))
                {
                    gameFolder = "Fury";
                }
                else if (folderName.StartsWith("wang", StringComparison.InvariantCultureIgnoreCase))
                {
                    gameFolder = "Wang";
                }
                else if (folderName.StartsWith("redneck", StringComparison.InvariantCultureIgnoreCase))
                {
                    gameFolder = "Redneck";
                }
                else if (folderName.StartsWith("ridesagain", StringComparison.InvariantCultureIgnoreCase))
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
    protected override void BeforeStart(IGame game, IAddon campaign)
    {
        var config = Path.Combine(PortExecutableFolderPath, ConfigFile);

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

        FixRoute66Files(game, campaign);
    }

    /// <inheritdoc/>
    public override void AfterStart(IGame game, IAddon campaign)
    {
        //nothing to do
    }

    /// <inheritdoc/>
    protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon)
    {
        _ = sb.Append($@" -savedir ""{GetPathToAddonSavedGamesFolder(game.ShortName, addon.Id)}""");

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
            ThrowHelper.NotImplementedException($"Mod type {addon} for game {game} is not supported");
        }
    }

    private void GetDukeArgs(StringBuilder sb, DukeGame game, IAddon addon)
    {
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

        if (dCamp.SupportedGame.GameVersion is not null &&
            dCamp.SupportedGame.GameVersion.Equals(nameof(DukeVersionEnum.Duke3D_WT), StringComparison.InvariantCultureIgnoreCase))
        {
            var config = Path.Combine(PortExecutableFolderPath, ConfigFile);

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

        if (dCamp.AdditionalCons?.Count > 0)
        {
            foreach (var con in dCamp.AdditionalCons)
            {
                _ = sb.Append($@" {AddConParam}""{con}""");
            }
        }


        if (dCamp.Type is AddonTypeEnum.TC)
        {
            _ = sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, dCamp.FileName)}""");
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

    private void GetWangArgs(StringBuilder sb, WangGame game, IAddon addon)
    {
        if (addon is LooseMap)
        {
            GetLooseMapArgs(sb, game, addon);
            return;
        }

        if (addon is not WangCampaign wCamp)
        {
            ThrowHelper.ArgumentException(nameof(addon));
            return;
        }

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
            _ = sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, wCamp.FileName)}""");
        }
        else if (wCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, game, wCamp);
        }
        else
        {
            ThrowHelper.NotImplementedException($"Mod type {wCamp.Type} is not supported");
            return;
        }
    }

    private void GetRedneckArgs(StringBuilder sb, RedneckGame game, IAddon addon)
    {
        if (addon is LooseMap)
        {
            GetLooseMapArgs(sb, game, addon);
            return;
        }

        if (addon is not RedneckCampaign rCamp)
        {
            ThrowHelper.ArgumentException(nameof(addon));
            return;
        }

        if (rCamp.DependentAddons is not null &&
            rCamp.DependentAddons.ContainsKey(nameof(RedneckAddonEnum.Route66)))
        {
            _ = sb.Append(" -route66");
            return;
        }

        if (rCamp.Id.Equals(nameof(GameEnum.RidesAgain), StringComparison.OrdinalIgnoreCase))
        {
            var pathToConfig = Path.Combine(PortExecutableFolderPath, ConfigFile);
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

        if (rCamp.AdditionalCons?.Count > 0)
        {
            foreach (var con in rCamp.AdditionalCons)
            {
                _ = sb.Append($@" {AddConParam}""{con}""");
            }
        }


        if (rCamp.Type is AddonTypeEnum.TC)
        {
            _ = sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, rCamp.FileName)}""");
        }
        else if (rCamp.Type is AddonTypeEnum.Map)
        {
            GetMapArgs(sb, game, rCamp);
        }
        else
        {
            ThrowHelper.NotImplementedException($"Mod type {rCamp.Type} is not supported");
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
        }
    }

    /// <summary>
    /// Remove route 66 art files overrides used for RedNukem
    /// </summary>
    private void FixRoute66Files(IGame game, IAddon _)
    {
        game.GameInstallFolder.ThrowIfNull();

        if (game is RedneckGame)
        {
            var tilesA2 = Path.Combine(game.GameInstallFolder, "TILES024.ART");
            var tilesB2 = Path.Combine(game.GameInstallFolder, "TILES025.ART");
            var turdMovAnm2 = Path.Combine(game.GameInstallFolder, "TURDMOV.ANM");
            var turdMovVoc2 = Path.Combine(game.GameInstallFolder, "TURDMOV.VOC");
            var endMovAnm2 = Path.Combine(game.GameInstallFolder, "RR_OUTRO.ANM");
            var endMovVoc2 = Path.Combine(game.GameInstallFolder, "LN_FINAL.VOC");

            if (File.Exists(tilesA2))
            {
                File.Delete(tilesA2);
            }

            if (File.Exists(tilesB2))
            {
                File.Delete(tilesB2);
            }

            if (File.Exists(turdMovAnm2))
            {
                File.Delete(turdMovAnm2);
            }

            if (File.Exists(turdMovVoc2))
            {
                File.Delete(turdMovVoc2);
            }

            if (File.Exists(endMovAnm2))
            {
                File.Delete(endMovAnm2);
            }

            if (File.Exists(endMovVoc2))
            {
                File.Delete(endMovVoc2);
            }
        }
    }

    /// <summary>
    /// Add paths to game and mods folder to the config
    /// </summary>
    private static void AddGamePathsToConfig(IGame game, IAddon campaign, string gameInstallFolder, string config)
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
                if (game is DukeGame dGame)
                {
                    foreach (var folder in dGame.AddonsPaths)
                    {
                        path = folder.Value.Replace('\\', '/');
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
                if (campaign is BloodCampaign bCamp &&
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
