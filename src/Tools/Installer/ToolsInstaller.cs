using Common.Tools;
using Tools.Tools;

namespace Tools.Installer
{
    public sealed class ToolsInstallerFactory(ToolsReleasesProvider toolsReleasesProvider)
    {
        /// <summary>
        /// Create <see cref="ToolsInstaller"/> instance
        /// </summary>
        /// <returns></returns>
        public ToolsInstaller Create() => new(toolsReleasesProvider);
    }

    public sealed class ToolsInstaller
    {
        private readonly ArchiveTools _fileTools;
        private readonly ToolsReleasesProvider _toolsReleasesProvider;

        public ToolsInstaller(ToolsReleasesProvider toolsReleasesProvider)
        {
            _fileTools = new();
            _toolsReleasesProvider = toolsReleasesProvider;
            Progress = _fileTools.Progress;
        }

        /// <summary>
        /// Installation progress
        /// </summary>
        public Progress<float> Progress { get; init; }

        /// <summary>
        /// Install tool
        /// </summary>
        /// <param name="port">Port</param>
        public async Task InstallAsync(BaseTool port)
        {
            var release = await _toolsReleasesProvider.GetLatestReleaseAsync(port).ConfigureAwait(false);

            if (release is null)
            {
                return;
            }

            await _fileTools.DownloadFileAsync(new Uri(release.Url), Path.GetFileName(release.Url)).ConfigureAwait(false);

            await _fileTools.UnpackArchiveAsync(Path.GetFileName(release.Url), port.PathToExecutableFolder).ConfigureAwait(false);

            File.Delete(Path.GetFileName(release.Url));

            File.WriteAllText(Path.Combine(port.PathToExecutableFolder, "version"), release.Version);
        }
    }
}
