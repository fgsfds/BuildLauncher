using System.Collections.Immutable;
using Common.All.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class ToolsViewModel : ObservableObject
{
    private readonly ViewModelsFactory _viewModelsFactory;

    [ObservableProperty]
    private bool _hasUpdates = false;

    public ImmutableList<ToolViewModel> ToolsList { get; set; } = [];

    public ToolsViewModel(
        ViewModelsFactory viewModelsFactory
        )
    {
        _viewModelsFactory = viewModelsFactory;

        Initialize();
    }

    /// <summary>
    /// Initialize VM
    /// </summary>
    private void Initialize()
    {
        var edukeVm = _viewModelsFactory.GetToolViewModel(ToolEnum.XMapEdit);
        edukeVm.ToolChangedEvent += OnToolChanged;

        var razeVm = _viewModelsFactory.GetToolViewModel(ToolEnum.Mapster32);
        razeVm.ToolChangedEvent += OnToolChanged;

        var dosbloodVm = _viewModelsFactory.GetToolViewModel(ToolEnum.DOSBlood);
        dosbloodVm.ToolChangedEvent += OnToolChanged;

        ToolsList = [edukeVm, razeVm, dosbloodVm];
        OnPropertyChanged(nameof(ToolsList));
    }

    private void OnToolChanged(ToolEnum toolEnum)
    {
        var isUpdateAvailable = ToolsList.Find(x => x.Tool.ToolEnum == toolEnum)?.IsUpdateAvailable ?? false;

        if (!HasUpdates && isUpdateAvailable)
        {
            HasUpdates = true;
        }
    }
}
