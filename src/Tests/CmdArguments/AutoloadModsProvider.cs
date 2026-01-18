using Addons.Addons;
using Common.All;
using Common.All.Enums;
using Common.All.Enums.Addons;
using Common.All.Enums.Versions;

namespace Tests.CmdArguments;

internal sealed class AutoloadModsProvider
{
    private readonly GameStruct _game;
    private readonly string _addon;
    private readonly FeatureEnum _feature;

    public AutoloadModsProvider(GameEnum gameEnum)
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
            _ => throw new NotSupportedException()
        };
    }

    public AutoloadMod EnabledModWithCons => new()
    {
        AddonId = new("enabledMod", "1.5"),
        Type = AddonTypeEnum.Mod,
        Title = "enabledMod",
        GridImageHash = null,
        Author = null,
        Description = null,
        SupportedGame = _game,
        IncompatibleAddons = null,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "enabled_mod.zip"),
        DependentAddons = null,
        AdditionalCons = ["ENABLED1.CON", "ENABLED2.CON"],
        MainDef = null,
        AdditionalDefs = ["ENABLED1.DEF", "ENABLED2.DEF"],
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        IsUnpacked = false,
        Executables = null,
        Options = null
    };

    public AutoloadMod EnabledMod => new()
    {
        AddonId = new("enabledMod", "1.5"),
        Type = AddonTypeEnum.Mod,
        Title = "enabledMod",
        GridImageHash = null,
        Author = null,
        Description = null,
        SupportedGame = _game,
        IncompatibleAddons = null,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "enabled_mod.zip"),
        DependentAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = ["ENABLED1.DEF", "ENABLED2.DEF"],
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        IsUnpacked = false,
        Executables = null,
        Options = null
    };

    public AutoloadMod ModThatRequiresOfficialAddon => new()
    {
        AddonId = new("modThatRequiresOfficialAddon", "1.5"),
        Type = AddonTypeEnum.Mod,
        Title = "modThatRequiredOfficialAddon",
        GridImageHash = null,
        Author = null,
        Description = null,
        SupportedGame = _game,
        IncompatibleAddons = null,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "mod_requires_addon.zip"),
        DependentAddons = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { { _addon, null } },
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        IsUnpacked = false,
        Executables = null,
        Options = null
    };

    public AutoloadMod ModThatIncompatibleWithAddon => new()
    {
        AddonId = new("modThatIncompatibleWithVaca", "1.5"),
        Type = AddonTypeEnum.Mod,
        Title = "modThatIncompatibleWithVaca",
        GridImageHash = null,
        Author = null,
        Description = null,
        SupportedGame = _game,
        IncompatibleAddons = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { { _addon, null } },
        DependentAddons = null,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "mod_incompatible_with_addon.zip"),
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        IsUnpacked = false,
        Executables = null,
        Options = null
    };

    public AutoloadMod IncompatibleMod => new()
    {
        AddonId = new("incompatibleMod", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "incompatibleMod",
        GridImageHash = null,
        Author = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = null,
        IncompatibleAddons = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { { "enabledMod", null } },
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        IsUnpacked = false,
        Executables = null,
        Options = null
    };

    public AutoloadMod IncompatibleModWithCompatibleVersion => new()
    {
        AddonId = new("incompatibleModWithCompatibleVersion", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "incompatibleModWithCompatibleVersion",
        GridImageHash = null,
        Author = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "incompatible_mod_with_compatible_version.zip"),
        DependentAddons = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { { "enabledMod", null } },
        IncompatibleAddons = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { { "enabledMod", "<=1.0" } },
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        IsUnpacked = false,
        Executables = null,
        Options = null
    };

    public AutoloadMod IncompatibleModWithIncompatibleVersion => new()
    {
        AddonId = new("incompatibleModWithIncompatibleVersion", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "incompatibleModWithIncompatibleVersion",
        GridImageHash = null,
        Author = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { { "enabledMod", null } },
        IncompatibleAddons = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { { "enabledMod", ">1.1" } },
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        IsUnpacked = false,
        Executables = null,
        Options = null
    };

    public AutoloadMod DependentMod => new()
    {
        AddonId = new("dependentMod", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "dependentMod",
        GridImageHash = null,
        Author = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "dependent_mod.zip"),
        DependentAddons = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { { "enabledMod", null } },
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        IsUnpacked = false,
        Executables = null,
        Options = null
    };


    public AutoloadMod MultipleDependenciesMod => new()
    {
        AddonId = new("multipleDependenciesMod", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "multipleDependenciesMod",
        GridImageHash = null,
        Author = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { { "enabledMod", null }, { "someMod", null } },
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        IsUnpacked = false,
        Executables = null,
        Options = null
    };

    public AutoloadMod DependentModWithIncompatibleVersion => new()
    {
        AddonId = new("dependentModWithIncompatibleVersion", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "dependentModWithIncompatibleVersion",
        GridImageHash = null,
        Author = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { { "enabledMod", "<=1.0" } },
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        IsUnpacked = false,
        Executables = null,
        Options = null
    };

    public AutoloadMod DependentModWithCompatibleVersion => new()
    {
        AddonId = new("dependentModWithCompatibleVersion", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "dependentModWithCompatibleVersion",
        GridImageHash = null,
        Author = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "dependent_mod_with_compatible_version.zip"),
        DependentAddons = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase) { { "enabledMod", ">1.1" } },
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        IsUnpacked = false,
        Executables = null,
        Options = null
    };

    public AutoloadMod DisabledMod => new()
    {
        AddonId = new("disabledMod", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "disabledMod",
        GridImageHash = null,
        Author = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = null,
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = false,
        IsUnpacked = false,
        Executables = null,
        Options = null
    };

    public AutoloadMod ModThatRequiresFeature => new()
    {
        AddonId = new("eduke32mod", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "eduke32mod",
        GridImageHash = null,
        Author = null,
        Description = null,
        SupportedGame = _game,
        RequiredFeatures = [_feature],
        PathToFile = Path.Combine("D:", "Mods", "feature_mod.zip"),
        DependentAddons = null,
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        IsUnpacked = false,
        Executables = null,
        Options = null
    };

    public AutoloadMod ModForAnotherGame => new()
    {
        AddonId = new("somegame-mod", "1.0"),
        Type = AddonTypeEnum.Mod,
        Title = "modForAnotherGame",
        GridImageHash = null,
        Author = null,
        Description = null,
        SupportedGame = new(GameEnum.TekWar),
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = null,
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImageHash = null,
        IsEnabled = true,
        IsUnpacked = false,
        Executables = null,
        Options = null
    };
}
