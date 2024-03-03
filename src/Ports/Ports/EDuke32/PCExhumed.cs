using Common.Enums;
using Common.Interfaces;
using Ports.Providers;

namespace Ports.Ports.EDuke32
{
    /// <summary>
    /// PCExhumed port
    /// </summary>
    public sealed class PCExhumed : EDuke32
    {
        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.PCExhumed;

        /// <inheritdoc/>
        public override string Exe => "pcexhumed.exe";

        /// <inheritdoc/>
        public override string Name => "PCExhumed";

        /// <inheritdoc/>
        public override string ConfigFile => "pcexhumed.cfg";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames => [GameEnum.Powerslave];

        /// <inheritdoc/>
        public override Uri RepoUrl => new("https://api.github.com/repos/nukeykt/NBlood/releases");

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => static x => x.FileName.StartsWith("pcexhumed_win64");

        /// <inheritdoc/>
        public override void BeforeStart(IGame game) { }
    }
}
