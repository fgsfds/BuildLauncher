using Common.Config;
using Common.Releases;

namespace Tools.Tools
{
    public sealed class XMapEdit : BaseTool
    {
        private readonly ConfigEntity _config;

        public XMapEdit(ConfigEntity config)
        {
            _config = config;
        }

        public override string Exe => "xmapedit.exe";

        public override string Name => "XMAPEDIT";

        public override string? InstalledVersion
        {
            get
            {
                var versionFile = Path.Combine(PathToExecutableFolder, "version");

                if (!File.Exists(versionFile))
                {
                    return null;
                }

                return File.ReadAllText(versionFile);
            }
        }

        public override Uri RepoUrl => new("https://api.github.com/repos/NoOneBlood/xmapedit/releases");

        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => static x => x.FileName.EndsWith("x64.zip", StringComparison.InvariantCultureIgnoreCase);

        public override string PathToExecutableFolder => _config.GamePathBlood ?? string.Empty;

    }
}
