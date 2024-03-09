using Avalonia.Controls;
using BuildLauncher.ViewModels;
using Common.Enums;
using Games.Providers;
using Ports.Providers;

namespace BuildLauncher.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm, GamesProvider gamesProvider, ViewModelsFactory vmFactory, PortsProvider portsProvider)
    {
        DataContext = vm;

        InitializeComponent();

        BloodPage.InitializeControl(GameEnum.Blood, portsProvider, vmFactory);
        DukePage.InitializeControl(GameEnum.Duke3D, portsProvider, vmFactory);
        WangPage.InitializeControl(GameEnum.Wang, portsProvider, vmFactory);

        SettingsTab.IsSelected = true;

        //Set active tab depending on what games are installed
        if (gamesProvider.Blood.IsBaseGameInstalled)
        {
            SettingsTab.IsSelected = false;

            BloodTab.IsSelected = true;
        }
        else if (gamesProvider.Duke3D.IsBaseGameInstalled || gamesProvider.Duke3D.IsDuke64Installed)
        {
            SettingsTab.IsSelected = false;

            DukeTab.IsSelected = true;
        }
        else if (gamesProvider.Wang.IsBaseGameInstalled)
        {
            SettingsTab.IsSelected = false;

            WangTab.IsSelected = true;
        }
    }
}
