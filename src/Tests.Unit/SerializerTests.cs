using System.Text.Json;
using Core.All.Enums;
using Core.All.Serializable.Addon;

namespace Tests.Unit;

/// <summary>
///     Tests for JSON serialization and deserialization of addon manifests.
/// </summary>
public sealed class SerializerTests
{
    private const string AddonJson =
        """
            {
              "id": "addon-id",
              "type": "mod",
              "game":
              {
                  "name": "shadowwarrior",
                  "version": "1.3d",
                  "crc": "0x982AFE4A"
              },
              "title": "Addon Title",
              "author": "Author",
              "release_date": "1991-06-10",
              "version": "1.0",
              "con_main": "MAIN.CON",
              "con_modules": [ "MODULE.CON", "MODULE2.CON" ],
              "def_main": "MAIN.DEF",
              "def_modules": [ "MODULE.DEF" ],
              "rts": "MAIN.RTS",
              "ini": "MAIN.INI",
              "rff_main": "MAIN.RFF",
              "rff_sound": "SOUND.RFF",
              "dependencies":
              {
                  "addons":
                  [
                      { "id": "Addon1" },
                      { "id": "Addon2", "version": "1.0" }
                  ],
                  "features":
                  [
                      "eduke32_con",
                      "tror"
                  ],
              },
              "incompatibles":
              {
                  "addons":
                  [
                      { "id": "IncompatibleAddon1" },
                      { "id": "IncompatibleAddon2", "version": "1.1" }
                  ]
              },
              "description": "Addon description",
              "startmap": { "file": "TEST.MAP" },
              "options":
              [
                  { 
                      "name": "option 1",
                      "parameters": {
                          "opt1.def": "DEF"
                      }
                  },
                  { 
                      "name": "option 2",
                      "parameters": {
                          "opt2.def": "DEF",
                          "opt2_2.def": "DEF"
                      }
                  },
              ]
            }
        """;

    private const string MinimalAddonJson =
        """
            {
              "id": "minimal-id",
              "type": "mod",
              "game": { "name": "duke3d" },
              "title": "Minimal Addon",
              "version": "0.1"
            }
        """;

    private const string OfficialAddonJson =
        """
            {
              "id": "duke1",
              "type": "official",
              "game": { "name": "duke3d" },
              "title": "Duke Nukem Ep1",
              "version": "1.0",
              "author": "3D Realms",
              "description": "Shareware episode"
            }
        """;

    private const string BrokenAddonJson =
        """
            {
              "id": "addon-id",
              "type": "mod",
              "game":
              {
                  "name": "shadowwarrior",
                  "version": "1.3d",
                  "crc": "0x982AFE4A",
                  "unknown_token": "123"
              },
              "title": "Addon Title",
              "author": "Author",
              "version": "1.0"
            }
        """;

    private const string SlotMapJson =
        """
            {
              "id": "addon-id",
              "type": "Map",
              "game": {
                "name": "Duke3D"
              },
              "title": "Addon Title",
              "version": "1.0",
              "author": "Author",
              "startmap": {
                "volume": 1,
                "level": 2
              }
            }
        """;

    private const string NoStartmapJson =
        """
            {
              "id": "no-startmap",
              "type": "TC",
              "game": { "name": "Blood" },
              "title": "No Startmap",
              "version": "2.0",
              "author": "Author"
            }
        """;

    private const string ExhumedGameJson =
        """
            {
              "id": "exhumed-addon",
              "type": "mod",
              "game": { "name": "Exhumed" },
              "title": "Exhumed Mod",
              "version": "1.0"
            }
        """;

    private const string IniOptionsJson =
        """
            {
              "id": "ini-options",
              "type": "mod",
              "game": { "name": "duke3d" },
              "title": "Ini Options Test",
              "version": "1.0",
              "options": [
                {
                  "name": "widescreen",
                  "parameters": {
                    "widescreen.ini": "INI"
                  }
                }
              ]
            }
        """;

    private const string AllFeaturesJson =
        """
            {
              "id": "all-features",
              "type": "mod",
              "game": { "name": "duke3d" },
              "title": "All Features",
              "version": "1.0",
              "dependencies": {
                "features": [
                  "eduke32_con", "Hightile", "Models", "Sloped_Sprites",
                  "tror", "Wall_Rotate_Cstat", "Dynamic_Lighting",
                  "Modern_Types", "SndInfo", "TileFromTexture"
                ]
              }
            }
        """;

    private const string StandaloneJsonOld =
        """
            {
              "id": "amc-squad",
              "type": "TC",
              "game": {
                "name": "Standalone"
              },
              "title": "AMC Squad",
              "version": "4.5.2",
              "author": "AMCSquad",
              "description": "---",
              "executables": {
                "Windows": "amcsquad.exe",
                "Linux": "amcsquad"
              }
            }
        """;

    private const string StandaloneJson =
        """
            {
              "id": "game-id",
              "type": "TC",
              "game": {
                "name": "Standalone"
              },
              "title": "Standalone Game",
              "version": "1.0",
              "author": "Author",
              "executables": {
                "Windows": {
                  "EDuke32": "eduke32.exe"
                },
                "Linux": {
                  "EDuke32": "eduke32"
                },
              }
            }
        """;

    private const string EmptyListsJson =
        """
            {
              "id": "empty-lists",
              "type": "mod",
              "game": { "name": "duke3d" },
              "title": "Empty Lists",
              "version": "1.0",
              "con_modules": [],
              "def_modules": []
            }
        """;

    /// <summary>
    ///     Tests that a full addon JSON is correctly deserialized.
    /// </summary>
    [Fact]
    public void DeserializeAddonJson()
    {
        var result = JsonSerializer.Deserialize(AddonJson, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        Assert.NotNull(result);

        Assert.Equal(AddonTypeEnum.Mod, result.AddonType);
        Assert.Equal("addon-id", result.Id);

        Assert.Equal(GameEnum.Wang, result.SupportedGame.Game);
        Assert.Equal("1.3d", result.SupportedGame.Version);
        Assert.Equal("0x982AFE4A", result.SupportedGame.Crc);

        Assert.Equal("Addon Title", result.Title);
        Assert.Equal("Author", result.Author);
        Assert.Equal("1.0", result.Version);

        Assert.Equal("MAIN.CON", result.MainCon);
        Assert.Contains("MODULE.CON", result.AdditionalCons!);
        Assert.Contains("MODULE2.CON", result.AdditionalCons!);

        Assert.Equal("MAIN.DEF", result.MainDef);
        Assert.Contains("MODULE.DEF", result.AdditionalDefs!);

        var depsIds = result.Dependencies!.Addons!.Select(x => x.Id);
        Assert.Contains("Addon1", depsIds);
        Assert.Contains("Addon2", depsIds);
        Assert.Equal("1.0", result.Dependencies!.Addons![^1].Version);

        var incompIds = result.Incompatibles!.Addons!.Select(x => x.Id);
        Assert.Contains("IncompatibleAddon1", incompIds);
        Assert.Contains("IncompatibleAddon2", incompIds);
        Assert.Equal("1.1", result.Incompatibles!.Addons![^1].Version);

        var depsFeatures = result.Dependencies!.RequiredFeatures!;
        Assert.Contains(FeatureEnum.EDuke32_CON, depsFeatures);
        Assert.Contains(FeatureEnum.TROR, depsFeatures);

        Assert.Equal("MAIN.RTS", result.Rts);
        Assert.Equal("MAIN.INI", result.Ini);
        Assert.Equal("MAIN.RFF", result.MainRff);
        Assert.Equal("SOUND.RFF", result.SoundRff);

        Assert.Equal("TEST.MAP", ((MapFileJsonModel)result.StartMap!).File);

        Assert.Equal("Addon description", result.Description);

        Assert.Equal(DateOnly.Parse("1991-06-10"), result.ReleaseDate);
    }

    /// <summary>
    ///     Tests that a minimal addon JSON is correctly deserialized.
    /// </summary>
    [Fact]
    public void DeserializeMinimalAddon()
    {
        var result = JsonSerializer.Deserialize(MinimalAddonJson, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        Assert.NotNull(result);
        Assert.Equal("minimal-id", result.Id);
        Assert.Equal(AddonTypeEnum.Mod, result.AddonType);
        Assert.Equal(GameEnum.Duke3D, result.SupportedGame.Game);
        Assert.Equal("Minimal Addon", result.Title);
        Assert.Equal("0.1", result.Version);

        Assert.Null(result.Author);
        Assert.Null(result.ReleaseDate);
        Assert.Null(result.MainCon);
        Assert.Null(result.AdditionalCons);
        Assert.Null(result.MainDef);
        Assert.Null(result.AdditionalDefs);
        Assert.Null(result.Rts);
        Assert.Null(result.Ini);
        Assert.Null(result.MainRff);
        Assert.Null(result.SoundRff);
        Assert.Null(result.Dependencies);
        Assert.Null(result.Incompatibles);
        Assert.Null(result.StartMap);
        Assert.Null(result.Description);
        Assert.Null(result.Executables);
        Assert.Null(result.Options);
    }

    /// <summary>
    ///     Tests that an official addon JSON is correctly deserialized.
    /// </summary>
    [Fact]
    public void DeserializeOfficialAddon()
    {
        var result = JsonSerializer.Deserialize(OfficialAddonJson, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        Assert.NotNull(result);
        Assert.Equal("duke1", result.Id);
        Assert.Equal(AddonTypeEnum.Official, result.AddonType);
        Assert.Equal(GameEnum.Duke3D, result.SupportedGame.Game);
        Assert.Equal("Duke Nukem Ep1", result.Title);
        Assert.Equal("1.0", result.Version);
        Assert.Equal("3D Realms", result.Author);
        Assert.Equal("Shareware episode", result.Description);

        Assert.Null(result.StartMap);
        Assert.Null(result.AdditionalCons);
    }

    /// <summary>
    ///     Tests that an Exhumed game JSON is correctly deserialized.
    /// </summary>
    [Fact]
    public void DeserializeExhumedGame()
    {
        var result = JsonSerializer.Deserialize(ExhumedGameJson, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        Assert.NotNull(result);
        Assert.Equal(GameEnum.Slave, result.SupportedGame.Game);
    }

    /// <summary>
    ///     Tests that deserializing broken JSON throws a JsonException.
    /// </summary>
    [Fact]
    public void DeserializeBrokenAddonJson_Throws()
    {
        var ex = Assert.Throws<JsonException>(() =>
                                                  JsonSerializer.Deserialize(BrokenAddonJson, AddonManifestJsonContext.Default.AddonManifestJsonModel));

        Assert.Contains("unknown_token", ex.Message);
    }

    /// <summary>
    ///     Tests that a slot map JSON is correctly deserialized.
    /// </summary>
    [Fact]
    public void DeserializeSlotMapJson()
    {
        var result = JsonSerializer.Deserialize(SlotMapJson, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        Assert.NotNull(result);
        Assert.Equal(AddonTypeEnum.Map, result.AddonType);
        _ = Assert.IsType<MapSlotJsonModel>(result.StartMap);

        Assert.Equal(1, ((MapSlotJsonModel)result.StartMap).Episode);
        Assert.Equal(2, ((MapSlotJsonModel)result.StartMap).Level);
    }

    /// <summary>
    ///     Tests that an addon JSON without a startmap is correctly deserialized.
    /// </summary>
    [Fact]
    public void DeserializeAddonWithoutStartmap()
    {
        var result = JsonSerializer.Deserialize(NoStartmapJson, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        Assert.NotNull(result);
        Assert.Equal("no-startmap", result.Id);
        Assert.Equal(GameEnum.Blood, result.SupportedGame.Game);
        Assert.Equal(AddonTypeEnum.TC, result.AddonType);
        Assert.Null(result.StartMap);
    }

    /// <summary>
    ///     Tests that an addon JSON with INI options is correctly deserialized.
    /// </summary>
    [Fact]
    public void DeserializeAddonWithIniOptions()
    {
        var result = JsonSerializer.Deserialize(IniOptionsJson, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        Assert.NotNull(result);
        Assert.NotNull(result.Options);
        var option = Assert.Single(result.Options);
        Assert.Equal("widescreen", option.OptionName);
        Assert.NotNull(option.Parameters);
        Assert.Equal(OptionalParameterTypeEnum.INI, Assert.Single(option.Parameters.Values));
    }

    /// <summary>
    ///     Tests that an addon JSON with all features is correctly deserialized.
    /// </summary>
    [Fact]
    public void DeserializeAddonWithAllFeatures()
    {
        var result = JsonSerializer.Deserialize(AllFeaturesJson, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        Assert.NotNull(result);
        Assert.NotNull(result.Dependencies);
        Assert.NotNull(result.Dependencies.RequiredFeatures);

        Assert.Equal(10, result.Dependencies.RequiredFeatures.Count);
        Assert.Contains(FeatureEnum.EDuke32_CON, result.Dependencies.RequiredFeatures);
        Assert.Contains(FeatureEnum.Hightile, result.Dependencies.RequiredFeatures);
        Assert.Contains(FeatureEnum.Models, result.Dependencies.RequiredFeatures);
        Assert.Contains(FeatureEnum.Sloped_Sprites, result.Dependencies.RequiredFeatures);
        Assert.Contains(FeatureEnum.TROR, result.Dependencies.RequiredFeatures);
        Assert.Contains(FeatureEnum.Wall_Rotate_Cstat, result.Dependencies.RequiredFeatures);
        Assert.Contains(FeatureEnum.Dynamic_Lighting, result.Dependencies.RequiredFeatures);
        Assert.Contains(FeatureEnum.Modern_Types, result.Dependencies.RequiredFeatures);
        Assert.Contains(FeatureEnum.SndInfo, result.Dependencies.RequiredFeatures);
        Assert.Contains(FeatureEnum.TileFromTexture, result.Dependencies.RequiredFeatures);
    }

    /// <summary>
    ///     Tests that an addon JSON with empty lists is correctly deserialized.
    /// </summary>
    [Fact]
    public void DeserializeEmptyLists()
    {
        var result = JsonSerializer.Deserialize(EmptyListsJson, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        Assert.NotNull(result);
        Assert.Empty(result.AdditionalCons!);
        Assert.Empty(result.AdditionalDefs!);
    }

    /// <summary>
    ///     Tests that the old standalone JSON format is correctly deserialized.
    /// </summary>
    [Fact]
    public void DeserializeStandaloneJsonOld()
    {
        var result = JsonSerializer.Deserialize(StandaloneJsonOld, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        Assert.NotNull(result);

        Assert.Equal(AddonTypeEnum.TC, result.AddonType);
        Assert.Equal(GameEnum.Standalone, result.SupportedGame.Game);
        Assert.Equal("AMC Squad", result.Title);
        Assert.Equal("amcsquad.exe", result.Executables?[OSEnum.Windows]?[PortEnum.Stub]);
        Assert.Equal("amcsquad", result.Executables?[OSEnum.Linux]?[PortEnum.Stub]);
    }

    /// <summary>
    ///     Tests that the new standalone JSON format is correctly deserialized.
    /// </summary>
    [Fact]
    public void DeserializeStandaloneJson()
    {
        var result = JsonSerializer.Deserialize(StandaloneJson, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        Assert.NotNull(result);

        Assert.Equal(AddonTypeEnum.TC, result.AddonType);
        Assert.Equal(GameEnum.Standalone, result.SupportedGame.Game);
        Assert.Equal("Standalone Game", result.Title);
        Assert.Equal("eduke32.exe", result.Executables?[OSEnum.Windows]?[PortEnum.EDuke32]);
        Assert.Equal("eduke32", result.Executables?[OSEnum.Linux]?[PortEnum.EDuke32]);
    }

    /// <summary>
    ///     Tests that serializing then deserializing a full addon round-trips correctly.
    /// </summary>
    [Fact]
    public void SerializeThenDeserialize_RoundTrips()
    {
        var original = JsonSerializer.Deserialize(AddonJson, AddonManifestJsonContext.Default.AddonManifestJsonModel);
        Assert.NotNull(original);

        var serialized = JsonSerializer.Serialize(original, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        var deserialized = JsonSerializer.Deserialize(serialized, AddonManifestJsonContext.Default.AddonManifestJsonModel);
        Assert.NotNull(deserialized);

        Assert.Equal(original.Id, deserialized.Id);
        Assert.Equal(original.AddonType, deserialized.AddonType);
        Assert.Equal(original.SupportedGame.Game, deserialized.SupportedGame.Game);
        Assert.Equal(original.Title, deserialized.Title);
        Assert.Equal(original.Version, deserialized.Version);
        Assert.Equal(original.MainCon, deserialized.MainCon);
        Assert.Equal(original.MainDef, deserialized.MainDef);
        Assert.Equal(original.Rts, deserialized.Rts);
        Assert.Equal(original.Ini, deserialized.Ini);
        Assert.Equal(original.MainRff, deserialized.MainRff);
        Assert.Equal(original.SoundRff, deserialized.SoundRff);
        Assert.Equal(original.Description, deserialized.Description);
        Assert.Equal(original.Author, deserialized.Author);
        Assert.Equal(original.ReleaseDate, deserialized.ReleaseDate);
    }

    /// <summary>
    ///     Tests that serializing then deserializing a minimal addon round-trips correctly.
    /// </summary>
    [Fact]
    public void SerializeMinimalAddon_RoundTrips()
    {
        var original = JsonSerializer.Deserialize(MinimalAddonJson, AddonManifestJsonContext.Default.AddonManifestJsonModel);
        Assert.NotNull(original);

        var serialized = JsonSerializer.Serialize(original, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        var deserialized = JsonSerializer.Deserialize(serialized, AddonManifestJsonContext.Default.AddonManifestJsonModel);
        Assert.NotNull(deserialized);

        Assert.Equal(original.Id, deserialized.Id);
        Assert.Equal(original.Title, deserialized.Title);
        Assert.Equal(original.Version, deserialized.Version);
        Assert.Equal(original.AddonType, deserialized.AddonType);
        Assert.Equal(original.SupportedGame.Game, deserialized.SupportedGame.Game);
    }

    /// <summary>
    ///     Tests that all addon type strings are correctly deserialized.
    /// </summary>
    [Fact]
    public void DeserializeAllAddonTypes()
    {
        Assert.Equal(AddonTypeEnum.Official, DeserializeType("official"));
        Assert.Equal(AddonTypeEnum.TC, DeserializeType("TC"));
        Assert.Equal(AddonTypeEnum.Map, DeserializeType("Map"));
        Assert.Equal(AddonTypeEnum.Mod, DeserializeType("mod"));

        static AddonTypeEnum DeserializeType(string typeName)
        {
            var json = $$"""
                {
                  "id": "test-{{typeName}}",
                  "type": "{{typeName}}",
                  "game": { "name": "duke3d" },
                  "title": "Test",
                  "version": "1.0"
                }
                """;

            var result = JsonSerializer.Deserialize(json, AddonManifestJsonContext.Default.AddonManifestJsonModel);
            Assert.NotNull(result);

            return result.AddonType;
        }
    }

    /// <summary>
    ///     Tests that all game name strings are correctly deserialized.
    /// </summary>
    [Fact]
    public void DeserializeAllGameTypes()
    {
        var games = new Dictionary<string, GameEnum>
        {
            ["duke3d"] = GameEnum.Duke3D,
            ["Duke64"] = GameEnum.Duke64,
            ["blood"] = GameEnum.Blood,
            ["ShadowWarrior"] = GameEnum.Wang,
            ["fury"] = GameEnum.Fury,
            ["Exhumed"] = GameEnum.Slave,
            ["nam"] = GameEnum.NAM,
            ["ww2gi"] = GameEnum.WW2GI,
            ["redneck"] = GameEnum.Redneck,
            ["ridesagain"] = GameEnum.RidesAgain,
            ["tekwar"] = GameEnum.TekWar,
            ["witchaven"] = GameEnum.Witchaven,
            ["witchaven2"] = GameEnum.Witchaven2,
            ["standalone"] = GameEnum.Standalone,
            ["DukeZeroHour"] = GameEnum.DukeZeroHour
        };

        foreach (var (name, expected) in games)
        {
            var json = $$"""
                {
                  "id": "test-{{name}}",
                  "type": "mod",
                  "game": { "name": "{{name}}" },
                  "title": "Game {{name}}",
                  "version": "1.0"
                }
                """;

            var result = JsonSerializer.Deserialize(json, AddonManifestJsonContext.Default.AddonManifestJsonModel);
            Assert.NotNull(result);
            Assert.Equal(expected, result.SupportedGame.Game);
        }
    }
}
