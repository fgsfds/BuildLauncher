using Avalonia.Controls;
using Avalonia.Desktop.Helpers;

namespace Avalonia.Desktop.Controls;

/// <summary>
///     Displays information and actions for a single source port.
/// </summary>
public sealed partial class PortControl : UserControl
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PortControl" /> class.
    /// </summary>
    public PortControl()
    {
        if (Design.IsDesignMode)
        {
            Resources.Add(new("CachedHashToBitmapConverter", new CachedHashToBitmapConverter(new([], []))));
        }

        InitializeComponent();
    }
}
