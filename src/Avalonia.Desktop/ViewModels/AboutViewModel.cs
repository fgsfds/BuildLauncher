using Common.Client;
using Common.Client.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class AboutViewModel(AppUpdateInstaller updateInstaller) : ObservableObject
{
    private readonly AppUpdateInstaller _updateInstaller = updateInstaller;

    #region Binding Properties

    /// <summary>
    /// Current app version
    /// </summary>
    public Version CurrentVersion => ClientProperties.CurrentVersion;

    [ObservableProperty]
    private string _aboutTabHeader = "About";

    [ObservableProperty]
    private string _checkForUpdatesButtonText = string.Empty;

    [ObservableProperty]
    private bool _isUpdateAvailable;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckForUpdatesCommand))]
    [NotifyCanExecuteChangedFor(nameof(DownloadAndInstallCommand))]
    private bool _isInProgress;

    #endregion Binding Properties


    #region Relay Commands

    /// <summary>
    /// VM initialization
    /// </summary>
    [RelayCommand]
    private Task InitializeAsync() => CheckForUpdateAsync();

    /// <summary>
    /// Check for SSH updates
    /// </summary>
    [RelayCommand(CanExecute = (nameof(CheckForUpdatesCanExecute)))]
    private async Task CheckForUpdatesAsync()
    {
        await CheckForUpdateAsync();
    }

    private bool CheckForUpdatesCanExecute() => IsInProgress is false;

    /// <summary>
    /// Download and install SSH update
    /// </summary>
    [RelayCommand(CanExecute = (nameof(DownloadAndInstallCanExecute)))]
    private async Task DownloadAndInstallAsync()
    {
        IsInProgress = true;

        await _updateInstaller.DownloadAndUnpackLatestRelease().ConfigureAwait(false);

        AppUpdateInstaller.InstallUpdate();
    }
    private bool DownloadAndInstallCanExecute() => IsUpdateAvailable;

    #endregion Relay Commands


    /// <summary>
    /// Update tab header
    /// </summary>
    private void UpdateHeader()
    {
        AboutTabHeader = "About" + (IsUpdateAvailable
            ? " (UPD)"
            : string.Empty);
    }

    /// <summary>
    /// Check for app update
    /// </summary>
    private async Task CheckForUpdateAsync()
    {
        IsInProgress = true;

        bool updates;

        try
        {
            CheckForUpdatesButtonText = "Checking...";
            updates = await _updateInstaller.CheckForUpdates(CurrentVersion);
        }
        catch
        {
            CheckForUpdatesButtonText = "Error while getting updates";
            IsInProgress = false;
            return;
        }

        if (updates)
        {
            IsUpdateAvailable = true;

            UpdateHeader();
        }
        else
        {
            CheckForUpdatesButtonText = "Already up-to-date";
        }

        IsInProgress = false;
    }
}