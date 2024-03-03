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
            if (DataContext is not GameViewModel gameViewModel)
            {
                ThrowHelper.ArgumentException(nameof(DataContext));
                return;
            }

            AddContextMenuButtons(gameViewModel);
        }

        /// <summary>
        /// Add button to the right click menu
        /// </summary>
        private void AddContextMenuButtons(GameViewModel gameViewModel)
        {
            if (ModsListControl.ContextMenu is null)
            {
                ThrowHelper.NullReferenceException(nameof(ModsListControl.ContextMenu));
            }

            var deleteButton = new MenuItem()
            {
                Header = "Delete",
                Command = new RelayCommand(() => gameViewModel.DeleteModCommand.Execute(null))
            };

            ModsListControl.ContextMenu.Items.Add(deleteButton);
        }
    }
}
