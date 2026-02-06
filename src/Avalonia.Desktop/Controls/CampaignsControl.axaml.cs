using Addons.Addons;
using Addons.Providers;
using Avalonia.Controls;
using Avalonia.Desktop.Controls.Bases;
using Avalonia.Desktop.Helpers;
using Avalonia.Desktop.Misc;
using Avalonia.Desktop.ViewModels;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Common.All.Enums;
using Common.All.Helpers;
using CommunityToolkit.Mvvm.Input;
using Ports.Ports;
using Ports.Providers;

namespace Avalonia.Desktop.Controls;

public sealed partial class CampaignsControl : DroppableControl
{
    private const string BuiltInPortStr = "Built-in port";
    private const string CustomPortStr = "Custom port";

    private readonly IReadOnlyList<BasePort> _supportedPorts;
    private readonly CampaignsViewModel _viewModel;
    private readonly PortsProvider _portsProvider;
    private readonly BitmapsCache _bitmapsCache;

    private ImplicitAnimationCollection? _implicitAnimations;

    public CampaignsControl() : base(null!)
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
        InstalledAddonsProvider installedAddonsProvider,
        BitmapsCache bitmapsCache
        ) : base(installedAddonsProvider)
    {
        InitializeComponent();

        _supportedPorts = portsProvider.GetPortsThatSupportGame(viewModel.Game.GameEnum);
        _portsProvider = portsProvider;
        _viewModel = viewModel;
        _bitmapsCache = bitmapsCache;

        AddPortsButtons();

        _portsProvider.CustomPortChangedEvent += OnCustomPortChanged;
    }

    /// <summary>
    /// Add "Start with..." buttons to the ports button panel
    /// </summary>
    private void AddPortsButtons()
    {
        if (_viewModel.Game.GameEnum is GameEnum.Standalone)
        {
            TextBlock textBlock = new() { Text = BuiltInPortStr };

            Button button = new()
            {
                Content = textBlock,
                Command = new RelayCommand(() =>
                    _viewModel.StartCampaignCommand.Execute(new StubPort()),
                    () => CampaignsList?.SelectedItem is BaseAddon selectedCampaign),
                Margin = new(5),
                Padding = new(5),
            };

            BottomPanel.PortsButtonsPanel.Children.Add(button);

            return;
        }

        foreach (var port in _supportedPorts)
        {
            var portIcon = _bitmapsCache.GetFromCache(port.PortEnum.GetUniqueHash());

            StackPanel sp = new() { Orientation = Layout.Orientation.Horizontal };
            sp.Children.Add(new Image() { Margin = new(0, 0, 5, 0), Height = 16, Source = portIcon });
            sp.Children.Add(new TextBlock() { Text = port.ShortName });

            Button portButton = new()
            {
                Content = sp,
                Command = new RelayCommand(() =>
                    _viewModel.StartCampaignCommand.Execute(port),
                    () => PortsHelper.CheckPortRequirements(CampaignsList.SelectedItem, _viewModel.Game, port)),
                Margin = new(5),
                Padding = new(5),
            };

            BottomPanel.PortsButtonsPanel.Children.Add(portButton);
        }

        Button customPortButton = new()
        {
            Content = new TextBlock() { Text = BuiltInPortStr },
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
    /// Add button with custom ports
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

        MenuFlyout flyout = new();
        flyout.Placement = PlacementMode.Top;

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
            Content = new TextBlock() { Text = CustomPortStr },
            Margin = new(5),
            Padding = new(5),
            IsEnabled = false,
            IsVisible = true
        };

        customPortButton.Click += (sender, e) => flyout.ShowAt(customPortButton);

        BottomPanel.PortsButtonsPanel.Children.Add(customPortButton);
    }

    /// <summary>
    /// Add button to the right click menu
    /// </summary>
    private void AddContextMenuButtons()
    {
        CampaignsList.ContextMenu = new();

        if (CampaignsList.SelectedItem is not BaseAddon addon)
        {
            return;
        }

        CampaignsList.ContextMenu.Items.Clear();

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

    /// <summary>
    /// Invoked on selected campaign changed
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

        AddContextMenuButtons();
    }

    private void OnCustomPortChanged(object? sender, EventArgs e)
    {
        AddContextMenuButtons();
        AddCustomPortsButton();
    }

    /// <summary>
    /// Reset selected item when empty space is clicked
    /// </summary>
    private void OnCampaignsListEmptySpaceClicked(object? sender, Input.PointerPressedEventArgs e)
    {
        CampaignsList.SelectedItem = null;
        CampaignsList.Focusable = true;
        _ = CampaignsList.Focus();
        CampaignsList.Focusable = false;
    }

    private void OnListBoxContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is ListBoxItem item)
        {
            if (item.Content is SeparatorItem)
            {
                item.IsHitTestVisible = false;
                item.Focusable = false;
            }
            else
            {
                if (_implicitAnimations != null)
                {
                    EnsureImplicitAnimationsAsync(item);
                }
            }
        }
    }

    private void OnControlAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (_implicitAnimations == null)
        {
            var compositor = ElementComposition.GetElementVisual(this)?.Compositor;

            ArgumentNullException.ThrowIfNull(compositor);

            var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Target = "Offset";
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(400);

            var animationGroup = compositor.CreateAnimationGroup();
            animationGroup.Add(offsetAnimation);

            _implicitAnimations = compositor.CreateImplicitAnimationCollection();
            _implicitAnimations["Offset"] = animationGroup;
        }
    }

    private async void EnsureImplicitAnimationsAsync(Control control)
    {
        await Task.Delay(500).ConfigureAwait(true);

        var visual = ElementComposition.GetElementVisual(control);

        if (visual != null)
        {
            visual.ImplicitAnimations = _implicitAnimations;
        }
    }
}
