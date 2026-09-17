using Core.All.Enums;
using Core.All.Enums.Addons;
using Games.Skills;

namespace Games.Games;

/// <summary>
///     Represents the game Duke Nukem 3D and its associated addon detection.
/// </summary>
public sealed class DukeGame : BaseGame
{
    /// <inheritdoc />
    protected override IReadOnlyCollection<Enum> SupportedAddons =>
    [
        DukeAddonEnum.DukeVaca,
        DukeAddonEnum.DukeDC,
        DukeAddonEnum.DukeNW
    ];

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
    protected override IReadOnlyCollection<string> RequiredFiles { get; } = ["DUKE3D.GRP"];

    /// <summary>
    ///     Is Duke it Out in DC installed.
    /// </summary>
    public bool IsDukeDCInstalled => AddonsFolders.ContainsKey(DukeAddonEnum.DukeDC);

    /// <summary>
    ///     Is Nuclear Winter installed.
    /// </summary>
    public bool IsNuclearWinterInstalled => AddonsFolders.ContainsKey(DukeAddonEnum.DukeNW);

    /// <summary>
    ///     Is Caribbean installed.
    /// </summary>
    public bool IsCaribbeanInstalled => AddonsFolders.ContainsKey(DukeAddonEnum.DukeVaca);

    /// <summary>
    ///     Is World Tour installed.
    /// </summary>
    public bool IsWorldTourInstalled => IsInstalled(
        [
            "EPISODE5BOSS.CON",
            "FIREFLYTROOPER.CON",
            "FLAMETHROWER.CON"
        ], DukeWTInstallPath
        );

    /// <summary>
    ///     Is Duke 64 installed.
    /// </summary>
    public bool IsDuke64Installed => File.Exists(Duke64RomPath);

    /// <summary>
    ///     Is Duke ZH installed.
    /// </summary>
    public bool IsDukeZHInstalled => File.Exists(DukeZHRomPath);

    /// <inheritdoc />
    public override Enum Skills => new Duke3DSkillsEnum();


    /// <inheritdoc />
    protected override IReadOnlyDictionary<Enum, string> DetectAddonsFolders()
    {
        Dictionary<Enum, string> folders = [];

        foreach (var addon in SupportedAddons)
        {
            if (TryFindAddon(addon, GameInstallFolder, out var addonFolder) && addonFolder is not null)
            {
                folders[addon] = addonFolder;
            }
        }

        return folders;
    }


    /// <summary>
    ///     Search for a specific Duke addon in the game install folder, checking all known retail/remaster layouts.
    /// </summary>
    /// <param name="addon">
    ///     The addon to search for.
    /// </param>
    /// <param name="gameInstallFolder">
    ///     Duke Nukem 3D base install folder.
    /// </param>
    /// <param name="addonFolder">
    ///     The directory containing the addon GRP, if found.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the addon GRP was found.
    /// </returns>
    private static bool TryFindAddon(Enum addon, string? gameInstallFolder, out string? addonFolder)
    {
        addonFolder = null;

        if (gameInstallFolder is null)
        {
            return false;
        }

        var file = addon switch
        {
            DukeAddonEnum.DukeDC => "DUKEDC.GRP",
            DukeAddonEnum.DukeNW => "NWINTER.GRP",
            DukeAddonEnum.DukeVaca => "VACATION.GRP",
            _ => throw new ArgumentOutOfRangeException(nameof(addon), addon, $"Unsupported addon value: {addon}.")
        };

        string[] searchPaths =
        [
            Path.Combine(gameInstallFolder, file),
            Path.Combine(gameInstallFolder, "AddOns", file),
            Path.Combine(gameInstallFolder, "addons", "dc", file),
            Path.Combine(gameInstallFolder, "addons", "nw", file),
            Path.Combine(gameInstallFolder, "addons", "vacation", file)
        ];

        foreach (var path in searchPaths)
        {
            if (!File.Exists(path))
            {
                continue;
            }

            addonFolder = Path.GetDirectoryName(path);

            return addonFolder is not null;
        }

        return false;
    }
}
