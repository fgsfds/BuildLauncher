using Common.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Updater;

namespace BuildLauncher.ViewModels
{
    public sealed partial class AboutViewModel(AppUpdateInstaller updateInstaller) : ObservableObject
    {
        private readonly AppUpdateInstaller _updateInstaller = updateInstaller;

        #region Binding Properties

        public Version? CurrentVersion => CommonProperties.CurrentVersion;

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
        private Task InitializeAsync() => CheckForUpdatesAsync();

        /// <summary>
        /// Check for SSH updates
        /// </summary>
        [RelayCommand(CanExecute = (nameof(CheckForUpdatesCanExecute)))]
        private async Task CheckForUpdatesAsync()
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
    }
}