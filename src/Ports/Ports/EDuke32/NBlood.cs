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
        protected override void BeforeStart(IGame game, IMod campaign)
        {
            game.CreateCombinedMod();
        }

        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IMod mod)
        {
            if (game is not BloodGame bGame || mod is not BloodCampaign bCamp)
            {
                ThrowHelper.NotImplementedException($"Mod type {mod} for game {game} is not supported");
                return;
            }

            var ini = bCamp.StartupFile;

            if (bCamp.ModType is ModTypeEnum.Map)
            {
                if (bCamp.AddonEnum is BloodAddonEnum.Blood)
                {
                    ini = Consts.BloodIni;
                }
                else if (bCamp.AddonEnum is BloodAddonEnum.Cryptic)
                {
                    ini = Consts.CrypticIni;
                }
            }

            sb.Append($@" -usecwd -nosetup -j ""{bGame.GameInstallFolder}"" -g BLOOD.RFF -ini ""{ini}""");

            if (bCamp.FileName is null)
            {
                return;
            }

            if (bCamp.ModType is ModTypeEnum.Campaign)
            {
                sb.Append($@" -g ""{Path.Combine(bGame.CampaignsFolderPath, bCamp.FileName)}""");
            }
            else if (bCamp.ModType is ModTypeEnum.Map)
            {
                if (bCamp.IsLoose)
                {
                    sb.Append($@" -j ""{Path.Combine(game.MapsFolderPath)}""");
                }
                else
                {
                    sb.Append($@" -g ""{Path.Combine(game.MapsFolderPath, bCamp.FileName)}""");
                }

                sb.Append($@" -map ""{bCamp.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {bCamp.ModType} is not supported");
                return;
            }
        }
    }
}
