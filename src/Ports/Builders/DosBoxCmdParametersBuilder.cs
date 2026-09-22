using Addons.Addons;
using Core.All.Enums;
using Core.All.Enums.Addons;
using Core.Client.Helpers;
using Games.Games;
using Ports.Ports;
using SharpCompress.Archives;

namespace Ports.Builders;

/// <summary>
///     Command parameters builder for the DosBox Staging port.
/// </summary>
public sealed class DosBoxCmdParametersBuilder : CmdParametersBuilder
{
    private const string BloodExe = "BLOOD.EXE";
    private const string CrypticExe = "CRYPTIC.EXE";
    private const string DukeExe = "DUKE3D.EXE";
    private const string RedneckExe = "RR.EXE";
    private const string RidesAgainExe = "RA.EXE";
    private const string Route66Exe = "ROUTE66.EXE";
    private const string ShadowWarriorExe = "Sw.EXE";
    private const string VacationExe = "VACATION.EXE";

    /// <summary>
    ///     Initializes a new instance of the <see cref="DosBoxCmdParametersBuilder" /> class.
    /// </summary>
    public DosBoxCmdParametersBuilder(BaseGame game, BaseAddon addon, BasePort port)
        : base(game, addon, port)
    {
    }

    /// <inheritdoc />
    public override CmdParametersBuilder AppendLooseMapArgs()
    {
        var exe = Game switch
        {
            BloodGame => BloodExe,
            DukeGame => DukeExe,
            WangGame => ShadowWarriorExe,
            RedneckGame => RedneckExe,
            _ => null
        };

        if (exe is null)
        {
            throw new InvalidOperationException();
        }

        _ = Append(@$" -c ""mount c \""{Game.GameInstallFolder}"""" -c ""c:""");
        _ = Append(@$" -c ""mount d \""{Game.MapsFolderPath}""""");
        _ = Append(@$" -c ""{exe} -map d:\\{Addon.FileInfo.Value.FileName}""");

        return this;
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendDukeArgs(DukeGame game, BaseAddon addon)
    {
        _ = Append($@" -c ""mount c \""{game.GameInstallFolder}"""" -c ""c:""");

        if (addon.AddonId.Id.Equals(nameof(DukeAddonEnum.DukeVaca), StringComparison.OrdinalIgnoreCase))
        {
            var pathToAddonFolder = game.AddonsFolders.TryGetValue(DukeAddonEnum.DukeVaca, out var vacaFolder) ? vacaFolder : game.GameInstallFolder;
            _ = Append($@" -c ""mount d \""{pathToAddonFolder}""""");
            _ = Append($@" -c ""{VacationExe} /gd:\\VACATION.GRP /xd:\\VACATION.CON""");
        }
        else if (addon.AddonId.Id.Equals(nameof(DukeAddonEnum.DukeDC), StringComparison.OrdinalIgnoreCase))
        {
            var pathToAddonFolder = game.AddonsFolders.TryGetValue(DukeAddonEnum.DukeDC, out var dcFolder) ? dcFolder : game.GameInstallFolder;
            _ = Append($@" -c ""mount d \""{pathToAddonFolder}""""");
            _ = Append($@" -c ""{DukeExe} /gd:\\DUKEDC.GRP /xd:\\DUKEDC.CON""");
        }
        else if (addon.AddonId.Id.Equals(nameof(DukeAddonEnum.DukeNW), StringComparison.OrdinalIgnoreCase))
        {
            var pathToAddonFolder = game.AddonsFolders.TryGetValue(DukeAddonEnum.DukeNW, out var nwFolder) ? nwFolder : game.GameInstallFolder;
            _ = Append($@" -c ""mount d \""{pathToAddonFolder}""""");
            _ = Append($@" -c ""{DukeExe} /gd:\\NWINTER.GRP /xd:\\NWINTER.CON""");
        }
        else if (addon is LooseMap map)
        {
            if (map.FileInfo is null)
            {
                throw new InvalidOperationException("Map file info is required for DosBox loose map args");
            }

            _ = Append($@" -c ""mount d \""{game.MapsFolderPath}""""");
            _ = Append($@" -c ""{DukeExe} -map d:\\{map.FileInfo.Value.FileName}""");
        }
        else
        {
            _ = Append($" -c {DukeExe}");
        }

        return this;
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendWangArgs(WangGame game, BaseAddon addon)
    {
        _ = Append($@" -c ""mount c \""{game.GameInstallFolder}"""" -c ""c:""");
        _ = Append($" -c {ShadowWarriorExe}");

        return this;
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendRedneckArgs(RedneckGame game, BaseAddon addon)
    {
        if (addon.AddonId.Id.Equals(nameof(GameEnum.Redneck), StringComparison.OrdinalIgnoreCase))
        {
            _ = Append($@" -c ""mount c \""{game.GameInstallFolder}"""" -c ""c:""");
            _ = Append($" -c {RedneckExe}");
        }
        else if (addon.AddonId.Id.Equals(nameof(GameEnum.RidesAgain), StringComparison.OrdinalIgnoreCase))
        {
            _ = Append($@" -c ""mount c \""{game.AgainInstallPath}"""" -c ""c:""");
            _ = Append($" -c {RidesAgainExe}");
        }
        else if (addon.AddonId.Id.Equals(nameof(RedneckAddonEnum.Route66), StringComparison.OrdinalIgnoreCase))
        {
            _ = Append($@" -c ""mount c \""{game.GameInstallFolder}"""" -c ""c:""");
            _ = Append($" -c {Route66Exe}");
        }

        return this;
    }

    /// <inheritdoc />
    protected override CmdParametersBuilder AppendBloodArgs(BloodGame game, BloodCampaign addon)
    {
        ArgumentNullException.ThrowIfNull(game.GameInstallFolder);

        if (addon.AddonId.Id.Equals(nameof(BloodAddonEnum.BloodCP), StringComparison.OrdinalIgnoreCase))
        {
            _ = Append(@$" -c ""mount c \""{game.GameInstallFolder}"""" -c ""c:""");
            _ = Append($" -c {CrypticExe}");

            return this;
        }

        if (addon.Type is AddonTypeEnum.TC && addon.FileInfo is not null)
        {
            var addonFileInfo = addon.FileInfo.Value;

            if (Directory.Exists(ClientProperties.TempFolderPath))
            {
                Directory.Delete(ClientProperties.TempFolderPath, true);
            }

            _ = Directory.CreateDirectory(ClientProperties.TempFolderPath);

            foreach (var filePath in Directory.GetFiles(game.GameInstallFolder))
            {
                var fileName = Path.GetFileName(filePath);

                if (fileName.EndsWith(".DEM", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var destFile = Path.Combine(ClientProperties.TempFolderPath, fileName);
                File.Copy(filePath, destFile, overwrite: true);
            }

            if (addonFileInfo.IsFolder)
            {
                foreach (var filePath in Directory.GetFiles(addonFileInfo.PathToFolder))
                {
                    var fileName = Path.GetFileName(filePath);
                    var destFile = Path.Combine(ClientProperties.TempFolderPath, fileName);
                    File.Copy(filePath, destFile, overwrite: true);
                }
            }
            else
            {
                Ensure.DirectoryExists(ClientProperties.TempFolderPath);

                using var archive = ArchiveFactory.Open(addonFileInfo.PathToFile);
                archive.WriteToDirectory(ClientProperties.TempFolderPath);
            }

            _ = Append(@$" -c ""mount c \""{ClientProperties.TempFolderPath}"""" -c ""c:""");
            _ = Append(@$" -c ""{BloodExe} -ini {addon.INI} {(addon.RFF is null ? string.Empty : $"-RFF {addon.RFF}")} {(addon.SND is null ? string.Empty : $"-snd {addon.SND}")}""");

            return this;
        }

        _ = Append(@$" -c ""mount c \""{game.GameInstallFolder}"""" -c ""c:""");
        _ = Append($" -c {BloodExe}");

        return this;
    }
}
