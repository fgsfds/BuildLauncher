using Common.Enums;
using Common.Enums.Addons;
using Common.Interfaces;
using Games.Games;
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
        public override List<GameEnum> SupportedGames =>
            [
            GameEnum.Duke3D,
            GameEnum.Redneck,
            GameEnum.RedneckRA,
            GameEnum.NAM,
            GameEnum.WWIIGI
            ];

        /// <inheritdoc/>
        public override Uri RepoUrl => new("https://api.github.com/repos/nukeykt/NBlood/releases");

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => static x => x.FileName.StartsWith("rednukem_win64");

        /// <inheritdoc/>
        protected override string ConfigFile => "rednukem.cfg";


        /// <inheritdoc/>
        protected override void BeforeStart(IGame game, IAddon campaign)
        {
            FixGrpInConfig();

            FixRoute66Files(game, campaign);
        }


        /// <summary>
        /// Override original art files with route 66's ones or remove overrides
        /// </summary>
        [Obsolete("Remove if RedNukem can ever properly launch R66")]
        private static void FixRoute66Files(IGame game, IAddon campaign)
        {
            if (game is not RedneckGame rGame || !rGame.IsRoute66Installed)
            {
                return;
            }

            var tilesA1 = Path.Combine(game.GameInstallFolder, "TILESA66.ART");
            var tilesA2 = Path.Combine(game.GameInstallFolder, "TILES024.ART");

            var tilesB1 = Path.Combine(game.GameInstallFolder, "TILESB66.ART");
            var tilesB2 = Path.Combine(game.GameInstallFolder, "TILES025.ART");

            var turdMovAnm1 = Path.Combine(game.GameInstallFolder, "TURD66.ANM");
            var turdMovAnm2 = Path.Combine(game.GameInstallFolder, "TURDMOV.ANM");

            var turdMovVoc1 = Path.Combine(game.GameInstallFolder, "TURD66.VOC");
            var turdMovVoc2 = Path.Combine(game.GameInstallFolder, "TURDMOV.VOC");

            var endMovAnm1 = Path.Combine(game.GameInstallFolder, "END66.ANM");
            var endMovAnm2 = Path.Combine(game.GameInstallFolder, "RR_OUTRO.ANM");

            var endMovVoc1 = Path.Combine(game.GameInstallFolder, "END66.VOC");
            var endMovVoc2 = Path.Combine(game.GameInstallFolder, "LN_FINAL.VOC");


            if (campaign.Id == RedneckAddonEnum.RedneckR66.ToString())
            {
                File.Copy(tilesA1, tilesA2, true);
                File.Copy(tilesB1, tilesB2, true);
                File.Copy(turdMovAnm1, turdMovAnm2, true);
                File.Copy(turdMovVoc1, turdMovVoc2, true);
                File.Copy(endMovAnm1, endMovAnm2, true);
                File.Copy(endMovVoc1, endMovVoc2, true);
            }
            else
            {
                if (File.Exists(tilesA2))
                {
                    File.Delete(tilesA2);
                }

                if (File.Exists(tilesB2))
                {
                    File.Delete(tilesB2);
                }

                if (File.Exists(turdMovAnm2))
                {
                    File.Delete(turdMovAnm2);
                }

                if (File.Exists(turdMovVoc2))
                {
                    File.Delete(turdMovVoc2);
                }

                if (File.Exists(endMovAnm2))
                {
                    File.Delete(endMovAnm2);
                }

                if (File.Exists(endMovVoc2))
                {
                    File.Delete(endMovVoc2);
                }
            }
        }
    }
}
