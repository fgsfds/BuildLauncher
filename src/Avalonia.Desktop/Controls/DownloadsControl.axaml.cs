using Avalonia.Controls;
using Avalonia.Desktop.ViewModels;
using Common.Helpers;
using CommunityToolkit.Mvvm.Input;

namespace Avalonia.Desktop.Controls;

public sealed partial class DownloadsControl : UserControl
{
    public DownloadsControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initialize control
    /// </summary>
    public void InitializeControl()
    {
        AddContextMenuButtons(DownloadableList);
    }

    /// <summary>
    /// Add button to the right click menu
    /// </summary>
    private void AddContextMenuButtons(DataGrid dataGrid)
    {
        DataContext.ThrowIfNotType<DownloadsViewModel>(out var viewModel);

        dataGrid.ContextMenu = new();

        var downloadButton = new MenuItem()
        {
            Header = "Download",
            Padding = new(5),
            Command = new RelayCommand(() =>
            viewModel.DownloadAddonCommand.Execute(null)
            )
        };

        _ = dataGrid.ContextMenu.Items.Add(downloadButton);
    }
}
