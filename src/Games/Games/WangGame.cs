using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Mods.Mods;
using Mods.Providers;

namespace Games.Games
{
    public sealed class WangGame(InstalledModsProvider modsProvider) : BaseGame(modsProvider)
    {
        /// <inheritdoc/>
        public override GameEnum GameEnum => GameEnum.Wang;

        /// <inheritdoc/>
        public override string FullName => "Shadow Warrior";

        /// <inheritdoc/>
        public override string ShortName => "Wang";

        /// <inheritdoc/>
        public override string MainFile => "SW.GRP";

        /// <inheritdoc/>
        public override string DefFile => "sw.def";

        /// <inheritdoc/>
        public override List<string> RequiredFiles => [MainFile];

        /// <summary>
        /// Is Duke it Out in DC installed
        /// </summary>
        public bool IsWantonInstalled => IsInstalled("WT.GRP");

        /// <summary>
        /// Is Nuclear Winter installed
        /// </summary>
        public bool IsTwinDragonInstalled => IsInstalled("TD.GRP");


        /// <summary>
        /// Get list of original campaigns
        /// </summary>
        protected override List<IMod> GetOriginalCampaigns()
        {
            List<IMod> campaigns = new(3);

            if (IsBaseGameInstalled)
            {
                campaigns.Add(new WangCampaign()
                {
                    Guid = new(Consts.WangGuid),
                    ModType = ModTypeEnum.Campaign,
                    DisplayName = "Shadow Warrior",
                    Image = ImageHelper.FileNameToStream("Wang.wang.png"),
                    AddonEnum = WangAddonEnum.Wang,
                    Version = null,
                    SupportedPorts = null,
                    Description = null,
                    Url = null,
                    Author = null,
                    IsOfficial = true,
                    PathToFile = null,
                    StartupFile = null
                });

                if (IsWantonInstalled)
                {
                    campaigns.Add(new WangCampaign()
                    {
                        Guid = new(Consts.WantonGuid),
                        ModType = ModTypeEnum.Campaign,
                        DisplayName = "Wanton Destruction",
                        Image = ImageHelper.FileNameToStream("Wang.wanton.png"),
                        AddonEnum = WangAddonEnum.Wanton,
                        Version = null,
                        SupportedPorts = null,
                        Description = null,
                        Url = null,
                        Author = null,
                        IsOfficial = true,
                        PathToFile = null,
                        StartupFile = null
                    });
                }

                if (IsTwinDragonInstalled)
                {
                    campaigns.Add(new WangCampaign()
                    {
                        Guid = new(Consts.TwinDragonGuid),
                        ModType = ModTypeEnum.Campaign,
                        DisplayName = "Twin Dragon",
                        Image = ImageHelper.FileNameToStream("Wang.twin.png"),
                        AddonEnum = WangAddonEnum.TwinDragon,
                        Version = null,
                        SupportedPorts = null,
                        Description = null,
                        Url = null,
                        Author = null,
                        IsOfficial = true,
                        PathToFile = null,
                        StartupFile = null
                    });
                }
            }

            return campaigns;
        }
    }
}
