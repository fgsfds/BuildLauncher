using Common.Tools;
using Ports.Ports;

namespace Ports.Installer;

public sealed class PortsInstallerFactory(PortsReleasesProvider portsReleasesProvider)
{
    /// <summary>
    /// Create <see cref="PortsInstaller"/> instance
    /// </summary>
    /// <returns></returns>
    public PortsInstaller Create() => new(portsReleasesProvider);
}

public sealed class PortsInstaller
{
    private readonly ArchiveTools _fileTools;
    private readonly PortsReleasesProvider _portsReleasesProvider;

    public PortsInstaller(PortsReleasesProvider portsReleasesProvider)
    {
        _fileTools = new();
        _portsReleasesProvider = portsReleasesProvider;
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
        var release = await _portsReleasesProvider.GetLatestReleaseAsync(port.PortEnum).ConfigureAwait(false);

        if (release?.WindowsDownloadUrl is null)
        {
            return;
        }

        var fileName = Path.GetFileName(release.WindowsDownloadUrl.ToString());

        await _fileTools.DownloadFileAsync(release.WindowsDownloadUrl, fileName).ConfigureAwait(false);

        await _fileTools.UnpackArchiveAsync(fileName, port.PortExecutableFolderPath).ConfigureAwait(false);

        File.Delete(fileName);

        if (port is not Raze)
        {
            File.WriteAllText(Path.Combine(port.PortExecutableFolderPath, "version"), release.Version);
        }
    }
}
