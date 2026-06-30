using Addons.Addons;
using Avalonia.Controls;
using Avalonia.Desktop.ViewModels;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using Core.All.Enums;

namespace Avalonia.Desktop.Controls;

public sealed partial class ModsControl : UserControl
{
    private readonly ModsViewModel _viewModel = null!;

    public ModsControl()
    {
        InitializeComponent();
    }

    public ModsControl(ModsViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
    }

    private void ContextMenuOpened(object? sender, RoutedEventArgs e)
    {
        if (ModsList.SelectedItem is not BaseAddon addon)
        {
            e.Handled = true;

            return;
        }

        if (ModsList.ContextMenu is not null)
        {
            ModsList.ContextMenu.Items.Clear();
        }

        if (addon.IsMetadataUpdateAvailable)
        {
            var updateMetadataButton = new MenuItem()
            {
                Header = "Update metadata",
                Padding = new(5),
                Command = new AsyncRelayCommand(async () => await _viewModel.UpdateMetadataAsync(addon).ConfigureAwait(true))
            };

            _ = ModsList.ContextMenu.Items.Add(updateMetadataButton);
            _ = ModsList.ContextMenu.Items.Add(new Separator());
        }

        var deleteButton = new MenuItem()
        {
            Header = "Delete",
            Padding = new(5),
            Command = new RelayCommand(
                () => _viewModel.DeleteModCommand.Execute(null),
                () => addon.Type is not AddonTypeEnum.Official
                )
        };

        _ = ModsList.ContextMenu.Items.Add(deleteButton);
    }

    private void ContextMenuClosed(object? sender, RoutedEventArgs e)
    {
        if (ModsList.ContextMenu is not null)
        {
            ModsList.ContextMenu.Items.Clear();
        }
    }
}
