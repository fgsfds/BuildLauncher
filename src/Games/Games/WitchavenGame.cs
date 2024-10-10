using Common;
using Common.Enums;
using Common.Helpers;
using Common.Interfaces;
using Mods.Addons;
using System.Reflection;

namespace Games.Games;

public sealed class WitchavenGame : BaseGame
{
    /// <inheritdoc/>
    public override GameEnum GameEnum => GameEnum.Witchaven;

    /// <inheritdoc/>
    public override string FullName => "Witchaven";

    /// <inheritdoc/>
    public override string ShortName => FullName;

    /// <inheritdoc/>
    public override List<string> RequiredFiles
    {
        get
        {
            List<string> result = ["JOESND", "SONGS"];

            for (var i = 0; i < 11; i++)
            {
                result.Add($"TILES{i:000}.ART");
            }

            for (var i = 1; i < 26; i++)
            {
                result.Add($"LEVEL{i}.MAP");
            }

            return result;
        }
    }
    
    /// <inheritdoc/>
    public List<string> Witchaven2RequiredFiles
    {
        get
        {
            List<string> result = ["JOESND", "W_SONGS"];

            for (var i = 0; i < 16; i++)
            {
                result.Add($"TILES{i:000}.ART");
            }

            for (var i = 1; i < 16; i++)
            {
                result.Add($"LEVEL{i}.MAP");
            }

            return result;
        }
    }

    public string? Witchaven2InstallPath { get; set; }

    public bool IsWitchaven2Installed => IsInstalled(Witchaven2RequiredFiles, Witchaven2InstallPath);


    /// <inheritdoc/>
    public override Dictionary<AddonVersion, IAddon> GetOriginalCampaigns()
    {
        Dictionary<AddonVersion, IAddon> campaigns = new(1);

        if (IsBaseGameInstalled)
        {
            var namId = nameof(GameEnum.Witchaven).ToLower();
            campaigns.Add(new(namId), new DukeCampaign()
            {
                Id = namId,
                Type = AddonTypeEnum.Official,
                Title = "Witchaven",
                GridImage = ImageHelper.FileNameToStream("Witchaven.wh1.jpg", Assembly.GetExecutingAssembly()),
                Author = "Capstone Software",
                Description = """
                    Descend into a dark and gruesome nightmare!
                    
                    You alone must face the evil within the volcanic pit of the Island of Char, toward the mystical lair of Witchaven. Confront witches that have cast a shadow of evil spells shrouding you in the never-ending darkness. Make use of your magic, might, and mind as you engage in bloody warfare with vile demons and monsters. Use medieval weapons to destroy these creatures of the night and cease the chaos.
                    
                    Dare to enter this 3D Hell... Dare to enter Witchaven!
                    """,
                Version = null,
                SupportedGame = new(GameEnum.Witchaven),
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
        
        if (IsWitchaven2Installed)
        {
            var namId = nameof(GameEnum.Witchaven2).ToLower();
            campaigns.Add(new(namId), new DukeCampaign()
            {
                Id = namId,
                Type = AddonTypeEnum.Official,
                Title = "Witchaven II",
                GridImage = ImageHelper.FileNameToStream("Witchaven.wh2.jpg", Assembly.GetExecutingAssembly()),
                Author = "Capstone Software",
                Description = """
                    The witches have been destroyed in their lair on the Island of Char!
                    
                    Returning to your homeland, you are greeted with newborn hope, pride, and great celebration. After the revelry, you awaken to a dawn filled with an eerie silence that looms in the still air. Your countrymen are gone!

                    The great witch, Circa-Argoth has taken them to avenge the death of her sister. You have only yourself and your foolish meddling to blame. But, you are not meant to die... yet!
                    
                    Alone in the land that you have fought so fiercely to protect, you must gather your strength and use your anger to fight for Blood Vengeance.
                    """,
                Version = null,
                SupportedGame = new(GameEnum.Witchaven2),
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
