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
            GameEnum.RedneckRampage,
            GameEnum.RidesAgain,
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
            mod.ThrowIfNotType<DukeCampaign>(out var dukeCamp);
            game.ThrowIfNotType<DukeGame>(out var dukeGame);

            sb.Append($@" -usecwd -nosetup");

            if (dukeCamp.AddonEnum is DukeAddonEnum.Duke64)
            {
                sb.Append(@$" -j {Path.GetDirectoryName(dukeGame.Duke64RomPath)} -gamegrp ""{Path.GetFileName(dukeGame.Duke64RomPath)}""");
                return;
            }

            sb.Append($@" -j ""{game.GameInstallFolder}"" -addon {(byte)dukeCamp.AddonEnum}");

            if (dukeCamp.FileName is null)
            {
                return;
            }

            if (dukeCamp.ModType is ModTypeEnum.Campaign)
            {
                sb.Append($@" -g ""{Path.Combine(game.CampaignsFolderPath, dukeCamp.FileName)}"" -x ""{dukeCamp.StartupFile}""");
            }
            else if (dukeCamp.ModType is ModTypeEnum.Map)
            {
                sb.Append($@" -g ""{Path.Combine(game.MapsFolderPath, dukeCamp.FileName)}"" -map ""{dukeCamp.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {dukeCamp.ModType} is not supported");
                return;
            }
        }
    }
}
