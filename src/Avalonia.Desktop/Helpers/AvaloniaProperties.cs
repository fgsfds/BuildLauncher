using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace Avalonia.Desktop.Helpers;

public static class AvaloniaProperties
{
    public static Window MainWindow => (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow
        ?? throw new ArgumentNullException(nameof(MainWindow));

    public static TopLevel TopLevel => TopLevel.GetTopLevel(MainWindow)
        ?? throw new ArgumentNullException(nameof(TopLevel));
}
