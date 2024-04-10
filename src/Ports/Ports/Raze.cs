using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Mods;
using Ports.Providers;
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
        public override List<GameEnum> SupportedGames =>
            [
            GameEnum.Blood,
            GameEnum.Duke3D,
            GameEnum.Wang,
            GameEnum.Slave,
            GameEnum.Redneck,
            GameEnum.RedneckRA,
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
        protected override string ConfigFile => "raze_portable.ini";

        /// <inheritdoc/>
        protected override string AddDirectoryParam => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override string AddFileParam => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override string AddDefParam => throw new NotImplementedException();

        protected override string AddConParam => throw new NotImplementedException();

        protected override string MainDefParam => throw new NotImplementedException();

        protected override string MainConParam => throw new NotImplementedException();


        /// <inheritdoc/>
        protected override void BeforeStart(IGame game, IAddon campaign)
        {
            var config = Path.Combine(PathToPortFolder, ConfigFile);

            if (!File.Exists(config))
            {
                //creating default config if it doesn't exist
                var text = """
                    [GameSearch.Directories]
                    Path=.

                    [FileSearch.Directories]
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

                if (!Directory.Exists(Path.GetDirectoryName(config)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(config)!);
                }

                File.WriteAllText(config, text);
            }

            AddGamePathsToConfig(game.GameInstallFolder, game.ModsFolderPath, game.MapsFolderPath, config);

            FixRoute66Files(game, campaign);
        }

        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon mod)
        {
            sb.Append($@" -savedir ""{Path.Combine(PathToPortFolder, "Save", mod.Title.Replace(' ', '_'))}""");

            if (game is BloodGame bGame)
            {
                GetBloodArgs(sb, bGame, mod);
            }
            else if (game is DukeGame dGame)
            {
                GetDukeArgs(sb, dGame, mod);
            }
            else if (game is WangGame wGame)
            {
                GetWangArgs(sb, wGame, mod);
            }
            else if (game is SlaveGame sGame)
            {
                GetSlaveArgs(sb, sGame, mod);
            }
            else if (game is RedneckGame rGame)
            {
                GetRedneckArgs(sb, rGame, mod);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {mod} for game {game} is not supported");
            }
        }

        /// <inheritdoc/>
        protected override void GetAutoloadModsArgs(StringBuilder sb, IGame game, IAddon campaign)
        {
            var addons = game.GetAutoloadMods(true);

            if (addons.Count == 0)
            {
                return;
            }

            foreach (var mod in addons)
            {
                if (mod.Value is not AutoloadMod aMod)
                {
                    continue;
                }

                if (!ValidateAutoloadMod(aMod, campaign))
                {
                    continue;
                }

                sb.Append($@" -file ""{aMod.FileName}""");
            }
        }

        /// <inheritdoc/>
        protected override void GetSkipIntroParameter(StringBuilder sb) => sb.Append(" -quick");

        /// <inheritdoc/>
        protected override void GetSkipStartupParameter(StringBuilder sb) => sb.Append(" -nosetup");


        private void GetDukeArgs(StringBuilder sb, DukeGame game, IAddon camp)
        {
            if (camp is not DukeCampaign dMod)
            {
                ThrowHelper.NotImplementedException($"Mod type {camp} for game {game} is not supported");
                return;
            }

            sb.Append($" -addon {(byte)dMod.RequiredAddonEnum}");

            if (camp.Id == DukeAddonEnum.WorldTour.ToString())
            {
                var config = Path.Combine(PathToPortFolder, ConfigFile);
                AddGamePathsToConfig(game.DukeWTInstallPath, game.ModsFolderPath, game.MapsFolderPath, config);

                return;
            }

            if (camp.FileName is null)
            {
                return;
            }

            if (camp.Type is ModTypeEnum.TC)
            {
                //TODO
                //sb.Append($@" -file ""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}"" -con ""{camp.StartupFile}""");
            }
            else if (camp.Type is ModTypeEnum.Map)
            {
                GetMapArgs(sb, game, camp);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {camp.Type} is not supported");
                return;
            }
        }


        private void GetBloodArgs(StringBuilder sb, BloodGame game, IAddon addon)
        {
            if (addon is not BloodCampaign bMod)
            {
                ThrowHelper.NotImplementedException($"Mod type {addon} for game {game} is not supported");
                return;
            }

            var ini = bMod.INI;

            if (ini is null)
            {
                if (bMod.Dependencies?.TryGetValue(BloodAddonEnum.BloodCP.ToString(), out var _) ?? false)
                {
                    ini = Consts.CrypticIni;
                }
            }

            sb.Append(@$" -ini ""{ini}""");

            if (bMod.FileName is null)
            {
                return;
            }

            if (bMod.Type is ModTypeEnum.TC)
            {
                sb.Append($@" -file ""{Path.Combine(game.CampaignsFolderPath, bMod.FileName)}""");
            }
            else if (bMod.Type is ModTypeEnum.Map)
            {
                GetMapArgs(sb, game, bMod);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {bMod.Type} is not supported");
                return;
            }
        }

        private static void GetWangArgs(StringBuilder sb, WangGame game, IAddon camp)
        {
            if (camp.FileName is null)
            {
                if (camp.Id == WangAddonEnum.WangWD.ToString())
                {
                    sb.Append(@" -file WT.GRP");
                }
                else if (camp.Id == WangAddonEnum.WangTD.ToString())
                {
                    sb.Append(@" -file TD.GRP");
                }

                return;
            }

            if (camp.Type is ModTypeEnum.TC)
            {
                sb.Append($@" -file ""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}""");
            }
            else if (camp.Type is ModTypeEnum.Map)
            {
                GetMapArgs(sb, game, camp);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {camp.Type} is not supported");
                return;
            }
        }

        private void GetRedneckArgs(StringBuilder sb, RedneckGame game, IAddon camp)
        {
            if (camp.Id == RedneckAddonEnum.RedneckR66.ToString())
            {
                sb.Append(" -route66");
                return;
            }

            if (camp.Id == GameEnum.RedneckRA.ToString())
            {
                var config = Path.Combine(PathToPortFolder, ConfigFile);
                AddGamePathsToConfig(game.AgainInstallPath, game.ModsFolderPath, game.MapsFolderPath, config);
            }

            if (camp.FileName is null)
            {
                return;
            }

            if (camp.Type is ModTypeEnum.TC)
            {
                sb.Append($@" -file ""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}""");
            }
            else if (camp.Type is ModTypeEnum.Map)
            {
                GetMapArgs(sb, game, camp);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {camp.Type} is not supported");
                return;
            }
        }

        private static void GetSlaveArgs(StringBuilder sb, SlaveGame game, IAddon camp)
        {
            if (camp.FileName is null)
            {
                return;
            }

            if (camp.Type is ModTypeEnum.TC)
            {
                sb.Append($@" -file ""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}""");
            }
            else if (camp.Type is ModTypeEnum.Map)
            {
                GetMapArgs(sb, game, camp);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {camp.Type} is not supported");
                return;
            }
        }

        /// <summary>
        /// Get startup args for packed and loose maps
        /// </summary>
        private static void GetMapArgs(StringBuilder sb, BaseGame game, IAddon camp)
        {
            //TODO
            //if (camp.IsLoose)
            //{
            //    sb.Append($@" -file ""{Path.Combine(game.MapsFolderPath, camp.StartupFile!)}"" -map ""{camp.StartupFile}""");
            //}
            //else
            //{
            //    sb.Append($@" -file ""{Path.Combine(game.MapsFolderPath, camp.FileName!)}"" -map ""{camp.StartupFile}""");
            //}
        }

        /// <summary>
        /// Remove route 66 art files overrides
        /// </summary>
        private static void FixRoute66Files(IGame game, IAddon _)
        {
            if (game is RedneckGame)
            {
                var tilesA2 = Path.Combine(game.GameInstallFolder!, "TILES024.ART");
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
        }

        /// <summary>
        /// Add paths to game and mods folder to the config
        /// </summary>
        [Obsolete("Remove if this ever implemented https://github.com/ZDoom/Raze/issues/1060")]
        private static void AddGamePathsToConfig(string gameFolder, string modsFolder, string mapsFolder, string config)
        {
            var contents = File.ReadAllLines(config);

            StringBuilder sb = new(contents.Length);

            for (var i = 0; i < contents.Length; i++)
            {
                if (contents[i].Equals("[GameSearch.Directories]"))
                {
                    sb.AppendLine(contents[i]);

                    var path = gameFolder.Replace('\\', '/');
                    sb.AppendLine("Path=" + path);

                    do
                    {
                        i++;
                    }
                    while (!string.IsNullOrWhiteSpace(contents[i]));

                    sb.AppendLine();
                    continue;
                }

                if (contents[i].Equals("[FileSearch.Directories]"))
                {
                    sb.AppendLine(contents[i]);

                    var path = gameFolder.Replace('\\', '/');
                    sb.AppendLine("Path=" + path);
                    path = modsFolder.Replace('\\', '/');
                    sb.AppendLine("Path=" + path);

                    do
                    {
                        i++;
                    }
                    while (!string.IsNullOrWhiteSpace(contents[i]));

                    sb.AppendLine();
                    continue;
                }

                sb.AppendLine(contents[i]);
            }

            var result = sb.ToString();
            File.WriteAllText(config, result);
        }
    }
}
