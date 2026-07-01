using System.IO.Compression;
using Addons.Helpers;
using Addons.Providers;
using Core.All;
using Core.Client.Api;
using Core.Client.Cache;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Games.Games;
using Games.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using DiHelper = Core.Client.Helpers.DiHelper;

namespace Tests.Unit.Sync;

/// <summary>
///     Tests for the <see cref="AddonDropHelper" /> class.
/// </summary>
[Collection("Sync")]
public sealed class AddonDropHelperTests : IDisposable
{
    /// <summary>
    ///     Path to the addons folder.
    /// </summary>
    private readonly string _addonsFolder;

    /// <summary>
    ///     Blood game instance.
    /// </summary>
    private readonly BloodGame _game;

    /// <summary>
    ///     Addon drop helper under test.
    /// </summary>
    private readonly AddonDropHelper _helper;

    /// <summary>
    ///     Path to the maps folder.
    /// </summary>
    private readonly string _mapsFolder;

    /// <summary>
    ///     Path to the mods folder.
    /// </summary>
    private readonly string _modsFolder;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonDropHelperTests" /> class.
    /// </summary>
    public AddonDropHelperTests()
    {
        _game = new BloodGame();
        _addonsFolder = ClientProperties.AddonsFolderPath;
        _mapsFolder = _game.MapsFolderPath;
        _modsFolder = _game.ModsFolderPath;

        Directory.CreateDirectory(_addonsFolder);
        Directory.CreateDirectory(_mapsFolder);
        Directory.CreateDirectory(_modsFolder);

        Mock<ICacheAdder<Stream>> cache = new();
        ChannelBroadcaster<DiHelper.LocalFileEvent> channelPubMock = new();
        Mock<InstalledGamesProvider> gamesProvider = new();
        gamesProvider.Setup(x => x.GetGames()).Returns([]);
        LocalFilesProvider scanner = new(gamesProvider.Object, cache.Object, channelPubMock, NullLogger<LocalFilesProvider>.Instance);

        Mock<IConfigProvider> config = new();
        config.Setup(x => x.DisabledAutoloadMods).Returns([]);
        config.Setup(x => x.FavoriteAddons).Returns([]);

        OriginalCampaignsProvider originalCampaignsProvider = new(config.Object);

        MetadataProvider metadataProvider = new(
            scanner,
            new OfflineApiInterface(NullLogger<OfflineApiInterface>.Instance),
            NullLogger<MetadataProvider>.Instance
            );

        Mock<IChannelSubscriber<DiHelper.LocalFileEvent>> channelSubMock = new();

        var providerFactory = new InstalledAddonsProviderFactory(
            config.Object,
            originalCampaignsProvider,
            metadataProvider,
            scanner,
            channelSubMock.Object,
            NullLoggerFactory.Instance
            );

        _helper = new AddonDropHelper(
            scanner,
            NullLogger<AddonDropHelper>.Instance
            );
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Directory.Exists(_addonsFolder))
        {
            Directory.Delete(_addonsFolder, true);
        }
    }

    /// <summary>
    ///     Creates a zip archive at the specified path with optional addon.json content.
    /// </summary>
    private static void CreateZipArchive(string path, string? addonJsonContent = null)
    {
        using var fileStream = new FileStream(path, FileMode.Create);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create);

        if (addonJsonContent is not null)
        {
            var entry = archive.CreateEntry("addon.json", CompressionLevel.NoCompression);
            using var writer = new StreamWriter(entry.Open());
            writer.Write(addonJsonContent);
        }
    }

    /// <summary>
    ///     Tests that adding an empty list returns null.
    /// </summary>
    [Fact]
    public async Task AddAddonsAsync_EmptyList_ReturnsNull()
    {
        var result = await _helper.AddAddonsAsync([], _game);

        Assert.Null(result);
    }

    /// <summary>
    ///     Tests that an unsupported extension returns the failed filename.
    /// </summary>
    [Fact]
    public async Task AddAddonsAsync_UnsupportedExtension_ReturnsFailedName()
    {
        var txtFile = Path.Combine(_addonsFolder, "readme.txt");
        await File.WriteAllTextAsync(txtFile, "test");

        var result = await _helper.AddAddonsAsync([txtFile], _game);

        Assert.NotNull(result);
        Assert.Contains("readme.txt", result);
    }

    /// <summary>
    ///     Tests that a valid zip file installs successfully and returns null.
    /// </summary>
    [Fact]
    public async Task AddAddonsAsync_ZipFileInstallsSuccessfully_ReturnsNull()
    {
        var sourceZip = Path.Combine("Files", "ZippedAddon.zip");

        var result = await _helper.AddAddonsAsync([sourceZip], _game);

        Assert.Null(result);
        Assert.True(File.Exists(Path.Combine(_modsFolder, "ZippedAddon.zip")));
    }

    /// <summary>
    ///     Tests that multiple files with failures return only the failed names.
    /// </summary>
    [Fact]
    public async Task AddAddonsAsync_MultipleFilesWithFailures_ReturnsOnlyFailedNames()
    {
        var sourceZip = Path.Combine("Files", "ZippedAddon.zip");
        var txtFile = Path.Combine(_addonsFolder, "readme.txt");
        var txtFile2 = Path.Combine(_addonsFolder, "notes.txt");

        await File.WriteAllTextAsync(txtFile, "test1");
        await File.WriteAllTextAsync(txtFile2, "test2");

        var result = await _helper.AddAddonsAsync([sourceZip, txtFile, txtFile2], _game);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains("readme.txt", result);
        Assert.Contains("notes.txt", result);

        Assert.True(File.Exists(Path.Combine(_modsFolder, "ZippedAddon.zip")));
        Assert.False(File.Exists(Path.Combine(_modsFolder, "readme.txt")));
        Assert.False(File.Exists(Path.Combine(_modsFolder, "notes.txt")));
    }

    /// <summary>
    ///     Tests that when all files fail, all names are returned.
    /// </summary>
    [Fact]
    public async Task AddAddonsAsync_AllFail_ReturnsAllNames()
    {
        var txtFile = Path.Combine(_addonsFolder, "readme.txt");
        var txtFile2 = Path.Combine(_addonsFolder, "notes.txt");

        await File.WriteAllTextAsync(txtFile, "test1");
        await File.WriteAllTextAsync(txtFile2, "test2");

        var result = await _helper.AddAddonsAsync([txtFile, txtFile2], _game);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains("readme.txt", result);
        Assert.Contains("notes.txt", result);

        Assert.False(File.Exists(Path.Combine(_modsFolder, "readme.txt")));
        Assert.False(File.Exists(Path.Combine(_modsFolder, "notes.txt")));
    }

    /// <summary>
    ///     Tests that a map file is copied to the maps folder and returns null.
    /// </summary>
    [Fact]
    public async Task AddAddonsAsync_MapFileCopiedToMapsFolder_ReturnsNull()
    {
        var mapFile = Path.Combine(_addonsFolder, "TEST.MAP");
        File.Copy(Path.Combine("Files", "TEST.MAP"), mapFile, true);

        var result = await _helper.AddAddonsAsync([mapFile], _game);

        Assert.Null(result);
        Assert.True(File.Exists(Path.Combine(_mapsFolder, "TEST.MAP")));
    }

    /// <summary>
    ///     Tests that an archive with no manifest returns the failed name.
    /// </summary>
    [Fact]
    public async Task AddAddonsAsync_ArchiveWithNoManifest_ReturnsFailedName()
    {
        var emptyZip = Path.Combine(_addonsFolder, "empty.zip");
        CreateZipArchive(emptyZip);

        var result = await _helper.AddAddonsAsync([emptyZip], _game);

        Assert.NotNull(result);
        Assert.Contains("empty.zip", result);
    }

    /// <summary>
    ///     Tests that an addon for a wrong game returns the failed name.
    /// </summary>
    [Fact]
    public async Task AddAddonsAsync_WrongGameAddon_ReturnsFailedName()
    {
        var wrongGameZip = Path.Combine(_addonsFolder, "wrong_game.zip");

        CreateZipArchive(wrongGameZip, """
                         {
                             "id": "duke-mod",
                             "type": "Mod",
                             "game": { "name": "Duke3D" },
                             "title": "Duke Mod",
                             "version": "1.0"
                         }
                         """);

        var result = await _helper.AddAddonsAsync([wrongGameZip], _game);

        Assert.NotNull(result);
        Assert.Contains("wrong_game.zip", result);
    }

    /// <summary>
    ///     Tests that a duplicate file overwrites and succeeds.
    /// </summary>
    [Fact]
    public async Task AddAddonsAsync_DuplicateFile_OverwritesAndSucceeds()
    {
        var sourceZip = Path.Combine("Files", "ZippedAddon.zip");
        var destZip = Path.Combine(_modsFolder, "ZippedAddon.zip");

        var firstResult = await _helper.AddAddonsAsync([sourceZip], _game);
        Assert.Null(firstResult);
        Assert.True(File.Exists(destZip));

        var secondResult = await _helper.AddAddonsAsync([sourceZip], _game);
        Assert.Null(secondResult);
        Assert.True(File.Exists(destZip));
    }
}
