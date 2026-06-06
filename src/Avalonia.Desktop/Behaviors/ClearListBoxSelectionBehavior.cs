using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace Avalonia.Desktop.Behaviors;

public class ClearSelectionOnEmptySpaceBehavior : Behavior<ListBox>
{
    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject?.PointerPressed += OnPointerPressed;
    }

    /// <inheritdoc/>
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject?.PointerPressed -= OnPointerPressed;
    }

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
