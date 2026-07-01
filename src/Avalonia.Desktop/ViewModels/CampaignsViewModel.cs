using System.Collections.Immutable;
using System.Diagnostics;
using Addons.Addons;
using Addons.Helpers;
using Addons.Providers;
using Avalonia.Controls.Notifications;
using Avalonia.Desktop.Helpers;
using Avalonia.Desktop.Misc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.All.Enums;
using Core.Client.Interfaces;
using Core.Client.Providers;
using Games.Games;
using Games.Providers;
using Microsoft.Extensions.Logging;
using Ports.Ports;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class CampaignsViewModel : RightPanelViewModel, IPortsButtonControl
{
    private readonly IAddonDropHelper _addonInstaller;

    private readonly IConfigProvider _config;

    /// <summary>
    ///     The downloadable addons provider.
    /// </summary>
    private readonly DownloadableAddonsProvider _downloadableAddonsProvider;

    private readonly InstalledGamesProvider _gamesProvider;

    /// <summary>
    ///     The installed addons provider.
    /// </summary>
    private readonly InstalledAddonsProvider _installedAddonsProvider;

    private readonly ILogger<CampaignsViewModel> _logger;

    private readonly MetadataProvider _metadataProvider;

    private readonly PortStarter _portStarter;

    /// <summary>
    ///     The separator item used between favorites and regular items.
    /// </summary>
    private readonly SeparatorItem _separator = new();


    /// <summary>
    ///     Initializes a new instance of the <see cref="CampaignsViewModel" /> class.
    /// </summary>
    /// <param name="game">The game.</param>
    /// <param name="gamesProvider">The installed games provider.</param>
    /// <param name="config">The configuration provider.</param>
    /// <param name="playtimeProvider">The playtime provider.</param>
    /// <param name="ratingProvider">The rating provider.</param>
    /// <param name="metadataProvider">The metadata provider.</param>
    /// <param name="installedAddonsProviderFactory">The installed addons provider factory.</param>
    /// <param name="downloadableAddonsProviderFactory">The downloadable addons provider factory.</param>
    /// <param name="portStarter">The port starter.</param>
    /// <param name="bitmapsCache">The bitmaps cache.</param>
    /// <param name="addonInstaller">The addon drop helper.</param>
    /// <param name="logger">The logger.</param>
    [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
    public CampaignsViewModel(
        BaseGame game,
        InstalledGamesProvider gamesProvider,
        IConfigProvider config,
        PlaytimeProvider playtimeProvider,
        RatingProvider ratingProvider,
        MetadataProvider metadataProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory downloadableAddonsProviderFactory,
        PortStarter portStarter,
        BitmapsCache bitmapsCache,
        IAddonDropHelper addonInstaller,
        ILogger<CampaignsViewModel> logger
        ) : base(playtimeProvider, ratingProvider, metadataProvider, bitmapsCache, config)
    {
        Game = game;

        _gamesProvider = gamesProvider;
        _config = config;
        _installedAddonsProvider = installedAddonsProviderFactory.Get(game);
        _downloadableAddonsProvider = downloadableAddonsProviderFactory.Get(game);
        _portStarter = portStarter;
        _metadataProvider = metadataProvider;
        _addonInstaller = addonInstaller;
        _logger = logger;

        _gamesProvider.GameChangedEvent += OnGameChanged;
        _installedAddonsProvider.AddonsChangedEvent += OnAddonChanged;
        //_downloadableAddonsProvider.AddonsChangedEvent += OnAddonChanged;
    }

    /// <summary>
    ///     Gets the game associated with this view model.
    /// </summary>
    public BaseGame Game { get; }


    /// <summary>
    ///     VM initialization
    /// </summary>
    public Task InitializeAsync() => UpdateAsync(false);

    /// <summary>
    ///     Updates the campaign list asynchronously.
    /// </summary>
    /// <param name="createNew">Whether to create a new cache.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task UpdateAsync(bool createNew)
    {
        IsInProgress = true;
        await _installedAddonsProvider.CreateCacheAsync(createNew, AddonTypeEnum.TC).ConfigureAwait(true);
        IsInProgress = false;
    }


    /// <summary>
    ///     Handles the game changed event.
    /// </summary>
    private void OnGameChanged(GameEnum parameterName)
    {
        if (parameterName == Game.GameEnum)
        {
            OnPropertyChanged(nameof(CampaignsList));
        }
    }

    /// <summary>
    ///     Handles the addon changed event.
    /// </summary>
    private void OnAddonChanged(GameEnum gameEnum, AddonTypeEnum? addonType)
    {
        if (gameEnum == Game.GameEnum && (addonType is AddonTypeEnum.TC))
        {
            OnPropertyChanged(nameof(CampaignsList));
        }
    }


    #region Binding Properties

    /// <summary>
    ///     List of installed campaigns and maps
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
                if (!isSearchEmpty && !addon.Title.Contains(SearchBoxText, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (addon.IsFavorite)
                {
                    favorites.Add(addon);

                    continue;
                }

                list.Add(addon);
            }

            if (favorites.Count > 0)
            {
                return [.. favorites, _separator, .. list];
            }

            return [.. list];
        }
    }

    /// <summary>
    ///     The currently selected addon.
    /// </summary>
    private BaseAddon? _selectedAddon;

    /// <summary>
    ///     Currently selected campaign
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
            OnPropertyChanged(nameof(IsMetadataUpdateAvailable));
            OnPropertyChanged(nameof(SelectedAddonPlaytime));
            OnPropertyChanged(nameof(IsPreviewVisible));

            UpdateAddonOptions();

            StartCampaignCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    ///     Search box text
    /// </summary>
    [ObservableProperty]
    /// <summary>
    ///     Search box text.
    /// </summary>
    [NotifyPropertyChangedFor(nameof(CampaignsList))]
    [NotifyCanExecuteChangedFor(nameof(ClearSearchBoxCommand))]
    private string _searchBoxText = string.Empty;

    /// <summary>
    ///     Is the form in progress.
    /// </summary>
    [ObservableProperty]
    private bool _isInProgress;

    /// <summary>
    ///     Gets whether the ports buttons panel is visible.
    /// </summary>
    public bool IsPortsButtonsVisible => true;

    #endregion


    #region Relay Commands

    /// <summary>
    ///     Start selected campaign
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
    ///     Open campaigns folder
    /// </summary>
    [RelayCommand]
    private void OpenFolder()
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = Game.CampaignsFolderPath,
            UseShellExecute = true
        });
    }


    /// <summary>
    ///     Refresh campaigns list
    /// </summary>
    [RelayCommand]
    private Task RefreshListAsync() => UpdateAsync(true);


    /// <summary>
    ///     Delete selected campaign
    /// </summary>
    [RelayCommand]
    private void DeleteCampaign()
    {
        ArgumentNullException.ThrowIfNull(SelectedAddon);

        _installedAddonsProvider.DeleteAddon(SelectedAddon);
    }


    /// <summary>
    ///     Clear search bar
    /// </summary>
    [RelayCommand(CanExecute = nameof(ClearSearchBoxCanExecute))]
    private void ClearSearchBox() => SearchBoxText = string.Empty;

    /// <summary>
    ///     Determines whether the clear search box command can execute.
    /// </summary>
    /// <returns>True if the search box text is not empty.</returns>
    private bool ClearSearchBoxCanExecute() => !string.IsNullOrEmpty(SearchBoxText);


    /// <summary>
    ///     Install dropped addon
    /// </summary>
    [RelayCommand]
    private Task ProcessDroppedFilesAsync(List<string> filePaths) => _addonInstaller.AddAddonsAsync(filePaths, Game);


    /// <summary>
    ///     Add selected campaign to favorites
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
    ///     Remove selected campaign from favorites
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


    /// <summary>
    ///     Updates metadata for selected campaign.
    /// </summary>
    public override async Task UpdateMetadataAsync(object? value)
    {
        if (value is BaseAddon addon) { }
        else if (value is null && SelectedAddon is not null)
        {
            addon = SelectedAddon;
        }
        else
        {
            throw new InvalidOperationException(value?.GetType().Name);
        }

        IsInProgress = true;

        if (addon.FileInfo is null)
        {
            throw new InvalidOperationException("Campaign file info is required for metadata update");
        }

        var result = await _metadataProvider.UpdateMetadataAsync(addon.FileInfo).ConfigureAwait(true);

        IsInProgress = false;

        if (result.IsSuccess)
        {
            NotificationsHelper.Show(
                "Metadata updated.",
                NotificationType.Success
                );
        }
        else
        {
            NotificationsHelper.Show(
                "Error while updating metadata.",
                NotificationType.Error
                );
        }
    }

    #endregion
}
