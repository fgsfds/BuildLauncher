using Common;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Mods.Addons;
using System.Reflection;

namespace Games.Games;

public sealed class FuryGame : BaseGame
{
    /// <inheritdoc/>
    public override GameEnum GameEnum => GameEnum.Fury;

    /// <inheritdoc/>
    public override string FullName => "Ion Fury";

    /// <inheritdoc/>
    public override string ShortName => "Fury";

    /// <inheritdoc/>
    public override List<string> RequiredFiles => ["fury.grp"];


    /// <inheritdoc/>
    public override Dictionary<AddonVersion, IAddon> GetOriginalCampaigns()
    {
        Dictionary<AddonVersion, IAddon> campaigns = new(1);

        if (IsBaseGameInstalled)
        {
            var furyId = nameof(GameEnum.Fury).ToLower();
            campaigns.Add(new(furyId), new FuryCampaign()
            {
                Id = furyId,
                Type = AddonTypeEnum.Official,
                Title = IsAftershock() ? "Ion Fury: Aftershock" : "Ion Fury",
                GridImage = IsAftershock() ? ImageHelper.FileNameToStream("Fury.aftershock.jpg", Assembly.GetExecutingAssembly()) : ImageHelper.FileNameToStream("Fury.fury.jpg", Assembly.GetExecutingAssembly()),
                Author = "Voidpoint, LLC",
                Description = """
                **Ion Fury** (originally titled Ion Maiden) is a 2019 cyberpunk first-person shooter developed by **Voidpoint** and published by **3D Realms**.

                It is a prequel to the 2016 video game Bombshell. Ion Fury runs on a modified version of Ken Silverman's Build engine and is the first original commercial game to utilize the engine in 20 years, the previous being World War II GI.

                You assume the role of Shelly "Bombshell" Harrison, a bomb disposal expert aligned to the Global Defense Force. Dr. Jadus Heskel, a transhumanist cult leader, unleashes an army of cybernetically-enhanced soldiers on the futuristic dystopian city of Neo D.C., which Shelly is tasked with fighting through.
                """,
                Version = null,
                SupportedGame = new(GameEnum.Fury),
                RequiredFeatures = null,
                PathToFile = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                MainCon = null,
                AdditionalCons = null,
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

    /// <summary>
    /// Is Aftershock addon installed
    /// </summary>
    private bool IsAftershock()
    {
        if (GameInstallFolder is null)
        {
            return false;
        }

        try
        {
            var text = File.ReadAllText(Path.Combine(GameInstallFolder, "fury.grpinfo"));

            if (text.Contains("ashock.def"))
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
