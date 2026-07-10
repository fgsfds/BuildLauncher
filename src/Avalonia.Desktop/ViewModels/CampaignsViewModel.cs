using System.Collections.Immutable;
using Addons.Addons;
using Addons.Helpers;
using Addons.Providers;
using Avalonia.Desktop.Helpers;
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
///     ViewModel for managing and launching campaign addons for a selected game.
/// </summary>
public sealed class CampaignsViewModel : AddonListViewModelBase
{
    private readonly SeparatorItem _separator = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="CampaignsViewModel" /> class.
    /// </summary>
    [Obsolete($"Don't create directly. Use {nameof(IViewModelsFactory)}.")]
    public CampaignsViewModel(
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
        ILogger<CampaignsViewModel> logger
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
    protected override AddonTypeEnum AddonType => AddonTypeEnum.TC;

    /// <summary>
    ///     Gets the file system path to the campaigns folder.
    /// </summary>
    protected override string BaseFolderPath => Game.CampaignsFolderPath;

    /// <inheritdoc />
    public override bool IsPortsButtonsVisible => true;

    /// <summary>
    ///     Gets the list of installed campaigns, sorted with favorites first
    ///     and filtered by the current <see cref="AddonListViewModelBase.SearchBoxText" />.
    /// </summary>
    public override ImmutableList<BaseAddon> AddonsList
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
                return
                [
                    .. favorites,
                    _separator,
                    .. list
                ];
            }

            return [.. list];
        }
    }

    /// <summary>
    ///     Called when <see cref="AddonListViewModelBase.SelectedAddon" /> changes. Updates options
    ///     and notifies the start command of a potential CanExecute change.
    /// </summary>
    protected override void OnSelectedAddonChanged()
    {
        OnPropertyChanged(nameof(SelectedAddonPlaytime));

        UpdateAddonOptions();

        StartAddonCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    ///     Starts the selected campaign with the specified port or custom port.
    /// </summary>
    /// <param name="command">
    ///     A <see cref="BasePort" /> or <see cref="CustomPort" /> to start with.
    /// </param>
    protected override async Task StartAddonCoreAsync(object? command)
    {
        if (SelectedAddon is null)
        {
            throw new NullReferenceException(nameof(SelectedAddon));
        }

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
            throw new ArgumentOutOfRangeException(nameof(command), command, $"Unsupported command value: {command}.");
        }
    }
}
