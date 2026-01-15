using System.Collections.Immutable;
using Avalonia.Controls.Notifications;
using Avalonia.Desktop.Helpers;
using Avalonia.Desktop.Misc;
using Avalonia.Platform.Storage;
using Common.All.Enums;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Ports.Ports;
using Ports.Providers;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class PortsViewModel : ObservableObject
{
    private readonly ViewModelsFactory _viewModelsFactory;
    private readonly PortsProvider _installedPortsProvider;
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly ILogger _logger;

    private bool _isNewPort;

    public ImmutableList<CustomPort> CustomPorts => _installedPortsProvider.GetCustomPorts();

    public ImmutableList<PortViewModel> PortsList { get; set; } = [];

    public ImmutableList<PortEnum> PortsTypes { get; }

    [ObservableProperty]
    private bool _hasUpdates = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCustomPortCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCustomPortCommand))]
    private CustomPort? _selectedCustomPort;

    [ObservableProperty]
    private string? _selectedCustomPortName;

    [ObservableProperty]
    private string? _selectedCustomPortPath;

    [ObservableProperty]
    private PortEnum? _selectedCustomPortType;

    [ObservableProperty]
    private bool _isEditorVisible;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public PortsViewModel(
        ViewModelsFactory viewModelsFactory,
        PortsProvider installedPortsProvider,
        IReadOnlyList<BasePort> ports,
        ILogger logger
        )
    {
        _viewModelsFactory = viewModelsFactory;
        _installedPortsProvider = installedPortsProvider;
        _logger = logger;

        PortsTypes = [.. ports.Select(x => x.PortEnum)];

        Initialize(ports);
    }

    [RelayCommand]
    private async Task AddCustomPortAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            SelectedCustomPortName = string.Empty;
            SelectedCustomPortPath = string.Empty;
            SelectedCustomPortType = null;
            SelectedCustomPort = null;

            IsEditorVisible = true;
            _isNewPort = true;

            await _semaphore.WaitAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            NotificationsHelper.Show(
                "Critical error! Exception is written to the log.",
                NotificationType.Error
                );

            _logger.LogCritical(ex, "=== Error while adding custom prot ===");
        }
    }


    [RelayCommand(CanExecute = nameof(EditCustomPortCanExecute))]
    private async Task EditCustomPortAsync()
    {
        Guard.IsNotNull(SelectedCustomPort);

        try
        {
            ErrorMessage = string.Empty;
            SelectedCustomPortName = SelectedCustomPort.Name;
            SelectedCustomPortPath = SelectedCustomPort.Path;
            SelectedCustomPortType = SelectedCustomPort.PortEnum;

            IsEditorVisible = true;
            _isNewPort = false;

            await _semaphore.WaitAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            NotificationsHelper.Show(
                "Critical error! Exception is written to the log.",
                NotificationType.Error
                );

            _logger.LogCritical(ex, "=== Error while editing custom port ===");
        }
    }
    private bool EditCustomPortCanExecute => SelectedCustomPort is not null;


    [RelayCommand(CanExecute = nameof(DeleteCustomPortCanExecute))]
    private void DeleteCustomPort()
    {
        Guard.IsNotNull(SelectedCustomPort);

        try
        {
            _installedPortsProvider.DeleteCustomPort(SelectedCustomPort.Name);

            OnPropertyChanged(nameof(CustomPorts));
        }
        catch (Exception ex)
        {
            NotificationsHelper.Show(
                "Critical error! Exception is written to the log.",
                NotificationType.Error
                );

            _logger.LogCritical(ex, "=== Error while deleting custom port ===");
        }
    }
    private bool DeleteCustomPortCanExecute => SelectedCustomPort is not null;


    [RelayCommand]
    private void SaveCustomPort()
    {
        IsEditorVisible = true;

        if (string.IsNullOrWhiteSpace(SelectedCustomPortName))
        {
            ErrorMessage = "Name is required";
            return;
        }
        if (string.IsNullOrWhiteSpace(SelectedCustomPortPath))
        {
            ErrorMessage = "Path to exe is required";
            return;
        }
        if (SelectedCustomPortType is null)
        {
            ErrorMessage = "Port type is required";
            return;
        }
        if (CustomPorts.Any(x => x.Name.Equals(SelectedCustomPortName, StringComparison.OrdinalIgnoreCase)) && _isNewPort)
        {
            ErrorMessage = "Port with the same name already exists";
            return;
        }
        if (!File.Exists(SelectedCustomPortPath))
        {
            ErrorMessage = "Executable doesn't exist";
            return;
        }

        try
        {
            _installedPortsProvider.AddOrChangeCustomPort(
                SelectedCustomPort?.Name,
                SelectedCustomPortName,
                SelectedCustomPortPath,
                SelectedCustomPortType.Value
                );

            OnPropertyChanged(nameof(CustomPorts));

        }
        catch (Exception ex)
        {
            NotificationsHelper.Show(
                "Critical error! Exception is written to the log.",
                NotificationType.Error
                );

            _logger.LogCritical(ex, "=== Error while saving custom port ===");
        }
        finally
        {
            ErrorMessage = string.Empty;
            IsEditorVisible = false;

            _ = _semaphore.Release();
        }
    }


    [RelayCommand]
    private void Cancel()
    {
        IsEditorVisible = false;

        _ = _semaphore.Release();
    }


    [RelayCommand]
    private async Task SelectPortExeAsync()
    {
        try
        {
            var file = await AvaloniaProperties.TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select port executable",
                AllowMultiple = false
            }).ConfigureAwait(true);

            if (file is null or [])
            {
                return;
            }

            SelectedCustomPortPath = file[0].Path.LocalPath;
        }
        catch (Exception ex)
        {
            NotificationsHelper.Show(
                "Critical error! Exception is written to the log.",
                NotificationType.Error
                );

            _logger.LogCritical(ex, "=== Error while selecting exe from custom port ===");
        }
    }


    /// <summary>
    /// Initialize VM
    /// </summary>
    private void Initialize(IReadOnlyList<BasePort> ports)
    {
        List<PortViewModel> viewModels = new(ports.Count);

        foreach (var port in ports.Where(x => x.IsDownloadable))
        {
            var vm = _viewModelsFactory.GetPortViewModel(port);
            vm.PortChangedEvent += OnPortChanged;
            viewModels.Add(vm);
        }

        PortsList = [.. viewModels];
        OnPropertyChanged(nameof(PortsList));
    }

    private void OnPortChanged(PortEnum portEnum)
    {
        var isUpdateAvailable = PortsList.Find(x => x.Port.PortEnum == portEnum)?.IsUpdateAvailable ?? false;

        if (!HasUpdates && isUpdateAvailable)
        {
            HasUpdates = true;
        }
    }
}
