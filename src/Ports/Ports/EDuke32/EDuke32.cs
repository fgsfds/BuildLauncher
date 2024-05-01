using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
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
        protected override string AddGrpParam => "-grp ";

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
        public override string? InstalledVersion
        {
            get
            {
                var versionFile = Path.Combine(PathToPortFolder, "version");

                if (!File.Exists(versionFile))
                {
                    return null;
                }

                return File.ReadAllText(versionFile);
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
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon addon)
        {
            //don't search for steam/gog installs
            sb.Append($@" -usecwd");

            if (addon.MainDef is not null)
            {
                sb.Append($@" {MainDefParam}""{addon.MainDef}""");
            }
            else
            {
                //overriding default def so gamename.def files are ignored
                sb.Append($@" {MainDefParam}""a""");
            }

            if (addon.AdditionalDefs is not null)
            {
                foreach (var def in addon.AdditionalDefs)
                {
                    sb.Append($@" {AddDefParam}""{def}""");
                }
            }


            if (game is DukeGame dGame && addon is DukeCampaign dCamp)
            {
                GetDukeArgs(sb, dGame, dCamp);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {addon.Type} for game {game} is not supported");
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
            if (camp.Id == nameof(GameEnum.Duke64))
            {
                sb.Append(@$" {AddDirectoryParam}""{Path.GetDirectoryName(game.Duke64RomPath)}"" -gamegrp ""{Path.GetFileName(game.Duke64RomPath)}""");
                return;
            }

            if (camp.RequiredAddonEnum is DukeAddonEnum.DukeWT)
            {
                sb.Append($@" {AddDirectoryParam}""{game.DukeWTInstallPath}"" -addon {(byte)DukeAddonEnum.Duke3D} {AddDirectoryParam}""{Path.Combine(game.SpecialFolderPath, Consts.WTStopgap)}"" -gamegrp e32wt.grp");
            }
            else
            {
                sb.Append($@" {AddDirectoryParam}""{game.GameInstallFolder}"" -addon {(byte)camp.RequiredAddonEnum}");
            }


            if (camp.FileName is null)
            {
                return;
            }


            if (camp.Type is AddonTypeEnum.TC)
            {
                sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}""");

                if (camp.GRPs is not null)
                {
                    foreach (var grp in camp.GRPs)
                    {
                        sb.Append($@" {AddGrpParam}""{grp}""");
                    }
                }

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
