using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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
    ///     The overlay bitmap displayed on highlighted items.
    /// </summary>
    private readonly Bitmap? _overlayBitmap;

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

        var uri = new Uri("avares://BuildLauncher/Assets/overlay.png");
        using var overlayStream = AssetLoader.Open(uri);
        _overlayBitmap = new Bitmap(overlayStream);
        Resources["HighlightOverlayBitmap"] = _overlayBitmap;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _overlayBitmap?.Dispose();
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

        if (!_config.IsConsented)
        {
            ConsentWindow.IsVisible = true;
        }

        if (_installedGamesProvider.IsDukeInstalled)
        {
            DukeTab.IsSelected = true;
        }
        else if (_installedGamesProvider.IsBloodInstalled)
        {
            BloodTab.IsSelected = true;
        }
        else if (_installedGamesProvider.IsWangInstalled)
        {
            WangTab.IsSelected = true;
        }
        else if (_installedGamesProvider.IsFuryInstalled)
        {
            FuryTab.IsSelected = true;
        }
        else if (_installedGamesProvider.IsRedneckInstalled)
        {
            RedneckTab.IsSelected = true;
        }
        else if (_installedGamesProvider.IsSlaveInstalled)
        {
            SlaveTab.IsSelected = true;
        }
        else if (_installedGamesProvider.IsNamInstalled)
        {
            NamTab.IsSelected = true;
        }
        else if (_installedGamesProvider.IsWW2GIInstalled)
        {
            WW2GITab.IsSelected = true;
        }
        else if (_installedGamesProvider.IsWitchavenInstalled)
        {
            WitchavenTab.IsSelected = true;
        }
        else if (_installedGamesProvider.IsTekWarInstalled)
        {
            TekWarTab.IsSelected = true;
        }
    }

    /// <summary>
    ///     Handles the consent button click event.
    /// </summary>
    private void OnConsentButtonClick(object? sender, RoutedEventArgs e)
    {
        ConsentWindow.IsVisible = false;
        _config.IsConsented = true;
    }
}
