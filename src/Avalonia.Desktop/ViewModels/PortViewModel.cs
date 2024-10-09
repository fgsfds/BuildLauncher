using Common.Entities;
using Common.Enums;
using Common.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Ports.Installer;
using Ports.Ports;
using Ports.Providers;
using System.Globalization;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class PortViewModel : ObservableObject
{
    private readonly PortsInstallerFactory _installerFactory;
    private readonly PortsReleasesProvider _portsReleasesProvider;
    private readonly BasePort _port;
    private GeneralReleaseEntity? _release;
    private ILogger _logger;

    public event EventHandler PortChangedEvent;


    [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
    public PortViewModel(
        PortsInstallerFactory installerFactory,
        PortsReleasesProvider portsReleasesProvider,
        BasePort port,
        ILogger logger
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
    /// Is port installed
    /// </summary>
    public bool IsInstalled => _port.IsInstalled;

    /// <summary>
    /// Latest available version
    /// </summary>
    public string LatestVersion => _release?.Version ?? "Not available";

    /// <summary>
    /// Is new version of the port available
    /// </summary>
    public bool IsUpdateAvailable
    {
        get
        {
            if (!_port.IsInstalled)
            {
                return false;
            }

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
                        return true;
                    }
                }

                return false;
            }

            return VersionComparer.Compare(_port.InstalledVersion!, _release?.Version!, "<");
        }
    }

    /// <summary>
    /// Can port be installed
    /// </summary>
    public bool CanBeInstalled => !IsInProgress && _release is not null;

    /// <summary>
    /// Download/install progress
    /// </summary>
    public float ProgressBarValue { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanBeInstalled))]
    private bool _isInProgress;

    #endregion


    #region Relay Commands

    /// <summary>
    /// Initialize VM
    /// </summary>
    public async Task InitializeAsync()
    {
        IsInProgress = true;

        _release = await _portsReleasesProvider.GetLatestReleaseAsync(_port.PortEnum).ConfigureAwait(true);

        OnPropertyChanged(nameof(LatestVersion));
        OnPropertyChanged(nameof(InstallButtonText));
        OnPropertyChanged(nameof(IsUpdateAvailable));
        OnPropertyChanged(nameof(IsInstalled));
        OnPropertyChanged(nameof(CanBeInstalled));

        IsInProgress = false;

        PortChangedEvent?.Invoke(this, EventArgs.Empty);
    }


    /// <summary>
    /// Download and install port
    /// </summary>
    [RelayCommand]
    private async Task InstallAsync()
    {
        try
        {
            IsInProgress = true;

            var installer = _installerFactory.Create();

            installer.Progress.ProgressChanged += OnProgressChanged;
            ProgressBarValue = 0;
            OnPropertyChanged(nameof(ProgressBarValue));

            await installer.InstallAsync(_port).ConfigureAwait(true);

            installer.Progress.ProgressChanged -= OnProgressChanged;
            ProgressBarValue = 0;

            OnPropertyChanged(nameof(ProgressBarValue));
            OnPropertyChanged(nameof(Version));
            OnPropertyChanged(nameof(InstallButtonText));
            OnPropertyChanged(nameof(IsUpdateAvailable));
            OnPropertyChanged(nameof(IsInstalled));

            UninstallCommand.NotifyCanExecuteChanged();
        }
        finally
        {
            IsInProgress = false;
            PortChangedEvent?.Invoke(this, EventArgs.Empty);
        }
    }


    /// <summary>
    /// Force check for updates
    /// </summary>
    [RelayCommand(CanExecute = nameof(UninstallCommandCanExecute))]
    private void Uninstall()
    {
        try
        {
            IsInProgress = true;

            Directory.Delete(_port.PortInstallFolderPath, true);

            OnPropertyChanged(nameof(Version));
            OnPropertyChanged(nameof(InstallButtonText));
            OnPropertyChanged(nameof(IsUpdateAvailable));
            OnPropertyChanged(nameof(IsInstalled));
        }
        finally
        {
            IsInProgress = false;
            PortChangedEvent?.Invoke(this, EventArgs.Empty);
        }
    }
    public bool UninstallCommandCanExecute => IsInstalled;


    #endregion


    private void OnProgressChanged(object? sender, float e)
    {
        ProgressBarValue = e;
        OnPropertyChanged(nameof(ProgressBarValue));
    }
}
