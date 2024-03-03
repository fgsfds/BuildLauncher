using Common.Enums;
using Common.Interfaces;
using Mods.Mods;
using Ports.Providers;

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
        public override string GetStartCampaignArgs(IGame game, IMod mod)
        {
            var args = $@" -usecwd -nosetup -j ""{game.GameInstallFolder}""";

            if (mod is BloodCampaign campaign)
            {
                if (campaign.FileName is not null)
                {
                    args += $@" -g ""{Path.Combine(game.CampaignsFolderPath, campaign.FileName)}""";
                }

                args += $@" -ini ""{campaign.IniFile}""";
            }

            if (mod is SingleMap map)
            {
                args += $@" -map ""{Path.Combine(game.MapsFolderPath, map.FileName!)}""";
            }

            return args;
        }
    }
}
