using Addons.Providers;
using Avalonia.Controls;
using Avalonia.Input;

namespace Avalonia.Desktop.Controls;

public sealed partial class ModsControl : UserControl
{
    private readonly InstalledAddonsProvider _installedAddonsProvider;

    public ModsControl()
    {
        InitializeComponent();
        _installedAddonsProvider = null!;
    }

    public ModsControl(InstalledAddonsProvider installedAddonsProvider)
    {
        InitializeComponent();
        _installedAddonsProvider = installedAddonsProvider;
    }

    private async void OnModsListDrop(object sender, DragEventArgs e)
    {
        var files = e.Data.GetFiles();

        if (files?.Any() is true)
        {
            var filePaths = files.Select(f => f.Path.LocalPath);

            foreach (var file in filePaths)
            {
                var isAdded = await _installedAddonsProvider.CopyAddonIntoFolder(file).ConfigureAwait(false);
            }
        }
    }
}
