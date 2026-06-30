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

public sealed partial class MapsControl : UserControl
{
    private readonly BitmapsCache _bitmapsCache = null!;
    private readonly IReadOnlyList<BasePort> _supportedPorts = [];
    private readonly MapsViewModel _viewModel = null!;

    private MenuFlyout? _flyout;

    public MapsControl()
    {
        InitializeComponent();
    }

    public MapsControl(
        MapsViewModel viewModel,
        PortsProvider portsProvider,
        BitmapsCache bitmapsCache
        )
    {
        InitializeComponent();

        _viewModel = viewModel;
        _supportedPorts = portsProvider.GetPortsThatSupportGame(viewModel.Game.GameEnum);
        _bitmapsCache = bitmapsCache;

        if (_viewModel.Game.AreSkillsAvailble)
        {
            CreateSkillsFlyout();
        }

        AddPortsButtons();
    }

    /// <summary>
    ///     Create skill flyout menu for a game
    /// </summary>
    private void CreateSkillsFlyout()
    {
        MenuFlyout flyout = new();
        flyout.Placement = PlacementMode.Top;

        var skills = GetSkillMenusItems();

        foreach (var skill in skills)
        {
            _ = flyout.Items.Add(skill);
        }

        _flyout = flyout;
    }

    /// <summary>
    ///     Add "Start with..." buttons to the ports button panel
    /// </summary>
    private void AddPortsButtons()
    {
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

            Button button = new()
            {
                Content = sp,
                CommandParameter = port,
                Command = new RelayCommand(() =>
                                           {
                                               if (!IsSkillFlyoutAvailable(port))
                                               {
                                                   _viewModel.StartMapCommand.Execute(new Tuple<BasePort, byte?>(port, null));
                                               }
                                           },
                                           () => PortsHelper.CheckPortRequirements(MapsList.SelectedItem, _viewModel.Game, port)),
                Margin = new(5),
                Padding = new(5)
            };

            button.Click += OnPortButtonClicked;

            BottomPanel.PortsButtonsPanel.Children.Add(button);
        }
    }


    /// <summary>
    ///     Get list of skill menu items
    /// </summary>
    private List<MenuItem> GetSkillMenusItems(BasePort? port = null)
    {
        ArgumentNullException.ThrowIfNull(_viewModel.Game.Skills);

        List<MenuItem> items = new(5);

        var enums = Enum.GetValues(_viewModel.Game.Skills.GetType())
                        .Cast<Enum>()
                        .ToDictionary(
                             Convert.ToByte,
                             e => e.GetDescription()
                             );

        foreach (var e in enums)
        {
            items.Add(
                new MenuItem()
                {
                    Header = e.Value,
                    Padding = new(5),
                    Command = new RelayCommand(() => _viewModel.StartMapCommand.Execute(new Tuple<BasePort, byte?>(GetPort(port), e.Key)
                                                   ))
                });
        }

        return items;
    }

    /// <summary>
    ///     Get port that should be run
    /// </summary>
    /// <param name="port">Port</param>
    private BasePort GetPort(BasePort? port)
    {
        if (_flyout?.Target is { } target)
        {
            return ((Button)target).CommandParameter as BasePort ?? throw new InvalidOperationException("CommandParameter is not BasePort");
        }
        else if (port is not null)
        {
            return port;
        }

        throw new ArgumentOutOfRangeException(nameof(port));
    }

    /// <summary>
    ///     Is skill flyout menu availably
    /// </summary>
    /// <param name="port">Port</param>
    private bool IsSkillFlyoutAvailable(BasePort port) =>
        _flyout is not null && port.IsSkillSelectionAvailable && _viewModel.Game.AreSkillsAvailble;


    /// <summary>
    ///     Update CanExecute for ports buttons and context menu buttons when selected campaign changed
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
    }

    private void OnPortButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            throw new InvalidCastException();
        }

        if (button.CommandParameter is not BasePort port)
        {
            throw new InvalidCastException();
        }

        if (IsSkillFlyoutAvailable(port))
        {
            if (_flyout is not null)
            {
                _flyout.ShowAt(button);
            }
        }
    }

    private void ContextMenuOpened(object? sender, RoutedEventArgs e)
    {
        if (MapsList.ContextMenu is not null)
        {
            MapsList.ContextMenu.Items.Clear();
        }

        if (MapsList.SelectedItem is not BaseAddon addon)
        {
            return;
        }

        if (MapsList.ContextMenu is not null)
        {
            MapsList.ContextMenu.Items.Clear();
        }

        if (addon.IsMetadataUpdateAvailable)
        {
            var updateMetadataButton = new MenuItem()
            {
                Header = "Update metadata",
                Padding = new(5),
                Command = new AsyncRelayCommand(async () => await _viewModel.UpdateMetadataAsync(addon).ConfigureAwait(true))
            };

            _ = MapsList.ContextMenu.Items.Add(updateMetadataButton);
            _ = MapsList.ContextMenu.Items.Add(new Separator());
        }

        foreach (var port in _supportedPorts)
        {
            if (!PortsHelper.CheckPortRequirements(MapsList.SelectedItem, _viewModel.Game, port))
            {
                continue;
            }

            MenuItem portButton;

            if (IsSkillFlyoutAvailable(port))
            {
                portButton = new MenuItem()
                {
                    Header = $"Start with {port.ShortName}",
                    Padding = new(5),
                    CommandParameter = port
                };

                var skills = GetSkillMenusItems(port);

                foreach (var skill in skills)
                {
                    _ = portButton.Items.Add(skill);
                }
            }
            else
            {
                portButton = new MenuItem()
                {
                    Header = $"Start with {port.ShortName}",
                    Padding = new(5),
                    Command = new RelayCommand(() => _viewModel.StartMapCommand.Execute(new Tuple<BasePort, byte?>(port, null)))
                };
            }

            _ = MapsList.ContextMenu.Items.Add(portButton);
        }

        if (MapsList.ContextMenu.Items.Count > 0)
        {
            _ = MapsList.ContextMenu.Items.Add(new Separator());
        }

        var deleteButton = new MenuItem()
        {
            Header = "Delete",
            Padding = new(5),
            Command = new RelayCommand(
                () => _viewModel.DeleteMapCommand.Execute(null),
                () => addon.Type is not AddonTypeEnum.Official
                )
        };

        _ = MapsList.ContextMenu.Items.Add(deleteButton);
    }

    private void ContextMenuClosed(object? sender, RoutedEventArgs e)
    {
        if (MapsList.ContextMenu is not null)
        {
            MapsList.ContextMenu.Items.Clear();
        }
    }
}
