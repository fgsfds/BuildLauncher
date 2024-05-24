using Common.Enums.Addons;

namespace Mods.Addons
{
    /// <summary>
    /// Redneck Rampage campaign
    /// </summary>
    public sealed class RedneckCampaign : Addon
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
}
