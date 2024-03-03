using Common.Enums;
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

        /// <inheritdoc/>
        public override void BeforeStart(IGame game) { }

        /// <inheritdoc/>
        public override void GetStartCampaignArgs(StringBuilder sb, IGame game, IMod mod)
        {
            sb.Append($@" -usecwd -nosetup -j ""{game.GameInstallFolder}""");

            if (mod is BloodCampaign campaign)
            {
                if (campaign.FileName is not null)
                {
                    sb.Append($@" -g ""{Path.Combine(game.CampaignsFolderPath, campaign.FileName)}""");
                }

                sb.Append($@" -ini ""{campaign.IniFile}""");
            }

            if (mod is SingleMap map)
            {
                sb.Append($@" -map ""{Path.Combine(game.MapsFolderPath, map.FileName!)}""");
            }

            return;
        }
    }
}
