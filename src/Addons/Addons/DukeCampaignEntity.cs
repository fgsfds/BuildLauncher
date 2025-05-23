using System.Collections.Immutable;

namespace Addons.Addons;

/// <summary>
/// Duke Nukem 3D campaign
/// </summary>
public sealed class DukeCampaignEntity : BaseAddonEntity
{
    /// <summary>
    /// Main .con file
    /// </summary>
    public required string? MainCon { get; init; }

    /// <summary>
    /// Additional .con files
    /// </summary>
    public required ImmutableArray<string>? AdditionalCons { get; init; }

    /// <summary>
    /// Main .rts file
    /// </summary>
    public required string? RTS { get; init; }
}
