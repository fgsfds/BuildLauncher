using Common.Enums;
using Common.Enums.Addons;
using Common.Interfaces;
using Games.Games;
using Mods.Mods;
using Ports.Providers;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Ports.Ports
{
    /// <summary>
    /// Raze port
    /// </summary>
    public sealed class Raze : BasePort
    {
        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.Raze;

        /// <inheritdoc/>
        public override string Exe => "raze.exe";

        /// <inheritdoc/>
        public override string Name => "Raze";

        /// <inheritdoc/>
        public override string ConfigFile => "raze_portable.ini";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames =>
            [
            GameEnum.Blood, 
            GameEnum.Duke3D,
            GameEnum.Wang,
            GameEnum.Powerslave,
            GameEnum.RedneckRampage,
            GameEnum.RidesAgain,
            GameEnum.NAM,
            GameEnum.WWIIGI
            ];

        /// <inheritdoc/>
        public override Uri RepoUrl => new("https://api.github.com/repos/ZDoom/Raze/releases");

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => static x => x.FileName.EndsWith(".zip") && !x.FileName.Contains("linux") && !x.FileName.Contains("macos");

        /// <inheritdoc/>
        public override int? InstalledVersion =>
            File.Exists(FullPathToExe)
            ? int.Parse(new string(FileVersionInfo.GetVersionInfo(FullPathToExe).FileVersion!.Where(static x => char.IsDigit(x)).ToArray()))
            : null;

        /// <inheritdoc/>
        public override void BeforeStart(IGame game)
        {
            var config = Path.Combine(FolderPath, ConfigFile);

            if (!File.Exists(config))
            {
                //creating default config if it doesn't exist
                var text = """
                    [GameSearch.Directories]
                    Path=.

                    [FileSearch.Directories]
                    Path=.
                    Path=.

                    [SoundfontSearch.Directories]
                    Path=$PROGDIR/soundfonts

                    [Exhumed.AutoExec]
                    Path=$PROGDIR/autoexec.cfg

                    [GlobalSettings]
                    gl_texture_filter=6
                    snd_alresampler=Nearest
                    gl_tonemap=5
                    hw_useindexedcolortextures=true
                    """;

                File.WriteAllText(config, text);
            }

            var contents = AddGamePathsToConfig(game.GameInstallFolder, game.ModsFolderPath, config);
            File.WriteAllLines(config, contents);
        }

        /// <inheritdoc/>
        public override string GetStartCampaignArgs(IGame game, IMod mod)
        {
            var args = $@" -nosetup -savedir ""{Path.Combine(FolderPath, "Save", mod.DisplayName.Replace(' ', '_'))}""";

            if (mod is BloodCampaign bMod)
            {
                args = GetBloodArgs(game, mod, args, bMod);
            }
            else if (mod is DukeCampaign dMod)
            {
                args = GetDukeArgs(game, args, dMod);
            }
            else if (mod is WangCampaign wMod)
            {
                args = GetWangArgs(game, args, wMod);
            }
            else if (mod is SingleMap)
            {
                args += $@" -file ""{game.MapsFolderPath}\{mod.FileName}"" -map ""{mod.FileName}""";
            }

            return args;
        }

        /// <inheritdoc/>
        public override string GetAutoloadModsArgs(IGame game, ImmutableList<IMod> mods)
        {
            if (mods.Count == 0)
            {
                return string.Empty;
            }

            string args = string.Empty;

            foreach (var mod in mods)
            {
                args += $@" -file ""{mod.FileName}""";
            }

            return args;
        }

        /// <inheritdoc/>
        public override string GetSkipIntroParameter() => " -quick";

        private static string GetWangArgs(IGame game, string args, WangCampaign wMod)
        {
            args += @" -iwad SW.GRP";

            if (wMod.FileName is null)
            {
                if (wMod.AddonEnum is WangAddonEnum.Wanton)
                {
                    args += @" -file WT.GRP";
                }
                else if (wMod.AddonEnum is WangAddonEnum.TwinDragon)
                {
                    args += @" -file TD.GRP";
                }
            }
            else if (wMod.FileName is not null)
            {
                args += $@" -file ""{Path.Combine(game.CampaignsFolderPath, wMod.FileName)}""";
            }

            return args;
        }

        private string GetDukeArgs(IGame game, string args, DukeCampaign dMod)
        {
            args += $" -addon {(byte)dMod.AddonEnum}";

            if (dMod.AddonEnum is DukeAddonEnum.WorldTour &&
                game is DukeGame dukeGame)
            {
                var config = Path.Combine(FolderPath, ConfigFile);
                var contents = AddGamePathsToConfig(dukeGame.DukeWTInstallPath, game.ModsFolderPath, config);
                File.WriteAllLines(config, contents);

                return args;
            }

            if (dMod.FileName is not null)
            {
                args += $@" -file ""{Path.Combine(game.CampaignsFolderPath, dMod.FileName)}""";
            }

            if (dMod.ConFile is not null)
            {
                args += $@" -con {dMod.ConFile}";
            }

            return args;
        }

        private static string GetBloodArgs(IGame game, IMod mod, string args, BloodCampaign bMod)
        {
            args += " -iwad BLOOD.RFF";

            if (mod.FileName is null)
            {
                args += $@" -ini ""{bMod.IniFile}""";
            }
            else if (mod.FileName is not null)
            {
                args += $@" -file ""{game.CampaignsFolderPath}\{mod.FileName}""";
            }

            return args;
        }

        /// <summary>
        /// Add paths to game and mods folder to the config
        /// </summary>
        [Obsolete("Remove if this ever implemented https://github.com/ZDoom/Raze/issues/1060")]
        private static string[] AddGamePathsToConfig(string gameFolder, string modsFolder, string config)
        {
            var contents = File.ReadAllLines(config);

            for (var i = 0; i < contents.Length; i++)
            {
                if (contents[i].Equals("[GameSearch.Directories]"))
                {
                    var path = gameFolder.Replace('\\', '/');

                    contents[i + 1] = $"Path={path}";
                    continue;
                }
                if (contents[i].Equals("[FileSearch.Directories]"))
                {
                    var path = modsFolder.Replace('\\', '/');

                    contents[i + 1] = $"Path={path}";

                    path = gameFolder.Replace('\\', '/');

                    contents[i + 2] = $"Path={path}";

                    break;
                }
            }

            return contents;
        }
    }
}
