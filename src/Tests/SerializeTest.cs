using Common.Enums;
using Mods.Serializable;
using Mods.Serializable.Addon;
using System.Text.Json;

namespace Tests;

public class SerializerTests
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

    [Fact]
    public void DeserializeAddonJson()
    {
        var result = JsonSerializer.Deserialize(AddonJson, AddonManifestContext.Default.AddonDto);

        Assert.NotNull(result);

        Assert.Equal(AddonTypeEnum.Mod, result.AddonType);
        Assert.Equal("addon-id", result.Id);

        Assert.Equal(GameEnum.ShadowWarrior, result.SupportedGame.Game);
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
        Assert.Equal("1.0", result.Dependencies!.Addons!.Last().Version);

        var incompIds = result.Incompatibles!.Addons!.Select(x => x.Id);
        Assert.Contains("IncompatibleAddon1", incompIds);
        Assert.Contains("IncompatibleAddon2", incompIds);
        Assert.Equal("1.1", result.Incompatibles!.Addons!.Last().Version);

        var depsFeatures = result.Dependencies!.RequiredFeatures!;
        Assert.Contains(FeatureEnum.EDuke32_CON, depsFeatures);
        Assert.Contains(FeatureEnum.TROR, depsFeatures);

        Assert.Equal("MAIN.RTS", result.Rts);
        Assert.Equal("MAIN.INI", result.Ini);
        Assert.Equal("MAIN.RFF", result.MainRff);
        Assert.Equal("SOUND.RFF", result.SoundRff);

        Assert.Equal("TEST.MAP", ((MapFileDto)result.StartMap).File);

        Assert.Equal("Addon description", result.Description);
    }
    
    [Fact]
    public void DeserializeBrokenAddonJson()
    {
        AddonDto? result = null;

        try
        {
            result = JsonSerializer.Deserialize(BrokenAddonJson, AddonManifestContext.Default.AddonDto);
        }
        catch (JsonException ex)
        {
            Assert.Contains("unknown_token", ex.Message);
        }

        Assert.Null(result);
    }
    
    [Fact]
    public void DeserializeSlotMapJson()
    {
        var result = JsonSerializer.Deserialize(SlotMapJson, AddonManifestContext.Default.AddonDto);

        Assert.NotNull(result);
        Assert.IsType<MapSlotDto>(result.StartMap);

        Assert.Equal(1, ((MapSlotDto)result.StartMap).Episode);
        Assert.Equal(2, ((MapSlotDto)result.StartMap).Level);
    }
}









