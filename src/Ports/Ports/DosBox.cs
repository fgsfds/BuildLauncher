using System.Text;
using Addons.Addons;
using Common.All;
using Common.All.Enums;
using Common.All.Enums.Addons;
using Common.All.Enums.Versions;
using Common.Client.Helpers;
using CommunityToolkit.Diagnostics;
using Games.Games;
using SharpCompress.Archives;

namespace Ports.Ports;

/// <summary>
/// DosBox
/// </summary>
public sealed class DosBox : BasePort
{
    /// <inheritdoc/>
    public override PortEnum PortEnum => PortEnum.DosBox;

    /// <inheritdoc/>
    protected override string WinExe => "dosbox.exe";

    /// <inheritdoc/>
    protected override string LinExe => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    public override string Name => "DosBox Staging";

    /// <inheritdoc/>
    public override string ShortName => "DosBox";

    /// <inheritdoc/>
    public override List<GameEnum> SupportedGames =>
        [
        GameEnum.Blood,
        GameEnum.Duke3D,
        GameEnum.Wang,
        //GameEnum.Slave,
        GameEnum.Redneck,
        GameEnum.RidesAgain,
        //GameEnum.NAM,
        //GameEnum.Witchaven,
        //GameEnum.Witchaven2,
        //GameEnum.TekWar
        ];

    /// <inheritdoc/>
    public override List<string> SupportedGamesVersions =>
        [
        nameof(DukeVersionEnum.Duke3D_13D),
        nameof(DukeVersionEnum.Duke3D_Atomic)
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
    public override List<FeatureEnum> SupportedFeatures => [];

    /// <inheritdoc/>
    public override bool IsSkillSelectionAvailable => false;


    /// <inheritdoc/>
    protected override string ConfigFile => string.Empty;

    /// <inheritdoc/>
    protected override string AddDirectoryParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string AddFileParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string AddDefParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string AddConParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string MainDefParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string MainConParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string MainGrpParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string AddGrpParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string SkillParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string AddGameDirParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string AddRffParam => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    protected override string AddSndParam => ThrowHelper.ThrowNotSupportedException<string>();


    /// <inheritdoc/>
    public override void BeforeStart(BaseGame game, BaseAddon campaign)
    {
        MoveSaveFilesToGameFolder(game, campaign);

        RestoreRoute66Files(game);
    }

    /// <inheritdoc/>
    public override void AfterEnd(BaseGame game, BaseAddon campaign)
    {
        MoveSaveFilesFromGameFolder(game, campaign);
    }

    /// <inheritdoc/>
    protected override void GetStartCampaignArgs(StringBuilder sb, BaseGame game, BaseAddon addon)
    {
        _ = sb.Append(@" --noconsole -c ""cycles max"" -c ""core dynamic""");

        if (game is BloodGame bGame)
        {
            GetBloodArgs(sb, bGame, addon);
        }
        else if (game is DukeGame dGame)
        {
            GetDukeArgs(sb, dGame, addon);
        }
        else if (game is RedneckGame rGame)
        {
            GetRedneckArgs(sb, rGame, addon);
        }
        else if (game is WangGame wGame)
        {
            GetWangArgs(sb, wGame);
        }

        _ = sb.Append(" -c \"exit\"");
    }

    private static void GetDukeArgs(StringBuilder sb, DukeGame game, BaseAddon addon)
    {
        _ = sb.Append($@" -c ""mount c \""{game.GameInstallFolder}"""" -c ""c:""");

        if (addon.AddonId.Id.Equals(nameof(DukeAddonEnum.DukeVaca), StringComparison.OrdinalIgnoreCase))
        {
            var pathToAddonFolder = game.AddonsPaths[DukeAddonEnum.DukeVaca];
            _ = sb.Append($@" -c ""mount d \""{pathToAddonFolder}""""");
            _ = sb.Append(@" -c ""VACATION.EXE /gd:\\VACATION.GRP /xd:\\VACATION.CON");
        }
        else if (addon.AddonId.Id.Equals(nameof(DukeAddonEnum.DukeDC), StringComparison.OrdinalIgnoreCase))
        {
            var pathToAddonFolder = game.AddonsPaths[DukeAddonEnum.DukeDC];
            _ = sb.Append($@" -c ""mount d \""{pathToAddonFolder}""""");
            _ = sb.Append(@" -c ""DUKE3D.EXE /gd:\\DUKEDC.GRP /xd:\\DUKEDC.CON");
        }
        else if (addon.AddonId.Id.Equals(nameof(DukeAddonEnum.DukeNW), StringComparison.OrdinalIgnoreCase))
        {
            var pathToAddonFolder = game.AddonsPaths[DukeAddonEnum.DukeNW];
            _ = sb.Append($@" -c ""mount d \""{pathToAddonFolder}""""");
            _ = sb.Append(@" -c ""DUKE3D.EXE /gd:\\NWINTER.GRP /xd:\\NWINTER.CON");
        }
        else if (addon is LooseMapEntity map)
        {
            _ = sb.Append($@" -c ""mount d \""{game.MapsFolderPath}""""");
            _ = sb.Append($@" -c ""DUKE3D.EXE -map d:\\{map.FileName}");
        }
        else
        {
            _ = sb.Append(" -c DUKE3D.EXE");
        }
    }

    private static void GetWangArgs(StringBuilder sb, WangGame game)
    {
        _ = sb.Append($@" -c ""mount c \""{game.GameInstallFolder}"""" -c ""c:""");
        _ = sb.Append(" -c Sw.EXE");
    }

    private static void GetRedneckArgs(StringBuilder sb, RedneckGame game, BaseAddon addon)
    {
        if (addon.AddonId.Id.Equals(nameof(GameEnum.Redneck), StringComparison.OrdinalIgnoreCase))
        {
            _ = sb.Append($@" -c ""mount c \""{game.GameInstallFolder}"""" -c ""c:""");
            _ = sb.Append(" -c RR.EXE");
        }
        else if (addon.AddonId.Id.Equals(nameof(GameEnum.RidesAgain), StringComparison.OrdinalIgnoreCase))
        {
            _ = sb.Append($@" -c ""mount c \""{game.AgainInstallPath}"""" -c ""c:""");
            _ = sb.Append(" -c RA.EXE");
        }
        else if (addon.AddonId.Id.Equals(nameof(RedneckAddonEnum.Route66), StringComparison.OrdinalIgnoreCase))
        {
            _ = sb.Append($@" -c ""mount c \""{game.GameInstallFolder}"""" -c ""c:""");
            _ = sb.Append(" -c ROUTE66.EXE");
        }
    }

    protected override void GetBloodArgs(StringBuilder sb, BloodGame game, BaseAddon addon)
    {
        if (addon.AddonId.Id.Equals(nameof(BloodAddonEnum.BloodCP), StringComparison.OrdinalIgnoreCase))
        {
            _ = sb.Append(@$" -c ""mount c \""{game.GameInstallFolder}"""" -c ""c:""");
            _ = sb.Append(" -c CRYPTIC.EXE");

            return;
        }

        if (addon is BloodCampaignEntity bCamp && bCamp.Type is AddonTypeEnum.TC)
        {
            if (Directory.Exists(ClientProperties.TempFolderPath))
            {
                Directory.Delete(ClientProperties.TempFolderPath, true);
            }

            _ = Directory.CreateDirectory(ClientProperties.TempFolderPath);

            foreach (var filePath in Directory.GetFiles(game.GameInstallFolder))
            {
                string fileName = Path.GetFileName(filePath);

                if (fileName.EndsWith(".DEM", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string destFile = Path.Combine(ClientProperties.TempFolderPath, fileName);
                File.Copy(filePath, destFile, overwrite: true);
            }

            if (addon.IsUnpacked)
            {
                foreach (var filePath in Directory.GetFiles(Path.GetDirectoryName(addon.PathToFile)))
                {
                    string fileName = Path.GetFileName(filePath);
                    string destFile = Path.Combine(ClientProperties.TempFolderPath, fileName);
                    File.Copy(filePath, destFile, overwrite: true);
                }
            }
            else
            {
                using var archive = ArchiveFactory.Open(addon.PathToFile);

                foreach (var file in archive.Entries)
                {
                    var dir = Path.GetDirectoryName(file.Key);

                    if (!string.IsNullOrWhiteSpace(dir))
                    {
                        dir = Path.Combine(ClientProperties.TempFolderPath, dir);

                        if (!Directory.Exists(dir))
                        {
                            _ = Directory.CreateDirectory(dir);
                        }
                    }

                    file.WriteToFile(Path.Combine(ClientProperties.TempFolderPath, file.Key.Replace('\\', Path.DirectorySeparatorChar)), new() { Overwrite = true });
                }
            }

            _ = sb.Append(@$" -c ""mount c \""{ClientProperties.TempFolderPath}"""" -c ""c:""");
            _ = sb.Append(@$" -c ""BLOOD.EXE -ini {bCamp.INI} {(bCamp.RFF is null ? string.Empty : $"-RFF {bCamp.RFF}")} {(bCamp.SND is null ? string.Empty : $"-snd {bCamp.SND}")}""");

            return;
        }

        if (addon is LooseMapEntity map)
        {
            _ = sb.Append(@$" -c ""mount c \""{game.GameInstallFolder}"""" -c ""c:""");
            _ = sb.Append(@$" -c ""mount d \""{game.MapsFolderPath}""""");
            _ = sb.Append(@$" -c ""BLOOD.EXE -map d:\\{map.FileName}""");

            return;
        }

        _ = sb.Append(@$" -c ""mount c \""{game.GameInstallFolder}"""" -c ""c:""");
        _ = sb.Append(" -c BLOOD.EXE");
    }

    /// <inheritdoc/>
    protected override void GetAutoloadModsArgs(StringBuilder sb, BaseGame _, BaseAddon addon, IEnumerable<KeyValuePair<AddonId, BaseAddon>> mods) { }

    /// <inheritdoc/>
    protected override void GetSkipIntroParameter(StringBuilder sb) { }

    /// <inheritdoc/>
    protected override void GetSkipStartupParameter(StringBuilder sb) { }
}
