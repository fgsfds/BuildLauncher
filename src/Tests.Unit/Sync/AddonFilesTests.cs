using Addons.Providers;
using Core.All.Enums;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Games.Games;
using Moq;
using Tests.Unit.Helpers;

namespace Tests.Unit.Sync;

/// <summary>
///     Tests for addon file processing.
/// </summary>
[Collection("Sync")]
public sealed class AddonFilesTests : IDisposable
{
    /// <summary>
    ///     Path to the addons folder.
    /// </summary>
    private readonly string _addonsFolder = ClientProperties.AddonsFolderPath;

    /// <summary>
    ///     Blood game instance.
    /// </summary>
    private readonly BloodGame _game = new();

    /// <summary>
    ///     Installed addons provider under test.
    /// </summary>
    private readonly InstalledAddonsProvider _installedAddonsProvider;

    /// <summary>
    ///     Local files provider for scanning addon files.
    /// </summary>
    private readonly LocalFilesProvider _localFilesProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonFilesTests" /> class.
    /// </summary>
    public AddonFilesTests()
    {
        Mock<IConfigProvider> configMock = new();
        configMock.Setup(x => x.DisabledAutoloadMods).Returns([]);
        configMock.Setup(x => x.FavoriteAddons).Returns([]);

        (_installedAddonsProvider, _localFilesProvider) = ObjectCreationHelper.CreateInstalledAddonsProvider(_game, configMock.Object);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _installedAddonsProvider.Dispose();

        if (Directory.Exists(_addonsFolder))
        {
            Directory.Delete(_addonsFolder, true);
        }
    }

    /// <summary>
    ///     Initializes test dependencies by copying a file and creating the provider cache.
    /// </summary>
    private async Task<string?> InitializeDependencies(string? fileName, AddonTypeEnum addonType)
    {
        string? pathToFile = null;

        if (fileName is not null)
        {
            var addonFolder = addonType switch
            {
                AddonTypeEnum.TC => _game.CampaignsFolderPath,
                AddonTypeEnum.Map => _game.MapsFolderPath,
                AddonTypeEnum.Mod => _game.ModsFolderPath,
                _ => _addonsFolder
            };

            pathToFile = Path.Combine(addonFolder, fileName);
            Directory.CreateDirectory(addonFolder);

            File.Copy(Path.Combine("Files", fileName), pathToFile, true);
        }

        await _localFilesProvider.InitializeAsync();

        await _installedAddonsProvider.CreateCacheAsync(true, addonType);

        return pathToFile;
    }

    /// <summary>
    ///     Tests that a zipped addon returns parsed addons.
    /// </summary>
    [Fact]
    public async Task GetAddonFromFile_ZippedAddon_ReturnsParsedAddons()
    {
        var pathToFile = await InitializeDependencies("ZippedAddon.zip", AddonTypeEnum.Mod);

        var addons = await _localFilesProvider.GetCachedAddonFilesAsync();
        Assert.Equal(2, addons.Count);

        var installedMods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        Assert.Equal(2, installedMods.Count);

        var voxel1 = installedMods.First(m => m.AddonId.Id == "blood-voxel-pack");
        Assert.Equal("p292", voxel1.AddonId.Version);
        Assert.Equal("Voxel Pack", voxel1.Title);
        Assert.Equal(GameEnum.Blood, voxel1.SupportedGame.GameEnum);

        var voxel2 = installedMods.First(m => m.AddonId.Id == "blood-voxel-pack-2");
        Assert.Equal("p292-2", voxel2.AddonId.Version);
        Assert.Equal("Voxel Pack 2", voxel2.Title);
        Assert.Equal(GameEnum.Blood, voxel2.SupportedGame.GameEnum);

        Assert.True(File.Exists(pathToFile));
        Assert.False(Directory.Exists(pathToFile.Replace(".zip", "")));
    }

    /// <summary>
    ///     Tests that an unpacked addon found during init returns parsed addons.
    /// </summary>
    [Fact]
    public async Task Init_GetAddonFromFile_UnpackedAddon_ReturnsParsedAddons()
    {
        var pathToFile = await InitializeDependencies("UnpackedAddon.zip", AddonTypeEnum.Mod);

        await Task.Delay(100);

        Assert.False(File.Exists(pathToFile));
        Assert.True(Directory.Exists(pathToFile.Replace(".zip", "")));

        var addons = await _localFilesProvider.GetCachedAddonFilesAsync();
        Assert.Equal(2, addons.Count);
        Assert.DoesNotContain(addons, x => x.FileInfo.PathToFile.Contains(".zip"));

        var installedMods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        Assert.Equal(2, installedMods.Count);

        var voxel1 = installedMods.First(m => m.AddonId.Id == "blood-voxel-pack");
        Assert.Equal("p292", voxel1.AddonId.Version);
        Assert.Equal("Voxel Pack", voxel1.Title);
        Assert.Equal(GameEnum.Blood, voxel1.SupportedGame.GameEnum);

        var voxel2 = installedMods.First(m => m.AddonId.Id == "blood-voxel-pack-2");
        Assert.Equal("p292-2", voxel2.AddonId.Version);
        Assert.Equal("Voxel Pack 2", voxel2.Title);
        Assert.Equal(GameEnum.Blood, voxel2.SupportedGame.GameEnum);
    }

    /// <summary>
    ///     Tests that adding an unpacked addon file returns parsed addons.
    /// </summary>
    [Fact]
    public async Task AddFile_GetAddonFromFile_UnpackedAddon_ReturnsParsedAddons()
    {
        _ = await InitializeDependencies(null, AddonTypeEnum.Mod);

        var pathToFile = Path.Combine(_game.ModsFolderPath, "UnpackedAddon.zip");
        Directory.CreateDirectory(_game.ModsFolderPath);

        File.Copy(Path.Combine("Files", "UnpackedAddon.zip"), pathToFile, true);

        await _localFilesProvider.TryAddFileToCacheAsync(pathToFile, _game.GameEnum);

        await Task.Delay(1000);

        var addons = await _localFilesProvider.GetCachedAddonFilesAsync();
        Assert.Equal(2, addons.Count);

        Assert.False(File.Exists(pathToFile));
        Assert.True(Directory.Exists(pathToFile.Replace(".zip", "")));
        Assert.DoesNotContain(addons, x => x.FileInfo.PathToFile.Contains(".zip"));

        var installedMods = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        Assert.Equal(2, installedMods.Count);

        var voxel1 = installedMods.First(m => m.AddonId.Id == "blood-voxel-pack");
        Assert.Equal("p292", voxel1.AddonId.Version);
        Assert.Equal("Voxel Pack", voxel1.Title);
        Assert.Equal(GameEnum.Blood, voxel1.SupportedGame.GameEnum);

        var voxel2 = installedMods.First(m => m.AddonId.Id == "blood-voxel-pack-2");
        Assert.Equal("p292-2", voxel2.AddonId.Version);
        Assert.Equal("Voxel Pack 2", voxel2.Title);
        Assert.Equal(GameEnum.Blood, voxel2.SupportedGame.GameEnum);
    }

    /// <summary>
    ///     Tests that a loose map returns a single map addon.
    /// </summary>
    [Fact]
    public async Task GetInstalledAddonsByType_LooseMap_ReturnsSingleMap()
    {
        var pathToFile = await InitializeDependencies("TEST.MAP", AddonTypeEnum.Map);

        var addons = await _localFilesProvider.GetCachedAddonFilesAsync();
        var addon = Assert.Single(addons);
        Assert.Equal(GameEnum.Blood, addon.SupportedGame);

        var installedMaps = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Map);
        var map = Assert.Single(installedMaps);

        Assert.Equal("TEST.MAP", map.AddonId.Id);
        Assert.Null(map.AddonId.Version);
        Assert.Equal("TEST.MAP", map.Title);
        Assert.Equal(GameEnum.Blood, map.SupportedGame.GameEnum);

        Assert.True(File.Exists(pathToFile));
    }

    /// <summary>
    ///     Tests that a grpinfo addon is extracted from cache.
    /// </summary>
    [Fact]
    public async Task GetCachedAddonFilesAsync_GrpInfoAddon_Extracts()
    {
        var pathToFile = await InitializeDependencies("GrpInfoAddon.zip", AddonTypeEnum.TC);

        var addons = await _localFilesProvider.GetCachedAddonFilesAsync();
        Assert.Single(addons);

        Assert.False(File.Exists(pathToFile));
        Assert.True(Directory.Exists(pathToFile.Replace(".zip", "")));
        Assert.True(File.Exists(Path.Combine(pathToFile.Replace(".zip", ""), "addons.grpinfo")));
    }

    /// <summary>
    ///     Tests that creating cache multiple times does not duplicate mods.
    /// </summary>
    [Fact]
    public async Task CreateCacheAsync_Idempotent_DoesNotDuplicateMods()
    {
        await InitializeDependencies("ZippedAddon.zip", AddonTypeEnum.Mod);

        var before = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        Assert.Equal(2, before.Count);

        await _installedAddonsProvider.CreateCacheAsync(true, AddonTypeEnum.Mod);

        var after = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Mod);
        Assert.Equal(2, after.Count);
        Assert.Equal(before.Count, after.Count);
    }
}
