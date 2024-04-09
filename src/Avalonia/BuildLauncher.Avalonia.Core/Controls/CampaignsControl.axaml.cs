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
    public sealed partial class CampaignsControl : UserControl
    {
        private IEnumerable<BasePort> _supportedPorts;
        private CampaignsViewModel _viewModel;

        public CampaignsControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize control
        /// </summary>
        public void InitializeControl(PortsProvider portsProvider)
        {
            DataContext.ThrowIfNotType<CampaignsViewModel>(out var viewModel);

            _viewModel = viewModel;
            _supportedPorts = portsProvider.GetPortsThatSupportGame(_viewModel.Game.GameEnum);

            CampaignsList.SelectionChanged += OnCampaignsListSelectionChanged;
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
                        _viewModel.StartCampaignCommand.Execute(port),
                        () => port.IsInstalled && CampaignsList.SelectedItem is not null &&
                        (((IAddon)CampaignsList.SelectedItem).SupportedPorts is null || ((IAddon)CampaignsList.SelectedItem).SupportedPorts!.Contains(port.PortEnum))
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
            CampaignsList.ContextMenu = new();

            if (CampaignsList.SelectedItem is not IAddon iMod)
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
                        Command = new RelayCommand(() => _viewModel.StartCampaignCommand.Execute(port))
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
                Command = new RelayCommand(
                    () => _viewModel.DeleteCampaignCommand.Execute(null),
                    () => iMod.Type is not ModTypeEnum.Official
                    )
            };

            CampaignsList.ContextMenu.Items.Add(deleteButton);
        }

        /// <summary>
        /// Update CanExecute for ports buttons and context menu buttons when selected campaign changed
        /// </summary>
        private void OnCampaignsListSelectionChanged(object? sender, SelectionChangedEventArgs e)
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
            CampaignsList.SelectedItem = null;
        }
    }
}
