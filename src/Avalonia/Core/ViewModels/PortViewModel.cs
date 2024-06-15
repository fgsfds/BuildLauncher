using Common.Entities;
using Common.Enums;
using Common.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ports.Installer;
using Ports.Ports;
using System.Globalization;

namespace BuildLauncher.ViewModels
{
    public sealed partial class PortViewModel : ObservableObject
    {
        private readonly PortsInstallerFactory _installerFactory;
        private readonly PortsReleasesProvider _portsReleasesProvider;
        private readonly BasePort _port;
        private GeneralReleaseEntity? _release;


        [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
        public PortViewModel(
            PortsInstallerFactory installerFactory,
            PortsReleasesProvider portsReleasesProvider,
            BasePort port
            )
        {
            _installerFactory = installerFactory;
            _portsReleasesProvider = portsReleasesProvider;
            _port = port;
        }


        #region Binding Properties

        /// <summary>
        /// Text of the install button
        /// </summary>
        public string InstallButtonText
        {
            get
            {
                if (_port.IsInstalled)
                {
                    //NotBlood Hack
                    if (_port.PortEnum is PortEnum.NotBlood)
                    {
                        var r1 = DateTime.TryParseExact(
                            _port.InstalledVersion,
                            "dd.MM.yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var currentVersion
                            );

                        var r2 = DateTime.TryParseExact(
                            _release?.Version,
                            "dd.MM.yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var newVersion
                            );

                        if (r1 && r2)
                        {
                            if (currentVersion < newVersion)
                            {
                                return "Update";
                            }
                            else
                            {
                                return "Reinstall";
                            }
                        }

                        return "Install";
                    }

                    if (IsUpdateAvailable)
                    {
                        return "Update";
                    }
                    else
                    {
                        return "Reinstall";
                    }
                }

                return "Install";
            }
        }

        /// <summary>
        /// Name of the port
        /// </summary>
        public string Name => _port.Name;

        /// <summary>
        /// Port's icon
        /// </summary>
        public Stream Icon => _port.Icon;

        /// <summary>
        /// Currently installed version
        /// </summary>
        public string Version => _port.InstalledVersion ?? "None";

        /// <summary>
        /// Latest available version
        /// </summary>
        public string LatestVersion => _release?.Version ?? "Not available";

        /// <summary>
        /// Is new version of the port available
        /// </summary>
        public bool IsUpdateAvailable => VersionComparer.Compare(_port.InstalledVersion!, _release?.Version!, "<");

        /// <summary>
        /// Download/install progress
        /// </summary>
        public float ProgressBarValue { get; set; }


        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InstallCommand))]
        [NotifyCanExecuteChangedFor(nameof(CheckUpdateCommand))]
        private bool _isInProgress;

        #endregion


        #region Relay Commands

        /// <summary>
        /// Initialize VM
        /// </summary>
        public async Task InitializeAsync()
        {
            _release = await _portsReleasesProvider.GetLatestReleaseAsync(_port.PortEnum);

            OnPropertyChanged(nameof(LatestVersion));
            OnPropertyChanged(nameof(InstallButtonText));
            OnPropertyChanged(nameof(IsUpdateAvailable));
        }


        /// <summary>
        /// Download and install port
        /// </summary>
        [RelayCommand(CanExecute = nameof(InstallCommandCanExecute))]
        private async Task InstallAsync()
        {
            IsInProgress = true;

            var installer = _installerFactory.Create();

            installer.Progress.ProgressChanged += OnProgressChanged;
            ProgressBarValue = 0;
            OnPropertyChanged(nameof(ProgressBarValue));

            await installer.InstallAsync(_port);

            installer.Progress.ProgressChanged -= OnProgressChanged;
            ProgressBarValue = 0;

            OnPropertyChanged(nameof(ProgressBarValue));
            OnPropertyChanged(nameof(Version));
            OnPropertyChanged(nameof(InstallButtonText));
            OnPropertyChanged(nameof(IsUpdateAvailable));

            IsInProgress = false;
        }
        public bool InstallCommandCanExecute() => !IsInProgress;


        /// <summary>
        /// Force check for updates
        /// </summary>
        [RelayCommand(CanExecute = nameof(CheckUpdateCommandCanExecute))]
        private async Task CheckUpdateAsync()
        {
            IsInProgress = true;

            _release = await _portsReleasesProvider.GetLatestReleaseAsync(_port.PortEnum);

            OnPropertyChanged(nameof(LatestVersion));
            OnPropertyChanged(nameof(InstallButtonText));
            OnPropertyChanged(nameof(IsUpdateAvailable));

            IsInProgress = false;
        }
        public bool CheckUpdateCommandCanExecute() => !IsInProgress;


        #endregion


        private void OnProgressChanged(object? sender, float e)
        {
            ProgressBarValue = e;
            OnPropertyChanged(nameof(ProgressBarValue));
        }
    }
}
