using System.Collections.Immutable;
using System.Diagnostics;
using Addons.Addons;
using Addons.Helpers;
using Addons.Providers;
using Avalonia.Controls.Notifications;
using Avalonia.Desktop.Misc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.All.Enums;
using Core.Client.Interfaces;
using Core.Client.Providers;
using Games.Games;
using Games.Providers;
using Microsoft.Extensions.Logging;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class ModsViewModel : RightPanelViewModel, IPortsButtonControl
{
    /// <summary>
    ///     The addon drop helper for installing dropped files.
    /// </summary>
    private readonly IAddonDropHelper _addonInstaller;

    /// <summary>
    ///     The downloadable addons provider.
    /// </summary>
    private readonly DownloadableAddonsProvider _downloadableAddonsProvider;

    /// <summary>
    ///     The installed games provider.
    /// </summary>
    private readonly InstalledGamesProvider _gamesProvider;

    /// <summary>
    ///     The installed addons provider.
    /// </summary>
    private readonly InstalledAddonsProvider _installedAddonsProvider;

    /// <summary>
    ///     The logger.
    /// </summary>
    private readonly ILogger<ModsViewModel> _logger;

    /// <summary>
    ///     The metadata provider.
    /// </summary>
    private readonly MetadataProvider _metadataProvider;


    /// <summary>
    ///     Initializes a new instance of the <see cref="ModsViewModel" /> class.
    /// </summary>
    /// <param name="game">The game.</param>
    /// <param name="gamesProvider">The installed games provider.</param>
    /// <param name="playtimeProvider">The playtime provider.</param>
    /// <param name="ratingProvider">The rating provider.</param>
    /// <param name="metadataProvider">The metadata provider.</param>
    /// <param name="installedAddonsProviderFactory">The installed addons provider factory.</param>
    /// <param name="downloadableAddonsProviderFactory">The downloadable addons provider factory.</param>
    /// <param name="bitmapsCache">The bitmaps cache.</param>
    /// <param name="config">The configuration provider.</param>
    /// <param name="addonInstaller">The addon drop helper.</param>
    /// <param name="logger">The logger.</param>
    [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
    public ModsViewModel(
        BaseGame game,
        InstalledGamesProvider gamesProvider,
        PlaytimeProvider playtimeProvider,
        RatingProvider ratingProvider,
        MetadataProvider metadataProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory downloadableAddonsProviderFactory,
        BitmapsCache bitmapsCache,
        IConfigProvider config,
        IAddonDropHelper addonInstaller,
        ILogger<ModsViewModel> logger
        ) : base(playtimeProvider, ratingProvider, metadataProvider, bitmapsCache, config)
    {
        Game = game;

        _gamesProvider = gamesProvider;
        _installedAddonsProvider = installedAddonsProviderFactory.Get(game);
        _downloadableAddonsProvider = downloadableAddonsProviderFactory.Get(game);
        _metadataProvider = metadataProvider;
        _addonInstaller = addonInstaller;

        _gamesProvider.GameChangedEvent += OnGameChanged;
        _installedAddonsProvider.AddonsChangedEvent += OnAddonChanged;
        //_downloadableAddonsProvider.AddonsChangedEvent += OnAddonChanged;
    }

    /// <summary>
    ///     Gets the game associated with this view model.
    /// </summary>
    public BaseGame Game { get; }


    /// <summary>
    ///     VM initialization.
    /// </summary>
    public Task InitializeAsync() => UpdateAsync(false);

    /// <summary>
    ///     Updates the mods list asynchronously.
    /// </summary>
    /// <param name="createNew">Whether to create a new cache.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task UpdateAsync(bool createNew)
    {
        IsInProgress = true;
        await _installedAddonsProvider.CreateCacheAsync(createNew, AddonTypeEnum.Mod).ConfigureAwait(true);
        IsInProgress = false;
    }

    /// <summary>
    ///     Handles the game changed event.
    /// </summary>
    private void OnGameChanged(GameEnum parameterName)
    {
        if (parameterName == Game.GameEnum)
        {
            OnPropertyChanged(nameof(ModsList));
        }
    }

    /// <summary>
    ///     Handles the addon changed event.
    /// </summary>
    private void OnAddonChanged(GameEnum gameEnum, AddonTypeEnum? addonType)
    {
        if (gameEnum == Game.GameEnum && (addonType is AddonTypeEnum.Mod))
        {
            OnPropertyChanged(nameof(ModsList));
        }
    }


    #region Binding Properties

    /// <summary>
    ///     List of installed autoload mods
    /// </summary>
    public ImmutableList<AutoloadMod> ModsList => [.. _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod).OfType<AutoloadMod>().OrderBy(static x => x.Title)];

    /// <summary>
    ///     The currently selected addon.
    /// </summary>
    private BaseAddon? _selectedAddon;

    /// <summary>
    ///     Currently selected autoload mod
    /// </summary>
    public override BaseAddon? SelectedAddon
    {
        get => _selectedAddon;
        set
        {
            _selectedAddon = value;

            OnPropertyChanged(nameof(SelectedAddonDescription));
            OnPropertyChanged(nameof(SelectedAddonPreview));
            OnPropertyChanged(nameof(IsPreviewVisible));
            OnPropertyChanged(nameof(SelectedAddonRating));
            OnPropertyChanged(nameof(IsMetadataUpdateAvailable));
        }
    }

    /// <summary>
    ///     Is form in progress
    /// </summary>
    [ObservableProperty]
    private bool _isInProgress;

    /// <summary>
    ///     Gets whether the ports buttons panel is visible.
    /// </summary>
    public bool IsPortsButtonsVisible => false;

    #endregion


    #region Relay Commands

    /// <summary>
    ///     Open mods folder
    /// </summary>
    [RelayCommand]
    private void OpenFolder()
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = Game.ModsFolderPath,
            UseShellExecute = true
        });
    }


    /// <summary>
    ///     Refresh mods list
    /// </summary>
    [RelayCommand]
    private Task RefreshListAsync() => UpdateAsync(true);


    /// <summary>
    ///     Delete selected mod
    /// </summary>
    [RelayCommand]
    private void DeleteMod()
    {
        ArgumentNullException.ThrowIfNull(SelectedAddon);

        _installedAddonsProvider.DeleteAddon(SelectedAddon);
    }


    /// <summary>
    ///     Enable/disable mod
    /// </summary>
    [RelayCommand]
    private void ModCheckboxPressed(object? obj)
    {
        if (obj is not AutoloadMod mod)
        {
            throw new InvalidCastException();
        }

        //disabling
        if (mod.IsEnabled)
        {
            _installedAddonsProvider.DisableAddon(mod.AddonId);
        }
        //enabling
        else
        {
            _installedAddonsProvider.EnableAddon(mod.AddonId);
        }

        OnPropertyChanged(nameof(ModsList));
    }


    /// <summary>
    ///     Install dropped addon
    /// </summary>
    [RelayCommand]
    private Task ProcessDroppedFilesAsync(List<string> filePaths) => _addonInstaller.AddAddonsAsync(filePaths, Game);


    /// <summary>
    ///     Updates metadata for selected mod.
    /// </summary>
    public override async Task UpdateMetadataAsync(object? value)
    {
        if (value is not null
         && value is BaseAddon addon) { }
        else if (value is null
              && SelectedAddon is not null)
        {
            addon = SelectedAddon;
        }
        else
        {
            throw new InvalidOperationException(value?.GetType().Name);
        }

        if (addon.FileInfo is null)
        {
            throw new InvalidOperationException("Mod file info is required for metadata update");
        }

        IsInProgress = true;

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
