using Common.Enums.Addons;

namespace Mods.Mods
{
    /// <summary>
    /// Blood campaign
    /// </summary>
    public sealed class BloodCampaign : BaseAddon
    {
        /// <summary>
        /// Blood Addon enum
        /// </summary>
        private BloodAddonEnum _addonEnum;
        public required BloodAddonEnum AddonEnum
        {
            get => _addonEnum;
            init
            {
                Addon = value.ToString();
                _addonEnum = value;
            }
        }

        /// <summary>
        /// Startup .ini file
        /// </summary>
        public required override string StartupFile { get; init; }
    }
}
