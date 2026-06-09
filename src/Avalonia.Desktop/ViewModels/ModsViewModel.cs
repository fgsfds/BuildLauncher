using System.Collections.Immutable;
using System.Diagnostics;
using Addons.Addons;
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
    public BaseGame Game { get; }

    private readonly InstalledGamesProvider _gamesProvider;
    private readonly InstalledAddonsProvider _installedAddonsProvider;
    private readonly IAddonDropHelper _addonInstaller;
    private readonly DownloadableAddonsProvider _downloadableAddonsProvider;
    private readonly MetadataProvider _metadataProvider;
    private readonly ILogger _logger;


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
        ILogger logger
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
        _downloadableAddonsProvider.AddonsChangedEvent += OnAddonChanged;
    }


    /// <summary>
    /// VM initialization
    /// </summary>
    public Task InitializeAsync() => UpdateAsync(false);

    /// <summary>
    /// Update mods list
    /// </summary>
    private async Task UpdateAsync(bool createNew)
    {
        IsInProgress = true;
        await _installedAddonsProvider.CreateCache(createNew, AddonTypeEnum.Mod).ConfigureAwait(true);
        IsInProgress = false;
    }


    #region Binding Properties

    /// <summary>
    /// List of installed autoload mods
    /// </summary>
    public ImmutableList<AutoloadMod> ModsList => [.. _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod).Select(x => (AutoloadMod)x.Value).OrderBy(static x => x.Title)];

    private BaseAddon? _selectedAddon;
    /// <summary>
    /// Currently selected autoload mod
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
    /// Is form in progress
    /// </summary>
    [ObservableProperty]
    private bool _isInProgress;

    public bool IsPortsButtonsVisible => false;

    #endregion


    #region Relay Commands

    /// <summary>
    /// Open mods folder
    /// </summary>
    [RelayCommand]
    private void OpenFolder()
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = Game.ModsFolderPath,
            UseShellExecute = true,
        });
    }


    /// <summary>
    /// Refresh campaigns list
    /// </summary>
    [RelayCommand]
    private Task RefreshListAsync() => UpdateAsync(true);


    /// <summary>
    /// Delete selected mod
    /// </summary>
    [RelayCommand]
    private void DeleteMod()
    {
        ArgumentNullException.ThrowIfNull(SelectedAddon);

        _installedAddonsProvider.DeleteAddon(SelectedAddon);
    }


    /// <summary>
    /// Enable/disable mod
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
    /// Install dropped addon
    /// </summary>
    [RelayCommand]
    private Task ProcessDroppedFilesAsync(List<string> filePaths) => _addonInstaller.AddAddonsAsync(filePaths, Game);


    /// <summary>
    /// Updates metadata for selected mod.
    /// </summary>
    public override async Task UpdateMetadataAsync(object? value)
    {
        if (value is not null
            && value is BaseAddon addon)
        {
        }
        else if (value is null
            && SelectedAddon is not null)
        {
            addon = SelectedAddon;
        }
        else
        {
            throw new InvalidOperationException(value?.GetType().Name);
        }

        IsInProgress = true;

        var result = await _metadataProvider.UpdateMetadataAsync(addon.PathToFile).ConfigureAwait(true);

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


    private void OnGameChanged(GameEnum parameterName)
    {
        if (parameterName == Game.GameEnum)
        {
            OnPropertyChanged(nameof(ModsList));
        }
    }

    private void OnAddonChanged(GameEnum gameEnum, AddonTypeEnum? addonType)
    {
        if (gameEnum == Game.GameEnum && (addonType is AddonTypeEnum.Mod))
        {
            OnPropertyChanged(nameof(ModsList));
        }
    }
}
