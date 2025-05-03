using Addons.Providers;
using Avalonia.Controls.Notifications;
using Common.Client.Interfaces;
using Common.Client.Providers;
using Common.Enums;
using Common.Interfaces;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Games.Providers;
using Microsoft.Extensions.Logging;
using Ports.Ports;
using Ports.Providers;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class CampaignsViewModel : RightPanelViewModel, IPortsButtonControl
{
    public readonly IGame Game;

    private readonly InstalledGamesProvider _gamesProvider;
    private readonly IConfigProvider _config;
    private readonly InstalledAddonsProvider _installedAddonsProvider;
    private readonly DownloadableAddonsProvider _downloadableAddonsProvider;
    private readonly PortStarter _portStarter;
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
    private string _searchBoxText = string.Empty;

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
        InstalledGamesProvider gamesProvider,
        IConfigProvider config,
        PlaytimeProvider playtimeProvider,
        RatingProvider ratingProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory downloadableAddonsProviderFactory,
        PortStarter portStarter,
        ILogger logger
        ) : base(playtimeProvider, ratingProvider)
    {
        Game = game;

        _gamesProvider = gamesProvider;
        _config = config;
        _installedAddonsProvider = installedAddonsProviderFactory.GetSingleton(game);
        _downloadableAddonsProvider = downloadableAddonsProviderFactory.GetSingleton(game);
        _portStarter = portStarter;
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
    /// <param name="command">Port to start campaign with</param>
    [RelayCommand]
    private async Task StartCampaignAsync(object? command)
    {
        try
        {
            Guard.IsNotNull(SelectedAddon);

            if (command is BasePort port)
            {
                await _portStarter.StartAsync(port, Game, SelectedAddon, null, _config.SkipIntro, _config.SkipStartup).ConfigureAwait(true);
            }
            else if (command is CustomPort customPort)
            {
                await _portStarter.StartAsync(customPort.BasePort, Game, SelectedAddon, null, _config.SkipIntro, _config.SkipStartup, customPort.Path).ConfigureAwait(true);
            }
            else
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(command));
            }

            OnPropertyChanged(nameof(SelectedAddonPlaytime));
        }
        catch (Exception ex)
        {
            var length = App.Random.Next(1, 100);
            var repeatedString = new string('\u200B', length);

            App.NotificationManager.Show(
                "Critical error! Exception is written to the log." + repeatedString,
                NotificationType.Error
                );

            _logger.LogCritical(ex, $"=== Error while starting campaign {SelectedAddon?.Title} ===");
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
