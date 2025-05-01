namespace Avalonia.Desktop.ViewModels;

public sealed class GamePageViewModel
{
    public CampaignsViewModel Campaigns { get; set; }

    public MapsViewModel? Maps { get; set; }

    public ModsViewModel? Mods { get; set; }

    public DownloadsViewModel Downloads { get; set; }

    public GamePageViewModel(
        CampaignsViewModel campaigns,
        MapsViewModel? maps,
        ModsViewModel? mods,
        DownloadsViewModel downloads)
    {
        Campaigns = campaigns;
        Maps = maps;
        Mods = mods;
        Downloads = downloads;
    }
}
