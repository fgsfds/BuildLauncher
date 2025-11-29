using System.Collections.Immutable;
using System.Diagnostics;
using Addons.Addons;
using Addons.Providers;
using Avalonia.Desktop.Misc;
using Common.All.Enums;
using Common.All.Helpers;
using Common.Client.Interfaces;
using Common.Client.Providers;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Games.Games;
using Games.Providers;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class ModsViewModel : RightPanelViewModel, IPortsButtonControl
{
    public readonly BaseGame Game;

    private readonly InstalledGamesProvider _gamesProvider;
    private readonly InstalledAddonsProvider _installedAddonsProvider;
    private readonly DownloadableAddonsProvider _downloadableAddonsProvider;


    [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
    public ModsViewModel(
        BaseGame game,
        InstalledGamesProvider gamesProvider,
        PlaytimeProvider playtimeProvider,
        RatingProvider ratingProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory downloadableAddonsProviderFactory,
        BitmapsCache bitmapsCache
        ) : base(playtimeProvider, ratingProvider, bitmapsCache)
    {
        Game = game;

        _gamesProvider = gamesProvider;
        _installedAddonsProvider = installedAddonsProviderFactory.GetSingleton(game);
        _downloadableAddonsProvider = downloadableAddonsProviderFactory.GetSingleton(game);

        _gamesProvider.GameChangedEvent += OnGameChanged;
        _installedAddonsProvider.AddonsChangedEvent += OnAddonChanged;
        _downloadableAddonsProvider.AddonDownloadedEvent += OnAddonChanged;
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
    public ImmutableList<AutoloadModEntity> ModsList => [.. _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod).Select(x => (AutoloadModEntity)x.Value).OrderBy(static x => x.Title)];

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
        Guard.IsNotNull(SelectedAddon);

        _installedAddonsProvider.DeleteAddon(SelectedAddon);
    }


    /// <summary>
    /// Enable/disable mod
    /// </summary>
    [RelayCommand]
    private void ModCheckboxPressed(object? obj)
    {
        obj.ThrowIfNotType<AutoloadModEntity>(out var mod);

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

    #endregion


    private void OnGameChanged(GameEnum parameterName)
    {
        if (parameterName == Game.GameEnum)
        {
            OnPropertyChanged(nameof(ModsList));
        }
    }

    private void OnAddonChanged(GameEnum game, AddonTypeEnum addonType)
    {
        if (game == Game.GameEnum && (addonType is AddonTypeEnum.Mod))
        {
            OnPropertyChanged(nameof(ModsList));
        }
    }
}
