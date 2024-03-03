using Avalonia.Controls;
using BuildLauncher.ViewModels;
using Common.DI;
using Common.Enums;
using Games.Providers;
using Microsoft.Extensions.DependencyInjection;
using Ports.Providers;

namespace BuildLauncher.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        var vm = BindingsManager.Provider.GetRequiredService<MainViewModel>();
        var gamesProvider = BindingsManager.Provider.GetRequiredService<GamesProvider>();
        var gameVmFactory = BindingsManager.Provider.GetRequiredService<GameViewModelFactory>();
        var portsProvider = BindingsManager.Provider.GetRequiredService<PortsProvider>();

        DataContext = vm;

        InitializeComponent();

        BloodPage.DataContext = gameVmFactory.Create(GameEnum.Blood);
        BloodPage.Init(portsProvider);

        DukePage.DataContext = gameVmFactory.Create(GameEnum.Duke3D);
        DukePage.Init(portsProvider);

        WangPage.DataContext = gameVmFactory.Create(GameEnum.Wang);
        WangPage.Init(portsProvider);

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
