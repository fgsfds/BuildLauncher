using Common.Helpers;

namespace Tools.Tools
{
    public sealed class XMapEdit : BaseTool
    {
        public override string Exe => "xmapedit.exe";

        public override string Name => "XMAPEDIT";

        public override string? InstalledVersion
        {
            get
            {
                var versionFile = Path.Combine(PathToToolFolder, "version");

                if (!File.Exists(versionFile))
                {
                    return null;
                }

                return File.ReadAllText(versionFile);
            }
        }

        public override Uri RepoUrl => new("https://api.github.com/repos/NoOneBlood/xmapedit/releases");

        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => static x => x.FileName.EndsWith("x64.zip", StringComparison.InvariantCultureIgnoreCase);
    }
}
