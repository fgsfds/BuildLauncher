using Addons.Providers;
using Avalonia.Desktop.Controls.Bases;

namespace Avalonia.Desktop.Controls;

public sealed partial class ModsControl : DroppableControl
{
    public ModsControl() : base(null!)
    {
        InitializeComponent();
    }

    public ModsControl(InstalledAddonsProvider installedAddonsProvider) : base(installedAddonsProvider)
    {
        InitializeComponent();
    }
}
