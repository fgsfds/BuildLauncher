using System.Collections.Immutable;
using System.Diagnostics;
using Addons.Addons;
using Core.All.Enums;
using Core.All.Enums.Versions;
using Core.Client.Helpers;
using Games.Games;
using Ports.Builders;
using Ports.Helpers;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;

namespace Ports.Ports.EDuke32;

/// <summary>
///     EDuke32 port.
/// </summary>
public class EDuke32 : BasePort
{
    /// <inheritdoc />
    public override PortEnum PortEnum => PortEnum.EDuke32;

    /// <inheritdoc />
    protected override string WinExe => "eduke32.exe";

    /// <inheritdoc />
    protected override string LinExe => throw new NotSupportedException();

    /// <inheritdoc />
    public override string Name => "EDuke32";

    /// <inheritdoc />
    protected override string ConfigFile => "eduke32.cfg";

    /// <inheritdoc />
    public override PortCmdArguments CmdArguments => new()
    {
        AddDirectory = "-j ",
        MainGrp = "-gamegrp ",
        AddGrp = "-grp ",
        AddFile = "-g ",
        AddDef = "-mh ",
        AddCon = "-mx ",
        MainDef = "-h ",
        MainCon = "-x ",
        SkillLevel = "-s",
        AddGameDir = "-game_dir ",
        SkipIntro = " -quick",
        SkipStartup = " -nosetup",
        SkipSteam = " -usecwd",
        AddRff = null,
        AddSnd = null
    };

    /// <inheritdoc />
    public override ImmutableHashSet<GameEnum> SupportedGames { get; } =
    [
        GameEnum.Duke3D,
        GameEnum.NAM,
        GameEnum.WW2GI,
        GameEnum.Fury
    ];

    /// <inheritdoc />
    public override ImmutableHashSet<string> SupportedGamesVersions { get; } =
    [
        nameof(DukeVersionEnum.Duke3D_13D),
        nameof(DukeVersionEnum.Duke3D_Atomic),
        nameof(DukeVersionEnum.Duke3D_WT)
    ];

    /// <inheritdoc />
    public override ImmutableHashSet<FeatureEnum> SupportedFeatures { get; } =
    [
        FeatureEnum.EDuke32_CON,
        FeatureEnum.Dynamic_Lighting,
        FeatureEnum.Hightile,
        FeatureEnum.Models,
        FeatureEnum.Sloped_Sprites,
        FeatureEnum.TROR,
        FeatureEnum.Wall_Rotate_Cstat,
        FeatureEnum.TileFromTexture
    ];


    /// <summary>
    ///     Creates the World Tour stopgap folder with required files if it does not exist.
    /// </summary>
    private void CreateWTStopgapFolder()
    {
        if (PortEnum is not PortEnum.EDuke32)
        {
            return;
        }

        var stopgapFolder = Path.Combine(InstallFolderPath, ClientConsts.WTStopgap);

        if (Directory.Exists(stopgapFolder))
        {
            return;
        }

        using var stream = typeof(EDuke32).Assembly.GetManifestResourceStream("Ports.Assets.WTStopgap.zip");

        ArgumentNullException.ThrowIfNull(stream);

        Ensure.DirectoryExists(stopgapFolder);

        using var archive = ZipArchive.Open(stream);
        archive.WriteToDirectory(stopgapFolder);
    }


    /// <inheritdoc />
    public override void BeforeStart(BaseGame game, BaseAddon campaign)
    {
        CreateWTStopgapFolder();

        SaveFilesHelper.MoveSaveFilesFromStorage(
            GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id),
            GetGameSaveFilesFolder(game, campaign));

        try
        {
            FixConfig();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error in EDuke32.FixConfig: {ex.Message}");
        }

        try
        {
            FixWtFiles(game, campaign);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error in EDuke32.FixWtFiles: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override void AfterEnd(BaseGame game, BaseAddon campaign)
    {
        SaveFilesHelper.MoveSaveFilesToStorage(
            GetPathToAddonSavedGamesFolder(game.ShortName, campaign.AddonId.Id),
            GetGameSaveFilesFolder(game, campaign));
    }

    /// <inheritdoc />
    protected override void CustomModifyArgs(CmdParametersBuilder sb, BaseGame game, BaseAddon addon)
    {
        _ = sb.AppendSkipSteam();
        _ = sb.Append(" -cachesize 262144"); //set cache to 256MiB
    }


    /// <summary>
    ///     Removes leftover entries from the config file.
    /// </summary>
    protected void FixConfig()
    {
        var config = Path.Combine(InstallFolderPath, ConfigFile);

        if (!File.Exists(config))
        {
            return;
        }

        try
        {
            var contents = File.ReadAllLines(config);

            for (var i = 0; i < contents.Length; i++)
            {
                if (contents[i].StartsWith("SelectedGRP", StringComparison.OrdinalIgnoreCase))
                {
                    contents[i] = @"SelectedGRP = """"";
                }
                else if (contents[i].StartsWith("LastINI", StringComparison.OrdinalIgnoreCase))
                {
                    contents[i] = string.Empty;
                }
                else if (contents[i].StartsWith("ModDir", StringComparison.OrdinalIgnoreCase))
                {
                    contents[i] = string.Empty;
                }
            }

            File.WriteAllLines(config, contents);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error fixing EDuke32 config: {ex.Message}");
        }
    }

    /// <summary>
    ///     Renames or restores Duke WT's ART files depending on the campaign.
    /// </summary>
    protected void FixWtFiles(BaseGame game, BaseAddon campaign)
    {
        if (game is not DukeGame)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(game.GameInstallFolder);

        var art1 = Path.Combine(game.GameInstallFolder, "TILES009.ART");
        var art1r = Path.Combine(game.GameInstallFolder, "TILES009._ART");

        var art2 = Path.Combine(game.GameInstallFolder, "TILES020.ART");
        var art2r = Path.Combine(game.GameInstallFolder, "TILES020._ART");

        var art3 = Path.Combine(game.GameInstallFolder, "TILES021.ART");
        var art3r = Path.Combine(game.GameInstallFolder, "TILES021._ART");

        var art4 = Path.Combine(game.GameInstallFolder, "TILES022.ART");
        var art4r = Path.Combine(game.GameInstallFolder, "TILES022._ART");


        if (campaign.AddonId.Id.Equals(nameof(DukeVersionEnum.Duke3D_WT), StringComparison.OrdinalIgnoreCase))
        {
            FilesHelpers.RestoreWtFiles(game);
        }
        else
        {
            SafeMove(art1, art1r);
            SafeMove(art2, art2r);
            SafeMove(art3, art3r);
            SafeMove(art4, art4r);
        }
    }

    private static void SafeMove(string source, string destination)
    {
        try
        {
            if (File.Exists(source))
            {
                File.Move(source, destination, true);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error moving file '{source}' to '{destination}': {ex.Message}");
        }
    }

    /// <inheritdoc />
    protected override string GetGameSaveFilesFolder(BaseGame game, BaseAddon campaign)
    {
        if (campaign.FileInfo is not null && campaign.FileInfo.Value.IsFolder)
        {
            return campaign.FileInfo.Value.PathToFolder;
        }

        return InstallFolderPath;
    }
}
