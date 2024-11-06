using Addons.Providers;
using Common.Client.Interfaces;
using Common.Client.Providers;
using Common.Enums;
using Games.Providers;
using Microsoft.Extensions.Logging;
using Ports.Installer;
using Ports.Ports;
using Ports.Providers;
using Tools.Installer;
using Tools.Providers;

namespace Avalonia.Desktop.ViewModels;

public sealed class ViewModelsFactory
{
    private readonly GamesProvider _gamesProvider;
    private readonly IConfigProvider _config;
    private readonly PortsInstallerFactory _portsInstallerFactory;
    private readonly ToolsInstallerFactory _toolsInstallerFactory;
    private readonly PortsProvider _portsProvider;
    private readonly ToolsProvider _toolsProvider;
    private readonly PlaytimeProvider _playtimeProvider;
    private readonly PortsReleasesProvider _portsReleasesProvider;
    private readonly ToolsReleasesProvider _toolsReleasesProvider;
    private readonly RatingProvider _ratingProvider;
    private readonly InstalledAddonsProviderFactory _installedAddonsProviderFactory;
    private readonly DownloadableAddonsProviderFactory _downloadableAddonsProviderFactory;
    private readonly PortStarter _portStarter;
    private readonly ILogger _logger;

    public ViewModelsFactory(
        GamesProvider gamesProvider,
        IConfigProvider IConfigProvider,
        PortsInstallerFactory portsInstallerFactory,
        ToolsInstallerFactory toolsInstallerFactory,
        PortsProvider portsProvider,
        ToolsProvider toolsProvider,
        PlaytimeProvider playtimeProvider,
        PortsReleasesProvider portsReleasesProvider,
        ToolsReleasesProvider toolsReleasesProvider,
        RatingProvider ratingProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory downloadableAddonsProviderFactory,
        PortStarter portStarter,
        ILogger logger
        )
    {
        _gamesProvider = gamesProvider;
        _config = IConfigProvider;
        _portsInstallerFactory = portsInstallerFactory;
        _toolsInstallerFactory = toolsInstallerFactory;
        _portsProvider = portsProvider;
        _toolsProvider = toolsProvider;
        _playtimeProvider = playtimeProvider;
        _portsReleasesProvider = portsReleasesProvider;
        _toolsReleasesProvider = toolsReleasesProvider;
        _ratingProvider = ratingProvider;
        _installedAddonsProviderFactory = installedAddonsProviderFactory;
        _downloadableAddonsProviderFactory = downloadableAddonsProviderFactory;
        _portStarter = portStarter;
        _logger = logger;
    }

#pragma warning disable CS0618 // Type or member is obsolete
    /// <summary>
    /// Create <see cref="CampaignsViewModel"/>
    /// </summary>
    /// <param name="gameEnum">Game enum</param>
    public CampaignsViewModel GetCampaignsViewModel(GameEnum gameEnum)
    {
        CampaignsViewModel vm = new(
            _gamesProvider.GetGame(gameEnum),
            _gamesProvider,
            _config,
            _playtimeProvider,
            _ratingProvider,
            _installedAddonsProviderFactory,
            _downloadableAddonsProviderFactory,
            _portStarter,
            _logger
            );

        _ = Task.Run(vm.InitializeAsync);
        return vm;
    }


    /// <summary>
    /// Create <see cref="CampaignsViewModel"/>
    /// </summary>
    /// <param name="gameEnum">Game enum</param>
    public MapsViewModel GetMapsViewModel(GameEnum gameEnum)
    {
        MapsViewModel vm = new(
            _gamesProvider.GetGame(gameEnum),
            _gamesProvider,
            _config,
            _playtimeProvider,
            _ratingProvider,
            _installedAddonsProviderFactory,
            _downloadableAddonsProviderFactory,
            _portStarter,
            _logger
            );

        _ = Task.Run(vm.InitializeAsync);
        return vm;
    }

    /// <summary>
    /// Create <see cref="CampaignsViewModel"/>
    /// </summary>
    /// <param name="gameEnum">Game enum</param>
    public ModsViewModel GetModsViewModel(GameEnum gameEnum)
    {
        ModsViewModel vm = new(
            _gamesProvider.GetGame(gameEnum),
            _gamesProvider,
            _playtimeProvider,
            _ratingProvider,
            _installedAddonsProviderFactory,
            _downloadableAddonsProviderFactory
            );

        _ = Task.Run(vm.InitializeAsync);
        return vm;
    }

    /// <summary>
    /// Create <see cref="CampaignsViewModel"/>
    /// </summary>
    /// <param name="gameEnum">Game enum</param>
    public DownloadsViewModel GetDownloadsViewModel(GameEnum gameEnum)
    {
        DownloadsViewModel vm = new(
            _gamesProvider.GetGame(gameEnum),
            _installedAddonsProviderFactory,
            _downloadableAddonsProviderFactory,
            _logger
            );

        _ = Task.Run(vm.InitializeAsync);
        return vm;
    }


    /// <summary>
    /// Create <see cref="PortViewModel"/>
    /// </summary>
    /// <param name="portEnum">Port enum</param>
    public PortViewModel GetPortViewModel(PortEnum portEnum)
    {
        PortViewModel vm = new(
            _portsInstallerFactory,
            _portsReleasesProvider,
            _portsProvider.GetPort(portEnum),
            _logger
            );

        _ = Task.Run(vm.InitializeAsync);
        return vm;
    }

#pragma warning restore CS0618 // Type or member is obsolete
}
