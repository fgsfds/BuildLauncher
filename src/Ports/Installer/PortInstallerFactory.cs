using Core.Client.Interfaces;
using Core.Client.Tools;
using Microsoft.Extensions.Logging;
using Ports.Ports;

namespace Ports.Installer;

public sealed class PortInstallerFactory(
    IApiInterface apiInterface,
    FilesDownloader filesDownloader,
    ArchiveTools archiveTools,
    ILoggerFactory loggerFactory
    ) : IInstallerFactory<BasePort, PortInstaller>
{
    /// <summary>
    ///     Create <see cref="PortInstaller" /> instance
    /// </summary>
    public PortInstaller Create(BasePort port) => new(
        port,
        apiInterface,
        filesDownloader,
        archiveTools,
        loggerFactory.CreateLogger<PortInstaller>()
        );
}
