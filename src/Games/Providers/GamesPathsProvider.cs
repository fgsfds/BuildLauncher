using Core.All.Enums;
using Core.All.Enums.Versions;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Microsoft.Win32;

namespace Games.Providers;

/// <summary>
///     Provides game install paths detected from Steam libraries and registry.
/// </summary>
public sealed class GamesPathsProvider
{
    /// <summary>
    ///     Path to Rides Again installation.
    /// </summary>
    private readonly string? _againPath = null;

    /// <summary>
    ///     Path to Blood installation.
    /// </summary>
    private readonly string? _bloodPath = null;

    private readonly IConfigProvider _config;

    /// <summary>
    ///     Path to Duke Nukem 3D installation.
    /// </summary>
    private readonly string? _dukePath = null;

    /// <summary>
    ///     Path to Duke Nukem 3D World Tour installation.
    /// </summary>
    private readonly string? _dukeWtPath = null;

    /// <summary>
    ///     Path to Ion Fury installation.
    /// </summary>
    private readonly string? _furyPath = null;

    /// <summary>
    ///     Path to NAM installation.
    /// </summary>
    private readonly string? _namPath = null;

    /// <summary>
    ///     Path to Redneck Rampage installation.
    /// </summary>
    private readonly string? _redneckPath = null;

    /// <summary>
    ///     Path to Powerslave installation.
    /// </summary>
    private readonly string? _slavePath = null;

    /// <summary>
    ///     Path to TekWar installation.
    /// </summary>
    private readonly string? _twPath = null;

    /// <summary>
    ///     Path to Shadow Warrior installation.
    /// </summary>
    private readonly string? _wangPath = null;

    /// <summary>
    ///     Path to Witchaven installation.
    /// </summary>
    private readonly string? _witch1Path = null;

    /// <summary>
    ///     Path to Witchaven 2 installation.
    /// </summary>
    private readonly string? _witch2Path = null;

    /// <summary>
    ///     Path to World War II GI installation.
    /// </summary>
    private readonly string? _ww2giPath = null;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GamesPathsProvider" /> class.
    /// </summary>
    /// <param name="config">Configuration provider.</param>
    public GamesPathsProvider(IConfigProvider config)
    {
        _config = config;

        if (OperatingSystem.IsWindows())
        {
            var zoomPath = (string?)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\ZOOM PLATFORM\Duke Nukem 3D - Atomic Edition", "InstallPath", null);

            if (!string.IsNullOrWhiteSpace(zoomPath))
            {
                if (Directory.Exists(zoomPath))
                {
                    _dukePath ??= zoomPath;
                }
            }
        }

        var libs = SteamHelper.GetSteamLibraries();

        foreach (var lib in libs)
        {
            //DUKE
            //3D Realms Anthology
            var pathToGame = Path.Combine(lib, "Duke Nukem 3D", "Duke Nukem 3D");

            if (Directory.Exists(pathToGame))
            {
                _dukePath ??= pathToGame;
            }

            //Megaton
            pathToGame = Path.Combine(lib, "Duke Nukem 3D", "gameroot");

            if (Directory.Exists(pathToGame))
            {
                _dukePath ??= pathToGame;
            }


            //WORLD TOUR
            pathToGame = Path.Combine(lib, "Duke Nukem 3D Twentieth Anniversary World Tour");

            if (Directory.Exists(pathToGame))
            {
                //Using WT as a base game as a last resort
                _dukePath ??= pathToGame;
                _dukeWtPath ??= pathToGame;
            }


            //WANG
            //Classic
            pathToGame = Path.Combine(lib, "Shadow Warrior DOS", "Shadow Warrior");

            if (Directory.Exists(pathToGame))
            {
                _wangPath ??= pathToGame;
            }

            //Free
            pathToGame = Path.Combine(lib, "Shadow Warrior Original", "gameroot");

            if (Directory.Exists(pathToGame))
            {
                _wangPath ??= pathToGame;
            }

            //Redux
            pathToGame = Path.Combine(lib, "Shadow Warrior Classic", "gameroot");

            if (Directory.Exists(pathToGame))
            {
                _wangPath ??= pathToGame;
            }


            //BLOOD
            //RS
            pathToGame = Path.Combine(lib, "Blood - Refreshed Supply", "DOS", "BLOOD121");

            if (Directory.Exists(pathToGame))
            {
                _bloodPath ??= pathToGame;
            }

            //FS
            pathToGame = Path.Combine(lib, "Blood", "DOS", "C", "BLOOD");

            if (Directory.Exists(pathToGame))
            {
                _bloodPath ??= pathToGame;
            }

            //OUWB
            pathToGame = Path.Combine(lib, "One Unit Whole Blood");

            if (Directory.Exists(pathToGame))
            {
                _bloodPath ??= pathToGame;
            }


            //FURY
            pathToGame = Path.Combine(lib, "Ion Fury");

            if (Directory.Exists(pathToGame))
            {
                _furyPath ??= pathToGame;
            }


            //SLAVE
            pathToGame = Path.Combine(lib, "PowerslaveCE", "PWRSLAVE");

            if (Directory.Exists(pathToGame))
            {
                _slavePath ??= pathToGame;
            }


            //REDNECK
            pathToGame = Path.Combine(lib, "Redneck Rampage", "Redneck");

            if (Directory.Exists(pathToGame))
            {
                _redneckPath ??= pathToGame;
            }


            //RIDES AGAIN
            pathToGame = Path.Combine(lib, "Redneck Rampage Rides Again", "AGAIN");

            if (Directory.Exists(pathToGame))
            {
                _againPath ??= pathToGame;
            }


            //NAM
            pathToGame = Path.Combine(lib, "Nam", "NAM");

            if (Directory.Exists(pathToGame))
            {
                _namPath ??= pathToGame;
            }


            //WWII GI
            pathToGame = Path.Combine(lib, "World War II GI", "WW2GI");

            if (Directory.Exists(pathToGame))
            {
                _ww2giPath ??= pathToGame;
            }


            //WITCHAVEN
            pathToGame = Path.Combine(lib, "Witchaven", "Original", "GAME", "WHAVEN");

            if (Directory.Exists(pathToGame))
            {
                _witch1Path ??= pathToGame;
            }

            //WITCHAVEN II
            pathToGame = Path.Combine(lib, "Witchaven II Blood Vengeance", "Original", "GAME", "WHAVEN2");

            if (Directory.Exists(pathToGame))
            {
                _witch2Path ??= pathToGame;
            }
        }

        FillConfig();
    }

    /// <summary>
    ///     Gets the install path for the specified game.
    /// </summary>
    /// <param name="game">The game enum.</param>
    /// <returns>The install path, or <see langword="null" /> if not found.</returns>
    public string? GetPath(GameEnum game)
    {
        return game switch
        {
            GameEnum.Blood => _bloodPath,
            GameEnum.Redneck => _redneckPath,
            GameEnum.RidesAgain => _againPath,
            GameEnum.Duke3D => _dukePath,
            GameEnum.Wang => _wangPath,
            GameEnum.Fury => _furyPath,
            GameEnum.Slave => _slavePath,
            GameEnum.NAM => _namPath,
            GameEnum.WW2GI => _ww2giPath,
            GameEnum.Witchaven => _witch1Path,
            GameEnum.Witchaven2 => _witch2Path,
            GameEnum.TekWar => _twPath,
            _ => throw new NotSupportedException()
        };
    }

    /// <summary>
    ///     Gets the install path for the specified Duke Nukem 3D version.
    /// </summary>
    /// <param name="game">The Duke version enum.</param>
    /// <returns>The install path, or <see langword="null" /> if not found.</returns>
    public string? GetPath(DukeVersionEnum game)
    {
        return game switch
        {
            DukeVersionEnum.Duke3D_13D => _dukePath,
            DukeVersionEnum.Duke3D_Atomic => _dukePath,
            DukeVersionEnum.Duke3D_WT => _dukeWtPath,
            _ => throw new NotSupportedException()
        };
    }

    /// <summary>
    ///     Fills missing configuration paths with detected game install paths.
    /// </summary>
    private void FillConfig()
    {
        _config.PathDuke3D ??= GetPath(GameEnum.Duke3D);
        _config.PathDukeWT ??= GetPath(DukeVersionEnum.Duke3D_WT);
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
