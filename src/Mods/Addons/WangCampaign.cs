using Common.Enums.Addons;

namespace Mods.Addons
{
    /// <summary>
    /// Shadow Warrior campaign
    /// </summary>
    public sealed class WangCampaign : Addon
    {
        /// <summary>
        /// Wang Addon enum
        /// </summary>
        public required WangAddonEnum RequiredAddonEnum { get; init; }
    }
}
