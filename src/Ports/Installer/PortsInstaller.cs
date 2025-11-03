using Common.All.Enums;
using Common.Client.Interfaces;
using Common.Client.Tools;
using Microsoft.Extensions.Logging;
using Ports.Ports;
using Ports.Providers;

namespace Ports.Installer;

public sealed class PortsInstallerFactory(
    IApiInterface apiInterface,
    FilesDownloader filesDownloader,
    ArchiveTools archiveTools,
    ILogger logger,
    InstalledPortsProvider portsProvider
    )
{
    /// <summary>
    /// Create <see cref="PortsInstaller"/> instance
    /// </summary>
    public PortsInstaller Create(PortEnum portEnum) => new(portEnum, apiInterface, filesDownloader, archiveTools, portsProvider, logger);
}

public sealed class PortsInstaller
{
    private readonly PortEnum _portEnum;
    private readonly InstalledPortsProvider _portsProvider;
    private readonly FilesDownloader _filesDownloader;
    private readonly ArchiveTools _archiveTools;
    private readonly IApiInterface _apiInterface;

    /// <summary>
    /// Installation progress
    /// </summary>
    public Progress<float> Progress { get; init; } = new();

    public PortsInstaller(
        PortEnum portEnum,
        IApiInterface apiInterface,
        FilesDownloader filesDownloader,
        ArchiveTools archiveTools,
        InstalledPortsProvider portsProvider,
        ILogger logger
        )
    {
        _portEnum = portEnum;
        _portsProvider = portsProvider;
        _filesDownloader = filesDownloader;
        _archiveTools = archiveTools;
        _apiInterface = apiInterface;
    }

    /// <summary>
    /// Install port
    /// </summary>
    public async Task InstallAsync()
    {
        try
        {
            _filesDownloader.ProgressChanged += OnProgressChanged;
            _archiveTools.ProgressChanged += OnProgressChanged;

            var port = _portsProvider.GetPort(_portEnum);

            var release = await _apiInterface.GetLatestPortReleaseAsync(port.PortEnum).ConfigureAwait(false);

            if (release?.DownloadUrl is null)
            {
                return;
            }

            var fileName = Path.GetFileName(release.DownloadUrl.ToString());

            _ = await _filesDownloader.DownloadFileAsync(release.DownloadUrl, fileName, CancellationToken.None).ConfigureAwait(false);

            await _archiveTools.UnpackArchiveAsync(fileName, port.PortInstallFolderPath, port.PortEnum is PortEnum.DosBox).ConfigureAwait(false);

            File.Delete(fileName);

            if (port is not Raze)
            {
                File.WriteAllText(Path.Combine(port.PortInstallFolderPath, "version"), release.Version);
            }

            if (port is DosBox)
            {
                File.WriteAllText(Path.Combine(port.PortInstallFolderPath, "dosbox-staging.conf"),
                    """
                [dosbox]
                memsize = 64
                
                """);
            }
        }
        finally
        {
            _filesDownloader.ProgressChanged -= OnProgressChanged;
            _archiveTools.ProgressChanged -= OnProgressChanged;
        }
    }

    public void Uninstall()
    {
        var port = _portsProvider.GetPort(_portEnum);
        Directory.Delete(port.PortInstallFolderPath, true);
    }

    private void OnProgressChanged(object? sender, float e)
    {
        ((IProgress<float>)Progress).Report(e);
    }
}
