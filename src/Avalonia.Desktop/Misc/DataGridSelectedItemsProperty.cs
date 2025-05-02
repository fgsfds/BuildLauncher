using Avalonia.Controls;
using Avalonia.Data;
using System.Collections;

namespace Avalonia.Desktop.Misc;

public sealed class DataGridSelectedItemsProperty
{
    public static readonly AttachedProperty<IList?> SelectedItemsProperty =
        AvaloniaProperty.RegisterAttached<DataGridSelectedItemsProperty, DataGrid, IList?>(
            "SelectedItems",
            defaultBindingMode: BindingMode.TwoWay);

    public static IList? GetSelectedItems(AvaloniaObject obj) => obj.GetValue(SelectedItemsProperty);

    public static void SetSelectedItems(AvaloniaObject obj, IList? value) => obj.SetValue(SelectedItemsProperty, value);

    static DataGridSelectedItemsProperty()
    {
        _ = SelectedItemsProperty.Changed.AddClassHandler<DataGrid>((grid, e) =>
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
        });
    }

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
