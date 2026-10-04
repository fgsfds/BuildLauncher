using System.Collections.Frozen;
using Core.All.Enums;
using Core.All.Serializable.Addon;

namespace Core.All.Helpers;

/// <summary>Validates authoring rules shared by the developer editor and addon preparation tools.</summary>
public static class AddonManifestValidator
{
    /// <summary>Conventional root names that conflict with game scripts and definitions.</summary>
    private static readonly FrozenSet<string> _forbiddenNames = new[]
    {
        "duke3d.def", "rr.def", "rrra.def", "nam.def", "napalm.def", "ww2gi.def",
        "blood.def", "sw.def", "exhumed.def", "game.con", "defs.con", "user.con",
        "nam.con", "napalm.con", "ww2gi.con", "blood.ini", "cryptic.ini"
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>Checks a parsed manifest and the names of files at its package root.</summary>
    /// <param name="manifest">The parsed user-addon manifest.</param>
    /// <param name="rootFileNames">Root filenames, not recursively collected paths.</param>
    /// <returns>The first authoring error or a successful validation result.</returns>
    /// <remarks>This is stricter than JSON parsing and does not check port support or referenced file existence.</remarks>
    public static Result Validate(AddonManifestJsonModel manifest, IEnumerable<string> rootFileNames)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(rootFileNames);
        if (manifest.AddonType is not (AddonTypeEnum.TC or AddonTypeEnum.Map or AddonTypeEnum.Mod))
        {
            return Error("Addon type (TC, Map, or Mod) must be selected.");
        }

        if (manifest.SupportedGame is null || !Enum.IsDefined(manifest.SupportedGame.Game))
        {
            return Error("Game must be selected.");
        }

        if (string.IsNullOrWhiteSpace(manifest.Title))
        {
            return Error("Addon title must be specified.");
        }

        if (string.IsNullOrWhiteSpace(manifest.Id))
        {
            return Error("Addon ID must be specified.");
        }

        if (string.IsNullOrWhiteSpace(manifest.Version))
        {
            return Error("Addon version must be specified.");
        }

        if (manifest.AddonType is AddonTypeEnum.Map)
        {
            if (manifest.StartMap is MapFileJsonModel mapFile)
            {
                if (string.IsNullOrWhiteSpace(mapFile.File))
                {
                    return Error("Start map file name must be specified.");
                }
            }
            else if (manifest.StartMap is not MapSlotJsonModel)
            {
                return Error("Start map type must be selected.");
            }
        }

        var hasCustomExecutable = manifest.Executables?.Values.Any(ports => ports?.Values.Any(path => !string.IsNullOrWhiteSpace(path)) is true) is true;
        if (manifest.SupportedGame.Game is GameEnum.Standalone || hasCustomExecutable ||
            !string.IsNullOrWhiteSpace(manifest.MainRff) || !string.IsNullOrWhiteSpace(manifest.SoundRff))
        {
            return new Result(ResultEnum.Success, "Addon manifest authoring checks passed.");
        }

        var files = rootFileNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var forbidden = files.Intersect(_forbiddenNames, StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToArray();
        if (forbidden.Length != 0)
        {
            return Error($"Common file names cannot be used in addon folder. Rename these files: {string.Join(", ", forbidden)}.");
        }

        if (manifest.SupportedGame.Game is GameEnum.Blood)
        {
            if (files.Any(name => name.EndsWith(".art", StringComparison.OrdinalIgnoreCase)))
            {
                return Error("ART files are not supported in Blood addons. Convert them to DEF.");
            }

            if (files.Any(name => name.EndsWith(".dat", StringComparison.OrdinalIgnoreCase)))
            {
                return Error("DAT files are not supported in Blood addons. Convert them to DEF.");
            }

            if (files.Any(name => name.EndsWith(".rfs", StringComparison.OrdinalIgnoreCase)))
            {
                return Error("Addons with RFS files are not supported.");
            }

            if (files.Any(IsReservedBloodMusic))
            {
                return Error("blood00.ogg through blood09.ogg are reserved and cannot be used. Start with blood10.ogg.");
            }
        }

        return new Result(ResultEnum.Success, "Addon manifest authoring checks passed.");
    }

    /// <summary>Recognizes the ten reserved Blood music filenames.</summary>
    /// <param name="name">The root filename.</param>
    /// <returns>Whether the filename is blood00.ogg through blood09.ogg.</returns>
    private static bool IsReservedBloodMusic(string name) =>
        name.Length == 11 && name.StartsWith("blood0", StringComparison.OrdinalIgnoreCase) &&
        name[6] is >= '0' and <= '9' && name.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase);

    /// <summary>Constructs an authoring-validation failure.</summary>
    /// <param name="message">The actionable diagnostic.</param>
    /// <returns>An error result.</returns>
    private static Result Error(string message) => new(ResultEnum.Error, message);
}
