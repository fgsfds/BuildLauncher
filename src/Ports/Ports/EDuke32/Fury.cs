using Common.Config;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Ports.Providers;

namespace Ports.Ports.EDuke32
{
    /// <summary>
    /// RedNukem port
    /// </summary>
    public sealed class Fury(ConfigEntity config) : EDuke32
    {
        private readonly ConfigEntity _config = config;

        /// <inheritdoc/>
        public override PortEnum PortEnum => PortEnum.Fury;

        /// <inheritdoc/>
        public override string Exe => "fury.exe";

        /// <inheritdoc/>
        public override string Name => "Fury";

        /// <inheritdoc/>
        public override List<GameEnum> SupportedGames => [GameEnum.Fury];

        /// <inheritdoc/>
        public override string PathToPortFolder => _config.GamePathFury ?? string.Empty;

        /// <inheritdoc/>
        public override bool IsInstalled => File.Exists(FullPathToExe);

        /// <inheritdoc/>
        protected override string ConfigFile => "fury.cfg";

        /// <inheritdoc/>
        public override Uri RepoUrl => ThrowHelper.NotImplementedException<Uri>();

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => ThrowHelper.NotImplementedException<Func<GitHubReleaseAsset, bool>>();


        /// <inheritdoc/>
        protected override void BeforeStart(IGame game, IAddon campaign)
        {
            FixGrpInConfig();
        }
    }
}
