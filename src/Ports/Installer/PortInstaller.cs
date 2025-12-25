using Common.All.Enums;
using Common.All.Serializable.Downloadable;
using Common.Client;
using Common.Client.Interfaces;
using Common.Client.Tools;
using Microsoft.Extensions.Logging;
using Ports.Ports;

namespace Ports.Installer;

public sealed class PortInstaller : InstallerBase<BasePort>
{
    private readonly IApiInterface _apiInterface;

    public PortInstaller(
        BasePort port,
        IApiInterface apiInterface,
        FilesDownloader filesDownloader,
        ArchiveTools archiveTools,
        ILogger logger
        ) : base(port, filesDownloader, archiveTools)
    {
        _apiInterface = apiInterface;
    }

    /// <summary>
    /// Install port
    /// </summary>
    protected override void InstallInternal()
    {
        if (_instance.PortEnum is PortEnum.DosBox)
        {
            File.WriteAllText(Path.Combine(_instance.InstallFolderPath, "dosbox-staging.conf"),
                """
                [dosbox]
                memsize = 64
                
                """);
        }
    }

    public override void Uninstall() => Directory.Delete(_instance.InstallFolderPath, true);

    public override Task<GeneralReleaseJsonModel?> GetRelease() => _apiInterface.GetLatestPortReleaseAsync(_instance.PortEnum);
}
