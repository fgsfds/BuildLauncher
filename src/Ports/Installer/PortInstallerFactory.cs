using Core.Client.Interfaces;
using Core.Client.Tools;
using Microsoft.Extensions.Logging;
using Ports.Ports;

namespace Ports.Installer;

/// <summary>
///     Factory for creating <see cref="PortInstaller" /> instances.
/// </summary>
public sealed class PortInstallerFactory(
    IApiInterface apiInterface,
    FilesDownloader filesDownloader,
    ArchiveTools archiveTools,
    ILoggerFactory loggerFactory
    ) : IInstallerFactory<BasePort, PortInstaller>
{
    /// <summary>
    ///     Creates a <see cref="PortInstaller" /> instance for the specified port.
    /// </summary>
    /// <param name="port">The port to install.</param>
    public PortInstaller Create(BasePort port) => new(
        port,
        apiInterface,
        filesDownloader,
        archiveTools,
        loggerFactory.CreateLogger<PortInstaller>()
        );
}
