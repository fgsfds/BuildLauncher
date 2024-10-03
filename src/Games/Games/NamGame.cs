using Common;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Mods.Addons;
using System.Reflection;

namespace Games.Games;

public sealed class NamGame : BaseGame
{
    /// <inheritdoc/>
    public override GameEnum GameEnum => GameEnum.NAM;

    /// <inheritdoc/>
    public override string FullName => "NAM";

    /// <inheritdoc/>
    public override string ShortName => "NAM";

    /// <inheritdoc/>
    public override List<string> RequiredFiles => ["NAM.GRP"];


    /// <inheritdoc/>
    public override Dictionary<AddonVersion, IAddon> GetOriginalCampaigns()
    {
        Dictionary<AddonVersion, IAddon> campaigns = new(1);

        if (IsBaseGameInstalled)
        {
            var namId = nameof(GameEnum.NAM).ToLower();
            campaigns.Add(new(namId), new DukeCampaign()
            {
                Id = namId,
                Type = AddonTypeEnum.Official,
                Title = "NAM",
                GridImage = ImageHelper.FileNameToStream("NAM.nam.jpg", Assembly.GetExecutingAssembly()),
                Author = "TNT Team",
                Description = """
                    You are Alan 'The Bear' Westmoreland, Marine Corps sergeant. The trouble starts on a deadly Viet Cong raid. Here the jungle is your battleground.

                    Your mission, survive.

                    NAM captures all of intensity and paranoia of jungle warfare. Fire-fights, ambushes, booby-traps, snipers, air-strikes, anti-personnel mines AND MORE.

                    Feel the tropical heat and the fear of tunnel skirmishes, paddy killing fields, swamps and thick jungles.

                    NAM is the first game of its kind. NAM IS WAR!
                    """,
                Version = null,
                SupportedGame = new(GameEnum.NAM),
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
                PortExeOverride = null
            });
        }

        return campaigns;
    }
}
