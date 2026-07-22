using Core.All.Enums;
using Core.All.Enums.Versions;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Microsoft.Win32;

namespace Games.Providers;

/// <summary>
///     Scans Steam libraries and the Windows registry to discover installed game paths.
/// </summary>
public sealed class GamesPathsProvider
{
    private readonly Dictionary<GameEnum, string?> _paths = new();
    private string? _dukeWtPath;
    private readonly IConfigProvider _config;

    private static readonly string[] DukeWtSteamPaths = [Path.Combine("Duke Nukem 3D Twentieth Anniversary World Tour")];

    private static readonly HashSet<GameEnum> SupportedGames =
    [
        GameEnum.Duke3D, GameEnum.Wang, GameEnum.Blood, GameEnum.Fury,
        GameEnum.Slave, GameEnum.Redneck, GameEnum.RidesAgain, GameEnum.NAM,
        GameEnum.WW2GI, GameEnum.Witchaven, GameEnum.Witchaven2, GameEnum.TekWar
    ];

    private static readonly (GameEnum Game, string[] SteamSubPaths)[] SteamScans =
    [
        (GameEnum.Duke3D, [
             Path.Combine("Duke Nukem 3D", "Duke Nukem 3D"),
             Path.Combine("Duke Nukem 3D", "gameroot")
         ]),
        (GameEnum.Wang, [
             Path.Combine("Shadow Warrior DOS", "Shadow Warrior"),
             Path.Combine("Shadow Warrior Original", "gameroot"),
             Path.Combine("Shadow Warrior Classic", "gameroot")
         ]),
        (GameEnum.Blood, [
             Path.Combine("Blood - Refreshed Supply", "DOS", "BLOOD121"),
             Path.Combine("Blood", "DOS", "C", "BLOOD"),
             "One Unit Whole Blood"
         ]),
        (GameEnum.Fury, ["Ion Fury"]),
        (GameEnum.Slave, [Path.Combine("PowerslaveCE", "PWRSLAVE")]),
        (GameEnum.Redneck, [Path.Combine("Redneck Rampage", "Redneck")]),
        (GameEnum.RidesAgain, [Path.Combine("Redneck Rampage Rides Again", "AGAIN")]),
        (GameEnum.NAM, [Path.Combine("Nam", "NAM")]),
        (GameEnum.WW2GI, [Path.Combine("World War II GI", "WW2GI")]),
        (GameEnum.Witchaven, [Path.Combine("Witchaven", "Original", "GAME", "WHAVEN")]),
        (GameEnum.Witchaven2, [Path.Combine("Witchaven II Blood Vengeance", "Original", "GAME", "WHAVEN2")])
    ];

    /// <summary>
    ///     Initializes a new instance of <see cref="GamesPathsProvider" />.
    /// </summary>
    /// <param name="config">
    ///     Configuration provider to populate with discovered paths.
    /// </param>
    public GamesPathsProvider(IConfigProvider config)
    {
        _config = config;

        if (OperatingSystem.IsWindows())
        {
            var zoomPath = (string?)Registry.GetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\ZOOM PLATFORM\Duke Nukem 3D - Atomic Edition",
                "InstallPath", null
                );

            if (!string.IsNullOrWhiteSpace(zoomPath) && Directory.Exists(zoomPath))
            {
                _paths[GameEnum.Duke3D] = zoomPath;
            }
        }

        var libs = SteamHelper.GetSteamLibraries();

        ScanLibraryPaths(
            libs, DukeWtSteamPaths, path =>
            {
                _dukeWtPath ??= path;
                _paths.TryAdd(GameEnum.Duke3D, path);
            }
            );

        foreach (var (game, subPaths) in SteamScans)
        {
            if (_paths.ContainsKey(game))
            {
                continue;
            }

            ScanLibraryPaths(libs, subPaths, path => _paths[game] = path);
        }

        FillConfig();
    }

    /// <summary>
    ///     Gets the install path for the specified game.
    /// </summary>
    /// <param name="game">
    ///     The game to look up.
    /// </param>
    /// <returns>
    ///     The install path, or <see langword="null" /> if not found.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown if the game is not supported.
    /// </exception>
    public string? GetPath(GameEnum game)
    {
        if (!SupportedGames.Contains(game))
        {
            throw new NotSupportedException($"Getting install path for game '{game}' is not supported.");
        }

        return _paths.TryGetValue(game, out var path) ? path : null;
    }

    /// <summary>
    ///     Gets the install path for the specified Duke Nukem version.
    /// </summary>
    /// <param name="game">
    ///     The Duke version to look up.
    /// </param>
    /// <returns>
    ///     The install path, or <see langword="null" /> if not found.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown if the version is not supported.
    /// </exception>
    public string? GetPath(DukeVersionEnum game)
    {
        return game switch
        {
            DukeVersionEnum.Duke3D_13D or DukeVersionEnum.Duke3D_Atomic => GetPath(GameEnum.Duke3D),
            DukeVersionEnum.Duke3D_WT => _dukeWtPath,
            _ => throw new NotSupportedException($"Getting install path for Duke version '{game}' is not supported.")
        };
    }

    private static void ScanLibraryPaths(IEnumerable<string> libs, IEnumerable<string> subPaths, Action<string> onFound)
    {
        foreach (var lib in libs)
        {
            foreach (var parts in subPaths)
            {
                var fullPath = Path.Combine(lib, parts);

                if (Directory.Exists(fullPath))
                {
                    onFound(fullPath);

                    return;
                }
            }
        }
    }

    private void FillConfig()
    {
        _config.PathDuke3D ??= GetPath(GameEnum.Duke3D);
        _config.PathDukeWT ??= _dukeWtPath;
        _config.PathWang ??= GetPath(GameEnum.Wang);
        _config.PathBlood ??= GetPath(GameEnum.Blood);
        _config.PathFury ??= GetPath(GameEnum.Fury);
        _config.PathSlave ??= GetPath(GameEnum.Slave);
        _config.PathRedneck ??= GetPath(GameEnum.Redneck);
        _config.PathRidesAgain ??= GetPath(GameEnum.RidesAgain);
        _config.PathNam ??= GetPath(GameEnum.NAM);
        _config.PathWW2GI ??= GetPath(GameEnum.WW2GI);
        _config.PathWitchaven ??= GetPath(GameEnum.Witchaven);
        _config.PathWitchaven2 ??= GetPath(GameEnum.Witchaven2);
        _config.PathTekWar ??= GetPath(GameEnum.TekWar);
    }
}
