using Avalonia.Controls;
using Avalonia.Media;
using BuildLauncher.Helpers;
using BuildLauncher.ViewModels;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.Input;
using Ports.Ports;
using Ports.Providers;
using System.Globalization;

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
            ImagePathToBitmapConverter converter = new();

            foreach (var port in _supportedPorts)
            {
                var portIcon = converter.Convert(port.Icon, typeof(IImage), null!, CultureInfo.InvariantCulture) as IImage;

                StackPanel sp = new() { Orientation = Avalonia.Layout.Orientation.Horizontal };
                sp.Children.Add(new Image() { Margin = new(0, 0, 5, 0), Height = 16, Source = portIcon });
                sp.Children.Add(new TextBlock() { Text = port.Name });

                Button button = new()
                {
                    Content = sp,
                    Command = new RelayCommand(() =>
                        _viewModel.StartMapCommand.Execute(port),
                        () =>
                        {
                            if (!port.IsInstalled)
                            {
                                return false;
                            }

                            if (MapsList.SelectedItem is null)
                            {
                                return false;
                            }

                            var selectedMap = (IAddon)MapsList.SelectedItem;

                            if (port.PortEnum is PortEnum.BuildGDX)
                            {
                                return false;
                            }

                            if (selectedMap.RequiredFeatures is null || !selectedMap.RequiredFeatures!.Except(port.SupportedFeatures).Any())
                            {
                                return true;
                            }

                            return false;
                        }),
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
                    (addon.RequiredFeatures is null || !addon.RequiredFeatures!.Except(port.SupportedFeatures).Any()))
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
            MapsList.Focusable = true;
            MapsList.Focus();
            MapsList.Focusable = false;
        }
    }
}
