using System.Collections.Immutable;
using Addons.Addons;
using Addons.Helpers;
using Core.All;
using Core.All.Enums;
using Core.All.Enums.Versions;
using Core.Client.Helpers;

namespace Tests.Unit;

/// <summary>
///     Tests for the <see cref="AutoloadModsValidator" /> class.
/// </summary>
public sealed class AutoloadModsValidatorTests
{
    private static readonly GameInfo DukeGame = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic);
    private static readonly GameInfo DukeGameWT = new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_WT);
    private static readonly GameInfo BloodGame = new(GameEnum.Blood);
    private static readonly GameInfo RedneckGame = new(GameEnum.Redneck);
    private static readonly GameInfo RidesAgainGame = new(GameEnum.RidesAgain);

    private static AutoloadMod CreateMod(
        string id,
        string? version,
        GameInfo game,
        bool enabled = true,
        IReadOnlyDictionary<string, string?>? dependentAddons = null,
        IReadOnlyDictionary<string, string?>? incompatibleAddons = null,
        ImmutableArray<FeatureEnum>? requiredFeatures = null)
    {
        return new AutoloadMod
        {
            AddonId = new(id, version),
            Type = AddonTypeEnum.Mod,
            Title = id,
            SupportedGame = game,
            FileInfo = new AddonFilePathWrapper("D:\\Mods", $"{id}.zip"),
            DependentAddons = dependentAddons,
            IncompatibleAddons = incompatibleAddons,
            RequiredFeatures = requiredFeatures,
            IsEnabled = enabled,
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null
        };
    }

    /// <summary>
    ///     Creates a test campaign <see cref="AutoloadMod" /> with the specified parameters.
    /// </summary>
    private static AutoloadMod CreateCampaign(
        string id,
        string? version,
        GameInfo game,
        IReadOnlyDictionary<string, string?>? incompatibleAddons = null,
        IReadOnlyDictionary<string, string?>? dependentAddons = null)
    {
        return new AutoloadMod
        {
            AddonId = new(id, version),
            Type = AddonTypeEnum.TC,
            Title = id,
            SupportedGame = game,
            FileInfo = new AddonFilePathWrapper("D:\\Campaigns", $"{id}.zip"),
            DependentAddons = dependentAddons,
            IncompatibleAddons = incompatibleAddons,
            RequiredFeatures = null,
            IsEnabled = true,
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            AdditionalCons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            PreviewImageHash = null,
            Executables = null,
            Options = null
        };
    }

    /// <summary>
    ///     Tests that a disabled mod fails validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_DisabledMod_ReturnsFalse()
    {
        var mod = CreateMod("someMod", "1.0", DukeGame, enabled: false);
        var campaign = CreateCampaign("campaign", "1.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that a mod for a different game fails validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_DifferentGame_ReturnsFalse()
    {
        var mod = CreateMod("dukeMod", "1.0", DukeGame);
        var campaign = CreateCampaign("bloodCampaign", "1.0", BloodGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that a mod for the same game passes validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_SameGame_ReturnTrue()
    {
        var mod = CreateMod("dukeMod", "1.0", DukeGame);
        var campaign = CreateCampaign("dukeCampaign", "1.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a Redneck mod with a Rides Again campaign passes validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_RedneckModWithRidesAgainCampaign_ReturnsTrue()
    {
        var mod = CreateMod("redneckMod", "1.0", RedneckGame);
        var campaign = CreateCampaign("ridesAgainCampaign", "1.0", RidesAgainGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a Rides Again mod with a Redneck campaign fails validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_RidesAgainModWithRedneckCampaign_ReturnsFalse()
    {
        var mod = CreateMod("ridesAgainMod", "1.0", RidesAgainGame);
        var campaign = CreateCampaign("redneckCampaign", "1.0", RedneckGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that a mod with a different game version fails validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_DifferentGameVersion_ReturnsFalse()
    {
        var mod = CreateMod("someMod", "1.0", DukeGameWT);
        var campaign = CreateCampaign("campaign", "1.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that a mod with a null game version passes validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_NullGameVersion_ReturnTrue()
    {
        var mod = CreateMod("someMod", "1.0", new GameInfo(GameEnum.Duke3D));
        var campaign = CreateCampaign("campaign", "1.0", new GameInfo(GameEnum.Duke3D));

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a mod requiring unsupported features fails validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_RequiredFeaturesNotSupported_ReturnsFalse()
    {
        var mod = CreateMod("featureMod", "1.0", DukeGame, requiredFeatures: [FeatureEnum.Models, FeatureEnum.Hightile]);
        var campaign = CreateCampaign("campaign", "1.0", DukeGame);

        var features = new List<FeatureEnum>
        {
            FeatureEnum.EDuke32_CON
        };

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], features);

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that a mod with supported features passes validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_RequiredFeaturesSupported_ReturnTrue()
    {
        var mod = CreateMod("featureMod", "1.0", DukeGame, requiredFeatures: [FeatureEnum.Models, FeatureEnum.Hightile]);
        var campaign = CreateCampaign("campaign", "1.0", DukeGame);

        var features = new List<FeatureEnum>
        {
            FeatureEnum.Models,
            FeatureEnum.Hightile,
            FeatureEnum.EDuke32_CON
        };

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], features);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a mod with null required features passes validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_NullRequiredFeatures_ReturnTrue()
    {
        var mod = CreateMod("simpleMod", "1.0", DukeGame);
        var campaign = CreateCampaign("campaign", "1.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a mod fails validation when the campaign has a wildcard incompatibility.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_CampaignIncompatibleWildcard_ReturnsFalse()
    {
        var mod = CreateMod("anyMod", "1.0", DukeGame);

        var campaign = CreateCampaign("campaign", "1.0", DukeGame, incompatibleAddons: new Dictionary<string, string?>
        {
            {
                "*", null
            }
        });

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that a mod fails validation when the campaign is incompatible with the mod id.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_CampaignIncompatibleWithModId_ReturnsFalse()
    {
        var mod = CreateMod("specificMod", "1.0", DukeGame);

        var campaign = CreateCampaign("campaign", "1.0", DukeGame, incompatibleAddons: new Dictionary<string, string?>
        {
            {
                "specificMod", null
            }
        });

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that a mod passes validation when the campaign is incompatible with a different mod id.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_CampaignIncompatibleWithDifferentModId_ReturnTrue()
    {
        var mod = CreateMod("myMod", "1.0", DukeGame);

        var campaign = CreateCampaign("campaign", "1.0", DukeGame, incompatibleAddons: new Dictionary<string, string?>
        {
            {
                "otherMod", null
            }
        });

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a mod fails validation when the campaign is incompatible with the matching version.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_CampaignIncompatibleWithMatchingVersion_ReturnsFalse()
    {
        var mod = CreateMod("versionedMod", "1.5", DukeGame);

        var campaign = CreateCampaign("campaign", "1.0", DukeGame, incompatibleAddons: new Dictionary<string, string?>
        {
            {
                "versionedMod", "1.5"
            }
        });

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that a mod passes validation when the campaign is incompatible with a non-matching version.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_CampaignIncompatibleWithNonMatchingVersion_ReturnTrue()
    {
        var mod = CreateMod("versionedMod", "1.5", DukeGame);

        var campaign = CreateCampaign("campaign", "1.0", DukeGame, incompatibleAddons: new Dictionary<string, string?>
        {
            {
                "versionedMod", "1.0"
            }
        });

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a mod with a dependency on the campaign itself passes validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_DependencyOnCampaignItself_ReturnTrue()
    {
        var mod = CreateMod("dependentMod", "1.0", DukeGame, dependentAddons: new Dictionary<string, string?>
        {
            {
                "campaign", null
            }
        });

        var campaign = CreateCampaign("campaign", "1.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a mod with a dependency matching the campaign's dependency passes validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_DependencyOnCampaignDependentAddon_ReturnTrue()
    {
        var mod = CreateMod("dependentMod", "1.0", DukeGame, dependentAddons: new Dictionary<string, string?>
        {
            {
                "someAddon", null
            }
        });

        var campaign = CreateCampaign("campaign", "1.0", DukeGame, dependentAddons: new Dictionary<string, string?>
        {
            {
                "someAddon", "1.0"
            }
        });

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a mod with a dependency on another enabled mod passes validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_DependencyOnOtherMod_ReturnTrue()
    {
        var mod = CreateMod("dependentMod", "1.0", DukeGame, dependentAddons: new Dictionary<string, string?>
        {
            {
                "helperMod", null
            }
        });

        var campaign = CreateCampaign("campaign", "1.0", DukeGame);
        var helperMod = CreateMod("helperMod", "2.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [helperMod], []);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a mod with an unsatisfied dependency fails validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_DependencyUnsatisfied_ReturnsFalse()
    {
        var mod = CreateMod("needyMod", "1.0", DukeGame, dependentAddons: new Dictionary<string, string?>
        {
            {
                "missingMod", null
            }
        });

        var campaign = CreateCampaign("campaign", "1.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that a mod with a satisfied version constraint dependency passes validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_DependencyVersionConstraintSatisfied_ReturnTrue()
    {
        var mod = CreateMod("dependentMod", "1.0", DukeGame, dependentAddons: new Dictionary<string, string?>
        {
            {
                "helperMod", ">1.0"
            }
        });

        var campaign = CreateCampaign("campaign", "1.0", DukeGame);
        var helperMod = CreateMod("helperMod", "2.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [helperMod], []);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a mod with an unsatisfied version constraint dependency fails validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_DependencyVersionConstraintUnsatisfied_ReturnsFalse()
    {
        var mod = CreateMod("dependentMod", "1.0", DukeGame, dependentAddons: new Dictionary<string, string?>
        {
            {
                "helperMod", "<=1.0"
            }
        });

        var campaign = CreateCampaign("campaign", "1.0", DukeGame);
        var helperMod = CreateMod("helperMod", "2.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [helperMod], []);

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that a mod with null dependent addons passes validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_NullDependentAddons_ReturnTrue()
    {
        var mod = CreateMod("independentMod", "1.0", DukeGame);
        var campaign = CreateCampaign("campaign", "1.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a mod incompatible with the campaign fails validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_IncompatibleWithCampaign_ReturnsFalse()
    {
        var mod = CreateMod("conflictingMod", "1.0", DukeGame, incompatibleAddons: new Dictionary<string, string?>
        {
            {
                "campaign", null
            }
        });

        var campaign = CreateCampaign("campaign", "1.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that a mod incompatible with an enabled mod fails validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_IncompatibleWithEnabledMod_ReturnsFalse()
    {
        var mod = CreateMod("conflictingMod", "1.0", DukeGame, incompatibleAddons: new Dictionary<string, string?>
        {
            {
                "otherMod", null
            }
        });

        var campaign = CreateCampaign("campaign", "1.0", DukeGame);
        var otherMod = CreateMod("otherMod", "1.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [otherMod], []);

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that a mod incompatible only with a disabled mod passes validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_IncompatibleWithDisabledMod_ReturnTrue()
    {
        var mod = CreateMod("conflictingMod", "1.0", DukeGame, incompatibleAddons: new Dictionary<string, string?>
        {
            {
                "disabledMod", null
            }
        });

        var campaign = CreateCampaign("campaign", "1.0", DukeGame);
        var disabledMod = CreateMod("disabledMod", "1.0", DukeGame, enabled: false);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [disabledMod], []);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a mod incompatible with a non-matching addon passes validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_IncompatibleWithNonMatchingAddon_ReturnTrue()
    {
        var mod = CreateMod("conflictingMod", "1.0", DukeGame, incompatibleAddons: new Dictionary<string, string?>
        {
            {
                "unrelatedMod", null
            }
        });

        var campaign = CreateCampaign("campaign", "1.0", DukeGame);
        var otherMod = CreateMod("otherMod", "1.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [otherMod], []);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a mod using an operator prefix for campaign incompatibility fails validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_IncompatibleWithCampaignUsingOperatorPrefix_ReturnsFalse()
    {
        var mod = CreateMod("conflictingMod", "1.0", DukeGame, incompatibleAddons: new Dictionary<string, string?>
        {
            {
                "campaign", "==1.0"
            }
        });

        var campaign = CreateCampaign("campaign", "1.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that a mod with a non-matching incompatible version passes validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_IncompatibleVersionDoesNotMatch_ReturnTrue()
    {
        var mod = CreateMod("conflictingMod", "1.0", DukeGame, incompatibleAddons: new Dictionary<string, string?>
        {
            {
                "campaign", "==2.0"
            }
        });

        var campaign = CreateCampaign("campaign", "1.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a mod with null incompatible addons passes validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_NullIncompatibleAddons_ReturnTrue()
    {
        var mod = CreateMod("friendlyMod", "1.0", DukeGame);
        var campaign = CreateCampaign("campaign", "1.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a mod passes validation when all checks pass.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_AllChecksPass_ReturnsTrue()
    {
        var mod = CreateMod("validMod", "1.0", DukeGame);
        var campaign = CreateCampaign("campaign", "1.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [], []);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a mod with a dependency satisfied by a disabled mod passes validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_DependencyOnDisabledMod_ReturnTrue()
    {
        var mod = CreateMod("dependentMod", "1.0", DukeGame, dependentAddons: new Dictionary<string, string?>
        {
            {
                "disabledSatisfier", null
            }
        });

        var campaign = CreateCampaign("campaign", "1.0", DukeGame);
        var disabledMod = CreateMod("disabledSatisfier", "2.0", DukeGame, enabled: false);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [disabledMod], []);

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that a mod with partially unsatisfied dependencies fails validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_PartialDependenciesUnsatisfied_ReturnsFalse()
    {
        var mod = CreateMod("needyMod", "1.0", DukeGame, dependentAddons: new Dictionary<string, string?>
        {
            {
                "presentMod", null
            },
            {
                "missingMod", null
            }
        });

        var campaign = CreateCampaign("campaign", "1.0", DukeGame);
        var presentMod = CreateMod("presentMod", "1.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [presentMod], []);

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that a mod incompatible with an enabled mod by version fails validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_IncompatibleWithEnabledModByVersion_ReturnsFalse()
    {
        var mod = CreateMod("conflictingMod", "1.0", DukeGame, incompatibleAddons: new Dictionary<string, string?>
        {
            {
                "versionedMod", "==2.0"
            }
        });

        var campaign = CreateCampaign("campaign", "1.0", DukeGame);
        var versionedMod = CreateMod("versionedMod", "2.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [versionedMod], []);

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that a mod incompatible with a different version of an enabled mod passes validation.
    /// </summary>
    [Fact]
    public void ValidateAutoloadMod_IncompatibleWithEnabledModByDifferentVersion_ReturnTrue()
    {
        var mod = CreateMod("conflictingMod", "1.0", DukeGame, incompatibleAddons: new Dictionary<string, string?>
        {
            {
                "versionedMod", "==2.0"
            }
        });

        var campaign = CreateCampaign("campaign", "1.0", DukeGame);
        var versionedMod = CreateMod("versionedMod", "3.0", DukeGame);

        var result = AutoloadModsValidator.ValidateAutoloadMod(mod, campaign, [versionedMod], []);

        Assert.True(result);
    }
}
