using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.All.Enums;
using Tools.Tools;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class ToolsViewModel : ObservableObject
{
    private readonly ViewModelsFactory _viewModelsFactory;

    [ObservableProperty]
    private bool _hasUpdates = false;

    public ToolsViewModel(
        ViewModelsFactory viewModelsFactory,
        IEnumerable<BaseTool> tools
        )
    {
        _viewModelsFactory = viewModelsFactory;

        Initialize(tools);
    }

    public ImmutableList<ToolViewModel> ToolsList { get; set; } = [];

    /// <summary>
    ///     Initialize VM
    /// </summary>
    private void Initialize(IEnumerable<BaseTool> tools)
    {
        List<ToolViewModel> viewModels = [];

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
