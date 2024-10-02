using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Diagnostics;

namespace Avalonia.Desktop.Helpers;

public static class Properties
{
    public static Window MainWindow => (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow
        ?? ThrowHelper.ThrowArgumentNullException<Window>(nameof(MainWindow));

    public static TopLevel TopLevel => TopLevel.GetTopLevel(MainWindow)
        ?? ThrowHelper.ThrowArgumentNullException<TopLevel>(nameof(TopLevel));
}
