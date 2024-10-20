using Addons.Providers;
using Avalonia.Controls;
using Avalonia.Desktop.Helpers;
using Avalonia.Desktop.ViewModels;
using Avalonia.Input;
using Avalonia.Media;
using Common.Client.Interfaces;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using CommunityToolkit.Mvvm.Input;
using Ports.Ports;
using Ports.Providers;
using System.Globalization;

namespace Avalonia.Desktop.Controls;

public sealed partial class CampaignsControl : UserControl
{
    private const string BuiltInPortStr = "Built-in port";

    private IEnumerable<BasePort> _supportedPorts;
    private CampaignsViewModel _viewModel;
    private InstalledAddonsProvider _installedAddonsProvider;

    public CampaignsControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initialize control
    /// </summary>
    public void InitializeControl(
        PortsProvider portsProvider,
        InstalledAddonsProviderFactory installedAddonsProviderFactory,
        IConfigProvider configProvider
        )
    {
        DataContext.ThrowIfNotType<CampaignsViewModel>(out var viewModel);

        _viewModel = viewModel;
        _supportedPorts = portsProvider.GetPortsThatSupportGame(_viewModel.Game.GameEnum);
        _installedAddonsProvider = installedAddonsProviderFactory.GetSingleton(_viewModel.Game);

        CampaignsList.SelectionChanged += OnCampaignsListSelectionChanged;
        BottomPanel.DataContext = viewModel;

        RightPanel.InitializeControl(configProvider);

        AddPortsButtons();

        AddContextMenuButtons();
    }

    /// <summary>
    /// Add "Start with..." buttons to the ports button panel
    /// </summary>
    private void AddPortsButtons()
    {
        ImagePathToBitmapConverter converter = new();

        if (_viewModel.Game.GameEnum is GameEnum.Standalone)
        {
            TextBlock textBlock = new() { Text = BuiltInPortStr };

            Button button = new()
            {
                Content = textBlock,
                Command = new RelayCommand(() =>
                    _viewModel.StartCampaignCommand.Execute(new StubPort()),
                    () =>
                    {
                        if (CampaignsList?.SelectedItem is not IAddon selectedCampaign)
                        {
                            return false;
                        }

                        return true;
                    }),
                Margin = new(5),
                Padding = new(5),
            };

            BottomPanel.PortsButtonsPanel.Children.Add(button);

            return;
        }

        foreach (var port in _supportedPorts)
        {
            var portIcon = converter.Convert(port.Icon, typeof(IImage), null!, CultureInfo.InvariantCulture) as IImage;

            StackPanel sp = new() { Orientation = Layout.Orientation.Horizontal };
            sp.Children.Add(new Image() { Margin = new(0, 0, 5, 0), Height = 16, Source = portIcon });
            sp.Children.Add(new TextBlock() { Text = port.Name });

            Button portButton = new()
            {
                Content = sp,
                Command = new RelayCommand(() =>
                    _viewModel.StartCampaignCommand.Execute(port),
                    () =>
                    {
                        if (CampaignsList?.SelectedItem is not IAddon selectedCampaign)
                        {
                            return false;
                        }

                        if (selectedCampaign.Executables?[OSEnum.Windows] is not null)
                        {
                            if (port.Exe.Equals(Path.GetFileName(selectedCampaign.Executables[OSEnum.Windows]), StringComparison.InvariantCultureIgnoreCase))
                            {
                                return true;
                            }

                            return false;
                        }

                        if (!port.IsInstalled)
                        {
                            return false;
                        }

                        if (port.PortEnum is PortEnum.BuildGDX && selectedCampaign.Type is not AddonTypeEnum.Official)
                        {
                            return false;
                        }

                        if (selectedCampaign.RequiredFeatures is not null &&
                            selectedCampaign.RequiredFeatures!.Except(port.SupportedFeatures).Any())
                        {
                            return false;
                        }

                        if (!port.SupportedGames.Contains(selectedCampaign.SupportedGame.GameEnum))
                        {
                            return false;
                        }

                        if (selectedCampaign.SupportedGame.GameVersion is not null &&
                            !port.SupportedGamesVersions.Contains(selectedCampaign.SupportedGame.GameVersion))
                        {
                            return false;
                        }

                        return true;
                    }),
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
                        if (CampaignsList?.SelectedItem is not IAddon selectedCampaign)
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
    }

    /// <summary>
    /// Add button to the right click menu
    /// </summary>
    private void AddContextMenuButtons()
    {
        CampaignsList.ContextMenu = new();

        if (CampaignsList.SelectedItem is not IAddon addon)
        {
            return;
        }

        CampaignsList.ContextMenu.Items.Clear();


        foreach (var port in _supportedPorts)
        {
            if (!port.IsInstalled ||
                (addon.RequiredFeatures is not null && addon.RequiredFeatures.Except(port.SupportedFeatures).Any()) ||
                (addon.Type is not AddonTypeEnum.Official && port.PortEnum is PortEnum.BuildGDX))
            {
                continue;
            }

            var portButton = new MenuItem()
            {
                Header = $"Start with {port.Name}",
                Command = new RelayCommand(() => _viewModel.StartCampaignCommand.Execute(port))
            };

            _ = CampaignsList.ContextMenu.Items.Add(portButton);
        }

        if (CampaignsList.ContextMenu.Items.Count > 0)
        {
            _ = CampaignsList.ContextMenu.Items.Add(new Separator());
        }

        var deleteButton = new MenuItem()
        {
            Header = "Delete",
            Command = new RelayCommand(
                () => _viewModel.DeleteCampaignCommand.Execute(null),
                () => addon.Type is not AddonTypeEnum.Official
                )
        };

        _ = CampaignsList.ContextMenu.Items.Add(deleteButton);
    }

    /// <summary>
    /// Update available ports buttons
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
    private void ListBoxEmptySpaceClicked(object? sender, Input.PointerPressedEventArgs e)
    {
        CampaignsList.SelectedItem = null;
        CampaignsList.Focusable = true;
        _ = CampaignsList.Focus();
        CampaignsList.Focusable = false;
    }

    /// <summary>
    /// Drag'n'drop handler
    /// </summary>
    private async void FilesDataGrid_DropAsync(object sender, DragEventArgs e)
    {
        var files = e.Data.GetFiles();

        if (files is not null && files.Any())
        {
            var filePaths = files.Select(f => f.Path.LocalPath);

            foreach (var file in filePaths)
            {
                var isAdded = await _installedAddonsProvider.CopyAddonIntoFolder(file).ConfigureAwait(false);
            }
        }
    }
}
