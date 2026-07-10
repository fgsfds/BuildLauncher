using Core.All.Enums;
using Ports.Ports;
using Tools.Tools;

namespace Avalonia.Desktop.ViewModels;

public interface IViewModelsFactory
{
    MainWindowViewModel GetMainWindowViewModel();

    CampaignsViewModel GetCampaignsViewModel(GameEnum gameEnum);

    MapsViewModel GetMapsViewModel(GameEnum gameEnum);

    ModsViewModel GetModsViewModel(GameEnum gameEnum);

    DownloadsViewModel GetDownloadsViewModel(GameEnum gameEnum);

    PortViewModel GetPortViewModel(BasePort port);

    ToolViewModel GetToolViewModel(BaseTool tool);
}
