using System.Diagnostics;
using System.Globalization;
using Avalonia.Controls.Notifications;
using Avalonia.Desktop.Misc;
using Common.All.Enums;
using Common.All.Helpers;
using Common.All.Serializable.Downloadable;
using Common.Client.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Ports.Installer;
using Ports.Ports;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class PortViewModel : ObservableObject
{
    public BasePort Port { get; init; }

    private readonly PortInstallerFactory _installerFactory;
    private readonly IApiInterface _apiInterface;
    private GeneralReleaseJsonModel? _release;
    private readonly ILogger _logger;

    public delegate void PortChanged(PortEnum portEnum);
    public event PortChanged? PortChangedEvent;


    [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
    public PortViewModel(
        PortInstallerFactory installerFactory,
        IApiInterface apiInterface,
        BasePort port,
        ILogger logger
        )
    {
        _installerFactory = installerFactory;
        _apiInterface = apiInterface;
        _logger = logger;
        Port = port;
    }


    #region Binding Properties

    /// <summary>
    /// Text of the install button
    /// </summary>
    public string InstallButtonText
    {
        get
        {
            if (Port.IsInstalled)
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
    public string Name => Port.Name;

    /// <summary>
    /// Port's icon
    /// </summary>
    public long IconId => Port.IconId;

    /// <summary>
    /// Currently installed version
    /// </summary>
    public string Version
    {
        get
        {
            if (Port.InstalledVersion is null)
            {
                return "None";
            }

            if (Port.PortEnum is PortEnum.NotBlood)
            {
                return DateTime.Parse(Port.InstalledVersion).ToString("dd.MM.yyyy");
            }

            return Port.InstalledVersion;
        }
    }

    /// <summary>
    /// Is port installed
    /// </summary>
    public bool IsInstalled => Port.IsInstalled;

    /// <summary>
    /// Latest available version
    /// </summary>
    public string LatestVersion
    {
        get
        {
            if (IsCheckingForUpdates)
            {
                return "Checking...";
            }

            if (_release?.Version is null)
            {
                return "Error";
            }

            if (Port.PortEnum is PortEnum.NotBlood)
            {
                return DateTime.Parse(_release.Version).ToString("dd.MM.yyyy");
            }

            return _release?.Version ?? "Error";
        }
    }

    /// <summary>
    /// Is new version of the port available
    /// </summary>
    public bool IsUpdateAvailable
    {
        get
        {
            if (!Port.IsInstalled)
            {
                return false;
            }

            if (_release is null)
            {
                return false;
            }

            if (Port.PortEnum is PortEnum.NotBlood)
            {
                var r1 = DateTime.TryParse(
                    Port.InstalledVersion,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out var currentVersion
                    );

                if (!r1)
                {
                    r1 = DateTime.TryParseExact(
                        Port.InstalledVersion,
                        "dd.MM.yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out currentVersion
                        );
                }

                var r2 = DateTime.TryParse(
                    _release?.Version,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out var newVersion
                    );

                if (r1 && r2 && currentVersion < newVersion)
                {
                    return true;
                }

                return false;
            }

            return VersionComparer.Compare(Port.InstalledVersion, _release?.Version, "<");
        }
    }

    /// <summary>
    /// Can port be installed
    /// </summary>
    public bool CanBeInstalled => !IsInProgress && !IsCheckingForUpdates && _release is not null;

    /// <summary>
    /// Download/install progress
    /// </summary>
    [ObservableProperty]
    private float _progressBarValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanBeInstalled))]
    private bool _isInProgress;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanBeInstalled))]
    private bool _isCheckingForUpdates;

    #endregion


    #region Relay Commands

    /// <summary>
    /// Initialize VM
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            IsCheckingForUpdates = true;

            _release = await _apiInterface.GetLatestPortReleaseAsync(Port.PortEnum).ConfigureAwait(true);
        }
        finally
        {
            IsCheckingForUpdates = false;

            OnPropertyChanged(nameof(LatestVersion));
            OnPropertyChanged(nameof(InstallButtonText));
            OnPropertyChanged(nameof(IsUpdateAvailable));
            OnPropertyChanged(nameof(IsInstalled));
            OnPropertyChanged(nameof(CanBeInstalled));

            PortChangedEvent?.Invoke(Port.PortEnum);
        }
    }


    /// <summary>
    /// Download and install port
    /// </summary>
    [RelayCommand]
    private async Task InstallAsync()
    {
        PortInstaller? installer = null;

        try
        {
            IsInProgress = true;

            installer = _installerFactory.Create(Port);

            installer.Progress.ProgressChanged += OnProgressChanged;
            ProgressBarValue = 0;

            var isInstalled = await installer.InstallAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            NotificationsHelper.Show(
                "Critical error! Exception is written to the log.",
                NotificationType.Error
                );

            _logger.LogCritical(ex, $"Error while installing port {Port.PortEnum}");
        }
        finally
        {
            installer?.Progress.ProgressChanged -= OnProgressChanged;
            IsInProgress = false;
            ProgressBarValue = 0;
            PortChangedEvent?.Invoke(Port.PortEnum);

            OnPropertyChanged(nameof(Version));
            OnPropertyChanged(nameof(InstallButtonText));
            OnPropertyChanged(nameof(IsUpdateAvailable));
            OnPropertyChanged(nameof(IsInstalled));
        }
    }


    /// <summary>
    /// Force check for updates
    /// </summary>
    [RelayCommand]
    private void Uninstall()
    {
        try
        {
            IsInProgress = true;

            var installer = _installerFactory.Create(Port);
            installer.Uninstall();
        }
        catch (Exception ex)
        {
            NotificationsHelper.Show(
                "Critical error! Exception is written to the log.",
                NotificationType.Error
                );

            _logger.LogCritical(ex, $"Error while uninstalling port {Port.PortEnum}");
        }
        finally
        {
            IsInProgress = false;
            PortChangedEvent?.Invoke(Port.PortEnum);

            OnPropertyChanged(nameof(Version));
            OnPropertyChanged(nameof(InstallButtonText));
            OnPropertyChanged(nameof(IsUpdateAvailable));
            OnPropertyChanged(nameof(IsInstalled));
        }
    }


    /// <summary>
    /// Open port folder
    /// </summary>
    [RelayCommand]
    private void OpenFolder()
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = Port.InstallFolderPath,
            UseShellExecute = true,
        });
    }


    #endregion


    private void OnProgressChanged(object? sender, float e)
    {
        ProgressBarValue = e;
    }
}
