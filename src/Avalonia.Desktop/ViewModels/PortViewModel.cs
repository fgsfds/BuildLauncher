using Avalonia.Controls.Notifications;
using Avalonia.Desktop.Misc;
using Common.Entities;
using Common.Enums;
using Common.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Ports.Installer;
using Ports.Ports;
using Ports.Providers;
using System.Diagnostics;
using System.Globalization;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class PortViewModel : ObservableObject
{
    public BasePort Port { get; init; }

    private readonly PortsInstallerFactory _installerFactory;
    private readonly PortsReleasesProvider _portsReleasesProvider;
    private GeneralReleaseEntity? _release;
    private readonly ILogger _logger;

    public delegate void PortChanged(PortEnum portEnum);
    public event PortChanged? PortChangedEvent;


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
    public Stream Icon => Port.Icon;

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
            if (_release?.Version is null)
            {
                return "Not available";
            }

            if (Port.PortEnum is PortEnum.NotBlood)
            {
                return DateTime.Parse(_release.Version).ToString("dd.MM.yyyy");
            }

            return _release?.Version ?? "Not available";
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

                if (r1 && r2)
                {
                    if (currentVersion < newVersion)
                    {
                        return true;
                    }
                }

                return false;
            }

            return VersionComparer.Compare(Port.InstalledVersion, _release?.Version, "<");
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

        _release = await _portsReleasesProvider.GetLatestReleaseAsync(Port.PortEnum).ConfigureAwait(true);

        OnPropertyChanged(nameof(LatestVersion));
        OnPropertyChanged(nameof(InstallButtonText));
        OnPropertyChanged(nameof(IsUpdateAvailable));
        OnPropertyChanged(nameof(IsInstalled));
        OnPropertyChanged(nameof(CanBeInstalled));

        IsInProgress = false;

        PortChangedEvent?.Invoke(Port.PortEnum);
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

            await installer.InstallAsync(Port).ConfigureAwait(true);

            installer.Progress.ProgressChanged -= OnProgressChanged;
            ProgressBarValue = 0;

            OnPropertyChanged(nameof(ProgressBarValue));
            OnPropertyChanged(nameof(Version));
            OnPropertyChanged(nameof(InstallButtonText));
            OnPropertyChanged(nameof(IsUpdateAvailable));
            OnPropertyChanged(nameof(IsInstalled));

            UninstallCommand.NotifyCanExecuteChanged();
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
            IsInProgress = false;
            PortChangedEvent?.Invoke(Port.PortEnum);
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

            Directory.Delete(Port.PortInstallFolderPath, true);

            OnPropertyChanged(nameof(Version));
            OnPropertyChanged(nameof(InstallButtonText));
            OnPropertyChanged(nameof(IsUpdateAvailable));
            OnPropertyChanged(nameof(IsInstalled));
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
            FileName = Port.PortInstallFolderPath,
            UseShellExecute = true,
        });
    }


    #endregion


    private void OnProgressChanged(object? sender, float e)
    {
        ProgressBarValue = e;
        OnPropertyChanged(nameof(ProgressBarValue));
    }
}
