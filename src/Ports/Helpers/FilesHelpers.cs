using Games.Games;

namespace Ports.Helpers;

/// <summary>
///     Helpers for restoring game files that ports temporarily override.
/// </summary>
public class FilesHelpers
{
    /// <summary>
    ///     Removes Route 66 art file overrides used for RedNukem.
    /// </summary>
    public static void RestoreRoute66Files(BaseGame game)
    {
        if (game is not RedneckGame)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(game.GameInstallFolder);

        var tilesA2 = Path.Combine(game.GameInstallFolder, "TILES024.ART");
        var tilesB2 = Path.Combine(game.GameInstallFolder, "TILES025.ART");
        var turdMovAnm2 = Path.Combine(game.GameInstallFolder, "TURDMOV.ANM");
        var turdMovVoc2 = Path.Combine(game.GameInstallFolder, "TURDMOV.VOC");
        var endMovAnm2 = Path.Combine(game.GameInstallFolder, "RR_OUTRO.ANM");
        var endMovVoc2 = Path.Combine(game.GameInstallFolder, "LN_FINAL.VOC");

        if (File.Exists(tilesA2))
        {
            File.Delete(tilesA2);
        }

        if (File.Exists(tilesB2))
        {
            File.Delete(tilesB2);
        }

        if (File.Exists(turdMovAnm2))
        {
            File.Delete(turdMovAnm2);
        }

        if (File.Exists(turdMovVoc2))
        {
            File.Delete(turdMovVoc2);
        }

        if (File.Exists(endMovAnm2))
        {
            File.Delete(endMovAnm2);
        }

        if (File.Exists(endMovVoc2))
        {
            File.Delete(endMovVoc2);
        }
    }

    /// <summary>
    ///     Restores Duke WT's ART files.
    /// </summary>
    public static void RestoreWtFiles(BaseGame game)
    {
        if (game is not DukeGame dGame)
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

        if (File.Exists(art1r))
        {
            File.Move(art1r, art1, true);
        }

        if (File.Exists(art2r))
        {
            File.Move(art2r, art2, true);
        }

        if (File.Exists(art3r))
        {
            File.Move(art3r, art3, true);
        }

        if (File.Exists(art4r))
        {
            File.Move(art4r, art4, true);
        }

        if (dGame.DukeWTInstallPath is not null)
        {
            art1 = Path.Combine(dGame.DukeWTInstallPath, "TILES009.ART");
            art1r = Path.Combine(dGame.DukeWTInstallPath, "TILES009._ART");

            art2 = Path.Combine(dGame.DukeWTInstallPath, "TILES020.ART");
            art2r = Path.Combine(dGame.DukeWTInstallPath, "TILES020._ART");

            art3 = Path.Combine(dGame.DukeWTInstallPath, "TILES021.ART");
            art3r = Path.Combine(dGame.DukeWTInstallPath, "TILES021._ART");

            art4 = Path.Combine(dGame.DukeWTInstallPath, "TILES022.ART");
            art4r = Path.Combine(dGame.DukeWTInstallPath, "TILES022._ART");

            if (File.Exists(art1r))
            {
                File.Move(art1r, art1, true);
            }

            if (File.Exists(art2r))
            {
                File.Move(art2r, art2, true);
            }

            if (File.Exists(art3r))
            {
                File.Move(art3r, art3, true);
            }

            if (File.Exists(art4r))
            {
                File.Move(art4r, art4, true);
            }
        }
    }
}
