using System.IO.Compression;
using System.Text.Json;
using Addons.Helpers;
using Core.All.Enums;
using Core.All.Serializable.Addon;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests.Unit.Sequential;

public sealed class ArchivedAddonExtractorTests : IDisposable
{
    private readonly ArchivedAddonExtractor _extractor;
    private readonly string _tempDir;

    public ArchivedAddonExtractorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _extractor = new ArchivedAddonExtractor(NullLogger<ArchivedAddonExtractor>.Instance);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            try
            {
                Directory.Delete(_tempDir, true);
            }
            catch { }
        }
    }

    [Fact]
    public async Task TryExtractIfNeededAsync_NonZip_ReturnsNull()
    {
        var path = Path.Combine(_tempDir, "test.bin");
        await File.WriteAllTextAsync(path, "not a zip");

        var result = await _extractor.TryExtractIfNeededAsync(path);

        Assert.Null(result);
    }

    [Fact]
    public async Task TryExtractIfNeededAsync_ZipWithGrpInfo_UnpacksAndDeletesZip()
    {
        var srcPath = Path.Combine("Files", "GrpInfoAddon.zip");
        var destPath = Path.Combine(_tempDir, "GrpInfoAddon.zip");
        File.Copy(srcPath, destPath);

        var result = await _extractor.TryExtractIfNeededAsync(destPath);

        Assert.NotNull(result);
        Assert.False(File.Exists(destPath));
        var extractDir = Path.Combine(_tempDir, "GrpInfoAddon");
        Assert.True(Directory.Exists(extractDir));
        Assert.True(File.Exists(Path.Combine(extractDir, "addons.grpinfo")));
    }

    [Fact]
    public async Task TryExtractIfNeededAsync_ZipWithRffManifest_Unpacks()
    {
        var manifest = new AddonManifestJsonModel
        {
            Id = "rff-addon",
            Title = "RFF Addon",
            Version = "1.0",
            AddonType = AddonTypeEnum.TC,
            SupportedGame = new SupportedGameJsonModel
            {
                Game = GameEnum.Blood
            },
            MainRff = "custom.rff"
        };

        var json = JsonSerializer.Serialize(manifest, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        var zipPath = Path.Combine(_tempDir, "rff-addon.zip");

        using (var stream = File.Create(zipPath))
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
        {
            var entry = archive.CreateEntry("addon.json");
            using var writer = new StreamWriter(entry.Open());
            writer.Write(json);
        }

        var result = await _extractor.TryExtractIfNeededAsync(zipPath);

        Assert.NotNull(result);
        Assert.False(File.Exists(zipPath));
        var extractDir = Path.Combine(_tempDir, "rff-addon");
        Assert.True(Directory.Exists(extractDir));
        Assert.True(File.Exists(Path.Combine(extractDir, "addon.json")));
    }

    [Fact]
    public async Task TryExtractIfNeededAsync_ZipWithExecutablesManifest_Unpacks()
    {
        var manifest = new AddonManifestJsonModel
        {
            Id = "exe-addon",
            Title = "Exe Addon",
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
        };

        var json = JsonSerializer.Serialize(manifest, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        var zipPath = Path.Combine(_tempDir, "exe-addon.zip");

        using (var stream = File.Create(zipPath))
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
        {
            var entry = archive.CreateEntry("addon.json");
            using var writer = new StreamWriter(entry.Open());
            writer.Write(json);
        }

        var result = await _extractor.TryExtractIfNeededAsync(zipPath);

        Assert.NotNull(result);
        Assert.False(File.Exists(zipPath));
        Assert.True(Directory.Exists(Path.Combine(_tempDir, "exe-addon")));
    }

    [Fact]
    public async Task TryExtractIfNeededAsync_ZipWithSimpleManifest_DoesNotUnpack()
    {
        var manifest = new AddonManifestJsonModel
        {
            Id = "simple",
            Title = "Simple",
            Version = "1.0",
            AddonType = AddonTypeEnum.TC,
            SupportedGame = new SupportedGameJsonModel
            {
                Game = GameEnum.Duke3D
            }
        };

        var json = JsonSerializer.Serialize(manifest, AddonManifestJsonContext.Default.AddonManifestJsonModel);

        var zipPath = Path.Combine(_tempDir, "simple.zip");

        using (var stream = File.Create(zipPath))
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
        {
            var entry = archive.CreateEntry("addon.json");
            using var writer = new StreamWriter(entry.Open());
            writer.Write(json);
        }

        var result = await _extractor.TryExtractIfNeededAsync(zipPath);

        Assert.NotNull(result);
        Assert.Null(result.UnpackedTo);
        Assert.True(File.Exists(zipPath));
    }

    [Fact]
    public async Task TryExtractIfNeededAsync_EmptyZip_ReturnsNull()
    {
        var zipPath = Path.Combine(_tempDir, "empty.zip");

        using (var stream = File.Create(zipPath))
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create)) { }

        var result = await _extractor.TryExtractIfNeededAsync(zipPath);

        Assert.Null(result);
        Assert.True(File.Exists(zipPath));
    }
}
