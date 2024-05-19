using Avalonia.Controls;
using BuildLauncher.Controls;
using BuildLauncher.ViewModels;
using Common.Enums;
using Ports.Providers;

namespace BuildLauncher.Pages
{
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
        public void InitializeControl(GameEnum gameEnum, PortsProvider portsProvider, ViewModelsFactory vmFactory)
        {
            CampControl.DataContext = vmFactory.GetCampaignsViewModel(gameEnum);
            MapssControl.DataContext = vmFactory.GetMapsViewModel(gameEnum);
            ModsControl.DataContext = vmFactory.GetModsViewModel(gameEnum);
            DownControl.DataContext = vmFactory.GetDownloadsViewModel(gameEnum);

            CampControl.InitializeControl(portsProvider);
            MapssControl.InitializeControl(portsProvider);
            ModsControl.InitializeControl();
            DownControl.InitializeControl();

            IsAlreadInitialized = true;
        }
    }
}
