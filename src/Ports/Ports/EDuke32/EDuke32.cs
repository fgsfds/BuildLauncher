using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Mods;
using Ports.Providers;
using System.Text;

namespace Ports.Ports.EDuke32
{
    /// <summary>
    /// EDuke32 port
    /// </summary>
    public class EDuke32 : BasePort
    {
        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.EDuke32;

        /// <inheritdoc/>
        public override string Exe => "eduke32.exe";

        /// <inheritdoc/>
        public override string Name => "EDuke32";

        /// <inheritdoc/>
        protected override string ConfigFile => "eduke32.cfg";

        /// <inheritdoc/>
        protected override string AddDirectoryParam => "-j ";

        /// <inheritdoc/>
        protected override string AddFileParam => "-g ";

        /// <inheritdoc/>
        protected override string AddDefParam => "-mh ";

        /// <inheritdoc/>
        protected override string AddConParam => "-mx ";

        /// <inheritdoc/>
        protected override string MainDefParam => "-h ";

        /// <inheritdoc/>
        protected override string MainConParam => "-x ";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames =>
            [
            GameEnum.Duke3D,
            GameEnum.NAM,
            GameEnum.WWIIGI
            ];

        /// <inheritdoc/>
        public override Uri RepoUrl => new("https://dukeworld.com/eduke32/synthesis/latest/");

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => ThrowHelper.NotImplementedException<Func<GitHubReleaseAsset, bool>>();

        /// <inheritdoc/>
        public override int? InstalledVersion
        {
            get
            {
                var versionFile = Path.Combine(PathToPortFolder, "version");

                if (!File.Exists(versionFile))
                {
                    return null;
                }

                return int.Parse(File.ReadAllText(versionFile));
            }
        }

        /// <inheritdoc/>
        protected override void GetSkipIntroParameter(StringBuilder sb) => sb.Append(" -quick");

        /// <inheritdoc/>
        protected override void GetSkipStartupParameter(StringBuilder sb) => sb.Append(" -nosetup");


        /// <inheritdoc/>
        protected override void BeforeStart(IGame game, IAddon campaign)
        {
            FixGrpInConfig();
        }

        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon mod)
        {
            //don't search for steam/gog installs
            sb.Append($@" -usecwd");

            if (game is DukeGame dGame && mod is DukeCampaign dCamp)
            {
                GetDukeArgs(sb, dGame, dCamp);
            }
            else if (game is BloodGame bGame && mod is BloodCampaign bMod)
            {
                GetBloodArgs(sb, bGame, bMod);
            }
            else if (game is WangGame wGame && mod is WangCampaign wMod)
            {
                GetWangArgs(sb, wGame, wMod);
            }
            else if (game is SlaveGame sGame)
            {
                GetSlaveArgs(sb, sGame, mod);
            }
            else if (game is RedneckGame rGame && mod is RedneckCampaign rCamp)
            {
                GetRedneckArgs(sb, rGame, rCamp);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {mod.Type} for game {game} is not supported");
            }
        }


        private void GetSlaveArgs(StringBuilder sb, SlaveGame sGame, IAddon mod)
        {
            throw new NotImplementedException();
        }


        protected void GetBloodArgs(StringBuilder sb, BloodGame game, BloodCampaign bMod)
        {
            sb.Append(@$" {AddDirectoryParam}""{game.GameInstallFolder}""");


            if (bMod.INI is not null)
            {
                sb.Append($@" -ini ""{bMod.INI}""");
            }
            else if (bMod.RequiredAddonEnum is BloodAddonEnum.BloodCP)
            {
                sb.Append($@" -ini ""{Consts.CrypticIni}""");
            }


            if (bMod.MainDef is not null)
            {
                sb.Append($@" {MainDefParam}""{bMod.MainDef}""");
            }
            else
            {
                //overriding default def so gamename.def files are ignored
                sb.Append($@" {MainDefParam}""a""");
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

        /// <summary>
        /// Get startup agrs for Duke
        /// </summary>
        /// <param name="sb">StringBuilder</param>
        /// <param name="game">DukeGame</param>
        /// <param name="camp">DukeCampaign</param>
        protected void GetDukeArgs(StringBuilder sb, DukeGame game, DukeCampaign camp)
        {
            if (camp.Id == GameEnum.Duke64.ToString())
            {
                sb.Append(@$" {AddDirectoryParam}""{Path.GetDirectoryName(game.Duke64RomPath)}"" -gamegrp ""{Path.GetFileName(game.Duke64RomPath)}""");
                return;
            }

            if (camp.Id == DukeAddonEnum.WorldTour.ToString())
            {
                sb.Append($@" {AddDirectoryParam}""{game.DukeWTInstallPath}"" -addon {(byte)DukeAddonEnum.Duke3D} {AddDirectoryParam}""{Path.Combine(game.SpecialFolderPath, Consts.WTStopgap)}"" -gamegrp e32wt.grp");
            }
            else
            {
                sb.Append($@" {AddDirectoryParam}""{game.GameInstallFolder}"" -addon {(byte)camp.RequiredAddonEnum}");
            }


            if (camp.MainDef is not null)
            {
                sb.Append($@" {MainDefParam}""{camp.MainDef}""");
            }
            else
            {
                //overriding default def so gamename.def files are ignored
                sb.Append($@" {MainDefParam}""a""");
            }


            if (camp.FileName is null)
            {
                return;
            }


            if (camp.Type is ModTypeEnum.TC)
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
        /// Get startup agrs for Redneck Rampage
        /// </summary>
        /// <param name="sb">StringBuilder</param>
        /// <param name="game">RedneckGame</param>
        /// <param name="camp">RedneckCampaign</param>
        private void GetRedneckArgs(StringBuilder sb, RedneckGame game, RedneckCampaign camp)
        {
            if (camp.Id == GameEnum.RedneckRA.ToString())
            {
                sb.Append($@" -j ""{game.AgainInstallPath}""");
            }
            else if (camp.Id == RedneckAddonEnum.RedneckR66.ToString())
            {
                sb.Append($@" -j ""{game.GameInstallFolder}"" -x GAME66.CON");
            }
            else
            {
                sb.Append($@" -j ""{game.GameInstallFolder}""");
            }


            if (camp.MainDef is not null)
            {
                sb.Append($@" {MainDefParam}""{camp.MainDef}""");
            }
            else
            {
                //overriding default def so gamename.def files are ignored
                sb.Append($@" {MainDefParam}""a""");
            }


            if (camp.FileName is null)
            {
                return;
            }


            if (camp.Type is ModTypeEnum.TC)
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


        private void GetWangArgs(StringBuilder sb, WangGame wGame, WangCampaign wMod)
        {
            sb.Append($@" {AddDirectoryParam}""{wGame.GameInstallFolder}"" -addon{(byte)wMod.RequiredAddonEnum}");

            AddWangMusicFolder(sb, wGame);


            if (wMod.MainDef is not null)
            {
                sb.Append($@" {MainDefParam}""{wMod.MainDef}""");
            }
            else
            {
                //overriding default def so gamename.def files are ignored
                sb.Append($@" {MainDefParam}""a""");
            }


            if (wMod.FileName is null)
            {
                return;
            }


            if (wMod.Type is ModTypeEnum.TC)
            {
                sb.Append($@" {AddDirectoryParam}""{wGame.CampaignsFolderPath}"" {AddFileParam}""{wMod.FileName}""");
            }
            else if (wMod.Type is ModTypeEnum.Map)
            {
                GetMapArgs(sb, wGame, wMod);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {wMod.Type} is not supported");
                return;
            }
        }


        /// <summary>
        /// Add music folders to the search list if music files don't exist in the game directory
        /// </summary>
        private static void AddWangMusicFolder(StringBuilder sb, IGame game)
        {
            if (File.Exists(Path.Combine(game.GameInstallFolder, "track02.ogg")))
            {
                return;
            }

            var folder = Path.Combine(game.GameInstallFolder, "MUSIC");
            if (Directory.Exists(folder))
            {
                sb.Append(@$" -j""{folder}""");
                return;
            }

            folder = Path.Combine(game.GameInstallFolder, "classic", "MUSIC");
            if (Directory.Exists(folder))
            {
                sb.Append(@$" -j""{folder}""");
                return;
            }
        }


        /// <inheritdoc/>
        protected override void GetAutoloadModsArgs(StringBuilder sb, IGame game, IAddon campaign)
        {
            var mods = game.GetAutoloadMods(true);

            if (mods.Count == 0)
            {
                return;
            }

            sb.Append($@" {AddDirectoryParam}""{game.ModsFolderPath}""");

            foreach (var mod in mods)
            {
                if (mod.Value is not AutoloadMod aMod)
                {
                    continue;
                }

                if (!ValidateAutoloadMod(aMod, campaign, mods))
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
        /// Remove GRP files from the config
        /// </summary>
        protected void FixGrpInConfig()
        {
            var config = Path.Combine(PathToPortFolder, ConfigFile);

            if (!File.Exists(config))
            {
                return;
            }

            var contents = File.ReadAllLines(config);

            for (var i = 0; i < contents.Length; i++)
            {
                if (contents[i].StartsWith("SelectedGRP"))
                {
                    contents[i] = @"SelectedGRP = """"";
                    break;
                }
            }

            File.WriteAllLines(config, contents);
        }
    }
}
