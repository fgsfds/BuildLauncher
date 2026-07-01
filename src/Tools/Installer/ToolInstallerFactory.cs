using Core.Client.Interfaces;
using Core.Client.Tools;
using Games.Providers;
using Microsoft.Extensions.Logging;
using Tools.Tools;

namespace Tools.Installer;

/// <summary>
///     Factory for creating <see cref="ToolInstaller" /> instances.
/// </summary>
public sealed class ToolInstallerFactory(
    IApiInterface apiInterface,
    InstalledGamesProvider gamesProvider,
    FilesDownloader filesDownloader,
    ArchiveTools archiveTools,
    ILoggerFactory loggerFactory
    ) : IInstallerFactory<BaseTool, ToolInstaller>
{
    /// <inheritdoc />
    public ToolInstaller Create(BaseTool tool) => new(
        tool,
        apiInterface,
        gamesProvider,
        filesDownloader,
        archiveTools,
        loggerFactory.CreateLogger<ToolInstaller>()
        );
}
