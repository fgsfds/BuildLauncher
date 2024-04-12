using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Mods;
using Ports.Providers;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

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
            sb.Append($@" -savedir ""{Path.Combine(PathToPortFolder, "Save", mod.Id.Replace(' ', '_'))}""");

            if (mod.MainDef is not null)
            {
                sb.Append($@" {MainDefParam}""{mod.MainDef}""");
            }
            else
            {
                //overriding default def so gamename.def files are ignored
                sb.Append($@" {MainDefParam}""a""");
            }


            if (game is DukeGame dGame && mod is DukeCampaign dMod)
            {
                GetDukeArgs(sb, dGame, dMod);
            }
            else if (game is BloodGame bGame && mod is BloodCampaign bCamp)
            {
                GetBloodArgs(sb, bGame, bCamp);
            }
            else if (game is WangGame wGame && mod is WangCampaign wCamp)
            {
                GetWangArgs(sb, wGame, wCamp);
            }
            else if (game is SlaveGame sGame && mod is SlaveCampaign sCamp)
            {
                GetSlaveArgs(sb, sGame, sCamp);
            }
            else if (game is RedneckGame rGame && mod is RedneckCampaign rCamp)
            {
                GetRedneckArgs(sb, rGame, rCamp);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {mod} for game {game} is not supported");
            }
        }

        private void GetDukeArgs(StringBuilder sb, DukeGame game, DukeCampaign dMod)
        {
            if (dMod.RequiredAddonEnum is DukeAddonEnum.DukeWT)
            {
                var config = Path.Combine(PathToPortFolder, ConfigFile);
                AddGamePathsToConfig(game.DukeWTInstallPath, game.ModsFolderPath, game.MapsFolderPath, config);

                sb.Append($" -addon {(byte)DukeAddonEnum.Duke3D}");
            }
            else
            {
                sb.Append($@" -addon {(byte)dMod.RequiredAddonEnum}");
            }


            if (dMod.FileName is null)
            {
                return;
            }


            if (dMod.Type is AddonTypeEnum.TC)
            {
                sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, dMod.FileName)}""");

                if (dMod.MainCon is not null)
                {
                    sb.Append($@" {MainConParam}""{dMod.MainCon}""");
                }

                if (dMod.AdditionalCons?.Count > 0)
                {
                    foreach (var con in dMod.AdditionalCons)
                    {
                        sb.Append($@" {AddConParam}""{con}""");
                    }
                }
            }
            else if (dMod.Type is AddonTypeEnum.Map)
            {
                GetMapArgs(sb, game, dMod);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {dMod.Type} is not supported");
                return;
            }
        }

        private void GetWangArgs(StringBuilder sb, WangGame game, WangCampaign camp)
        {
            if (camp.RequiredAddonEnum is WangAddonEnum.WangWD)
            {
                sb.Append($" {AddFileParam}WT.GRP");
            }
            else if (camp.RequiredAddonEnum is WangAddonEnum.WangTD)
            {
                sb.Append($" {AddFileParam}TD.GRP");
            }


            if (camp.FileName is null)
            {
                return;
            }


            if (camp.Type is AddonTypeEnum.TC)
            {
                sb.Append($@" {AddDirectoryParam}""{game.CampaignsFolderPath}"" {AddFileParam}""{camp.FileName}""");
            }
            else if (camp.Type is AddonTypeEnum.Map)
            {
                GetMapArgs(sb, game, camp);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {camp.Type} is not supported");
                return;
            }
        }

        private void GetRedneckArgs(StringBuilder sb, RedneckGame game, RedneckCampaign camp)
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


            if (camp.Type is AddonTypeEnum.TC)
            {
                sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}""");

                if (camp.MainCon is not null)
                {
                    sb.Append($@" {MainConParam}""{camp.MainCon}""");
                }

                if (camp.AdditionalCons?.Count > 0)
                {
                    foreach (var con in camp.AdditionalCons)
                    {
                        sb.Append($@" {AddConParam}""{con}""");
                    }
                }
            }
            else if (camp.Type is AddonTypeEnum.Map)
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

                if (aMod.AdditionalDefs is not null)
                {
                    foreach (var def in aMod.AdditionalDefs)
                    {
                        sb.Append($@" {AddDefParam}""{def}""");
                    }
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
