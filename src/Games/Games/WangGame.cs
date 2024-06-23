using Common;
using Common.Enums;
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
    /// Get list of original campaigns
    /// </summary>
    public override Dictionary<AddonVersion, IAddon> GetOriginalCampaigns()
    {
        Dictionary<AddonVersion, IAddon> campaigns = new(1);

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
        }

        return campaigns;
    }
}
