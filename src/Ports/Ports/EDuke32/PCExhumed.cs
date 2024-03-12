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
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IMod mod)
        {
            if (game is SlaveGame && mod is SlaveCampaign sMod)
            {
                sb.Append($@" -usecwd -nosetup");
                sb.Append(@$" -j {game.GameInstallFolder}");
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {mod} for game {game} is not supported");
                return;
            }

            if (sMod.FileName is null)
            {
                return;
            }

            if (sMod.ModType is ModTypeEnum.Campaign)
            {
                sb.Append($@" -g ""{Path.Combine(game.CampaignsFolderPath, sMod.FileName)}"" -x ""{sMod.StartupFile}""");
            }
            else if (sMod.ModType is ModTypeEnum.Map)
            {
                sb.Append($@" -g ""{Path.Combine(game.MapsFolderPath, sMod.FileName)}"" -map ""{sMod.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {sMod.ModType} is not supported");
                return;
            }
        }

        /// <summary>
        /// No need to do anything
        /// </summary>
        protected override void BeforeStart(IGame game, IMod campaign) { }
    }
}
