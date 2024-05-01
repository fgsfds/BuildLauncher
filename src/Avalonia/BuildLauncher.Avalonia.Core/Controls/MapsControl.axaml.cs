using Avalonia.Controls;
using BuildLauncher.ViewModels;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.Input;
using Ports.Ports;
using Ports.Providers;

namespace BuildLauncher.Controls
{
    public sealed partial class MapsControl : UserControl
    {
        private IEnumerable<BasePort> _supportedPorts;
        private MapsViewModel _viewModel;

        public MapsControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize control
        /// </summary>
        public void InitializeControl(PortsProvider portsProvider)
        {
            DataContext.ThrowIfNotType<MapsViewModel>(out var viewModel);

            _viewModel = viewModel;
            _supportedPorts = portsProvider.GetPortsThatSupportGame(_viewModel.Game.GameEnum);

            MapsList.SelectionChanged += OnMapsListSelectionChanged;
            BottomPanel.DataContext = viewModel;

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
                        _viewModel.StartMapCommand.Execute(port),
                        () => port.IsInstalled && MapsList.SelectedItem is not null &&
                        (((IAddon)MapsList.SelectedItem)?.SupportedPorts is null || ((IAddon)MapsList.SelectedItem).SupportedPorts!.Contains(port.PortEnum))
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
            MapsList.ContextMenu = new();

            if (MapsList.SelectedItem is not IAddon addon)
            {
                return;
            }

            MapsList.ContextMenu.Items.Clear();

            foreach (var port in _supportedPorts)
            {
                if (port.IsInstalled &&
                    (addon.SupportedPorts is null || addon.SupportedPorts!.Contains(port.PortEnum)))
                {
                    var portButton = new MenuItem()
                    {
                        Header = $"Start with {port.Name}",
                        Command = new RelayCommand(() => _viewModel.StartMapCommand.Execute(port))
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
                Command = new RelayCommand(
                    () => _viewModel.DeleteMapCommand.Execute(null),
                    () => addon.Type is not AddonTypeEnum.Official
                    )
            };

            MapsList.ContextMenu.Items.Add(deleteButton);
        }

        /// <summary>
        /// Update CanExecute for ports buttons and context menu buttons when selected campaign changed
        /// </summary>
        private void OnMapsListSelectionChanged(object? sender, SelectionChangedEventArgs e)
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
