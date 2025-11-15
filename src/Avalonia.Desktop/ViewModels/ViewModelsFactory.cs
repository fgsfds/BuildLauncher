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
using Tools.Providers;
using Tools.Tools;

namespace Avalonia.Desktop.ViewModels;

public sealed class ViewModelsFactory
{
    private readonly InstalledGamesProvider _gamesProvider;
    private readonly IConfigProvider _config;
    private readonly PortsInstallerFactory _portsInstallerFactory;
    private readonly ToolsInstallerFactory _toolsInstallerFactory;
    private readonly InstalledPortsProvider _portsProvider;
    private readonly InstalledToolsProvider _toolsProvider;
    private readonly PlaytimeProvider _playtimeProvider;
    private readonly IApiInterface _apiInterface;
    private readonly RatingProvider _ratingProvider;
    private readonly InstalledAddonsProviderFactory _installedAddonsProviderFactory;
    private readonly DownloadableAddonsProviderFactory _downloadableAddonsProviderFactory;
    private readonly PortStarter _portStarter;
    private readonly FilesUploader _filesUploader;
    private readonly AppUpdateInstaller _appUpdateInstaller;
    private readonly GamesPathsProvider _gamesPathsProvider;
    private readonly BitmapsCache _bitmapsCache;
    private readonly IEnumerable<BasePort> _ports;
    private readonly IEnumerable<BaseTool> _tools;
    private readonly ILogger _logger;

    public ViewModelsFactory(
        InstalledGamesProvider gamesProvider,
        IConfigProvider IConfigProvider,
        PortsInstallerFactory portsInstallerFactory,
        ToolsInstallerFactory toolsInstallerFactory,
        InstalledPortsProvider portsProvider,
        InstalledToolsProvider toolsProvider,
        PlaytimeProvider playtimeProvider,
        IApiInterface apiInterface,
        RatingProvider ratingProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory downloadableAddonsProviderFactory,
        PortStarter portStarter,
        FilesUploader filesUploader,
        AppUpdateInstaller appUpdateInstaller,
        GamesPathsProvider gamesPathsProvider,
        BitmapsCache bitmapsCache,
        IEnumerable<BasePort> ports,
        IEnumerable<BaseTool> tools,
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
        _apiInterface = apiInterface;
        _ratingProvider = ratingProvider;
        _installedAddonsProviderFactory = installedAddonsProviderFactory;
        _downloadableAddonsProviderFactory = downloadableAddonsProviderFactory;
        _portStarter = portStarter;
        _filesUploader = filesUploader;
        _appUpdateInstaller = appUpdateInstaller;
        _gamesPathsProvider = gamesPathsProvider;
        _bitmapsCache = bitmapsCache;
        _ports = ports;
        _tools = tools;
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
            _toolsProvider,
            _appUpdateInstaller,
            this,
            _gamesPathsProvider,
            _ports,
            _tools,
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
    /// <param name="port">Port enum</param>
    public PortViewModel GetPortViewModel(BasePort port)
    {
        PortViewModel vm = new(
            _portsInstallerFactory,
            _apiInterface,
            port,
            _logger
            );

        _ = Dispatcher.UIThread.Invoke(vm.InitializeAsync);
        return vm;
    }


    /// <summary>
    /// Create <see cref="PortViewModel"/>
    /// </summary>
    /// <param name="tool">Port enum</param>
    public ToolViewModel GetToolViewModel(BaseTool tool)
    {
        ToolViewModel vm = new(
            _toolsInstallerFactory,
            _apiInterface,
            tool,
            _logger
            );

        _ = Dispatcher.UIThread.Invoke(vm.InitializeAsync);
        return vm;
    }

#pragma warning restore CS0618 // Type or member is obsolete
}
