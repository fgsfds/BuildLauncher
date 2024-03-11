using Avalonia.Controls;
using BuildLauncher.ViewModels;
using Common.Enums;
using Games.Providers;
using Ports.Providers;

namespace BuildLauncher.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainViewModel vm, GamesProvider gamesProvider, ViewModelsFactory vmFactory, PortsProvider portsProvider)
    {
        DataContext = vm;

        InitializeComponent();

        BloodPage.InitializeControl(GameEnum.Blood, portsProvider, vmFactory);
        DukePage.InitializeControl(GameEnum.Duke3D, portsProvider, vmFactory);
        WangPage.InitializeControl(GameEnum.Wang, portsProvider, vmFactory);
        FuryPage.InitializeControl(GameEnum.Fury, portsProvider, vmFactory);
        RedneckPage.InitializeControl(GameEnum.Redneck, portsProvider, vmFactory);
        SlavePage.InitializeControl(GameEnum.Slave, portsProvider, vmFactory);


        //Set active tab depending on what games are installed
        if (gamesProvider.Blood.IsBaseGameInstalled)
        {
            BloodTab.IsSelected = true;
        }
        else if (gamesProvider.Duke3D.IsBaseGameInstalled || gamesProvider.Duke3D.IsDuke64Installed)
        {
            DukeTab.IsSelected = true;
        }
        else if (gamesProvider.Wang.IsBaseGameInstalled)
        {
            WangTab.IsSelected = true;
        }
        else if (gamesProvider.Fury.IsBaseGameInstalled)
        {
            FuryTab.IsSelected = true;
        }
        else if (gamesProvider.Redneck.IsBaseGameInstalled)
        {
            RedneckTab.IsSelected = true;
        }
        else if (gamesProvider.Slave.IsBaseGameInstalled)
        {
            SlaveTab.IsSelected = true;
        }
        else
        {
            SettingsTab.IsSelected = true;
        }
    }
}
