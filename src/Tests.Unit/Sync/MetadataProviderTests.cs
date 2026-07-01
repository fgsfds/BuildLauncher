using Addons.Providers;
using Core.All;
using Core.All.Enums;
using Core.All.Serializable.Addon;
using Core.Client.Cache;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Games.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SharpCompress.Archives;

namespace Tests.Unit.Sync;

/// <summary>
///     Tests for the <see cref="MetadataProvider" /> class.
/// </summary>
[Collection("Sync")]
public sealed class MetadataProviderTests : IDisposable
{
    /// <summary>
    ///     Mock cache adder for testing.
    /// </summary>
    private readonly Mock<ICacheAdder<Stream>> _cacheMock;

    /// <summary>
    ///     Local files provider for scanning addon files.
    /// </summary>
    private readonly LocalFilesProvider _scanner;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MetadataProviderTests" /> class.
    /// </summary>
    public MetadataProviderTests()
    {
        _cacheMock = new Mock<ICacheAdder<Stream>>();

        Directory.CreateDirectory(ClientProperties.AddonsFolderPath);

        File.Copy(
            Path.Combine("Files", "ZippedAddon.zip"),
            Path.Combine(ClientProperties.AddonsFolderPath, "ZippedAddon.zip"),
            true
            );

        var unpackToFolder = Path.Combine(ClientProperties.AddonsFolderPath, "UnpackedAddon");
        Directory.CreateDirectory(unpackToFolder);
        using var archive = ArchiveFactory.OpenArchive(Path.Combine("Files", "UnpackedAddon.zip"));
        archive.WriteToDirectory(unpackToFolder);

        Mock<IChannelPublisher<DiHelper.LocalFileEvent>> channelPubMock = new();
        Mock<InstalledGamesProvider> gamesProvider = new();
        gamesProvider.Setup(x => x.GetGames()).Returns([]);
        _scanner = new(gamesProvider.Object, _cacheMock.Object, channelPubMock.Object, NullLogger<LocalFilesProvider>.Instance);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!Directory.Exists(ClientProperties.AddonsFolderPath))
        {
            return;
        }

        try
        {
            Directory.Delete(ClientProperties.AddonsFolderPath, true);
        }
        catch
        {
            // Best-effort cleanup
        }
    }

    private static AddonManifestJsonModel CreateRemoteManifest1()
    {
        return new()
        {
            AddonType = AddonTypeEnum.TC,
            Id = "blood-voxel-pack",
            Version = "p292",
            SupportedGame = new()
            {
                Game = GameEnum.Blood
            },
            Title = "blood-voxel-pack",
            Incompatibles = new()
            {
                Addons =
                [
                    new()
                    {
                        Id = "blood-coagulated"
                    }
                ]
            },
            Description = "New description"
        };
    }

    private static AddonManifestJsonModel CreateRemoteManifest2()
    {
        return new()
        {
            AddonType = AddonTypeEnum.TC,
            Id = "blood-voxel-pack-2",
            Version = "p292-2",
            SupportedGame = new()
            {
                Game = GameEnum.Blood
            },
            Title = "blood-voxel-pack",
            Incompatibles = new()
            {
                Addons =
                [
                    new()
                    {
                        Id = "blood-coagulated"
                    }
                ]
            },
            Description = "Voxel Pack 2",
            MainRff = "MAIN.RFF"
        };
    }

    private static AddonManifestJsonModel CreateMatchingLocalManifest1()
    {
        return new()
        {
            AddonType = AddonTypeEnum.Mod,
            Id = "blood-voxel-pack",
            Version = "p292",
            SupportedGame = new()
            {
                Game = GameEnum.Blood
            },
            Title = "Voxel Pack",
            AdditionalDefs = ["blood_voxels.def"],
            Incompatibles = new()
            {
                Addons =
                [
                    new()
                    {
                        Id = "blood-coagulated"
                    }
                ]
            },
            Description = "https://github.com/fgsfds/Blood-Voxel-Pack/\r\n\r\nVoxel replacements for sprites in Blood"
        };
    }

    private async Task<(MetadataProvider provider, Mock<IApiInterface> api)> CreateProviderAsync(
        List<AddonManifestJsonModel>? metadata)
    {
        Mock<IApiInterface> api = new();
        api.Setup(x => x.GetMetadataAsync()).ReturnsAsync(metadata);

        MetadataProvider provider = new(
            _scanner,
            api.Object,
            NullLogger<MetadataProvider>.Instance
            );

        await _scanner.InitializeAsync();
        await provider.InitializeAsync();

        return (provider, api);
    }

    /// <summary>
    ///     Tests that a zipped addon with a different manifest has an update available.
    /// </summary>
    [Fact]
    public async Task UpdateAvailable_ZippedAddonWithDifferentManifest_ReturnsTrue()
    {
        var (provider, _) = await CreateProviderAsync(new List<AddonManifestJsonModel>
        {
            CreateRemoteManifest1(),
            CreateRemoteManifest2()
        });

        var result = provider.IsMetadataUpdateAvailable(
            new("blood-voxel-pack", "p292"),
            new(Path.Combine(ClientProperties.AddonsFolderPath, "ZippedAddon.zip"), "addon.json"));

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that an unpacked addon with a different manifest has an update available.
    /// </summary>
    [Fact]
    public async Task UpdateAvailable_UnpackedAddonWithDifferentManifest_ReturnsTrue()
    {
        var (provider, _) = await CreateProviderAsync(new List<AddonManifestJsonModel>
        {
            CreateRemoteManifest1(),
            CreateRemoteManifest2()
        });

        var result = provider.IsMetadataUpdateAvailable(
            new("blood-voxel-pack-2", "p292-2"),
            new(Path.Combine(ClientProperties.AddonsFolderPath, "UnpackedAddon"), "addon2.json"));

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that when remote manifest matches local, no update is available.
    /// </summary>
    [Fact]
    public async Task IsMetadataUpdateAvailable_RemoteManifestMatchesLocal_ReturnsFalse()
    {
        var matchingManifest = CreateMatchingLocalManifest1();

        var (provider, _) = await CreateProviderAsync(new List<AddonManifestJsonModel>
        {
            matchingManifest
        });

        var result = provider.IsMetadataUpdateAvailable(
            new("blood-voxel-pack", "p292"),
            new(Path.Combine(ClientProperties.AddonsFolderPath, "ZippedAddon.zip"), "addon.json"));

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that an addon not in metadata returns false.
    /// </summary>
    [Fact]
    public async Task AddonNotInMetadata_ReturnsFalse()
    {
        var (provider, _) = await CreateProviderAsync(new List<AddonManifestJsonModel>
        {
            CreateRemoteManifest1()
        });

        var result = provider.IsMetadataUpdateAvailable(
            new("unknown-addon", "v1.0"),
            new(Path.Combine(ClientProperties.AddonsFolderPath, "ZippedAddon.zip"), "addon.json"));

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that an already cached update returns true.
    /// </summary>
    [Fact]
    public async Task AlreadyCachedUpdate_ReturnsTrue()
    {
        var (provider, _) = await CreateProviderAsync(new List<AddonManifestJsonModel>
        {
            CreateRemoteManifest1(),
            CreateRemoteManifest2()
        });

        _ = provider.IsMetadataUpdateAvailable(
            new("blood-voxel-pack", "p292"),
            new(Path.Combine(ClientProperties.AddonsFolderPath, "ZippedAddon.zip"), "addon.json"));

        var result = provider.IsMetadataUpdateAvailable(
            new("blood-voxel-pack", "p292"),
            new(Path.Combine(ClientProperties.AddonsFolderPath, "ZippedAddon.zip"), "addon.json"));

        Assert.True(result);
    }

    /// <summary>
    ///     Tests that when provider is not initialized, no update is available.
    /// </summary>
    [Fact]
    public async Task NotInitialized_ReturnsFalse()
    {
        Mock<IApiInterface> api = new();

        MetadataProvider provider = new(
            _scanner,
            api.Object,
            NullLogger<MetadataProvider>.Instance
            );

        await _scanner.InitializeAsync();

        var result = provider.IsMetadataUpdateAvailable(
            new("blood-voxel-pack", "p292"),
            new(Path.Combine(ClientProperties.AddonsFolderPath, "ZippedAddon.zip"), "addon.json"));

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that when metadata is null, no update is available.
    /// </summary>
    [Fact]
    public async Task MetadataIsNull_ReturnsFalse()
    {
        var (provider, _) = await CreateProviderAsync(null);

        var result = provider.IsMetadataUpdateAvailable(
            new("blood-voxel-pack", "p292"),
            new(Path.Combine(ClientProperties.AddonsFolderPath, "ZippedAddon.zip"), "addon.json"));

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that IsInitialized is false before initialization.
    /// </summary>
    [Fact]
    public void IsInitialized_FalseBeforeInit()
    {
        Mock<IApiInterface> api = new();

        MetadataProvider provider = new(
            _scanner,
            api.Object,
            NullLogger<MetadataProvider>.Instance
            );

        Assert.False(provider.IsInitialized);
    }

    /// <summary>
    ///     Tests that InitializeAsync sets IsInitialized to true.
    /// </summary>
    [Fact]
    public async Task InitializeAsync_SetsIsInitialized()
    {
        Mock<IApiInterface> api = new();
        api.Setup(x => x.GetMetadataAsync()).ReturnsAsync([]);

        MetadataProvider provider = new(
            _scanner,
            api.Object,
            NullLogger<MetadataProvider>.Instance
            );

        await provider.InitializeAsync();

        Assert.True(provider.IsInitialized);
    }

    /// <summary>
    ///     Tests that initializing twice returns true.
    /// </summary>
    [Fact]
    public async Task InitializeAsync_Twice_ReturnsTrue()
    {
        Mock<IApiInterface> api = new();
        api.Setup(x => x.GetMetadataAsync()).ReturnsAsync([]);

        MetadataProvider provider = new(
            _scanner,
            api.Object,
            NullLogger<MetadataProvider>.Instance
            );

        Assert.True(await provider.InitializeAsync());
        Assert.True(await provider.InitializeAsync());
    }

    /// <summary>
    ///     Tests that the MetadataInitialized event fires on initialization.
    /// </summary>
    [Fact]
    public async Task MetadataInitializedEvent_FiresOnInit()
    {
        Mock<IApiInterface> api = new();
        api.Setup(x => x.GetMetadataAsync()).ReturnsAsync([]);

        MetadataProvider provider = new(
            _scanner,
            api.Object,
            NullLogger<MetadataProvider>.Instance
            );

        var eventFired = false;
        provider.MetadataInitializedEvent += (_, _) => eventFired = true;

        await provider.InitializeAsync();

        Assert.True(eventFired);
    }

    /// <summary>
    ///     Tests that the MetadataUpdated event fires on folder update.
    /// </summary>
    [Fact]
    public async Task MetadataUpdatedEvent_FiresOnFolderUpdate()
    {
        var (provider, _) = await CreateProviderAsync(new List<AddonManifestJsonModel>
        {
            CreateRemoteManifest1(),
            CreateRemoteManifest2()
        });

        _ = provider.IsMetadataUpdateAvailable(
            new("blood-voxel-pack-2", "p292-2"),
            new(Path.Combine(ClientProperties.AddonsFolderPath, "UnpackedAddon"), "addon2.json"));

        var eventFired = false;
        provider.MetadataUpdatedEvent += (_, _) => eventFired = true;

        var result = await provider.UpdateMetadataAsync(
            new(Path.Combine(ClientProperties.AddonsFolderPath, "UnpackedAddon"), "addon2.json"));

        Assert.True(result.IsSuccess);
        Assert.True(eventFired);
    }

    /// <summary>
    ///     Tests that updating metadata for a folder succeeds.
    /// </summary>
    [Fact]
    public async Task UpdateMetadataAsync_ForFolder_Success()
    {
        var (provider, _) = await CreateProviderAsync(new List<AddonManifestJsonModel>
        {
            CreateRemoteManifest1(),
            CreateRemoteManifest2()
        });

        _ = provider.IsMetadataUpdateAvailable(
            new("blood-voxel-pack-2", "p292-2"),
            new(Path.Combine(ClientProperties.AddonsFolderPath, "UnpackedAddon"), "addon2.json"));

        var result = await provider.UpdateMetadataAsync(
            new(Path.Combine(ClientProperties.AddonsFolderPath, "UnpackedAddon"), "addon2.json"));

        Assert.True(result.IsSuccess);
    }

    /// <summary>
    ///     Tests that updating metadata when not cached returns an error.
    /// </summary>
    [Fact]
    public async Task UpdateMetadataAsync_WhenNotCached_ReturnsError()
    {
        var (provider, _) = await CreateProviderAsync(new List<AddonManifestJsonModel>
        {
            CreateRemoteManifest1(),
            CreateRemoteManifest2()
        });

        var result = await provider.UpdateMetadataAsync(
            new(Path.Combine(ClientProperties.AddonsFolderPath, "ZippedAddon.zip"), "addon.json"));

        Assert.False(result.IsSuccess);
    }

    /// <summary>
    ///     Tests that when the scanner is missing a file, no update is available.
    /// </summary>
    [Fact]
    public async Task IsMetadataUpdateAvailable_WhenScannerMissingFile_ReturnsFalse()
    {
        var (provider, _) = await CreateProviderAsync(new List<AddonManifestJsonModel>
        {
            CreateRemoteManifest1()
        });

        var result = provider.IsMetadataUpdateAvailable(
            new("blood-voxel-pack", "p292"),
            new(Path.Combine(ClientProperties.AddonsFolderPath, "NonExistent.zip"), "addon.json"));

        Assert.False(result);
    }

    /// <summary>
    ///     Tests that concurrent calls to InitializeAsync only initialize once.
    /// </summary>
    [Fact]
    public async Task InitializeAsync_ConcurrentCalls_OnlyInitializesOnce()
    {
        Mock<IApiInterface> api = new();
        api.Setup(x => x.GetMetadataAsync()).ReturnsAsync([]);

        MetadataProvider provider = new(
            _scanner,
            api.Object,
            NullLogger<MetadataProvider>.Instance
            );

        var eventFireCount = 0;
        provider.MetadataInitializedEvent += (_, _) => eventFireCount++;

        var tasks = Enumerable.Range(0, 10).Select(_ => provider.InitializeAsync());
        var results = await Task.WhenAll(tasks);

        Assert.All(results, Assert.True);
        Assert.True(provider.IsInitialized);
        Assert.Equal(1, eventFireCount);
        api.Verify(x => x.GetMetadataAsync(), Times.Once);
    }

    /// <summary>
    ///     Tests that updating metadata for a zip succeeds and fires the event.
    /// </summary>
    [Fact]
    public async Task UpdateMetadataAsync_ForZip_SuccessAndFiresEvent()
    {
        var (provider, _) = await CreateProviderAsync(new List<AddonManifestJsonModel>
        {
            CreateRemoteManifest1(),
            CreateRemoteManifest2()
        });

        var zipWrapper = new AddonFilePathWrapper(Path.Combine(ClientProperties.AddonsFolderPath, "ZippedAddon.zip"), "addon.json");

        _ = provider.IsMetadataUpdateAvailable(
            new("blood-voxel-pack", "p292"),
            zipWrapper);

        var eventFired = false;
        ParsedAddonFile? received = null;

        provider.MetadataUpdatedEvent += (_, e) =>
        {
            eventFired = true;
            received = e;
        };

        var result = await provider.UpdateMetadataAsync(zipWrapper);

        Assert.True(result.IsSuccess);
        Assert.True(eventFired);
        Assert.NotNull(received);
        Assert.Equal("blood-voxel-pack", received!.Manifest!.Id);
        Assert.Equal("p292", received.Manifest.Version);
    }

    /// <summary>
    ///     Tests that updating metadata when the file does not exist returns an error.
    /// </summary>
    [Fact]
    public async Task UpdateMetadataAsync_WhenFileDoesNotExist_ReturnsError()
    {
        var (provider, _) = await CreateProviderAsync(new List<AddonManifestJsonModel>
        {
            CreateRemoteManifest1()
        });

        var result = await provider.UpdateMetadataAsync(
            new AddonFilePathWrapper(Path.Combine(ClientProperties.AddonsFolderPath, "NonExistent.zip"), "addon.json"));

        Assert.False(result.IsSuccess);
    }

    /// <summary>
    ///     Tests that when an addon is not in the local files cache, no update is available.
    /// </summary>
    [Fact]
    public async Task IsMetadataUpdateAvailable_AddonNotInLocalFilesCache_ReturnsFalse()
    {
        var (provider, _) = await CreateProviderAsync(new List<AddonManifestJsonModel>
        {
            CreateRemoteManifest1()
        });

        var wrapper = new AddonFilePathWrapper(Path.Combine(ClientProperties.AddonsFolderPath, "ZippedAddon.zip"), "doesnotexist.json");

        var result = provider.IsMetadataUpdateAvailable(
            new("blood-voxel-pack", "p292"),
            wrapper);

        Assert.False(result);
    }
}
