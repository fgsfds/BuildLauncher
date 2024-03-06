using Avalonia.Controls;
using BuildLauncher.ViewModels;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.Input;
using Ports.Ports;
using Ports.Providers;

namespace BuildLauncher.Controls
{
    public partial class MapsControl : UserControl
    {
        private IEnumerable<BasePort> _supportedPorts;
        private GameViewModel _gameViewModel;

        public MapsControl()
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

            MapsList.SelectionChanged += MapsListSelectionChanged;

            _supportedPorts = portsProvider.GetPortsThatSupportGame(_gameViewModel.Game.GameEnum);

            MapsList.ContextMenu = new();

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
                                _gameViewModel.StartMapCommand.Execute(port),
        () => port.IsInstalled && MapsList.SelectedItem is not null &&
                        (((IMod)MapsList.SelectedItem)?.SupportedPorts is null || ((IMod)MapsList.SelectedItem).SupportedPorts!.Contains(port.PortEnum))
                        ),
                    Margin = new(5),
                    Padding = new(5),
                };

                BottomPanel.PortsButtonsPanel.Children.Add(button);
            }
        }

        /// <summary>
        /// Add button to the right click menu
        /// </summary>
        private void AddContextMenuButtons()
        {
            MapsList.ContextMenu.ThrowIfNull();

            if (MapsList.SelectedItem is not IMod iMod)
            {
                return;
            }

            MapsList.ContextMenu.Items.Clear();


            foreach (var port in _supportedPorts)
            {
                if (port.IsInstalled &&
                (iMod.SupportedPorts is null || iMod.SupportedPorts!.Contains(port.PortEnum)))
                {
                    var portButton = new MenuItem()
                    {
                        Header = $"Start with {port.Name}",
                        Command = new RelayCommand(() =>
                                            _gameViewModel.StartMapCommand.Execute(port)
                        )
                    };

                    MapsList.ContextMenu.Items.Add(portButton);
                }

            }

            if (MapsList.ContextMenu.Items.Count > 0)
            {
                MapsList.ContextMenu.Items.Add(new Separator());
            }

            var deleteButton = new MenuItem()
            {
                Header = "Delete",
                Command = new RelayCommand(() =>
                                            _gameViewModel.DeleteMapCommand.Execute(null),
() => !iMod.IsOfficial
                    )
            };

            MapsList.ContextMenu.Items.Add(deleteButton);
        }

        /// <summary>
        /// Update CanExecute for ports buttons and context menu buttons when selected campaign changed
        /// </summary>
        private void MapsListSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            foreach (var control in BottomPanel.PortsButtonsPanel.Children)
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
            MapsList.SelectedItem = null;
        }
    }
}
