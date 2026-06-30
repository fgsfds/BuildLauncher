using Addons.Addons;
using Avalonia.Controls;
using Avalonia.Desktop.Helpers;
using Avalonia.Desktop.Misc;
using Avalonia.Desktop.ViewModels;
using Avalonia.Interactivity;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;
using Core.All.Enums;
using Core.All.Helpers;
using Ports.Ports;
using Ports.Providers;

namespace Avalonia.Desktop.Controls;

public sealed partial class CampaignsControl : UserControl
{
    private const string BuiltInPortStr = "Built-in port";
    private const string CustomPortStr = "Custom port";
    private readonly BitmapsCache _bitmapsCache;
    private readonly PortsProvider _portsProvider;

    private readonly IReadOnlyList<BasePort> _supportedPorts;
    private readonly CampaignsViewModel _viewModel;

    public CampaignsControl()
    {
        InitializeComponent();

        _supportedPorts = [];
        _portsProvider = null!;
        _viewModel = null!;
        _bitmapsCache = null!;
    }

    public CampaignsControl(
        CampaignsViewModel viewModel,
        PortsProvider portsProvider,
        BitmapsCache bitmapsCache
        )
    {
        InitializeComponent();

        _supportedPorts = portsProvider.GetPortsThatSupportGame(viewModel.Game.GameEnum);

        if (viewModel.Game.GameEnum is GameEnum.Duke3D)
        {
            var zhPorts = portsProvider.GetPortsThatSupportGame(GameEnum.DukeZeroHour);
            _supportedPorts = [.. _supportedPorts, .. zhPorts];
        }

        _portsProvider = portsProvider;
        _viewModel = viewModel;
        _bitmapsCache = bitmapsCache;

        AddPortsButtons();

        _portsProvider.CustomPortChangedEvent += OnCustomPortChanged;
    }

    /// <summary>
    ///     Add "Start with..." buttons to the ports button panel
    /// </summary>
    private void AddPortsButtons()
    {
        if (_viewModel.Game.GameEnum is GameEnum.Standalone)
        {
            TextBlock textBlock = new()
            {
                Text = BuiltInPortStr
            };

            Button button = new()
            {
                Content = textBlock,
                Command = new RelayCommand(() =>
                                               _viewModel.StartCampaignCommand.Execute(new StubPort()),
                                           () => CampaignsList?.SelectedItem is BaseAddon selectedCampaign),
                Margin = new(5),
                Padding = new(5)
            };

            BottomPanel.PortsButtonsPanel.Children.Add(button);

            return;
        }

        foreach (var port in _supportedPorts)
        {
            var portIcon = _bitmapsCache.GetFromCache(port.PortEnum.GetUniqueHash());

            StackPanel sp = new()
            {
                Orientation = Orientation.Horizontal
            };

            sp.Children.Add(new Image()
            {
                Margin = new(0, 0, 5, 0),
                Height = 16,
                Source = portIcon
            });

            sp.Children.Add(new TextBlock()
            {
                Text = port.ShortName
            });

            Button portButton = new()
            {
                Content = sp,
                Command = new RelayCommand(() =>
                                               _viewModel.StartCampaignCommand.Execute(port),
                                           () => PortsHelper.CheckPortRequirements(CampaignsList.SelectedItem, _viewModel.Game, port)),
                Margin = new(5),
                Padding = new(5)
            };

            BottomPanel.PortsButtonsPanel.Children.Add(portButton);
        }

        Button customPortButton = new()
        {
            Content = new TextBlock()
            {
                Text = BuiltInPortStr
            },
            Command = new RelayCommand(() =>
                                           _viewModel.StartCampaignCommand.Execute(null),
                                       () =>
                                       {
                                           if (CampaignsList?.SelectedItem is not BaseAddon selectedCampaign)
                                           {
                                               return false;
                                           }

                                           if (selectedCampaign.Executables is null)
                                           {
                                               return false;
                                           }

                                           return true;
                                       }),
            Margin = new(5),
            Padding = new(5),
            IsVisible = false
        };

        BottomPanel.PortsButtonsPanel.Children.Add(customPortButton);

        AddCustomPortsButton();
    }

    /// <summary>
    ///     Add button with custom ports
    /// </summary>
    private void AddCustomPortsButton()
    {
        var existing = BottomPanel.PortsButtonsPanel.Children.FirstOrDefault(
            x => x is Button button && button.Content is TextBlock text && text.Text?.Equals(CustomPortStr) is true
            );

        if (existing is not null)
        {
            _ = BottomPanel.PortsButtonsPanel.Children.Remove(existing);
        }

        MenuFlyout flyout = new()
        {
            Placement = PlacementMode.Top
        };

        var customPorts = _portsProvider.GetCustomPorts(_viewModel.Game.GameEnum);

        if (customPorts.Count < 1)
        {
            return;
        }

        foreach (var port in customPorts)
        {
            MenuItem item = new()
            {
                Header = port.Name,
                Padding = new(5),
                Command = new RelayCommand(() => _viewModel.StartCampaignCommand.Execute(port))
            };

            _ = flyout.Items.Add(item);
        }

        Button customPortButton = new()
        {
            Content = new TextBlock()
            {
                Text = CustomPortStr
            },
            Margin = new(5),
            Padding = new(5),
            IsEnabled = false,
            IsVisible = true
        };

        customPortButton.Click += (sender, e) => flyout.ShowAt(customPortButton);

        BottomPanel.PortsButtonsPanel.Children.Add(customPortButton);
    }

    /// <summary>
    ///     Invoked on selected campaign changed
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

        var customPortButton = BottomPanel.PortsButtonsPanel.Children.FirstOrDefault(
            x => x is Button button && button.Content is TextBlock text && text.Text?.Equals(CustomPortStr) is true
            ) as Button;

        if (customPortButton is not null)
        {
            customPortButton.IsEnabled = CampaignsList.SelectedItem is not null;
        }
    }

    private void OnCustomPortChanged(object? sender, EventArgs e)
    {
        AddCustomPortsButton();
    }

    private void ContextMenuOpened(object? sender, RoutedEventArgs e)
    {
        if (CampaignsList.ContextMenu is not null)
        {
            CampaignsList.ContextMenu.Items.Clear();
        }

        if (CampaignsList.SelectedItem is not BaseAddon addon)
        {
            return;
        }

        if (CampaignsList.ContextMenu is not null)
        {
            CampaignsList.ContextMenu.Items.Clear();
        }

        MenuItem favoriteButton;

        if (addon.IsFavorite)
        {
            favoriteButton = new MenuItem()
            {
                Header = "Remove from favorites",
                Padding = new(5),
                Command = new RelayCommand(() => _viewModel.RemoveFromFavoriteCommand.Execute(CampaignsList.SelectedItem))
            };
        }
        else
        {
            favoriteButton = new MenuItem()
            {
                Header = "Add to favorites",
                Padding = new(5),
                Command = new RelayCommand(() => _viewModel.AddToFavoriteCommand.Execute(CampaignsList.SelectedItem))
            };
        }

        _ = CampaignsList.ContextMenu.Items.Add(favoriteButton);
        _ = CampaignsList.ContextMenu.Items.Add(new Separator());

        if (addon.IsMetadataUpdateAvailable)
        {
            var updateMetadataButton = new MenuItem()
            {
                Header = "Update metadata",
                Padding = new(5),
                Command = new AsyncRelayCommand(async () => await _viewModel.UpdateMetadataAsync(addon).ConfigureAwait(true))
            };

            _ = CampaignsList.ContextMenu.Items.Add(updateMetadataButton);
            _ = CampaignsList.ContextMenu.Items.Add(new Separator());
        }

        foreach (var port in _supportedPorts)
        {
            if (!PortsHelper.CheckPortRequirements(CampaignsList.SelectedItem, _viewModel.Game, port))
            {
                continue;
            }

            var portButton = new MenuItem()
            {
                Header = $"Start with {port.ShortName}",
                Padding = new(5),
                Command = new RelayCommand(() => _viewModel.StartCampaignCommand.Execute(port))
            };

            _ = CampaignsList.ContextMenu.Items.Add(portButton);
        }

        if (CampaignsList.ContextMenu.Items.Count > 2)
        {
            _ = CampaignsList.ContextMenu.Items.Add(new Separator());
        }


        byte cPortsCount = 0;
        var customPorts = _portsProvider.GetCustomPorts(_viewModel.Game.GameEnum);

        foreach (var port in customPorts)
        {
            if ((addon.RequiredFeatures?.Except(port.BasePort.SupportedFeatures).Any() is true) ||
                (addon.Type is not AddonTypeEnum.Official && port.BasePort.PortEnum is PortEnum.BuildGDX))
            {
                continue;
            }

            var portButton = new MenuItem()
            {
                Header = $"Start with {port.Name}",
                Padding = new(5),
                Command = new RelayCommand(() => _viewModel.StartCampaignCommand.Execute(port))
            };

            _ = CampaignsList.ContextMenu.Items.Add(portButton);
            cPortsCount++;
        }

        if (cPortsCount > 0)
        {
            _ = CampaignsList.ContextMenu.Items.Add(new Separator());
        }


        var deleteButton = new MenuItem()
        {
            Header = "Delete",
            Padding = new(5),
            Command = new RelayCommand(
                () => _viewModel.DeleteCampaignCommand.Execute(null),
                () => addon.Type is not AddonTypeEnum.Official
                )
        };

        _ = CampaignsList.ContextMenu.Items.Add(deleteButton);
    }

    private void ContextMenuClosed(object? sender, RoutedEventArgs e)
    {
        if (CampaignsList.ContextMenu is not null)
        {
            CampaignsList.ContextMenu.Items.Clear();
        }
    }
}
