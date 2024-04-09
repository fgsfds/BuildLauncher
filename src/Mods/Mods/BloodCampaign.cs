using Common.Enums.Addons;

namespace Mods.Mods
{
    /// <summary>
    /// Blood campaign
    /// </summary>
    public sealed class BloodCampaign : Addon
    {
        /// <summary>
        /// Blood Addon enum
        /// </summary>
        public required BloodAddonEnum AddonEnum { get; init; }

        /// <summary>
        /// Startup .ini file
        /// </summary>
        public required string INI { get; init; }

        public required string RFF { get; init; }

        public required string SND { get; init; }
    }
}
