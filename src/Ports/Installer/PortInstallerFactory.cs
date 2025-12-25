using Common.Client.Interfaces;
using Common.Client.Tools;
using Microsoft.Extensions.Logging;
using Ports.Ports;

namespace Ports.Installer;

public sealed class PortInstallerFactory(
    IApiInterface apiInterface,
    FilesDownloader filesDownloader,
    ArchiveTools archiveTools,
    ILogger logger
    ) : IInstallerFactory<BasePort, PortInstaller>
{
    /// <summary>
    /// Create <see cref="PortInstaller"/> instance
    /// </summary>
    public PortInstaller Create(BasePort port) => new(port, apiInterface, filesDownloader, archiveTools, logger);
}
