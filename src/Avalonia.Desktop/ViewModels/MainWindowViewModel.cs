using Common.All.Enums;
using Common.Client;
using Common.Client.Helpers;
using Common.Client.Interfaces;
using Common.Client.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
using Games.Providers;
using Microsoft.Extensions.Logging;
using Ports.Providers;

namespace Avalonia.Desktop.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly InstalledGamesProvider _gamesProvider;

    public MainWindowViewModel(
        IConfigProvider configProvider,
        FilesUploader filesUploader,
        InstalledGamesProvider gamesProvider,
        InstalledPortsProvider portsProvider,
        AppUpdateInstaller appUpdateInstaller,
        ViewModelsFactory viewModelsFactory,
        GamesPathsProvider gamesPathsProvider,
        ILogger logger
        )
    {
        _gamesProvider = gamesProvider;
        _gamesProvider.GameChangedEvent += OnGameChanged;

        DevPageViewModel = new DevViewModel(configProvider, filesUploader, gamesProvider, logger);
        AboutPageViewModel = new AboutViewModel(appUpdateInstaller);
        PortsPageViewModel = new PortsViewModel(viewModelsFactory, portsProvider, logger);
        SettingsPageViewModel = new SettingsViewModel(configProvider, gamesPathsProvider);

        DukeViewModel = new(
            viewModelsFactory.GetCampaignsViewModel(GameEnum.Duke3D),
            viewModelsFactory.GetMapsViewModel(GameEnum.Duke3D),
            viewModelsFactory.GetModsViewModel(GameEnum.Duke3D),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.Duke3D)
            );

        BloodViewModel = new(
            viewModelsFactory.GetCampaignsViewModel(GameEnum.Blood),
            viewModelsFactory.GetMapsViewModel(GameEnum.Blood),
            viewModelsFactory.GetModsViewModel(GameEnum.Blood),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.Blood)
            );

        WangViewModel = new(
            viewModelsFactory.GetCampaignsViewModel(GameEnum.Wang),
            viewModelsFactory.GetMapsViewModel(GameEnum.Wang),
            viewModelsFactory.GetModsViewModel(GameEnum.Wang),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.Wang)
            );

        FuryViewModel = new(
            viewModelsFactory.GetCampaignsViewModel(GameEnum.Fury),
            viewModelsFactory.GetMapsViewModel(GameEnum.Fury),
            viewModelsFactory.GetModsViewModel(GameEnum.Fury),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.Fury)
            );

        RedneckViewModel = new(
            viewModelsFactory.GetCampaignsViewModel(GameEnum.Redneck),
            viewModelsFactory.GetMapsViewModel(GameEnum.Redneck),
            viewModelsFactory.GetModsViewModel(GameEnum.Redneck),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.Redneck)
            );

        SlaveViewModel = new(
            viewModelsFactory.GetCampaignsViewModel(GameEnum.Slave),
            viewModelsFactory.GetMapsViewModel(GameEnum.Slave),
            viewModelsFactory.GetModsViewModel(GameEnum.Slave),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.Slave)
            );

        NamViewModel = new(
            viewModelsFactory.GetCampaignsViewModel(GameEnum.NAM),
            viewModelsFactory.GetMapsViewModel(GameEnum.NAM),
            viewModelsFactory.GetModsViewModel(GameEnum.NAM),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.NAM)
            );

        WWIIViewModel = new(
            viewModelsFactory.GetCampaignsViewModel(GameEnum.WW2GI),
            viewModelsFactory.GetMapsViewModel(GameEnum.WW2GI),
            viewModelsFactory.GetModsViewModel(GameEnum.WW2GI),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.WW2GI)
            );

        WitchavenViewModel = new(
            viewModelsFactory.GetCampaignsViewModel(GameEnum.Witchaven),
            viewModelsFactory.GetMapsViewModel(GameEnum.Witchaven),
            viewModelsFactory.GetModsViewModel(GameEnum.Witchaven),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.Witchaven)
            );

        TekWarViewModel = new(
            viewModelsFactory.GetCampaignsViewModel(GameEnum.TekWar),
            viewModelsFactory.GetMapsViewModel(GameEnum.TekWar),
            viewModelsFactory.GetModsViewModel(GameEnum.TekWar),
            viewModelsFactory.GetDownloadsViewModel(GameEnum.TekWar)
            );

        StandaloneViewModel = new(
            viewModelsFactory.GetCampaignsViewModel(GameEnum.Standalone),
            null,
            null,
            viewModelsFactory.GetDownloadsViewModel(GameEnum.Standalone)
            );
    }


    #region Binding Properties

    public DevViewModel DevPageViewModel { get; init; }

    public AboutViewModel AboutPageViewModel { get; init; }

    public PortsViewModel PortsPageViewModel { get; init; }

    public SettingsViewModel SettingsPageViewModel { get; init; }

    public GamePageViewModel DukeViewModel { get; init; }

    public GamePageViewModel BloodViewModel { get; init; }

    public GamePageViewModel WangViewModel { get; init; }

    public GamePageViewModel FuryViewModel { get; init; }

    public GamePageViewModel RedneckViewModel { get; init; }

    public GamePageViewModel SlaveViewModel { get; init; }

    public GamePageViewModel NamViewModel { get; init; }

    public GamePageViewModel WWIIViewModel { get; init; }

    public GamePageViewModel WitchavenViewModel { get; init; }

    public GamePageViewModel TekWarViewModel { get; init; }

    public GamePageViewModel StandaloneViewModel { get; init; }



    /// <summary>
    /// Is Blood tab enabled
    /// </summary>
    public bool IsBloodTabEnabled => _gamesProvider.IsBloodInstalled;

    /// <summary>
    /// Is Duke Nukem 3D tab enabled
    /// </summary>
    public bool IsDukeTabEnabled => _gamesProvider.IsDukeInstalled;

    /// <summary>
    /// Is Shadow Warrior tab enabled
    /// </summary>
    public bool IsWangTabEnabled => _gamesProvider.IsWangInstalled;

    /// <summary>
    /// Is Ion Fury tab enabled
    /// </summary>
    public bool IsFuryTabEnabled => _gamesProvider.IsFuryInstalled;

    /// <summary>
    /// Is Redneck Rampage tab enabled
    /// </summary>
    public bool IsRedneckTabEnabled => _gamesProvider.IsRedneckInstalled;

    /// <summary>
    /// Is Powerslave tab enabled
    /// </summary>
    public bool IsSlaveTabEnabled => _gamesProvider.IsSlaveInstalled;

    /// <summary>
    /// Is NAM tab enabled
    /// </summary>
    public bool IsNamTabEnabled => _gamesProvider.IsNamInstalled;

    /// <summary>
    /// Is WW2I tab enabled
    /// </summary>
    public bool IsWW2GITabEnabled => _gamesProvider.IsWW2GIInstalled;

    /// <summary>
    /// Is WW2I tab enabled
    /// </summary>
    public bool IsWitchavenTabEnabled => _gamesProvider.IsWitchavenInstalled;

    /// <summary>
    /// Is WW2I tab enabled
    /// </summary>
    public bool IsTekWarTabEnabled => _gamesProvider.IsTekWarInstalled;

    /// <summary>
    /// Is app running in the developer mode
    /// </summary>
    public bool IsDeveloperMode => ClientProperties.IsDeveloperMode;

    #endregion


    /// <summary>
    /// Update VM with path to the game changes in the config
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
}
