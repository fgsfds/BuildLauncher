using Common.Enums.Addons;

namespace Mods.Mods
{
    /// <summary>
    /// Redneck Rampage campaign
    /// </summary>
    public sealed class RedneckCampaign : BaseMod
    {
        /// <summary>
        /// Redneck Addon enum
        /// </summary>
        public required RedneckAddonEnum AddonEnum { get; set; }
    }
}
