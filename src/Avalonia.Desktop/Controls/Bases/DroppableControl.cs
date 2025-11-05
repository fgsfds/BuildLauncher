using Addons.Providers;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Desktop.Misc;
using Avalonia.Input;

namespace Avalonia.Desktop.Controls.Bases;

public class DroppableControl : UserControl
{
    private readonly InstalledAddonsProvider _installedAddonsProvider;

    public DroppableControl(InstalledAddonsProvider installedAddonsProvider)
    {
        _installedAddonsProvider = installedAddonsProvider;
    }

    protected async void OnDrop(object sender, DragEventArgs e)
    {
        var files = e.Data.GetFiles();

        if (files?.Any() is true)
        {
            var filePaths = files.Select(f => f.Path.LocalPath);

            foreach (var file in filePaths)
            {
                var isAdded = await _installedAddonsProvider.CopyAddonIntoFolder(file).ConfigureAwait(false);

                if (isAdded)
                {
                    NotificationsHelper.Show(
                        "Addon added.",
                        NotificationType.Success
                        );
                }
                else
                {
                    NotificationsHelper.Show(
                        "Addon wasn't added.",
                        NotificationType.Error
                        );
                }
            }
        }
    }
}
