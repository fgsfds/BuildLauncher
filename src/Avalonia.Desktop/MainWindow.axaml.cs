using Avalonia.Controls;
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
    private readonly IConfigProvider _config;

    private readonly InstalledGamesProvider _installedGamesProvider;

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
    public MainWindow(
        InstalledGamesProvider installedGamesProvider,
        IConfigProvider config
        )
    {
        ArgumentNullException.ThrowIfNull(installedGamesProvider);
        ArgumentNullException.ThrowIfNull(config);

        _installedGamesProvider = installedGamesProvider;
        _config = config;

        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.HighQuality);

        InitializeComponent();

        EnableSystemBackdrop();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _config.ParameterChangedEvent -= OnConfigParameterChanged;

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
