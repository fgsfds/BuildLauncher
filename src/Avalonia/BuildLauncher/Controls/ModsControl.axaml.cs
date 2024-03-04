using Avalonia.Controls;
using BuildLauncher.ViewModels;
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
        public void Init()
        {
            DataContext.ThrowIfNotType<GameViewModel>(out var gameViewModel);

            AddContextMenuButtons(gameViewModel);
        }

        /// <summary>
        /// Add button to the right click menu
        /// </summary>
        private void AddContextMenuButtons(GameViewModel gameViewModel)
        {
            ModsListControl.ContextMenu.ThrowIfNull();

            var deleteButton = new MenuItem()
            {
                Header = "Delete",
                Command = new RelayCommand(() => gameViewModel.DeleteModCommand.Execute(null))
            };

            ModsListControl.ContextMenu.Items.Add(deleteButton);
        }
    }
}
