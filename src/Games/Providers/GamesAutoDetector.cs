using Common.Config;
using Common.Enums;
using Common.Helpers;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace Games.Providers
{
    public class GamesAutoDetector
    {
        private readonly ConfigEntity _config;

        private readonly string? _dukePath = null;
        private readonly string? _dukeWtPath = null;
        private readonly string? _wangPath = null;
        private readonly string? _bloodPath = null;
        private readonly string? _furyPath = null;
        private readonly string? _redneckPath = null;
        private readonly string? _againPath = null;
        private readonly string? _slavePath = null;
        private readonly string? _namPath = null;
        private readonly string? _wwiiPath = null;
        private readonly string? _witch1Path = null;
        private readonly string? _witch2Path = null;

        public GamesAutoDetector(ConfigProvider configProvider)
        {
            _config = configProvider.Config;

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
                pathToGame = Path.Combine(lib, "Duke Nukem 3D", "gameroot", "classic");
                if (Directory.Exists(pathToGame))
                {
                    _dukePath ??= pathToGame;
                }

                //World Tour
                //Using WT as a base game as a last resort
                pathToGame = Path.Combine(lib, "Duke Nukem 3D Twentieth Anniversary World Tour");
                if (Directory.Exists(pathToGame))
                {
                    _dukePath ??= pathToGame;
                }


                //WORLD TOUR
                pathToGame = Path.Combine(lib, "Duke Nukem 3D Twentieth Anniversary World Tour");
                if (Directory.Exists(pathToGame))
                {
                    _dukeWtPath ??= pathToGame;
                }
                

                //WANG
                //Classic
                pathToGame = Path.Combine(lib, "Shadow Warrior DOS", "Shadow Warrior");
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

                //Free
                pathToGame = Path.Combine(lib, "Shadow Warrior Original", "gameroot");
                if (Directory.Exists(pathToGame))
                {
                    _wangPath ??= pathToGame;
                }
                

                //BLOOD
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
                    _wwiiPath ??= pathToGame;
                }


                //WITCHAVEN
                pathToGame = Path.Combine(lib, "Witchaven", "Original", "GAME", "WHAVEN");
                if (Directory.Exists(pathToGame))
                {
                    _witch1Path ??= pathToGame;
                }

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
                GameEnum.Again => _againPath,
                GameEnum.Duke3D => _dukePath,
                GameEnum.DukeWT => _dukeWtPath,
                GameEnum.Wang => _wangPath,
                GameEnum.Fury => _furyPath,
                GameEnum.Slave => _slavePath,
                GameEnum.NAM => _namPath,
                GameEnum.WWIIGI => _wwiiPath,
                GameEnum.Witchaven => _witch1Path,
                GameEnum.Witchaven2 => _witch2Path,
                GameEnum.TekWar => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }

        private void FillConfig()
        {
            _config.GamePathDuke3D ??= GetPath(GameEnum.Duke3D);
            _config.GamePathDukeWT ??= GetPath(GameEnum.DukeWT);
            _config.GamePathWang ??= GetPath(GameEnum.Wang);
            _config.GamePathBlood ??= GetPath(GameEnum.Blood);
            _config.GamePathFury ??= GetPath(GameEnum.Fury);
            _config.GamePathSlave ??= GetPath(GameEnum.Slave);
            _config.GamePathRedneck ??= GetPath(GameEnum.Redneck);
            _config.GamePathAgain ??= GetPath(GameEnum.Again);
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

                var dir = dirLine.ElementAt(dirLine.Length - 2).Trim();

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
                return ThrowHelper.PlatformNotSupportedException<string>("Can't identify platform");
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
}
