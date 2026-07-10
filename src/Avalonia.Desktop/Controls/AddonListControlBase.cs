using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Desktop.ViewModels;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;

namespace Avalonia.Desktop.Controls;

/// <summary>
///     Base class for addon list controls (campaigns, maps, mods)
///     that provides shared helpers for port buttons and context menus.
/// </summary>
public abstract class AddonListControlBase : UserControl
{
    protected readonly AddonListViewModelBase _viewModel;

    protected AddonListControlBase(AddonListViewModelBase viewModel)
    {
        _viewModel = viewModel;
    }

    /// <summary>
    ///     Notifies all port buttons in the panel to re-evaluate their CanExecute state.
    /// </summary>
    /// <param name="portsPanel">The panel containing the port buttons.</param>
    protected static void NotifyPortButtonsCanExecuteChanged(Panel portsPanel)
    {
        foreach (var control in portsPanel.Children)
        {
            if (control is Button button &&
                button.Command is IRelayCommand relayCommand)
            {
                relayCommand.NotifyCanExecuteChanged();
            }
        }
    }

    /// <summary>
    ///     Creates a <see cref="StackPanel" /> containing a port icon and short name.
    /// </summary>
    protected static StackPanel CreatePortButtonContent(Bitmap? icon, string name)
    {
        var sp = new StackPanel { Orientation = Orientation.Horizontal };

        if (icon is not null)
        {
            sp.Children.Add(new Image
            {
                Margin = new(0, 0, 5, 0),
                Height = 16,
                Source = icon
            });
        }

        sp.Children.Add(new TextBlock { Text = name });

        return sp;
    }

    /// <summary>
    ///     Finds a <see cref="Button" /> in the panel whose <see cref="Button.Content" />
    ///     is a <see cref="TextBlock" /> with the specified text.
    /// </summary>
    protected static Button? FindButtonByText(Panel panel, string text)
    {
        foreach (var child in panel.Children)
        {
            if (child is Button { Content: TextBlock tb } button && tb.Text?.Equals(text) is true)
            {
                return button;
            }
        }

        return null;
    }

    /// <summary>
    ///     Removes a <see cref="Button" /> with matching <see cref="TextBlock" /> content from the panel.
    /// </summary>
    protected static void RemoveButtonByText(Panel panel, string text)
    {
        var existing = FindButtonByText(panel, text);

        if (existing is not null)
        {
            panel.Children.Remove(existing);
        }
    }

    /// <summary>
    ///     Clears all items from the given context menu.
    /// </summary>
    protected static void ClearContextMenu(ContextMenu? contextMenu)
    {
        contextMenu?.Items.Clear();
    }

    /// <summary>
    /// Handles the ContextMenu opening event by determining if the operation should be canceled
    /// based on the current state of the selected addon in the ViewModel.
    /// </summary>
    /// <param name="sender">The source of the event, typically the control opening the ContextMenu.</param>
    /// <param name="e">The event data containing details about the cancelable ContextMenu opening operation.</param>
    protected void ContextMenuOpening(object? sender, CancelEventArgs e)
    {
        if (_viewModel.SelectedAddon is null)
        {
            e.Cancel = true;
        }
    }
}
