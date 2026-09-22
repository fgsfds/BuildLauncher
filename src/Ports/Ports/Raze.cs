using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Addons.Addons;
using Core.All.Enums;
using Core.All.Enums.Versions;
using Games.Games;
using Ports.Builders;
using Ports.Helpers;

namespace Ports.Ports;

/// <summary>
///     Raze port.
/// </summary>
public sealed class Raze : BasePort
{
    /// <inheritdoc />
    public override PortEnum PortEnum => PortEnum.Raze;

    /// <inheritdoc />
    protected override string WinExe => "raze.exe";

    /// <inheritdoc />
    protected override string LinExe => "raze";

    /// <inheritdoc />
    public override string Name => "Raze";

    /// <inheritdoc />
    public override ImmutableHashSet<GameEnum> SupportedGames { get; } =
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

    /// <inheritdoc />
    public override ImmutableHashSet<string> SupportedGamesVersions { get; } =
    [
        nameof(DukeVersionEnum.Duke3D_13D),
        nameof(DukeVersionEnum.Duke3D_Atomic),
        nameof(DukeVersionEnum.Duke3D_WT)
    ];

    /// <inheritdoc />
    public override string? InstalledVersion => File.Exists(PortExeFilePath)
        ? FileVersionInfo.GetVersionInfo(PortExeFilePath).FileVersion
        : null;

    /// <inheritdoc />
    protected override string ConfigFile => "raze_portable.ini";

    /// <inheritdoc />
    public override PortCmdArguments CmdArguments => new()
    {
        AddDirectory = "-file ",
        MainGrp = "-file ",
        AddGrp = "-file ",
        AddFile = "-file ",
        AddDef = "-adddef ",
        AddCon = "-addcon ",
        MainDef = "-def ",
        MainCon = "-con ",
        SkillLevel = null,
        AddGameDir = "-file ",
        AddRff = "-file ",
        AddSnd = "-file ",
        SkipIntro = " -quick",
        SkipStartup = " -nosetup",
        SkipSteam = null
    };

    /// <inheritdoc />
    public override ImmutableHashSet<FeatureEnum> SupportedFeatures { get; } =
    [
        FeatureEnum.TROR,
        FeatureEnum.Hightile,
        FeatureEnum.Models,
        FeatureEnum.Sloped_Sprites,
        FeatureEnum.Wall_Rotate_Cstat,
        FeatureEnum.SndInfo,
        FeatureEnum.TileFromTexture
    ];

    /// <inheritdoc />
    public override void BeforeStart(BaseGame game, BaseAddon campaign)
    {
        try
        {
            var config = Path.Combine(InstallFolderPath, ConfigFile);

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

                var configDir = Path.GetDirectoryName(config);

                if (configDir is not null && !Directory.Exists(configDir))
                {
                    _ = Directory.CreateDirectory(configDir);
                }

                File.WriteAllText(config, DefaultConfig);
            }

            ArgumentNullException.ThrowIfNull(game.GameInstallFolder);
            AddGamePathsToConfig(game, campaign, game.GameInstallFolder, config);

            FilesHelpers.RestoreRoute66Files(game);

            FilesHelpers.RestoreWtFiles(game);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error in Raze.BeforeStart: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override void AfterEnd(BaseGame game, BaseAddon campaign)
    {
        //nothing to do
    }

    /// <inheritdoc />
    protected override void CustomModifyArgs(CmdParametersBuilder sb, BaseGame game, BaseAddon addon)
    {
        _ = sb.Append($@" -savedir ""{GetPathToAddonSavedGamesFolder(game.ShortName, addon.AddonId.Id)}""");
    }

    /// <summary>
    ///     Adds game and file search directory paths to the Raze config file.
    /// </summary>
    /// <param name="game">Game instance.</param>
    /// <param name="campaign">Campaign or addon.</param>
    /// <param name="gameInstallFolder">Path to the game install folder.</param>
    /// <param name="config">Path to the config file.</param>
    internal static void AddGamePathsToConfig(BaseGame game, BaseAddon campaign, string gameInstallFolder, string config)
    {
        try
        {
            var contents = File.ReadAllLines(config);

            StringBuilder sb = new((int)(contents.Sum(x => x.Length) * 1.2));

            for (var i = 0; i < contents.Length; i++)
            {
                if (contents[i].Equals("[GameSearch.Directories]"))
                {
                    _ = sb.AppendLine(contents[i]);

                    //game folder
                    var path = gameInstallFolder.Replace('\\', '/');
                    _ = sb.Append("Path=").AppendLine(path);

                    //additional folders
                    if (!campaign.AddonId.Id.Equals(nameof(DukeVersionEnum.Duke3D_WT), StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var folder in game.AdditionalFolders)
                        {
                            if (folder.Equals(gameInstallFolder, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            path = folder.Replace('\\', '/');
                            _ = sb.Append("Path=").AppendLine(path);
                        }
                    }

                    while (i < contents.Length && !string.IsNullOrWhiteSpace(contents[i]))
                    {
                        i++;
                    }

                    _ = sb.AppendLine();

                    continue;
                }

                if (contents[i].Equals("[FileSearch.Directories]"))
                {
                    _ = sb.AppendLine(contents[i]);

                    //mods folder
                    var path = game.ModsFolderPath.Replace('\\', '/');
                    _ = sb.Append("Path=").AppendLine(path);

                    //unpacked addon folder
                    if (campaign is BloodCampaign bCamp &&
                        bCamp.FileInfo is not null &&
                        bCamp.FileInfo.Value.IsFolder)
                    {
                        path = bCamp.FileInfo.Value.PathToFolder.Replace('\\', '/');
                        _ = sb.Append("Path=").AppendLine(path);
                    }

                    //zipped addon folder (parent directory), unless it's the game install folder
                    if (campaign.FileInfo is not null && !campaign.FileInfo.Value.IsFolder)
                    {
                        var addonFolder = campaign.FileInfo.Value.PathToFolder.Replace('\\', '/');

                        if (!addonFolder.Equals(gameInstallFolder.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase))
                        {
                            _ = sb.Append("Path=").AppendLine(addonFolder);
                        }
                    }

                    while (i < contents.Length && !string.IsNullOrWhiteSpace(contents[i]))
                    {
                        i++;
                    }

                    _ = sb.AppendLine();

                    continue;
                }

                _ = sb.AppendLine(contents[i]);
            }

            var result = sb.ToString();
            File.WriteAllText(config, result);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error updating Raze config: {ex.Message}");
        }
    }
}
