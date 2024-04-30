using Common.Enums.Addons;

namespace Mods.Addons
{
    /// <summary>
    /// Duke Nukem 3D campaign
    /// </summary>
    public sealed class DukeCampaign : Addon
    {
        /// <summary>
        /// Duke Addon enum
        /// </summary>
        public required DukeAddonEnum RequiredAddonEnum { get; init; }

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
        public required HashSet<string>? GRPs { get; init; }

        /// <summary>
        /// Main .rts file
        /// </summary>
        public required string? RTS { get; init; }
    }
}
