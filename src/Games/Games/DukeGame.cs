using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using CommunityToolkit.Diagnostics;

namespace Games.Games;

public sealed class DukeGame : BaseGame
{
    /// <inheritdoc/>
    public override GameEnum GameEnum => GameEnum.Duke3D;

    /// <inheritdoc/>
    public override string FullName => "Duke Nukem 3D";

    /// <inheritdoc/>
    public override string ShortName => "Duke3D";

    /// <summary>
    /// Path to Duke64 rom file
    /// </summary>
    public required string? Duke64RomPath { get; set; }

    /// <summary>
    /// Path to World Tour folder
    /// </summary>
    public required string? DukeWTInstallPath { get; set; }

    /// <inheritdoc/>
    public override List<string> RequiredFiles => ["DUKE3D.GRP"];

    /// <summary>
    /// Is Duke it Out in DC installed
    /// </summary>
    public bool IsDukeDCInstalled => GetDukeAddon(DukeAddonEnum.DukeDC);

    /// <summary>
    /// Is Nuclear Winter installed
    /// </summary>
    public bool IsNuclearWinterInstalled => GetDukeAddon(DukeAddonEnum.DukeNW);

    /// <summary>
    /// Is Caribbean installed
    /// </summary>
    public bool IsCaribbeanInstalled => GetDukeAddon(DukeAddonEnum.DukeVaca);

    /// <summary>
    /// Is World Tour installed
    /// </summary>
    public bool IsWorldTourInstalled => IsInstalled(["EPISODE5BOSS.CON", "FIREFLYTROOPER.CON", "FLAMETHROWER.CON"], DukeWTInstallPath);

    /// <summary>
    /// Is Duke 64 installed
    /// </summary>
    public bool IsDuke64Installed => File.Exists(Duke64RomPath);

    /// <summary>
    /// List of paths to Duke's addons folders
    /// </summary>
    public Dictionary<DukeAddonEnum, string> AddonsPaths { get; set; } = [];


    /// <summary>
    /// Find Duke's addon files
    /// </summary>
    /// <param name="addon">Duke addon</param>
    private bool GetDukeAddon(DukeAddonEnum addon)
    {
        if (GameInstallFolder is null)
        {
            return false;
        }

        var file = addon switch
        {
            DukeAddonEnum.DukeDC => "DUKEDC.GRP",
            DukeAddonEnum.DukeNW => "NWINTER.GRP",
            DukeAddonEnum.DukeVaca => "VACATION.GRP",
            DukeAddonEnum.Base => ThrowHelper.ThrowArgumentOutOfRangeException<string>(),
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<string>(),
        };

        //root
        var path = Path.Combine(GameInstallFolder, file);
        if (File.Exists(path))
        {
            AddonsPaths.AddOrReplace(addon, Path.GetDirectoryName(path));
            return true;
        }

        //zoom
        path = Path.Combine(GameInstallFolder, "AddOns", file);
        if (File.Exists(path))
        {
            AddonsPaths.AddOrReplace(addon, Path.GetDirectoryName(path));
            return true;
        }

        //megaton
        path = Path.Combine(GameInstallFolder, "addons", "dc", file);
        if (File.Exists(path))
        {
            AddonsPaths.AddOrReplace(addon, Path.GetDirectoryName(path));
            return true;
        }

        //megaton
        path = Path.Combine(GameInstallFolder, "addons", "nw", file);
        if (File.Exists(path))
        {
            AddonsPaths.AddOrReplace(addon, Path.GetDirectoryName(path));
            return true;
        }

        //megaton
        path = Path.Combine(GameInstallFolder, "addons", "vacation", file);
        if (File.Exists(path))
        {
            AddonsPaths.AddOrReplace(addon, Path.GetDirectoryName(path));
            return true;
        }

        _ = AddonsPaths.Remove(addon);
        return false;
    }
}
