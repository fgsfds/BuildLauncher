using Common.Client.Interfaces;
using Common.Client.Tools;
using Games.Providers;
using Microsoft.Extensions.Logging;
using Tools.Tools;

namespace Tools.Installer;

public sealed class ToolInstallerFactory(
    IApiInterface apiInterface,
    InstalledGamesProvider gamesProvider,
    FilesDownloader filesDownloader,
    ArchiveTools archiveTools,
    ILogger logger
    ) : IInstallerFactory<BaseTool, ToolInstaller>
{
    /// <summary>
    /// Create <see cref="ToolInstaller"/> instance
    /// </summary>
    public ToolInstaller Create(BaseTool tool) => new(tool, apiInterface, gamesProvider, filesDownloader, archiveTools, logger);
}
