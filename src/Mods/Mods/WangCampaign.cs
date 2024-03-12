using Common.Enums.Addons;

namespace Mods.Mods
{
    /// <summary>
    /// Shadow Warrior campaign
    /// </summary>
    public sealed class WangCampaign : BaseMod
    {
        /// <summary>
        /// Wang Addon enum
        /// </summary>
        private WangAddonEnum _addonEnum;
        public required WangAddonEnum AddonEnum
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
