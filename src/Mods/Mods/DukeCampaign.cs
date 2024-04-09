using Common.Enums.Addons;

namespace Mods.Mods
{
    /// <summary>
    /// Duke Nukem 3D campaign
    /// </summary>
    public sealed class DukeCampaign : Addon
    {
        /// <summary>
        /// Duke Addon enum
        /// </summary>
        public required DukeAddonEnum AddonEnum { get; init; }

        public required string? MainCon { get; init; }

        public required HashSet<string>? AdditionalCons { get; init; }

        public required string? RTS { get; init; }
    }
}
