using Avalonia.Controls;
using Avalonia.Desktop.Helpers;

namespace Avalonia.Desktop.Controls;

public sealed partial class PortControl : UserControl
{
    public PortControl()
    {
        if (Design.IsDesignMode)
        {
            Resources.Add(new("CachedHashToBitmapConverter", new CachedHashToBitmapConverter(new([], []))));
        }

        InitializeComponent();
    }
}
