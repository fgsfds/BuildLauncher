using Avalonia.Controls;
using Avalonia.Desktop.Helpers;

namespace Avalonia.Desktop.Controls;

/// <summary>
///     Displays information and actions for a single tool.
/// </summary>
public sealed partial class ToolControl : UserControl
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ToolControl" /> class.
    /// </summary>
    public ToolControl()
    {
        if (Design.IsDesignMode)
        {
            Resources.Add(new("CachedHashToBitmapConverter", new CachedHashToBitmapConverter(new([], []))));
        }

        InitializeComponent();
    }
}
