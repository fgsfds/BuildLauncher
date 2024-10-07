using Common.Entities;
using Common.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using Ports.Installer;
using Ports.Providers;
using System.Collections.Immutable;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class PortsViewModel : ObservableObject
{
    private readonly PortsInstallerFactory _installerFactory;
    private readonly PortsReleasesProvider _portsReleasesProvider;
    private readonly ViewModelsFactory _viewModelsFactory;
    private readonly GeneralReleaseEntity? _release;

    [ObservableProperty]
    private bool _hasUpdates;

    public ImmutableList<PortViewModel> PortsList { get; set; } = [];


    [Obsolete($"Don't create directly. Use {nameof(ViewModelsFactory)}.")]
    public PortsViewModel(
        PortsInstallerFactory installerFactory,
        PortsReleasesProvider portsReleasesProvider,
        ViewModelsFactory viewModelsFactory
        )
    {
        _installerFactory = installerFactory;
        _portsReleasesProvider = portsReleasesProvider;
        _viewModelsFactory = viewModelsFactory;
    }


    /// <summary>
    /// Initialize VM
    /// </summary>
    public void Initialize()
    {
        var edukeVm = _viewModelsFactory.GetPortViewModel(PortEnum.EDuke32);
        edukeVm.PortChangedEvent += OnPortChanged;

        var razeVm = _viewModelsFactory.GetPortViewModel(PortEnum.Raze);
        razeVm.PortChangedEvent += OnPortChanged;

        var nbloodVm = _viewModelsFactory.GetPortViewModel(PortEnum.NBlood);
        nbloodVm.PortChangedEvent += OnPortChanged;

        var notbloodVm = _viewModelsFactory.GetPortViewModel(PortEnum.NotBlood);
        notbloodVm.PortChangedEvent += OnPortChanged;

        var pcexVm = _viewModelsFactory.GetPortViewModel(PortEnum.PCExhumed);
        pcexVm.PortChangedEvent += OnPortChanged;

        var rednukemVm = _viewModelsFactory.GetPortViewModel(PortEnum.RedNukem);
        rednukemVm.PortChangedEvent += OnPortChanged;

        var bgdxVm = _viewModelsFactory.GetPortViewModel(PortEnum.BuildGDX);
        bgdxVm.PortChangedEvent += OnPortChanged;

        PortsList = [edukeVm, razeVm, nbloodVm, notbloodVm, pcexVm, rednukemVm, bgdxVm];
        OnPropertyChanged(nameof(PortsList));
    }


    private void OnPortChanged(object? sender, EventArgs e)
    {
        HasUpdates = PortsList.Any(x => x.IsUpdateAvailable);
    }
}
