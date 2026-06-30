using Addons.Providers;
using Core.All;
using Core.All.Enums;
using Core.Client.Cache;
using Core.Client.Helpers;
using Games.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SharpCompress.Archives;

namespace Tests.Unit.Sync;

[Collection("Sync")]
public sealed class LocalFilesProviderTests : IDisposable
{
    private readonly string _addonsFolder;

    public LocalFilesProviderTests()
    {
        _addonsFolder = ClientProperties.AddonsFolderPath;
        Directory.CreateDirectory(_addonsFolder);

        File.Copy(
            Path.Combine("Files", "ZippedAddon.zip"),
            Path.Combine(_addonsFolder, "ZippedAddon.zip"),
            true
            );

        var unpackToFolder = Path.Combine(_addonsFolder, "UnpackedAddon");
        Directory.CreateDirectory(unpackToFolder);
        using var archive = ArchiveFactory.OpenArchive(Path.Combine("Files", "UnpackedAddon.zip"));
        archive.WriteToDirectory(unpackToFolder);
    }

    public void Dispose()
    {
        if (Directory.Exists(_addonsFolder))
        {
            Directory.Delete(_addonsFolder, true);
        }
    }

    private static LocalFilesProvider CreateScanner()
    {
        Mock<ICacheAdder<Stream>> cache = new();
        Mock<IChannelPublisher<DiHelper.LocalFileEvent>> channelPubMock = new();
        Mock<InstalledGamesProvider> gamesProvider = new();
        gamesProvider.Setup(x => x.GetGames()).Returns([]);

        return new(gamesProvider.Object, cache.Object, channelPubMock.Object, NullLogger<LocalFilesProvider>.Instance);
    }

    [Fact]
    public async Task InitializeAsync_WhenAlreadyInitialized_ReturnsTrue()
    {
        var scanner = CreateScanner();

        Assert.True(await scanner.InitializeAsync());
        Assert.True(await scanner.InitializeAsync());

        var addons = await scanner.GetCachedAddonFilesAsync();
        Assert.Equal(4, addons.Count);
    }

    [Fact]
    public async Task InitializeAsync_WithEmptyAddonsFolder_ReturnsTrue()
    {
        foreach (var file in Directory.EnumerateFiles(_addonsFolder, "*", SearchOption.AllDirectories))
        {
            File.Delete(file);
        }

        foreach (var dir in Directory.EnumerateDirectories(_addonsFolder))
        {
            Directory.Delete(dir, true);
        }

        var scanner = CreateScanner();

        Assert.True(await scanner.InitializeAsync());
        Assert.True(scanner.IsInitialized);

        var addons = await scanner.GetCachedAddonFilesAsync();
        Assert.Empty(addons);
    }

    [Fact]
    public void IsInitialized_FalseBeforeInit()
    {
        var scanner = CreateScanner();

        Assert.False(scanner.IsInitialized);
    }

    [Fact]
    public async Task IsInitialized_TrueAfterInit()
    {
        var scanner = CreateScanner();
        await scanner.InitializeAsync();

        Assert.True(scanner.IsInitialized);
    }

    [Fact]
    public async Task GetCachedAddonFilesAsync_AutoInitializes()
    {
        var scanner = CreateScanner();

        var addons = await scanner.GetCachedAddonFilesAsync();

        Assert.Equal(4, addons.Count);
        Assert.True(scanner.IsInitialized);
    }

    [Fact]
    public async Task GetCachedAddonFilesAsync_ReturnsAllParsedAddonFiles()
    {
        var scanner = CreateScanner();
        await scanner.InitializeAsync();

        var addons = await scanner.GetCachedAddonFilesAsync();

        Assert.All(addons, addon =>
        {
            Assert.NotNull(addon.Manifest);
            Assert.NotNull(addon.Manifest.Id);
            Assert.NotNull(addon.Manifest.Title);
            Assert.NotNull(addon.FileInfo);
        });
    }

    [Fact]
    public async Task TryGetCachedAddonFile_WhenExists_ReturnsTrueAndFile()
    {
        var scanner = CreateScanner();
        await scanner.InitializeAsync();

        var addons = await scanner.GetCachedAddonFilesAsync();
        var target = addons[0];

        var found = scanner.TryGetCachedAddonFile(target.FileInfo, out var retrieved);

        Assert.True(found);
        Assert.NotNull(retrieved);
        Assert.Equal(target.Manifest.Id, retrieved.Manifest.Id);
        Assert.Equal(target.Manifest.Title, retrieved.Manifest.Title);
    }

    [Fact]
    public async Task TryGetCachedAddonFile_WhenNotExists_ReturnsFalse()
    {
        var scanner = CreateScanner();
        await scanner.InitializeAsync();

        var notFound = new AddonFilePathWrapper(@"C:\nonexistent", "nonexistent.json");

        Assert.False(scanner.TryGetCachedAddonFile(notFound, out var file));
        Assert.Null(file);
    }

    [Fact]
    public void TryGetCachedAddonFile_WhenNotInitialized_ReturnsFalse()
    {
        var scanner = CreateScanner();

        var wrapper = new AddonFilePathWrapper(@"C:\test", "test.json");

        Assert.False(scanner.TryGetCachedAddonFile(wrapper, out var file));
        Assert.Null(file);
    }

    [Fact]
    public async Task ReplacePath_WhenMatchExists_ReplacesPaths()
    {
        var scanner = CreateScanner();
        await scanner.InitializeAsync();

        var oldPath = Path.Combine(_addonsFolder, "UnpackedAddon");
        var newPath = Path.Combine(_addonsFolder, "MovedAddon");

        Directory.Move(oldPath, newPath);

        var updated = await scanner.ReplacePathAsync(oldPath, newPath);

        Assert.Equal(2, updated.Count);
        Assert.All(updated, x => Assert.Equal(newPath, x.FileInfo.PathToFolder));
    }

    [Fact]
    public async Task ReplacePath_WhenNoMatch_ReturnsEmpty()
    {
        var scanner = CreateScanner();
        await scanner.InitializeAsync();

        var updated = await scanner.ReplacePathAsync(@"C:\nonexistent", @"C:\new");

        Assert.Empty(updated);
    }

    [Fact]
    public async Task TryAddFileToCache_WithZip_AddsToCache()
    {
        var scanner = CreateScanner();
        await scanner.InitializeAsync();

        var additionalZip = Path.Combine(_addonsFolder, "AdditionalAddon.zip");
        File.Copy(Path.Combine("Files", "ZippedAddon.zip"), additionalZip, true);

        var result = await scanner.TryAddFileToCacheAsync(additionalZip, null);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var addons = await scanner.GetCachedAddonFilesAsync();
        Assert.Equal(6, addons.Count);
    }

    [Fact]
    public async Task TryAddFileToCache_WithUnsupportedExtension_ReturnsNull()
    {
        var scanner = CreateScanner();
        await scanner.InitializeAsync();

        var txtFile = Path.Combine(_addonsFolder, "readme.txt");
        await File.WriteAllTextAsync(txtFile, "hello");

        var result = await scanner.TryAddFileToCacheAsync(txtFile, null);

        Assert.Null(result);
    }

    [Fact]
    public async Task Concurrency_DoesNotDeadlock()
    {
        var scanner = CreateScanner();

        var t1 = scanner.InitializeAsync();
        var t2 = scanner.InitializeAsync();

        var results = await Task.WhenAll(t1, t2);

        Assert.True(results[0]);
        Assert.True(results[1]);
        Assert.True(scanner.IsInitialized);

        var addons = await scanner.GetCachedAddonFilesAsync();
        Assert.Equal(4, addons.Count);
    }

    [Fact]
    public async Task TryAddFileToCache_JsonFile_AddsToCache()
    {
        var scanner = CreateScanner();
        await scanner.InitializeAsync();

        var folder = Path.Combine(_addonsFolder, "JsonAddon");
        Directory.CreateDirectory(folder);
        var jsonPath = Path.Combine(folder, "addon.json");

        await File.WriteAllTextAsync(jsonPath, """
                                     {
                                         "id": "json-addon",
                                         "type": "Mod",
                                         "game": { "name": "Duke3D" },
                                         "title": "Json Addon",
                                         "version": "1.0"
                                     }
                                     """);

        var result = await scanner.TryAddFileToCacheAsync(jsonPath, null);

        Assert.NotNull(result);
        var parsed = Assert.Single(result);
        Assert.Equal("json-addon", parsed.Manifest!.Id);
    }

    [Fact]
    public async Task ReplacePath_WhenNotInitialized_Throws()
    {
        var scanner = CreateScanner();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => scanner.ReplacePathAsync(@"C:\old", @"C:\new"));

        Assert.Contains("Cache is not initialized", ex.Message);
    }

    [Fact]
    public async Task TryRemoveFileFromCache_WhenNotInitialized_ReturnsFalse()
    {
        var scanner = CreateScanner();

        Assert.False(await scanner.TryRemoveFileFromCacheAsync(@"C:\anything.zip"));
    }

    [Fact]
    public async Task TryRemoveFileFromCache_WhenNoMatch_ReturnsFalse()
    {
        var scanner = CreateScanner();
        await scanner.InitializeAsync();

        Assert.False(await scanner.TryRemoveFileFromCacheAsync(@"C:\nonexistent\file.zip"));
    }

    [Fact]
    public async Task TryRemoveFileFromCache_WithZipPath_RemovesAllZipEntries()
    {
        var scanner = CreateScanner();
        await scanner.InitializeAsync();

        var zipPath = Path.Combine(_addonsFolder, "ZippedAddon.zip");

        var removed = await scanner.TryRemoveFileFromCacheAsync(zipPath);

        Assert.True(removed);

        var addons = await scanner.GetCachedAddonFilesAsync();
        Assert.Equal(2, addons.Count);
        Assert.DoesNotContain(addons, x => x.FileInfo.PathToFile.Equals(zipPath, StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task TryRemoveFileFromCache_WithFolderPath_RemovesFolderEntries()
    {
        var scanner = CreateScanner();
        await scanner.InitializeAsync();

        var folderPath = Path.Combine(_addonsFolder, "UnpackedAddon");

        var removed = await scanner.TryRemoveFileFromCacheAsync(folderPath);

        Assert.True(removed);

        var addons = await scanner.GetCachedAddonFilesAsync();
        Assert.Equal(2, addons.Count);
        Assert.DoesNotContain(addons, x => x.FileInfo.PathToFolder.Equals(folderPath, StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task TryRemoveFileFromCache_AfterRemoval_EntryNoLongerFound()
    {
        var scanner = CreateScanner();
        await scanner.InitializeAsync();

        var addonsBefore = await scanner.GetCachedAddonFilesAsync();
        var target = addonsBefore[0];

        Assert.True(await scanner.TryRemoveFileFromCacheAsync(target.FileInfo.PathToFile));

        Assert.False(scanner.TryGetCachedAddonFile(target.FileInfo, out _));
    }

    [Fact]
    public async Task TryRemoveFileFromCache_FiresFileRemovedEvent()
    {
        Mock<ICacheAdder<Stream>> cache = new();
        Mock<IChannelPublisher<DiHelper.LocalFileEvent>> channelPubMock = new();
        Mock<InstalledGamesProvider> gamesProvider = new();
        gamesProvider.Setup(x => x.GetGames()).Returns([]);
        var scanner = new LocalFilesProvider(gamesProvider.Object, cache.Object, channelPubMock.Object, NullLogger<LocalFilesProvider>.Instance);
        await scanner.InitializeAsync();

        var zipPath = Path.Combine(_addonsFolder, "ZippedAddon.zip");

        await scanner.TryRemoveFileFromCacheAsync(zipPath);

        channelPubMock.Verify(x => x.PublishAsync(It.Is<DiHelper.LocalFileEvent>(
                                                      e => !e.IsAdded && e.Files.Count == 2
                                                      )), Times.Once);
    }

    [Fact]
    public async Task ReplacePath_FiresBothRemoveAndAddEvents()
    {
        Mock<ICacheAdder<Stream>> cache = new();
        Mock<IChannelPublisher<DiHelper.LocalFileEvent>> channelPubMock = new();
        Mock<InstalledGamesProvider> gamesProvider = new();
        gamesProvider.Setup(x => x.GetGames()).Returns([]);
        var scanner = new LocalFilesProvider(gamesProvider.Object, cache.Object, channelPubMock.Object, NullLogger<LocalFilesProvider>.Instance);
        await scanner.InitializeAsync();

        var oldPath = Path.Combine(_addonsFolder, "UnpackedAddon");
        var newPath = Path.Combine(_addonsFolder, "MovedAddon");

        Directory.Move(oldPath, newPath);

        await scanner.ReplacePathAsync(oldPath, newPath);

        channelPubMock.Verify(x => x.PublishAsync(It.Is<DiHelper.LocalFileEvent>(e => !e.IsAdded)), Times.Once);
        channelPubMock.Verify(x => x.PublishAsync(It.Is<DiHelper.LocalFileEvent>(e => e.IsAdded)), Times.Once);
    }

    [Fact]
    public async Task TryAddFileToCache_GrpInfoFile_AddsToCache()
    {
        var scanner = CreateScanner();
        await scanner.InitializeAsync();

        var grpInfoPath = Path.Combine(_addonsFolder, "test.grpinfo");
        await File.WriteAllTextAsync(grpInfoPath, "// test");

        var result = await scanner.TryAddFileToCacheAsync(grpInfoPath, null);

        Assert.NotNull(result);
        var parsed = Assert.Single(result);
        Assert.True(parsed.FileInfo.IsGrpInfo);
        Assert.Equal(GameEnum.Duke3D, parsed.SupportedGame);
        Assert.Null(parsed.Manifest);
    }

    [Fact]
    public async Task TryAddFileToCache_MapFile_AddsToCacheWithCorrectGame()
    {
        var scanner = CreateScanner();
        await scanner.InitializeAsync();

        var mapFile = Path.Combine(_addonsFolder, "E1L1.map");
        await File.WriteAllTextAsync(mapFile, "");

        var result = await scanner.TryAddFileToCacheAsync(mapFile, GameEnum.Blood);

        Assert.NotNull(result);
        var parsed = Assert.Single(result);
        Assert.True(parsed.FileInfo.IsMap);
        Assert.Equal(GameEnum.Blood, parsed.SupportedGame);
    }

    [Fact]
    public async Task TryAddFileToCache_MapFileWithNullGameEnum_Throws()
    {
        var scanner = CreateScanner();
        await scanner.InitializeAsync();

        var mapFile = Path.Combine(_addonsFolder, "E1L1.map");
        await File.WriteAllTextAsync(mapFile, "");

        await Assert.ThrowsAsync<InvalidOperationException>(() => scanner.TryAddFileToCacheAsync(mapFile, null));
    }

    [Fact]
    public async Task TryAddFileToCache_UnsupportedExtension_ReturnsNull()
    {
        var scanner = CreateScanner();
        await scanner.InitializeAsync();

        var txtFile = Path.Combine(_addonsFolder, "readme.txt");
        await File.WriteAllTextAsync(txtFile, "hello");

        var result = await scanner.TryAddFileToCacheAsync(txtFile, null);

        Assert.Null(result);
    }

    [Fact]
    public async Task Concurrency_TryAddAndRemove_DoesNotDeadlock()
    {
        var scanner = CreateScanner();
        await scanner.InitializeAsync();

        var txtFile = Path.Combine(_addonsFolder, "concurrent.txt");
        await File.WriteAllTextAsync(txtFile, "data");

        var mapFile = Path.Combine(_addonsFolder, "concurrent.map");
        await File.WriteAllTextAsync(mapFile, "");

        var t1 = scanner.TryAddFileToCacheAsync(txtFile, null);
        var t2 = scanner.TryAddFileToCacheAsync(mapFile, GameEnum.Duke3D);
        var t3 = scanner.TryRemoveFileFromCacheAsync(Path.Combine(_addonsFolder, "ZippedAddon.zip"));

        await Task.WhenAll(t1, t2, t3);

        Assert.NotNull(t2.Result);
    }

    [Fact]
    public async Task GetCachedAddonFilesAsync_AutoInitializesWhenNotInitialized()
    {
        var scanner = CreateScanner();

        Assert.False(scanner.IsInitialized);
        var addons = await scanner.GetCachedAddonFilesAsync();

        Assert.True(scanner.IsInitialized);
        Assert.NotEmpty(addons);
    }

    [Fact]
    public async Task ProcessArchive_WithGrpInfoEntriesInside_AddsThem()
    {
        var scanner = CreateScanner();

        var grpInfoZip = Path.Combine(_addonsFolder, "GrpInfoAddon.zip");

        File.Copy(
            Path.Combine("Files", "GrpInfoAddon.zip"),
            grpInfoZip,
            true
            );

        await scanner.InitializeAsync();

        var addons = await scanner.GetCachedAddonFilesAsync();
        Assert.Contains(addons, a => a.Manifest is null && a.SupportedGame == GameEnum.Duke3D);
    }
}
