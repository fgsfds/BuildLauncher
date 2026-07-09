using System.Reflection;
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

/// <summary>
///     Tests for the <see cref="InstalledAddonsProvider" /> class.
/// </summary>
[Collection("Sync")]
public sealed class InstalledAddonsProviderTests : IDisposable
{
    /// <summary>
    ///     Mock configuration provider.
    /// </summary>
    private readonly Mock<IConfigProvider> _configMock;

    /// <summary>
    ///     Set of disabled mod identifiers.
    /// </summary>
    private readonly HashSet<string> _disabledMods;

    /// <summary>
    ///     Set of favorite addon identifiers.
    /// </summary>
    private readonly HashSet<AddonId> _favorites;

    /// <summary>
    ///     Duke Nukem 3D game instance.
    /// </summary>
    private readonly DukeGame _game;

    /// <summary>
    ///     Installed addons provider under test.
    /// </summary>
    private readonly InstalledAddonsProvider _installedAddonsProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstalledAddonsProviderTests" /> class.
    /// </summary>
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

        _installedAddonsProvider = ObjectCreationHelper.CreateInstalledAddonsProvider(_game, _configMock.Object);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _installedAddonsProvider.Dispose();
    }

    /// <summary>
    ///     Tests that adding a campaign addon adds it to the campaigns cache.
    /// </summary>
    [Fact]
    public void AddAddon_Campaign_AddsToCampaignsCache()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("test-camp", "Test Campaign", "1.0", AddonTypeEnum.TC);

        _installedAddonsProvider.AddAddon(parsed);

        var campaigns = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        Assert.Contains(campaigns, c => c.AddonId.Id == "test-camp");
    }

    /// <summary>
    ///     Tests that adding a map addon adds it to the maps cache.
    /// </summary>
    [Fact]
    public void AddAddon_Map_AddsToMapsCache()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("test-map", "Test Map", "1.0", AddonTypeEnum.Map);

        _installedAddonsProvider.AddAddon(parsed);

        var maps = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Map);
        Assert.Contains(maps, m => m.AddonId.Id == "test-map");
    }

    /// <summary>
    ///     Tests that adding a mod addon adds it to the mods cache.
    /// </summary>
    [Fact]
    public void AddAddon_Mod_AddsToModsCache()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedModFile("test-mod", "Test Mod", "1.0");

        _installedAddonsProvider.AddAddon(parsed);

        var mods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        Assert.Contains(mods, m => m.AddonId.Id == "test-mod");
    }

    /// <summary>
    ///     Tests that a mod is enabled by default when not in the disabled list.
    /// </summary>
    [Fact]
    public void AddAddon_ModIsEnabledByDefault_WhenNotInDisabledList()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedModFile("test-mod", "Test Mod", "1.0");

        _installedAddonsProvider.AddAddon(parsed);

        var mods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        var mod = mods.OfType<AutoloadMod>().First(m => m.AddonId.Id == "test-mod");
        Assert.True(mod.IsEnabled);
    }

    /// <summary>
    ///     Tests that a mod is disabled when it is in the disabled list.
    /// </summary>
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

    /// <summary>
    ///     Tests that adding a mod with a MainDef set throws an exception.
    /// </summary>
    [Fact]
    public void AddAddon_ModWithMainDef_Throws()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedModFile("test-mod", "Test Mod", "1.0");
        parsed.Manifest.MainDef = "GAME.CON";

        Assert.Throws<ArgumentException>(() => _installedAddonsProvider.AddAddon(parsed));
    }

    /// <summary>
    ///     Tests that adding an addon fires the AddonsChanged event.
    /// </summary>
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

    /// <summary>
    ///     Tests that an addon in the favorites list is marked as favorite.
    /// </summary>
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

    /// <summary>
    ///     Tests that querying an unknown addon type throws a NotSupportedException.
    /// </summary>
    [Fact]
    public void GetInstalledAddonsByType_UnknownType_Throws()
    {
        Assert.Throws<NotSupportedException>(() =>
                                                 _installedAddonsProvider.GetInstalledAddonsByType((AddonTypeEnum)99)
            );
    }

    /// <summary>
    ///     Tests that custom campaigns are included in the installed campaigns list.
    /// </summary>
    [Fact]
    public void GetInstalledCampaigns_IncludesCustomCampaigns()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("custom", "ZZ Custom", "1.0", AddonTypeEnum.TC);
        _installedAddonsProvider.AddAddon(parsed);

        var campaigns = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);

        Assert.Contains(campaigns, c => c.AddonId.Id == "custom");
    }

    /// <summary>
    ///     Tests that custom campaigns are sorted by title.
    /// </summary>
    [Fact]
    public void GetInstalledCampaigns_CustomCampaignsAreSortedByTitle()
    {
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedAddonFile("camp-b", "B Camp", "1.0", AddonTypeEnum.TC));
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedAddonFile("camp-a", "A Camp", "1.0", AddonTypeEnum.TC));

        var campaigns = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);

        var titles = campaigns.Where(c => c.AddonId.Id is "camp-a" or "camp-b")
                              .Select(c => c.Title)
                              .ToList();

        Assert.Equal(
            [
                "A Camp",
                "B Camp"
            ], titles
            );
    }

    /// <summary>
    ///     Tests that enabling a non-existent mod does nothing.
    /// </summary>
    [Fact]
    public void EnableAddon_NonExistentMod_DoesNothing()
    {
        _installedAddonsProvider.EnableAddon(new AddonId("ghost", "1.0"));

        _configMock.Verify(x => x.ChangeModState(It.IsAny<AddonId>(), It.IsAny<bool>()), Times.Never);
    }

    /// <summary>
    ///     Tests that enabling an already enabled mod does nothing.
    /// </summary>
    [Fact]
    public void EnableAddon_AlreadyEnabled_DoesNothing()
    {
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("enabled-mod", "Enabled", "1.0"));

        _installedAddonsProvider.EnableAddon(new AddonId("enabled-mod", "1.0"));

        _configMock.Verify(x => x.ChangeModState(It.IsAny<AddonId>(), It.IsAny<bool>()), Times.Never);
    }

    /// <summary>
    ///     Tests that enabling a disabled mod enables it and updates the config.
    /// </summary>
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

    /// <summary>
    ///     Tests that disabling a non-existent mod does nothing.
    /// </summary>
    [Fact]
    public void DisableAddon_NonExistentMod_DoesNothing()
    {
        _installedAddonsProvider.DisableAddon(new AddonId("ghost", "1.0"));

        _configMock.Verify(x => x.ChangeModState(It.IsAny<AddonId>(), It.IsAny<bool>()), Times.Never);
    }

    /// <summary>
    ///     Tests that disabling an already disabled mod does nothing.
    /// </summary>
    [Fact]
    public void DisableAddon_AlreadyDisabled_DoesNothing()
    {
        _disabledMods.Add("already-disabled");
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("already-disabled", "Disabled", "1.0"));

        _installedAddonsProvider.DisableAddon(new AddonId("already-disabled", "1.0"));

        _configMock.Verify(x => x.ChangeModState(It.IsAny<AddonId>(), It.IsAny<bool>()), Times.Never);
    }

    /// <summary>
    ///     Tests that disabling an enabled mod disables it and updates the config.
    /// </summary>
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

    /// <summary>
    ///     Tests that enabling a mod cascades to its dependencies.
    /// </summary>
    [Fact]
    public void EnableAddon_CascadesToDependencies()
    {
        _disabledMods.Add("dep-mod");
        _disabledMods.Add("main-mod");

        _installedAddonsProvider.AddAddon(
            ParsedAddonFileHelper.CreateParsedModFile(
                "main-mod", "Main", "1.0",
                deps: new Dictionary<string, string?>
                {
                    ["dep-mod"] = null
                }
                )
            );

        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("dep-mod", "Dep", "1.0"));

        _installedAddonsProvider.EnableAddon(new AddonId("main-mod", "1.0"));

        _configMock.Verify(x => x.ChangeModState(new AddonId("main-mod", "1.0"), true), Times.Once);
        _configMock.Verify(x => x.ChangeModState(new AddonId("dep-mod", "1.0"), true), Times.Once);
    }

    /// <summary>
    ///     Tests that enabling a mod disables its incompatible mods.
    /// </summary>
    [Fact]
    public void EnableAddon_DisablesIncompatibleMods()
    {
        _disabledMods.Add("main-mod");

        _installedAddonsProvider.AddAddon(
            ParsedAddonFileHelper.CreateParsedModFile(
                "main-mod", "Main", "1.0",
                incompatibles: new Dictionary<string, string?>
                {
                    ["incompat-mod"] = null
                }
                )
            );

        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("incompat-mod", "Incompat", "1.0"));

        _installedAddonsProvider.EnableAddon(new AddonId("main-mod", "1.0"));

        _configMock.Verify(x => x.ChangeModState(new AddonId("incompat-mod", "1.0"), false), Times.Once);
        var mods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        var incompMod = mods.OfType<AutoloadMod>().First(m => m.AddonId.Id == "incompat-mod");
        Assert.False(incompMod.IsEnabled);
    }

    /// <summary>
    ///     Tests that disabling a mod cascades to its dependants.
    /// </summary>
    [Fact]
    public void DisableAddon_CascadesToDependants()
    {
        _installedAddonsProvider.AddAddon(
            ParsedAddonFileHelper.CreateParsedModFile(
                "dep-mod", "Dep", "1.0",
                deps: new Dictionary<string, string?>
                {
                    ["main-mod"] = null
                }
                )
            );

        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("main-mod", "Main", "1.0"));

        _installedAddonsProvider.DisableAddon(new AddonId("main-mod", "1.0"));

        _configMock.Verify(x => x.ChangeModState(new AddonId("main-mod", "1.0"), false), Times.AtLeastOnce);
        var mods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        var depMod = mods.OfType<AutoloadMod>().First(m => m.AddonId.Id == "dep-mod");
        Assert.False(depMod.IsEnabled);
    }

    /// <summary>
    ///     Tests that enabling a mod disables other versions of the same mod.
    /// </summary>
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

    /// <summary>
    ///     Tests that querying maps when none are installed returns an empty list.
    /// </summary>
    [Fact]
    public void GetInstalledAddonsByType_Maps_ReturnsEmptyList_WhenNoMaps()
    {
        var maps = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Map);

        Assert.Empty(maps);
    }

    /// <summary>
    ///     Tests that querying mods when none are installed returns an empty list.
    /// </summary>
    [Fact]
    public void GetInstalledAddonsByType_Mods_ReturnsEmptyList_WhenNoMods()
    {
        var mods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);

        Assert.Empty(mods);
    }

    /// <summary>
    ///     Tests that the mods list does not contain loose maps.
    /// </summary>
    [Fact]
    public async Task GetInstalledAddonsByType_Mods_DoesNotContainLooseMaps()
    {
        var mapDir = _game.MapsFolderPath;
        Directory.CreateDirectory(mapDir);

        try
        {
            var mapPath = Path.Combine(mapDir, "test.map");
            await File.WriteAllTextAsync(mapPath, "map content");

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
    public void FileRemovedEvent_CampaignAddon_RemovesFromCache()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("del-camp", "Del Camp", "1.0", AddonTypeEnum.TC);
        Directory.CreateDirectory(parsed.FileInfo!.PathToFolder);
        _installedAddonsProvider.AddAddon(parsed);

        var before = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        Assert.Contains(before, c => c.AddonId.Id == "del-camp");

        var addon = before.First(c => c.AddonId.Id == "del-camp");
        _installedAddonsProvider.DeleteAddon(addon);

        var after = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        Assert.DoesNotContain(after, c => c.AddonId.Id == "del-camp");
    }

    [Fact]
    public void FileRemovedEvent_ModAddon_RemovesFromCache()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedModFile("del-mod", "Del Mod", "1.0");
        Directory.CreateDirectory(parsed.FileInfo!.PathToFolder);
        _installedAddonsProvider.AddAddon(parsed);

        var before = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        Assert.Contains(before, m => m.AddonId.Id == "del-mod");

        var addon = before.First(m => m.AddonId.Id == "del-mod");
        _installedAddonsProvider.DeleteAddon(addon);

        var after = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        Assert.DoesNotContain(after, m => m.AddonId.Id == "del-mod");
    }

    [Fact]
    public void FileRemovedEvent_Map_RemovesFromCache()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("del-map", "Del Map", "1.0", AddonTypeEnum.Map);
        Directory.CreateDirectory(parsed.FileInfo!.PathToFolder);
        File.WriteAllText(parsed.FileInfo.PathToFile, "map content");
        _installedAddonsProvider.AddAddon(parsed);

        var before = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Map);
        Assert.Contains(before, m => m.AddonId.Id == "del-map");

        var addon = before.First(m => m.AddonId.Id == "del-map");
        _installedAddonsProvider.DeleteAddon(addon);

        var after = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Map);
        Assert.DoesNotContain(after, m => m.AddonId.Id == "del-map");
    }

    /// <summary>
    ///     Tests that the AddonsChanged event fires when a file is removed.
    /// </summary>
    [Fact]
    public void FileRemovedEvent_FiresAddonsChangedEvent()
    {
        var fileInfo = FileCreationHelper.CreateFileInTempDir();

        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("del-camp", "Del", "1.0", AddonTypeEnum.TC) with
        {
            FileInfo = fileInfo
        };

        var jsonPath = Path.Combine(fileInfo.PathToFolder, "addon.json");

        File.WriteAllText(
            jsonPath, """
            {
                "id": "del-camp",
                "type": "TC",
                "game": { "name": "Duke3D" },
                "title": "Del",
                "version": "1.0"
            }
            """
            );

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

    /// <summary>
    ///     Tests that a JSON manifest returns a DukeCampaign.
    /// </summary>
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

    /// <summary>
    ///     Tests that dependencies from a JSON manifest are correctly added.
    /// </summary>
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

    /// <summary>
    ///     Tests that executables from a JSON manifest are correctly added.
    /// </summary>
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

    /// <summary>
    ///     Tests that options from a JSON manifest are correctly added.
    /// </summary>
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

    /// <summary>
    ///     Tests that additional CON files from a JSON manifest are correctly added.
    /// </summary>
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
                AdditionalCons =
                [
                    "extra.con",
                    "more.con"
                ]
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

    /// <summary>
    ///     Tests that the preview hash is used as a fallback for the grid image hash.
    /// </summary>
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

    /// <summary>
    ///     Tests that unsupported file types return null.
    /// </summary>
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

    /// <summary>
    ///     Tests that TwinDragon and Wanton appear before other campaigns in the custom Wang order.
    /// </summary>
    [Fact]
    public void GetInstalledCampaigns_WangCustomOrder_TwinDragonFirst()
    {
        WangGame wangGame = new();
        var wangProvider = ObjectCreationHelper.CreateInstalledAddonsProvider(wangGame, _configMock.Object);

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

    /// <summary>
    ///     Tests that adding an official type addon throws a NotSupportedException.
    /// </summary>
    [Fact]
    public void AddAddon_OfficialType_Throws()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("official", "Official", "1.0", AddonTypeEnum.Official);

        Assert.Throws<NotSupportedException>(() => _installedAddonsProvider.AddAddon(parsed));
    }

    /// <summary>
    ///     Tests that a null manifest throws an InvalidOperationException.
    /// </summary>
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

    /// <summary>
    ///     Tests that a Blood game addon returns a BloodCampaign.
    /// </summary>
    [Fact]
    public void GetAddonFromFile_BloodGame_ReturnsBloodCampaign()
    {
        BloodGame bloodGame = new();
        var bloodProvider = ObjectCreationHelper.CreateInstalledAddonsProvider(bloodGame, _configMock.Object);

        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("blood-camp", "Blood Camp", "1.0", AddonTypeEnum.TC, GameEnum.Blood);
        bloodProvider.AddAddon(parsed);

        var campaigns = bloodProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        var addon = campaigns.First(c => c.AddonId.Id == "blood-camp");

        Assert.IsType<BloodCampaign>(addon);
    }

    /// <summary>
    ///     Tests that a standalone game addon returns a StandaloneGame instance.
    /// </summary>
    [Fact]
    public void GetAddonFromFile_StandaloneGame_ReturnsStandaloneGame()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("standalone", "Standalone", "1.0", AddonTypeEnum.TC, GameEnum.Standalone);
        StandaloneGame standaloneGame = new();
        var standaloneProvider = ObjectCreationHelper.CreateInstalledAddonsProvider(standaloneGame, _configMock.Object);

        standaloneProvider.AddAddon(parsed);

        var campaigns = standaloneProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        var addon = campaigns.First(c => c.AddonId.Id == "standalone");

        Assert.IsType<Addons.Addons.StandaloneGame>(addon);
    }

    /// <summary>
    ///     Tests that a Slave game addon returns a GenericCampaign.
    /// </summary>
    [Fact]
    public void GetAddonFromFile_SlaveGame_ReturnsGenericCampaign()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("slave-camp", "Slave Camp", "1.0", AddonTypeEnum.TC, GameEnum.Slave);
        var slaveGame = new SlaveGame();
        var slaveProvider = ObjectCreationHelper.CreateInstalledAddonsProvider(slaveGame, _configMock.Object);

        slaveProvider.AddAddon(parsed);

        var campaigns = slaveProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        var addon = campaigns.First(c => c.AddonId.Id == "slave-camp");

        Assert.IsType<GenericCampaign>(addon);
    }

    /// <summary>
    ///     Tests that enabling a mod with a missing dependency does not throw.
    /// </summary>
    [Fact]
    public void EnableAddon_WithMissingDependency_DoesNotThrow()
    {
        _disabledMods.Add("main-mod");

        _installedAddonsProvider.AddAddon(
            ParsedAddonFileHelper.CreateParsedModFile(
                "main-mod", "Main", "1.0",
                deps: new Dictionary<string, string?>
                {
                    ["missing-dep"] = null
                }
                )
            );

        _installedAddonsProvider.EnableAddon(new AddonId("main-mod", "1.0"));

        _configMock.Verify(x => x.ChangeModState(new AddonId("main-mod", "1.0"), true), Times.Once);
    }

    /// <summary>
    ///     Tests that disabling a mod transitively cascades to disable grandchild mods.
    /// </summary>
    [Fact]
    public void DisableAddon_TransitiveCascade_DisablesGrandchild()
    {
        _installedAddonsProvider.AddAddon(
            ParsedAddonFileHelper.CreateParsedModFile(
                "leaf-mod", "Leaf", "1.0",
                deps: new Dictionary<string, string?>
                {
                    ["mid-mod"] = null
                }
                )
            );

        _installedAddonsProvider.AddAddon(
            ParsedAddonFileHelper.CreateParsedModFile(
                "mid-mod", "Mid", "1.0",
                deps: new Dictionary<string, string?>
                {
                    ["root-mod"] = null
                }
                )
            );

        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("root-mod", "Root", "1.0"));

        _installedAddonsProvider.DisableAddon(new AddonId("root-mod", "1.0"));

        var mods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        Assert.False(mods.OfType<AutoloadMod>().First(m => m.AddonId.Id == "root-mod").IsEnabled);
        Assert.False(mods.OfType<AutoloadMod>().First(m => m.AddonId.Id == "mid-mod").IsEnabled);
        Assert.False(mods.OfType<AutoloadMod>().First(m => m.AddonId.Id == "leaf-mod").IsEnabled);
    }

    /// <summary>
    ///     Tests that a loose map with a matching INI file detects it.
    /// </summary>
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

    /// <summary>
    ///     Tests that a loose map without a matching INI file does not set BloodIni.
    /// </summary>
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

    /// <summary>
    ///     Tests that enabling a mod with null FileInfo does not throw.
    /// </summary>
    [Fact]
    public void EnableAddon_ModWithNullFileInfo_DoesNotThrow()
    {
        _disabledMods.Add("null-file-mod");
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("null-file-mod", "Null File", "1.0"));
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedModFile("other-mod", "Other", "1.0"));

        _installedAddonsProvider.EnableAddon(new AddonId("null-file-mod", "1.0"));
    }

    /// <summary>
    ///     Tests that adding the same campaign twice replaces the original entry.
    /// </summary>
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
    public void FileRemovedEvent_NonExistentFile_DoesNotThrow()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("ghost", "Ghost", "1.0", AddonTypeEnum.TC);
        Directory.CreateDirectory(parsed.FileInfo!.PathToFolder);
        File.WriteAllText(parsed.FileInfo.PathToFile, "{}");
        _installedAddonsProvider.AddAddon(parsed);

        File.Delete(parsed.FileInfo.PathToFile);

        var addon = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC)
                                            .First(c => c.AddonId.Id == "ghost");

        var ex = Record.Exception(() => _installedAddonsProvider.DeleteAddon(addon));
        Assert.Null(ex);

        var after = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        Assert.DoesNotContain(after, c => c.AddonId.Id == "ghost");
    }

    /// <summary>
    ///     Tests that deleting a loose map also deletes its associated INI file.
    /// </summary>
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

            _installedAddonsProvider.AddAddon(
                new ParsedAddonFile
                {
                    FileInfo = new AddonFilePathWrapper(mapDir, "test.map"),
                    SupportedGame = GameEnum.Duke3D,
                    Manifest = null,
                    GridHash = null,
                    PreviewHash = null
                }
                );

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

    /// <summary>
    ///     Tests that deleting a zip addon deletes the zip file.
    /// </summary>
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

    /// <summary>
    ///     Tests that deleting a folder addon deletes the directory.
    /// </summary>
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

    /// <summary>
    ///     Tests that deleting a parsed addon file with a null manifest throws.
    /// </summary>
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

    /// <summary>
    ///     Tests that creating a cache with CreateNew set to false does not clear existing entries.
    /// </summary>
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

    /// <summary>
    ///     Tests that adding a grpinfo file adds the associated campaigns.
    /// </summary>
    [Fact]
    public void AddAddon_GrpInfoFile_AddsCampaigns()
    {
        var grpInfoDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(grpInfoDir);

        try
        {
            var grpInfoPath = Path.Combine(grpInfoDir, "addons.grpinfo");

            File.WriteAllText(
                grpInfoPath, """
                grpinfo
                {
                    name       "Grp Campaign"
                    scriptname "scripts/test.con"
                    size       1234
                }
                """
                );

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

    /// <summary>
    ///     Tests that disposing the provider does not throw.
    /// </summary>
    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var config = new Mock<IConfigProvider>();
        config.Setup(x => x.DisabledAutoloadMods).Returns([]);
        config.Setup(x => x.FavoriteAddons).Returns([]);

        var newProvider = ObjectCreationHelper.CreateInstalledAddonsProvider(
            new DukeGame
            {
                Duke64RomPath = null,
                DukeZHRomPath = null,
                DukeWTInstallPath = null
            }, config.Object
            );

        var ex = Record.Exception(() => newProvider.Dispose());
        Assert.Null(ex);
    }

    [Fact]
    public async Task CreateCacheAsync_WithLocalFiles_FiresAddonsChangedEvent()
    {
        var fileInfo = FileCreationHelper.CreateAddonManifestInTempFolder("evt-camp", "TC", "Duke3D", "Event Camp", "1.0");

        try
        {
            var destDir = Path.Combine(_game.CampaignsFolderPath, "evt-camp");
            Directory.CreateDirectory(destDir);
            File.Copy(fileInfo.PathToFile, Path.Combine(destDir, Path.GetFileName(fileInfo.PathToFile)), true);

            GameEnum? firedGame = null;
            AddonTypeEnum? firedType = null;

            _installedAddonsProvider.AddonsChangedEvent += (g, t) =>
            {
                firedGame = g;
                firedType = t;
            };

            await _installedAddonsProvider.CreateCacheAsync(true, AddonTypeEnum.TC);

            Assert.Equal(GameEnum.Duke3D, firedGame);
            Assert.Equal(AddonTypeEnum.TC, firedType);

            var campaigns = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
            Assert.Contains(campaigns, c => c.AddonId.Id == "evt-camp");
        }
        finally
        {
            var dir = Path.GetDirectoryName(fileInfo.PathToFile);

            if (dir is not null && Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }
    }

    /// <summary>
    ///     Tests that <see cref="InstalledAddonsProvider.GetInstalledAddonsByType" /> returns only
    ///     official campaigns when the cache update semaphore is held (the
    ///     <c>
    ///         Wait(1)
    ///     </c>
    ///     fallback),
    ///     and returns the full list once the semaphore is released.
    /// </summary>
    [Fact]
    public async Task GetInstalledCampaigns_SemaphoreHeldByUpdate_ReturnsOnlyOfficial()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("held-camp", "Held Camp", "1.0", AddonTypeEnum.TC);
        _installedAddonsProvider.AddAddon(parsed);

        var before = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        Assert.Contains(before, c => c.AddonId.Id == "held-camp");

        await _installedAddonsProvider._cacheUpdateSemaphore.WaitAsync();

        try
        {
            var during = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
            Assert.DoesNotContain(during, c => c.AddonId.Id == "held-camp");
        }
        finally
        {
            _installedAddonsProvider._cacheUpdateSemaphore.Release();
        }

        var after = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        Assert.Contains(after, c => c.AddonId.Id == "held-camp");
    }

    /// <summary>
    ///     Tests that AddAddon with a null manifest, when the file is not a map and not grpinfo,
    ///     returns early without throwing and without adding to any cache.
    /// </summary>
    [Fact]
    public void AddAddon_NoManifestNotMapNotGrpInfo_ReturnsEarly()
    {
        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(PathHelper.GetFakePath(), "addon.json"),
            SupportedGame = _game.GameEnum,
            Manifest = null,
            GridHash = null,
            PreviewHash = null
        };

        var ex = Record.Exception(() => _installedAddonsProvider.AddAddon(parsed));
        Assert.Null(ex);

        var campaigns = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        Assert.DoesNotContain(campaigns, c => c.FileInfo?.PathToFile == parsed.FileInfo.PathToFile);

        var maps = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Map);
        Assert.DoesNotContain(maps, m => m.FileInfo?.PathToFile == parsed.FileInfo.PathToFile);

        var mods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        Assert.DoesNotContain(mods, m => m.FileInfo?.PathToFile == parsed.FileInfo.PathToFile);
    }

    /// <summary>
    ///     Tests that AddAddon with a null manifest (not a map, not grpinfo)
    ///     does not fire the AddonsChangedEvent.
    /// </summary>
    [Fact]
    public void AddAddon_NoManifestNotMapNotGrpInfo_DoesNotFireEvent()
    {
        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(PathHelper.GetFakePath(), "addon.json"),
            SupportedGame = _game.GameEnum,
            Manifest = null,
            GridHash = null,
            PreviewHash = null
        };

        var fired = false;
        _installedAddonsProvider.AddonsChangedEvent += (_, _) => fired = true;

        _installedAddonsProvider.AddAddon(parsed);

        Assert.False(fired);
    }

    /// <summary>
    ///     Tests that <see cref="InstalledAddonsProvider.GetInstalledAddonsByType" /> for maps
    ///     returns an empty list when the cache update semaphore is held.
    /// </summary>
    [Fact]
    public async Task GetInstalledMaps_SemaphoreHeldByUpdate_ReturnsEmptyList()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("held-map", "Held Map", "1.0", AddonTypeEnum.Map);
        _installedAddonsProvider.AddAddon(parsed);

        var before = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Map);
        Assert.NotEmpty(before);

        await _installedAddonsProvider._cacheUpdateSemaphore.WaitAsync();

        try
        {
            var during = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Map);
            Assert.Empty(during);
        }
        finally
        {
            _installedAddonsProvider._cacheUpdateSemaphore.Release();
        }

        var after = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Map);
        Assert.NotEmpty(after);
    }

    /// <summary>
    ///     Tests that <see cref="InstalledAddonsProvider.CreateCacheAsync" /> completes without
    ///     propagating when called with <see cref="AddonTypeEnum.Official" /> (the provider catches
    ///     the <see cref="ArgumentOutOfRangeException" /> internally).
    /// </summary>
    [Fact]
    public async Task CreateCacheAsync_OfficialAddonType_Completes()
    {
        var ex = await Record.ExceptionAsync(() => _installedAddonsProvider.CreateCacheAsync(true, AddonTypeEnum.Official));
        Assert.Null(ex);
    }

    /// <summary>
    ///     Tests that <see cref="InstalledAddonsProvider.GetInstalledAddonsByType" /> for mods
    ///     returns an empty list when the cache update semaphore is held.
    /// </summary>
    [Fact]
    public async Task GetInstalledMods_SemaphoreHeld_ReturnsEmpty()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedModFile("held-mod", "Held Mod", "1.0");
        _installedAddonsProvider.AddAddon(parsed);

        var before = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        Assert.NotEmpty(before);

        await _installedAddonsProvider._cacheUpdateSemaphore.WaitAsync();

        try
        {
            var during = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
            Assert.Empty(during);
        }
        finally
        {
            _installedAddonsProvider._cacheUpdateSemaphore.Release();
        }

        var after = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        Assert.NotEmpty(after);
    }

    /// <summary>
    ///     Tests that <see cref="InstalledAddonsProvider.DeleteAddon(BaseAddon)" /> throws
    ///     <see cref="ArgumentNullException" /> when the addon has a null <see cref="BaseAddon.FileInfo" />.
    /// </summary>
    [Fact]
    public void DeleteAddon_BaseAddon_NullFileInfo_Throws()
    {
        var addon = new DukeCampaign
        {
            AddonId = new("no-file", "1.0"),
            Type = AddonTypeEnum.TC,
            Title = "No File",
            SupportedGame = new GameInfo(GameEnum.Duke3D),
            FileInfo = null,
            GridImageHash = null,
            PreviewImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            RequiredFeatures = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            Executables = null,
            Options = null,
            MainCon = null,
            AdditionalCons = null,
            RTS = null
        };

        Assert.Throws<ArgumentNullException>(() => _installedAddonsProvider.DeleteAddon(addon));
    }

    /// <summary>
    ///     Tests that <see cref="InstalledAddonsProvider.DeleteAddon(ParsedAddonFile)" /> does not crash
    ///     when the parsed file does not match any cached addon.
    /// </summary>
    [Fact]
    public void DeleteAddon_ParsedAddonFile_NotFoundInCache_DoesNotCrash()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("ghost", "Ghost", "1.0", AddonTypeEnum.TC);

        var ex = Record.Exception(() => _installedAddonsProvider.DeleteAddon(parsed));

        Assert.Null(ex);
    }

    /// <summary>
    ///     Tests that the
    ///     <c>
    ///         OnMetadataInitialized
    ///     </c>
    ///     handler skips addons with null FileInfo
    ///     and checks metadata for addons with valid FileInfo.
    /// </summary>
    [Fact]
    public void OnMetadataInitialized_SkipsNullFileInfo_ChecksValid()
    {
        var parsedWithFile = ParsedAddonFileHelper.CreateParsedAddonFile("valid-camp", "Valid", "1.0", AddonTypeEnum.TC);
        _installedAddonsProvider.AddAddon(parsedWithFile);

        var parsedNoFile = ParsedAddonFileHelper.CreateParsedAddonFile("no-file", "No File", "1.0", AddonTypeEnum.TC) with
        {
            FileInfo = null
        };

        var method = typeof(InstalledAddonsProvider).GetMethod("OnMetadataInitialized", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);

        var ex = Record.Exception(() => method.Invoke(
                                      _installedAddonsProvider, [
                                          null,
                                          EventArgs.Empty
                                      ]
                                      )
            );

        Assert.Null(ex);
    }

    /// <summary>
    ///     Tests that the
    ///     <c>
    ///         OnMetadataUpdated
    ///     </c>
    ///     handler returns early when the game does not match.
    /// </summary>
    [Fact]
    public void OnMetadataUpdated_GameMismatch_ReturnsEarly()
    {
        var bloodParsed = ParsedAddonFileHelper.CreateParsedAddonFile("blood-camp", "Blood Camp", "1.0", AddonTypeEnum.TC, GameEnum.Blood);

        var method = typeof(InstalledAddonsProvider).GetMethod("OnMetadataUpdated", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);

        var ex = Record.Exception(() => method.Invoke(
                                      _installedAddonsProvider, [
                                          null,
                                          bloodParsed
                                      ]
                                      )
            );

        Assert.Null(ex);
    }

    /// <summary>
    ///     Tests that the
    ///     <c>
    ///         OnMetadataUpdated
    ///     </c>
    ///     handler completes without propagating when the addon is not found in the cache
    ///     (the provider catches the <see cref="InvalidOperationException" /> internally).
    /// </summary>
    [Fact]
    public void OnMetadataUpdated_NotFoundInCache_Completes()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("missing", "Missing", "1.0", AddonTypeEnum.TC);

        var method = typeof(InstalledAddonsProvider).GetMethod("OnMetadataUpdated", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);

        var ex = Record.Exception(() => method.Invoke(
                                      _installedAddonsProvider, [
                                          null,
                                          parsed
                                      ]
                                      )
            );

        Assert.Null(ex);
    }

    /// <summary>
    ///     Tests that the
    ///     <c>
    ///         OnMetadataUpdated
    ///     </c>
    ///     handler replaces an existing cached addon
    ///     and fires the <see cref="InstalledAddonsProvider.AddonsChangedEvent" />.
    /// </summary>
    [Fact]
    public void OnMetadataUpdated_AddonInCache_ReplacesEntry()
    {
        var original = ParsedAddonFileHelper.CreateParsedAddonFile("replaced", "Original", "1.0", AddonTypeEnum.TC);
        _installedAddonsProvider.AddAddon(original);

        var updated = ParsedAddonFileHelper.CreateParsedAddonFile("replaced", "Updated", "1.0", AddonTypeEnum.TC);

        GameEnum? firedGame = null;
        AddonTypeEnum? firedType = null;

        _installedAddonsProvider.AddonsChangedEvent += (g, t) =>
        {
            firedGame = g;
            firedType = t;
        };

        var method = typeof(InstalledAddonsProvider).GetMethod("OnMetadataUpdated", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);

        var ex = Record.Exception(() => method.Invoke(
                                      _installedAddonsProvider, [
                                          null,
                                          updated
                                      ]
                                      )
            );

        Assert.Null(ex);

        var campaigns = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
        var match = campaigns.FirstOrDefault(c => c.AddonId.Id == "replaced");
        Assert.NotNull(match);
        Assert.Equal("Updated", match.Title);

        Assert.Equal(GameEnum.Duke3D, firedGame);
        Assert.Equal(AddonTypeEnum.TC, firedType);
    }

    /// <summary>
    ///     Tests that <see cref="InstalledAddonsProvider.AddAddon" /> for a grpinfo file
    ///     correctly fires <see cref="InstalledAddonsProvider.AddonsChangedEvent" />.
    /// </summary>
    [Fact]
    public void AddAddon_GrpInfoFile_FiresAddonsChangedEvent()
    {
        var grpInfoDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(grpInfoDir);

        try
        {
            var grpInfoPath = Path.Combine(grpInfoDir, "addons.grpinfo");

            File.WriteAllText(
                grpInfoPath, """
                grpinfo
                {
                    name       "Grp Campaign"
                    scriptname "scripts/test.con"
                    size       1234
                }
                """
                );

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

            var campaigns = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
            Assert.Contains(campaigns, c => c.AddonId.Id == "grp_campaign");
        }
        finally
        {
            if (Directory.Exists(grpInfoDir))
            {
                Directory.Delete(grpInfoDir, true);
            }
        }
    }
}
