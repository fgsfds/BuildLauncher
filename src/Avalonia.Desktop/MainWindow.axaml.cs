using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Avalonia.Desktop;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {

#if DEBUG
        this.AttachDevTools();
#endif
        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.HighQuality);

        InitializeComponent();

        var uri = new Uri("avares://BuildLauncher/Assets/overlay.png");
        Resources["HighlightOverlayBitmap"] = new Bitmap(AssetLoader.Open(uri));
    }
}
