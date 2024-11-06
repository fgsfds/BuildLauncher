using Common.Client;
using Common.Client.Helpers;
using Common.Client.Interfaces;
using Common.Client.Tools;
using Common.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using Games.Providers;
using Ports.Installer;
using Ports.Providers;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly GamesProvider _gamesProvider;

    public MainViewModel(
        IConfigProvider configProvider,
        FilesUploader filesUploader,
        GamesProvider gamesProvider,
        AppUpdateInstaller appUpdateInstaller,
        PortsInstallerFactory portsInstallerFactory,
        PortsReleasesProvider portsReleasesProvider,
        ViewModelsFactory viewModelsFactory,
        GamesPathsProvider gamesPathsProvider
        )
    {
        _gamesProvider = gamesProvider;
        _gamesProvider.GameChangedEvent += OnGameChanged;

        DevPageViewModel = new DevViewModel(configProvider, filesUploader, gamesProvider);
        AboutPageViewModel = new AboutViewModel(appUpdateInstaller);
        PortsPageViewModel = new PortsViewModel(portsInstallerFactory, portsReleasesProvider, viewModelsFactory);
        SettingsPageViewModel = new SettingsViewModel(configProvider, gamesPathsProvider);
    }


    #region Binding Properties

    [ObservableProperty]
    private DevViewModel _devPageViewModel;

    [ObservableProperty]
    private AboutViewModel _aboutPageViewModel;

    [ObservableProperty]
    private PortsViewModel _portsPageViewModel;

    [ObservableProperty]
    private SettingsViewModel _settingsPageViewModel;

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
