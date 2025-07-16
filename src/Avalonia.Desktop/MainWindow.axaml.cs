using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Avalonia.Desktop;

public sealed partial class MainWindow : Window, IDisposable
{
    private readonly Stream? _overlayStream;

    public MainWindow()
    {

#if DEBUG
        this.AttachDevTools();
#endif

        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.HighQuality);

        InitializeComponent();

        var uri = new Uri("avares://BuildLauncher/Assets/overlay.png");
        _overlayStream = AssetLoader.Open(uri);
        Resources["HighlightOverlayBitmap"] = new Bitmap(_overlayStream);
    }

    public void Dispose()
    {
        _overlayStream?.Dispose();
    }

    private void OnWindowClosing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
    {
        _overlayStream?.Dispose();
    }
}
