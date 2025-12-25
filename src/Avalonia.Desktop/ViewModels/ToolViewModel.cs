using System.Diagnostics;
using Avalonia.Controls.Notifications;
using Avalonia.Desktop.Misc;
using Common.All.Enums;
using Common.All.Helpers;
using Common.All.Serializable.Downloadable;
using Common.Client.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Tools.Installer;
using Tools.Tools;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class ToolViewModel : ObservableObject
{
    public BaseTool Tool { get; init; }

    private readonly ToolsInstallerFactory _installerFactory;
    private readonly IApiInterface _apiInterface;
    private GeneralReleaseJsonModel? _release;
    private readonly ILogger _logger;

    public delegate void ToolChanged(ToolEnum toolEnum);
    public event ToolChanged? ToolChangedEvent;


    [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
    public ToolViewModel(
        ToolsInstallerFactory installerFactory,
        IApiInterface apiInterface,
        BaseTool tool,
        ILogger logger
        )
    {
        _installerFactory = installerFactory;
        _apiInterface = apiInterface;
        _logger = logger;
        Tool = tool;
    }


    #region Binding Properties

    /// <summary>
    /// Text of the install button
    /// </summary>
    public string InstallButtonText
    {
        get
        {
            if (Tool.IsInstalled)
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
    /// Name of the tool
    /// </summary>
    public string Name => Tool.Name;

    /// <summary>
    /// Tool's icon
    /// </summary>
    public long IconId => Tool.IconId;

    /// <summary>
    /// Currently installed version
    /// </summary>
    public string Version
    {
        get
        {
            if (Tool.InstalledVersion is null)
            {
                return "None";
            }

            if (Tool.ToolEnum is ToolEnum.XMapEdit or ToolEnum.DOSBlood)
            {
                return DateTime.Parse(Tool.InstalledVersion).ToString("dd.MM.yyyy");
            }

            return Tool.InstalledVersion;
        }
    }

    /// <summary>
    /// Is tool installed
    /// </summary>
    public bool IsInstalled => Tool.IsInstalled;

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

            if (Tool.ToolEnum is ToolEnum.XMapEdit or ToolEnum.DOSBlood)
            {
                return DateTime.Parse(_release.Version).ToString("dd.MM.yyyy");
            }

            return _release?.Version ?? "Not available";
        }
    }

    /// <summary>
    /// Is new version of the tool available
    /// </summary>
    public bool IsUpdateAvailable
    {
        get
        {
            if (!Tool.IsInstalled)
            {
                return false;
            }

            if (_release is null)
            {
                return false;
            }

            return VersionComparer.Compare(Tool.InstalledVersion, _release?.Version, "<");
        }
    }

    /// <summary>
    /// Can tool be installed
    /// </summary>
    public bool CanBeInstalled => !IsInProgress && !IsCheckingForUpdates && Tool.CanBeInstalled && _release is not null;

    /// <summary>
    /// Download/install progress
    /// </summary>
    public float ProgressBarValue { get; set; }

    [ObservableProperty]
    private bool _isInProgress;

    [ObservableProperty]
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

            _release = await _apiInterface.GetLatestToolReleaseAsync(Tool.ToolEnum).ConfigureAwait(true);
        }
        finally
        {
            IsCheckingForUpdates = false;

            OnPropertyChanged(nameof(LatestVersion));
            OnPropertyChanged(nameof(InstallButtonText));
            OnPropertyChanged(nameof(IsUpdateAvailable));
            OnPropertyChanged(nameof(IsInstalled));
            OnPropertyChanged(nameof(CanBeInstalled));

            ToolChangedEvent?.Invoke(Tool.ToolEnum);
        }
    }


    /// <summary>
    /// Download and install tool
    /// </summary>
    [RelayCommand]
    private async Task InstallAsync()
    {
        ToolsInstaller? installer = null;

        try
        {
            IsInProgress = true;

            installer = _installerFactory.Create(Tool);

            installer.Progress.ProgressChanged += OnProgressChanged;
            ProgressBarValue = 0;
            OnPropertyChanged(nameof(ProgressBarValue));

            var isInstalled = await installer.InstallAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            NotificationsHelper.Show(
                "Critical error! Exception is written to the log.",
                NotificationType.Error
                );

            _logger.LogCritical(ex, $"Error while installing tool {Tool.ToolEnum}");
        }
        finally
        {
            installer?.Progress.ProgressChanged -= OnProgressChanged;
            ProgressBarValue = 0;
            IsInProgress = false;
            ToolChangedEvent?.Invoke(Tool.ToolEnum);

            OnPropertyChanged(nameof(ProgressBarValue));
            OnPropertyChanged(nameof(Version));
            OnPropertyChanged(nameof(InstallButtonText));
            OnPropertyChanged(nameof(IsUpdateAvailable));
            OnPropertyChanged(nameof(IsInstalled));
            OnPropertyChanged(nameof(CanBeInstalled));
        }
    }


    /// <summary>
    /// Delete tool
    /// </summary>
    [RelayCommand]
    private void Uninstall()
    {
        try
        {
            IsInProgress = true;

            var installer = _installerFactory.Create(Tool);
            installer.Uninstall();
        }
        catch (Exception ex)
        {
            NotificationsHelper.Show(
                "Critical error! Exception is written to the log.",
                NotificationType.Error
                );

            _logger.LogCritical(ex, $"Error while uninstalling tool {Tool.ToolEnum}");
        }
        finally
        {
            IsInProgress = false;
            ToolChangedEvent?.Invoke(Tool.ToolEnum);

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
    private void Launch()
    {
        try
        {
            var args = Tool.GetStartToolArgs();
            using var a = Process.Start(Tool.ToolExeFilePath, args);
            ProcessStartInfo psi = new();
        }
        catch (Exception ex)
        {
            NotificationsHelper.Show(
                "Critical error! Exception is written to the log.",
                NotificationType.Error
                );

            _logger.LogCritical(ex, $"Error while launching tool {Tool.ToolEnum}");
        }
    }


    /// <summary>
    /// Open tool folder
    /// </summary>
    [RelayCommand]
    private void OpenFolder()
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = Tool.InstallFolderPath,
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
