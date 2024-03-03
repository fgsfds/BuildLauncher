using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Mods;
using Ports.Providers;

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
        public override string GetStartCampaignArgs(IGame game, IMod mod)
        {
            var args = $@" -usecwd -nosetup";

            if (mod is not DukeCampaign dukeCamp ||
                game is not DukeGame dukeGame)
            {
                ThrowHelper.ArgumentException();
                return null;
            }

            if (dukeCamp.AddonEnum is DukeAddonEnum.Duke64)
            {
                return args += @$" -j {Path.GetDirectoryName(dukeGame.Duke64RomPath)} -gamegrp ""{Path.GetFileName(dukeGame.Duke64RomPath)}""";
            }

            if (dukeCamp.FileName is null)
            {
                args += $@" -j ""{game.GameInstallFolder}"" -addon {(byte)dukeCamp.AddonEnum}";
            }
            else
            {
                args += $@" -j ""{game.GameInstallFolder}"" -addon {(byte)dukeCamp.AddonEnum} -g ""{Path.Combine(game.CampaignsFolderPath, dukeCamp.FileName)}""";
            }

            if (dukeCamp.ConFile is not null)
            {
                args += $@" -con {dukeCamp.ConFile}";
            }

            return args;
        }
    }
}
