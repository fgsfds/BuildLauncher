using Addons.Providers;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.All.Enums;
using Core.Client.Helpers;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class GamePageViewModel : ObservableObject
{
    private readonly GameEnum _gameEnum;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GamePageViewModel" /> class.
    /// </summary>
    /// <param name="gameEnum">The game enum.</param>
    /// <param name="campaigns">The campaigns view model.</param>
    /// <param name="maps">The maps view model.</param>
    /// <param name="mods">The mods view model.</param>
    /// <param name="downloads">The downloads view model.</param>
    /// <param name="metadataProvider">The metadata provider.</param>
    /// <param name="downloadablesProvider">The downloadable addons provider.</param>
    public GamePageViewModel(
        GameEnum gameEnum,
        CampaignsViewModel campaigns,
        MapsViewModel? maps,
        ModsViewModel? mods,
        DownloadsViewModel downloads,
        MetadataProvider metadataProvider,
        DownloadableAddonsProvider downloadablesProvider
        )
    {
        ArgumentNullException.ThrowIfNull(metadataProvider);
        ArgumentNullException.ThrowIfNull(downloadablesProvider);

        _gameEnum = gameEnum;

        Campaigns = campaigns;
        Maps = maps;
        Mods = mods;
        Downloads = downloads;

        metadataProvider.MetadataUpdatedEvent += OnMetadataUpdated;
        metadataProvider.MetadataInitializedEvent += OnMetadataInitialized;
        //downloadablesProvider.AddonsChangedEvent += OnAddonsChanged;
    }

    /// <summary>
    ///     Gets or sets the campaigns view model.
    /// </summary>
    public CampaignsViewModel Campaigns { get; set; }

    /// <summary>
    ///     Gets or sets the maps view model.
    /// </summary>
    public MapsViewModel? Maps { get; set; }

    /// <summary>
    ///     Gets or sets the mods view model.
    /// </summary>
    public ModsViewModel? Mods { get; set; }

    /// <summary>
    ///     Gets or sets the downloads view model.
    /// </summary>
    public DownloadsViewModel Downloads { get; set; }

    /// <summary>
    ///     Gets or sets whether the campaigns alarm is shown.
    /// </summary>
    [ObservableProperty]
    public partial bool IsCampaignsAlarmShown { get; set; }

    /// <summary>
    ///     Gets or sets whether the maps alarm is shown.
    /// </summary>
    [ObservableProperty]
    public partial bool IsMapsAlarmShown { get; set; }

    /// <summary>
    ///     Gets or sets whether the mods alarm is shown.
    /// </summary>
    [ObservableProperty]
    public partial bool IsModsAlarmShown { get; set; }

    /// <summary>
    ///     Gets or sets whether the downloads alarm is shown.
    /// </summary>
    [ObservableProperty]
    public partial bool IsDownloadsAlarmShown { get; set; }

    /// <summary>
    ///     Handles the addons changed event.
    /// </summary>
    private void OnAddonsChanged(GameEnum gameEnum, AddonTypeEnum? addonType)
    {
        if (gameEnum != _gameEnum)
        {
            return;
        }

        IsDownloadsAlarmShown = Downloads.HasUpdates;
    }

    /// <summary>
    ///     Handles the metadata updated event.
    /// </summary>
    private void OnMetadataUpdated(object? sender, ParsedAddonFile e)
    {
        IsCampaignsAlarmShown = Campaigns.AddonsList.Any(x => x.IsMetadataUpdateAvailable);
        IsMapsAlarmShown = Maps?.AddonsList.Any(x => x.IsMetadataUpdateAvailable) ?? false;
        IsModsAlarmShown = Mods?.AddonsList.Any(x => x.IsMetadataUpdateAvailable) ?? false;
    }

    /// <summary>
    ///     Handles the metadata initialized event.
    /// </summary>
    private void OnMetadataInitialized(object? sender, EventArgs e)
    {
        IsCampaignsAlarmShown = Campaigns.AddonsList.Any(x => x.IsMetadataUpdateAvailable);
        IsMapsAlarmShown = Maps?.AddonsList.Any(x => x.IsMetadataUpdateAvailable) ?? false;
        IsModsAlarmShown = Mods?.AddonsList.Any(x => x.IsMetadataUpdateAvailable) ?? false;
    }
}
