using Addons.Addons;
using Avalonia.Controls;
using Avalonia.Desktop.ViewModels;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using Core.All.Enums;

namespace Avalonia.Desktop.Controls;

/// <summary>
///     Displays and manages installed mods for a selected game.
/// </summary>
public sealed partial class ModsControl : AddonListControlBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ModsControl" /> class.
    /// </summary>
    public ModsControl() : base(null!)
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModsControl" /> class.
    /// </summary>
    /// <param name="viewModel">
    ///     The mods view model.
    /// </param>
    public ModsControl(ModsViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Handles the context menu opening event.
    /// </summary>
    private void ContextMenuOpened(object? sender, RoutedEventArgs e)
    {
        if (ModsList.SelectedItem is not BaseAddon addon)
        {
            e.Handled = true;

            return;
        }

        ClearContextMenu(ModsList.ContextMenu);

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
                () => _viewModel.DeleteAddonCommand.Execute(null),
                () => addon.Type is not AddonTypeEnum.Official
                )
        };

        _ = ModsList.ContextMenu.Items.Add(deleteButton);
    }

    /// <summary>
    ///     Handles the context menu closing event.
    /// </summary>
    private void ContextMenuClosed(object? sender, RoutedEventArgs e) => ClearContextMenu(ModsList.ContextMenu);
}
