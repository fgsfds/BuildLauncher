using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Mods;
using Ports.Providers;
using System.Text;

namespace Ports.Ports.EDuke32
{
    /// <summary>
    /// PCExhumed port
    /// </summary>
    public sealed class PCExhumed : EDuke32
    {
        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.PCExhumed;

        /// <inheritdoc/>
        public override string Exe => "pcexhumed.exe";

        /// <inheritdoc/>
        public override string Name => "PCExhumed";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames => [GameEnum.Exhumed];

        /// <inheritdoc/>
        public override Uri RepoUrl => new("https://api.github.com/repos/nukeykt/NBlood/releases");

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => static x => x.FileName.StartsWith("pcexhumed_win64");

        /// <inheritdoc/>
        protected override string ConfigFile => "pcexhumed.cfg";


        /// <inheritdoc/>
        protected override void BeforeStart(IGame game, IAddon campaign)
        {
            //nothing to do
        }


        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon mod)
        {
            //don't search for steam/gog installs
            sb.Append($@" -usecwd");

            sb.Append(@$" {AddDirectoryParam}""{game.GameInstallFolder}""");

            if (mod.MainDef is not null)
            {
                sb.Append($@" {MainDefParam}""{mod.MainDef}""");
            }
            else
            {
                //overriding default def so gamename.def files are ignored
                sb.Append($@" {MainDefParam}""a""");
            }


            if (game is SlaveGame sGame && mod is SlaveCampaign sMod)
            {
                GetSlaveArgs(sb, sGame, sMod);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {mod.Type} for game {game} is not supported");
            }
        }
    }
}
