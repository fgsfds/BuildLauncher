using Common.Enums.Addons;

namespace Mods.Mods
{
    /// <summary>
    /// Duke Nukem 3D campaign
    /// </summary>
    public sealed class WangCampaign : BaseMod
    {
        /// <summary>
        /// Wang Addon enum
        /// </summary>
        public required WangAddonEnum AddonEnum { get; init; }
    }
}
