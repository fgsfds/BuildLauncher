using System.Collections;
using Avalonia.Controls;
using Avalonia.Data;

namespace Avalonia.Desktop.Misc;

/// <summary>
///     Provides an attached property for binding <see cref="DataGrid.SelectedItems" /> to a list.
/// </summary>
public sealed class DataGridSelectedItemsProperty
{
    /// <summary>
    ///     Defines the SelectedItems attached property.
    /// </summary>
    public static readonly AttachedProperty<IList?> SelectedItemsProperty =
        AvaloniaProperty.RegisterAttached<DataGridSelectedItemsProperty, DataGrid, IList?>(
            "SelectedItems",
            defaultBindingMode: BindingMode.TwoWay
            );

    /// <summary>
    ///     Initializes static members of the <see cref="DataGridSelectedItemsProperty" /> class.
    /// </summary>
    /// <summary>
    ///     Initializes static members of the <see cref="DataGridSelectedItemsProperty" /> class.
    /// </summary>
    static DataGridSelectedItemsProperty()
    {
        var process = SelectedItemsProperty.Changed.AddClassHandler<DataGrid>((grid, e) =>
            {
                grid.SelectionChanged -= OnSelectionChanged;

                if (e.NewValue is IList list)
                {
                    list.Clear();

                    foreach (var item in grid.SelectedItems)
                    {
                        _ = list.Add(item);
                    }

                    grid.SelectionChanged += OnSelectionChanged;
                }
            }
            );
    }

    /// <summary>
    ///     Gets the selected items list from the specified object.
    /// </summary>
    /// <param name="obj">The object to get the value from.</param>
    /// <returns>The selected items list.</returns>
    public static IList? GetSelectedItems(AvaloniaObject obj) => obj.GetValue(SelectedItemsProperty);

    /// <summary>
    ///     Sets the selected items list on the specified object.
    /// </summary>
    /// <param name="obj">The object to set the value on.</param>
    /// <param name="value">The selected items list.</param>
    public static void SetSelectedItems(AvaloniaObject obj, IList? value) => obj.SetValue(SelectedItemsProperty, value);

    /// <summary>
    ///     Handles the selection changed event on the DataGrid.
    /// </summary>
    private static void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid grid &&
            GetSelectedItems(grid) is IList list)
        {
            list.Clear();

            foreach (var item in grid.SelectedItems)
            {
                _ = list.Add(item);
            }
        }
    }
}
