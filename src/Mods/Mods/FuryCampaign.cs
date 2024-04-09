namespace Mods.Mods
{
    /// <summary>
    /// Duke Nukem 3D campaign
    /// </summary>
    public sealed class FuryCampaign : Addon
    {
        public required string? MainCon { get; init; }

        public required HashSet<string>? AdditionalCons { get; init; }

        public required string? RTS { get; init; }
    }
}
