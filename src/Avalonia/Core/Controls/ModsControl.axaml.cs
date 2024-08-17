using Avalonia.Controls;
using BuildLauncher.ViewModels;
using Common.Client.Config;
using Common.Helpers;
using CommunityToolkit.Mvvm.Input;

namespace BuildLauncher.Controls
{
    public sealed partial class ModsControl : UserControl
    {
        public ModsControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize control
        /// </summary>
        public void InitializeControl(IConfigProvider configProvider)
        {
            DataContext.ThrowIfNotType<ModsViewModel>(out var viewModel);

            RightPanel.InitializeControl(configProvider);

            AddContextMenuButtons(viewModel);
        }

        /// <summary>
        /// Add button to the right click menu
        /// </summary>
        private void AddContextMenuButtons(ModsViewModel viewModel)
        {
            ModsList.ContextMenu = new();

            var deleteButton = new MenuItem()
            {
                Header = "Delete",
                Command = new RelayCommand(() => viewModel.DeleteModCommand.Execute(null))
            };

            ModsList.ContextMenu.Items.Add(deleteButton);
        }
    }
}
