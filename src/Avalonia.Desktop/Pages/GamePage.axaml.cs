using Avalonia.Controls;
using Avalonia.Desktop.ViewModels;
using Common.Client.Config;
using Common.Enums;
using Ports.Providers;

namespace Avalonia.Desktop.Pages;

public sealed partial class GamePage : UserControl
{
    public bool IsAlreadInitialized { get; private set; }

    public GamePage()
    {
        //preventing early setting of the wrong view model
        DataContext = null;
        InitializeComponent();
    }

    /// <summary>
    /// Initialize control
    /// </summary>
    public void InitializeControl(GameEnum gameEnum, PortsProvider portsProvider, ViewModelsFactory vmFactory, IConfigProvider configProvider)
    {
        CampControl.DataContext = vmFactory.GetCampaignsViewModel(gameEnum);
        MapssControl.DataContext = vmFactory.GetMapsViewModel(gameEnum);
        ModsControl.DataContext = vmFactory.GetModsViewModel(gameEnum);
        DownControl.DataContext = vmFactory.GetDownloadsViewModel(gameEnum);

        CampControl.InitializeControl(portsProvider, configProvider);
        MapssControl.InitializeControl(portsProvider, configProvider);
        ModsControl.InitializeControl(configProvider);
        DownControl.InitializeControl();

        IsAlreadInitialized = true;
    }
}
