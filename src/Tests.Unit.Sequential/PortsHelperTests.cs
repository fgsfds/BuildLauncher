using System.Collections.Immutable;
using System.Text;
using Addons.Addons;
using Avalonia.Desktop.Helpers;
using Core.All;
using Core.All.Enums;
using Core.All.Enums.Addons;
using Games.Games;
using Ports.Ports;

namespace Tests.Unit;

public sealed class PortsHelperTests
{
    [Fact]
    public void CheckPortRequirements_NullObject_ReturnsFalse()
    {
        var game = new TestGame(GameEnum.Duke3D);
        var port = new TestPort(PortEnum.EDuke32);

        var result = PortsHelper.CheckPortRequirements(null, game, port);

        Assert.False(result);
    }

    [Fact]
    public void CheckPortRequirements_NonAddonObject_ReturnsFalse()
    {
        var game = new TestGame(GameEnum.Duke3D);
        var port = new TestPort(PortEnum.EDuke32);

        var result = PortsHelper.CheckPortRequirements("not an addon", game, port);

        Assert.False(result);
    }

    [Fact]
    public void CheckPortRequirements_PortNotInstalled_ReturnsFalse()
    {
        var game = new TestGame(GameEnum.Duke3D);
        var port = new TestPort(PortEnum.EDuke32, isInstalled: false);

        var result = PortsHelper.CheckPortRequirements(CreateCampaign(), game, port);

        Assert.False(result);
    }

    [Fact]
    public void CheckPortRequirements_ExecutablesMatchPort_ReturnsTrue()
    {
        var game = new TestGame(GameEnum.Duke3D);
        var port = new TestPort(PortEnum.EDuke32);

        var addon = CreateCampaign(
            executables: new()
            {
                [OSEnum.Windows] = new()
                {
                    [PortEnum.EDuke32] = "eduke32.exe"
                }
            }
            );

        var result = PortsHelper.CheckPortRequirements(addon, game, port);

        Assert.True(result);
    }

    [Fact]
    public void CheckPortRequirements_ExecutablesMismatchPort_ReturnsFalse()
    {
        var game = new TestGame(GameEnum.Duke3D);
        var port = new TestPort(PortEnum.NBlood);

        var addon = CreateCampaign(
            executables: new()
            {
                [OSEnum.Windows] = new()
                {
                    [PortEnum.EDuke32] = "eduke32.exe"
                }
            }
            );

        var result = PortsHelper.CheckPortRequirements(addon, game, port);

        Assert.False(result);
    }

    [Fact]
    public void CheckPortRequirements_RequiredFeaturesNotSupported_ReturnsFalse()
    {
        var game = new TestGame(GameEnum.Duke3D);
        var port = new TestPort(PortEnum.EDuke32);
        port.SetSupportedFeatures([]);

        var addon = CreateCampaign(requiredFeatures: [FeatureEnum.Models]);

        var result = PortsHelper.CheckPortRequirements(addon, game, port);

        Assert.False(result);
    }

    [Fact]
    public void CheckPortRequirements_RequiredFeaturesAllSupported_Passes()
    {
        var game = new TestGame(GameEnum.Duke3D);
        var port = new TestPort(PortEnum.EDuke32);

        port.SetSupportedFeatures(
            [
                FeatureEnum.Models,
                FeatureEnum.Hightile,
                FeatureEnum.EDuke32_CON
            ]
            );

        port.SetSupportedGames([GameEnum.Duke3D]);

        var addon = CreateCampaign(
            requiredFeatures:
            [
                FeatureEnum.Models,
                FeatureEnum.Hightile
            ]
            );

        var result = PortsHelper.CheckPortRequirements(addon, game, port);

        Assert.True(result);
    }

    [Fact]
    public void CheckPortRequirements_GameNotInSupportedList_ReturnsFalse()
    {
        var game = new TestGame(GameEnum.Blood);
        var port = new TestPort(PortEnum.EDuke32);

        port.SetSupportedGames(
            [
                GameEnum.Duke3D,
                GameEnum.NAM
            ]
            );

        var addon = CreateCampaign(supportedGame: new GameInfo(GameEnum.Blood));

        var result = PortsHelper.CheckPortRequirements(addon, game, port);

        Assert.False(result);
    }

    [Fact]
    public void CheckPortRequirements_GameVersionNotSupported_ReturnsFalse()
    {
        var game = new TestGame(GameEnum.Blood);
        var port = new TestPort(PortEnum.EDuke32);
        port.SetSupportedGames([GameEnum.Blood]);
        port.SetSupportedGamesVersions(["1.0"]);

        var addon = CreateCampaign(supportedGame: new GameInfo(GameEnum.Blood, "2.0", null));

        var result = PortsHelper.CheckPortRequirements(addon, game, port);

        Assert.False(result);
    }

    [Fact]
    public void CheckPortRequirements_GameVersionMatches_ReturnsTrue()
    {
        var game = new TestGame(GameEnum.Blood);
        var port = new TestPort(PortEnum.EDuke32);
        port.SetSupportedGames([GameEnum.Blood]);
        port.SetSupportedGamesVersions(["1.0"]);

        var addon = CreateCampaign(supportedGame: new GameInfo(GameEnum.Blood, "1.0", null));

        var result = PortsHelper.CheckPortRequirements(addon, game, port);

        Assert.True(result);
    }

    [Fact]
    public void CheckPortRequirements_BuildGDX_NonOfficial_ReturnsFalse()
    {
        var game = new TestGame(GameEnum.Duke3D);
        var port = new TestPort(PortEnum.BuildGDX);
        port.SetSupportedGames([GameEnum.Duke3D]);

        var addon = CreateCampaign(type: AddonTypeEnum.TC);

        var result = PortsHelper.CheckPortRequirements(addon, game, port);

        Assert.False(result);
    }

    [Fact]
    public void CheckPortRequirements_BuildGDX_LooseMap_ReturnsFalse()
    {
        var game = new TestGame(GameEnum.Duke3D);
        var port = new TestPort(PortEnum.BuildGDX);
        port.SetSupportedGames([GameEnum.Duke3D]);

        var result = PortsHelper.CheckPortRequirements(CreateLooseMap(), game, port);

        Assert.False(result);
    }

    [Fact]
    public void CheckPortRequirements_BuildGDX_OfficialCampaign_ReturnsTrue()
    {
        var game = new TestGame(GameEnum.Duke3D);
        var port = new TestPort(PortEnum.BuildGDX);
        port.SetSupportedGames([GameEnum.Duke3D]);

        var addon = CreateCampaign(type: AddonTypeEnum.Official);

        var result = PortsHelper.CheckPortRequirements(addon, game, port);

        Assert.True(result);
    }

    [Fact]
    public void CheckPortRequirements_DosBox_WithMainDef_ReturnsFalse()
    {
        var game = new TestGame(GameEnum.Blood);
        var port = new TestPort(PortEnum.DosBox);
        port.SetSupportedGames([GameEnum.Blood]);

        var addon = CreateCampaign(mainDef: "custom.def");

        var result = PortsHelper.CheckPortRequirements(addon, game, port);

        Assert.False(result);
    }

    [Fact]
    public void CheckPortRequirements_DosBox_WithAdditionalDefs_ReturnsFalse()
    {
        var game = new TestGame(GameEnum.Blood);
        var port = new TestPort(PortEnum.DosBox);
        port.SetSupportedGames([GameEnum.Blood]);

        var addon = CreateCampaign(additionalDefs: ["extra.def"]);

        var result = PortsHelper.CheckPortRequirements(addon, game, port);

        Assert.False(result);
    }

    [Fact]
    public void CheckPortRequirements_DosBox_Duke3D_NonOfficial_ReturnsFalse()
    {
        var game = new TestGame(GameEnum.Duke3D);
        var port = new TestPort(PortEnum.DosBox);
        port.SetSupportedGames([GameEnum.Duke3D]);

        var addon = CreateCampaign(type: AddonTypeEnum.TC);

        var result = PortsHelper.CheckPortRequirements(addon, game, port);

        Assert.False(result);
    }

    [Fact]
    public void CheckPortRequirements_DosBox_Wang_NonBaseAddon_ReturnsFalse()
    {
        var game = new TestGame(GameEnum.Wang);
        var port = new TestPort(PortEnum.DosBox);
        port.SetSupportedGames([GameEnum.Wang]);

        var addon = CreateCampaign(addonId: new AddonId("twin_dragon"));

        var result = PortsHelper.CheckPortRequirements(addon, game, port);

        Assert.False(result);
    }

    [Fact]
    public void CheckPortRequirements_DosBox_Wang_BaseAddon_ReturnsTrue()
    {
        var game = new TestGame(GameEnum.Wang);
        var port = new TestPort(PortEnum.DosBox);
        port.SetSupportedGames([GameEnum.Wang]);

        var addon = CreateCampaign(addonId: new AddonId(nameof(GameEnum.Wang)), supportedGame: new GameInfo(GameEnum.Wang));

        var result = PortsHelper.CheckPortRequirements(addon, game, port);

        Assert.True(result);
    }

    [Fact]
    public void CheckPortRequirements_DosBox_LooseMap_ReturnsFalse()
    {
        var game = new TestGame(GameEnum.Blood);
        var port = new TestPort(PortEnum.DosBox);
        port.SetSupportedGames([GameEnum.Blood]);

        var result = PortsHelper.CheckPortRequirements(CreateLooseMap(), game, port);

        Assert.False(result);
    }

    [Fact]
    public void CheckPortRequirements_DosBox_Duke3D_Vaca_WithVacationExe_ReturnsTrue()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            _ = Directory.CreateDirectory(tempDir);
            File.WriteAllText(Path.Combine(tempDir, "VACATION.EXE"), "fake exe");

            var game = new TestGame(GameEnum.Duke3D)
            {
                GameInstallFolder = tempDir
            };

            var port = new TestPort(PortEnum.DosBox);
            port.SetSupportedGames([GameEnum.Duke3D]);

            var addon = CreateCampaign(type: AddonTypeEnum.Official, addonId: new AddonId(nameof(DukeAddonEnum.DukeVaca)));

            var result = PortsHelper.CheckPortRequirements(addon, game, port);

            Assert.True(result);
        }
        finally
        {
            try
            {
                Directory.Delete(tempDir, true);
            }
            catch
            {
                /* ignore */
            }
        }
    }

    [Fact]
    public void CheckPortRequirements_DosBox_Duke3D_Vaca_WithoutVacationExe_ReturnsFalse()
    {
        var game = new TestGame(GameEnum.Duke3D)
        {
            GameInstallFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
        };

        var port = new TestPort(PortEnum.DosBox);
        port.SetSupportedGames([GameEnum.Duke3D]);

        var addon = CreateCampaign(type: AddonTypeEnum.Official, addonId: new AddonId(nameof(DukeAddonEnum.DukeVaca)));

        var result = PortsHelper.CheckPortRequirements(addon, game, port);

        Assert.False(result);
    }

    [Fact]
    public void CheckPortRequirements_DosBox_Duke3D_Vaca_NullInstallFolder_ReturnsFalse()
    {
        var game = new TestGame(GameEnum.Duke3D)
        {
            GameInstallFolder = null
        };

        var port = new TestPort(PortEnum.DosBox);
        port.SetSupportedGames([GameEnum.Duke3D]);

        var addon = CreateCampaign(type: AddonTypeEnum.Official, addonId: new AddonId(nameof(DukeAddonEnum.DukeVaca)));

        var result = PortsHelper.CheckPortRequirements(addon, game, port);

        Assert.False(result);
    }

    [Fact]
    public void CheckPortRequirements_HappyPath_AllChecksPass_ReturnsTrue()
    {
        var game = new TestGame(GameEnum.Blood);
        var port = new TestPort(PortEnum.NBlood);
        port.SetSupportedGames([GameEnum.Blood]);

        var addon = CreateCampaign(supportedGame: new GameInfo(GameEnum.Blood));

        var result = PortsHelper.CheckPortRequirements(addon, game, port);

        Assert.True(result);
    }

    [Fact]
    public void CheckPortRequirements_StubPort_ReturnsTrue()
    {
        var game = new TestGame(GameEnum.Duke3D);
        var port = new TestPort(PortEnum.DosBox);
        port.SetSupportedGames([GameEnum.Duke3D]);

        var addon = CreateCampaign(type: AddonTypeEnum.Official, addonId: new AddonId(nameof(GameEnum.Duke3D)));

        var result = PortsHelper.CheckPortRequirements(addon, game, port);

        Assert.True(result);
    }

    private static DukeCampaign CreateCampaign(
        AddonId? addonId = null,
        AddonTypeEnum type = AddonTypeEnum.Official,
        GameInfo? supportedGame = null,
        string? mainDef = null,
        ImmutableArray<string>? additionalDefs = null,
        ImmutableArray<FeatureEnum>? requiredFeatures = null,
        Dictionary<OSEnum, Dictionary<PortEnum, string>>? executables = null)
    {
        return new DukeCampaign
        {
            AddonId = addonId ?? new AddonId("test"),
            Type = type,
            SupportedGame = supportedGame ?? new GameInfo(GameEnum.Duke3D),
            Title = "Test",
            Author = null,
            ReleaseDate = null,
            Description = null,
            RequiredFeatures = requiredFeatures,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            GridImageHash = null,
            PreviewImageHash = null,
            MainDef = mainDef,
            AdditionalDefs = additionalDefs,
            StartMap = null,
            Executables = executables,
            Options = null,
            MainCon = null,
            AdditionalCons = null,
            RTS = null
        };
    }

    private static LooseMap CreateLooseMap()
    {
        return new LooseMap
        {
            AddonId = new AddonId("test_map"),
            Type = AddonTypeEnum.Map,
            SupportedGame = new GameInfo(GameEnum.Duke3D),
            Title = "Test Map",
            Author = null,
            ReleaseDate = null,
            Description = null,
            RequiredFeatures = null,
            FileInfo = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            GridImageHash = null,
            PreviewImageHash = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            Executables = null,
            Options = null,
            BloodIni = null
        };
    }


    private sealed class TestGame : BaseGame
    {
        public TestGame(GameEnum gameEnum)
        {
            GameEnum = gameEnum;
        }

        public override GameEnum GameEnum { get; }

        public override string FullName => "TestGame";
        public override string ShortName => "TestGame";
        public override List<string> RequiredFiles => [];
        public override Enum? Skills => null;
    }


    private sealed class TestPort : BasePort
    {
        private List<FeatureEnum> _supportedFeatures = [];
        private List<GameEnum> _supportedGames = [];
        private List<string> _supportedGamesVersions = [];

        public TestPort(PortEnum portEnum, bool isInstalled = true)
        {
            PortEnum = portEnum;
            IsInstalled = isInstalled;
        }

        public override PortEnum PortEnum { get; }

        public override string Name => "TestPort";
        public override string ShortName => "TestPort";
        protected override string WinExe => "test.exe";
        protected override string LinExe => "test";
        public override List<GameEnum> SupportedGames => _supportedGames;
        public override List<FeatureEnum> SupportedFeatures => _supportedFeatures;
        public override List<string> SupportedGamesVersions => _supportedGamesVersions;
        public override bool IsInstalled { get; }
        public override bool IsSkillSelectionAvailable => false;
        public override string? InstalledVersion => IsInstalled ? "1.0" : null;
        protected override string ConfigFile => "";
        protected override string AddDirectoryParam => "";
        protected override string MainGrpParam => "";
        protected override string AddGrpParam => "";
        protected override string AddFileParam => "";
        protected override string AddDefParam => "";
        protected override string AddConParam => "";
        protected override string MainDefParam => "";
        protected override string MainConParam => "";
        protected override string SkillParam => "";
        protected override string AddGameDirParam => "";
        protected override string AddRffParam => throw new NotSupportedException();
        protected override string AddSndParam => throw new NotSupportedException();

        public override void AfterEnd(BaseGame game, BaseAddon campaign) { }
        public override void BeforeStart(BaseGame game, BaseAddon campaign) { }
        protected override void GetStartCampaignArgs(StringBuilder sb, BaseGame game, BaseAddon addon) { }
        protected override void GetSkipIntroParameter(StringBuilder sb) { }
        protected override void GetSkipStartupParameter(StringBuilder sb) { }

        public void SetSupportedGames(List<GameEnum> games) => _supportedGames = games;
        public void SetSupportedFeatures(List<FeatureEnum> features) => _supportedFeatures = features;
        public void SetSupportedGamesVersions(List<string> versions) => _supportedGamesVersions = versions;
    }
}
