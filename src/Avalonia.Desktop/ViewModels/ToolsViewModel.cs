using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.All.Enums;
using Tools.Tools;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class ToolsViewModel : ObservableObject
{
    private readonly ViewModelsFactory _viewModelsFactory;

    /// <summary>
    ///     Gets or sets whether there are tool updates available.
    /// </summary>
    [ObservableProperty]
    private bool _hasUpdates = false;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ToolsViewModel" /> class.
    /// </summary>
    /// <param name="viewModelsFactory">The view models factory.</param>
    /// <param name="tools">The available tools.</param>
    public ToolsViewModel(
        ViewModelsFactory viewModelsFactory,
        IEnumerable<BaseTool> tools
        )
    {
        _viewModelsFactory = viewModelsFactory;

        Initialize(tools);
    }

    /// <summary>
    ///     Gets or sets the list of tool view models.
    /// </summary>
    public ImmutableList<ToolViewModel> ToolsList { get; set; } = [];

    /// <summary>
    ///     Initialize VM
    /// </summary>
    /// <summary>
    ///     Initializes the tool view models.
    /// </summary>
    /// <param name="tools">The available tools.</param>
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

    /// <summary>
    ///     Handles the tool changed event.
    /// </summary>
    private void OnToolChanged(ToolEnum toolEnum)
    {
        var isUpdateAvailable = ToolsList.Find(x => x.Tool.ToolEnum == toolEnum)?.IsUpdateAvailable ?? false;

        if (!HasUpdates && isUpdateAvailable)
        {
            HasUpdates = true;
        }
    }
}
