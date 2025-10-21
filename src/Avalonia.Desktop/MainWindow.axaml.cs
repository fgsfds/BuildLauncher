using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Games.Providers;

namespace Avalonia.Desktop;

public sealed partial class MainWindow : Window, IDisposable
{
    private readonly InstalledGamesProvider _installedGamesProvider;
    private readonly Stream? _overlayStream;
    private readonly Bitmap? _overlayBitmap;

    public MainWindow(InstalledGamesProvider installedGamesProvider)
    {
        ArgumentNullException.ThrowIfNull(installedGamesProvider);
        _installedGamesProvider = installedGamesProvider;

#if DEBUG
        this.AttachDevTools();
#endif

        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.HighQuality);

        InitializeComponent();

        var uri = new Uri("avares://BuildLauncher/Assets/overlay.png");
        _overlayStream = AssetLoader.Open(uri);
        _overlayBitmap = new Bitmap(_overlayStream);
        Resources["HighlightOverlayBitmap"] = _overlayBitmap;
    }

    public void Dispose()
    {
        _overlayBitmap?.Dispose();
        _overlayStream?.Dispose();
    }

    private void OnWindowOpened(object? sender, EventArgs e)
    {
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
}
