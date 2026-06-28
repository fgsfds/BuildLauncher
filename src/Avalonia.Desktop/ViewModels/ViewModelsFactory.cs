using Addons.Helpers;
using Addons.Providers;
using Avalonia.Desktop.Misc;
using Avalonia.Threading;
using Core.All.Enums;
using Core.Client;
using Core.Client.Interfaces;
using Core.Client.Providers;
using Core.Client.Tools;
using Games.Providers;
using Microsoft.Extensions.Logging;
using Ports.Installer;
using Ports.Ports;
using Ports.Providers;
using Tools.Installer;
using Tools.Tools;

namespace Avalonia.Desktop.ViewModels;

public sealed class ViewModelsFactory
{
    private readonly InstalledGamesProvider _gamesProvider;
    private readonly IConfigProvider _config;
    private readonly PortInstallerFactory _portsInstallerFactory;
    private readonly ToolInstallerFactory _toolsInstallerFactory;
    private readonly PortsProvider _portsProvider;
    private readonly PlaytimeProvider _playtimeProvider;
    private readonly MetadataProvider _metadataProvider;
    private readonly IApiInterface _apiInterface;
    private readonly RatingProvider _ratingProvider;
    private readonly InstalledAddonsProviderFactory _installedAddonsProviderFactory;
    private readonly DownloadableAddonsProviderFactory _downloadableAddonsProviderFactory;
    private readonly PortStarter _portStarter;
    private readonly IFilesUploader _filesUploader;
    private readonly AddonsDatabaseManager _addonsDatabaseManager;
    private readonly AppUpdateInstaller _appUpdateInstaller;
    private readonly GamesPathsProvider _gamesPathsProvider;
    private readonly BitmapsCache _bitmapsCache;
    private readonly IReadOnlyList<BasePort> _ports;
    private readonly IReadOnlyList<BaseTool> _tools;
    private readonly IAddonDropHelper _addonInstaller;
    private readonly ILoggerFactory _loggerFactory;

    public ViewModelsFactory(
        InstalledGamesProvider gamesProvider,
        IConfigProvider IConfigProvider,
        PortInstallerFactory portsInstallerFactory,
        ToolInstallerFactory toolsInstallerFactory,
        PortsProvider portsProvider,
        PlaytimeProvider playtimeProvider,
        IApiInterface apiInterface,
        RatingProvider ratingProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory downloadableAddonsProviderFactory,
        PortStarter portStarter,
        IFilesUploader filesUploader,
        AddonsDatabaseManager addonsDatabaseManager,
        AppUpdateInstaller appUpdateInstaller,
        GamesPathsProvider gamesPathsProvider,
        BitmapsCache bitmapsCache,
        IEnumerable<BasePort> ports,
        IEnumerable<BaseTool> tools,
        MetadataProvider metadataProvider,
        IAddonDropHelper addonInstaller,
        ILoggerFactory loggerFactory
        )
    {
        _gamesProvider = gamesProvider;
        _config = IConfigProvider;
        _portsInstallerFactory = portsInstallerFactory;
        _toolsInstallerFactory = toolsInstallerFactory;
        _portsProvider = portsProvider;
        _playtimeProvider = playtimeProvider;
        _apiInterface = apiInterface;
        _ratingProvider = ratingProvider;
        _installedAddonsProviderFactory = installedAddonsProviderFactory;
        _downloadableAddonsProviderFactory = downloadableAddonsProviderFactory;
        _portStarter = portStarter;
        _filesUploader = filesUploader;
        _addonsDatabaseManager = addonsDatabaseManager;
        _appUpdateInstaller = appUpdateInstaller;
        _gamesPathsProvider = gamesPathsProvider;
        _bitmapsCache = bitmapsCache;
        _ports = [.. ports];
        _tools = [.. tools];
        _metadataProvider = metadataProvider;
        _addonInstaller = addonInstaller;
        _loggerFactory = loggerFactory;
    }

#pragma warning disable CS0618 // Type or member is obsolete

    public MainWindowViewModel GetMainWindowViewModel()
    {
        MainWindowViewModel vm = new(
            _config,
            _filesUploader,
            _addonsDatabaseManager,
            _gamesProvider,
            _portsProvider,
            _appUpdateInstaller,
            this,
            _gamesPathsProvider,
            _ports,
            _tools,
            _metadataProvider,
            _downloadableAddonsProviderFactory,
            _loggerFactory
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
            _metadataProvider,
            _installedAddonsProviderFactory,
            _downloadableAddonsProviderFactory,
            _portStarter,
            _bitmapsCache,
            _addonInstaller,
            _loggerFactory.CreateLogger<CampaignsViewModel>()
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
            _metadataProvider,
            _installedAddonsProviderFactory,
            _downloadableAddonsProviderFactory,
            _portStarter,
            _bitmapsCache,
            _addonInstaller,
            _loggerFactory.CreateLogger<MapsViewModel>()
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
            _metadataProvider,
            _installedAddonsProviderFactory,
            _downloadableAddonsProviderFactory,
            _bitmapsCache,
            _config,
            _addonInstaller,
            _loggerFactory.CreateLogger<ModsViewModel>()
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
            _loggerFactory.CreateLogger<DownloadsViewModel>()
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
            _loggerFactory.CreateLogger<PortViewModel>()
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
            _loggerFactory.CreateLogger<ToolViewModel>()
            );

        _ = Dispatcher.UIThread.Invoke(vm.InitializeAsync);
        return vm;
    }

#pragma warning restore CS0618 // Type or member is obsolete
}
