namespace Mods.Addons;

public sealed class AutoloadMod : Addon
{
    /// <summary>
    /// Is mod enabled
    /// </summary>
    public required bool IsEnabled { get; set; }

    /// <summary>
    /// List of additional cons
    /// </summary>
    public required HashSet<string>? AdditionalCons { get; set; }
}
