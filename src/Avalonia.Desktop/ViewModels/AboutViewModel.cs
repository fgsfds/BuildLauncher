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
    /// <summary>
    ///     Update is in progress.
    /// </summary>
    InProgress,

    /// <summary>
    ///     Update has failed.
    /// </summary>
    UpdateFailed,

    /// <summary>
    ///     Application is up to date.
    /// </summary>
    UpToDate,

    /// <summary>
    ///     An update is available.
    /// </summary>
    UpdateAvailable
}


public sealed partial class AboutViewModel : ObservableObject
{
    private readonly AppUpdateInstaller _updateInstaller;


    /// <summary>
    ///     Initializes a new instance of the <see cref="AboutViewModel" /> class.
    /// </summary>
    /// <param name="updateInstaller">The app update installer.</param>
    public AboutViewModel(AppUpdateInstaller updateInstaller)
    {
        _updateInstaller = updateInstaller;
        _ = CheckForUpdateAsync();
    }


    /// <summary>
    ///     Checks for application updates asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
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
    ///     Checks for or installs an update.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
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

    /// <summary>
    ///     Determines whether the check or install update command can execute.
    /// </summary>
    /// <returns>True if an update is not already in progress.</returns>
    private bool CheckOrInstallUpdateCanExecute() => CurrentUpdateState is not UpdateState.InProgress;

    #endregion Relay Commands
}
