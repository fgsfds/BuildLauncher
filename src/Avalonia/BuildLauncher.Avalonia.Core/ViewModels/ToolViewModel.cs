using Common.Helpers;
using Common.Releases;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using Tools.Installer;
using Tools.Tools;

namespace BuildLauncher.ViewModels
{
    public sealed partial class ToolViewModel : ObservableObject
    {
        private readonly ToolsInstallerFactory _installerFactory;
        private readonly BaseTool _tool;
        private CommonRelease? _release;


        [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
        public ToolViewModel(
            ToolsInstallerFactory installerFactory,
            BaseTool tool
            )
        {
            _installerFactory = installerFactory;
            _tool = tool;
        }


        #region Binding Properties

        /// <summary>
        /// Text of the install button
        /// </summary>
        public string InstallButtonText
        {
            get
            {
                if (_tool.IsInstalled && VersionComparer.Compare(_tool.InstalledVersion!, _release?.Version!, "<"))
                {
                    return "Update";
                }

                if (_tool.IsInstalled)
                {
                    return "Reinstall";
                }

                return "Install";
            }
        }

        /// <summary>
        /// Name of the port
        /// </summary>
        public string Name => _tool.Name;

        /// <summary>
        /// Port's icon
        /// </summary>
        public Stream Icon => _tool.Icon;

        /// <summary>
        /// Currently installed version
        /// </summary>
        public string Version => _tool.InstalledVersion ?? "None";

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
            _release = await ToolsReleasesProvider.GetLatestReleaseAsync(_tool);

            OnPropertyChanged(nameof(LatestVersion));
            OnPropertyChanged(nameof(InstallButtonText));
        }

        /// <summary>
        /// Initialize VM
        /// </summary>
        [RelayCommand(CanExecute = nameof(StartCommandCanExecute))]
        public void Start()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _tool.FullPathToExe,
                UseShellExecute = true
            });
        }
        public bool StartCommandCanExecute() => _tool.IsInstalled;


        /// <summary>
        /// Download and install port
        /// </summary>
        [RelayCommand(CanExecute=nameof(InstallCommandCanExecute))]
        private async Task InstallAsync()
        {
            var installer = _installerFactory.Create();

            installer.Progress.ProgressChanged += OnProgressChanged;
            ProgressBarValue = 0;
            OnPropertyChanged(nameof(ProgressBarValue));

            await installer.InstallAsync(_tool);

            installer.Progress.ProgressChanged -= OnProgressChanged;
            ProgressBarValue = 0;
            OnPropertyChanged(nameof(ProgressBarValue));
            OnPropertyChanged(nameof(Version));
            OnPropertyChanged(nameof(InstallButtonText));
            StartCommand.NotifyCanExecuteChanged();
        }
        public bool InstallCommandCanExecute() => !CommonProperties.IsDevMode;

        #endregion


        private void OnProgressChanged(object? sender, float e)
        {
            ProgressBarValue = e;
            OnPropertyChanged(nameof(ProgressBarValue));
        }
    }
}
