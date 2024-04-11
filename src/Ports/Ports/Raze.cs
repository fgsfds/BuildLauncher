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
            GameEnum.Exhumed,
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
        protected override string AddDirectoryParam => "-j ";

        /// <inheritdoc/>
        protected override string AddFileParam => "-file ";

        /// <inheritdoc/>
        protected override string AddDefParam => "-adddef ";

        /// <inheritdoc/>
        protected override string AddConParam => "-addcon ";

        /// <inheritdoc/>
        protected override string MainDefParam => "-def ";

        /// <inheritdoc/>
        protected override string MainConParam => "-con ";

        /// <inheritdoc/>
        protected override void GetSkipIntroParameter(StringBuilder sb) => sb.Append(" -quick");

        /// <inheritdoc/>
        protected override void GetSkipStartupParameter(StringBuilder sb) => sb.Append(" -nosetup");


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

            if (game is DukeGame dGame && mod is DukeCampaign dMod)
            {
                GetDukeArgs(sb, dGame, dMod);
            }
            else if (game is BloodGame bGame && mod is BloodCampaign bCamp)
            {
                GetBloodArgs(sb, bGame, bCamp);
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

        private void GetDukeArgs(StringBuilder sb, DukeGame game, DukeCampaign dMod)
        {
            sb.Append($" -addon {(byte)dMod.RequiredAddonEnum}");

            if (dMod.Id == DukeAddonEnum.WorldTour.ToString())
            {
                var config = Path.Combine(PathToPortFolder, ConfigFile);
                AddGamePathsToConfig(game.DukeWTInstallPath, game.ModsFolderPath, game.MapsFolderPath, config);

                return;
            }

            if (dMod.FileName is null)
            {
                return;
            }

            if (dMod.Type is ModTypeEnum.TC)
            {
                //TODO
                //sb.Append($@" -file ""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}"" -con ""{camp.StartupFile}""");
            }
            else if (dMod.Type is ModTypeEnum.Map)
            {
                GetMapArgs(sb, game, dMod);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {dMod.Type} is not supported");
                return;
            }
        }

        protected void GetBloodArgs(StringBuilder sb, BloodGame game, BloodCampaign bMod)
        {
            if (bMod.INI is not null)
            {
                sb.Append($@" -ini ""{bMod.INI}""");
            }
            else if (bMod.RequiredAddonEnum is BloodAddonEnum.BloodCP)
            {
                sb.Append($@" -ini ""{Consts.CrypticIni}""");
            }


            if (bMod.FileName is null)
            {
                return;
            }


            if (bMod.RFF is not null)
            {
                sb.Append($@" -rff {bMod.RFF}");
            }


            if (bMod.SND is not null)
            {
                sb.Append($@" -snd {bMod.SND}");
            }


            if (bMod.MainDef is not null)
            {
                sb.Append($@" {MainDefParam}""{bMod.MainDef}""");
            }
            else
            {
                //overriding default def so gamename.def files are ignored
                sb.Append($@" {MainDefParam}a");
            }


            if (bMod.Type is ModTypeEnum.TC)
            {
                sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, bMod.FileName)}""");
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
        private void GetWangArgs(StringBuilder sb, WangGame game, IAddon camp)
        {
            if (camp.FileName is null)
            {
                if (camp.Id == WangAddonEnum.WangWD.ToString())
                {
                    sb.Append($" {AddFileParam}WT.GRP");
                }
                else if (camp.Id == WangAddonEnum.WangTD.ToString())
                {
                    sb.Append($" {AddFileParam}TD.GRP");
                }

                return;
            }

            if (camp.Type is ModTypeEnum.TC)
            {
                sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}""");
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
                sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}""");
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

        private void GetSlaveArgs(StringBuilder sb, SlaveGame game, IAddon camp)
        {
            if (camp.FileName is null)
            {
                return;
            }

            if (camp.Type is ModTypeEnum.TC)
            {
                sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}""");
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

                if (!ValidateAutoloadMod(aMod, campaign, addons))
                {
                    continue;
                }

                sb.Append($@" {AddFileParam}""{aMod.FileName}""");

                foreach (var def in aMod.AdditionalDefs)
                {
                    sb.Append($@" {AddDefParam}""{def}""");
                }
            }
        }

        /// <summary>
        /// Remove route 66 art files overrides used for RedNukem
        /// </summary>
        private void FixRoute66Files(IGame game, IAddon _)
        {
            if (game is RedneckGame)
            {
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
