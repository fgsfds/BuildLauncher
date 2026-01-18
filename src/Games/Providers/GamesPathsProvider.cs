using System.Runtime.InteropServices;
using Common.All.Enums;
using Common.All.Enums.Versions;
using Common.Client.Interfaces;
using Microsoft.Win32;

namespace Games.Providers;

public sealed class GamesPathsProvider
{
    private readonly IConfigProvider _config;

    private readonly string? _dukePath = null;
    private readonly string? _dukeWtPath = null;
    private readonly string? _wangPath = null;
    private readonly string? _bloodPath = null;
    private readonly string? _furyPath = null;
    private readonly string? _redneckPath = null;
    private readonly string? _againPath = null;
    private readonly string? _slavePath = null;
    private readonly string? _namPath = null;
    private readonly string? _ww2giPath = null;
    private readonly string? _witch1Path = null;
    private readonly string? _witch2Path = null;
    private readonly string? _twPath = null;

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

        var libs = GetSteamLibraries();

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


    /// <summary>
    /// Get list of Steam libraries
    /// </summary>
    /// <returns>List of paths to Steam libraries</returns>
    private List<string> GetSteamLibraries()
    {
        var steamInstallPath = GetSteamInstallPath();

        if (steamInstallPath is null)
        {
            return [];
        }

        var libraryfolders = Path.Combine(steamInstallPath, "steamapps", "libraryfolders.vdf");

        if (!File.Exists(libraryfolders))
        {
            return [];
        }

        List<string> result = [];

        var lines = File.ReadAllLines(libraryfolders);

        foreach (var line in lines)
        {
            if (!line.Contains("\"path\""))
            {
                continue;
            }

            var dirLine = line.Split('"');

            var dir = dirLine[^2].Trim();

            if (Directory.Exists(dir))
            {
                var path = Path.Combine(dir.Replace("\\\\", "\\"), "steamapps", "common");
                result.Add(path);
            }
        }

        return result;
    }

    /// <summary>
    /// Get Steam install path
    /// </summary>
    /// <returns></returns>
    private string? GetSteamInstallPath()
    {
        string? result;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var path = (string?)Registry
            .GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam", "SteamPath", null);

            if (path is null)
            {
                //Logger.Error("Can't find Steam install folder");
                return null;
            }

            result = path.Replace('/', Path.DirectorySeparatorChar);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            result = Path.Combine(home, ".local/share/Steam");
        }
        else
        {
            throw new PlatformNotSupportedException("Can't identify platform");
        }

        if (!Directory.Exists(result))
        {
            //Logger.Error($"Steam install folder {result} doesn't exist");
            return null;
        }

        //Logger.Info($"Steam install folder is {result}");
        return result;
    }
}
