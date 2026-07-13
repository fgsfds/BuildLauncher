using Addons.Providers;
using Core.All.Enums;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Games.Games;
using Moq;
using Tests.Unit.Helpers;

namespace Tests.Unit.Sequential;

public sealed class AddonFilesTests : IDisposable
{
    private readonly string _addonsFolder = ClientProperties.AddonsFolderPath;
    private readonly BloodGame _game = new();
    private readonly InstalledAddonsProvider _installedAddonsProvider;

    public AddonFilesTests()
    {
        Mock<IConfigProvider> configMock = new();
        configMock.Setup(x => x.DisabledAutoloadMods).Returns([]);
        configMock.Setup(x => x.FavoriteAddons).Returns([]);

        _installedAddonsProvider = ObjectCreationHelper.CreateInstalledAddonsProvider(_game, configMock.Object);
    }

    public void Dispose()
    {
        _installedAddonsProvider.Dispose();

        if (Directory.Exists(_addonsFolder))
        {
            Directory.Delete(_addonsFolder, true);
        }
    }

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

        await _installedAddonsProvider.CreateCacheAsync(true, addonType);

        return pathToFile;
    }

    [Fact]
    public async Task GetAddonFromFile_ZippedAddon_ReturnsParsedAddons()
    {
        var pathToFile = await InitializeDependencies("ZippedAddon.zip", AddonTypeEnum.Mod);

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

    [Fact]
    public async Task Init_GetAddonFromFile_UnpackedAddon_ReturnsParsedAddons()
    {
        var pathToFile = await InitializeDependencies("UnpackedAddon.zip", AddonTypeEnum.Mod);

        await Task.Delay(100);

        Assert.False(File.Exists(pathToFile));
        Assert.True(Directory.Exists(pathToFile.Replace(".zip", "")));

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

    [Fact]
    public async Task GetInstalledAddonsByType_LooseMap_ReturnsSingleMap()
    {
        var pathToFile = await InitializeDependencies("TEST.MAP", AddonTypeEnum.Map);

        var installedMaps = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.Map);
        var map = Assert.Single(installedMaps);

        Assert.Equal("TEST.MAP", map.AddonId.Id);
        Assert.Null(map.AddonId.Version);
        Assert.Equal("TEST.MAP", map.Title);
        Assert.Equal(GameEnum.Blood, map.SupportedGame.GameEnum);

        Assert.True(File.Exists(pathToFile));
    }

    [Fact]
    public async Task GetCachedAddonFilesAsync_GrpInfoAddon_Extracts()
    {
        DukeGame dukeGame = new()
        {
            Duke64RomPath = null,
            DukeZHRomPath = null,
            DukeWTInstallPath = null
        };

        Mock<IConfigProvider> configMock = new();
        configMock.Setup(x => x.DisabledAutoloadMods).Returns([]);
        configMock.Setup(x => x.FavoriteAddons).Returns([]);

        var installedAddonsProvider = ObjectCreationHelper.CreateInstalledAddonsProvider(dukeGame, configMock.Object);

        try
        {
            var pathToFile = Path.Combine(dukeGame.CampaignsFolderPath, "GrpInfoAddon.zip");
            Directory.CreateDirectory(dukeGame.CampaignsFolderPath);

            File.Copy(Path.Combine("Files", "GrpInfoAddon.zip"), pathToFile, true);

            await installedAddonsProvider.CreateCacheAsync(true, AddonTypeEnum.TC);

            var campaigns = installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC);
            Assert.Single(campaigns);

            Assert.False(File.Exists(pathToFile));
            Assert.True(Directory.Exists(pathToFile.Replace(".zip", "")));
            Assert.True(File.Exists(Path.Combine(pathToFile.Replace(".zip", ""), "addons.grpinfo")));
        }
        finally
        {
            installedAddonsProvider.Dispose();
        }
    }

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
