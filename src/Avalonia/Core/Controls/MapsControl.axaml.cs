using Avalonia.Controls;
using Avalonia.Media;
using BuildLauncher.Helpers;
using BuildLauncher.ViewModels;
using Common.Enums;
using Common.Enums.Skills;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.Input;
using Ports.Ports;
using Ports.Providers;
using System.Globalization;

namespace BuildLauncher.Controls;

public sealed partial class MapsControl : UserControl
{
    private IEnumerable<BasePort> _supportedPorts;
    private MapsViewModel _viewModel;
    private MenuFlyout? _flyout = null;

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
            flyout.Items.Add(skill);
        }

        _flyout = flyout;
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
                CommandParameter = port,
                Command = new RelayCommand(() =>
                {
                    if (!IsSkillFlyoutAvailable(port))
                    {
                        _viewModel.StartMapCommand.Execute(new Tuple<BasePort, byte?>(port, null));
                    }
                },
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

                    if (selectedMap.RequiredFeatures is not null &&
                        selectedMap.RequiredFeatures!.Except(port.SupportedFeatures).Any())
                    {
                        return false;
                    }

                    if (!port.SupportedGames.Contains(selectedMap.SupportedGame.GameEnum))
                    {
                        return false;
                    }

                    if (selectedMap.SupportedGame.GameVersion is not null &&
                        !port.SupportedGamesVersions.Contains(selectedMap.SupportedGame.GameVersion))
                    {
                        return false;
                    }

                    return true;
                }),
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
            if (!port.IsInstalled ||
                addon.RequiredFeatures is not null && addon.RequiredFeatures!.Except(port.SupportedFeatures).Any() ||
                port.PortEnum is PortEnum.BuildGDX)
            {
                continue;
            }

            MenuItem portButton;

            if (IsSkillFlyoutAvailable(port))
            {
                portButton = new MenuItem()
                {
                    Header = $"Start with {port.Name}",
                    CommandParameter = port
                };

                var skills = GetSkillMenusItems(port);

                foreach (var skill in skills)
                {
                    portButton.Items.Add(skill);
                }
            }
            else
            {
                portButton = new MenuItem()
                {
                    Header = $"Start with {port.Name}",
                    Command = new RelayCommand(() => _viewModel.StartMapCommand.Execute(new Tuple<BasePort, byte?>(port, null)))
                };
            }

            MapsList.ContextMenu.Items.Add(portButton);
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
    /// Get list of skill menu items
    /// </summary>
    private List<MenuItem> GetSkillMenusItems(BasePort? port = null)
    {
        List<MenuItem> items = new();
        Dictionary<byte, string> enums;

        if (_viewModel.Game.GameEnum is GameEnum.Duke3D)
        {
            enums = Enum.GetValues<Duke3DSkillsEnum>().ToDictionary(static x => (byte)x, static y => y.ToReadableString());
        }
        else if (_viewModel.Game.GameEnum is GameEnum.ShadowWarrior)
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
            return ((Button)_flyout.Target!).CommandParameter as BasePort;
        }
        else if (port is not null)
        {
            return port;
        }

        return ThrowHelper.ArgumentOutOfRangeException<BasePort>(nameof(port));
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

        if (port.PortEnum is 
            PortEnum.EDuke32 
            or PortEnum.VoidSW 
            or PortEnum.RedNukem 
            or PortEnum.Fury
            or PortEnum.NBlood
            or PortEnum.NotBlood
            )
        {
            return true;
        }

        return false;
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
    private void OnListBoxEmptySpaceClicked(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        MapsList.SelectedItem = null;
        MapsList.Focusable = true;
        MapsList.Focus();
        MapsList.Focusable = false;
    }

    private void OnPortButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        sender.ThrowIfNotType<Button>(out var button);
        button.CommandParameter.ThrowIfNotType<BasePort>(out var port);

        if (IsSkillFlyoutAvailable(port))
        {
            _flyout!.ShowAt(button);
        }
    }
}
