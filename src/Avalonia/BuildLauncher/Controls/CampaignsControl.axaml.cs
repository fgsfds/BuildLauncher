using Avalonia.Controls;
using BuildLauncher.ViewModels;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.Input;
using Ports.Ports;
using Ports.Providers;

namespace BuildLauncher.Controls
{
    public partial class CampaignsControl : UserControl
    {
        private IEnumerable<BasePort> _supportedPorts;
        private GameViewModel _gameViewModel;

        public CampaignsControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize control
        /// </summary>
        public void Init(PortsProvider portsProvider)
        {
            DataContext.ThrowIfNotType<GameViewModel>(out var gameViewModel);

            _gameViewModel = gameViewModel;

            CampaignsListControl.SelectionChanged += CampaignsListSelectionChanged;

            _supportedPorts = portsProvider.GetPortsThatSupportGame(_gameViewModel.Game.GameEnum);

            AddPortsButtons();

            AddContextMenuButtons();
        }

        /// <summary>
        /// Add "Start with..." buttons to the ports button panel
        /// </summary>
        private void AddPortsButtons()
        {
            foreach (var port in _supportedPorts)
            {
                Button button = new()
                {
                    Content = port.Name,
                    Command = new RelayCommand(() =>
                        _gameViewModel.StartGameCommand.Execute(port),
                        () => port.IsInstalled && CampaignsListControl.SelectedItem is not null &&
                        (((IMod)CampaignsListControl.SelectedItem)?.SupportedPorts is null || ((IMod)CampaignsListControl.SelectedItem).SupportedPorts!.Contains(port.PortEnum))
                        ),
                    Margin = new(5),
                    Padding = new(5),
                };

                PortsButtonsPanel.Children.Add(button);
            }
        }

        /// <summary>
        /// Add button to the right click menu
        /// </summary>
        private void AddContextMenuButtons()
        {
            CampaignsListControl.ContextMenu.ThrowIfNull();

            if (CampaignsListControl.SelectedItem is not IMod iMod)
            {
                return;
            }

            CampaignsListControl.ContextMenu.Items.Clear();

            var deleteButton = new MenuItem()
            {
                Header = "Delete",
                Command = new RelayCommand(() =>
                _gameViewModel.DeleteCampaignCommand.Execute(null),
                () => !iMod.IsOfficial
                )
            };

            CampaignsListControl.ContextMenu.Items.Add(deleteButton);

            foreach (var port in _supportedPorts)
            {
                if (port.IsInstalled &&
                    (iMod.SupportedPorts is null || iMod.SupportedPorts!.Contains(port.PortEnum)))
                {
                    var portButton = new MenuItem()
                    {
                        Header = $"Start with {port.Name}",
                        Command = new RelayCommand(() =>
                        _gameViewModel.StartGameCommand.Execute(port)
                        )
                    };

                    CampaignsListControl.ContextMenu.Items.Add(portButton);
                }

            }
        }

        /// <summary>
        /// Update CanExecute for ports buttons and context menu buttons when selected campaign changed
        /// </summary>
        private void CampaignsListSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            foreach (var control in PortsButtonsPanel.Children)
            {
                if (control is Button button &&
                    button.Command is IRelayCommand relayCommand)
                {
                    relayCommand.NotifyCanExecuteChanged();
                }
            }

            AddContextMenuButtons();
        }

        /// <summary>
        /// Reset selected item when empty space is clicked
        /// </summary>
        private void ListBoxEmptySpaceClicked(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            CampaignsListControl.SelectedItem = null;
        }
    }
}
