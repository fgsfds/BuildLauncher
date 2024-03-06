using Avalonia.Controls;
using BuildLauncher.ViewModels;
using Common.Helpers;
using CommunityToolkit.Mvvm.Input;
using Mods.Serializable;

namespace BuildLauncher.Controls
{
    public partial class DownloadsControl : UserControl
    {
        public DownloadsControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize control
        /// </summary>
        public void Init()
        {
            AddContextMenuButtons(DownloadableCampaignsList);
            AddContextMenuButtons(DownloadableModsList);
        }

        /// <summary>
        /// Add button to the right click menu
        /// </summary>
        private void AddContextMenuButtons(ListBox listBox)
        {
            DataContext.ThrowIfNotType<GameViewModel>(out var gameViewModel);
            listBox.ContextMenu.ThrowIfNull();

            listBox.ContextMenu.Items.Clear();

            var downloadButton = new MenuItem()
            {
                Header = "Download",
                Command = new RelayCommand(() =>
                gameViewModel.DownloadModCommand.Execute(null)
                )
            };

            listBox.ContextMenu.Items.Add(downloadButton);
        }
    }
}
