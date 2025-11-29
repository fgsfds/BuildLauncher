using System.Diagnostics.CodeAnalysis;
using Addons.Addons;

namespace Avalonia.Desktop.Helpers;

public sealed class SeparatorItem : BaseAddon
{
    [SetsRequiredMembers]
    public SeparatorItem()
    {
        AddonId = null!;
        Title = null!;
    }
}
