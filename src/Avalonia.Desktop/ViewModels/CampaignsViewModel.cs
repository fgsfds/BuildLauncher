using System.Collections.Immutable;
using System.Diagnostics;
using Addons.Addons;
using Addons.Providers;
using Avalonia.Controls.Notifications;
using Avalonia.Desktop.Helpers;
using Avalonia.Desktop.Misc;
using Common.All.Enums;
using Common.Client.Interfaces;
using Common.Client.Providers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Games.Games;
using Games.Providers;
using Microsoft.Extensions.Logging;
using Ports.Ports;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class CampaignsViewModel : RightPanelViewModel, IPortsButtonControl
{
    public BaseGame Game { get; }

    private readonly InstalledGamesProvider _gamesProvider;
    private readonly IConfigProvider _config;
    private readonly InstalledAddonsProvider _installedAddonsProvider;
    private readonly DownloadableAddonsProvider _downloadableAddonsProvider;
    private readonly PortStarter _portStarter;
    private readonly ILogger _logger;

    private readonly SeparatorItem _separator = new();


    #region Binding Properties

    /// <summary>
    /// List of installed campaigns and maps
    /// </summary>
    public ImmutableList<BaseAddon> CampaignsList
    {
        get
        {
            var addons = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);

            var isSearchEmpty = string.IsNullOrWhiteSpace(SearchBoxText);
            List<BaseAddon> favorites = [];
            List<BaseAddon> list = new(addons.Count);

            foreach (var addon in addons)
            {
                if (!isSearchEmpty && !addon.Value.Title.Contains(SearchBoxText, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (addon.Value.IsFavorite)
                {
                    favorites.Add(addon.Value);
                    continue;
                }

                list.Add(addon.Value);
            }

            if (favorites.Count > 0)
            {
                return [.. favorites, _separator, .. list];
            }

            return [.. list];
        }
    }

    private BaseAddon? _selectedAddon;
    /// <summary>
    /// Currently selected campaign
    /// </summary>
    public override BaseAddon? SelectedAddon
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

            UpdateAddonOptions();

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
        BaseGame game,
        InstalledGamesProvider gamesProvider,
        IConfigProvider config,
        PlaytimeProvider playtimeProvider,
        RatingProvider ratingProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory downloadableAddonsProviderFactory,
        PortStarter portStarter,
        BitmapsCache bitmapsCache,
        ILogger logger
        ) : base(playtimeProvider, ratingProvider, bitmapsCache, config)
    {
        Game = game;

        _gamesProvider = gamesProvider;
        _config = config;
        _installedAddonsProvider = installedAddonsProviderFactory.Get(game);
        _downloadableAddonsProvider = downloadableAddonsProviderFactory.Get(game);
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
        await _installedAddonsProvider.CreateCache(createNew, AddonTypeEnum.TC).ConfigureAwait(true);
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
            ArgumentNullException.ThrowIfNull(SelectedAddon);

            var enabledOptions = AddonOptions.Where(x => x.IsEnabled).Select(x => x.Name);

            if (command is BasePort port)
            {
                await _portStarter.StartAsync(
                    port,
                    Game,
                    SelectedAddon,
                    [.. enabledOptions],
                    null,
                    _config.SkipIntro,
                    _config.SkipStartup
                    ).ConfigureAwait(true);
            }
            else if (command is CustomPort customPort)
            {
                await _portStarter.StartAsync(
                    customPort.BasePort,
                    Game,
                    SelectedAddon,
                    [.. enabledOptions],
                    null,
                    _config.SkipIntro,
                    _config.SkipStartup,
                    customPort.Path
                    ).ConfigureAwait(true);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            OnPropertyChanged(nameof(SelectedAddonPlaytime));
        }
        catch (Exception ex)
        {
            NotificationsHelper.Show(
                "Critical error! Exception is written to the log.",
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
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = Game.CampaignsFolderPath,
            UseShellExecute = true,
        });
    }


    /// <summary>
    /// Refresh campaigns list
    /// </summary>
    [RelayCommand]
    private Task RefreshListAsync() => UpdateAsync(true);


    /// <summary>
    /// Delete selected campaign
    /// </summary>
    [RelayCommand]
    private void DeleteCampaign()
    {
        ArgumentNullException.ThrowIfNull(SelectedAddon);

        _installedAddonsProvider.DeleteAddon(SelectedAddon);
    }


    /// <summary>
    /// Clear search bar
    /// </summary>
    [RelayCommand(CanExecute = nameof(ClearSearchBoxCanExecute))]
    private void ClearSearchBox() => SearchBoxText = string.Empty;
    private bool ClearSearchBoxCanExecute() => !string.IsNullOrEmpty(SearchBoxText);


    /// <summary>
    /// Delete selected campaign
    /// </summary>
    [RelayCommand]
    private void AddToFavorite(object? value)
    {
        if (value is not BaseAddon addon)
        {
            throw new InvalidCastException();
        }

        _config.ChangeFavoriteState(addon.AddonId, true);
        addon.IsFavorite = true;

        OnPropertyChanged(nameof(CampaignsList));
    }


    /// <summary>
    /// Delete selected campaign
    /// </summary>
    [RelayCommand]
    private void RemoveFromFavorite(object? value)
    {
        if (value is not BaseAddon addon)
        {
            throw new InvalidCastException();
        }

        _config.ChangeFavoriteState(addon.AddonId, false);
        addon.IsFavorite = false;

        OnPropertyChanged(nameof(CampaignsList));
    }

    #endregion


    private void OnGameChanged(GameEnum parameterName)
    {
        if (parameterName == Game.GameEnum)
        {
            OnPropertyChanged(nameof(CampaignsList));
        }
    }

    private void OnAddonChanged(GameEnum game, AddonTypeEnum addonType)
    {
        if (game == Game.GameEnum && (addonType is AddonTypeEnum.TC))
        {
            OnPropertyChanged(nameof(CampaignsList));
        }
    }
}
