using Common.Entities;
using Common.Enums;
using Common.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Games.Providers;
using System.Diagnostics;
using Tools.Installer;
using Tools.Tools;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class ToolViewModel : ObservableObject
{
    private readonly ToolsInstallerFactory _installerFactory;
    private readonly GamesProvider _gamesProvider;
    private readonly BaseTool _tool;
    private readonly ToolsReleasesProvider _toolsReleasesProvider;

    private GeneralReleaseEntity? _release;


    [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
    public ToolViewModel(
        ToolsInstallerFactory installerFactory,
        ToolsReleasesProvider toolsReleasesProvider,
        GamesProvider gamesProvider,
        BaseTool tool
        )
    {
        _installerFactory = installerFactory;
        _gamesProvider = gamesProvider;
        _toolsReleasesProvider = toolsReleasesProvider;
        _tool = tool;

        _gamesProvider.GameChangedEvent += OnGameChanged;
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
    /// Name of the tool
    /// </summary>
    public string Name => _tool.Name;

    /// <summary>
    /// Tool's icon
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

    /// <summary>
    /// Can tool be installed
    /// </summary>
    public bool CanBeInstalled => _tool.CanBeInstalled;

    public string? InstallText => _tool.InstallText;

    public bool IsInstallTextVisible => !_tool.IsInstalled && _tool.InstallText is not null;


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(InstallCommand))]
    [NotifyCanExecuteChangedFor(nameof(CheckUpdateCommand))]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    private bool _isInProgress;

    #endregion


    #region Relay Commands

    /// <summary>
    /// Initialize VM
    /// </summary>
    public async Task InitializeAsync()
    {
        _release = await _toolsReleasesProvider.GetLatestReleaseAsync(_tool);

        OnPropertyChanged(nameof(LatestVersion));
        OnPropertyChanged(nameof(InstallButtonText));
    }


    /// <summary>
    /// Download and install tool
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
    public bool InstallCommandCanExecute() => CanBeInstalled && !IsInProgress;


    /// <summary>
    /// Force check for updates
    /// </summary>
    [RelayCommand(CanExecute = nameof(CheckUpdateCommandCanExecute))]
    private async Task CheckUpdateAsync()
    {
        IsInProgress = true;

        _release = await _toolsReleasesProvider.GetLatestReleaseAsync(_tool);

        OnPropertyChanged(nameof(LatestVersion));
        OnPropertyChanged(nameof(InstallButtonText));

        IsInProgress = false;
    }
    public bool CheckUpdateCommandCanExecute() => !IsInProgress;


    /// <summary>
    /// Initialize VM
    /// </summary>
    [RelayCommand(CanExecute = nameof(StartCommandCanExecute))]
    public void Start()
    {
        var args = _tool.GetStartToolArgs();

        Process.Start(new ProcessStartInfo
        {
            FileName = _tool.FullPathToExe,
            UseShellExecute = true,
            WorkingDirectory = _tool.PathToExecutableFolder,
            Arguments = args
        });
    }
    public bool StartCommandCanExecute() => _tool.IsInstalled && _tool.CanBeLaunched && !IsInProgress;

    #endregion


    private void OnProgressChanged(object? sender, float e)
    {
        ProgressBarValue = e;
        OnPropertyChanged(nameof(ProgressBarValue));
    }

    private void OnGameChanged(GameEnum game)
    {
        if (game is GameEnum.Duke3D or GameEnum.Blood)
        {
            StartCommand.NotifyCanExecuteChanged();
        }
    }
}
