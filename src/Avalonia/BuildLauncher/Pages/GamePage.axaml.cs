using Avalonia.Controls;
using BuildLauncher.Controls;
using BuildLauncher.ViewModels;
using Common.Helpers;
using Ports.Providers;

namespace BuildLauncher.Pages
{
    public sealed partial class GamePage : UserControl
    {
        public GamePage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize control
        /// </summary>
        public void Init(PortsProvider portsProvider)
        {
            CampControl.Init(portsProvider);
            ModsControl.Init();
            DownControl.Init();

            DataContext.ThrowIfNotType<GameViewModel>(out var gameVM);

            gameVM.InitializeCommand.Execute(null);
        }
    }
}
