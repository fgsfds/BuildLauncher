using Common.Enums;
using Common.Interfaces;
using Ports.Providers;
using System.Collections.Immutable;
using System.Text;

namespace Ports.Ports
{
    /// <summary>
    /// BuildGDX port
    /// </summary>
    public sealed class BuildGDX : BasePort
    {
        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.BuildGDX;

        /// <inheritdoc/>
        public override string Exe => string.Empty;

        /// <inheritdoc/>
        public override string Name => "BuildGDX";

        /// <inheritdoc/>
        public override string ConfigFile => string.Empty;

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames =>
            [
            GameEnum.Blood,
            GameEnum.Duke3D,
            GameEnum.Wang,
            GameEnum.Powerslave,
            GameEnum.RedneckRampage,
            GameEnum.RidesAgain,
            GameEnum.NAM,
            GameEnum.WWIIGI,
            GameEnum.Witchaven,
            GameEnum.Witchaven2,
            GameEnum.TekWar
            ];

        /// <inheritdoc/>
        public override Uri RepoUrl => throw new NotImplementedException();

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => throw new NotImplementedException();

        public override int? InstalledVersion => null;

        /// <inheritdoc/>
        public override void GetAutoloadModsArgs(StringBuilder sb, IGame provider, ImmutableList<IMod> mods) => throw new NotImplementedException();

        /// <inheritdoc/>
        public override void GetStartCampaignArgs(StringBuilder sb, IGame game, IMod mod) => throw new NotImplementedException();

        /// <inheritdoc/>
        public override void GetSkipIntroParameter(StringBuilder sb) => throw new NotImplementedException();
    }
}
