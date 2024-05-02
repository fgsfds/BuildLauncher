using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mods.Helpers;
using Ports.Ports;
using Ports.Providers;
using Ports.Tools;

namespace BuildLauncher.ViewModels
{
    public sealed partial class PortViewModel : ObservableObject
    {
        private readonly PortsInstallerFactory _installerFactory;
        private readonly BasePort _port;
        private PortRelease? _release;


        [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
        public PortViewModel(
            PortsInstallerFactory installerFactory,
            BasePort port
            )
        {
            _installerFactory = installerFactory;
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
                if (_port.IsInstalled && VersionComparer.Compare(_port.InstalledVersion!, _release?.Version!, "<"))
                {
                    return "Update";
                }

                if (_port.IsInstalled)
                {
                    return "Reinstall";
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
        /// Download/install progress
        /// </summary>
        public float ProgressBarValue { get; set; }

        #endregion


        #region Relay Commands

        /// <summary>
        /// Initialize VM
        /// </summary>
        public async Task InitializeAsync()
        {
            _release = await PortsReleasesProvider.GetLatestReleaseAsync(_port);

            OnPropertyChanged(nameof(LatestVersion));
            OnPropertyChanged(nameof(InstallButtonText));
        }


        /// <summary>
        /// Download and install port
        /// </summary>
        [RelayCommand]
        private async Task InstallAsync()
        {
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
        }

        #endregion


        private void OnProgressChanged(object? sender, float e)
        {
            ProgressBarValue = e;
            OnPropertyChanged(nameof(ProgressBarValue));
        }
    }
}
