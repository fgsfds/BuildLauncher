using Addons.Providers;
using Common;
using Common.Client.Interfaces;
using Common.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;

namespace Tests;

[Collection("Sync")]
public sealed class AddonFilesTests : IDisposable
{
    private readonly InstalledAddonsProvider _installedAddonsProvider;
    private readonly MethodInfo _getAddonsFromFilesAsync;

    public AddonFilesTests()
    {
        var game = new Mock<IGame>();
        var config = new Mock<IConfigProvider>();
        _ = config.Setup(x => x.DisabledAutoloadMods).Returns([]);
        var logger = new Mock<ILogger>();

        _installedAddonsProvider = new(game.Object, config.Object, logger.Object);
        _getAddonsFromFilesAsync = typeof(InstalledAddonsProvider).GetMethod("GetAddonsFromFilesAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    public void Dispose()
    {
        Directory.Delete("FilesTemp", true);
    }


    [Fact]
    public async Task AddonArchiveTest()
    {
        _ = Directory.CreateDirectory("FilesTemp");
        File.Copy(Path.Combine("Files", "ZippedAddon.zip"), Path.Combine("FilesTemp", "ZippedAddon.zip"));

        var pathToFile = Path.Combine(Directory.GetCurrentDirectory(), "FilesTemp", "ZippedAddon.zip");

        var result = await (Task<Dictionary<AddonVersion, IAddon>>)_getAddonsFromFilesAsync.Invoke(_installedAddonsProvider, [(object)new List<string>() { pathToFile }])!;

        Assert.Equal(2, result.Count);

        var a = result.First();
        var b = result.Last();

        Assert.Equal("blood-voxel-pack", a.Key.Id);
        Assert.Equal("p292", a.Key.Version);
        Assert.Equal("Voxel Pack", a.Value.Title);

        Assert.Equal("blood-voxel-pack-2", b.Key.Id);
        Assert.Equal("p292-2", b.Key.Version);
        Assert.Equal("Voxel Pack 2", b.Value.Title);

        Assert.True(File.Exists(pathToFile));
        Assert.False(Directory.Exists(pathToFile.Replace(".zip", "")));
    }

    [Fact]
    public async Task UnpackedAddonTest()
    {
        _ = Directory.CreateDirectory("FilesTemp");
        File.Copy(Path.Combine("Files", "UnpackedAddon.zip"), Path.Combine("FilesTemp", "UnpackedAddon.zip"));

        var pathToFile = Path.Combine(Directory.GetCurrentDirectory(), "FilesTemp", "UnpackedAddon.zip");

        var result = await (Task<Dictionary<AddonVersion, IAddon>>)_getAddonsFromFilesAsync.Invoke(_installedAddonsProvider, [(object)new List<string>() { pathToFile }])!;

        Assert.Equal(2, result.Count);

        var a = result.First();
        var b = result.Last();

        Assert.Equal("blood-voxel-pack", a.Key.Id);
        Assert.Equal("p292", a.Key.Version);
        Assert.Equal("Voxel Pack", a.Value.Title);

        Assert.Equal("blood-voxel-pack-2", b.Key.Id);
        Assert.Equal("p292-2", b.Key.Version);
        Assert.Equal("Voxel Pack 2", b.Value.Title);

        Assert.False(File.Exists(pathToFile));
        Assert.True(Directory.Exists(pathToFile.Replace(".zip", "")));
    }

    [Fact]
    public async Task LooseMapTest()
    {
        _ = Directory.CreateDirectory("FilesTemp");
        File.Copy(Path.Combine("Files", "TEST.MAP"), Path.Combine("FilesTemp", "TEST.MAP"));

        var pathToFile = Path.Combine(Directory.GetCurrentDirectory(), "FilesTemp", "TEST.MAP");

        var result = await (Task<Dictionary<AddonVersion, IAddon>>)_getAddonsFromFilesAsync.Invoke(_installedAddonsProvider, [(object)new List<string>() { pathToFile }])!;

        var map = Assert.Single(result);

        Assert.Equal("TEST.MAP", map.Key.Id);
        Assert.Null(map.Key.Version);
        Assert.Equal("TEST.MAP", map.Value.Title);

        Assert.True(File.Exists(pathToFile));
    }

    [Fact]
    public async Task GrpInfoTest()
    {
        _ = Directory.CreateDirectory("FilesTemp");
        File.Copy(Path.Combine("Files", "GrpInfoAddon.zip"), Path.Combine("FilesTemp", "GrpInfoAddon.zip"));

        var pathToFile = Path.Combine(Directory.GetCurrentDirectory(), "FilesTemp", "GrpInfoAddon.zip");

        var result = await (Task<Dictionary<AddonVersion, IAddon>>)_getAddonsFromFilesAsync.Invoke(_installedAddonsProvider, [(object)new List<string>() { pathToFile }])!;

        Assert.Empty(result);

        Assert.False(File.Exists(pathToFile));
        Assert.True(Directory.Exists(pathToFile.Replace(".zip", "")));
        Assert.True(File.Exists(Path.Combine(pathToFile.Replace(".zip", ""), "addons.grpinfo")));
    }
}
