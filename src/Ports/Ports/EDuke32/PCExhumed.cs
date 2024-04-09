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
        public override List<GameEnum> SupportedGames => [GameEnum.Slave];

        /// <inheritdoc/>
        public override Uri RepoUrl => new("https://api.github.com/repos/nukeykt/NBlood/releases");

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => static x => x.FileName.StartsWith("pcexhumed_win64");


        /// <inheritdoc/>
        protected override string ConfigFile => "pcexhumed.cfg";


        /// <inheritdoc/>
        protected override void BeforeStart(IGame game, IAddon campaign)
        {
            game.CreateCombinedMod();
        }

        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IAddon mod)
        {
            if (game is not SlaveGame sGame)
            {
                ThrowHelper.NotImplementedException($"Mod type {mod} for game {game} is not supported");
                return;
            }

            sb.Append($@" -usecwd -nosetup");
            sb.Append(@$" -j ""{game.GameInstallFolder}""");

            if (mod.FileName is null)
            {
                return;
            }

            if (mod.Type is ModTypeEnum.TC)
            {
                //TODO
                //sb.Append($@" -g ""{Path.Combine(sGame.CampaignsFolderPath, mod.FileName)}"" -x ""{mod.StartupFile}""");
            }
            else if (mod.Type is ModTypeEnum.Map)
            {
                //TODO restore loose maps
                //if (mod.IsLoose)
                //{
                //    sb.Append($@" -j ""{Path.Combine(sGame.MapsFolderPath)}""");
                //}
                //else
                //{
                //    sb.Append($@" -g ""{Path.Combine(sGame.MapsFolderPath, mod.FileName)}""");
                //}

                //sb.Append($@" -map ""{mod.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {mod.Type} is not supported");
                return;
            }
        }
    }
}
