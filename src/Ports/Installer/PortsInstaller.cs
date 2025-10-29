using Common.All.Enums;
using Common.Client.Interfaces;
using Common.Client.Tools;
using Microsoft.Extensions.Logging;
using Ports.Ports;
using Ports.Providers;

namespace Ports.Installer;

public sealed class PortsInstallerFactory(
    IApiInterface apiInterface,
    HttpClient httpClient,
    ILogger logger,
    InstalledPortsProvider portsProvider
    )
{
    /// <summary>
    /// Create <see cref="PortsInstaller"/> instance
    /// </summary>
    public PortsInstaller Create(PortEnum portEnum) => new(portEnum, apiInterface, httpClient, portsProvider, logger);
}

public sealed class PortsInstaller
{
    private readonly PortEnum _portEnum;
    private readonly InstalledPortsProvider _portsProvider;
    private readonly ArchiveTools _fileTools;
    private readonly IApiInterface _apiInterface;

    public PortsInstaller(
        PortEnum portEnum,
        IApiInterface apiInterface,
        HttpClient httpClient,
        InstalledPortsProvider portsProvider,
        ILogger logger
        )
    {
        _portEnum = portEnum;
        _portsProvider = portsProvider;
        _fileTools = new(httpClient, logger);
        _apiInterface = apiInterface;
        Progress = _fileTools.Progress;
    }

    /// <summary>
    /// Installation progress
    /// </summary>
    public Progress<float> Progress { get; init; }

    /// <summary>
    /// Install port
    /// </summary>
    public async Task InstallAsync()
    {
        var port = _portsProvider.GetPort(_portEnum);

        var release = await _apiInterface.GetLatestPortReleaseAsync(port.PortEnum).ConfigureAwait(false);

        if (release?.DownloadUrl is null)
        {
            return;
        }

        var fileName = Path.GetFileName(release.DownloadUrl.ToString());

        _ = await _fileTools.DownloadFileAsync(release.DownloadUrl, fileName, CancellationToken.None).ConfigureAwait(false);

        await _fileTools.UnpackArchiveAsync(fileName, port.PortInstallFolderPath, port.PortEnum is PortEnum.DosBox).ConfigureAwait(false);

        File.Delete(fileName);

        if (port is not Raze)
        {
            File.WriteAllText(Path.Combine(port.PortInstallFolderPath, "version"), release.Version);
        }
    }

    public void Uninstall()
    {
        var port = _portsProvider.GetPort(_portEnum);
        Directory.Delete(port.PortInstallFolderPath, true);
    }
}
