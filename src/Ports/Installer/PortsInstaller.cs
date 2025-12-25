using Common.All.Serializable.Downloadable;
using Common.Client.Interfaces;
using Common.Client.Tools;
using Microsoft.Extensions.Logging;
using Ports.Ports;

namespace Ports.Installer;

public sealed class PortsInstallerFactory(
    IApiInterface apiInterface,
    FilesDownloader filesDownloader,
    ArchiveTools archiveTools,
    ILogger logger
    ) : IInstallerFactory<BasePort, PortsInstaller>
{
    /// <summary>
    /// Create <see cref="PortsInstaller"/> instance
    /// </summary>
    public PortsInstaller Create(BasePort portEnum) => new(portEnum, apiInterface, filesDownloader, archiveTools, logger);
}

public sealed class PortsInstaller : InstallerBase<BasePort>
{
    private readonly IApiInterface _apiInterface;

    public PortsInstaller(
        BasePort portEnum,
        IApiInterface apiInterface,
        FilesDownloader filesDownloader,
        ArchiveTools archiveTools,
        ILogger logger
        ) : base(portEnum, filesDownloader, archiveTools)
    {
        _apiInterface = apiInterface;
    }

    /// <summary>
    /// Install port
    /// </summary>
    protected override void InstallInternal()
    {
        if (_instance is DosBox)
        {
            File.WriteAllText(Path.Combine(_instance.InstallFolderPath, "dosbox-staging.conf"),
                """
                [dosbox]
                memsize = 64
                
                """);
        }
    }

    public override void Uninstall()
    {
        Directory.Delete(_instance.InstallFolderPath, true);
    }

    public override Task<GeneralReleaseJsonModel?> GetRelease() => _apiInterface.GetLatestPortReleaseAsync(_instance.PortEnum);
}
