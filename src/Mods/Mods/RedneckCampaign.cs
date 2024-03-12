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
        private RedneckAddonEnum _addonEnum;
        public required RedneckAddonEnum AddonEnum
        {
            get => _addonEnum;
            init
            {
                Addon = value.ToString();
                _addonEnum = value;
            }
        }
    }
}
