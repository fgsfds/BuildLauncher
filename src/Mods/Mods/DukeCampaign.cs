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
    }
}
