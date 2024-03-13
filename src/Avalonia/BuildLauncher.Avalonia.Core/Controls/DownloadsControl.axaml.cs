using Avalonia.Controls;
using BuildLauncher.ViewModels;
using Common.Helpers;
using CommunityToolkit.Mvvm.Input;

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
            DataContext.ThrowIfNotType<DownloadsViewModel>(out _);

            AddContextMenuButtons(DownloadableCampaignsList);
            AddContextMenuButtons(DownloadableMapsList);
            AddContextMenuButtons(DownloadableModsList);
        }

        /// <summary>
        /// Add button to the right click menu
        /// </summary>
        private void AddContextMenuButtons(ListBox listBox)
        {
            DataContext.ThrowIfNotType<DownloadsViewModel>(out var viewModel);

            listBox.ContextMenu = new();

            var downloadButton = new MenuItem()
            {
                Header = "Download",
                Command = new RelayCommand(() =>
                viewModel.DownloadModCommand.Execute(null)
                )
            };

            listBox.ContextMenu.Items.Add(downloadButton);
        }
    }
}
