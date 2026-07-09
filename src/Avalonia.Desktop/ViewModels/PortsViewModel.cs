using System.Collections.Immutable;
using Avalonia.Controls.Notifications;
using Avalonia.Desktop.Helpers;
using Avalonia.Desktop.Misc;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.All.Enums;
using Microsoft.Extensions.Logging;
using Ports.Ports;
using Ports.Providers;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class PortsViewModel : ObservableObject
{
    private readonly PortsProvider _installedPortsProvider;

    private readonly ILogger<PortsViewModel> _logger;

    /// <summary>
    ///     The semaphore for thread-safe editor operations.
    /// </summary>
    private readonly SemaphoreSlim _semaphore = new(1);

    private readonly ViewModelsFactory _viewModelsFactory;

    /// <summary>
    ///     Gets or sets the error message.
    /// </summary>
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>
    ///     Gets or sets whether there are port updates available.
    /// </summary>
    [ObservableProperty]
    private bool _hasUpdates = false;

    /// <summary>
    ///     Gets or sets whether the custom port editor is visible.
    /// </summary>
    [ObservableProperty]
    private bool _isEditorVisible;

    /// <summary>
    ///     Whether a new custom port is being added (as opposed to editing an existing one).
    /// </summary>
    private bool _isNewPort;

    /// <summary>
    ///     Gets or sets the selected custom port.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCustomPortCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCustomPortCommand))]
    private CustomPort? _selectedCustomPort;

    /// <summary>
    ///     Gets or sets the selected custom port name.
    /// </summary>
    [ObservableProperty]
    private string? _selectedCustomPortName;

    /// <summary>
    ///     Gets or sets the selected custom port path.
    /// </summary>
    [ObservableProperty]
    private string? _selectedCustomPortPath;

    /// <summary>
    ///     Gets or sets the selected custom port type.
    /// </summary>
    [ObservableProperty]
    private PortEnum? _selectedCustomPortType;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PortsViewModel" /> class.
    /// </summary>
    /// <param name="viewModelsFactory">The view models factory.</param>
    /// <param name="installedPortsProvider">The installed ports provider.</param>
    /// <param name="ports">The available ports.</param>
    /// <param name="logger">The logger.</param>
    public PortsViewModel(
        ViewModelsFactory viewModelsFactory,
        PortsProvider installedPortsProvider,
        IEnumerable<BasePort> ports,
        ILogger<PortsViewModel> logger
        )
    {
        _viewModelsFactory = viewModelsFactory;
        _installedPortsProvider = installedPortsProvider;
        _logger = logger;

        PortsTypes = [.. ports.Select(x => x.PortEnum)];

        Initialize(ports);
    }

    /// <summary>
    ///     Gets the list of custom ports.
    /// </summary>
    public ImmutableList<CustomPort> CustomPorts => _installedPortsProvider.GetCustomPorts();

    /// <summary>
    ///     Gets or sets the list of port view models.
    /// </summary>
    public ImmutableList<PortViewModel> PortsList { get; set; } = [];

    /// <summary>
    ///     Gets the list of available port types.
    /// </summary>
    public ImmutableList<PortEnum> PortsTypes { get; }
    /// <summary>
    ///     Determines whether the edit custom port command can execute.
    /// </summary>
    private bool EditCustomPortCanExecute => SelectedCustomPort is not null;

    /// <summary>
    ///     Determines whether the delete custom port command can execute.
    /// </summary>
    private bool DeleteCustomPortCanExecute => SelectedCustomPort is not null;

    /// <summary>
    ///     Opens the editor to add a new custom port.
    /// </summary>
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


    /// <summary>
    ///     Opens the editor to modify the selected custom port.
    /// </summary>
    [RelayCommand(CanExecute = nameof(EditCustomPortCanExecute))]
    private async Task EditCustomPortAsync()
    {
        ArgumentNullException.ThrowIfNull(SelectedCustomPort);

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


    /// <summary>
    ///     Deletes the selected custom port.
    /// </summary>
    [RelayCommand(CanExecute = nameof(DeleteCustomPortCanExecute))]
    private void DeleteCustomPort()
    {
        ArgumentNullException.ThrowIfNull(SelectedCustomPort);

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


    /// <summary>
    ///     Saves the custom port configuration.
    /// </summary>
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


    /// <summary>
    ///     Cancels the custom port editor.
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        IsEditorVisible = false;

        _ = _semaphore.Release();
    }

    /// <summary>
    ///     Opens a file picker to select a port executable.
    /// </summary>
    [RelayCommand]
    private async Task SelectPortExeAsync()
    {
        try
        {
            var file = await AvaloniaProperties.TopLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Select port executable",
                    AllowMultiple = false
                }
                ).ConfigureAwait(true);

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
    ///     Initialize VM
    /// </summary>
    /// <summary>
    ///     Initializes the port view models.
    /// </summary>
    /// <param name="ports">The available ports.</param>
    private void Initialize(IEnumerable<BasePort> ports)
    {
        List<PortViewModel> viewModels = [];

        foreach (var port in ports.Where(x => x.IsDownloadable))
        {
            var vm = _viewModelsFactory.GetPortViewModel(port);
            vm.PortChangedEvent += OnPortChanged;
            viewModels.Add(vm);
        }

        PortsList = [.. viewModels];
        OnPropertyChanged(nameof(PortsList));
    }

    /// <summary>
    ///     Handles the port changed event.
    /// </summary>
    private void OnPortChanged(PortEnum portEnum)
    {
        HasUpdates = PortsList.Any(static x => x.IsUpdateAvailable);
    }
}
