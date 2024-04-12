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
        public required BloodAddonEnum RequiredAddonEnum { get; init; }

        /// <summary>
        /// Startup .ini file
        /// </summary>
        public required string? INI { get; init; }

        /// <summary>
        /// Main .rff file
        /// </summary>
        public required string? RFF { get; init; }

        /// <summary>
        /// Main .snd file
        /// </summary>
        public required string? SND { get; init; }
    }
}
