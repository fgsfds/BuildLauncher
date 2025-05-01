using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

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
    }
}
