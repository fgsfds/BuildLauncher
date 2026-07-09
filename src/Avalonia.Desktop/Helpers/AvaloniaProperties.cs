using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace Avalonia.Desktop.Helpers;

/// <summary>
///     Provides access to Avalonia application-level properties.
/// </summary>
public static class AvaloniaProperties
{
    /// <summary>
    ///     Gets the main application window.
    /// </summary>
    public static Window MainWindow => (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow
                                    ?? throw new ArgumentNullException(nameof(MainWindow));

    /// <summary>
    ///     Gets the top level element of the main window.
    /// </summary>
    public static TopLevel TopLevel => TopLevel.GetTopLevel(MainWindow)
                                    ?? throw new ArgumentNullException(nameof(TopLevel));
}
