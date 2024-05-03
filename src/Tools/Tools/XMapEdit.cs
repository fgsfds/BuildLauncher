using Common.Releases;
using Games.Providers;

namespace Tools.Tools
{
    public sealed class XMapEdit : BaseTool
    {
        private readonly GamesProvider _gamesProvider;

        public override string Exe => "xmapedit.exe";

        public override string Name => "XMAPEDIT";

        public override Uri RepoUrl => new("https://api.github.com/repos/NoOneBlood/xmapedit/releases");

        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => static x => x.FileName.EndsWith("x64.zip", StringComparison.InvariantCultureIgnoreCase);

        public override string PathToExecutableFolder => _gamesProvider.GetGame(Common.Enums.GameEnum.Blood).GameInstallFolder ?? string.Empty;

        public override bool CanBeLaunched => _gamesProvider.IsBloodInstalled;

        public XMapEdit(GamesProvider gamesProvider)
        {
            _gamesProvider = gamesProvider;
        }

        public override string GetStartToolArgs() => string.Empty;
    }
}
