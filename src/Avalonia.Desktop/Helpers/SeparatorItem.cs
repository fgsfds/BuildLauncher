using System.Diagnostics.CodeAnalysis;
using Addons.Addons;

namespace Avalonia.Desktop.Helpers;

/// <summary>
///     Represents a visual separator item in addon lists.
/// </summary>
public sealed class SeparatorItem : BaseAddon
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SeparatorItem" /> class.
    /// </summary>
    [SetsRequiredMembers]
    public SeparatorItem()
    {
        AddonId = null!;
        Title = null!;
    }
}
