using Common.Enums.Addons;

namespace Mods.Mods
{
    /// <summary>
    /// Redneck Rampage campaign
    /// </summary>
    public sealed class RedneckCampaign : Addon
    {
        /// <summary>
        /// Redneck Addon enum
        /// </summary>
        public required RedneckAddonEnum AddonEnum { get; init; }

        public required string? MainCon { get; init; }

        public required HashSet<string>? AdditionalCons { get; init; }

        public required string? RTS { get; init; }
    }
}
