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
    private readonly Dictionary<PortEnum, bool> _updatesList = [];
    private readonly object _lock = new();

    public bool HasUpdates
    {
        get
        {
            lock (_lock)
            {
                return _updatesList.Values.Any(x => x);
            }
        }
    }

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
        lock (_lock)
        {
            var edukeVm = _viewModelsFactory.GetPortViewModel(PortEnum.EDuke32);
            edukeVm.PortChangedEvent += OnPortChanged;
            _updatesList.Add(PortEnum.EDuke32, false);

            var razeVm = _viewModelsFactory.GetPortViewModel(PortEnum.Raze);
            razeVm.PortChangedEvent += OnPortChanged;
            _updatesList.Add(PortEnum.Raze, false);

            var nbloodVm = _viewModelsFactory.GetPortViewModel(PortEnum.NBlood);
            nbloodVm.PortChangedEvent += OnPortChanged;
            _updatesList.Add(PortEnum.NBlood, false);

            var notbloodVm = _viewModelsFactory.GetPortViewModel(PortEnum.NotBlood);
            notbloodVm.PortChangedEvent += OnPortChanged;
            _updatesList.Add(PortEnum.NotBlood, false);

            var pcexVm = _viewModelsFactory.GetPortViewModel(PortEnum.PCExhumed);
            pcexVm.PortChangedEvent += OnPortChanged;
            _updatesList.Add(PortEnum.PCExhumed, false);

            var rednukemVm = _viewModelsFactory.GetPortViewModel(PortEnum.RedNukem);
            rednukemVm.PortChangedEvent += OnPortChanged;
            _updatesList.Add(PortEnum.RedNukem, false);

            var bgdxVm = _viewModelsFactory.GetPortViewModel(PortEnum.BuildGDX);
            bgdxVm.PortChangedEvent += OnPortChanged;
            _updatesList.Add(PortEnum.BuildGDX, false);

            PortsList = [edukeVm, razeVm, nbloodVm, notbloodVm, pcexVm, rednukemVm, bgdxVm];
            OnPropertyChanged(nameof(PortsList));
        }
    }

    private void OnPortChanged(PortEnum portEnum)
    {
        _updatesList[portEnum] = PortsList.Find(x => x.Port.PortEnum == portEnum)!.IsUpdateAvailable;

        OnPropertyChanged(nameof(HasUpdates));
    }
}
