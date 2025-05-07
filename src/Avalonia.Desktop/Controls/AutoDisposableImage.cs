using Avalonia.Controls;

namespace Avalonia.Desktop.Controls;

public sealed class AutoDisposableImage : Image
{
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (Source is IDisposable bmp)
        {
            Source = null;
            bmp.Dispose();
        }

        base.OnDetachedFromVisualTree(e);
    }
}
