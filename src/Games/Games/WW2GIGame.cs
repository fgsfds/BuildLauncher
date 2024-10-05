using Common;
using Common.Enums;
using Common.Enums.Addons;
using Common.Helpers;
using Common.Interfaces;
using Mods.Addons;
using System.Reflection;

namespace Games.Games;

public sealed class WW2GIGame : BaseGame
{
    /// <inheritdoc/>
    public override GameEnum GameEnum => GameEnum.WW2GI;

    /// <inheritdoc/>
    public override string FullName => "World War II GI";

    /// <inheritdoc/>
    public override string ShortName => "WW2GI";

    /// <inheritdoc/>
    public override List<string> RequiredFiles => ["WW2GI.GRP"];

    /// <inheritdoc/>
    private List<string> PlatoonFiles => ["PLATOONL.DAT", "PLATOONL.DEF"];

    public bool IsPlatoonInstalled => IsInstalled(PlatoonFiles);


    /// <inheritdoc/>
    public override Dictionary<AddonVersion, IAddon> GetOriginalCampaigns()
    {
        Dictionary<AddonVersion, IAddon> campaigns = new(1);

        if (IsBaseGameInstalled)
        {
            var ww2id = nameof(GameEnum.WW2GI).ToLower();
            campaigns.Add(new(ww2id), new DukeCampaign()
            {
                Id = ww2id,
                Type = AddonTypeEnum.Official,
                Title = "World War II GI",
                GridImage = ImageHelper.FileNameToStream("WW2GI.ww2gi.jpg", Assembly.GetExecutingAssembly()),
                Author = "TNT Team",
                Description = """
                    **WWII GI** is the invasion of Normandy. The paranoia. The fear. the intensity that was D-Day. You will experience it first hand.

                    You're in the 101st Airborne, part of the first wave of allied forces to touch down in a no-man's land of twisted shrapnel, dead bodies and heavily armed Nazi-infested machine-gun bunkers. Now you must fight your way through hostile beaches, abandoned country roads with tall, sniper infested hedgerows, the narrow streets of devastated villages and more.

                    This is D-Day!
                    """,
                Version = null,
                SupportedGame = new(GameEnum.WW2GI),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainCon = null,
                AdditionalCons = null,
                RTS = null,
                MainDef = null,
                AdditionalDefs = null,
                StartMap = null,
                PreviewImage = null,
                IsFolder = false,
                Executables = null
            });
        }

        if (IsPlatoonInstalled)
        {
            var platoon = nameof(WW2GIAddonEnum.Platoon).ToLower();
            campaigns.Add(new(platoon), new DukeCampaign()
            {
                Id = platoon,
                Type = AddonTypeEnum.Official,
                Title = "Platoon Leader",
                GridImage = ImageHelper.FileNameToStream("WW2GI.platoon.jpg", Assembly.GetExecutingAssembly()),
                Author = "TNT Team",
                Description = """
                    **Platoon Leader** is an add-on for GT Interactive game WWII GI.

                    This add-on features three single-player-only levels: one WWII Pacific and two Vietnam War scenarios. Includes many new effects and features not seen in the game WWII GI.
                    """,
                Version = null,
                SupportedGame = new(GameEnum.WW2GI),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainCon = null,
                AdditionalCons = null,
                RTS = null,
                MainDef = null,
                AdditionalDefs = null,
                StartMap = null,
                PreviewImage = null,
                IsFolder = false,
                Executables = null
            });
        }

        return campaigns;
    }
}
