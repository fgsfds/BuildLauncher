using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Games.Games;
using Mods.Addons;
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
        public override List<GameEnum> SupportedGames =>
            [
            GameEnum.Duke3D,
            GameEnum.Redneck,
            GameEnum.RidesAgain,
            GameEnum.NAM,
            GameEnum.WWIIGI
            ];

        /// <inheritdoc/>
        public override List<FeatureEnum> SupportedFeatures => [];

        /// <inheritdoc/>
        protected override string ConfigFile => "rednukem.cfg";


        /// <inheritdoc/>
        protected override void BeforeStart(IGame game, IAddon campaign)
        {
            FixGrpInConfig();

            FixRoute66Files(game, campaign);
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
            else if (game is RedneckGame rGame && addon is RedneckCampaign rCamp)
            {
                GetRedneckArgs(sb, rGame, rCamp);
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {addon.Type} for game {game} is not supported");
            }
        }


        /// <summary>
        /// Get startup agrs for Redneck Rampage
        /// </summary>
        /// <param name="sb">StringBuilder</param>
        /// <param name="game">RedneckGame</param>
        /// <param name="camp">RedneckCampaign</param>
        private void GetRedneckArgs(StringBuilder sb, RedneckGame game, RedneckCampaign camp)
        {
            if (camp.Id.Equals(nameof(GameEnum.RidesAgain), StringComparison.OrdinalIgnoreCase))
            {
                sb.Append($@" {AddDirectoryParam}""{game.AgainInstallPath}""");
            }
            else if (camp.Id.Equals(nameof(RedneckAddonEnum.RedneckR66), StringComparison.OrdinalIgnoreCase))
            {
                sb.Append($@" {AddDirectoryParam}""{game.GameInstallFolder}"" -x GAME66.CON");
            }
            else
            {
                sb.Append($@" {AddDirectoryParam}""{game.GameInstallFolder}""");
            }


            if (camp.FileName is null)
            {
                return;
            }


            if (camp.Type is AddonTypeEnum.TC)
            {
                sb.Append($@" {AddFileParam}""{Path.Combine(game.CampaignsFolderPath, camp.FileName)}""");

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


            if (campaign.Id.Equals(nameof(RedneckAddonEnum.RedneckR66), StringComparison.OrdinalIgnoreCase))
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
