using Common.Enums.Addons;

namespace Mods.Mods
{
    /// <summary>
    /// Blood campaign
    /// </summary>
    public sealed class BloodCampaign : BaseMod
    {
        /// <summary>
        /// Blood Addon enum
        /// </summary>
        public required BloodAddonEnum AddonEnum { get; set; }

        /// <summary>
        /// Startup .ini file
        /// </summary>
        public required override string StartupFile { get; init; }
    }
}
