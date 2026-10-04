using Core.All.Enums;
using Core.All.Helpers;
using Core.All.Serializable.Addon;

namespace Tests.Unit;

/// <summary>Verifies manifest authoring rules independently of filesystem and source ports.</summary>
public sealed class AddonManifestValidatorTests
{
    /// <summary>Accepts a prepared Blood campaign with unique resource filenames.</summary>
    [Fact]
    public void Validate_PreparedBloodCampaign_Succeeds()
    {
        var manifest = CreateManifest();
        manifest.Ini = "500ml.ini";
        manifest.AdditionalDefs = ["500ml.def"];

        var result = AddonManifestValidator.Validate(manifest, ["addon.json", "500ml.ini", "500ml.def", "500ML.MAP", "AV.mid", "blood10.ogg", "grid.png", "preview.png"]);

        Assert.True(result.IsSuccess, result.Message);
    }

    /// <summary>Checks every common root filename without depending on case.</summary>
    /// <param name="filename">The reserved filename.</param>
    [Theory]
    [InlineData("DUKE3D.DEF")]
    [InlineData("rr.def")]
    [InlineData("RRRA.def")]
    [InlineData("nam.def")]
    [InlineData("napalm.def")]
    [InlineData("ww2gi.def")]
    [InlineData("blood.def")]
    [InlineData("sw.def")]
    [InlineData("exhumed.def")]
    [InlineData("GaMe.CoN")]
    [InlineData("defs.con")]
    [InlineData("user.con")]
    [InlineData("nam.con")]
    [InlineData("napalm.con")]
    [InlineData("ww2gi.con")]
    [InlineData("Blood.INI")]
    [InlineData("cryptic.ini")]
    public void Validate_CommonRootFilename_ReturnsRenameError(string filename)
    {
        var manifest = CreateManifest();
        manifest.SupportedGame = new() { Game = GameEnum.Duke3D };

        var result = AddonManifestValidator.Validate(manifest, [filename]);

        Assert.False(result.IsSuccess);
        Assert.StartsWith("Common file names cannot be used in addon folder. Rename these files: ", result.Message, StringComparison.Ordinal);
        Assert.Contains(filename, result.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Rejects legacy Blood resources with the original actionable messages.</summary>
    /// <param name="filename">The unsupported root resource.</param>
    /// <param name="message">The expected diagnostic.</param>
    [Theory]
    [InlineData("Tiles018.aRt", "ART files are not supported in Blood addons. Convert them to DEF.")]
    [InlineData("Surface.dAt", "DAT files are not supported in Blood addons. Convert them to DEF.")]
    [InlineData("CUSTOM.rFs", "Addons with RFS files are not supported.")]
    public void Validate_BloodLegacyResource_ReturnsConversionError(string filename, string message)
    {
        var result = AddonManifestValidator.Validate(CreateManifest(), [filename]);

        Assert.False(result.IsSuccess);
        Assert.Equal(message, result.Message);
    }

    /// <summary>Checks all reserved Blood CD-track replacement filenames.</summary>
    /// <param name="filename">The reserved OGG filename.</param>
    [Theory]
    [InlineData("blood00.ogg")]
    [InlineData("BLOOD01.OGG")]
    [InlineData("blood02.ogg")]
    [InlineData("blood03.ogg")]
    [InlineData("blood04.ogg")]
    [InlineData("blood05.ogg")]
    [InlineData("blood06.ogg")]
    [InlineData("blood07.ogg")]
    [InlineData("blood08.ogg")]
    [InlineData("Blood09.OgG")]
    public void Validate_ReservedBloodMusic_ReturnsReservedNameError(string filename)
    {
        var result = AddonManifestValidator.Validate(CreateManifest(), [filename]);

        Assert.False(result.IsSuccess);
        Assert.Equal("blood00.ogg through blood09.ogg are reserved and cannot be used. Start with blood10.ogg.", result.Message);
    }

    /// <summary>Limits Blood-specific resource rules to Blood packages.</summary>
    [Fact]
    public void Validate_OtherGameLegacyResources_Succeeds()
    {
        var manifest = CreateManifest();
        manifest.SupportedGame = new() { Game = GameEnum.Duke3D };

        var result = AddonManifestValidator.Validate(manifest, ["TILES001.ART", "SURFACE.DAT", "CUSTOM.RFS", "blood00.ogg"]);

        Assert.True(result.IsSuccess, result.Message);
    }

    /// <summary>Allows actual custom executables, RFF replacements and standalone packages.</summary>
    /// <param name="bypass">The supported exception to package file restrictions.</param>
    [Theory]
    [InlineData("standalone")]
    [InlineData("exe")]
    [InlineData("main-rff")]
    [InlineData("sound-rff")]
    public void Validate_RealPackageException_BypassesFileRules(string bypass)
    {
        var manifest = WithBypass(bypass);

        var result = AddonManifestValidator.Validate(manifest, ["GAME.CON", "BLOOD.INI", "TILES001.ART", "SURFACE.DAT", "CUSTOM.RFS", "BLOOD00.OGG"]);

        Assert.True(result.IsSuccess, result.Message);
    }

    /// <summary>Requires a nonblank replacement value rather than merely a populated property.</summary>
    /// <param name="emptyReplacement">The ineffective replacement metadata.</param>
    [Theory]
    [InlineData("empty-exes")]
    [InlineData("empty-os")]
    [InlineData("blank-exe")]
    [InlineData("blank-main-rff")]
    [InlineData("blank-sound-rff")]
    public void Validate_EmptyReplacement_DoesNotBypassFileRules(string emptyReplacement)
    {
        var manifest = CreateManifest();
        switch (emptyReplacement)
        {
            case "empty-exes":
                manifest.Executables = [];
                break;
            case "empty-os":
                manifest.Executables = new() { [OSEnum.Linux] = [] };
                break;
            case "blank-exe":
                manifest.Executables = new() { [OSEnum.Linux] = new() { [PortEnum.NBlood] = " \t" } };
                break;
            case "blank-main-rff":
                manifest.MainRff = " \t";
                break;
            case "blank-sound-rff":
                manifest.SoundRff = "";
                break;
            default:
                throw new ArgumentException("Unknown fixture", nameof(emptyReplacement));
        }

        var result = AddonManifestValidator.Validate(manifest, ["TILES001.ART"]);

        Assert.False(result.IsSuccess);
        Assert.Equal("ART files are not supported in Blood addons. Convert them to DEF.", result.Message);
    }

    /// <summary>Retains nonblank metadata authoring requirements.</summary>
    /// <param name="field">The required metadata field.</param>
    /// <param name="value">The missing or whitespace value.</param>
    /// <param name="message">The original diagnostic.</param>
    [Theory]
    [InlineData("id", null, "Addon ID must be specified.")]
    [InlineData("id", " \t", "Addon ID must be specified.")]
    [InlineData("title", "", "Addon title must be specified.")]
    [InlineData("title", " \t", "Addon title must be specified.")]
    [InlineData("version", "", "Addon version must be specified.")]
    [InlineData("version", " \t", "Addon version must be specified.")]
    public void Validate_BlankMetadata_ReturnsRequiredFieldError(string field, string? value, string message)
    {
        var manifest = CreateManifest();
        switch (field)
        {
            case "id":
                manifest.Id = value!;
                break;
            case "title":
                manifest.Title = value!;
                break;
            case "version":
                manifest.Version = value!;
                break;
            default:
                throw new ArgumentException("Unknown fixture", nameof(field));
        }

        var result = AddonManifestValidator.Validate(manifest, []);

        Assert.False(result.IsSuccess);
        Assert.Equal(message, result.Message);
    }

    /// <summary>Rejects undefined games.</summary>
    [Fact]
    public void Validate_UndefinedGame_ReturnsGameSelectionError()
    {
        var manifest = CreateManifest();
        manifest.SupportedGame = new() { Game = (GameEnum)255 };

        var result = AddonManifestValidator.Validate(manifest, []);

        Assert.False(result.IsSuccess);
        Assert.Equal("Game must be selected.", result.Message);
    }

    /// <summary>Rejects official and undefined package types in the authoring workflow.</summary>
    /// <param name="type">The unsupported authoring type.</param>
    [Theory]
    [InlineData(AddonTypeEnum.Official)]
    [InlineData((AddonTypeEnum)255)]
    public void Validate_UnsupportedType_ReturnsTypeSelectionError(AddonTypeEnum type)
    {
        var manifest = CreateManifest();
        manifest.AddonType = type;

        var result = AddonManifestValidator.Validate(manifest, []);

        Assert.False(result.IsSuccess);
        Assert.Equal("Addon type (TC, Map, or Mod) must be selected.", result.Message);
    }

    /// <summary>Requires a map entry point for map packages.</summary>
    [Fact]
    public void Validate_MapWithoutStart_ReturnsStartTypeError()
    {
        var manifest = CreateManifest();
        manifest.AddonType = AddonTypeEnum.Map;

        var result = AddonManifestValidator.Validate(manifest, []);

        Assert.False(result.IsSuccess);
        Assert.Equal("Start map type must be selected.", result.Message);
    }

    /// <summary>Rejects empty map file entry points.</summary>
    /// <param name="file">The missing or whitespace file path.</param>
    [Theory]
    [InlineData("")]
    [InlineData(" \t")]
    public void Validate_BlankMapFile_ReturnsStartFileError(string file)
    {
        var manifest = CreateManifest();
        manifest.AddonType = AddonTypeEnum.Map;
        manifest.StartMap = new MapFileJsonModel { File = file };

        var result = AddonManifestValidator.Validate(manifest, []);

        Assert.False(result.IsSuccess);
        Assert.Equal("Start map file name must be specified.", result.Message);
    }

    /// <summary>Accepts nonblank map paths without checking the filesystem.</summary>
    [Fact]
    public void Validate_MapFile_SucceedsWithoutFilesystemLookup()
    {
        var manifest = CreateManifest();
        manifest.AddonType = AddonTypeEnum.Map;
        manifest.StartMap = new MapFileJsonModel { File = "maps/not-on-disk.map" };

        var result = AddonManifestValidator.Validate(manifest, []);

        Assert.True(result.IsSuccess, result.Message);
    }

    /// <summary>Preserves existing slot-number acceptance without inventing new launch semantics.</summary>
    [Fact]
    public void Validate_MapSlot_DoesNotAddNumericRestrictions()
    {
        var manifest = CreateManifest();
        manifest.AddonType = AddonTypeEnum.Map;
        manifest.StartMap = new MapSlotJsonModel { Episode = -2, Level = -3 };

        var result = AddonManifestValidator.Validate(manifest, []);

        Assert.True(result.IsSuccess, result.Message);
    }

    /// <summary>Does not let package exceptions suppress required metadata and map rules.</summary>
    /// <param name="bypass">The package exception.</param>
    [Theory]
    [InlineData("standalone")]
    [InlineData("exe")]
    [InlineData("main-rff")]
    [InlineData("sound-rff")]
    public void Validate_PackageException_StillRequiresMetadataAndStartMap(string bypass)
    {
        var manifest = WithBypass(bypass);
        manifest.Title = " ";

        var metadataResult = AddonManifestValidator.Validate(manifest, ["TILES001.ART"]);

        Assert.False(metadataResult.IsSuccess);
        Assert.Equal("Addon title must be specified.", metadataResult.Message);
        manifest.Title = "Example campaign";
        manifest.AddonType = AddonTypeEnum.Map;

        var mapResult = AddonManifestValidator.Validate(manifest, ["TILES001.ART"]);

        Assert.False(mapResult.IsSuccess);
        Assert.Equal("Start map type must be selected.", mapResult.Message);
    }

    /// <summary>Accepts autoload authoring without requiring a map entry point.</summary>
    [Fact]
    public void Validate_ModWithoutStart_Succeeds()
    {
        var manifest = CreateManifest();
        manifest.AddonType = AddonTypeEnum.Mod;

        var result = AddonManifestValidator.Validate(manifest, []);

        Assert.True(result.IsSuccess, result.Message);
    }

    /// <summary>Creates complete valid required fields for an ordinary Blood campaign.</summary>
    /// <returns>The fresh manifest.</returns>
    private static AddonManifestJsonModel CreateManifest() => new()
    {
        Id = "blood-example",
        AddonType = AddonTypeEnum.TC,
        SupportedGame = new() { Game = GameEnum.Blood },
        Title = "Example campaign",
        Version = "1.0"
    };

    /// <summary>Creates a supported package exception with actual nonblank values.</summary>
    /// <param name="bypass">The exception to exercise.</param>
    /// <returns>The fresh manifest containing the exception.</returns>
    private static AddonManifestJsonModel WithBypass(string bypass)
    {
        var manifest = CreateManifest();
        switch (bypass)
        {
            case "standalone":
                manifest.SupportedGame = new() { Game = GameEnum.Standalone };
                break;
            case "exe":
                manifest.Executables = new() { [OSEnum.Linux] = new() { [PortEnum.NBlood] = "bin/custom-nblood" } };
                break;
            case "main-rff":
                manifest.MainRff = "custom.rff";
                break;
            case "sound-rff":
                manifest.SoundRff = "custom-sounds.rff";
                break;
            default:
                throw new ArgumentException("Unknown fixture", nameof(bypass));
        }

        return manifest;
    }
}
