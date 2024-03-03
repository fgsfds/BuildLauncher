using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Mods;
using Ports.Providers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

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

            AddGamePathsToConfig(game.GameInstallFolder, game.ModsFolderPath, config);
        }

        /// <inheritdoc/>
        public override void GetStartCampaignArgs(StringBuilder sb, IGame game, IMod mod)
        {
            sb.Append($@" -nosetup -savedir ""{Path.Combine(FolderPath, "Save", mod.DisplayName.Replace(' ', '_'))}""");

            if (mod is BloodCampaign bMod)
            {
                GetBloodArgs(sb, game, bMod);
            }
            else if (mod is DukeCampaign dMod)
            {
                GetDukeArgs(sb, game, dMod);
            }
            else if (mod is WangCampaign wMod)
            {
                GetWangArgs(sb, game, wMod);
            }
            else
            {
                ThrowHelper.NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public override void GetAutoloadModsArgs(StringBuilder sb, IGame game, ImmutableList<IMod> mods)
        {
            if (mods.Count == 0)
            {
                return;
            }

            foreach (var mod in mods)
            {
                sb.Append($@" -file ""{mod.FileName}""");
            }
        }

        /// <inheritdoc/>
        public override void GetSkipIntroParameter(StringBuilder sb) => sb.Append(" -quick");

        private static void GetWangArgs(StringBuilder sb, IGame game, WangCampaign wangCamp)
        {
            sb.Append(@" -iwad SW.GRP");

            if (wangCamp.FileName is null)
            {
                if (wangCamp.AddonEnum is WangAddonEnum.Wanton)
                {
                    sb.Append(@" -file WT.GRP");
                }
                else if (wangCamp.AddonEnum is WangAddonEnum.TwinDragon)
                {
                    sb.Append(@" -file TD.GRP");
                }

                return;
            }

            if (wangCamp.ModType is ModTypeEnum.Campaign)
            {
                sb.Append($@" -file ""{Path.Combine(game.CampaignsFolderPath, wangCamp.FileName)}""");
            }
            else if (wangCamp.ModType is ModTypeEnum.Map)
            {
                sb.Append($@" -file ""{Path.Combine(game.MapsFolderPath, wangCamp.FileName)}"" -map ""{wangCamp.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException();
                return;
            }
        }

        private void GetDukeArgs(StringBuilder sb, IGame game, DukeCampaign dukeCamp)
        {
            sb.Append($" -addon {(byte)dukeCamp.AddonEnum}");

            if (dukeCamp.AddonEnum is DukeAddonEnum.WorldTour &&
                game is DukeGame dukeGame)
            {
                var config = Path.Combine(FolderPath, ConfigFile);
                AddGamePathsToConfig(dukeGame.DukeWTInstallPath, game.ModsFolderPath, config);

                return;
            }

            if (dukeCamp.FileName is null)
            {
                return;
            }

            if (dukeCamp.ModType is ModTypeEnum.Campaign)
            {
                sb.Append($@" -file ""{Path.Combine(game.CampaignsFolderPath, dukeCamp.FileName)}"" -con ""{dukeCamp.StartupFile}""");
            }
            else if (dukeCamp.ModType is ModTypeEnum.Map)
            {
                sb.Append($@" -file ""{Path.Combine(game.MapsFolderPath, dukeCamp.FileName)}"" -map ""{dukeCamp.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException();
                return;
            }
        }

        private void GetBloodArgs(StringBuilder sb, IGame game, BloodCampaign bloodCamp)
        {
            var ini = bloodCamp.StartupFile;

            if (bloodCamp.ModType is ModTypeEnum.Map)
            {
                if (bloodCamp.AddonEnum is BloodAddonEnum.Blood)
                {
                    ini = Consts.BloodIni;
                }
                else if (bloodCamp.AddonEnum is BloodAddonEnum.Cryptic)
                {
                    ini = Consts.CrypticIni;
                }
            }

            sb.Append(@$" -iwad BLOOD.RFF -ini ""{ini}""");

            if (bloodCamp.FileName is null)
            {
                return;
            }

            if (bloodCamp.ModType is ModTypeEnum.Campaign)
            {
                sb.Append($@" -file ""{Path.Combine(game.CampaignsFolderPath, bloodCamp.FileName)}""");
            }
            else if (bloodCamp.ModType is ModTypeEnum.Map)
            {
                sb.Append($@" -file ""{Path.Combine(game.MapsFolderPath, bloodCamp.FileName)}"" -map ""{bloodCamp.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException();
                return;
            }
        }

        /// <summary>
        /// Add paths to game and mods folder to the config
        /// </summary>
        [Obsolete("Remove if this ever implemented https://github.com/ZDoom/Raze/issues/1060")]
        private static void AddGamePathsToConfig(string gameFolder, string modsFolder, string config)
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

            File.WriteAllLines(config, contents);
        }
    }
}
