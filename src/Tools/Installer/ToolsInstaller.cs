using Common.Tools;
using Tools.Tools;

namespace Tools.Installer
{
    public sealed class ToolsInstallerFactory
    {
        /// <summary>
        /// Create <see cref="ToolsInstaller"/> instance
        /// </summary>
        /// <returns></returns>
        public ToolsInstaller Create() => new();
    }

    public sealed class ToolsInstaller
    {
        private readonly ArchiveTools _fileTools;

        public ToolsInstaller()
        {
            _fileTools = new();
            Progress = _fileTools.Progress;
        }

        /// <summary>
        /// Installation progress
        /// </summary>
        public Progress<float> Progress { get; init; }

        /// <summary>
        /// Install port
        /// </summary>
        /// <param name="port">Port</param>
        public async Task InstallAsync(BaseTool port)
        {
            var release = await ToolsReleasesProvider.GetLatestReleaseAsync(port).ConfigureAwait(false);

            if (release is null)
            {
                return;
            }

            await _fileTools.DownloadFileAsync(new Uri(release.Url), Path.GetFileName(release.Url)).ConfigureAwait(false);

            await _fileTools.UnpackArchiveAsync(Path.GetFileName(release.Url), port.PathToToolFolder).ConfigureAwait(false);

            File.Delete(Path.GetFileName(release.Url));

            File.WriteAllText(Path.Combine(port.PathToToolFolder, "version"), release.Version.ToString());
        }
    }
}
