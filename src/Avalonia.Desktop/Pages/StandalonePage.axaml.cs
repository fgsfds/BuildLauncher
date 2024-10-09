using Avalonia.Controls;
using Avalonia.Desktop.ViewModels;
using Common.Client.Config;
using Common.Enums;
using Mods.Providers;
using Ports.Providers;

namespace Avalonia.Desktop.Pages;

public sealed partial class StandalonePage : UserControl
{
    public bool IsAlreadInitialized { get; private set; }

    public StandalonePage()
    {
        //preventing early setting of the wrong view model
        DataContext = null;
        InitializeComponent();
    }

    /// <summary>
    /// Initialize control
    /// </summary>
    public void InitializeControl(
        GameEnum gameEnum,
        PortsProvider portsProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        ViewModelsFactory vmFactory,
        IConfigProvider configProvider)
    {
        CampControl.DataContext = vmFactory.GetCampaignsViewModel(gameEnum);
        DownControl.DataContext = vmFactory.GetDownloadsViewModel(gameEnum);

        CampControl.InitializeControl(portsProvider, installedAddonsProviderFactory, configProvider);
        DownControl.InitializeControl();

        IsAlreadInitialized = true;
    }
}
