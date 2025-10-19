using Addons.Providers;
using Avalonia.Desktop.Misc;
using Avalonia.Threading;
using Common.All.Enums;
using Common.Client;
using Common.Client.Interfaces;
using Common.Client.Providers;
using Common.Client.Tools;
using Games.Providers;
using Microsoft.Extensions.Logging;
using Ports.Installer;
using Ports.Ports;
using Ports.Providers;
using Tools.Installer;

namespace Avalonia.Desktop.ViewModels;

public sealed class ViewModelsFactory
{
    private readonly InstalledGamesProvider _gamesProvider;
    private readonly IConfigProvider _config;
    private readonly PortsInstallerFactory _portsInstallerFactory;
    private readonly InstalledPortsProvider _portsProvider;
    private readonly PlaytimeProvider _playtimeProvider;
    private readonly PortsReleasesProvider _portsReleasesProvider;
    private readonly ToolsReleasesProvider _toolsReleasesProvider;
    private readonly RatingProvider _ratingProvider;
    private readonly InstalledAddonsProviderFactory _installedAddonsProviderFactory;
    private readonly DownloadableAddonsProviderFactory _downloadableAddonsProviderFactory;
    private readonly PortStarter _portStarter;
    private readonly FilesUploader _filesUploader;
    private readonly AppUpdateInstaller _appUpdateInstaller;
    private readonly GamesPathsProvider _gamesPathsProvider;
    private readonly BitmapsCache _bitmapsCache;
    private readonly ILogger _logger;

    public ViewModelsFactory(
        InstalledGamesProvider gamesProvider,
        IConfigProvider IConfigProvider,
        PortsInstallerFactory portsInstallerFactory,
        InstalledPortsProvider portsProvider,
        PlaytimeProvider playtimeProvider,
        PortsReleasesProvider portsReleasesProvider,
        ToolsReleasesProvider toolsReleasesProvider,
        RatingProvider ratingProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory downloadableAddonsProviderFactory,
        PortStarter portStarter,
        FilesUploader filesUploader,
        AppUpdateInstaller appUpdateInstaller,
        GamesPathsProvider gamesPathsProvider,
        BitmapsCache bitmapsCache,
        ILogger logger
        )
    {
        _gamesProvider = gamesProvider;
        _config = IConfigProvider;
        _portsInstallerFactory = portsInstallerFactory;
        _portsProvider = portsProvider;
        _playtimeProvider = playtimeProvider;
        _portsReleasesProvider = portsReleasesProvider;
        _toolsReleasesProvider = toolsReleasesProvider;
        _ratingProvider = ratingProvider;
        _installedAddonsProviderFactory = installedAddonsProviderFactory;
        _downloadableAddonsProviderFactory = downloadableAddonsProviderFactory;
        _portStarter = portStarter;
        _filesUploader = filesUploader;
        _appUpdateInstaller = appUpdateInstaller;
        _gamesPathsProvider = gamesPathsProvider;
        _bitmapsCache = bitmapsCache;
        _logger = logger;
    }

#pragma warning disable CS0618 // Type or member is obsolete

    public MainWindowViewModel GetMainWindowViewModel()
    {
        MainWindowViewModel vm = new(
            _config,
            _filesUploader,
            _gamesProvider,
            _portsProvider,
            _appUpdateInstaller,
            this,
            _gamesPathsProvider,
            _logger
            );

        return vm;
    }

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
            _bitmapsCache,
            _logger
            );

        _ = Dispatcher.UIThread.Invoke(vm.InitializeAsync);
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
            _bitmapsCache,
            _logger
            );

        _ = Dispatcher.UIThread.Invoke(vm.InitializeAsync);
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
            _downloadableAddonsProviderFactory,
            _bitmapsCache
            );

        _ = Dispatcher.UIThread.Invoke(vm.InitializeAsync);
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

        _ = Dispatcher.UIThread.Invoke(vm.InitializeAsync);
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

        _ = Dispatcher.UIThread.Invoke(vm.InitializeAsync);
        return vm;
    }

#pragma warning restore CS0618 // Type or member is obsolete
}
