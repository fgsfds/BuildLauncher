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

            CampaignsList.SelectionChanged += CampaignsListSelectionChanged;

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
                        () => port.IsInstalled && CampaignsList.SelectedItem is not null &&
                        (((IMod)CampaignsList.SelectedItem)?.SupportedPorts is null || ((IMod)CampaignsList.SelectedItem).SupportedPorts!.Contains(port.PortEnum))
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
            CampaignsList.ContextMenu.ThrowIfNull();

            if (CampaignsList.SelectedItem is not IMod iMod)
            {
                return;
            }

            CampaignsList.ContextMenu.Items.Clear();


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

                    CampaignsList.ContextMenu.Items.Add(portButton);
                }

            }

            if (CampaignsList.ContextMenu.Items.Count > 0)
            {
                CampaignsList.ContextMenu.Items.Add(new Separator());
            }

            var deleteButton = new MenuItem()
            {
                Header = "Delete",
                Command = new RelayCommand(() =>
                _gameViewModel.DeleteCampaignCommand.Execute(null),
                () => !iMod.IsOfficial
                )
            };

            CampaignsList.ContextMenu.Items.Add(deleteButton);
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
            CampaignsList.SelectedItem = null;
        }
    }
}
