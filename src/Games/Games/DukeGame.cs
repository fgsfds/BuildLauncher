using Core.All.Enums;
using Core.All.Enums.Addons;
using Games.Helpers;
using Games.Skills;

namespace Games.Games;

/// <summary>
///     Represents the game Duke Nukem 3D and its associated addon detection.
/// </summary>
public sealed class DukeGame : BaseGame
{
    /// <summary>
    ///     Detector for Duke Nukem 3D addon installations.
    /// </summary>
    private readonly DukeAddonDetector _addonDetector = new();

    /// <inheritdoc />
    public override GameEnum GameEnum => GameEnum.Duke3D;

    /// <inheritdoc />
    public override string FullName => "Duke Nukem 3D";

    /// <inheritdoc />
    public override string ShortName => "Duke3D";

    /// <summary>
    ///     Path to Duke64 rom file.
    /// </summary>
    public required string? Duke64RomPath { get; set; }

    /// <summary>
    ///     Path to Duke Zero Hour rom file.
    /// </summary>
    public required string? DukeZHRomPath { get; set; }

    /// <summary>
    ///     Path to World Tour folder.
    /// </summary>
    public required string? DukeWTInstallPath { get; set; }

    /// <inheritdoc />
    public override List<string> RequiredFiles => ["DUKE3D.GRP"];

    /// <summary>
    ///     Is Duke it Out in DC installed.
    /// </summary>
    public bool IsDukeDCInstalled => _addonDetector.TryFindAddon(DukeAddonEnum.DukeDC, GameInstallFolder);

    /// <summary>
    ///     Is Nuclear Winter installed.
    /// </summary>
    public bool IsNuclearWinterInstalled => _addonDetector.TryFindAddon(DukeAddonEnum.DukeNW, GameInstallFolder);

    /// <summary>
    ///     Is Caribbean installed.
    /// </summary>
    public bool IsCaribbeanInstalled => _addonDetector.TryFindAddon(DukeAddonEnum.DukeVaca, GameInstallFolder);

    /// <summary>
    ///     Is World Tour installed.
    /// </summary>
    public bool IsWorldTourInstalled => IsInstalled(["EPISODE5BOSS.CON", "FIREFLYTROOPER.CON", "FLAMETHROWER.CON"], DukeWTInstallPath);

    /// <summary>
    ///     Is Duke 64 installed.
    /// </summary>
    public bool IsDuke64Installed => File.Exists(Duke64RomPath);

    /// <summary>
    ///     Is Duke ZH installed.
    /// </summary>
    public bool IsDukeZHInstalled => File.Exists(DukeZHRomPath);

    /// <summary>
    ///     List of paths to Duke's addons folders.
    /// </summary>
    public Dictionary<DukeAddonEnum, string> AddonsPaths
    {
        get => _addonDetector.AddonsPaths;
        init
        {
            _addonDetector.AddonsPaths.Clear();

            foreach (var kvp in value)
            {
                _addonDetector.AddonsPaths[kvp.Key] = kvp.Value;
            }
        }
    }

    /// <inheritdoc />
    public override Enum Skills => new Duke3DSkillsEnum();
}
