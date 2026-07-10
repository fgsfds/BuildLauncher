using System.Collections.Immutable;
using Addons.Addons;
using Addons.Helpers;
using Addons.Providers;
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

/// <summary>
///     Base class for ViewModels that display and manage a list of installed addons
///     (campaigns, maps, or mods) for a specific game.
/// </summary>
public abstract partial class AddonListViewModelBase : RightPanelViewModel, IPortsButtonControl
{
    protected readonly InstalledAddonsProvider _installedAddonsProvider;
    protected readonly ILogger _logger;
    protected readonly PortStarter _portStarter;
    protected readonly IConfigProvider _config;

    private readonly IAddonDropHelper _addonInstaller;
    private readonly InstalledGamesProvider _gamesProvider;
    private readonly MetadataProvider _metadataProvider;
    private readonly IFolderOpener _folderOpener;
    private readonly IUserNotifier _userNotifier;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonListViewModelBase" /> class.
    /// </summary>
    /// <param name="game">
    ///     The game associated with this panel.
    /// </param>
    /// <param name="gamesProvider">
    ///     The installed games provider.
    /// </param>
    /// <param name="playtimeProvider">
    ///     The playtime provider.
    /// </param>
    /// <param name="ratingProvider">
    ///     The rating provider.
    /// </param>
    /// <param name="metadataProvider">
    ///     The metadata provider.
    /// </param>
    /// <param name="installedAddonsProviderFactory">
    ///     Factory to create the installed addons provider for the given game.
    /// </param>
    /// <param name="portStarter">
    ///     The port starter for launching addons, or null if not supported.
    /// </param>
    /// <param name="bitmapsCache">
    ///     The bitmaps cache.
    /// </param>
    /// <param name="addonInstaller">
    ///     The addon drop helper for installing dropped files.
    /// </param>
    /// <param name="folderOpener">
    ///     Service to open OS file explorer folders.
    /// </param>
    /// <param name="userNotifier">
    ///     Service to show user notifications.
    /// </param>
    /// <param name="config">
    ///     The configuration provider.
    /// </param>
    /// <param name="logger">
    ///     The logger instance.
    /// </param>
    protected AddonListViewModelBase(
        BaseGame game,
        InstalledGamesProvider gamesProvider,
        PlaytimeProvider playtimeProvider,
        RatingProvider ratingProvider,
        MetadataProvider metadataProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        PortStarter portStarter,
        BitmapsCache bitmapsCache,
        IAddonDropHelper addonInstaller,
        IFolderOpener folderOpener,
        IUserNotifier userNotifier,
        IConfigProvider config,
        ILogger logger
        ) : base(playtimeProvider, ratingProvider, metadataProvider, bitmapsCache, config)
    {
        Game = game;
        _gamesProvider = gamesProvider;
        _installedAddonsProvider = installedAddonsProviderFactory.Get(game);
        _metadataProvider = metadataProvider;
        _addonInstaller = addonInstaller;
        _folderOpener = folderOpener;
        _userNotifier = userNotifier;
        _portStarter = portStarter;
        _config = config;
        _logger = logger;

        _gamesProvider.GameChangedEvent += OnGameChanged;
        _installedAddonsProvider.AddonsChangedEvent += OnAddonChanged;
    }

    /// <summary>
    ///     Gets the game associated with this panel.
    /// </summary>
    public BaseGame Game { get; }

    /// <summary>
    ///     Gets the addon type that this panel manages.
    /// </summary>
    protected abstract AddonTypeEnum AddonType { get; }

    /// <summary>
    ///     Gets the file system path to the folder containing the managed addons.
    /// </summary>
    protected abstract string BaseFolderPath { get; }

    /// <summary>
    ///     Initializes the ViewModel by refreshing the addon cache.
    /// </summary>
    /// <returns>
    ///     A task representing the asynchronous operation.
    /// </returns>
    public Task InitializeAsync() => UpdateAsync(false);

    private async Task UpdateAsync(bool createNew)
    {
        IsInProgress = true;
        await _installedAddonsProvider.CreateCacheAsync(createNew, AddonType).ConfigureAwait(true);
        IsInProgress = false;
    }

    private void OnGameChanged(GameEnum parameterName)
    {
        if (parameterName == Game.GameEnum)
        {
            OnPropertyChanged(nameof(AddonsList));
        }
    }

    private void OnAddonChanged(GameEnum gameEnum, AddonTypeEnum? addonType)
    {
        if (gameEnum == Game.GameEnum && addonType == AddonType)
        {
            OnPropertyChanged(nameof(AddonsList));
        }
    }


    #region Binding Properties

    /// <summary>
    ///     Gets the list of installed addons for the managed <see cref="AddonType" />.
    /// </summary>
    public virtual ImmutableList<BaseAddon> AddonsList
    {
        get => [.. _installedAddonsProvider.GetInstalledAddonsByType(AddonType)];
    }

    /// <summary>
    ///     Gets or sets the currently selected addon.
    /// </summary>
    public override BaseAddon? SelectedAddon
    {
        get;
        set
        {
            field = value;

            OnPropertyChanged(nameof(SelectedAddonDescription));
            OnPropertyChanged(nameof(SelectedAddonPreview));
            OnPropertyChanged(nameof(IsPreviewVisible));
            OnPropertyChanged(nameof(SelectedAddonRating));
            OnPropertyChanged(nameof(IsMetadataUpdateAvailable));

            OnSelectedAddonChanged();
        }
    }

    /// <summary>
    ///     Called when <see cref="SelectedAddon" /> changes. Override to add additional property change
    ///     notifications or command invalidation.
    /// </summary>
    protected virtual void OnSelectedAddonChanged() { }

    /// <summary>
    ///     Gets or sets the search box filter text.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AddonsList))]
    [NotifyCanExecuteChangedFor(nameof(ClearSearchBoxCommand))]
    public partial string SearchBoxText { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets whether a background operation is in progress.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AddonsList))]
    public partial bool IsInProgress { get; set; }

    /// <summary>
    ///     Gets whether the ports buttons panel should be visible.
    /// </summary>
    public abstract bool IsPortsButtonsVisible { get; }

    #endregion


    #region Relay Commands

    /// <summary>
    ///     Starts the selected addon with the specified port command.
    /// </summary>
    /// <param name="command">
    ///     The port or custom port to use for starting.
    /// </param>
    [RelayCommand]
    private async Task StartAddonAsync(object? command)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(SelectedAddon);

            await StartAddonCoreAsync(command).ConfigureAwait(true);

            OnPropertyChanged(nameof(SelectedAddonPlaytime));
        }
        catch (Exception ex)
        {
            _userNotifier.Show("Critical error! Exception is written to the log.", NotificationSeverity.Error);
            _logger.LogCritical(ex, $"=== Error while starting addon {SelectedAddon?.Title} ===");
        }
    }

    /// <summary>
    ///     Called by <see cref="StartAddonAsync" /> to execute the type-specific start logic.
    ///     Override to handle starting for the specific addon type.
    /// </summary>
    /// <param name="command">
    ///     The port or custom port to use for starting.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation.
    /// </returns>
    protected virtual Task StartAddonCoreAsync(object? command)
    {
        throw new NotSupportedException("This addon type does not support starting.");
    }

    /// <summary>
    ///     Opens the addon folder in the OS file explorer.
    /// </summary>
    [RelayCommand]
    private void OpenFolder() => _folderOpener.OpenFolder(BaseFolderPath);

    /// <summary>
    ///     Refreshes the addon list by recreating the cache.
    /// </summary>
    /// <returns>
    ///     A task representing the asynchronous operation.
    /// </returns>
    [RelayCommand]
    private Task RefreshListAsync() => UpdateAsync(true);

    /// <summary>
    ///     Deletes the currently selected addon.
    /// </summary>
    [RelayCommand]
    private void DeleteAddon()
    {
        ArgumentNullException.ThrowIfNull(SelectedAddon);

        _installedAddonsProvider.DeleteAddon(SelectedAddon);
    }

    /// <summary>
    ///     Clears the search box text.
    /// </summary>
    [RelayCommand(CanExecute = nameof(ClearSearchBoxCanExecute))]
    private void ClearSearchBox() => SearchBoxText = string.Empty;

    private bool ClearSearchBoxCanExecute() => !string.IsNullOrEmpty(SearchBoxText);

    /// <summary>
    ///     Processes dropped files by installing them as addons.
    /// </summary>
    /// <param name="filePaths">
    ///     The list of file paths to install.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation.
    /// </returns>
    [RelayCommand]
    private Task ProcessDroppedFilesAsync(List<string> filePaths) => _addonInstaller.AddAddonsAsync(filePaths, Game);

    /// <summary>
    ///     Updates metadata for the specified addon, or for the selected addon if null is passed.
    /// </summary>
    /// <param name="value">
    ///     The addon to update, or null to use the selected addon.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation.
    /// </returns>
    public override async Task UpdateMetadataAsync(object? value)
    {
        BaseAddon addon;

        if (value is BaseAddon v)
        {
            addon = v;
        }
        else if (value is null && SelectedAddon is not null)
        {
            addon = SelectedAddon;
        }
        else
        {
            throw new ArgumentException(
                $"Cannot update metadata. Unexpected type: {(value is null ? "null" : value.GetType().Name)}.",
                nameof(value)
                );
        }

        if (addon.FileInfo is null)
        {
            throw new InvalidOperationException("Cannot update metadata because file info is missing.");
        }

        IsInProgress = true;

        var result = await _metadataProvider.UpdateMetadataAsync(addon.FileInfo).ConfigureAwait(true);

        IsInProgress = false;

        if (result.IsSuccess)
        {
            _userNotifier.Show("Metadata updated.", NotificationSeverity.Success);
        }
        else
        {
            _userNotifier.Show("Error while updating metadata.", NotificationSeverity.Error);
        }
    }

    /// <summary>
    ///     Adds the specified campaign to favorites.
    /// </summary>
    [RelayCommand]
    public void AddToFavorite(object? value)
    {
        if (value is not BaseAddon addon)
        {
            throw new ArgumentException($"Expected {nameof(BaseAddon)} but received {value?.GetType().Name}.", nameof(value));
        }

        _config.ChangeFavoriteState(addon.AddonId, true);
        addon.IsFavorite = true;

        OnPropertyChanged(nameof(AddonsList));
    }

    /// <summary>
    ///     Removes the specified campaign from favorites.
    /// </summary>
    [RelayCommand]
    public void RemoveFromFavorite(object? value)
    {
        if (value is not BaseAddon addon)
        {
            throw new ArgumentException($"Expected {nameof(BaseAddon)} but received {value?.GetType().Name}.", nameof(value));
        }

        _config.ChangeFavoriteState(addon.AddonId, false);
        addon.IsFavorite = false;

        OnPropertyChanged(nameof(AddonsList));
    }

    #endregion
}
