﻿using Addons.Addons;
using Common;
using Common.Enums;
using Common.Enums.Addons;
using Common.Enums.Versions;
using CommunityToolkit.Diagnostics;

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
            GameEnum.ShadowWarrior => new(GameEnum.ShadowWarrior),
            GameEnum.Exhumed => new(GameEnum.Exhumed),
            GameEnum.Redneck => new(GameEnum.Redneck),
            GameEnum.RidesAgain => new(GameEnum.RidesAgain),
            GameEnum.Fury => new(GameEnum.Fury),
            GameEnum.NAM => new(GameEnum.NAM),
            _ => ThrowHelper.ThrowNotSupportedException<GameStruct>()
        };

        _addon = gameEnum switch
        {
            GameEnum.Duke3D => nameof(DukeAddonEnum.DukeVaca).ToLower(),
            GameEnum.Blood => nameof(BloodAddonEnum.BloodCP).ToLower(),
            GameEnum.ShadowWarrior => nameof(WangAddonEnum.TwinDragon).ToLower(),
            GameEnum.Redneck => nameof(RedneckAddonEnum.Route66).ToLower(),
            GameEnum.Fury => "",
            GameEnum.Duke64 => "",
            GameEnum.Exhumed => "",
            GameEnum.NAM => "",
            _ => ThrowHelper.ThrowNotSupportedException<string>()
        };

        _feature = gameEnum switch
        {
            GameEnum.Duke3D => FeatureEnum.EDuke32_CON,
            GameEnum.Blood => FeatureEnum.Modern_Types,
            GameEnum.ShadowWarrior => FeatureEnum.Models,
            GameEnum.Duke64 => FeatureEnum.Models,
            GameEnum.Exhumed => FeatureEnum.Models,
            GameEnum.Redneck => FeatureEnum.Models,
            GameEnum.RidesAgain => FeatureEnum.Models,
            GameEnum.Fury => FeatureEnum.EDuke32_CON,
            GameEnum.NAM => FeatureEnum.Models,
            _ => ThrowHelper.ThrowNotSupportedException<FeatureEnum>()
        };
    }

    public AutoloadMod EnabledModWithCons => new()
    {
        Id = "enabledMod",
        Type = AddonTypeEnum.Mod,
        Title = "enabledMod",
        GridImage = null,
        Author = null,
        Description = null,
        Version = "1.5",
        SupportedGame = _game,
        IncompatibleAddons = null,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "enabled_mod.zip"),
        DependentAddons = null,
        AdditionalCons = ["ENABLED1.CON", "ENABLED2.CON"],
        MainDef = null,
        AdditionalDefs = ["ENABLED1.DEF", "ENABLED2.DEF"],
        StartMap = null,
        PreviewImage = null,
        IsEnabled = true,
        IsFolder = false,
        Executables = null
    };

    public AutoloadMod EnabledMod => new()
    {
        Id = "enabledMod",
        Type = AddonTypeEnum.Mod,
        Title = "enabledMod",
        GridImage = null,
        Author = null,
        Description = null,
        Version = "1.5",
        SupportedGame = _game,
        IncompatibleAddons = null,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "enabled_mod.zip"),
        DependentAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = ["ENABLED1.DEF", "ENABLED2.DEF"],
        StartMap = null,
        PreviewImage = null,
        IsEnabled = true,
        IsFolder = false,
        Executables = null
    };

    public AutoloadMod ModThatRequiresOfficialAddon => new()
    {
        Id = "modThatRequiresOfficialAddon",
        Type = AddonTypeEnum.Mod,
        Title = "modThatRequiredOfficialAddon",
        GridImage = null,
        Author = null,
        Description = null,
        Version = "1.5",
        SupportedGame = _game,
        IncompatibleAddons = null,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "mod_requires_addon.zip"),
        DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { _addon, null } },
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImage = null,
        IsEnabled = true,
        IsFolder = false,
        Executables = null
    };

    public AutoloadMod ModThatIncompatibleWithAddon => new()
    {
        Id = "modThatIncompatibleWithVaca",
        Type = AddonTypeEnum.Mod,
        Title = "modThatIncompatibleWithVaca",
        GridImage = null,
        Author = null,
        Description = null,
        Version = "1.5",
        SupportedGame = _game,
        IncompatibleAddons = new(StringComparer.OrdinalIgnoreCase) { { _addon, null } },
        DependentAddons = null,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "mod_incompatible_with_addon.zip"),
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImage = null,
        IsEnabled = true,
        IsFolder = false,
        Executables = null
    };

    public AutoloadMod IncompatibleMod => new()
    {
        Id = "incompatibleMod",
        Type = AddonTypeEnum.Mod,
        Title = "incompatibleMod",
        GridImage = null,
        Author = null,
        Description = null,
        Version = "1.0",
        SupportedGame = _game,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = null,
        IncompatibleAddons = new(StringComparer.OrdinalIgnoreCase) { { "enabledMod", null } },
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImage = null,
        IsEnabled = true,
        IsFolder = false,
        Executables = null
    };

    public AutoloadMod IncompatibleModWithCompatibleVersion => new()
    {
        Id = "incompatibleModWithCompatibleVersion",
        Type = AddonTypeEnum.Mod,
        Title = "incompatibleModWithCompatibleVersion",
        GridImage = null,
        Author = null,
        Description = null,
        Version = "1.0",
        SupportedGame = _game,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "incompatible_mod_with_compatible_version.zip"),
        DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { "enabledMod", null } },
        IncompatibleAddons = new(StringComparer.OrdinalIgnoreCase) { { "enabledMod", "<=1.0" } },
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImage = null,
        IsEnabled = true,
        IsFolder = false,
        Executables = null
    };

    public AutoloadMod IncompatibleModWithIncompatibleVersion => new()
    {
        Id = "incompatibleModWithIncompatibleVersion",
        Type = AddonTypeEnum.Mod,
        Title = "incompatibleModWithIncompatibleVersion",
        GridImage = null,
        Author = null,
        Description = null,
        Version = "1.0",
        SupportedGame = _game,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { "enabledMod", null } },
        IncompatibleAddons = new(StringComparer.OrdinalIgnoreCase) { { "enabledMod", ">1.1" } },
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImage = null,
        IsEnabled = true,
        IsFolder = false,
        Executables = null
    };

    public AutoloadMod DependentMod => new()
    {
        Id = "dependentMod",
        Type = AddonTypeEnum.Mod,
        Title = "dependentMod",
        GridImage = null,
        Author = null,
        Description = null,
        Version = "1.0",
        SupportedGame = _game,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "dependent_mod.zip"),
        DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { "enabledMod", null } },
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImage = null,
        IsEnabled = true,
        IsFolder = false,
        Executables = null
    };
    

    public AutoloadMod MultipleDependenciesMod => new()
    {
        Id = "multipleDependenciesMod",
        Type = AddonTypeEnum.Mod,
        Title = "multipleDependenciesMod",
        GridImage = null,
        Author = null,
        Description = null,
        Version = "1.0",
        SupportedGame = _game,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { "enabledMod", null }, { "someMod", null } },
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImage = null,
        IsEnabled = true,
        IsFolder = false,
        Executables = null
    };

    public AutoloadMod DependentModWithIncompatibleVersion => new()
    {
        Id = "dependentModWithIncompatibleVersion",
        Type = AddonTypeEnum.Mod,
        Title = "dependentModWithIncompatibleVersion",
        GridImage = null,
        Author = null,
        Description = null,
        Version = "1.0",
        SupportedGame = _game,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { "enabledMod", "<=1.0" } },
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImage = null,
        IsEnabled = true,
        IsFolder = false,
        Executables = null
    };

    public AutoloadMod DependentModWithCompatibleVersion => new()
    {
        Id = "dependentModWithCompatibleVersion",
        Type = AddonTypeEnum.Mod,
        Title = "dependentModWithCompatibleVersion",
        GridImage = null,
        Author = null,
        Description = null,
        Version = "1.0",
        SupportedGame = _game,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "dependent_mod_with_compatible_version.zip"),
        DependentAddons = new(StringComparer.OrdinalIgnoreCase) { { "enabledMod", ">1.1" } },
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImage = null,
        IsEnabled = true,
        IsFolder = false,
        Executables = null
    };

    public AutoloadMod DisabledMod => new()
    {
        Id = "disabledMod",
        Type = AddonTypeEnum.Mod,
        Title = "disabledMod",
        GridImage = null,
        Author = null,
        Description = null,
        Version = "1.0",
        SupportedGame = _game,
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = null,
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImage = null,
        IsEnabled = false,
        IsFolder = false,
        Executables = null
    };

    public AutoloadMod ModThatRequiredFeature => new()
    {
        Id = "eduke32mod",
        Type = AddonTypeEnum.Mod,
        Title = "eduke32mod",
        GridImage = null,
        Author = null,
        Description = null,
        Version = "1.0",
        SupportedGame = _game,
        RequiredFeatures = [_feature],
        PathToFile = Path.Combine("D:", "Mods", "feature_mod.zip"),
        DependentAddons = null,
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImage = null,
        IsEnabled = true,
        IsFolder = false,
        Executables = null
    };

    public AutoloadMod ModForAnotherGame => new()
    {
        Id = "somegame-mod",
        Type = AddonTypeEnum.Mod,
        Title = "modForAnotherGame",
        GridImage = null,
        Author = null,
        Description = null,
        Version = "1.0",
        SupportedGame = new(GameEnum.TekWar),
        RequiredFeatures = null,
        PathToFile = Path.Combine("D:", "Mods", "!!!!!!!!!!NOPE!!!!!!!!!!"),
        DependentAddons = null,
        IncompatibleAddons = null,
        AdditionalCons = null,
        MainDef = null,
        AdditionalDefs = null,
        StartMap = null,
        PreviewImage = null,
        IsEnabled = true,
        IsFolder = false,
        Executables = null
    };
}
