using Common;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Mods.Addons;
using System.Reflection;

namespace Games.Games;

public sealed class TekWarGame : BaseGame
{
    /// <inheritdoc/>
    public override GameEnum GameEnum => GameEnum.TekWar;

    /// <inheritdoc/>
    public override string FullName => "TekWar";

    /// <inheritdoc/>
    public override string ShortName => FullName;

    /// <inheritdoc/>
    public override List<string> RequiredFiles
    {
        get
        {
            List<string> result = ["SONGS", "SOUNDS"];

            for (var i = 0; i < 16; i++)
            {
                result.Add($"TILES{i:000}.ART");
            }

            return result;
        }
    }


    /// <inheritdoc/>
    public override Dictionary<AddonVersion, IAddon> GetOriginalCampaigns()
    {
        Dictionary<AddonVersion, IAddon> campaigns = new(1);

        if (IsBaseGameInstalled)
        {
            var namId = nameof(GameEnum.TekWar).ToLower();
            campaigns.Add(new(namId), new DukeCampaign()
            {
                Id = namId,
                Type = AddonTypeEnum.Official,
                Title = "TekWar",
                GridImage = ImageHelper.FileNameToStream("TekWar.tekwar.jpg", Assembly.GetExecutingAssembly()),
                Author = "Capstone Software",
                Description = """
                    You're an ex-cop who was sentenced to cryo sleep. When you awake you are recruited by the Cosmos Detective Agency as a hitman. Why? Cause there's a dangerous new drug on the streets of New LA: Tek!
                    
                    Take out the seven Tek Lords and their minions in 7 missions, but spare those innocent civilians.
                    """,
                Version = null,
                SupportedGame = new(GameEnum.TekWar),
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
