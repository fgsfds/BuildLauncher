using Common.Tools;
using Ports.Ports;
using Ports.Providers;

namespace Ports.Tools
{
    public sealed class PortsInstallerFactory
    {
        /// <summary>
        /// Create <see cref="PortsInstaller"/> instance
        /// </summary>
        /// <returns></returns>
        public PortsInstaller Create() => new();
    }

    public sealed class PortsInstaller
    {
        private readonly ArchiveTools _fileTools;

        public PortsInstaller()
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
        public async Task InstallAsync(BasePort port)
        {
            var release = await PortsReleasesProvider.GetLatestRelease(port);

            if (release is null)
            {
                return;
            }

            await _fileTools.DownloadFileAsync(new Uri(release.Url), Path.GetFileName(release.Url));

            await _fileTools.UnpackArchiveAsync(Path.GetFileName(release.Url), port.FolderPath);

            File.Delete(Path.GetFileName(release.Url));

            if (port is not Raze)
            {
                File.WriteAllText(Path.Combine(port.FolderPath, "version"), release.Version.ToString());
            }
        }
    }
}
