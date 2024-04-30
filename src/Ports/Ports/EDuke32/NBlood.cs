using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
using Ports.Providers;
using System.Text;

namespace Ports.Ports.EDuke32
{
    /// <summary>
    /// NBlood port
    /// </summary>
    public class NBlood : EDuke32
    {
        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.NBlood;

        /// <inheritdoc/>
        public override string Exe => "nblood.exe";

        /// <inheritdoc/>
        public override string Name => "NBlood";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames => [GameEnum.Blood];

        /// <inheritdoc/>
        public override Uri RepoUrl => new("https://api.github.com/repos/nukeykt/NBlood/releases");

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => static x => x.FileName.StartsWith("nblood_win64");

        /// <inheritdoc/>
        protected override string ConfigFile => "nblood.cfg";


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

            if (mod.AdditionalDefs is not null)
            {
                foreach (var def in mod.AdditionalDefs)
                {
                    sb.Append($@" {AddDefParam}""{def}""");
                }
            }



            if (game is BloodGame bGame && mod is BloodCampaign bMod)
            {
                GetBloodArgs(sb, bGame, bMod);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {mod.Type} for game {game} is not supported");
            }
        }
    }
}
