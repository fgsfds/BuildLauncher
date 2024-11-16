using Addons.Providers;
using Avalonia.Controls;
using Avalonia.Desktop.ViewModels;
using Avalonia.Input;
using Common.Client.Interfaces;
using Common.Helpers;
using CommunityToolkit.Mvvm.Input;

namespace Avalonia.Desktop.Controls;

public sealed partial class ModsControl : UserControl
{
    private ModsViewModel _viewModel;
    private InstalledAddonsProvider _installedAddonsProvider;

    public ModsControl()
    {
        InitializeComponent();

        _viewModel = null!;
        _installedAddonsProvider = null!;
    }

    /// <summary>
    /// Initialize control
    /// </summary>
    public void InitializeControl(
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        IConfigProvider configProvider
        )
    {
        DataContext.ThrowIfNotType<ModsViewModel>(out var viewModel);

        _viewModel = viewModel;
        _installedAddonsProvider = installedAddonsProviderFactory.GetSingleton(_viewModel.Game);

        RightPanel.InitializeControl(configProvider);

        AddContextMenuButtons(viewModel);
    }

    /// <summary>
    /// Add button to the right click menu
    /// </summary>
    private void AddContextMenuButtons(ModsViewModel viewModel)
    {
        ModsList.ContextMenu = new();

        var deleteButton = new MenuItem()
        {
            Header = "Delete",
            Padding = new(5),
            Command = new RelayCommand(() => viewModel.DeleteModCommand.Execute(null))
        };

        _ = ModsList.ContextMenu.Items.Add(deleteButton);
    }

    private async void FilesDataGrid_DropAsync(object sender, DragEventArgs e)
    {
        var files = e.Data.GetFiles();

        if (files is not null && files.Any())
        {
            var filePaths = files.Select(f => f.Path.LocalPath);

            foreach (var file in filePaths)
            {
                var isAdded = await _installedAddonsProvider.CopyAddonIntoFolder(file).ConfigureAwait(false);
            }
        }
    }
}
