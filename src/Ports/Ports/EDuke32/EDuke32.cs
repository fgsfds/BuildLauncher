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
        protected override void BeforeStart(IGame game, IAddon campaign)
        {
            FixGrpInConfig();

            //TODO
            //game.CreateCombinedMod(campaign.DefFileContents);
        }

        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon mod)
        {
            if (game is DukeGame dGame)
            {
                GetDukeArgs(sb, dGame, mod);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {mod.Type} for game {game} is not supported");
            }
        }

        /// <summary>
        /// Get startup agrs for Duke
        /// </summary>
        /// <param name="sb">StringBuilder</param>
        /// <param name="game">DukeGame</param>
        /// <param name="camp">DukeCampaign</param>
        protected static void GetDukeArgs(StringBuilder sb, DukeGame game, IAddon camp)
        {
            //TODO
            //sb.Append($@" -usecwd");

            //if (camp.Id == DukeAddonEnum.WorldTour.ToString())
            //{
            //    sb.Append($@" -addon {(byte)DukeAddonEnum.Duke3D} -j ""{game.DukeWTInstallPath}"" -j ""{Path.Combine(game.SpecialFolderPath, Consts.WTStopgap)}"" -gamegrp e32wt.grp");
            //    return;
            //}

            //if (camp.Id == GameEnum.Duke64.ToString())
            //{
            //    sb.Append(@$" -j ""{Path.GetDirectoryName(game.Duke64RomPath)}"" -gamegrp ""{Path.GetFileName(game.Duke64RomPath)}""");
            //    return;
            //}

            //sb.Append($@" -j ""{game.GameInstallFolder}"" -addon {(byte)camp.AddonEnum}");

            //if (camp.FileName is null)
            //{
            //    return;
            //}

            //if (camp.Type is ModTypeEnum.TC)
            //{
            //    sb.Append($@" -g ""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}"" -x ""{camp.StartupFile}""");
            //}
            //else if (camp.Type is ModTypeEnum.Map)
            //{
            //    if (camp.IsLoose)
            //    {
            //        sb.Append($@" -j ""{Path.Combine(game.MapsFolderPath)}""");
            //    }
            //    else
            //    {
            //        sb.Append($@" -g ""{Path.Combine(game.MapsFolderPath, camp.FileName)}""");
            //    }

            //    sb.Append($@" -map ""{camp.StartupFile}""");
            //}
            //else
            //{
            //    ThrowHelper.NotImplementedException($"Mod type {camp.Type} is not supported");
            //    return;
            //}
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

                if (!ValidateAutoloadMod(aMod, campaign))
                {
                    continue;
                }

                sb.Append($@" {AddFileParam}""{aMod.FileName}""");
            }

            sb.Append($@" {AddDirectoryParam}""{Path.Combine(game.SpecialFolderPath, Consts.CombinedModFolder)}"" {AddDefParam}""{Consts.CombinedDef}""");
        }

        /// <inheritdoc/>
        protected override void GetSkipIntroParameter(StringBuilder sb) => sb.Append(" -quick");

        /// <inheritdoc/>
        protected override void GetSkipStartupParameter(StringBuilder sb) => sb.Append(" -nosetup");


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
