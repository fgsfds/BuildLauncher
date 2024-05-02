using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Ports.Providers;

namespace Ports.Ports.EDuke32
{
    /// <summary>
    /// NotBlood port
    /// </summary>
    public sealed class NotBlood : NBlood
    {
        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.NotBlood;

        /// <inheritdoc/>
        public override string Exe => "notblood.exe";

        /// <inheritdoc/>
        public override string Name => "NotBlood";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames => [GameEnum.Blood];

        /// <inheritdoc/>
        public override Uri RepoUrl => new("https://api.github.com/repos/NoOneBlood/xmapedit/releases");

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => static x => x.FileName.StartsWith("notblood-win64", StringComparison.CurrentCultureIgnoreCase);

        /// <inheritdoc/>
        protected override string ConfigFile => "notblood.cfg";


        /// <inheritdoc/>
        protected override void BeforeStart(IGame game, IAddon campaign)
        {
            //nothing to do
        }
    }
}
