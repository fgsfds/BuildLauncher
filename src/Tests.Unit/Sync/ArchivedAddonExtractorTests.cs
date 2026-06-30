using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Addons.Helpers;
using Addons.Providers;
using Core.All;
using Core.All.Enums;
using Core.All.Serializable.Addon;
using Core.Client.Cache;
using Core.Client.Helpers;
using Games.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using DiHelper = Core.Client.Helpers.DiHelper;

namespace Tests.Unit.Sync;

[Collection("Sync")]
public sealed class ArchivedAddonExtractorTests : IDisposable
{
    private readonly ArchivedAddonExtractor _extractor;
    private readonly LocalFilesProvider _localFilesProvider;
    private readonly string _tempDir;

    public ArchivedAddonExtractorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);

        Mock<ICacheAdder<Stream>> cacheMock = new();
        ChannelBroadcaster<DiHelper.LocalFileEvent> channel = new();
        Mock<InstalledGamesProvider> gamesMock = new(MockBehavior.Loose);
        gamesMock.Setup(x => x.GetGames()).Returns([]);

        _localFilesProvider = new LocalFilesProvider(
            gamesMock.Object,
            cacheMock.Object,
            channel,
            NullLogger<LocalFilesProvider>.Instance
            );

        _extractor = new ArchivedAddonExtractor(_localFilesProvider, NullLogger<ArchivedAddonExtractor>.Instance);

        _localFilesProvider.InitializeAsync().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            try { Directory.Delete(_tempDir, true); }
            catch
            {
                /* best effort */
            }
        }
    }

    [Fact]
    public async Task UnpackAndUpdateIfNeededAsync_NonZip_ReturnsFalse()
    {
        var path = Path.Combine(_tempDir, "test.bin");
        await File.WriteAllTextAsync(path, "not a zip");

        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(path, "test.bin"),
            SupportedGame = GameEnum.Duke3D,
            Manifest = null,
            GridHash = null,
            PreviewHash = null
        };

        var result = await _extractor.UnpackAndUpdateIfNeededAsync(parsed);

        Assert.False(result);
    }

    [Fact]
    public async Task UnpackAndUpdateIfNeededAsync_ZipWithGrpInfo_UnpacksAndDeletesZip()
    {
        var srcPath = Path.Combine("Files", "GrpInfoAddon.zip");
        var destPath = Path.Combine(_tempDir, "GrpInfoAddon.zip");
        File.Copy(srcPath, destPath);

        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(destPath, "addons.grpinfo"),
            SupportedGame = GameEnum.Duke3D,
            Manifest = null,
            GridHash = null,
            PreviewHash = null
        };

        var result = await _extractor.UnpackAndUpdateIfNeededAsync(parsed);

        Assert.True(result);
        Assert.False(File.Exists(destPath));
        var extractDir = Path.Combine(_tempDir, "GrpInfoAddon");
        Assert.True(Directory.Exists(extractDir));
        Assert.True(File.Exists(Path.Combine(extractDir, "addons.grpinfo")));
    }

    [Fact]
    public async Task UnpackAndUpdateIfNeededAsync_ZipWithRffManifest_Unpacks()
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
        var manifestBytes = Encoding.UTF8.GetBytes(json);

        var zipPath = Path.Combine(_tempDir, "rff-addon.zip");

        using (var stream = File.Create(zipPath))
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
        {
            var entry = archive.CreateEntry("addon.json");
            using var writer = new StreamWriter(entry.Open());
            writer.Write(json);
        }

        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(zipPath, "addon.json"),
            SupportedGame = GameEnum.Blood,
            Manifest = null,
            GridHash = null,
            PreviewHash = null
        };

        var result = await _extractor.UnpackAndUpdateIfNeededAsync(parsed);

        Assert.True(result);
        Assert.False(File.Exists(zipPath));
        var extractDir = Path.Combine(_tempDir, "rff-addon");
        Assert.True(Directory.Exists(extractDir));
        Assert.True(File.Exists(Path.Combine(extractDir, "addon.json")));
    }

    [Fact]
    public async Task UnpackAndUpdateIfNeededAsync_ZipWithExecutablesManifest_Unpacks()
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

        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(zipPath, "addon.json"),
            SupportedGame = GameEnum.Duke3D,
            Manifest = null,
            GridHash = null,
            PreviewHash = null
        };

        var result = await _extractor.UnpackAndUpdateIfNeededAsync(parsed);

        Assert.True(result);
        Assert.False(File.Exists(zipPath));
        Assert.True(Directory.Exists(Path.Combine(_tempDir, "exe-addon")));
    }

    [Fact]
    public async Task UnpackAndUpdateIfNeededAsync_ZipWithSimpleManifest_DoesNotUnpack()
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

        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(zipPath, "addon.json"),
            SupportedGame = GameEnum.Duke3D,
            Manifest = null,
            GridHash = null,
            PreviewHash = null
        };

        var result = await _extractor.UnpackAndUpdateIfNeededAsync(parsed);

        Assert.False(result);
        Assert.True(File.Exists(zipPath));
    }

    [Fact]
    public async Task UnpackAndUpdateIfNeededAsync_EmptyZip_ReturnsFalse()
    {
        var zipPath = Path.Combine(_tempDir, "empty.zip");

        using (var stream = File.Create(zipPath))
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create)) { }

        var parsed = new ParsedAddonFile
        {
            FileInfo = new AddonFilePathWrapper(zipPath, "addon.json"),
            SupportedGame = GameEnum.Duke3D,
            Manifest = null,
            GridHash = null,
            PreviewHash = null
        };

        var result = await _extractor.UnpackAndUpdateIfNeededAsync(parsed);

        Assert.False(result);
        Assert.True(File.Exists(zipPath));
    }
}
