using Common.Enums.Addons;

namespace Mods.Mods
{
    /// <summary>
    /// Duke Nukem 3D campaign
    /// </summary>
    public sealed class DukeCampaign : BaseMod
    {
        /// <summary>
        /// Duke Addon enum
        /// </summary>
        private DukeAddonEnum _addonEnum;
        public required DukeAddonEnum AddonEnum
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
