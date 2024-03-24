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
        if (gamesProvider.IsDukeInstalled)
        {
            DukeTab.IsSelected = true;
        }
        else if (gamesProvider.IsBloodInstalled)
        {
            BloodTab.IsSelected = true;
        }
        else if (gamesProvider.IsWangInstalled)
        {
            WangTab.IsSelected = true;
        }
        else if (gamesProvider.IsFuryInstalled)
        {
            FuryTab.IsSelected = true;
        }
        else if (gamesProvider.IsRedneckInstalled)
        {
            RedneckTab.IsSelected = true;
        }
        else if (gamesProvider.IsSlaveInstalled)
        {
            SlaveTab.IsSelected = true;
        }
        else
        {
            SettingsTab.IsSelected = true;
        }
    }
}
