using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Mods.Mods;
using Mods.Providers;

namespace Games.Games
{
    public sealed class WangGame(InstalledModsProviderFactory modsProvider, DownloadableModsProviderFactory downloadableModsProviderFactory) : BaseGame(modsProvider, downloadableModsProviderFactory)
    {
        /// <inheritdoc/>
        public override GameEnum GameEnum => GameEnum.Wang;

        /// <inheritdoc/>
        public override string FullName => "Shadow Warrior";

        /// <inheritdoc/>
        public override string ShortName => "Wang";

        /// <inheritdoc/>
        public override string DefFileName => "sw.def";

        /// <inheritdoc/>
        public override List<string> RequiredFiles => ["SW.GRP"];

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
        protected override Dictionary<string, IAddon> GetOriginalCampaigns()
        {
            Dictionary<string, IAddon> campaigns = new(3);

            if (IsBaseGameInstalled)
            {
                campaigns.Add(GameEnum.Wang.ToString(), new WangCampaign()
                {
                    Id = GameEnum.Wang.ToString(),
                    Type = ModTypeEnum.Official,
                    Title = "Shadow Warrior",
                    Image = ImageHelper.FileNameToStream("Wang.wang.jpg"),
                    Version = null,
                    SupportedPorts = null,
                    Author = "3D Realms",
                    Description = """
                        **Shadow Warrior** is a first-person shooter developed by **3D Realms** and released on May 13, 1997 by **GT Interactive**.
                        
                        The premise of Shadow Warrior is that the protagonist, Chinese-Japanese, Lo Wang, worked as a bodyguard for Zilla Enterprises, a conglomerate that had power over every major industry in Japan.
                        However, this led to corruption, and Master Zilla - the president - planned to conquer Japan using creatures from the "dark side".
                        In discovery of this, Lo Wang quit his job as a bodyguard. Master Zilla realized that not having a warrior as powerful as Lo Wang would be dangerous, and sent his creatures to battle Lo Wang.
                        """,
                    PathToFile = null,
                    SupportedGames = null,
                    SupportedGamesCrcs = null,
                    Dependencies = null,
                    Incompatibles = null,
                    MainDef = null,
                    AdditionalDefs = null,
                    StartMap = null,
                    RequiredAddonEnum = WangAddonEnum.Wang
                });

                if (IsWantonInstalled)
                {
                    campaigns.Add(WangAddonEnum.WangWD.ToString(), new WangCampaign()
                    {
                        Id = WangAddonEnum.WangWD.ToString(),
                        Type = ModTypeEnum.Official,
                        Title = "Wanton Destruction",
                        Image = ImageHelper.FileNameToStream("Wang.wanton.jpg"),
                        Author = "Sunstorm Interactive",
                        Description = """
                            **Wanton Destruction** is an official expansion that was created by **Sunstorm Interactive** and tested by **3D Realms**, but was not released by the distributor.
                            It was found and released for free on September 9, 2005.
                        
                            The add-on chronicles Lo Wang's adventures after the original game. He visits his relatives in USA, but is forced to fight off Zilla's forces again.
                            The game culminates with a battle against Master Zilla above the streets of Tokyo, which ends with Master Zilla's death.
                        
                            The game features 12 new levels, new artwork and a few new enemy replacements, such as human enemies; though they still act like their original counterparts.
                            """,
                        Version = null,
                        SupportedPorts = null,
                        PathToFile = null,
                        SupportedGames = null,
                        SupportedGamesCrcs = null,
                        Dependencies = null,
                        Incompatibles = null,
                        MainDef = null,
                        AdditionalDefs = null,
                        StartMap = null,
                        RequiredAddonEnum = WangAddonEnum.WangWD
                    });
                }

                if (IsTwinDragonInstalled)
                {
                    campaigns.Add(WangAddonEnum.WangTD.ToString(), new WangCampaign()
                    {
                        Id = WangAddonEnum.WangTD.ToString(),
                        Type = ModTypeEnum.Official,
                        Title = "Twin Dragon",
                        Image = ImageHelper.FileNameToStream("Wang.twin.jpg"),
                        Author = "Wylde Productions, Level Infinity",
                        Description = """
                            **Twin Dragon** is an official expansion to the Shadow Warrior that was released as a free download on July 4, 1998.
                            
                            The game reveals that Lo Wang has a twin brother, Hung Lo, with whom he was separated in early childhood. Hung Lo becomes a dark person whose goal is to destroy the world.
                            Similar to Master Zilla, he uses the creatures from the dark side, criminal underworld and Zilla's remnants to further his goals. Lo Wang has to journey through his dark minions,
                            reach his palace and defeat the evil Twin Dragon Hung Lo once and for all. When he reaches the palace, Lo Wang defeats his brother by shooting a Nuke at him, which kills Hung Lo in the process.
                            
                            The game features 13 new levels, new sounds, artwork and a new final boss, Hung Lo, who replaced Zilla.
                            """,
                        Version = null,
                        SupportedPorts = null,
                        PathToFile = null,
                        SupportedGames = null,
                        SupportedGamesCrcs = null,
                        Dependencies = null,
                        Incompatibles = null,
                        MainDef = null,
                        AdditionalDefs = null,
                        StartMap = null,
                        RequiredAddonEnum = WangAddonEnum.WangTD
                    });
                }
            }

            return campaigns;
        }
    }
}
