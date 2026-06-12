using Addons.Providers;
using Core.All.Enums;
using Core.Client.Api;
using Core.Client.Cache;
using Core.Client.Interfaces;
using Core.Client.Providers;
using Games.Games;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Tests;

[Collection("Sync")]
public sealed class AddonFilesTests : IDisposable
{
    private readonly InstalledAddonsProvider _installedAddonsProvider;

    public AddonFilesTests()
    {
        var game = new Mock<BaseGame>();
        _ = game.Setup(x => x.GameEnum).Returns(GameEnum.Blood);
        _ = game.Setup(x => x.FullName).Returns("Blood");
        _ = game.Setup(x => x.ShortName).Returns("Blood");

        var config = new Mock<IConfigProvider>();
        _ = config.Setup(x => x.DisabledAutoloadMods).Returns([]);
        _ = config.Setup(x => x.FavoriteAddons).Returns([]);

        var bmCache = new Mock<ICacheAdder<Stream>>();
        MetadataProvider metadataProvider = new(new OfflineApiInterface(NullLogger<OfflineApiInterface>.Instance), NullLogger<MetadataProvider>.Instance);
        OriginalCampaignsProvider originalCampaignsProvider = new(config.Object);

        _installedAddonsProvider = new(
            game.Object,
            config.Object,
            bmCache.Object,
            originalCampaignsProvider,
            metadataProvider,
            NullLogger<InstalledAddonsProvider>.Instance
            );
    }

    public void Dispose()
    {
        _installedAddonsProvider.Dispose();
        Directory.Delete("FilesTemp", true);
    }


    [Fact]
    public async Task AddonArchiveTest()
    {
        _ = Directory.CreateDirectory("FilesTemp");
        File.Copy(Path.Combine("Files", "ZippedAddon.zip"), Path.Combine("FilesTemp", "ZippedAddon.zip"));

        var pathToFile = Path.Combine(Directory.GetCurrentDirectory(), "FilesTemp", "ZippedAddon.zip");

        var result = await _installedAddonsProvider.GetAddonsFromFilesAsync([pathToFile]);

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

        var result = await _installedAddonsProvider.GetAddonsFromFilesAsync([pathToFile]);

        Assert.Equal(2, result.Count);

        var a = result[new("blood-voxel-pack", "p292")];
        var b = result[new("blood-voxel-pack-2", "p292-2")];

        Assert.Equal("blood-voxel-pack", a.AddonId.Id);
        Assert.Equal("p292", a.AddonId.Version);
        Assert.Equal("Voxel Pack", a.Title);

        Assert.Equal("blood-voxel-pack-2", b.AddonId.Id);
        Assert.Equal("p292-2", b.AddonId.Version);
        Assert.Equal("Voxel Pack 2", b.Title);

        Assert.False(File.Exists(pathToFile));
        Assert.True(Directory.Exists(pathToFile.Replace(".zip", "")));
    }

    [Fact]
    public async Task LooseMapTest()
    {
        _ = Directory.CreateDirectory("FilesTemp");
        File.Copy(Path.Combine("Files", "TEST.MAP"), Path.Combine("FilesTemp", "TEST.MAP"));

        var pathToFile = Path.Combine(Directory.GetCurrentDirectory(), "FilesTemp", "TEST.MAP");

        var result = await _installedAddonsProvider.GetAddonsFromFilesAsync([pathToFile]);

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

        var result = await _installedAddonsProvider.GetAddonsFromFilesAsync([pathToFile]);

        Assert.Empty(result);

        Assert.False(File.Exists(pathToFile));
        Assert.True(Directory.Exists(pathToFile.Replace(".zip", "")));
        Assert.True(File.Exists(Path.Combine(pathToFile.Replace(".zip", ""), "addons.grpinfo")));
    }

    [Fact]
    public async Task WhatLiesBeneathTest()
    {
        _ = Directory.CreateDirectory("FilesTemp");
        File.Copy(Path.Combine("Files", "WhatLiesBeneathAddon.zip"), Path.Combine("FilesTemp", "WhatLiesBeneathAddon.zip"));

        var pathToFile = Path.Combine(Directory.GetCurrentDirectory(), "FilesTemp", "WhatLiesBeneathAddon.zip");

        var result = await _installedAddonsProvider.GetAddonsFromFilesAsync([pathToFile]);

        _ = Assert.Single(result);

        var a = result.First();

        Assert.Equal("blood-what-lies-beneath", a.Key.Id);
        Assert.Equal("1.1.7", a.Key.Version);
        Assert.Equal("What Lies Beneath", a.Value.Title);

        Assert.False(File.Exists(pathToFile));
        Assert.True(Directory.Exists(pathToFile.Replace(".zip", "")));
    }
}
