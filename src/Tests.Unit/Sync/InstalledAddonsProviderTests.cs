using Addons.Addons;
using Addons.Providers;
using Core.All;
using Core.All.Enums;
using Core.All.Serializable.Addon;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Games.Games;
using Moq;
using Tests.Unit.Helpers;
using StandaloneGame = Games.Games.StandaloneGame;

namespace Tests.Unit.Sync;

[Collection("Sync")]
public sealed class InstalledAddonsProviderTests : IDisposable
{
    private readonly Mock<IConfigProvider> _configMock;
    private readonly HashSet<string> _disabledMods;
    private readonly HashSet<AddonId> _favorites;
    private readonly DukeGame _game;
    private readonly InstalledAddonsProvider _installedAddonsProvider;
    private readonly LocalFilesProvider _localFilesProvider;

    public InstalledAddonsProviderTests()
    {
        _game = new DukeGame
        {
            Duke64RomPath = null,
            DukeZHRomPath = null,
            DukeWTInstallPath = null,
            GameInstallFolder = null
        };

        _disabledMods = [];
        _favorites = [];
        _configMock = new Mock<IConfigProvider>();
        _configMock.Setup(x => x.DisabledAutoloadMods).Returns(_disabledMods);
        _configMock.Setup(x => x.FavoriteAddons).Returns(_favorites);

        (_installedAddonsProvider, _localFilesProvider) = ObjectCreationHelper.CreateInstalledAddonsProvider(_game, _configMock.Object);
    }

    public void Dispose()
    {
        _installedAddonsProvider.Dispose();
    }

    [Fact]
    public void AddAddon_Campaign_AddsToCampaignsCache()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("test-camp", "Test Campaign", "1.0", AddonTypeEnum.TC);

        _installedAddonsProvider.AddAddon(parsed);

        var campaigns = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        Assert.Contains(campaigns, c => c.AddonId.Id == "test-camp");
    }

    [Fact]
    public void AddAddon_Map_AddsToMapsCache()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("test-map", "Test Map", "1.0", AddonTypeEnum.Map);

        _installedAddonsProvider.AddAddon(parsed);

        var maps = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Map);
        Assert.Contains(maps, m => m.AddonId.Id == "test-map");
    }

    [Fact]
    public void AddAddon_Mod_AddsToModsCache()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedModFile("test-mod", "Test Mod", "1.0");

        _installedAddonsProvider.AddAddon(parsed);

        var mods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        Assert.Contains(mods, m => m.AddonId.Id == "test-mod");
    }

    [Fact]
    public void AddAddon_ModIsEnabledByDefault_WhenNotInDisabledList()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedModFile("test-mod", "Test Mod", "1.0");

        _installedAddonsProvider.AddAddon(parsed);

        var mods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        var mod = mods.OfType<AutoloadMod>().First(m => m.AddonId.Id == "test-mod");
        Assert.True(mod.IsEnabled);
    }

    [Fact]
    public void AddAddon_ModIsDisabled_WhenInDisabledList()
    {
        _disabledMods.Add("test-mod");
        var parsed = ParsedAddonFileHelper.CreateParsedModFile("test-mod", "Test Mod", "1.0");

        _installedAddonsProvider.AddAddon(parsed);

        var mods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        var mod = mods.OfType<AutoloadMod>().First(m => m.AddonId.Id == "test-mod");
        Assert.False(mod.IsEnabled);
    }

    [Fact]
    public void AddAddon_ModWithMainDef_Throws()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedModFile("test-mod", "Test Mod", "1.0");
        parsed.Manifest.MainDef = "GAME.CON";

        Assert.Throws<ArgumentException>(() => _installedAddonsProvider.AddAddon(parsed));
    }

    [Fact]
    public void AddAddon_FiresAddonsChangedEvent()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("test", "Test", "1.0", AddonTypeEnum.TC);
        GameEnum? firedGame = null;
        AddonTypeEnum? firedType = null;

        _installedAddonsProvider.AddonsChangedEvent += (g, t) =>
        {
            firedGame = g;
            firedType = t;
        };

        _installedAddonsProvider.AddAddon(parsed);

        Assert.Equal(GameEnum.Duke3D, firedGame);
        Assert.Equal(AddonTypeEnum.TC, firedType);
    }

    [Fact]
    public void AddAddon_AddsFavorite_WhenInFavoritesList()
    {
        AddonId favId = new("fav-camp", "1.0");
        _favorites.Add(favId);
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("fav-camp", "Fav", "1.0", AddonTypeEnum.TC);

        _installedAddonsProvider.AddAddon(parsed);

        var campaigns = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        var camp = campaigns.First(c => c.AddonId.Id == "fav-camp");
        Assert.True(camp.IsFavorite);
    }

    [Fact]
    public void GetInstalledAddonsByType_UnknownType_Throws()
    {
        Assert.Throws<NotSupportedException>(() =>
                                                 _installedAddonsProvider.GetInstalledAddonsByType((AddonTypeEnum)99));
    }

    [Fact]
    public void GetInstalledCampaigns_IncludesCustomCampaigns()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("custom", "ZZ Custom", "1.0", AddonTypeEnum.TC);
        _installedAddonsProvider.AddAddon(parsed);

        var campaigns = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);

        Assert.Contains(campaigns, c => c.AddonId.Id == "custom");
    }

    [Fact]
    public void GetInstalledCampaigns_CustomCampaignsAreSortedByTitle()
    {
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedAddonFile("camp-b", "B Camp", "1.0", AddonTypeEnum.TC));
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedAddonFile("camp-a", "A Camp", "1.0", AddonTypeEnum.TC));

        var campaigns = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);

        var titles = campaigns.Where(c => c.AddonId.Id is "camp-a" or "camp-b")
                              .Select(c => c.Title)
                              .ToList();

        Assert.Equal(["A Camp", "B Camp"], titles);
    }

    [Fact]
    public void EnableAddon_NonExistentMod_DoesNothing()
    {
        _installedAddonsProvider.EnableAddon(new AddonId("ghost", "1.0"));

        _configMock.Verify(x => x.ChangeModState(It.IsAny<AddonId>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void EnableAddon_AlreadyEnabled_DoesNothing()
    {
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("enabled-mod", "Enabled", "1.0"));

        _installedAddonsProvider.EnableAddon(new AddonId("enabled-mod", "1.0"));

        _configMock.Verify(x => x.ChangeModState(It.IsAny<AddonId>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void EnableAddon_DisabledMod_EnablesIt()
    {
        _disabledMods.Add("disabled-mod");
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("disabled-mod", "Disabled", "1.0"));

        _installedAddonsProvider.EnableAddon(new AddonId("disabled-mod", "1.0"));

        _configMock.Verify(x => x.ChangeModState(new AddonId("disabled-mod", "1.0"), true), Times.Once);
        var mods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        var mod = mods.OfType<AutoloadMod>().First(m => m.AddonId.Id == "disabled-mod");
        Assert.True(mod.IsEnabled);
    }

    [Fact]
    public void DisableAddon_NonExistentMod_DoesNothing()
    {
        _installedAddonsProvider.DisableAddon(new AddonId("ghost", "1.0"));

        _configMock.Verify(x => x.ChangeModState(It.IsAny<AddonId>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void DisableAddon_AlreadyDisabled_DoesNothing()
    {
        _disabledMods.Add("already-disabled");
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("already-disabled", "Disabled", "1.0"));

        _installedAddonsProvider.DisableAddon(new AddonId("already-disabled", "1.0"));

        _configMock.Verify(x => x.ChangeModState(It.IsAny<AddonId>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void DisableAddon_EnabledMod_DisablesIt()
    {
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("enabled-mod", "Enabled", "1.0"));

        _installedAddonsProvider.DisableAddon(new AddonId("enabled-mod", "1.0"));

        _configMock.Verify(x => x.ChangeModState(new AddonId("enabled-mod", "1.0"), false), Times.Once);
        var mods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        var mod = mods.OfType<AutoloadMod>().First(m => m.AddonId.Id == "enabled-mod");
        Assert.False(mod.IsEnabled);
    }

    [Fact]
    public void EnableAddon_CascadesToDependencies()
    {
        _disabledMods.Add("dep-mod");
        _disabledMods.Add("main-mod");

        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("main-mod", "Main", "1.0",
                                                                                    deps: new Dictionary<string, string?>
                                                                                    {
                                                                                        ["dep-mod"] = null
                                                                                    }));

        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("dep-mod", "Dep", "1.0"));

        _installedAddonsProvider.EnableAddon(new AddonId("main-mod", "1.0"));

        _configMock.Verify(x => x.ChangeModState(new AddonId("main-mod", "1.0"), true), Times.Once);
        _configMock.Verify(x => x.ChangeModState(new AddonId("dep-mod", "1.0"), true), Times.Once);
    }

    [Fact]
    public void EnableAddon_DisablesIncompatibleMods()
    {
        _disabledMods.Add("main-mod");

        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("main-mod", "Main", "1.0",
                                                                                    incompatibles: new Dictionary<string, string?>
                                                                                    {
                                                                                        ["incompat-mod"] = null
                                                                                    }));

        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("incompat-mod", "Incompat", "1.0"));

        _installedAddonsProvider.EnableAddon(new AddonId("main-mod", "1.0"));

        _configMock.Verify(x => x.ChangeModState(new AddonId("incompat-mod", "1.0"), false), Times.Once);
        var mods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        var incompMod = mods.OfType<AutoloadMod>().First(m => m.AddonId.Id == "incompat-mod");
        Assert.False(incompMod.IsEnabled);
    }

    [Fact]
    public void DisableAddon_CascadesToDependants()
    {
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("dep-mod", "Dep", "1.0",
                                                                                    deps: new Dictionary<string, string?>
                                                                                    {
                                                                                        ["main-mod"] = null
                                                                                    }));

        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("main-mod", "Main", "1.0"));

        _installedAddonsProvider.DisableAddon(new AddonId("main-mod", "1.0"));

        _configMock.Verify(x => x.ChangeModState(new AddonId("main-mod", "1.0"), false), Times.AtLeastOnce);
        var mods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        var depMod = mods.OfType<AutoloadMod>().First(m => m.AddonId.Id == "dep-mod");
        Assert.False(depMod.IsEnabled);
    }

    [Fact]
    public void EnableAddon_DisablesOtherVersionsOfSameMod()
    {
        _disabledMods.Add("my-mod");
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("my-mod", "MyMod v1", "1.0"));
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("my-mod", "MyMod v2", "2.0"));

        _installedAddonsProvider.EnableAddon(new AddonId("my-mod", "1.0"));

        var mods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        var v1 = mods.OfType<AutoloadMod>().First(m => m.AddonId.Version == "1.0");
        var v2 = mods.OfType<AutoloadMod>().First(m => m.AddonId.Version == "2.0");
        Assert.True(v1.IsEnabled);
        Assert.False(v2.IsEnabled);
    }

    [Fact]
    public void GetInstalledAddonsByType_Maps_ReturnsEmptyList_WhenNoMaps()
    {
        var maps = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Map);

        Assert.Empty(maps);
    }

    [Fact]
    public void GetInstalledAddonsByType_Mods_ReturnsEmptyList_WhenNoMods()
    {
        var mods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);

        Assert.Empty(mods);
    }

    [Fact]
    public async Task GetInstalledAddonsByType_Mods_DoesNotContainLooseMaps()
    {
        var mapDir = _game.MapsFolderPath;
        Directory.CreateDirectory(mapDir);

        try
        {
            var mapPath = Path.Combine(mapDir, "test.map");
            await File.WriteAllTextAsync(mapPath, "map content");

            await _localFilesProvider.InitializeAsync();
            await _installedAddonsProvider.CreateCacheAsync(true, AddonTypeEnum.Mod);

            var mods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
            Assert.DoesNotContain(mods, m => m is LooseMap);
        }
        finally
        {
            if (Directory.Exists(mapDir))
            {
                Directory.Delete(mapDir, true);
            }
        }
    }

    [Fact]
    public async Task FileRemovedEvent_CampaignAddon_RemovesFromCache()
    {
        await _localFilesProvider.InitializeAsync().ConfigureAwait(false);

        var fileInfo = FileCreationHelper.CreateAddonManifestInTempFolder("del-camp", "TC", "Duke3D", "Del Camp", "1.0");
        var addResult = await _localFilesProvider.TryAddFileToCacheAsync(fileInfo.PathToFile, null).ConfigureAwait(false);
        Assert.NotNull(addResult);
        Assert.NotEmpty(addResult);

        await Task.Delay(100);
        var before = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        Assert.Contains(before, c => c.AddonId.Id == "del-camp");

        await _localFilesProvider.TryRemoveFileFromCacheAsync(fileInfo.PathToFolder);

        await Task.Delay(100);
        var after = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        Assert.DoesNotContain(after, c => c.AddonId.Id == "del-camp");
    }

    [Fact]
    public async Task FileRemovedEvent_ModAddon_RemovesFromCache()
    {
        await _localFilesProvider.InitializeAsync().ConfigureAwait(false);

        var fileInfo = FileCreationHelper.CreateAddonManifestInTempFolder("del-mod", "Mod", "Duke3D", "Del Mod", "1.0");
        var addResult = await _localFilesProvider.TryAddFileToCacheAsync(fileInfo.PathToFile, null).ConfigureAwait(false);
        Assert.NotNull(addResult);
        Assert.NotEmpty(addResult);

        await Task.Delay(100);
        var before = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        Assert.Contains(before, m => m.AddonId.Id == "del-mod");

        await _localFilesProvider.TryRemoveFileFromCacheAsync(fileInfo.PathToFolder);

        await Task.Delay(100);
        var after = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        Assert.DoesNotContain(after, m => m.AddonId.Id == "del-mod");
    }

    [Fact]
    public async Task FileRemovedEvent_Map_RemovesFromCache()
    {
        await _localFilesProvider.InitializeAsync().ConfigureAwait(false);

        var fileInfo = FileCreationHelper.CreateAddonManifestInTempFolder("del-map", "Map", "Duke3D", "Del Map", "1.0");
        var addResult = await _localFilesProvider.TryAddFileToCacheAsync(fileInfo.PathToFile, null).ConfigureAwait(false);
        Assert.NotNull(addResult);
        Assert.NotEmpty(addResult);

        await Task.Delay(100);
        var before = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Map);
        Assert.Contains(before, m => m.AddonId.Id == "del-map");

        await _localFilesProvider.TryRemoveFileFromCacheAsync(fileInfo.PathToFolder);

        await Task.Delay(100);
        var after = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Map);
        Assert.DoesNotContain(after, m => m.AddonId.Id == "del-map");
    }

    [Fact]
    public void FileRemovedEvent_FiresAddonsChangedEvent()
    {
        var fileInfo = FileCreationHelper.CreateFileInTempDir();

        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("del-camp", "Del", "1.0", AddonTypeEnum.TC) with
        {
            FileInfo = fileInfo
        };

        var jsonPath = Path.Combine(fileInfo.PathToFolder, "addon.json");

        File.WriteAllText(jsonPath, """
                          {
                              "id": "del-camp",
                              "type": "TC",
                              "game": { "name": "Duke3D" },
                              "title": "Del",
                              "version": "1.0"
                          }
                          """);

        GameEnum? firedGame = null;
        AddonTypeEnum? firedType = null;

        _installedAddonsProvider.AddonsChangedEvent += (g, t) =>
        {
            firedGame = g;
            firedType = t;
        };

        _installedAddonsProvider.AddAddon(parsed);
        _installedAddonsProvider.DeleteAddon(parsed);

        Assert.Equal(GameEnum.Duke3D, firedGame);
        Assert.Equal(AddonTypeEnum.TC, firedType);
    }

    [Fact]
    public void GetAddonFromFile_JsonManifest_ReturnsDukeCampaign()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("duke-camp", "Duke Camp", "1.0", AddonTypeEnum.TC);

        var addon = _installedAddonsProvider.GetAddonFromFile(parsed);

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
        var deps = new Dictionary<string, string?>
        {
            ["dep-addon"] = "1.0"
        };

        var incomps = new Dictionary<string, string?>
        {
            ["bad-addon"] = "2.0"
        };

        var parsed = ParsedAddonFileHelper.CreateParsedModFile("test-mod", "Test Mod", "1.0", deps: deps, incompatibles: incomps);

        var addon = _installedAddonsProvider.GetAddonFromFile(parsed);

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
            SupportedGame = _game.GameEnum,
            Manifest = new AddonManifestJsonModel
            {
                Id = "exe-addon",
                Title = "Executable Addon",
                Version = "1.0",
                AddonType = AddonTypeEnum.TC,
                SupportedGame = new SupportedGameJsonModel
                {
                    Game = GameEnum.Duke3D
                },
                Executables = new Dictionary<OSEnum, Dictionary<PortEnum, string>>
                {
                    [OSEnum.Windows] = new()
                    {
                        [PortEnum.EDuke32] = "custom.exe"
                    }
                }
            },
            GridHash = null,
            PreviewHash = null
        };

        var addon = _installedAddonsProvider.GetAddonFromFile(parsed);

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
            SupportedGame = _game.GameEnum,
            Manifest = new AddonManifestJsonModel
            {
                Id = "opt-addon",
                Title = "Options Addon",
                Version = "1.0",
                AddonType = AddonTypeEnum.TC,
                SupportedGame = new SupportedGameJsonModel
                {
                    Game = GameEnum.Duke3D
                },
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
            PreviewHash = null
        };

        var addon = _installedAddonsProvider.GetAddonFromFile(parsed);

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
            SupportedGame = _game.GameEnum,
            Manifest = new AddonManifestJsonModel
            {
                Id = "con-addon",
                Title = "Con Addon",
                Version = "1.0",
                AddonType = AddonTypeEnum.TC,
                SupportedGame = new SupportedGameJsonModel
                {
                    Game = GameEnum.Duke3D
                },
                AdditionalCons = ["extra.con", "more.con"]
            },
            GridHash = null,
            PreviewHash = null
        };

        var addon = _installedAddonsProvider.GetAddonFromFile(parsed);

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
            SupportedGame = _game.GameEnum,
            Manifest = new AddonManifestJsonModel
            {
                Id = "prev-addon",
                Title = "Preview Addon",
                Version = "1.0",
                AddonType = AddonTypeEnum.TC,
                SupportedGame = new SupportedGameJsonModel
                {
                    Game = GameEnum.Duke3D
                }
            },
            GridHash = null,
            PreviewHash = 67890
        };

        var addon = _installedAddonsProvider.GetAddonFromFile(parsed);

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
            SupportedGame = _game.GameEnum,
            Manifest = new AddonManifestJsonModel
            {
                Id = "bad",
                Title = "Bad",
                Version = "1.0",
                AddonType = AddonTypeEnum.TC,
                SupportedGame = new SupportedGameJsonModel
                {
                    Game = GameEnum.Duke3D
                }
            },
            GridHash = null,
            PreviewHash = null
        };

        var addon = _installedAddonsProvider.GetAddonFromFile(parsed);

        Assert.Null(addon);
    }

    [Fact]
    public void GetInstalledCampaigns_WangCustomOrder_TwinDragonFirst()
    {
        WangGame wangGame = new();
        var (wangProvider, _) = ObjectCreationHelper.CreateInstalledAddonsProvider(wangGame, _configMock.Object);

        wangProvider.AddAddon(ParsedAddonFileHelper.CreateParsedAddonFile("Wanton", "Wanton", "1.0", AddonTypeEnum.TC, GameEnum.Wang));
        wangProvider.AddAddon(ParsedAddonFileHelper.CreateParsedAddonFile("TwinDragon", "TwinDragon", "1.0", AddonTypeEnum.TC, GameEnum.Wang));
        wangProvider.AddAddon(ParsedAddonFileHelper.CreateParsedAddonFile("Other", "Aaa Other", "1.0", AddonTypeEnum.TC, GameEnum.Wang));

        var wangCampaigns = wangProvider.GetInstalledAddonsByType(AddonTypeEnum.TC)
                                        .Where(c => c.SupportedGame.GameEnum == GameEnum.Wang)
                                        .ToList();

        var tdIndex = wangCampaigns.FindIndex(c => c.AddonId.Id == "TwinDragon");
        var wantonIndex = wangCampaigns.FindIndex(c => c.AddonId.Id == "Wanton");
        var otherIndex = wangCampaigns.FindIndex(c => c.AddonId.Id == "Other");

        Assert.True(tdIndex >= 0 && wantonIndex >= 0 && otherIndex >= 0);
        Assert.True(tdIndex < otherIndex, "TwinDragon should appear before other campaigns");
        Assert.True(wantonIndex < otherIndex, "Wanton should appear before other campaigns");
    }

    [Fact]
    public void AddAddon_OfficialType_Throws()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("official", "Official", "1.0", AddonTypeEnum.Official);

        Assert.Throws<NotSupportedException>(() => _installedAddonsProvider.AddAddon(parsed));
    }

    [Fact]
    public void GetAddonFromFile_NullManifest_Throws()
    {
        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(PathHelper.GetFakePath(), "addon.json"),
            SupportedGame = _game.GameEnum,
            Manifest = null,
            GridHash = null,
            PreviewHash = null
        };

        Assert.Throws<InvalidOperationException>(() => _installedAddonsProvider.GetAddonFromFile(parsed));
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
    public void EnableAddon_WithMissingDependency_DoesNotThrow()
    {
        _disabledMods.Add("main-mod");

        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("main-mod", "Main", "1.0",
                                                                                    deps: new Dictionary<string, string?>
                                                                                    {
                                                                                        ["missing-dep"] = null
                                                                                    }));

        _installedAddonsProvider.EnableAddon(new AddonId("main-mod", "1.0"));

        _configMock.Verify(x => x.ChangeModState(new AddonId("main-mod", "1.0"), true), Times.Once);
    }

    [Fact]
    public void DisableAddon_TransitiveCascade_DisablesGrandchild()
    {
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("leaf-mod", "Leaf", "1.0",
                                                                                    deps: new Dictionary<string, string?>
                                                                                    {
                                                                                        ["mid-mod"] = null
                                                                                    }));

        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("mid-mod", "Mid", "1.0",
                                                                                    deps: new Dictionary<string, string?>
                                                                                    {
                                                                                        ["root-mod"] = null
                                                                                    }));

        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("root-mod", "Root", "1.0"));

        _installedAddonsProvider.DisableAddon(new AddonId("root-mod", "1.0"));

        var mods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        Assert.False(mods.OfType<AutoloadMod>().First(m => m.AddonId.Id == "root-mod").IsEnabled);
        Assert.False(mods.OfType<AutoloadMod>().First(m => m.AddonId.Id == "mid-mod").IsEnabled);
        Assert.False(mods.OfType<AutoloadMod>().First(m => m.AddonId.Id == "leaf-mod").IsEnabled);
    }

    [Fact]
    public void AddAddon_LooseMapWithBloodIni_DetectsIniFile()
    {
        var mapsDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(mapsDir);

        try
        {
            var mapFile = Path.Combine(mapsDir, "TEST.MAP");
            var iniFile = Path.Combine(mapsDir, "TEST.INI");
            File.WriteAllText(mapFile, "");
            File.WriteAllText(iniFile, "");

            var fileInfo = new AddonFilePathWrapper(mapsDir, "TEST.MAP");

            var parsed = new ParsedAddonFile
            {
                FileInfo = fileInfo,
                SupportedGame = GameEnum.Duke3D,
                Manifest = null,
                GridHash = null,
                PreviewHash = null
            };

            _installedAddonsProvider.AddAddon(parsed);

            var maps = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Map);
            var map = Assert.Single(maps);
            var looseMap = Assert.IsType<LooseMap>(map);
            Assert.Equal("TEST.ini", looseMap.BloodIni, StringComparer.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(mapsDir))
            {
                Directory.Delete(mapsDir, true);
            }
        }
    }

    [Fact]
    public void AddAddon_LooseMapWithoutBloodIni_DoesNotSetIni()
    {
        var mapsDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(mapsDir);

        try
        {
            var mapFile = Path.Combine(mapsDir, "TEST2.MAP");
            File.WriteAllText(mapFile, "");

            var fileInfo = new AddonFilePathWrapper(mapsDir, "TEST2.MAP");

            var parsed = new ParsedAddonFile
            {
                FileInfo = fileInfo,
                SupportedGame = GameEnum.Duke3D,
                Manifest = null,
                GridHash = null,
                PreviewHash = null
            };

            _installedAddonsProvider.AddAddon(parsed);

            var maps = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Map);
            var map = Assert.Single(maps);
            var looseMap = Assert.IsType<LooseMap>(map);
            Assert.Null(looseMap.BloodIni);
        }
        finally
        {
            if (Directory.Exists(mapsDir))
            {
                Directory.Delete(mapsDir, true);
            }
        }
    }

    [Fact]
    public void EnableAddon_ModWithNullFileInfo_DoesNotThrow()
    {
        _disabledMods.Add("null-file-mod");
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("null-file-mod", "Null File", "1.0"));
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("other-mod", "Other", "1.0"));

        _installedAddonsProvider.EnableAddon(new AddonId("null-file-mod", "1.0"));
    }

    [Fact]
    public void AddAddon_SameCampaignTwice_ReplacesEntry()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("dup-camp", "Original", "1.0", AddonTypeEnum.TC);
        _installedAddonsProvider.AddAddon(parsed);

        var parsed2 = ParsedAddonFileHelper.CreateParsedAddonFile("dup-camp", "Updated", "1.0", AddonTypeEnum.TC);
        _installedAddonsProvider.AddAddon(parsed2);

        var campaigns = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        Assert.Single(campaigns.Where(c => c.AddonId.Id == "dup-camp"));
        Assert.Equal("Updated", campaigns.First(c => c.AddonId.Id == "dup-camp").Title);
    }

    [Fact]
    public async Task FileRemovedEvent_NonExistentFile_DoesNotThrow()
    {
        await _localFilesProvider.InitializeAsync().ConfigureAwait(false);

        var fileInfo = FileCreationHelper.CreateAddonManifestInTempFolder("ghost", "TC", "Duke3D", "Ghost", "1.0");
        var addResult = await _localFilesProvider.TryAddFileToCacheAsync(fileInfo.PathToFile, null).ConfigureAwait(false);
        Assert.NotNull(addResult);
        Assert.NotEmpty(addResult);

        File.Delete(fileInfo.PathToFile);

        await _localFilesProvider.TryRemoveFileFromCacheAsync(fileInfo.PathToFolder);

        await Task.Delay(100);
        var after = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        Assert.DoesNotContain(after, c => c.AddonId.Id == "ghost");
    }

    [Fact]
    public void DeleteAddon_BaseAddon_LooseMapWithBloodIni_DeletesMapAndIni()
    {
        var mapDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(mapDir);

        try
        {
            var mapPath = Path.Combine(mapDir, "test.map");
            var iniPath = Path.Combine(mapDir, "test.ini");
            File.WriteAllText(mapPath, "map content");
            File.WriteAllText(iniPath, "ini content");

            var looseMap = new LooseMap
            {
                AddonId = new("test.map"),
                Type = AddonTypeEnum.Map,
                Title = "Test Map",
                SupportedGame = new(GameEnum.Duke3D),
                FileInfo = new AddonFilePathWrapper(mapDir, "test.map"),
                StartMap = new MapFileJsonModel
                {
                    File = "test.map"
                },
                BloodIni = "test.ini",
                GridImageHash = null,
                Author = null,
                ReleaseDate = null,
                Description = null,
                PreviewImageHash = null,
                MainDef = null,
                AdditionalDefs = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                RequiredFeatures = null,
                Executables = null,
                Options = null
            };

            _installedAddonsProvider.AddAddon(new ParsedAddonFile
            {
                FileInfo = new AddonFilePathWrapper(mapDir, "test.map"),
                SupportedGame = GameEnum.Duke3D,
                Manifest = null,
                GridHash = null,
                PreviewHash = null
            });

            _installedAddonsProvider.DeleteAddon(looseMap);

            Assert.False(File.Exists(mapPath));
            Assert.False(File.Exists(iniPath));
        }
        finally
        {
            if (Directory.Exists(mapDir))
            {
                Directory.Delete(mapDir, true);
            }
        }
    }

    [Fact]
    public void DeleteAddon_BaseAddon_ZipAddon_DeletesFile()
    {
        var zipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
        File.WriteAllText(zipPath, "zip content");

        try
        {
            var addon = new DukeCampaign
            {
                AddonId = new("zip-camp", "1.0"),
                Type = AddonTypeEnum.TC,
                Title = "Zip Camp",
                SupportedGame = new(GameEnum.Duke3D),
                FileInfo = new AddonFilePathWrapper(zipPath, "addon.json"),
                GridImageHash = null,
                Author = null,
                ReleaseDate = null,
                Description = null,
                PreviewImageHash = null,
                StartMap = null,
                MainDef = null,
                AdditionalDefs = null,
                MainCon = null,
                AdditionalCons = null,
                RTS = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                RequiredFeatures = null,
                Executables = null,
                Options = null
            };

            _installedAddonsProvider.DeleteAddon(addon);

            Assert.False(File.Exists(zipPath));
        }
        finally
        {
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }
        }
    }

    [Fact]
    public void DeleteAddon_BaseAddon_FolderAddon_DeletesDirectory()
    {
        var folderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(folderPath);
        File.WriteAllText(Path.Combine(folderPath, "addon.json"), "{}");

        try
        {
            var addon = new DukeCampaign
            {
                AddonId = new("folder-camp", "1.0"),
                Type = AddonTypeEnum.TC,
                Title = "Folder Camp",
                SupportedGame = new(GameEnum.Duke3D),
                FileInfo = new AddonFilePathWrapper(folderPath, "addon.json"),
                GridImageHash = null,
                Author = null,
                ReleaseDate = null,
                Description = null,
                PreviewImageHash = null,
                StartMap = null,
                MainDef = null,
                AdditionalDefs = null,
                MainCon = null,
                AdditionalCons = null,
                RTS = null,
                DependentAddons = null,
                IncompatibleAddons = null,
                RequiredFeatures = null,
                Executables = null,
                Options = null
            };

            _installedAddonsProvider.DeleteAddon(addon);

            Assert.False(Directory.Exists(folderPath));
        }
        finally
        {
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
        }
    }

    [Fact]
    public void DeleteAddon_ParsedAddonFile_NullManifest_Throws()
    {
        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(PathHelper.GetFakePath(), "test.json"),
            SupportedGame = _game.GameEnum,
            Manifest = null,
            GridHash = null,
            PreviewHash = null
        };

        Assert.Throws<InvalidOperationException>(() => _installedAddonsProvider.DeleteAddon(parsed));
    }

    [Fact]
    public async Task CreateCacheAsync_CreateNewFalse_DoesNotClearCache()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedModFile("test-mod", "Test Mod", "1.0");
        _installedAddonsProvider.AddAddon(parsed);

        var before = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        Assert.NotEmpty(before);

        await _installedAddonsProvider.CreateCacheAsync(false, AddonTypeEnum.Mod);

        var after = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        Assert.NotEmpty(after);
    }

    [Fact]
    public void AddAddon_GrpInfoFile_AddsCampaigns()
    {
        var grpInfoDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(grpInfoDir);

        try
        {
            var grpInfoPath = Path.Combine(grpInfoDir, "addons.grpinfo");

            File.WriteAllText(grpInfoPath, """
                              grpinfo
                              {
                                  name       "Grp Campaign"
                                  scriptname "scripts/test.con"
                                  size       1234
                              }
                              """);

            var grpPath = Path.Combine(grpInfoDir, "test.grp");
            File.WriteAllBytes(grpPath, new byte[1234]);

            var parsed = new ParsedAddonFile
            {
                FileInfo = new AddonFilePathWrapper(grpInfoDir, "addons.grpinfo"),
                SupportedGame = GameEnum.Duke3D,
                Manifest = null,
                GridHash = null,
                PreviewHash = null
            };

            _installedAddonsProvider.AddAddon(parsed);

            var campaigns = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
            Assert.Contains(campaigns, c => c.AddonId.Id == "grp_campaign");
            Assert.Contains(campaigns, c => c.Title == "Grp Campaign");
        }
        finally
        {
            if (Directory.Exists(grpInfoDir))
            {
                Directory.Delete(grpInfoDir, true);
            }
        }
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var config = new Mock<IConfigProvider>();
        config.Setup(x => x.DisabledAutoloadMods).Returns([]);
        config.Setup(x => x.FavoriteAddons).Returns([]);

        var (newProvider, _) = ObjectCreationHelper.CreateInstalledAddonsProvider(new DukeGame
        {
            Duke64RomPath = null,
            DukeZHRomPath = null,
            DukeWTInstallPath = null
        }, config.Object);

        var ex = Record.Exception(() => newProvider.Dispose());
        Assert.Null(ex);
    }
}
