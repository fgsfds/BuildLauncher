using Avalonia.Controls;
using Common.Entities;

namespace Avalonia.Desktop.Controls;

public sealed partial class DownloadsControl : UserControl
{
    public DownloadsControl()
    {
        InitializeComponent();
    }

    private void OnDownloadableListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        foreach (var a in DownloadableList.SelectedItems)
        {
            _ = DownloadableList.SelectedItems.Add((DownloadableAddonEntity)a);
        }
    }
}
