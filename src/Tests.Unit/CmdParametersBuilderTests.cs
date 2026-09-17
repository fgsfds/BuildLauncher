using System.Collections.Immutable;
using Addons.Addons;
using Core.All.Enums;
using Core.All.Serializable.Addon;
using Core.Client.Config;
using Core.Client.Helpers;
using Games.Games;
using Ports;
using Ports.Builders;
using Ports.Ports;
using Ports.Ports.EDuke32;
using Tests.Unit.Helpers;

namespace Tests.Unit;

/// <summary>
///     Tests for the command parameters builder and the features it consolidates.
/// </summary>
public sealed class CmdParametersBuilderTests
{
    private sealed class ConfigurablePort : BasePort
    {
        public PortCmdArguments? CmdArgumentsOverride { get; set; }

        public override PortEnum PortEnum => PortEnum.Stub;

        protected override string WinExe => string.Empty;

        protected override string LinExe => string.Empty;

        public override string Name => "Test";

        public override ImmutableHashSet<GameEnum> SupportedGames { get; } = [];

        public override ImmutableHashSet<FeatureEnum> SupportedFeatures { get; } = [];

        public override string? InstalledVersion => string.Empty;

        protected override string ConfigFile => string.Empty;

        public override PortCmdArguments CmdArguments => CmdArgumentsOverride ?? NullArgs;

        public override void AfterEnd(BaseGame game, BaseAddon campaign) { }

        public override void BeforeStart(BaseGame game, BaseAddon campaign) { }
    }

    private static readonly PortCmdArguments NullArgs = new()
    {
        AddDirectory = null,
        MainGrp = null,
        AddGrp = null,
        AddFile = null,
        AddDef = null,
        AddCon = null,
        MainDef = null,
        MainCon = null,
        SkillLevel = null,
        AddGameDir = null,
        AddRff = null,
        AddSnd = null,
        SkipIntro = null,
        SkipStartup = null,
        SkipSteam = null
    };

    private static DukeCampaign CreateCampaign(
        string? mainDef = null,
        ImmutableArray<string>? additionalDefs = null,
        AddonFilePathWrapper? fileInfo = null,
        MapFileJsonModel? startMap = null,
        AddonTypeEnum type = AddonTypeEnum.TC)
    {
        return new DukeCampaign
        {
            AddonId = new("id", null),
            Type = type,
            Title = "Test",
            SupportedGame = new(GameEnum.Duke3D, null, null),
            FileInfo = fileInfo,
            GridImageHash = null,
            PreviewImageHash = null,
            Description = null,
            Author = null,
            ReleaseDate = null,
            MainDef = mainDef,
            AdditionalDefs = additionalDefs,
            MainCon = null,
            AdditionalCons = null,
            RTS = null,
            StartMap = startMap,
            DependentAddons = null,
            IncompatibleAddons = null,
            RequiredFeatures = null,
            Executables = null,
            Options = null
        };
    }

    private static DukeGame CreateGame()
    {
        return new DukeGame()
        {
            Duke64RomPath = null,
            DukeZHRomPath = null,
            DukeWTInstallPath = null,
            GameInstallFolder = Path.GetTempPath()
        };
    }

    private static DukeCampaign CreateDukeCampaign(
        GameEnum gameEnum,
        string id,
        AddonFilePathWrapper? fileInfo,
        AddonTypeEnum type = AddonTypeEnum.TC)
    {
        return new DukeCampaign
        {
            AddonId = new(id, null),
            Type = type,
            Title = "Test",
            SupportedGame = new(gameEnum),
            FileInfo = fileInfo,
            GridImageHash = null,
            PreviewImageHash = null,
            Description = null,
            Author = null,
            ReleaseDate = null,
            MainDef = null,
            AdditionalDefs = null,
            MainCon = null,
            AdditionalCons = null,
            RTS = null,
            StartMap = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            RequiredFeatures = null,
            Executables = null,
            Options = null
        };
    }

    private static GenericCampaign CreateSlaveTcCampaign()
    {
        return new GenericCampaign
        {
            AddonId = new("slave-tc", null),
            Type = AddonTypeEnum.TC,
            Title = "Slave TC",
            SupportedGame = new(GameEnum.Slave),
            FileInfo = new AddonFilePathWrapper("D:\\Maps\\slave_tc.zip", "slave_tc.zip"),
            GridImageHash = null,
            PreviewImageHash = null,
            Description = null,
            Author = null,
            ReleaseDate = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            RequiredFeatures = null,
            Executables = null,
            Options = null
        };
    }

    private static CmdParametersBuilder CreateBuilder(ConfigurablePort port)
        => CmdParametersBuilderFactory.Create(CreateGame(), CreateCampaign(), port);

    /// <summary>
    ///     Tests that the skip-intro parameter is appended when set.
    /// </summary>
    [Fact]
    public void SkipIntro_WhenSet_AppendsParameter()
    {
        var port = new ConfigurablePort { CmdArgumentsOverride = NullArgs with { SkipIntro = " -quick" } };
        Assert.Contains(" -quick", CreateBuilder(port).AppendSkipIntroParameter().ToString());
    }

    /// <summary>
    ///     Tests that nothing is appended when the skip-intro parameter is null.
    /// </summary>
    [Fact]
    public void SkipIntro_WhenNull_AppendsNothing()
    {
        var port = new ConfigurablePort();
        Assert.Equal(string.Empty, CreateBuilder(port).AppendSkipIntroParameter().ToString());
    }

    /// <summary>
    ///     Tests that the skip-startup parameter is appended when set.
    /// </summary>
    [Fact]
    public void SkipStartup_WhenSet_AppendsParameter()
    {
        var port = new ConfigurablePort { CmdArgumentsOverride = NullArgs with { SkipStartup = " -nosetup" } };
        Assert.Contains(" -nosetup", CreateBuilder(port).AppendSkipStartupParameter().ToString());
    }

    /// <summary>
    ///     Tests that nothing is appended when the skip-startup parameter is null.
    /// </summary>
    [Fact]
    public void SkipStartup_WhenNull_AppendsNothing()
    {
        var port = new ConfigurablePort();
        Assert.Equal(string.Empty, CreateBuilder(port).AppendSkipStartupParameter().ToString());
    }

    /// <summary>
    ///     Tests that the skip-steam parameter is appended when set.
    /// </summary>
    [Fact]
    public void SkipSteam_WhenSet_AppendsParameter()
    {
        var port = new ConfigurablePort { CmdArgumentsOverride = NullArgs with { SkipSteam = " -usecwd" } };
        Assert.Contains(" -usecwd", CreateBuilder(port).AppendSkipSteam().ToString());
    }

    /// <summary>
    ///     Tests that nothing is appended when the skip-steam parameter is null.
    /// </summary>
    [Fact]
    public void SkipSteam_WhenNull_AppendsNothing()
    {
        var port = new ConfigurablePort();
        Assert.Equal(string.Empty, CreateBuilder(port).AppendSkipSteam().ToString());
    }

    /// <summary>
    ///     Tests that GetStartGameArgs emits both skip parameters when set.
    /// </summary>
    [Fact]
    public void GetStartGameArgs_SkipParamsSet_EmitsBoth()
    {
        var port = new ConfigurablePort { CmdArgumentsOverride = NullArgs with { SkipIntro = " -quick", SkipStartup = " -nosetup" } };
        var args = port.GetStartGameArgs(CreateGame(), CreateCampaign(), [], [], skipIntro: true, skipStartup: true);

        Assert.Contains(" -quick", args);
        Assert.Contains(" -nosetup", args);
    }

    /// <summary>
    ///     Tests that GetStartGameArgs emits nothing for skip parameters when they are null.
    /// </summary>
    [Fact]
    public void GetStartGameArgs_SkipParamsNull_EmitsNothing()
    {
        var port = new ConfigurablePort();
        var args = port.GetStartGameArgs(CreateGame(), CreateCampaign(), [], [], skipIntro: true, skipStartup: true);

        Assert.Equal(string.Empty, args);
    }

    /// <summary>
    ///     Tests that AppendOverrideMainDef appends the main def override parameter.
    /// </summary>
    [Fact]
    public void AppendOverrideMainDef_AppendsMainDefParameter()
    {
        var port = new ConfigurablePort { CmdArgumentsOverride = NullArgs with { MainDef = " -h " } };
        Assert.Contains(" -h \"a\"", CreateBuilder(port).AppendOverrideMainDef().ToString());
    }

    /// <summary>
    ///     Tests that AppendMainDefArgs appends the main def parameter when set.
    /// </summary>
    [Fact]
    public void AppendMainDefArgs_WhenMainDefSet_AppendsParameter()
    {
        var port = new ConfigurablePort { CmdArgumentsOverride = NullArgs with { MainDef = " -h " } };

        var result = CmdParametersBuilderFactory.Create(CreateGame(), CreateCampaign("TC.DEF"), port).AppendMainDefArgs().ToString();
        Assert.Contains(" -h \"TC.DEF\"", result);
    }

    /// <summary>
    ///     Tests that AppendMainDefArgs appends nothing when the main def is null.
    /// </summary>
    [Fact]
    public void AppendMainDefArgs_WhenMainDefNull_AppendsNothing()
    {
        var port = new ConfigurablePort { CmdArgumentsOverride = NullArgs with { MainDef = " -h " } };
        Assert.Equal(string.Empty, CreateBuilder(port).AppendMainDefArgs().ToString());
    }

    /// <summary>
    ///     Tests that AppendAdditionalDefsArgs appends each additional def when set.
    /// </summary>
    [Fact]
    public void AppendAdditionalDefsArgs_WhenDefsSet_AppendsEach()
    {
        var port = new ConfigurablePort { CmdArgumentsOverride = NullArgs with { AddDef = " -mh " } };

        var result = CmdParametersBuilderFactory.Create(CreateGame(), CreateCampaign(additionalDefs: ["TC1.DEF", "TC2.DEF"]), port).AppendAdditionalDefsArgs().ToString();

        Assert.Contains(" -mh \"TC1.DEF\"", result);
        Assert.Contains(" -mh \"TC2.DEF\"", result);
    }

    /// <summary>
    ///     Tests that AppendAdditionalDefsArgs appends nothing when no additional defs are set.
    /// </summary>
    [Fact]
    public void AppendAdditionalDefsArgs_WhenDefsNull_AppendsNothing()
    {
        var port = new ConfigurablePort();
        Assert.Equal(string.Empty, CreateBuilder(port).AppendAdditionalDefsArgs().ToString());
    }

    /// <summary>
    ///     Tests that skill selection is available when the skill-level parameter is set.
    /// </summary>
    [Fact]
    public void IsSkillSelectionAvailable_True_WhenSkillLevelSet()
    {
        var port = new ConfigurablePort { CmdArgumentsOverride = NullArgs with { SkillLevel = "-s" } };
        Assert.True(port.IsSkillSelectionAvailable);
    }

    /// <summary>
    ///     Tests that skill selection is unavailable when the skill-level parameter is null.
    /// </summary>
    [Fact]
    public void IsSkillSelectionAvailable_False_WhenSkillLevelNull()
    {
        var port = new ConfigurablePort();
        Assert.False(port.IsSkillSelectionAvailable);
    }

    /// <summary>
    ///     Tests that AppendRequiredFiles appends the campaign file and start map when a manifested map is set.
    /// </summary>
    [Fact]
    public void AppendRequiredFiles_WhenManifestedMap_AppendsFileAndMap()
    {
        var camp = CreateCampaign(
            fileInfo: new AddonFilePathWrapper("D:\\Maps\\camp.zip", "camp.zip"),
            startMap: new MapFileJsonModel { File = "TEST.MAP" },
            type: AddonTypeEnum.Map);
        var port = new ConfigurablePort { CmdArgumentsOverride = NullArgs with { AddFile = "-file " } };

        var builder = CmdParametersBuilderFactory.Create(CreateGame(), camp, port);
        builder.AppendTcOrMapArgs(camp);

        var result = builder.ToString();

        Assert.Contains(" -file \"D:\\Maps\\camp.zip\"", NormalizerHelper.NormalizePath(result));
        Assert.Contains(" -map \"TEST.MAP\"", result);
    }

    /// <summary>
    ///     Tests that AppendOptionsArgs appends the ini parameter for an INI option on a Blood game.
    /// </summary>
    [Fact]
    public void AppendOptionsArgs_IniOptionForBlood_AppendsIniParameter()
    {
        var bloodGame = new BloodGame { GameInstallFolder = Path.GetTempPath() };
        var bloodCamp = new BloodCampaign
        {
            AddonId = new("blood", null),
            Type = AddonTypeEnum.Official,
            Title = "Blood",
            SupportedGame = new(GameEnum.Blood),
            FileInfo = null,
            GridImageHash = null,
            PreviewImageHash = null,
            Description = null,
            Author = null,
            ReleaseDate = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            RequiredFeatures = null,
            Executables = null,
            INI = null,
            RFF = null,
            SND = null,
            Options = new()
            {
                ["iniOpt"] = new()
                {
                    ["TEST.INI"] = OptionalParameterTypeEnum.INI
                }
            }
        };
        var port = new ConfigurablePort();

        var result = CmdParametersBuilderFactory.Create(bloodGame, bloodCamp, port).AppendOptionsArgs(["iniOpt"]).ToString();

        Assert.Contains(" -ini \"TEST.INI\"", result);
    }

    /// <summary>
    ///     Tests that the factory returns the expected builder type for each port.
    /// </summary>
    [Fact]
    public void Factory_ReturnsExpectedBuilderPerPort()
    {
        var game = CreateGame();
        var addon = CreateCampaign();

        (BasePort Port, Type Expected)[] cases =
        [
            (new VoidSW(), typeof(VoidSWCmdParametersBuilder)),
            (new RedNukem(), typeof(RedNukemCmdParametersBuilder)),
            (new EDuke32(), typeof(EDuke32CmdParametersBuilder)),
            (new NBlood(), typeof(EDuke32CmdParametersBuilder)),
            (new NotBlood(), typeof(EDuke32CmdParametersBuilder)),
            (new PCExhumed(), typeof(EDuke32CmdParametersBuilder)),
            (new Fury(new ConfigProviderFake()), typeof(FuryCmdParametersBuilder)),
            (new Raze(), typeof(RazeCmdParametersBuilder)),
            (new DosBox(), typeof(DosBoxCmdParametersBuilder)),
            (new BuildGDX(), typeof(BuildGDXCmdParametersBuilder)),
            (new ConfigurablePort(), typeof(CmdParametersBuilder))
        ];

        foreach (var (port, expected) in cases)
        {
            Assert.IsType(expected, CmdParametersBuilderFactory.Create(game, addon, port));
        }
    }

    /// <summary>
    ///     Tests that a Slave (PowerSlave) total conversion appends its addon file to the command line.
    /// </summary>
    [Fact]
    public void SlaveTcAddon_AppendsAddonFile()
    {
        var port = new ConfigurablePort { CmdArgumentsOverride = NullArgs with { AddDirectory = "-j ", AddFile = "-g " } };
        var game = new SlaveGame { GameInstallFolder = Path.GetTempPath() };
        var addon = CreateSlaveTcCampaign();

        var args = port.GetStartGameArgs(game, addon, [], [], skipIntro: false, skipStartup: false);

        Assert.Contains($" -j \"{addon.FileInfo!.Value.PathToFolder}\"", args);
        Assert.Contains(" -g \"slave_tc.zip\"", args);
    }

    /// <summary>
    ///     Tests that a NAM total conversion appends its addon file to the command line.
    /// </summary>
    [Fact]
    public void NamTcAddon_AppendsAddonFile()
    {
        var port = new ConfigurablePort { CmdArgumentsOverride = NullArgs with { AddDirectory = "-j ", AddFile = "-g ", MainGrp = "-gamegrp ", MainCon = "-x " } };
        var game = new NamGame { GameInstallFolder = Path.GetTempPath() };
        var addon = CreateDukeCampaign(GameEnum.NAM, "nam-tc", new AddonFilePathWrapper("D:\\Maps\\nam_tc.zip", "nam_tc.zip"));

        var args = port.GetStartGameArgs(game, addon, [], [], skipIntro: false, skipStartup: false);

        Assert.Contains($" -j \"{addon.FileInfo!.Value.PathToFolder}\"", args);
        Assert.Contains(" -g \"nam_tc.zip\"", args);
    }

    /// <summary>
    ///     Tests that a WW2GI total conversion appends its addon file to the command line.
    /// </summary>
    [Fact]
    public void Ww2GiTcAddon_AppendsAddonFile()
    {
        var port = new ConfigurablePort { CmdArgumentsOverride = NullArgs with { AddDirectory = "-j ", AddFile = "-g ", MainGrp = "-gamegrp ", MainCon = "-x " } };
        var game = new WW2GIGame { GameInstallFolder = Path.GetTempPath() };
        var addon = CreateDukeCampaign(GameEnum.WW2GI, "ww2gi-tc", new AddonFilePathWrapper("D:\\Maps\\ww2gi_tc.zip", "ww2gi_tc.zip"));

        var args = port.GetStartGameArgs(game, addon, [], [], skipIntro: false, skipStartup: false);

        Assert.Contains($" -j \"{addon.FileInfo!.Value.PathToFolder}\"", args);
        Assert.Contains(" -g \"ww2gi_tc.zip\"", args);
    }
}
