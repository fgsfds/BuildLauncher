using Avalonia.Controls.Notifications;
using Avalonia.Desktop.Helpers;
using Avalonia.Platform.Storage;
using Common.Enums;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Ports.Providers;
using System.Collections.Immutable;

namespace Avalonia.Desktop.ViewModels;

public sealed partial class PortsViewModel : ObservableObject
{
    private readonly ViewModelsFactory _viewModelsFactory;
    private readonly InstalledPortsProvider _installedPortsProvider;
    private readonly Dictionary<PortEnum, bool> _updatesList = [];
    private readonly Lock _lock = new();
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly ILogger _logger;

    private bool _isNewPort;

    public bool HasUpdates
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _updatesList.Values.Any(x => x);
            }
        }
    }


    public PortsViewModel(
        ViewModelsFactory viewModelsFactory,
        InstalledPortsProvider installedPortsProvider,
        ILogger logger
        )
    {
        _viewModelsFactory = viewModelsFactory;
        _installedPortsProvider = installedPortsProvider;
        _logger = logger;

        Initialize();
    }


    public ImmutableList<PortViewModel> PortsList { get; set; } = [];

    public ImmutableList<CustomPort> CustomPorts => _installedPortsProvider.GetCustomPorts();

    public List<PortEnum> PortsTypes =>
        [
        PortEnum.EDuke32,
        PortEnum.Raze,
        PortEnum.VoidSW,
        PortEnum.NBlood,
        PortEnum.NotBlood,
        PortEnum.RedNukem,
        PortEnum.PCExhumed,
        ];

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
            var length = App.Random.Next(1, 100);
            var repeatedString = new string('\u200B', length);

            App.NotificationManager.Show(
                "Critical error! Exception is written to the log.",
                NotificationType.Error
                );

            _logger.LogCritical(ex, "=== Critical error ===");
        }
    }


    [RelayCommand(CanExecute = nameof(EditCustomPortCanExecute))]
    private async Task EditCustomPortAsync()
    {
        Guard.IsNotNull(SelectedCustomPort);

        try
        {
            ErrorMessage = string.Empty;
            SelectedCustomPortName = SelectedCustomPort.Value.Name;
            SelectedCustomPortPath = SelectedCustomPort.Value.Path;
            SelectedCustomPortType = SelectedCustomPort.Value.PortEnum;

            IsEditorVisible = true;
            _isNewPort = false;

            await _semaphore.WaitAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            var length = App.Random.Next(1, 100);
            var repeatedString = new string('\u200B', length);

            App.NotificationManager.Show(
                "Critical error! Exception is written to the log.",
                NotificationType.Error
                );

            _logger.LogCritical(ex, "=== Critical error ===");
        }
    }
    private bool EditCustomPortCanExecute => SelectedCustomPort is not null;


    [RelayCommand(CanExecute = nameof(DeleteCustomPortCanExecute))]
    private void DeleteCustomPort()
    {
        Guard.IsNotNull(SelectedCustomPort);

        try
        {
            _installedPortsProvider.DeleteCustomPort(SelectedCustomPort.Value.Name);

            OnPropertyChanged(nameof(CustomPorts));
        }
        catch (Exception ex)
        {
            var length = App.Random.Next(1, 100);
            var repeatedString = new string('\u200B', length);

            App.NotificationManager.Show(
                "Critical error! Exception is written to the log.",
                NotificationType.Error
                );

            _logger.LogCritical(ex, "=== Critical error ===");
        }
    }
    private bool DeleteCustomPortCanExecute => SelectedCustomPort is not null;


    [RelayCommand]
    private void SaveCustomPort()
    {
        try
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
            if (CustomPorts.Any(x => x.Name.Equals(SelectedCustomPortName, StringComparison.InvariantCultureIgnoreCase)) && _isNewPort)
            {
                ErrorMessage = "Port with the same name already exists";
                return;
            }
            if (!File.Exists(SelectedCustomPortPath))
            {
                ErrorMessage = "Executable doesn't exist";
                return;
            }

            ErrorMessage = string.Empty;

            _installedPortsProvider.AddOrChangeCustomPort(
                SelectedCustomPort.HasValue ? SelectedCustomPort.Value.Name : null,
                SelectedCustomPortName,
                SelectedCustomPortPath,
                SelectedCustomPortType.Value
                );

            OnPropertyChanged(nameof(CustomPorts));

        }
        catch (Exception ex)
        {
            var length = App.Random.Next(1, 100);
            var repeatedString = new string('\u200B', length);

            App.NotificationManager.Show(
                "Critical error! Exception is written to the log.",
                NotificationType.Error
                );

            _logger.LogCritical(ex, "=== Critical error ===");
        }
        finally
        {
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
            var length = App.Random.Next(1, 100);
            var repeatedString = new string('\u200B', length);

            App.NotificationManager.Show(
                "Critical error! Exception is written to the log.",
                NotificationType.Error
                );

            _logger.LogCritical(ex, "=== Critical error ===");
        }
    }


    /// <summary>
    /// Initialize VM
    /// </summary>
    private void Initialize()
    {
        using (_lock.EnterScope())
        {
            var edukeVm = _viewModelsFactory.GetPortViewModel(PortEnum.EDuke32);
            edukeVm.PortChangedEvent += OnPortChanged;
            _updatesList.Add(PortEnum.EDuke32, false);

            var razeVm = _viewModelsFactory.GetPortViewModel(PortEnum.Raze);
            razeVm.PortChangedEvent += OnPortChanged;
            _updatesList.Add(PortEnum.Raze, false);

            var nbloodVm = _viewModelsFactory.GetPortViewModel(PortEnum.NBlood);
            nbloodVm.PortChangedEvent += OnPortChanged;
            _updatesList.Add(PortEnum.NBlood, false);

            var notbloodVm = _viewModelsFactory.GetPortViewModel(PortEnum.NotBlood);
            notbloodVm.PortChangedEvent += OnPortChanged;
            _updatesList.Add(PortEnum.NotBlood, false);

            var pcexVm = _viewModelsFactory.GetPortViewModel(PortEnum.PCExhumed);
            pcexVm.PortChangedEvent += OnPortChanged;
            _updatesList.Add(PortEnum.PCExhumed, false);

            var rednukemVm = _viewModelsFactory.GetPortViewModel(PortEnum.RedNukem);
            rednukemVm.PortChangedEvent += OnPortChanged;
            _updatesList.Add(PortEnum.RedNukem, false);

            var bgdxVm = _viewModelsFactory.GetPortViewModel(PortEnum.BuildGDX);
            bgdxVm.PortChangedEvent += OnPortChanged;
            _updatesList.Add(PortEnum.BuildGDX, false);

            PortsList = [edukeVm, razeVm, nbloodVm, notbloodVm, pcexVm, rednukemVm, bgdxVm];
            OnPropertyChanged(nameof(PortsList));
        }
    }

    private void OnPortChanged(PortEnum portEnum)
    {
        _updatesList[portEnum] = PortsList.Find(x => x.Port.PortEnum == portEnum)!.IsUpdateAvailable;

        OnPropertyChanged(nameof(HasUpdates));
    }
}
