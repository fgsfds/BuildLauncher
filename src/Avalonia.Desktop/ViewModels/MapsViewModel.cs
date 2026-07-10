using System.Collections.Immutable;
using Addons.Addons;
using Addons.Helpers;
using Addons.Providers;
using Avalonia.Desktop.Misc;
using Core.All.Enums;
using Core.Client.Interfaces;
using Core.Client.Providers;
using Games.Games;
using Games.Providers;
using Microsoft.Extensions.Logging;
using Ports.Ports;

namespace Avalonia.Desktop.ViewModels;

/// <summary>
///     ViewModel for managing and launching map addons for a selected game.
/// </summary>
public sealed class MapsViewModel : AddonListViewModelBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MapsViewModel" /> class.
    /// </summary>
    [Obsolete($"Don't create directly. Use {nameof(IViewModelsFactory)}.")]
    public MapsViewModel(
        BaseGame game,
        InstalledGamesProvider gamesProvider,
        IConfigProvider config,
        PlaytimeProvider playtimeProvider,
        RatingProvider ratingProvider,
        MetadataProvider metadataProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        PortStarter portStarter,
        BitmapsCache bitmapsCache,
        IAddonDropHelper addonInstaller,
        IFolderOpener folderOpener,
        IUserNotifier userNotifier,
        ILogger<MapsViewModel> logger
        ) : base(
        game,
        gamesProvider,
        playtimeProvider,
        ratingProvider,
        metadataProvider,
        installedAddonsProviderFactory,
        portStarter,
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
    protected override AddonTypeEnum AddonType => AddonTypeEnum.Map;

    /// <summary>
    ///     Gets the file system path to the maps directory.
    /// </summary>
    protected override string BaseFolderPath => Game.MapsFolderPath;

    /// <inheritdoc />
    public override bool IsPortsButtonsVisible => true;

    /// <summary>
    ///     Gets the list of installed maps, filtered by the current
    ///     <see cref="AddonListViewModelBase.SearchBoxText" />.
    /// </summary>
    public override ImmutableList<BaseAddon> AddonsList
    {
        get
        {
            var result = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Map).OrderBy(static x => x.Title);

            if (string.IsNullOrWhiteSpace(SearchBoxText))
            {
                return [.. result];
            }

            return [.. result.Where(x => x.Title.Contains(SearchBoxText, StringComparison.CurrentCultureIgnoreCase))];
        }
    }

    /// <summary>
    ///     Called when <see cref="AddonListViewModelBase.SelectedAddon" /> changes. Updates options
    ///     and notifies the start command of a potential CanExecute change.
    /// </summary>
    protected override void OnSelectedAddonChanged()
    {
        UpdateAddonOptions();

        StartAddonCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    ///     Starts the selected map with the specified port.
    /// </summary>
    /// <param name="command">
    ///     A <see cref="BasePort" /> to start with.
    /// </param>
    protected override async Task StartAddonCoreAsync(object? command)
    {
        if (SelectedAddon is null)
        {
            throw new NullReferenceException(nameof(SelectedAddon));
        }

        if (command is not Tuple<BasePort, byte?> parameter)
        {
            throw new ArgumentException($"Expected {nameof(Tuple)} but received {command?.GetType().Name}.", nameof(command));
        }

        await _portStarter.StartAsync(
            parameter.Item1,
            Game,
            SelectedAddon,
            [],
            parameter.Item2,
            _config.SkipIntro,
            _config.SkipStartup
            ).ConfigureAwait(true);
    }
}
