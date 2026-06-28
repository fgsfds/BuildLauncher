using Addons.Addons;
using Addons.Providers;
using Core.All;
using Core.All.Enums;
using Core.All.Serializable.Addon;
using Core.Client.Api;
using Core.Client.Cache;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Games.Games;
using Games.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Tests.Unit.Helpers;
using StandaloneGame = Games.Games.StandaloneGame;
using DiHelper = Core.Client.Helpers.DiHelper;

namespace Tests.Unit;

[Collection("Sync")]
public sealed class AddonFactoryTests
{
    private readonly DukeGame _game;
    private readonly Mock<IConfigProvider> _configMock;
    private readonly HashSet<AddonId> _favorites;
    private readonly HashSet<string> _disabledMods;
    private readonly AddonFactory _factory;

    public AddonFactoryTests()
    {
        _game = new DukeGame
        {
            Duke64RomPath = null,
            DukeZHRomPath = null,
            DukeWTInstallPath = null,
            GameInstallFolder = null,
        };

        _favorites = [];
        _disabledMods = [];
        _configMock = new Mock<IConfigProvider>();
        _configMock.Setup(x => x.FavoriteAddons).Returns(_favorites);
        _configMock.Setup(x => x.DisabledAutoloadMods).Returns(_disabledMods);

        Mock<ICacheAdder<Stream>> cacheMock = new();
        ChannelBroadcaster<DiHelper.LocalFileEvent> channel = new();
        Mock<InstalledGamesProvider> gamesMock = new(MockBehavior.Loose);
        gamesMock.Setup(x => x.GetGames()).Returns([_game]);

        var localFilesProvider = new LocalFilesProvider(
            gamesMock.Object,
            cacheMock.Object,
            channel,
            NullLogger<LocalFilesProvider>.Instance
        );

        var metadataProvider = new MetadataProvider(
            localFilesProvider,
            new OfflineApiInterface(NullLogger<OfflineApiInterface>.Instance),
            NullLogger<MetadataProvider>.Instance
        );

        _factory = new AddonFactory(_game, _configMock.Object, metadataProvider);
    }

    [Fact]
    public void GetAddonFromFile_JsonManifest_ReturnsDukeCampaign()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("duke-camp", "Duke Camp", "1.0", AddonTypeEnum.TC);

        var addon = _factory.GetAddonFromFile(parsed);

        Assert.NotNull(addon);
        Assert.IsType<DukeCampaign>(addon);
        Assert.Equal("duke-camp", addon.AddonId.Id);
        Assert.Equal("1.0", addon.AddonId.Version);
        Assert.Equal("Duke Camp", addon.Title);
        Assert.Equal(AddonTypeEnum.TC, addon.Type);
    }

    [Fact]
    public void GetAddonFromFile_JsonManifestWithDependencies_AddsThem()
    {
        var deps = new Dictionary<string, string?> { ["dep-addon"] = "1.0" };
        var incomps = new Dictionary<string, string?> { ["bad-addon"] = "2.0" };
        var parsed = ParsedAddonFileHelper.CreateParsedModFile("test-mod", "Test Mod", "1.0", deps: deps, incompatibles: incomps);

        var addon = _factory.GetAddonFromFile(parsed);

        Assert.NotNull(addon);
        Assert.NotNull(addon.DependentAddons);
        Assert.True(addon.DependentAddons.ContainsKey("dep-addon"));
        Assert.NotNull(addon.IncompatibleAddons);
        Assert.True(addon.IncompatibleAddons.ContainsKey("bad-addon"));
    }

    [Fact]
    public void GetAddonFromFile_JsonManifestWithExecutables_AddsThem()
    {
        var folder = PathHelper.GetFakePath();
        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(folder, "addon.json"),
            SupportedGame = GameEnum.Duke3D,
            Manifest = new AddonManifestJsonModel
            {
                Id = "exe-addon",
                Title = "Executable Addon",
                Version = "1.0",
                AddonType = AddonTypeEnum.TC,
                SupportedGame = new SupportedGameJsonModel { Game = GameEnum.Duke3D },
                Executables = new Dictionary<OSEnum, Dictionary<PortEnum, string>>
                {
                    [OSEnum.Windows] = new()
                    {
                        [PortEnum.EDuke32] = "custom.exe"
                    }
                }
            },
            GridHash = null,
            PreviewHash = null,
        };

        var addon = _factory.GetAddonFromFile(parsed);

        Assert.NotNull(addon);
        Assert.NotNull(addon.Executables);
        Assert.Contains(OSEnum.Windows, addon.Executables);
        Assert.Contains(PortEnum.EDuke32, addon.Executables[OSEnum.Windows]);
        Assert.EndsWith("custom.exe", addon.Executables[OSEnum.Windows][PortEnum.EDuke32]);
    }

    [Fact]
    public void GetAddonFromFile_JsonManifestWithOptions_AddsThem()
    {
        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(PathHelper.GetFakePath(), "addon.json"),
            SupportedGame = GameEnum.Duke3D,
            Manifest = new AddonManifestJsonModel
            {
                Id = "opt-addon",
                Title = "Options Addon",
                Version = "1.0",
                AddonType = AddonTypeEnum.TC,
                SupportedGame = new SupportedGameJsonModel { Game = GameEnum.Duke3D },
                Options = new List<OptionJsonModel>
                {
                    new()
                    {
                        OptionName = "opt1",
                        Parameters = new Dictionary<string, OptionalParameterTypeEnum>
                        {
                            ["test.def"] = OptionalParameterTypeEnum.DEF
                        }
                    }
                }
            },
            GridHash = null,
            PreviewHash = null,
        };

        var addon = _factory.GetAddonFromFile(parsed);

        Assert.NotNull(addon);
        Assert.NotNull(addon.Options);
        Assert.Contains("opt1", addon.Options);
        Assert.Contains("test.def", addon.Options["opt1"]);
        Assert.Equal(OptionalParameterTypeEnum.DEF, addon.Options["opt1"]["test.def"]);
    }

    [Fact]
    public void GetAddonFromFile_JsonManifestWithAddCons_AddsThem()
    {
        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(PathHelper.GetFakePath(), "addon.json"),
            SupportedGame = GameEnum.Duke3D,
            Manifest = new AddonManifestJsonModel
            {
                Id = "con-addon",
                Title = "Con Addon",
                Version = "1.0",
                AddonType = AddonTypeEnum.TC,
                SupportedGame = new SupportedGameJsonModel { Game = GameEnum.Duke3D },
                AdditionalCons = ["extra.con", "more.con"],
            },
            GridHash = null,
            PreviewHash = null,
        };

        var addon = _factory.GetAddonFromFile(parsed);

        Assert.NotNull(addon);
        var duke = Assert.IsType<DukeCampaign>(addon);
        Assert.NotNull(duke.AdditionalCons);
        Assert.Contains("extra.con", duke.AdditionalCons);
        Assert.Contains("more.con", duke.AdditionalCons);
    }

    [Fact]
    public void GetAddonFromFile_JsonManifest_FallsBackToPreviewForGrid()
    {
        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(PathHelper.GetFakePath(), "addon.json"),
            SupportedGame = GameEnum.Duke3D,
            Manifest = new AddonManifestJsonModel
            {
                Id = "prev-addon",
                Title = "Preview Addon",
                Version = "1.0",
                AddonType = AddonTypeEnum.TC,
                SupportedGame = new SupportedGameJsonModel { Game = GameEnum.Duke3D },
            },
            GridHash = null,
            PreviewHash = 67890,
        };

        var addon = _factory.GetAddonFromFile(parsed);

        Assert.NotNull(addon);
        Assert.Equal(67890, addon.GridImageHash);
        Assert.Equal(67890, addon.PreviewImageHash);
    }

    [Fact]
    public void GetAddonFromFile_NotJsonNotMapNotZip_ReturnsNull()
    {
        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), "addon.xyz"),
            SupportedGame = GameEnum.Duke3D,
            Manifest = new AddonManifestJsonModel
            {
                Id = "bad",
                Title = "Bad",
                Version = "1.0",
                AddonType = AddonTypeEnum.TC,
                SupportedGame = new SupportedGameJsonModel { Game = GameEnum.Duke3D },
            },
            GridHash = null,
            PreviewHash = null,
        };

        var addon = _factory.GetAddonFromFile(parsed);

        Assert.Null(addon);
    }

    [Fact]
    public void GetAddonFromFile_NullManifest_Throws()
    {
        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(PathHelper.GetFakePath(), "addon.json"),
            SupportedGame = GameEnum.Duke3D,
            Manifest = null,
            GridHash = null,
            PreviewHash = null,
        };

        Assert.Throws<InvalidOperationException>(() => _factory.GetAddonFromFile(parsed));
    }

    [Fact]
    public void GetAddonFromFile_BloodGame_ReturnsBloodCampaign()
    {
        BloodGame bloodGame = new();
        var (bloodProvider, _) = ObjectCreationHelper.CreateInstalledAddonsProvider(bloodGame, _configMock.Object);
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("blood-camp", "Blood Camp", "1.0", AddonTypeEnum.TC, GameEnum.Blood);

        bloodProvider.AddAddon(parsed);

        var campaigns = bloodProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        var addon = campaigns.First(c => c.AddonId.Id == "blood-camp");
        Assert.IsType<BloodCampaign>(addon);
    }

    [Fact]
    public void GetAddonFromFile_StandaloneGame_ReturnsStandaloneGame()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("standalone", "Standalone", "1.0", AddonTypeEnum.TC, GameEnum.Standalone);
        StandaloneGame standaloneGame = new();
        var (standaloneProvider, _) = ObjectCreationHelper.CreateInstalledAddonsProvider(standaloneGame, _configMock.Object);

        standaloneProvider.AddAddon(parsed);

        var campaigns = standaloneProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        var addon = campaigns.First(c => c.AddonId.Id == "standalone");
        Assert.IsType<Addons.Addons.StandaloneGame>(addon);
    }

    [Fact]
    public void GetAddonFromFile_SlaveGame_ReturnsGenericCampaign()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("slave-camp", "Slave Camp", "1.0", AddonTypeEnum.TC, GameEnum.Slave);
        var slaveGame = new SlaveGame();
        var (slaveProvider, _) = ObjectCreationHelper.CreateInstalledAddonsProvider(slaveGame, _configMock.Object);

        slaveProvider.AddAddon(parsed);

        var campaigns = slaveProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        var addon = campaigns.First(c => c.AddonId.Id == "slave-camp");
        Assert.IsType<GenericCampaign>(addon);
    }

    [Fact]
    public void GetAddonFromFile_Mod_ReturnsAutoloadMod()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedModFile("test-mod", "Test Mod", "1.0");

        var addon = _factory.GetAddonFromFile(parsed);

        Assert.NotNull(addon);
        Assert.IsType<AutoloadMod>(addon);
        Assert.Equal("test-mod", addon.AddonId.Id);
        Assert.Equal(AddonTypeEnum.Mod, addon.Type);
        Assert.Null(addon.MainDef);
    }

    [Fact]
    public void GetAddonFromFile_ModWithMainDef_Throws()
    {
        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(PathHelper.GetFakePath(), "addon.json"),
            SupportedGame = GameEnum.Duke3D,
            Manifest = new AddonManifestJsonModel
            {
                Id = "bad-mod",
                Title = "Bad Mod",
                Version = "1.0",
                AddonType = AddonTypeEnum.Mod,
                MainDef = "test.def",
                SupportedGame = new SupportedGameJsonModel { Game = GameEnum.Duke3D },
            },
            GridHash = null,
            PreviewHash = null,
        };

        Assert.Throws<ArgumentException>(() => _factory.GetAddonFromFile(parsed));
    }

    [Fact]
    public void GetAddonFromFile_DisabledMod_SetsIsEnabledFalse()
    {
        _disabledMods.Add("disabled-mod");
        var parsed = ParsedAddonFileHelper.CreateParsedModFile("disabled-mod", "Disabled", "1.0");

        var addon = _factory.GetAddonFromFile(parsed);

        Assert.NotNull(addon);
        var autoloadMod = Assert.IsType<AutoloadMod>(addon);
        Assert.False(autoloadMod.IsEnabled);
    }

    [Fact]
    public void GetLooseMapFromFile_ValidMap_ReturnsLooseMap()
    {
        var mapFolder = PathHelper.GetFakePath();
        Directory.CreateDirectory(mapFolder);
        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(mapFolder, "test.map"),
            SupportedGame = GameEnum.Duke3D,
            Manifest = null,
            GridHash = null,
            PreviewHash = null,
        };

        var addon = _factory.GetLooseMapFromFile(parsed);

        Assert.NotNull(addon);
        var looseMap = Assert.IsType<LooseMap>(addon);
        Assert.Equal("test.map", looseMap.AddonId.Id);
        Assert.Equal(AddonTypeEnum.Map, looseMap.Type);
        Assert.Equal("test.map", looseMap.Title);
        Assert.Null(looseMap.BloodIni);
    }

    [Fact]
    public void GetLooseMapFromFile_NonMap_ReturnsNull()
    {
        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(PathHelper.GetFakePath(), "addon.json"),
            SupportedGame = GameEnum.Duke3D,
            Manifest = null,
            GridHash = null,
            PreviewHash = null,
        };

        var addon = _factory.GetLooseMapFromFile(parsed);

        Assert.Null(addon);
    }
}
