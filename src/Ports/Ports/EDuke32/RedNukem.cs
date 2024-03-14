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
    /// RedNukem port
    /// </summary>
    public sealed class RedNukem : EDuke32
    {
        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.RedNukem;

        /// <inheritdoc/>
        public override string Exe => "rednukem.exe";

        /// <inheritdoc/>
        public override string Name => "RedNukem";

        /// <inheritdoc/>
        public override string ConfigFile => "rednukem.cfg";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames =>
            [
            GameEnum.Duke3D,
            GameEnum.Redneck,
            GameEnum.Again,
            GameEnum.NAM,
            GameEnum.WWIIGI
            ];

        /// <inheritdoc/>
        public override Uri RepoUrl => new("https://api.github.com/repos/nukeykt/NBlood/releases");

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => static x => x.FileName.StartsWith("rednukem_win64");


        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IMod mod)
        {
            if (game is DukeGame dGame && mod is DukeCampaign dCamp)
            {
                GetDukeArgs(sb, dGame, dCamp);
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

        /// <summary>
        /// Get startup agrs for Redneck Rampage
        /// </summary>
        /// <param name="sb">StringBuilder</param>
        /// <param name="game">RedneckGame</param>
        /// <param name="camp">RedneckCampaign</param>
        private static void GetRedneckArgs(StringBuilder sb, RedneckGame game, RedneckCampaign camp)
        {
            sb.Append($@" -usecwd -nosetup");

            if (camp.AddonEnum is RedneckAddonEnum.Again)
            {
                sb.Append($@" -j ""{game.AgainInstallPath}""");
            }
            else if (camp.AddonEnum is RedneckAddonEnum.Route66)
            {
                sb.Append($@" -j ""{game.GameInstallFolder}"" -x GAME66.CON");
            }
            else
            {
                sb.Append($@" -j ""{game.GameInstallFolder}""");
            }

            if (camp.FileName is null)
            {
                return;
            }

            if (camp.ModType is ModTypeEnum.Campaign)
            {
                sb.Append($@" -g ""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}"" -x ""{camp.StartupFile}""");
            }
            else if (camp.ModType is ModTypeEnum.Map)
            {
                sb.Append($@" -g ""{Path.Combine(game.MapsFolderPath, camp.FileName)}"" -map ""{camp.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {camp.ModType} is not supported");
                return;
            }
        }
    }
}
