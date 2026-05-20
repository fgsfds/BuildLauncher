using Addons.Providers;
using Core.All.Enums;
using Core.Client.Providers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class GamePageViewModel : ObservableObject
{
    private readonly GameEnum _gameEnum;

    public CampaignsViewModel Campaigns { get; set; }

    public MapsViewModel? Maps { get; set; }

    public ModsViewModel? Mods { get; set; }

    public DownloadsViewModel Downloads { get; set; }

    [ObservableProperty]
    public partial bool IsCampaignsAlarmShown { get; set; }

    [ObservableProperty]
    public partial bool IsMapsAlarmShown { get; set; }

    [ObservableProperty]
    public partial bool IsModsAlarmShown { get; set; }

    [ObservableProperty]
    public partial bool IsDownloadsAlarmShown { get; set; }

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
        downloadablesProvider.AddonsChangedEvent += OnAddonsChanged;
    }

    private void OnAddonsChanged(GameEnum gameEnum, AddonTypeEnum? addonType)
    {
        if (gameEnum != _gameEnum)
        {
            return;
        }

        IsDownloadsAlarmShown = Downloads.HasUpdates;
    }

    private void OnMetadataUpdated(object? sender, ValueTuple<GameEnum, AddonTypeEnum, string>? e)
    {
        IsCampaignsAlarmShown = Campaigns.CampaignsList.Any(x => x.IsMetadataUpdateAvailable);
        IsMapsAlarmShown = Maps?.MapsList.Any(x => x.IsMetadataUpdateAvailable) ?? false;
        IsModsAlarmShown = Mods?.ModsList.Any(x => x.IsMetadataUpdateAvailable) ?? false;
    }
}
