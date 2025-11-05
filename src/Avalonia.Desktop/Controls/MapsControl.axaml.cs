using Addons.Providers;
using Avalonia.Controls;
using Avalonia.Desktop.Controls.Bases;
using Avalonia.Desktop.Helpers;
using Avalonia.Desktop.Misc;
using Avalonia.Desktop.ViewModels;
using Common.All.Enums;
using Common.All.Helpers;
using Common.All.Interfaces;
using Common.Client.Enums.Skills;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using Ports.Ports;
using Ports.Providers;

namespace Avalonia.Desktop.Controls;

public sealed partial class MapsControl : DroppableControl
{
    private readonly IEnumerable<BasePort> _supportedPorts = [];
    private readonly MapsViewModel _viewModel = null!;
    private readonly BitmapsCache _bitmapsCache = null!;

    private MenuFlyout? _flyout;

    public MapsControl() : base(null!)
    {
        InitializeComponent();
    }

    public MapsControl(
        MapsViewModel viewModel,
        InstalledPortsProvider portsProvider,
        InstalledAddonsProvider installedAddonsProvider,
        BitmapsCache bitmapsCache
        ) : base(installedAddonsProvider)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _supportedPorts = portsProvider.GetPortsThatSupportGame(viewModel.Game.GameEnum);
        _bitmapsCache = bitmapsCache;

        CreateSkillsFlyout();
        AddPortsButtons();
        AddContextMenuButtons();
    }

    /// <summary>
    /// Create skill flyout menu for a game
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
    /// Add "Start with..." buttons to the ports button panel
    /// </summary>
    private void AddPortsButtons()
    {
        foreach (var port in _supportedPorts)
        {
            var portIcon = _bitmapsCache.GetFromCache(port.PortEnum.GetUniqueHash());

            StackPanel sp = new() { Orientation = Layout.Orientation.Horizontal };
            sp.Children.Add(new Image() { Margin = new(0, 0, 5, 0), Height = 16, Source = portIcon });
            sp.Children.Add(new TextBlock() { Text = port.ShortName });

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
                () => PortsHelper.CheckPortRequirements(MapsList.SelectedItem, _viewModel.Game.GameEnum, port)),
                Margin = new(5),
                Padding = new(5),
            };

            button.Click += OnPortButtonClicked;

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
            if (!PortsHelper.CheckPortRequirements(MapsList.SelectedItem, _viewModel.Game.GameEnum, port))
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


    /// <summary>
    /// Get list of skill menu items
    /// </summary>
    private List<MenuItem> GetSkillMenusItems(BasePort? port = null)
    {
        List<MenuItem> items = [];
        Dictionary<byte, string> enums;

        if (_viewModel.Game.GameEnum is GameEnum.Duke3D)
        {
            enums = Enum.GetValues<Duke3DSkillsEnum>().ToDictionary(static x => (byte)x, static y => y.ToReadableString());
        }
        else if (_viewModel.Game.GameEnum is GameEnum.Wang)
        {
            enums = Enum.GetValues<WangSkillsEnum>().ToDictionary(static x => (byte)x, static y => y.ToReadableString());
        }
        else if (_viewModel.Game.GameEnum is GameEnum.Redneck)
        {
            enums = Enum.GetValues<RedneckSkillsEnum>().ToDictionary(static x => (byte)x, static y => y.ToReadableString());
        }
        else if (_viewModel.Game.GameEnum is GameEnum.Fury)
        {
            enums = Enum.GetValues<FurySkillsEnum>().ToDictionary(static x => (byte)x, static y => y.ToReadableString());
        }
        else if (_viewModel.Game.GameEnum is GameEnum.Blood)
        {
            enums = Enum.GetValues<BloodSkillsEnum>().ToDictionary(static x => (byte)x, static y => y.ToReadableString());
        }
        else
        {
            return items;
        }

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
    /// Get port that should be run
    /// </summary>
    /// <param name="port">Port</param>
    private BasePort GetPort(BasePort? port)
    {
        if (_flyout?.Target is not null)
        {
            return ((Button)_flyout.Target!).CommandParameter as BasePort ?? ThrowHelper.ThrowFormatException<BasePort>();
        }
        else if (port is not null)
        {
            return port;
        }

        return ThrowHelper.ThrowArgumentOutOfRangeException<BasePort>(nameof(port));
    }

    /// <summary>
    /// Is skill flyout menu availably
    /// </summary>
    /// <param name="port">Port</param>
    private bool IsSkillFlyoutAvailable(BasePort port)
    {
        if (_flyout is null)
        {
            return false;
        }

        return port.PortEnum
            is PortEnum.EDuke32
            or PortEnum.VoidSW
            or PortEnum.RedNukem
            or PortEnum.Fury
            or PortEnum.NBlood
            or PortEnum.NotBlood;
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
    private void OnListBoxEmptySpaceClicked(object? sender, Input.PointerPressedEventArgs e)
    {
        MapsList.SelectedItem = null;
        MapsList.Focusable = true;
        _ = MapsList.Focus();
        MapsList.Focusable = false;
    }

    private void OnPortButtonClicked(object? sender, Interactivity.RoutedEventArgs e)
    {
        sender.ThrowIfNotType(out Button button);
        button.CommandParameter.ThrowIfNotType<BasePort>(out var port);

        if (IsSkillFlyoutAvailable(port))
        {
            _flyout!.ShowAt(button);
        }
    }
}
