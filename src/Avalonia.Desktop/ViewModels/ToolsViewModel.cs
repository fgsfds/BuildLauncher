using System.Collections.Immutable;
using Common.All.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using Tools.Tools;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class ToolsViewModel : ObservableObject
{
    private readonly ViewModelsFactory _viewModelsFactory;

    [ObservableProperty]
    private bool _hasUpdates = false;

    public ImmutableList<ToolViewModel> ToolsList { get; set; } = [];

    public ToolsViewModel(
        ViewModelsFactory viewModelsFactory,
        IReadOnlyList<BaseTool> tools
        )
    {
        _viewModelsFactory = viewModelsFactory;

        Initialize(tools);
    }

    /// <summary>
    /// Initialize VM
    /// </summary>
    private void Initialize(IReadOnlyList<BaseTool> tools)
    {
        List<ToolViewModel> viewModels = new(tools.Count);

        foreach (var tool in tools)
        {
            var vm = _viewModelsFactory.GetToolViewModel(tool);
            vm.ToolChangedEvent += OnToolChanged;
            viewModels.Add(vm);
        }

        ToolsList = [.. viewModels];
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
