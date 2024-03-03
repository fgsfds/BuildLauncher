using Common.Enums.Addons;

namespace Mods.Mods
{
    /// <summary>
    /// Duke Nukem 3D campaign
    /// </summary>
    public sealed class DukeCampaign : BaseMod
    {
        /// <summary>
        /// Duke Addon enum
        /// </summary>
        public required DukeAddonEnum AddonEnum { get; set; }

        /// <summary>
        /// CON file that will be loaded unless a port-specific CON is present
        /// </summary>
        public required string? ConFile { get; init; }
    }
}
