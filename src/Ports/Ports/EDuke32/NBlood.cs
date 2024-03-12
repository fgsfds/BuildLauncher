using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
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
        public override string ConfigFile => "nblood.cfg";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames => [GameEnum.Blood];

        /// <inheritdoc/>
        public override Uri RepoUrl => new("https://api.github.com/repos/nukeykt/NBlood/releases");

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => static x => x.FileName.StartsWith("nblood_win64");

        /// <summary>
        /// No need to do anything
        /// </summary>
        protected override void BeforeStart(IGame game, IMod campaign) { }

        /// <inheritdoc/>
        protected override void GetStartCampaignArgs(StringBuilder sb, IGame game, IMod mod)
        {
            mod.ThrowIfNotType<BloodCampaign>(out var bloodCamp);

            var ini = bloodCamp.StartupFile;

            if (bloodCamp.ModType is ModTypeEnum.Map)
            {
                if (bloodCamp.AddonEnum is BloodAddonEnum.Blood)
                {
                    ini = Consts.BloodIni;
                }
                else if (bloodCamp.AddonEnum is BloodAddonEnum.Cryptic)
                {
                    ini = Consts.CrypticIni;
                }
            }

            sb.Append($@" -usecwd -nosetup -j ""{game.GameInstallFolder}"" -g BLOOD.RFF -ini ""{ini}""");

            if (bloodCamp.FileName is null)
            {
                return;
            }

            if (bloodCamp.ModType is ModTypeEnum.Campaign)
            {
                sb.Append($@" -g ""{Path.Combine(game.CampaignsFolderPath, bloodCamp.FileName)}""");
            }
            else if (bloodCamp.ModType is ModTypeEnum.Map)
            {
                sb.Append($@" -g ""{Path.Combine(game.MapsFolderPath, bloodCamp.FileName)}"" -map ""{bloodCamp.StartupFile}""");
            }
            else
            {
                ThrowHelper.NotImplementedException($"Mod type {bloodCamp.ModType} is not supported");
                return;
            }
        }
    }
}
