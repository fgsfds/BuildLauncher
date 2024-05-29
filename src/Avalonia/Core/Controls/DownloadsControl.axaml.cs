using Avalonia.Controls;
using BuildLauncher.ViewModels;
using Common.Helpers;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;

namespace BuildLauncher.Controls
{
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
                Command = new RelayCommand(() =>
                viewModel.DownloadAddonCommand.Execute(null)
                )
            };

            dataGrid.ContextMenu.Items.Add(downloadButton);
        }
    }
}
