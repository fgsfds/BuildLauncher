using Core.All.Enums.Addons;
using Core.All.Helpers;

namespace Games.Helpers;

/// <summary>
/// Scans Duke Nukem 3D's install folder for addon GRP files across various retail/remaster layouts.
/// </summary>
public sealed class DukeAddonDetector
{
    /// <summary>
    /// Map of found addons to their install paths.
    /// </summary>
    public Dictionary<DukeAddonEnum, string> AddonsPaths { get; } = [];

    /// <summary>
    /// Search for a specific Duke addon in the game install folder, checking all known retail/remaster layouts.
    /// </summary>
    /// <param name="addon">The addon to search for.</param>
    /// <param name="gameInstallFolder">Duke Nukem 3D base install folder.</param>
    /// <returns><see langword="true"/> if the addon GRP was found.</returns>
    public bool TryFindAddon(DukeAddonEnum addon, string? gameInstallFolder)
    {
        if (gameInstallFolder is null)
        {
            return false;
        }

        var file = addon switch
        {
            DukeAddonEnum.DukeDC => "DUKEDC.GRP",
            DukeAddonEnum.DukeNW => "NWINTER.GRP",
            DukeAddonEnum.DukeVaca => "VACATION.GRP",
            DukeAddonEnum.Base => throw new ArgumentOutOfRangeException(nameof(addon)),
            _ => throw new ArgumentOutOfRangeException(nameof(addon)),
        };

        string[] searchPaths =
        [
            Path.Combine(gameInstallFolder, file),
            Path.Combine(gameInstallFolder, "AddOns", file),
            Path.Combine(gameInstallFolder, "addons", "dc", file),
            Path.Combine(gameInstallFolder, "addons", "nw", file),
            Path.Combine(gameInstallFolder, "addons", "vacation", file),
        ];

        foreach (var path in searchPaths)
        {
            if (File.Exists(path))
            {
                var dirName = Path.GetDirectoryName(path);
                if (dirName is null)
                {
                    return false;
                }

                AddonsPaths.AddOrReplace(addon, dirName);
                return true;
            }
        }

        _ = AddonsPaths.Remove(addon);
        return false;
    }
}
