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
        public override string ConfigFile => "pcexhumed.cfg";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames => [GameEnum.Slave];

        /// <inheritdoc/>
        public override Uri RepoUrl => new("https://api.github.com/repos/nukeykt/NBlood/releases");

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => static x => x.FileName.StartsWith("pcexhumed_win64");


        /// <inheritdoc/>
        protected override void BeforeStart(IGame game, IMod campaign)
        {
            game.CreateCombinedMod();
        }

        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IMod mod)
        {
            if (game is not SlaveGame sGame || mod is not SlaveCampaign sCamp)
            {
                ThrowHelper.NotImplementedException($"Mod type {mod} for game {game} is not supported");
                return;
            }

            sb.Append($@" -usecwd -nosetup");
            sb.Append(@$" -j {game.GameInstallFolder}");

            if (sCamp.FileName is null)
            {
                return;
            }

            if (sCamp.ModType is ModTypeEnum.Campaign)
            {
                sb.Append($@" -g ""{Path.Combine(sGame.CampaignsFolderPath, sCamp.FileName)}"" -x ""{sCamp.StartupFile}""");
            }
            else if (sCamp.ModType is ModTypeEnum.Map)
            {
                if (sCamp.IsLoose)
                {
                    sb.Append($@" -j ""{Path.Combine(sGame.MapsFolderPath)}""");
                }
                else
                {
                    sb.Append($@" -g ""{Path.Combine(sGame.MapsFolderPath, sCamp.FileName)}""");
                }

                sb.Append($@" -map ""{sCamp.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {sCamp.ModType} is not supported");
                return;
            }
        }
    }
}
