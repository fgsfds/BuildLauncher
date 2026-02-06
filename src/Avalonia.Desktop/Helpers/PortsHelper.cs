using Addons.Addons;
using Common.All;
using Common.All.Enums;
using Common.All.Enums.Addons;
using Games.Games;
using Ports.Ports;

namespace Avalonia.Desktop.Helpers;

public static class PortsHelper
{
    /// <summary>
    /// Checks if addon can be run with the selected port.
    /// </summary>
    /// <param name="obj">Addon/</param>
    /// <param name="game">Game enum.</param>
    /// <param name="port">Port.</param>
    public static bool CheckPortRequirements(object? obj, BaseGame game, BasePort port)
    {
        if (obj is not BaseAddon addon)
        {
            return false;
        }

        if (!port.IsInstalled)
        {
            return false;
        }

        if (addon.Executables?[OSEnum.Windows] is not null)
        {
            return addon.Executables[OSEnum.Windows].ContainsKey(port.PortEnum);
        }

        if (addon.RequiredFeatures?.Except(port.SupportedFeatures).Any() is true)
        {
            return false;
        }

        if (!port.SupportedGames.Contains(addon.SupportedGame.GameEnum))
        {
            return false;
        }

        if (addon.SupportedGame.GameVersion is not null &&
            !port.SupportedGamesVersions.Contains(addon.SupportedGame.GameVersion))
        {
            return false;
        }

        if (port.PortEnum is PortEnum.BuildGDX)
        {
            if (addon.Type is not AddonTypeEnum.Official)
            {
                return false;
            }

            if (addon is LooseMap)
            {
                return false;
            }
        }

        if (port.PortEnum is PortEnum.DosBox)
        {
            if (addon.MainDef is not null || addon.AdditionalDefs is not null)
            {
                return false;
            }

            if (game.GameEnum is GameEnum.Duke3D && addon.Type is not AddonTypeEnum.Official)
            {
                return false;
            }

            if (game.GameEnum is GameEnum.Duke3D && addon.AddonId.Id.Equals(nameof(DukeAddonEnum.DukeVaca), StringComparison.OrdinalIgnoreCase))
            {
                return File.Exists(Path.Combine(game.GameInstallFolder!, "VACATION.EXE"));
            }

            if (game.GameEnum is GameEnum.Wang && addon.AddonId != new AddonId(nameof(GameEnum.Wang)))
            {
                return false;
            }

            if (addon is LooseMap)
            {
                return false;
            }
        }

        return true;
    }
}
