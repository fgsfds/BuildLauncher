using Common.Releases;
using Games.Providers;

namespace Tools.Tools
{
    public sealed class XMapEdit : BaseTool
    {
        private readonly GamesProvider _gamesProvider;

        /// <inheritdoc/>
        public override string Exe => "xmapedit.exe";

        /// <inheritdoc/>
        public override string Name => "XMAPEDIT";

        /// <inheritdoc/>
        public override Uri RepoUrl => new("https://api.github.com/repos/NoOneBlood/xmapedit/releases");

        /// <inheritdoc/>
        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => static x => x.FileName.EndsWith("x64.zip", StringComparison.InvariantCultureIgnoreCase);

        /// <inheritdoc/>
        public override string PathToExecutableFolder => _gamesProvider.GetGame(Common.Enums.GameEnum.Blood).GameInstallFolder ?? string.Empty;

        /// <inheritdoc/>
        public override bool CanBeLaunched => _gamesProvider.IsBloodInstalled;


        public XMapEdit(GamesProvider gamesProvider)
        {
            _gamesProvider = gamesProvider;
        }


        /// <inheritdoc/>
        public override string GetStartToolArgs() => string.Empty;
    }
}
