﻿using Addons.Providers;
using Avalonia.Controls;
using Avalonia.Desktop.ViewModels;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Common.Client.Interfaces;
using Common.Enums;
using Games.Providers;
using Ports.Providers;

namespace Avalonia.Desktop.Views;

public sealed partial class MainWindow : Window
{
    private readonly ViewModelsFactory _vmFactory;
    private readonly InstalledPortsProvider _portsProvider;
    private readonly InstalledGamesProvider _gamesProvider;
    private readonly InstalledAddonsProviderFactory _installedAddonsProviderFactory;
    private readonly IConfigProvider _configProvider;


    public MainWindow()
    {
        InitializeComponent();

        _portsProvider = null!;
        _gamesProvider = null!;
        _installedAddonsProviderFactory = null!;
        _vmFactory = null!;
        _configProvider = null!;
    }

    public MainWindow(
        MainViewModel vm,
        InstalledGamesProvider gamesProvider,
        ViewModelsFactory vmFactory,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        InstalledPortsProvider portsProvider,
        IConfigProvider configProvider
        )
    {
        _portsProvider = portsProvider;
        _gamesProvider = gamesProvider;
        _installedAddonsProviderFactory = installedAddonsProviderFactory;
        _vmFactory = vmFactory;
        _configProvider = configProvider;

        DataContext = vm;
        InitializeComponent();

        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.HighQuality);

        InitializePages();

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
        else if (gamesProvider.IsNamInstalled)
        {
            NamTab.IsSelected = true;
        }
        else if (gamesProvider.IsWW2GIInstalled)
        {
            WW2GITab.IsSelected = true;
        }
        else if (gamesProvider.IsWitchavenInstalled)
        {
            WitchavenTab.IsSelected = true;
        }
        else if (gamesProvider.IsTekWarInstalled)
        {
            TekWarTab.IsSelected = true;
        }
        else
        {
            SettingsTab.IsSelected = true;
        }

        gamesProvider.GameChangedEvent += OnGameChangedEvent;

#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void OnGameChangedEvent(GameEnum gameEnum)
    {
        InitializePages();
    }

    private void InitializePages()
    {
        if (_gamesProvider.IsDukeInstalled && !DukePage.IsAlreadyInitialized)
        {
            DukePage.InitializeControl(GameEnum.Duke3D, _portsProvider, _vmFactory, _installedAddonsProviderFactory, _configProvider);
        }

        if (_gamesProvider.IsBloodInstalled && !BloodPage.IsAlreadyInitialized)
        {
            BloodPage.InitializeControl(GameEnum.Blood, _portsProvider, _vmFactory, _installedAddonsProviderFactory, _configProvider);
        }

        if (_gamesProvider.IsWangInstalled && !WangPage.IsAlreadyInitialized)
        {
            WangPage.InitializeControl(GameEnum.ShadowWarrior, _portsProvider, _vmFactory, _installedAddonsProviderFactory, _configProvider);
        }

        if (_gamesProvider.IsFuryInstalled && !FuryPage.IsAlreadyInitialized)
        {
            FuryPage.InitializeControl(GameEnum.Fury, _portsProvider, _vmFactory, _installedAddonsProviderFactory, _configProvider);
        }

        if (_gamesProvider.IsRedneckInstalled && !RedneckPage.IsAlreadyInitialized)
        {
            RedneckPage.InitializeControl(GameEnum.Redneck, _portsProvider, _vmFactory, _installedAddonsProviderFactory, _configProvider);
        }

        if (_gamesProvider.IsSlaveInstalled && !SlavePage.IsAlreadyInitialized)
        {
            SlavePage.InitializeControl(GameEnum.Exhumed, _portsProvider, _vmFactory, _installedAddonsProviderFactory, _configProvider);
        }

        if (_gamesProvider.IsNamInstalled && !NamPage.IsAlreadyInitialized)
        {
            NamPage.InitializeControl(GameEnum.NAM, _portsProvider, _vmFactory, _installedAddonsProviderFactory, _configProvider);
        }

        if (_gamesProvider.IsWW2GIInstalled && !WW2GIPage.IsAlreadyInitialized)
        {
            WW2GIPage.InitializeControl(GameEnum.WW2GI, _portsProvider, _vmFactory, _installedAddonsProviderFactory, _configProvider);
        }

        if (_gamesProvider.IsWitchavenInstalled && !WitchavenPage.IsAlreadyInitialized)
        {
            WitchavenPage.InitializeControl(GameEnum.Witchaven, _portsProvider, _vmFactory, _installedAddonsProviderFactory, _configProvider);
        }

        if (_gamesProvider.IsTekWarInstalled && !TekWarPage.IsAlreadyInitialized)
        {
            TekWarPage.InitializeControl(GameEnum.TekWar, _portsProvider, _vmFactory, _installedAddonsProviderFactory, _configProvider);
        }

        if (!StandalonePage.IsAlreadInitialized)
        {
            StandalonePage.InitializeControl(GameEnum.Standalone, _portsProvider, _installedAddonsProviderFactory, _vmFactory, _configProvider);
        }
    }
}
