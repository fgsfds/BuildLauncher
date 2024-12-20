using Common.Client.Tools;
using Microsoft.Extensions.Logging;
using Ports.Ports;
using Ports.Providers;

namespace Ports.Installer;

public sealed class PortsInstallerFactory(
    PortsReleasesProvider portsReleasesProvider,
    HttpClient httpClient,
    ILogger logger
    )
{
    /// <summary>
    /// Create <see cref="PortsInstaller"/> instance
    /// </summary>
    public PortsInstaller Create() => new(portsReleasesProvider, httpClient, logger);
}

public sealed class PortsInstaller
{
    private readonly ArchiveTools _fileTools;
    private readonly PortsReleasesProvider _portsReleasesProvider;

    public PortsInstaller(
        PortsReleasesProvider portsReleasesProvider,
        HttpClient httpClient,
        ILogger logger
        )
    {
        _fileTools = new(httpClient, logger);
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

        if (release?.DownloadUrl is null)
        {
            return;
        }

        var fileName = Path.GetFileName(release.DownloadUrl.ToString());

        _ = await _fileTools.DownloadFileAsync(release.DownloadUrl, fileName, CancellationToken.None).ConfigureAwait(false);

        await _fileTools.UnpackArchiveAsync(fileName, port.PortInstallFolderPath).ConfigureAwait(false);

        File.Delete(fileName);

        if (port is not Raze)
        {
            File.WriteAllText(Path.Combine(port.PortInstallFolderPath, "version"), release.Version);
        }
    }
}
