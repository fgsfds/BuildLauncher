namespace Mods.Addons;

/// <summary>
/// Duke Nukem 3D campaign
/// </summary>
public sealed class DukeCampaign : Addon
{
    /// <summary>
    /// Main .con file
    /// </summary>
    public required string? MainCon { get; init; }

    /// <summary>
    /// Additional .con files
    /// </summary>
    public required HashSet<string>? AdditionalCons { get; init; }

    /// <summary>
    /// Main .rts file
    /// </summary>
    public required string? RTS { get; init; }
}
