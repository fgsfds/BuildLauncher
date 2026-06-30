using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Client;
using Core.Client.Helpers;

namespace Avalonia.Desktop.ViewModels;

/// <summary>
///     Represents the state of the update process.
/// </summary>
public enum UpdateState
{
    InProgress,
    UpdateFailed,
    UpToDate,
    UpdateAvailable
}


public sealed partial class AboutViewModel : ObservableObject
{
    private readonly AppUpdateInstaller _updateInstaller;


    public AboutViewModel(AppUpdateInstaller updateInstaller)
    {
        _updateInstaller = updateInstaller;
        _ = CheckForUpdateAsync();
    }


    /// <summary>
    ///     Check for app update
    /// </summary>
    private async Task CheckForUpdateAsync()
    {
        try
        {
            CurrentUpdateState = UpdateState.InProgress;
            var updates = await _updateInstaller.CheckForUpdates(CurrentVersion).ConfigureAwait(true);

            if (updates is null)
            {
                CurrentUpdateState = UpdateState.UpdateFailed;
            }
            else if (updates.Value)
            {
                CurrentUpdateState = UpdateState.UpdateAvailable;
            }
            else
            {
                CurrentUpdateState = UpdateState.UpToDate;
            }
        }
        catch
        {
            CurrentUpdateState = UpdateState.UpdateFailed;
        }
    }


    #region Binding Properties

    /// <summary>
    ///     Current app version
    /// </summary>
    public Version CurrentVersion => ClientProperties.CurrentVersion;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckOrInstallUpdateCommand))]
    public partial UpdateState CurrentUpdateState { get; set; }

    #endregion Binding Properties


    #region Relay Commands

    /// <summary>
    ///     Check for BuildLauncher updates
    /// </summary>
    [RelayCommand(CanExecute = nameof(CheckOrInstallUpdateCanExecute))]
    private async Task CheckOrInstallUpdateAsync()
    {
        if (CurrentUpdateState is UpdateState.UpdateAvailable)
        {
            CurrentUpdateState = UpdateState.InProgress;
            await _updateInstaller.DownloadAndUnpackLatestRelease().ConfigureAwait(true);
            AppUpdateInstaller.InstallUpdate();
        }
        else
        {
            await CheckForUpdateAsync().ConfigureAwait(true);
        }
    }

    private bool CheckOrInstallUpdateCanExecute() => CurrentUpdateState is not UpdateState.InProgress;

    #endregion Relay Commands
}
