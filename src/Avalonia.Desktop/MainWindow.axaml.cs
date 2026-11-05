using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Avalonia.Controls;
using Avalonia.Desktop.Helpers;
using Avalonia.Desktop.ViewModels;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Core.Client.Interfaces;
using Games.Providers;

namespace Avalonia.Desktop;

/// <summary>
///     Represents the main application window.
/// </summary>
public sealed partial class MainWindow : Window, IDisposable
{
    /// <summary> The persisted application settings. </summary>
    private readonly IConfigProvider _config;

    /// <summary> Records best-effort settings persistence failures. </summary>
    private readonly ILogger<MainWindow> _logger = NullLogger<MainWindow>.Instance;

    /// <summary> The installed games used for the default tab selection. </summary>
    private readonly InstalledGamesProvider _installedGamesProvider;

    /// <summary> The last normal bounds, retained while maximized or minimized. </summary>
    private WindowPlacement? _normalPlacement;

    /// <summary> Whether startup restoration has completed. </summary>
    private bool _isOpened;

    /// <summary> The last non-minimized window state. </summary>
    private bool _wasMaximized;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MainWindow" /> class.
    /// </summary>
    public MainWindow()
    {
        _installedGamesProvider = null!;
        _config = null!;

        InitializeComponent();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MainWindow" /> class.
    /// </summary>
    /// <param name="installedGamesProvider">The installed games provider.</param>
    /// <param name="config">The configuration provider.</param>
    /// <param name="logger">Records settings persistence failures.</param>
    public MainWindow(
        InstalledGamesProvider installedGamesProvider,
        IConfigProvider config,
        ILogger<MainWindow>? logger = null
        )
    {
        ArgumentNullException.ThrowIfNull(installedGamesProvider);
        ArgumentNullException.ThrowIfNull(config);

        _installedGamesProvider = installedGamesProvider;
        _config = config;
        _logger = logger ?? NullLogger<MainWindow>.Instance;

        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.HighQuality);

        InitializeComponent();

        RestoreWindowPlacement();
        PositionChanged += OnPositionChanged;
        EnableSystemBackdrop();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _config.ParameterChangedEvent -= OnConfigParameterChanged;
        PropertyChanged -= OnWindowPropertyChanged;
        PositionChanged -= OnPositionChanged;

        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.Dispose();
        }
    }

    /// <summary>
    ///     Handles the window opened event.
    /// </summary>
    private void OnWindowOpened(object? sender, EventArgs e)
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        UpdateBackdropState();

        if (!_config.IsConsented)
        {
            ConsentWindow!.IsVisible = true;
        }

        MainTabs.SelectedItem = SettingsTab;

        if (_installedGamesProvider.IsDukeInstalled)
        {
            DukeTab!.IsSelected = true;
        }
        else if (_installedGamesProvider.IsBloodInstalled)
        {
            BloodTab!.IsSelected = true;
        }
        else if (_installedGamesProvider.IsWangInstalled)
        {
            WangTab!.IsSelected = true;
        }
        else if (_installedGamesProvider.IsFuryInstalled)
        {
            FuryTab!.IsSelected = true;
        }
        else if (_installedGamesProvider.IsRedneckInstalled)
        {
            RedneckTab!.IsSelected = true;
        }
        else if (_installedGamesProvider.IsSlaveInstalled)
        {
            SlaveTab!.IsSelected = true;
        }
        else if (_installedGamesProvider.IsNamInstalled)
        {
            NamTab!.IsSelected = true;
        }
        else if (_installedGamesProvider.IsWW2GIInstalled)
        {
            WW2GITab!.IsSelected = true;
        }
        else if (_installedGamesProvider.IsWitchavenInstalled)
        {
            WitchavenTab!.IsSelected = true;
        }
        else if (_installedGamesProvider.IsTekWarInstalled)
        {
            TekWarTab!.IsSelected = true;
        }

        var savedTabName = _config.ActiveTab;
        var savedTab = MainTabs.Items.OfType<TabItem>().FirstOrDefault(tab =>
            tab.Name == savedTabName && tab.IsEnabled && tab.IsVisible);

        if (savedTab is not null)
        {
            MainTabs.SelectedItem = savedTab;
        }

        _isOpened = true;
        CaptureWindowPlacement();
    }

    /// <summary> Restores bounds before showing the window and fits them to the current monitor layout. </summary>
    private void RestoreWindowPlacement()
    {
        var saved = WindowPlacement.Parse(_config.WindowPlacement);
        var placement = saved ?? new WindowPlacement(Position, new Size(1300, 800), true);
        var fitted = placement.FitToScreens(Screens.All.Select(screen => (screen.WorkingArea, screen.Scaling)).ToArray());

        MinWidth = Math.Min(1300, fitted.Size.Width);
        MinHeight = Math.Min(800, fitted.Size.Height);
        Width = fitted.Size.Width;
        Height = fitted.Size.Height;
        _normalPlacement = fitted;
        _wasMaximized = fitted.IsMaximized;

        if (saved is not null)
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Position = fitted.Position;
            WindowState = fitted.IsMaximized ? WindowState.Maximized : WindowState.Normal;
        }
    }

    /// <summary> Captures normal bounds without replacing them with maximized or minimized bounds. </summary>
    private void CaptureWindowPlacement()
    {
        if (!_isOpened)
        {
            return;
        }

        if (WindowState != WindowState.Minimized)
        {
            _wasMaximized = WindowState == WindowState.Maximized;
        }

        if (WindowState == WindowState.Normal && Bounds.Width > 0 && Bounds.Height > 0)
        {
            _normalPlacement = new WindowPlacement(Position, Bounds.Size, false);
        }
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BoundsProperty || change.Property == WindowStateProperty)
        {
            CaptureWindowPlacement();
        }
    }

    /// <summary> Captures the normal window position when it moves. </summary>
    /// <param name="sender">The window that moved.</param>
    /// <param name="e">The new desktop position.</param>
    private void OnPositionChanged(object? sender, PixelPointEventArgs e) => CaptureWindowPlacement();

    /// <inheritdoc />
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (e.Cancel || !_isOpened)
        {
            return;
        }

        CaptureWindowPlacement();

        try
        {
            if (_normalPlacement is not null)
            {
                _config.WindowPlacement = (_normalPlacement with { IsMaximized = _wasMaximized }).ToString();
            }

            if (MainTabs.SelectedItem is TabItem { Name: not null } tab)
            {
                _config.ActiveTab = tab.Name;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not save main window settings.");
        }
    }

    /// <summary>
    ///     Handles the consent button click event.
    /// </summary>
    private void OnConsentButtonClick(object? sender, RoutedEventArgs e)
    {
        ConsentWindow!.IsVisible = false;
        _config.IsConsented = true;
    }

    /// <summary> Requests a system backdrop on Windows, falling back to acrylic and blur when Mica is unavailable. </summary>
    private void EnableSystemBackdrop()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        PropertyChanged += OnWindowPropertyChanged;
        _config.ParameterChangedEvent += OnConfigParameterChanged;

        ApplyBackdrop();
    }

    /// <summary> Applies or removes the system backdrop according to the current configuration. </summary>
    private void ApplyBackdrop()
    {
        TransparencyLevelHint = _config.UseMica
            ?
            [
                WindowTransparencyLevel.Mica,
                WindowTransparencyLevel.AcrylicBlur,
                WindowTransparencyLevel.Blur,
                WindowTransparencyLevel.None
            ]
            : [WindowTransparencyLevel.None];

        UpdateBackdropState();
    }

    /// <summary> Re-applies the system backdrop when the Mica setting changes. </summary>
    /// <param name="parameterName"> The name of the changed configuration parameter. </param>
    private void OnConfigParameterChanged(string? parameterName)
    {
        if (parameterName == nameof(IConfigProvider.UseMica))
        {
            ApplyBackdrop();
        }
    }

    /// <summary> Enables the transparent window background when the platform provided an actual system backdrop. </summary>
    private void UpdateBackdropState()
    {
        var hasBackdrop =
            ActualTransparencyLevel.Equals(WindowTransparencyLevel.Mica)
         || ActualTransparencyLevel.Equals(WindowTransparencyLevel.AcrylicBlur)
         || ActualTransparencyLevel.Equals(WindowTransparencyLevel.Blur);

        Classes.Set("mica", hasBackdrop);
    }

    /// <summary> Updates the backdrop state when the platform changes the achieved transparency level. </summary>
    /// <param name="sender"> The event source. </param>
    /// <param name="e"> The property change arguments. </param>
    private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != ActualTransparencyLevelProperty)
        {
            return;
        }

        UpdateBackdropState();
    }
}
