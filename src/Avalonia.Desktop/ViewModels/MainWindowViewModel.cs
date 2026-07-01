using Addons.Providers;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.All.Enums;
using Core.Client;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Core.Client.Tools;
using Games.Providers;
using Microsoft.Extensions.Logging;
using Ports.Ports;
using Ports.Providers;
using Tools.Tools;

namespace Avalonia.Desktop.ViewModels;

/// <summary>
///     Provides view model data and commands for the main window.
/// </summary>
public sealed class MainWindowViewModel : ObservableObject
{
    private readonly InstalledGamesProvider _gamesProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MainWindowViewModel" /> class.
    /// </summary>
    /// <param name="configProvider">The configuration provider.</param>
    /// <param name="filesUploader">The files uploader.</param>
    /// <param name="addonsDatabaseManager">The addons database manager.</param>
    /// <param name="gamesProvider">The installed games provider.</param>
    /// <param name="portsProvider">The ports provider.</param>
    /// <param name="appUpdateInstaller">The app update installer.</param>
    /// <param name="viewModelsFactory">The view models factory.</param>
    /// <param name="gamesPathsProvider">The games paths provider.</param>
    /// <param name="ports">The available ports.</param>
    /// <param name="tools">The available tools.</param>
    /// <param name="metadataProvider">The metadata provider.</param>
    /// <param name="downloadablesProviderFactory">The downloadable addons provider factory.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public MainWindowViewModel(
        IConfigProvider configProvider,
        IFilesUploader filesUploader,
        AddonsDatabaseManager addonsDatabaseManager,
        InstalledGamesProvider gamesProvider,
        PortsProvider portsProvider,
        AppUpdateInstaller appUpdateInstaller,
        ViewModelsFactory viewModelsFactory,
        GamesPathsProvider gamesPathsProvider,
        IEnumerable<BasePort> ports,
        IEnumerable<BaseTool> tools,
        MetadataProvider metadataProvider,
        DownloadableAddonsProviderFactory downloadablesProviderFactory,
        ILoggerFactory loggerFactory
        )
    {
        _gamesProvider = gamesProvider;
        _gamesProvider.GameChangedEvent += OnGameChanged;

        DevPageViewModel = new DevViewModel(configProvider, filesUploader, addonsDatabaseManager, gamesProvider, loggerFactory.CreateLogger<DevViewModel>());
        AboutPageViewModel = new AboutViewModel(appUpdateInstaller);
        PortsPageViewModel = new PortsViewModel(viewModelsFactory, portsProvider, ports, loggerFactory.CreateLogger<PortsViewModel>());
        ToolsPageViewModel = new ToolsViewModel(viewModelsFactory, tools);
        SettingsPageViewModel = new SettingsViewModel(configProvider, gamesPathsProvider);

        DukeViewModel = new(
            GameEnum.Duke3D,
            viewModelsFactory.GetCampaignsViewModel(GameEnum.Duke3D),
            viewModelsFactory.GetMapsViewModel(GameEnum.Duke3D),
            viewModelsFactory.GetModsViewModel(GameEnum.Duke3D),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.Duke3D),
            metadataProvider,
            downloadablesProviderFactory.Get(_gamesProvider.GetGame(GameEnum.Duke3D))
            );

        BloodViewModel = new(
            GameEnum.Blood,
            viewModelsFactory.GetCampaignsViewModel(GameEnum.Blood),
            viewModelsFactory.GetMapsViewModel(GameEnum.Blood),
            viewModelsFactory.GetModsViewModel(GameEnum.Blood),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.Blood),
            metadataProvider,
            downloadablesProviderFactory.Get(_gamesProvider.GetGame(GameEnum.Blood))
            );

        WangViewModel = new(
            GameEnum.Wang,
            viewModelsFactory.GetCampaignsViewModel(GameEnum.Wang),
            viewModelsFactory.GetMapsViewModel(GameEnum.Wang),
            viewModelsFactory.GetModsViewModel(GameEnum.Wang),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.Wang),
            metadataProvider,
            downloadablesProviderFactory.Get(_gamesProvider.GetGame(GameEnum.Wang))
            );

        FuryViewModel = new(
            GameEnum.Fury,
            viewModelsFactory.GetCampaignsViewModel(GameEnum.Fury),
            viewModelsFactory.GetMapsViewModel(GameEnum.Fury),
            viewModelsFactory.GetModsViewModel(GameEnum.Fury),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.Fury),
            metadataProvider,
            downloadablesProviderFactory.Get(_gamesProvider.GetGame(GameEnum.Fury))
            );

        RedneckViewModel = new(
            GameEnum.Redneck,
            viewModelsFactory.GetCampaignsViewModel(GameEnum.Redneck),
            viewModelsFactory.GetMapsViewModel(GameEnum.Redneck),
            viewModelsFactory.GetModsViewModel(GameEnum.Redneck),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.Redneck),
            metadataProvider,
            downloadablesProviderFactory.Get(_gamesProvider.GetGame(GameEnum.Redneck))
            );

        SlaveViewModel = new(
            GameEnum.Slave,
            viewModelsFactory.GetCampaignsViewModel(GameEnum.Slave),
            viewModelsFactory.GetMapsViewModel(GameEnum.Slave),
            viewModelsFactory.GetModsViewModel(GameEnum.Slave),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.Slave),
            metadataProvider,
            downloadablesProviderFactory.Get(_gamesProvider.GetGame(GameEnum.Slave))
            );

        NamViewModel = new(
            GameEnum.NAM,
            viewModelsFactory.GetCampaignsViewModel(GameEnum.NAM),
            viewModelsFactory.GetMapsViewModel(GameEnum.NAM),
            viewModelsFactory.GetModsViewModel(GameEnum.NAM),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.NAM),
            metadataProvider,
            downloadablesProviderFactory.Get(_gamesProvider.GetGame(GameEnum.NAM))
            );

        WWIIViewModel = new(
            GameEnum.WW2GI,
            viewModelsFactory.GetCampaignsViewModel(GameEnum.WW2GI),
            viewModelsFactory.GetMapsViewModel(GameEnum.WW2GI),
            viewModelsFactory.GetModsViewModel(GameEnum.WW2GI),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.WW2GI),
            metadataProvider,
            downloadablesProviderFactory.Get(_gamesProvider.GetGame(GameEnum.WW2GI))
            );

        WitchavenViewModel = new(
            GameEnum.Witchaven,
            viewModelsFactory.GetCampaignsViewModel(GameEnum.Witchaven),
            viewModelsFactory.GetMapsViewModel(GameEnum.Witchaven),
            viewModelsFactory.GetModsViewModel(GameEnum.Witchaven),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.Witchaven),
            metadataProvider,
            downloadablesProviderFactory.Get(_gamesProvider.GetGame(GameEnum.Witchaven))
            );

        TekWarViewModel = new(
            GameEnum.TekWar,
            viewModelsFactory.GetCampaignsViewModel(GameEnum.TekWar),
            viewModelsFactory.GetMapsViewModel(GameEnum.TekWar),
            viewModelsFactory.GetModsViewModel(GameEnum.TekWar),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.TekWar),
            metadataProvider,
            downloadablesProviderFactory.Get(_gamesProvider.GetGame(GameEnum.TekWar))
            );

        StandaloneViewModel = new(
            GameEnum.Standalone,
            viewModelsFactory.GetCampaignsViewModel(GameEnum.Standalone),
            null,
            null,
            viewModelsFactory.GetDownloadsViewModel(GameEnum.Standalone),
            metadataProvider,
            downloadablesProviderFactory.Get(_gamesProvider.GetGame(GameEnum.Standalone))
            );
    }


    /// <summary>
    ///     Update VM with path to the game changes in the config
    /// </summary>
    private void OnGameChanged(GameEnum _)
    {
        OnPropertyChanged(nameof(IsBloodTabEnabled));
        OnPropertyChanged(nameof(IsDukeTabEnabled));
        OnPropertyChanged(nameof(IsWangTabEnabled));
        OnPropertyChanged(nameof(IsFuryTabEnabled));
        OnPropertyChanged(nameof(IsRedneckTabEnabled));
        OnPropertyChanged(nameof(IsSlaveTabEnabled));
        OnPropertyChanged(nameof(IsNamTabEnabled));
        OnPropertyChanged(nameof(IsWW2GITabEnabled));
        OnPropertyChanged(nameof(IsWitchavenTabEnabled));
        OnPropertyChanged(nameof(IsTekWarTabEnabled));
    }


    #region Binding Properties

    /// <summary>
    ///     Gets the developer page view model.
    /// </summary>
    public DevViewModel DevPageViewModel { get; init; }

    /// <summary>
    ///     Gets the about page view model.
    /// </summary>
    public AboutViewModel AboutPageViewModel { get; init; }

    /// <summary>
    ///     Gets the ports page view model.
    /// </summary>
    public PortsViewModel PortsPageViewModel { get; init; }

    /// <summary>
    ///     Gets the tools page view model.
    /// </summary>
    public ToolsViewModel ToolsPageViewModel { get; init; }

    /// <summary>
    ///     Gets the settings page view model.
    /// </summary>
    public SettingsViewModel SettingsPageViewModel { get; init; }

    /// <summary>
    ///     Gets the Duke Nukem 3D game page view model.
    /// </summary>
    public GamePageViewModel DukeViewModel { get; init; }

    /// <summary>
    ///     Gets the Blood game page view model.
    /// </summary>
    public GamePageViewModel BloodViewModel { get; init; }

    /// <summary>
    ///     Gets the Shadow Warrior game page view model.
    /// </summary>
    public GamePageViewModel WangViewModel { get; init; }

    /// <summary>
    ///     Gets the Ion Fury game page view model.
    /// </summary>
    public GamePageViewModel FuryViewModel { get; init; }

    /// <summary>
    ///     Gets the Redneck Rampage game page view model.
    /// </summary>
    public GamePageViewModel RedneckViewModel { get; init; }

    /// <summary>
    ///     Gets the Powerslave game page view model.
    /// </summary>
    public GamePageViewModel SlaveViewModel { get; init; }

    /// <summary>
    ///     Gets the NAM game page view model.
    /// </summary>
    public GamePageViewModel NamViewModel { get; init; }

    /// <summary>
    ///     Gets the WW2GI game page view model.
    /// </summary>
    public GamePageViewModel WWIIViewModel { get; init; }

    /// <summary>
    ///     Gets the Witchaven game page view model.
    /// </summary>
    public GamePageViewModel WitchavenViewModel { get; init; }

    /// <summary>
    ///     Gets the TekWar game page view model.
    /// </summary>
    public GamePageViewModel TekWarViewModel { get; init; }

    /// <summary>
    ///     Gets the Standalone game page view model.
    /// </summary>
    public GamePageViewModel StandaloneViewModel { get; init; }


    /// <summary>
    ///     Is Blood tab enabled
    /// </summary>
    public bool IsBloodTabEnabled => _gamesProvider.IsBloodInstalled;

    /// <summary>
    ///     Is Duke Nukem 3D tab enabled
    /// </summary>
    public bool IsDukeTabEnabled => _gamesProvider.IsDukeInstalled;

    /// <summary>
    ///     Is Shadow Warrior tab enabled
    /// </summary>
    public bool IsWangTabEnabled => _gamesProvider.IsWangInstalled;

    /// <summary>
    ///     Is Ion Fury tab enabled
    /// </summary>
    public bool IsFuryTabEnabled => _gamesProvider.IsFuryInstalled;

    /// <summary>
    ///     Is Redneck Rampage tab enabled
    /// </summary>
    public bool IsRedneckTabEnabled => _gamesProvider.IsRedneckInstalled;

    /// <summary>
    ///     Is Powerslave tab enabled
    /// </summary>
    public bool IsSlaveTabEnabled => _gamesProvider.IsSlaveInstalled;

    /// <summary>
    ///     Is NAM tab enabled
    /// </summary>
    public bool IsNamTabEnabled => _gamesProvider.IsNamInstalled;

    /// <summary>
    ///     Is WW2I tab enabled
    /// </summary>
    public bool IsWW2GITabEnabled => _gamesProvider.IsWW2GIInstalled;

    /// <summary>
    ///     Is Witchaven tab enabled.
    /// </summary>
    public bool IsWitchavenTabEnabled => _gamesProvider.IsWitchavenInstalled;

    /// <summary>
    ///     Is TekWar tab enabled.
    /// </summary>
    public bool IsTekWarTabEnabled => _gamesProvider.IsTekWarInstalled;

    /// <summary>
    ///     Is app running in the developer mode.
    /// </summary>
    public bool IsDeveloperMode => ClientProperties.IsDeveloperMode;

    #endregion
}
