using Common.Enums.Addons;

namespace Mods.Mods
{
    /// <summary>
    /// Shadow Warrior campaign
    /// </summary>
    public sealed class WangCampaign : Addon
    {
        /// <summary>
        /// Wang Addon enum
        /// </summary>
        public required WangAddonEnum AddonEnum { get; init; }
    }
}
