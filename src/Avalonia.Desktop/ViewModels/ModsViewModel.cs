using Common.Client.Providers;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.Input;
using Games.Providers;
using Mods.Addons;
using Mods.Providers;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class ModsViewModel : RightPanelViewModel, IPortsButtonControl
{
    public readonly IGame Game;

    private readonly GamesProvider _gamesProvider;
    private readonly InstalledAddonsProvider _installedAddonsProvider;
    private readonly DownloadableAddonsProvider _downloadableAddonsProvider;


    [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
    public ModsViewModel(
        IGame game,
        GamesProvider gamesProvider,
        PlaytimeProvider playtimeProvider,
        RatingProvider ratingProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        DownloadableAddonsProviderFactory _downloadableAddonsProviderFactory
        ) : base(playtimeProvider, ratingProvider)
    {
        Game = game;

        _gamesProvider = gamesProvider;
        _installedAddonsProvider = installedAddonsProviderFactory.GetSingleton(game);
        _downloadableAddonsProvider = _downloadableAddonsProviderFactory.GetSingleton(game);

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
        await _installedAddonsProvider.CreateCache(createNew).ConfigureAwait(true);
    }


    #region Binding Properties

    /// <summary>
    /// List of installed autoload mods
    /// </summary>
    public ImmutableList<AutoloadMod> ModsList => [.. _installedAddonsProvider.GetInstalledMods().Select(x => (AutoloadMod)x.Value).OrderBy(static x => x.Title)];

    private IAddon? _selectedAddon;
    /// <summary>
    /// Currently selected autoload mod
    /// </summary>
    public override IAddon? SelectedAddon
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

    public bool IsPortsButtonsVisible => false;

    #endregion


    #region Relay Commands

    /// <summary>
    /// Open mods folder
    /// </summary>
    [RelayCommand]
    private void OpenFolder()
    {
        _ = Process.Start(new ProcessStartInfo
        {
            FileName = Game.ModsFolderPath,
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
    /// Delete selected mod
    /// </summary>
    [RelayCommand]
    private void DeleteMod()
    {
        SelectedAddon.ThrowIfNull();

        _installedAddonsProvider.DeleteAddon(SelectedAddon);
    }


    /// <summary>
    /// Enable/disable mod
    /// </summary>
    [RelayCommand]
    private void ModCheckboxPressed(object? obj)
    {
        obj.ThrowIfNotType<AutoloadMod>(out var mod);

        if (!mod.IsEnabled)
        {
            var deps = ModsList.Where(x => x.DependentAddons?.ContainsKey(mod.Id) ?? false);

            _installedAddonsProvider.DisableAddon(new(mod.Id, mod.Version));

            foreach (var depMod in deps)
            {
                _installedAddonsProvider.DisableAddon(new(depMod.Id, depMod.Version));
            }
        }
        else if (mod.IsEnabled)
        {
            var dependencies = mod.DependentAddons;
            var depMods = dependencies is null ? [] : ModsList.Where(x => dependencies.ContainsKey(x.Id));

            _installedAddonsProvider.EnableAddon(new(mod.Id, mod.Version));

            foreach (var depMod in depMods)
            {
                _installedAddonsProvider.EnableAddon(new(depMod.Id, depMod.Version));
            }
        }

        //OnPropertyChanged(nameof(ModsList));
    }

    #endregion


    private void OnGameChanged(GameEnum parameterName)
    {
        if (parameterName == Game.GameEnum)
        {
            OnPropertyChanged(nameof(ModsList));
        }
    }

    private void OnAddonChanged(IGame game, AddonTypeEnum? addonType)
    {
        if (game.GameEnum == Game.GameEnum && (addonType is AddonTypeEnum.Mod || addonType is null))
        {
            OnPropertyChanged(nameof(ModsList));
        }
    }
}
