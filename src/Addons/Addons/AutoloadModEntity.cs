using System.Collections.Immutable;

namespace Addons.Addons;

public sealed class AutoloadModEntity : BaseAddon
{
    /// <summary>
    /// Is mod enabled
    /// </summary>
    public required bool IsEnabled { get; set; }

    /// <summary>
    /// List of additional cons
    /// </summary>
    public required ImmutableArray<string>? AdditionalCons { get; set; }
}
