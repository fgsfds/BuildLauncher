using Avalonia.Controls.Notifications;
using Common.Client.Providers;
using Common.Enums;
using Common.Interfaces;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Games.Providers;
using Microsoft.Extensions.Logging;
using Mods.Addons;
using Mods.Providers;
using Ports.Ports;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class CampaignsViewModel : RightPanelViewModel, IPortsButtonControl
{
    public readonly IGame Game;

    private readonly GamesProvider _gamesProvider;
    private readonly IConfigProvider _config;
    private readonly PlaytimeProvider _playtimeProvider;
    private readonly InstalledAddonsProvider _installedAddonsProvider;
    private readonly DownloadableAddonsProvider _downloadableAddonsProvider;
    private readonly ILogger _logger;


    #region Binding Properties

    /// <summary>
    /// List of installed campaigns and maps
    /// </summary>
    public ImmutableList<IAddon> CampaignsList
    {
        get
        {
            var result = _installedAddonsProvider.GetInstalledCampaigns().Select(static x => x.Value);

            if (string.IsNullOrWhiteSpace(SearchBoxText))
            {
                return [.. result];
            }

            return [.. result.Where(x => x.Title.Contains(SearchBoxText, StringComparison.CurrentCultureIgnoreCase))];
        }
    }

    private IAddon? _selectedAddon;
    /// <summary>
    /// Currently selected campaign
    /// </summary>
    public override IAddon? SelectedAddon
    {
        get => _selectedAddon;
        set
        {
            _selectedAddon = value;

            OnPropertyChanged(nameof(SelectedAddonDescription));
            OnPropertyChanged(nameof(SelectedAddonPreview));
            OnPropertyChanged(nameof(SelectedAddonRating));
            OnPropertyChanged(nameof(SelectedAddonPlaytime));
            OnPropertyChanged(nameof(IsPreviewVisible));

            StartCampaignCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// Search box text
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CampaignsList))]
    [NotifyCanExecuteChangedFor(nameof(ClearSearchBoxCommand))]
    private string _searchBoxText;

    /// <summary>
    /// Is form in progress
    /// </summary>
    [ObservableProperty]
    private bool _isInProgress;

    public bool IsPortsButtonsVisible => true;

    #endregion


    [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
    public CampaignsViewModel(
        IGame game,
        GamesProvider gamesProvider,
        IConfigProvider config,
        PlaytimeProvider playtimeProvider,
        RatingProvider ratingProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory downloadableAddonsProviderFactory,
        ILogger logger
        ) : base(playtimeProvider, ratingProvider)
    {
        Game = game;

        _gamesProvider = gamesProvider;
        _config = config;
        _playtimeProvider = playtimeProvider;
        _installedAddonsProvider = installedAddonsProviderFactory.GetSingleton(game);
        _downloadableAddonsProvider = downloadableAddonsProviderFactory.GetSingleton(game);
        _logger = logger;

        _gamesProvider.GameChangedEvent += OnGameChanged;
        _installedAddonsProvider.AddonsChangedEvent += OnAddonChanged;
        _downloadableAddonsProvider.AddonDownloadedEvent += OnAddonChanged;
    }


    /// <summary>
    /// VM initialization
    /// </summary>
    public Task InitializeAsync() => UpdateAsync(false);

    /// <summary>
    /// Update campaigns list
    /// </summary>
    private async Task UpdateAsync(bool createNew)
    {
        IsInProgress = true;
        await _installedAddonsProvider.CreateCache(createNew).ConfigureAwait(true);
        IsInProgress = false;
    }


    #region Relay Commands

    /// <summary>
    /// Start selected campaign
    /// </summary>
    /// <param name="value">Port to start campaign with</param>
    [RelayCommand]
    private async Task StartCampaignAsync(object? value)
    {
        try
        {
            Guard.IsNotNull(SelectedAddon);

            if (value is BasePort port)
            {
                var mods = _installedAddonsProvider.GetInstalledMods();

                var args = port.GetStartGameArgs(Game, SelectedAddon, mods, _config.SkipIntro, _config.SkipStartup);
                var addon = SelectedAddon;

                _logger.LogInformation("=== Starting addon {SelectedAddon.Id} for {Game.FullName} ===", SelectedAddon.Id, Game.FullName);
                _logger.LogInformation("Path to port exe {port.PortExeFilePath}", port.PortExeFilePath);
                _logger.LogInformation("Startup args: {args}", args);

                await StartPortAsync(port, addon, args).ConfigureAwait(true);
            }
            else if (SelectedAddon is StandaloneAddon standalone)
            {
                await StartPortAsync(standalone).ConfigureAwait(true);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }
        catch (Exception ex)
        {
            var length = App.Random.Next(1, 100);
            var repeatedString = new string('\u200B', length);

            App.NotificationManager.Show(
                "Critical error! Exception is written to the log.",
                NotificationType.Error
                );

            _logger.LogCritical(ex, "=== Critical error ===");
        }
    }


    /// <summary>
    /// Open campaigns folder
    /// </summary>
    [RelayCommand]
    private void OpenFolder()
    {
        _ = Process.Start(new ProcessStartInfo
        {
            FileName = Game.CampaignsFolderPath,
            UseShellExecute = true,
        });
    }


    /// <summary>
    /// Refresh campaigns list
    /// </summary>
    [RelayCommand]
    private async Task RefreshListAsync()
    {
        await UpdateAsync(true).ConfigureAwait(true);
    }


    /// <summary>
    /// Delete selected campaign
    /// </summary>
    [RelayCommand]
    private void DeleteCampaign()
    {
        Guard.IsNotNull(SelectedAddon);

        _installedAddonsProvider.DeleteAddon(SelectedAddon);
    }


    /// <summary>
    /// Clear search bar
    /// </summary>
    [RelayCommand(CanExecute = nameof(ClearSearchBoxCanExecute))]
    private void ClearSearchBox() => SearchBoxText = string.Empty;
    private bool ClearSearchBoxCanExecute() => !string.IsNullOrEmpty(SearchBoxText);

    #endregion


    /// <summary>
    /// Start port with command line args
    /// </summary>
    /// <param name="port">Port</param>
    /// <param name="addon">Campaign</param>
    /// <param name="args">Command line arguments</param>
    private async Task StartPortAsync(BasePort port, IAddon addon, string args)
    {
        var sw = Stopwatch.StartNew();

        var portExe = addon.Executables?[OSEnum.Windows] is not null ? addon.Executables[OSEnum.Windows] : port.PortExeFilePath;

        await Process.Start(new ProcessStartInfo
        {
            FileName = Path.GetFileName(portExe),
            UseShellExecute = true,
            Arguments = args,
            WorkingDirectory = Path.GetDirectoryName(portExe)
        })!.WaitForExitAsync().ConfigureAwait(true);

        sw.Stop();
        var time = sw.Elapsed;

        _playtimeProvider.AddTime(addon.Id, time);

        OnPropertyChanged(nameof(SelectedAddonPlaytime));

        port.AfterEnd(Game, addon);
    }

    /// <summary>
    /// Start port with command line args
    /// </summary>
    /// <param name="addon">Campaign</param>
    private async Task StartPortAsync(StandaloneAddon addon)
    {
        var sw = Stopwatch.StartNew();

        await Process.Start(new ProcessStartInfo
        {
            FileName = Path.GetFileName(addon.Executables![OSEnum.Windows]),
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(addon.PathToFile)
        })!.WaitForExitAsync().ConfigureAwait(true);

        sw.Stop();
        var time = sw.Elapsed;

        _playtimeProvider.AddTime(addon.Id, time);

        OnPropertyChanged(nameof(SelectedAddonPlaytime));
    }

    private void OnGameChanged(GameEnum parameterName)
    {
        if (parameterName == Game.GameEnum)
        {
            OnPropertyChanged(nameof(CampaignsList));
        }
    }

    private void OnAddonChanged(IGame game, AddonTypeEnum? addonType)
    {
        if (game.GameEnum == Game.GameEnum && (addonType is AddonTypeEnum.TC || addonType is null))
        {
            OnPropertyChanged(nameof(CampaignsList));
        }
    }
}
