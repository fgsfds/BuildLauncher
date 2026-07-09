using System.Collections.Immutable;

namespace Addons.Addons;

/// <summary>
///     Represents an autoload mod that can be toggled on or off at game startup.
/// </summary>
public sealed class AutoloadMod : BaseAddon
{
    /// <summary>
    ///     Is mod enabled.
    /// </summary>
    public required bool IsEnabled { get; set; }

    /// <summary>
    ///     List of additional .CON files.
    /// </summary>
    public required ImmutableArray<string>? AdditionalCons { get; set; }
}
