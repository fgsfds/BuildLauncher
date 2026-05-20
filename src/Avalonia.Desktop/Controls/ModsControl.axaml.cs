using Addons.Addons;
using Addons.Providers;
using Avalonia.Controls;
using Avalonia.Desktop.Controls.Bases;
using Avalonia.Desktop.ViewModels;
using Core.All.Enums;
using CommunityToolkit.Mvvm.Input;

namespace Avalonia.Desktop.Controls;

public sealed partial class ModsControl : DroppableControl
{
    private readonly ModsViewModel _viewModel = null!;

    public ModsControl() : base(null!)
    {
        InitializeComponent();
    }

    public ModsControl(
        ModsViewModel viewModel,
        InstalledAddonsProvider installedAddonsProvider
        ) : base(installedAddonsProvider)
    {
        _viewModel = viewModel;

        InitializeComponent();
    }

    /// <summary>
    /// Add button to the right click menu
    /// </summary>
    private void AddContextMenuButtons()
    {
        ModsList.ContextMenu = new();

        if (ModsList.SelectedItem is not BaseAddon addon)
        {
            return;
        }

        ModsList.ContextMenu.Items.Clear();

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


    /// <summary>
    /// Update CanExecute for ports buttons and context menu buttons when selected campaign changed
    /// </summary>
    private void OnModsListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        AddContextMenuButtons();
    }


    /// <summary>
    /// Reset selected item when empty space is clicked
    /// </summary>
    private void OnListBoxEmptySpaceClicked(object? sender, Input.PointerPressedEventArgs e)
    {
        ModsList.SelectedItem = null;
        ModsList.Focusable = true;
        _ = ModsList.Focus();
        ModsList.Focusable = false;
    }
}
