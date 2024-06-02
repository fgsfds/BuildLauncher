using Common;
using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Mods.Addons;
using System.Reflection;

namespace Games.Games;

public sealed class WangGame : BaseGame
{
    /// <inheritdoc/>
    public override GameEnum GameEnum => GameEnum.ShadowWarrior;

    /// <inheritdoc/>
    public override string FullName => "Shadow Warrior";

    /// <inheritdoc/>
    public override string ShortName => "Wang";

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
    public override Dictionary<AddonVersion, IAddon> GetOriginalCampaigns()
    {
        Dictionary<AddonVersion, IAddon> campaigns = new(3);

        if (IsBaseGameInstalled)
        {
            var wangId = nameof(GameEnum.ShadowWarrior).ToLower();
            campaigns.Add(new(wangId), new WangCampaign()
            {
                Id = wangId,
                Type = AddonTypeEnum.Official,
                Title = "Shadow Warrior",
                GridImage = ImageHelper.FileNameToStream("Wang.wang.jpg", Assembly.GetExecutingAssembly()),
                Version = null,
                Author = "3D Realms",
                Description = """
                    **Shadow Warrior** is a first-person shooter developed by **3D Realms** and released on May 13, 1997 by **GT Interactive**.
                    
                    The premise of Shadow Warrior is that the protagonist, Chinese-Japanese, Lo Wang, worked as a bodyguard for Zilla Enterprises, a conglomerate that had power over every major industry in Japan.
                    However, this led to corruption, and Master Zilla - the president - planned to conquer Japan using creatures from the "dark side".
                    In discovery of this, Lo Wang quit his job as a bodyguard. Master Zilla realized that not having a warrior as powerful as Lo Wang would be dangerous, and sent his creatures to battle Lo Wang.
                    """,
                PathToFile = null,
                SupportedGame = new(GameEnum.ShadowWarrior),
                RequiredFeatures = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainDef = null,
                AdditionalDefs = null,
                StartMap = null,
                PreviewImage = null,
            });

            if (IsWantonInstalled)
            {
                var wangWdId = nameof(WangAddonEnum.Wanton).ToLower();
                campaigns.Add(new(wangWdId), new WangCampaign()
                {
                    Id = wangWdId,
                    Type = AddonTypeEnum.Official,
                    Title = "Wanton Destruction",
                    GridImage = ImageHelper.FileNameToStream("Wang.wanton.jpg", Assembly.GetExecutingAssembly()),
                    Author = "Sunstorm Interactive",
                    Description = """
                        **Wanton Destruction** is an official expansion that was created by **Sunstorm Interactive** and tested by **3D Realms**, but was not released by the distributor.
                        It was found and released for free on September 9, 2005.
                    
                        The add-on chronicles Lo Wang's adventures after the original game. He visits his relatives in USA, but is forced to fight off Zilla's forces again.
                        The game culminates with a battle against Master Zilla above the streets of Tokyo, which ends with Master Zilla's death.
                    
                        The game features 12 new levels, new artwork and a few new enemy replacements, such as human enemies; though they still act like their original counterparts.
                        """,
                    Version = null,
                    SupportedGame = new(GameEnum.ShadowWarrior),
                    RequiredFeatures = null,
                    PathToFile = null,
                    DependentAddons = new() { { WangAddonEnum.Wanton.ToString(), null } },
                    IncompatibleAddons = null,
                    MainDef = null,
                    AdditionalDefs = null,
                    StartMap = null,
                    PreviewImage = null,
                });
            }

            if (IsTwinDragonInstalled)
            {
                var wangTdId = nameof(WangAddonEnum.TwinDragon).ToLower();
                campaigns.Add(new(wangTdId), new WangCampaign()
                {
                    Id = wangTdId,
                    Type = AddonTypeEnum.Official,
                    Title = "Twin Dragon",
                    GridImage = ImageHelper.FileNameToStream("Wang.twin.jpg", Assembly.GetExecutingAssembly()),
                    Author = "Wylde Productions, Level Infinity",
                    Description = """
                        **Twin Dragon** is an official expansion to the Shadow Warrior that was released as a free download on July 4, 1998.
                        
                        The game reveals that Lo Wang has a twin brother, Hung Lo, with whom he was separated in early childhood. Hung Lo becomes a dark person whose goal is to destroy the world.
                        Similar to Master Zilla, he uses the creatures from the dark side, criminal underworld and Zilla's remnants to further his goals. Lo Wang has to journey through his dark minions,
                        reach his palace and defeat the evil Twin Dragon Hung Lo once and for all. When he reaches the palace, Lo Wang defeats his brother by shooting a Nuke at him, which kills Hung Lo in the process.
                        
                        The game features 13 new levels, new sounds, artwork and a new final boss, Hung Lo, who replaced Zilla.
                        """,
                    Version = null,
                    SupportedGame = new(GameEnum.ShadowWarrior),
                    RequiredFeatures = null,
                    PathToFile = null,
                    DependentAddons = new() { { WangAddonEnum.TwinDragon.ToString(), null } },
                    IncompatibleAddons = null,
                    MainDef = null,
                    AdditionalDefs = null,
                    StartMap = null,
                    PreviewImage = null,
                });
            }
        }

        return campaigns;
    }
}
