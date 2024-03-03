using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Common.Helpers;

namespace BuildLauncher.Helpers
{
    public static class Properties
    {
        public static Window MainWindow => (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow
            ?? ThrowHelper.ArgumentNullException<Window>(nameof(MainWindow));

        public static TopLevel TopLevel => TopLevel.GetTopLevel(MainWindow)
            ?? ThrowHelper.ArgumentNullException<TopLevel>(nameof(TopLevel));
    }
}
