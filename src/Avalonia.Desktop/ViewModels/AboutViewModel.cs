using Common.Client;
using Common.Client.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class AboutViewModel : ObservableObject
{
    private readonly AppUpdateInstaller _updateInstaller;

    #region Binding Properties

    /// <summary>
    /// Current app version
    /// </summary>
    public Version CurrentVersion => ClientProperties.CurrentVersion;

    [ObservableProperty]
    private string _checkForUpdatesButtonText = string.Empty;

    [ObservableProperty]
    private bool _isUpdateAvailable;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckForUpdatesCommand))]
    [NotifyCanExecuteChangedFor(nameof(DownloadAndInstallCommand))]
    private bool _isInProgress;

    #endregion Binding Properties


    public AboutViewModel(AppUpdateInstaller updateInstaller)
    {
        _updateInstaller = updateInstaller;
        _ = CheckForUpdateAsync();
    }


    #region Relay Commands

    /// <summary>
    /// Check for SSH updates
    /// </summary>
    [RelayCommand(CanExecute = nameof(CheckForUpdatesCanExecute))]
    private Task CheckForUpdatesAsync() => CheckForUpdateAsync();
    private bool CheckForUpdatesCanExecute() => !IsInProgress;

    /// <summary>
    /// Download and install SSH update
    /// </summary>
    [RelayCommand(CanExecute = nameof(DownloadAndInstallCanExecute))]
    private async Task DownloadAndInstallAsync()
    {
        IsInProgress = true;

        await _updateInstaller.DownloadAndUnpackLatestRelease().ConfigureAwait(false);

        AppUpdateInstaller.InstallUpdate();
    }
    private bool DownloadAndInstallCanExecute() => IsUpdateAvailable;

    #endregion Relay Commands


    /// <summary>
    /// Check for app update
    /// </summary>
    private async Task CheckForUpdateAsync()
    {
        IsInProgress = true;

        bool? updates;

        try
        {
            CheckForUpdatesButtonText = "Checking...";
            updates = await _updateInstaller.CheckForUpdates(CurrentVersion).ConfigureAwait(true);
        }
        catch
        {
            CheckForUpdatesButtonText = "Error while getting updates";
            IsInProgress = false;
            return;
        }

        if (updates is null)
        {
            CheckForUpdatesButtonText = "Error while getting updates";
        }
        else if (updates.Value)
        {
            IsUpdateAvailable = true;
        }
        else
        {
            CheckForUpdatesButtonText = "Already up-to-date";
        }

        IsInProgress = false;
    }
}