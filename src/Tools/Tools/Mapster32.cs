using Common.Config;
using Common.Helpers;
using Common.Releases;

namespace Tools.Tools
{
    public sealed class Mapster32 : BaseTool
    {
        private readonly ConfigEntity _config;

        public Mapster32(ConfigEntity config)
        {
            _config = config;
        }

        public override string Exe => "mapster32.exe";

        public override string Name => "Mapster32";

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

        public override Uri RepoUrl => null;

        public override Func<GitHubReleaseAsset, bool> WindowsReleasePredicate => null;

        public override string PathToExecutableFolder => Path.Combine(CommonProperties.PortsFolderPath, "EDuke32");

    }
}
