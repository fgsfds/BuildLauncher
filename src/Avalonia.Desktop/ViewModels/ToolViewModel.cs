using System.Diagnostics;
using Avalonia.Controls.Notifications;
using Avalonia.Desktop.Misc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.All.Enums;
using Core.All.Helpers;
using Core.All.Serializable.Downloadable;
using Core.Client.Interfaces;
using Microsoft.Extensions.Logging;
using Tools.Installer;
using Tools.Tools;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class ToolViewModel : ObservableObject
{
    /// <summary>
    ///     Represents the method that handles tool change events.
    /// </summary>
    /// <param name="toolEnum">The tool enum.</param>
    public delegate void ToolChanged(ToolEnum toolEnum);


    private readonly IApiInterface _apiInterface;

    private readonly ToolInstallerFactory _installerFactory;

    private readonly ILogger<ToolViewModel> _logger;

    /// <summary>
    ///     The latest release information.
    /// </summary>
    private GeneralReleaseJsonModel? _release;


    /// <summary>
    ///     Initializes a new instance of the <see cref="ToolViewModel" /> class.
    /// </summary>
    /// <param name="installerFactory">The tool installer factory.</param>
    /// <param name="apiInterface">The API interface.</param>
    /// <param name="tool">The tool.</param>
    /// <param name="logger">The logger.</param>
    [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
    public ToolViewModel(
        ToolInstallerFactory installerFactory,
        IApiInterface apiInterface,
        BaseTool tool,
        ILogger<ToolViewModel> logger
        )
    {
        _installerFactory = installerFactory;
        _apiInterface = apiInterface;
        _logger = logger;
        Tool = tool;
    }

    /// <summary>
    ///     Gets the tool.
    /// </summary>
    public BaseTool Tool { get; init; }

    /// <summary>
    ///     Occurs when the tool state changes.
    /// </summary>
    public event ToolChanged? ToolChangedEvent;


    /// <summary>
    ///     Handles the progress changed event.
    /// </summary>
    private void OnProgressChanged(object? sender, float e)
    {
        ProgressBarValue = e;
        OnPropertyChanged(nameof(ProgressBarValue));
    }


    #region Binding Properties

    /// <summary>
    ///     Text of the install button
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
    ///     Gets the name of the tool.
    /// </summary>
    public string Name => Tool.Name;

    /// <summary>
    ///     Gets the tool's icon identifier.
    /// </summary>
    public long IconId => Tool.IconId;

    /// <summary>
    ///     Gets the currently installed version.
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
    ///     Gets whether the tool is installed.
    /// </summary>
    public bool IsInstalled => Tool.IsInstalled;

    /// <summary>
    ///     Gets the latest available version.
    /// </summary>
    public string LatestVersion
    {
        get
        {
            if (_release?.Version is null)
            {
                return "Error";
            }

            if (Tool.ToolEnum is ToolEnum.XMapEdit or ToolEnum.DOSBlood)
            {
                return DateTime.Parse(_release.Version).ToString("dd.MM.yyyy");
            }

            return _release.Version;
        }
    }

    /// <summary>
    ///     Gets whether a new version of the tool is available.
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

            return VersionComparer.Compare(Tool.InstalledVersion, _release.Version, ComparisonOperatorEnum.LessThan);
        }
    }

    /// <summary>
    ///     Gets whether the tool can be installed.
    /// </summary>
    public bool CanBeInstalled => !IsInProgress && !IsCheckingForUpdates && Tool.CanBeInstalled && _release is not null;

    /// <summary>
    ///     Gets or sets the download/install progress.
    /// </summary>
    public float ProgressBarValue { get; set; }

    /// <summary>
    ///     Gets or sets whether an install operation is in progress.
    /// </summary>
    [ObservableProperty]
    private bool _isInProgress;

    /// <summary>
    ///     Gets or sets whether the view model is checking for updates.
    /// </summary>
    [ObservableProperty]
    private bool _isCheckingForUpdates;

    #endregion


    #region Relay Commands

    /// <summary>
    ///     Initialize VM
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
    ///     Download and install tool
    /// </summary>
    [RelayCommand]
    private async Task InstallAsync()
    {
        ToolInstaller? installer = null;

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
    ///     Delete tool
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
    ///     Force check for updates
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
    ///     Open tool folder
    /// </summary>
    [RelayCommand]
    private void OpenFolder()
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = Tool.InstallFolderPath,
            UseShellExecute = true
        });
    }

    #endregion
}
