using Avalonia;
using Avalonia.Controls;
using Avalonia.Desktop.Helpers;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Optris.Icons.Avalonia;
using Optris.Icons.Avalonia.FontAwesome7;

namespace Tests.Unit.Sequential.Helpers;

/// <summary> Hosts production controls with application styles and keyboard input, without starting application services. </summary>
internal sealed class HeadlessView : IDisposable
{
    /// <summary> Tracks the process-wide icon registration across headless application instances. </summary>
    private static bool _iconsRegistered;

    /// <summary> The window hosting the control under test. </summary>
    public Window Window { get; }

    /// <summary> Registers resources needed while loading production XAML. </summary>
    public static void Prepare()
    {
        var app = Application.Current!;
        if (!app.Resources.ContainsKey("CachedHashToBitmapConverter"))
        {
            app.Resources["CachedHashToBitmapConverter"] = new CachedHashToBitmapConverter();
        }

        if (!_iconsRegistered)
        {
            _ = IconProvider.Current.Register<FontAwesome7IconProvider>();
            _iconsRegistered = true;
        }
    }

    /// <summary> Shows the control with the real Fluent and DataGrid templates. </summary>
    /// <param name="control">The production view and its data context.</param>
    public HeadlessView(Control control)
    {
        Window = new Window { Width = 1200, Height = 900, Content = control };
        Window.Styles.Add(new FluentTheme());
        Window.Styles.Add(new StyleInclude(new Uri("avares://Avalonia.Controls.DataGrid/"))
        {
            Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
        });
        Window.Show();
        Dispatcher.UIThread.RunJobs();
    }

    /// <summary> Finds a button by its displayed label. </summary>
    /// <param name="label">The displayed button label.</param>
    /// <returns>The matching button.</returns>
    public Button Button(string label) => Window.GetVisualDescendants().OfType<Button>()
        .Single(x => x.Content?.ToString()?.Trim() == label);

    /// <summary> Activates a focused button through headless keyboard input. </summary>
    /// <param name="button">The enabled button to activate.</param>
    public void Click(Button button)
    {
        Assert.True(button.IsEffectivelyVisible);
        Assert.True(button.IsEffectivelyEnabled);
        Assert.True(button.Focus());
        Window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
        Window.KeyRelease(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
        Dispatcher.UIThread.RunJobs();
    }

    /// <summary> Closes the window and releases the view tree. </summary>
    public void Dispose()
    {
        Window.Close();
        Window.Content = null;
        Dispatcher.UIThread.RunJobs();
    }
}
