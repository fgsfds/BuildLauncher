using Addons.Addons;
using Core.All;
using Core.All.Enums;
using Core.All.Enums.Addons;
using Core.All.Enums.Versions;

namespace Tests.Unit.Helpers;

internal sealed class AutoloadModsTestSetups
{
    private readonly string _addon;
    private readonly FeatureEnum _feature;
    private readonly GameInfo _game;
    private readonly FeatureEnum _unsupportedFeature;

    public AutoloadModsTestSetups(GameEnum gameEnum)
    {
        _game = gameEnum switch
        {
            GameEnum.Duke3D => new(GameEnum.Duke3D, DukeVersionEnum.Duke3D_Atomic),
            GameEnum.Duke64 => new(GameEnum.Duke64),
            GameEnum.Blood => new(GameEnum.Blood),
            GameEnum.Wang => new(GameEnum.Wang),
            GameEnum.Slave => new(GameEnum.Slave),
            GameEnum.Redneck => new(GameEnum.Redneck),
            GameEnum.RidesAgain => new(GameEnum.RidesAgain),
            GameEnum.Fury => new(GameEnum.Fury),
            GameEnum.NAM => new(GameEnum.NAM),
            GameEnum.WW2GI => new(GameEnum.WW2GI),
            _ => throw new NotSupportedException()
        };

        _addon = gameEnum switch
        {
            GameEnum.Duke3D => nameof(DukeAddonEnum.DukeVaca).ToLower(),
            GameEnum.Blood => nameof(BloodAddonEnum.BloodCP).ToLower(),
            GameEnum.Wang => nameof(WangAddonEnum.TwinDragon).ToLower(),
            GameEnum.Redneck => nameof(RedneckAddonEnum.Route66).ToLower(),
            GameEnum.Fury => "",
            GameEnum.Duke64 => "",
            GameEnum.Slave => "",
            GameEnum.NAM => "",
            GameEnum.WW2GI => nameof(WW2GIAddonEnum.Platoon).ToLower(),
            _ => throw new NotSupportedException()
        };

        _feature = gameEnum switch
        {
            GameEnum.Duke3D => FeatureEnum.EDuke32_CON,
            GameEnum.Blood => FeatureEnum.Modern_Types,
            GameEnum.Wang => FeatureEnum.Models,
            GameEnum.Duke64 => FeatureEnum.Models,
            GameEnum.Slave => FeatureEnum.Models,
            GameEnum.Redneck => FeatureEnum.Models,
            GameEnum.RidesAgain => FeatureEnum.Models,
            GameEnum.Fury => FeatureEnum.EDuke32_CON,
            GameEnum.NAM => FeatureEnum.Models,
            GameEnum.WW2GI => FeatureEnum.Models,
            _ => throw new NotSupportedException()
        };

        _unsupportedFeature = gameEnum switch
        {
            GameEnum.Duke3D => FeatureEnum.Modern_Types,
            GameEnum.Blood => FeatureEnum.EDuke32_CON,
            GameEnum.Wang => FeatureEnum.Modern_Types,
            GameEnum.Duke64 => FeatureEnum.Modern_Types,
            GameEnum.Slave => FeatureEnum.Modern_Types,
            GameEnum.Redneck => FeatureEnum.Modern_Types,
            GameEnum.RidesAgain => FeatureEnum.Modern_Types,
            GameEnum.Fury => FeatureEnum.Modern_Types,
            GameEnum.NAM => FeatureEnum.Modern_Types,
            GameEnum.WW2GI => FeatureEnum.Modern_Types,
            _ => throw new NotSupportedException()
        };
    }

    private AutoloadMod EnabledModWithCons => new()
    {
        AddonId = new("enabledMod", "1.5"),
        Type = AddonTypeEnum.Mod,
        Title = "enabledMod",
        GridImageHash = null,
        Author = null,
        ReleaseDate = null,
        Description = null,
        SupportedGame = _game,
        IncompatibleAddons = null,
        RequiredFeatures = null,
        FileInfo = new(Path.Combine("D:", "Mods", "enabled_mod.zip"), "addon.json"),
        DependentAddons = null,
        AdditionalCons = ["ENABLED1.CON", "ENABLED2.CON"],
        MainDef = null,
        AdditionalDefs = ["ENABLED1.DEF", "ENABLED2.DEF"],
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        Executables = null,
        Options = null
    };

    private AutoloadMod EnabledMod => new()
    {
        AddonId = new("enabledMod", "1.5"),
        Type = AddonTypeEnum.Mod,
        Title = "enabledMod",
        GridImageHash = null,
        Author = null,
        ReleaseDate = null,
        Description = null,
        SupportedGame = _game,
        IncompatibleAddons = null,
        RequiredFeatures = null,
        FileInfo = new(Path.Combine("D:", "Mods", "enabled_mod.zip"), "addon.json"),
        DependentAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = ["ENABLED1.DEF", "ENABLED2.DEF"],
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        Executables = null,
        Options = null
    };

    private AutoloadMod ModThatRequiresOfficialAddon => new()
    {
        AddonId = new("modThatRequiresOfficialAddon", "1.5"),
        Type = AddonTypeEnum.Mod,
        Title = "modThatRequiredOfficialAddon",
        GridImageHash = null,
        Author = null,
        ReleaseDate = null,
        Description = null,
        SupportedGame = _game,
        IncompatibleAddons = null,
        RequiredFeatures = null,
        FileInfo = new(Path.Combine("D:", "Mods", "mod_requires_addon.zip"), "addon.json"),
        DependentAddons = new Dictionary<string, string?>
        {
            {
                _addon, null
            }
        },
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        Executables = null,
        Options = null
    };

    private AutoloadMod ModThatIncompatibleWithAddon => new()
    {
        AddonId = new("modThatIncompatibleWithOfficialAddon", "1.5"),
        Type = AddonTypeEnum.Mod,
        Title = "modThatIncompatibleWithOfficialAddon",
        GridImageHash = null,
        Author = null,
        ReleaseDate = null,
        Description = null,
        SupportedGame = _game,
        IncompatibleAddons = new Dictionary<string, string?>
        {
            {
                _addon, null
            }
        },
        DependentAddons = null,
        RequiredFeatures = null,
        FileInfo = new(Path.Combine("D:", "Mods", "mod_incompatible_with_addon.zip"), "addon.json"),
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        Executables = null,
        Options = null
    };

    private AutoloadMod IncompatibleWithEnabledMod => new()
    {
        AddonId = new("incompatibleMod", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "incompatibleMod",
        GridImageHash = null,
        Author = null,
        ReleaseDate = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = null,
        FileInfo = new(Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"), "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = null,
        IncompatibleAddons = new Dictionary<string, string?>
        {
            {
                "EnAbLeDmOd", null
            }
        },
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        Executables = null,
        Options = null
    };

    private AutoloadMod IncompatibleModWithCompatibleVersion => new()
    {
        AddonId = new("incompatibleModWithCompatibleVersion", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "incompatibleModWithCompatibleVersion",
        GridImageHash = null,
        Author = null,
        ReleaseDate = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = null,
        FileInfo = new(Path.Combine("D:", "Mods", "incompatible_mod_with_compatible_version.zip"), "addon.json"),
        DependentAddons = new Dictionary<string, string?>
        {
            {
                "enabledMod", null
            }
        },
        IncompatibleAddons = new Dictionary<string, string?>
        {
            {
                "enabledMod", "<=1.0"
            }
        },
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        Executables = null,
        Options = null
    };

    private AutoloadMod IncompatibleModWithIncompatibleVersion => new()
    {
        AddonId = new("incompatibleModWithIncompatibleVersion", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "incompatibleModWithIncompatibleVersion",
        GridImageHash = null,
        Author = null,
        ReleaseDate = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = null,
        FileInfo = new(Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"), "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = new Dictionary<string, string?>
        {
            {
                "enabledMod", null
            }
        },
        IncompatibleAddons = new Dictionary<string, string?>
        {
            {
                "enabledMod", ">1.1"
            }
        },
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        Executables = null,
        Options = null
    };

    private AutoloadMod ModThatDependsOnEnabled => new()
    {
        AddonId = new("dependentMod", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "dependentMod",
        GridImageHash = null,
        Author = null,
        ReleaseDate = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = null,
        FileInfo = new(Path.Combine("D:", "Mods", "dependent_mod.zip"), "addon.json"),
        DependentAddons = new Dictionary<string, string?>
        {
            {
                "enabledMod", null
            }
        },
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        Executables = null,
        Options = null
    };


    private AutoloadMod MultipleDependenciesMod => new()
    {
        AddonId = new("multipleDependenciesMod", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "multipleDependenciesMod",
        GridImageHash = null,
        Author = null,
        ReleaseDate = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = null,
        FileInfo = new(Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"), "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = new Dictionary<string, string?>
        {
            {
                "enabledMod", null
            },
            {
                "someMod", null
            }
        },
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        Executables = null,
        Options = null
    };

    private AutoloadMod DependentModWithIncompatibleVersion => new()
    {
        AddonId = new("dependentModWithIncompatibleVersion", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "dependentModWithIncompatibleVersion",
        GridImageHash = null,
        Author = null,
        ReleaseDate = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = null,
        FileInfo = new(Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"), "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = new Dictionary<string, string?>
        {
            {
                "enabledMod", "<=1.0"
            }
        },
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        Executables = null,
        Options = null
    };

    private AutoloadMod DependentModWithCompatibleVersion => new()
    {
        AddonId = new("dependentModWithCompatibleVersion", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "dependentModWithCompatibleVersion",
        GridImageHash = null,
        Author = null,
        ReleaseDate = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = null,
        FileInfo = new(Path.Combine("D:", "Mods", "dependent_mod_with_compatible_version.zip"), "addon.json"),
        DependentAddons = new Dictionary<string, string?>
        {
            {
                "enabledMod", ">1.1"
            }
        },
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        Executables = null,
        Options = null
    };

    private AutoloadMod DisabledMod => new()
    {
        AddonId = new("disabledMod", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "disabledMod",
        GridImageHash = null,
        Author = null,
        ReleaseDate = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = null,
        FileInfo = new(Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"), "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = null,
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = false,
        Executables = null,
        Options = null
    };

    private AutoloadMod ModThatRequiresFeature => new()
    {
        AddonId = new("featureMod", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "featureMod",
        GridImageHash = null,
        Author = null,
        ReleaseDate = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = [_feature],
        FileInfo = new(Path.Combine("D:", "Mods", "feature_mod.zip"), "addon.json"),
        DependentAddons = null,
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        Executables = null,
        Options = null
    };

    private AutoloadMod ModThatRequiresUnsupportedFeature => new()
    {
        AddonId = new("unsupportedFeatureMod", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "unsupportedFeatureMod",
        GridImageHash = null,
        Author = null,
        ReleaseDate = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = [_unsupportedFeature],
        FileInfo = new(Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"), "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = null,
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        Executables = null,
        Options = null
    };

    private static AutoloadMod ModForAnotherGame => new()
    {
        AddonId = new("somegame-mod", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "modForAnotherGame",
        GridImageHash = null,
        Author = null,
        ReleaseDate = null,
        Description = null,
        SupportedGame = new(GameEnum.TekWar),
        RequiredFeatures = null,
        FileInfo = new(Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"), "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = null,
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        Executables = null,
        Options = null
    };

    private List<AutoloadMod> _standardMods => [DisabledMod, MultipleDependenciesMod, IncompatibleWithEnabledMod, IncompatibleModWithIncompatibleVersion, IncompatibleModWithCompatibleVersion, ModThatDependsOnEnabled, DependentModWithCompatibleVersion, DependentModWithIncompatibleVersion, ModForAnotherGame, ModThatRequiresFeature, ModThatRequiresUnsupportedFeature];

    private List<AutoloadMod> _addonMods => [ModThatRequiresOfficialAddon, ModThatIncompatibleWithAddon];


    public List<AutoloadMod> Enabled => [EnabledMod];
    public List<AutoloadMod> StandardMods => [EnabledMod, .._addonMods, .._standardMods];

    public List<AutoloadMod> StandardModsWithCons => [EnabledModWithCons, .._addonMods, .._standardMods];

    public List<AutoloadMod> MinimalMods => [EnabledMod, IncompatibleWithEnabledMod];

    public List<AutoloadMod> AddonMods => [EnabledMod, .._addonMods];

    public List<AutoloadMod> AddonModsWithCons => [EnabledModWithCons, .._addonMods];
}
