using System.Collections.Immutable;
using Addons.Addons;
using Addons.Helpers;
using Addons.Providers;
using Avalonia.Desktop.Misc;
using CommunityToolkit.Mvvm.Input;
using Core.All.Enums;
using Core.Client.Interfaces;
using Core.Client.Providers;
using Games.Games;
using Games.Providers;
using Microsoft.Extensions.Logging;

namespace Avalonia.Desktop.ViewModels;

/// <summary>
///     ViewModel for managing and enabling/disabling mods for a selected game.
/// </summary>
public sealed partial class ModsViewModel : AddonListViewModelBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ModsViewModel" /> class.
    /// </summary>
    [Obsolete($"Don't create directly. Use {nameof(IViewModelsFactory)}.")]
    public ModsViewModel(
        BaseGame game,
        InstalledGamesProvider gamesProvider,
        PlaytimeProvider playtimeProvider,
        RatingProvider ratingProvider,
        MetadataProvider metadataProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        BitmapsCache bitmapsCache,
        IConfigProvider config,
        IAddonDropHelper addonInstaller,
        IFolderOpener folderOpener,
        IUserNotifier userNotifier,
        ILogger<ModsViewModel> logger
        ) : base(
        game,
        gamesProvider,
        playtimeProvider,
        ratingProvider,
        metadataProvider,
        installedAddonsProviderFactory,
        portStarter: null!,
        bitmapsCache,
        addonInstaller,
        folderOpener,
        userNotifier,
        config,
        logger
        ) { }

    /// <summary>
    ///     Gets the type of addon managed by this ViewModel.
    /// </summary>
    protected override AddonTypeEnum AddonType => AddonTypeEnum.Mod;

    /// <summary>
    ///     Gets the file system path to the mods directory.
    /// </summary>
    protected override string BaseFolderPath => Game.ModsFolderPath;

    /// <summary>
    ///     Gets a value indicating whether ports buttons are visible. Mods always return <see langword="false" />.
    /// </summary>
    public override bool IsPortsButtonsVisible => false;

    /// <summary>
    ///     Gets the list of installed autoload mods. No filtering is applied.
    /// </summary>
    public override ImmutableList<BaseAddon> AddonsList
    {
        get
        {
            return
            [
                .. _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod)
                                           .OfType<AutoloadMod>()
                                           .OrderBy(static x => x.Title)
            ];
        }
    }

    /// <summary>
    ///     Toggles the enabled state of a mod and refreshes the list.
    /// </summary>
    [RelayCommand]
    private void ModCheckboxPressed(object? obj)
    {
        if (obj is not AutoloadMod mod)
        {
            _logger.LogWarning("ModCheckboxPressed received unexpected type: {Type}", obj?.GetType().Name);

            return;
        }

        try
        {
            if (mod.IsEnabled)
            {
                _installedAddonsProvider.DisableAddon(mod.AddonId);
            }
            else
            {
                _installedAddonsProvider.EnableAddon(mod.AddonId);
            }

            OnPropertyChanged(nameof(AddonsList));
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "=== Error while toggling mod {ModId} ===", mod.AddonId.Id);
        }
    }
}
