using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace Avalonia.Desktop.Behaviors;

/// <summary>
///     Clears <see cref="ListBox" /> selection when clicking on empty space within the list.
/// </summary>
public class ClearSelectionOnEmptySpaceBehavior : Behavior<ListBox>
{
    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject?.PointerPressed += OnPointerPressed;
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject?.PointerPressed -= OnPointerPressed;
    }

    /// <summary>
    ///     Handles the pointer pressed event to clear selection when clicking on empty space.
    /// </summary>
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (AssociatedObject is null)
        {
            return;
        }

        var visualSource = e.Source as Visual;
        var clickedItem = visualSource?.FindAncestorOfType<ListBoxItem>();

        // if an actual item is clicked, let normal selection happen
        if (clickedItem is not null)
        {
            return;
        }

        // otherwise, clear selection
        AssociatedObject.SelectedItem = null;
        AssociatedObject.Focusable = true;
        _ = AssociatedObject.Focus();
        AssociatedObject.Focusable = false;

        e.Handled = true;
    }
}
