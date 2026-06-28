using System.Reflection;
using Addons.Addons;
using Addons.Providers;
using Core.All;
using Core.All.Enums;
using Core.All.Serializable.Downloadable;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Games.Games;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Unit;

public sealed class DownloadableAddonsProviderTests
{
    private static FieldInfo GetCacheField()
    {
        var field = typeof(DownloadableAddonsProvider).GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        return field;
    }

    private static DukeCampaign CreateCampaign(string id, string? version)
    {
        return new DukeCampaign
        {
            AddonId = new(id, version),
            Type = AddonTypeEnum.TC,
            Title = id,
            SupportedGame = new(GameEnum.Duke3D),
            FileInfo = new AddonFilePathWrapper("D:\\Campaigns", $"{id}.zip"),
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            PreviewImageHash = null,
            StartMap = null,
            MainDef = null,
            AdditionalDefs = null,
            MainCon = null,
            AdditionalCons = null,
            RTS = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            RequiredFeatures = null,
            IsFavorite = false,
            IsMetadataUpdateAvailable = false,
            Executables = null,
            Options = null,
        };
    }

    private static AutoloadMod CreateMod(string id, string? version)
    {
        return new AutoloadMod
        {
            AddonId = new(id, version),
            Type = AddonTypeEnum.Mod,
            Title = id,
            SupportedGame = new(GameEnum.Duke3D),
            FileInfo = new AddonFilePathWrapper("D:\\Mods", $"{id}.zip"),
            GridImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            PreviewImageHash = null,
            StartMap = null,
            MainDef = null,
            AdditionalDefs = null,
            AdditionalCons = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            RequiredFeatures = null,
            IsEnabled = true,
            IsFavorite = false,
            IsMetadataUpdateAvailable = false,
            Executables = null,
            Options = null,
        };
    }

    // ---- Mod type tests ----

    [Fact]
    public void GetDownloadableAddons_SingleInstalledVersionOlderThanDownloadable_SetsIsUpdateAvailableTrue()
    {
        var provider = CreateProvider();
        PopulateCache(provider, AddonTypeEnum.Mod, [("mod-a", "2.0", "Mod A")]);
        MockInstalledAddons(provider, AddonTypeEnum.Mod, [CreateMod("mod-a", "1.0")]);

        var result = provider.GetDownloadableAddons(AddonTypeEnum.Mod);

        var item = Assert.Single(result);
        Assert.True(item.IsUpdateAvailable);
    }

    [Fact]
    public void GetDownloadableAddons_SingleInstalledVersionNewerThanDownloadable_SetsIsUpdateAvailableFalse()
    {
        var provider = CreateProvider();
        PopulateCache(provider, AddonTypeEnum.Mod, [("mod-a", "1.0", "Mod A")]);
        MockInstalledAddons(provider, AddonTypeEnum.Mod, [CreateMod("mod-a", "3.0")]);

        var result = provider.GetDownloadableAddons(AddonTypeEnum.Mod);

        var item = Assert.Single(result);
        Assert.False(item.IsUpdateAvailable);
    }

    [Fact]
    public void GetDownloadableAddons_MultipleInstalledVersions_DownloadableBetweenThem_ShouldNotShowUpdate()
    {
        var provider = CreateProvider();
        PopulateCache(provider, AddonTypeEnum.Mod, [("mod-a", "2.0", "Mod A")]);
        MockInstalledAddons(provider, AddonTypeEnum.Mod, [CreateMod("mod-a", "1.0"), CreateMod("mod-a", "3.0")]);

        var result = provider.GetDownloadableAddons(AddonTypeEnum.Mod);

        var item = Assert.Single(result);
        Assert.False(item.IsUpdateAvailable, "Downloadable 2.0 should not be an update when installed 3.0 exists");
    }

    [Fact]
    public void GetDownloadableAddons_MultipleInstalledVersions_DownloadableNewerThanAll_ShouldShowUpdate()
    {
        var provider = CreateProvider();
        PopulateCache(provider, AddonTypeEnum.Mod, [("mod-a", "5.0", "Mod A")]);
        MockInstalledAddons(provider, AddonTypeEnum.Mod, [CreateMod("mod-a", "1.0"), CreateMod("mod-a", "3.0")]);

        var result = provider.GetDownloadableAddons(AddonTypeEnum.Mod);

        var item = Assert.Single(result);
        Assert.True(item.IsUpdateAvailable, "Downloadable 5.0 should be an update over installed 1.0 and 3.0");
    }

    [Fact]
    public void GetDownloadableAddons_NoInstalledVersion_SetsIsInstalledFalse()
    {
        var provider = CreateProvider();
        PopulateCache(provider, AddonTypeEnum.Mod, [("mod-a", "2.0", "Mod A")]);
        MockInstalledAddons(provider, AddonTypeEnum.Mod, []);

        var result = provider.GetDownloadableAddons(AddonTypeEnum.Mod);

        var item = Assert.Single(result);
        Assert.False(item.IsInstalled);
        Assert.False(item.IsUpdateAvailable);
    }

    [Fact]
    public void GetDownloadableAddons_NullCache_ReturnsEmpty()
    {
        var provider = CreateProvider();

        var result = provider.GetDownloadableAddons(AddonTypeEnum.Mod);

        Assert.Empty(result);
    }

    [Fact]
    public void GetDownloadableAddons_UnknownAddonType_ReturnsEmpty()
    {
        var provider = CreateProvider();
        PopulateCache(provider, AddonTypeEnum.Mod, [("mod-a", "2.0", "Mod A")]);

        var result = provider.GetDownloadableAddons(AddonTypeEnum.TC);

        Assert.Empty(result);
    }

    // ---- TC type tests ----

    [Fact]
    public void GetDownloadableAddons_TC_SingleInstalledVersionOlder_ShowsUpdate()
    {
        var provider = CreateProvider();
        PopulateCache(provider, AddonTypeEnum.TC, [("camp-a", "2.0", "Camp A")]);
        MockInstalledAddons(provider, AddonTypeEnum.TC, [CreateCampaign("camp-a", "1.0")]);

        var result = provider.GetDownloadableAddons(AddonTypeEnum.TC);

        var item = Assert.Single(result);
        Assert.True(item.IsUpdateAvailable);
    }

    [Fact]
    public void GetDownloadableAddons_TC_SingleInstalledVersionNewer_DoesNotShowUpdate()
    {
        var provider = CreateProvider();
        PopulateCache(provider, AddonTypeEnum.TC, [("camp-a", "1.0", "Camp A")]);
        MockInstalledAddons(provider, AddonTypeEnum.TC, [CreateCampaign("camp-a", "3.0")]);

        var result = provider.GetDownloadableAddons(AddonTypeEnum.TC);

        var item = Assert.Single(result);
        Assert.False(item.IsUpdateAvailable);
    }

    // ---- Death Wish hack tests ----

    [Fact]
    public void GetDownloadableAddons_DeathWishV1_ExactVersionInstalled_SetsIsInstalledTrue()
    {
        var provider = CreateProvider();
        PopulateCache(provider, AddonTypeEnum.TC, [("death-wish", "1.6.7", "Death Wish")]);
        // installed at the same version
        MockInstalledAddons(provider, AddonTypeEnum.TC, [CreateCampaign("death-wish", "1.6.7")]);

        var result = provider.GetDownloadableAddons(AddonTypeEnum.TC);

        var item = Assert.Single(result);
        Assert.True(item.IsInstalled, "Death Wish v1 should be installed when exact version matches");
    }

    [Fact]
    public void GetDownloadableAddons_DeathWishV1_DifferentVersionInstalled_SetsIsInstalledFalse()
    {
        var provider = CreateProvider();
        PopulateCache(provider, AddonTypeEnum.TC, [("death-wish", "1.6.7", "Death Wish")]);
        // installed at a different version
        MockInstalledAddons(provider, AddonTypeEnum.TC, [CreateCampaign("death-wish", "2.0")]);

        var result = provider.GetDownloadableAddons(AddonTypeEnum.TC);

        var item = Assert.Single(result);
        Assert.False(item.IsInstalled, "Death Wish v1 should not be installed when only v2 exists");
    }

    [Fact]
    public void GetDownloadableAddons_DeathWishV2_FallsThroughToNormalComparison()
    {
        var provider = CreateProvider();
        PopulateCache(provider, AddonTypeEnum.TC, [("death-wish", "2.1", "Death Wish")]);
        // version starts with '2' so the hack doesn't apply — uses normal version comparison
        MockInstalledAddons(provider, AddonTypeEnum.TC, [CreateCampaign("death-wish", "1.0")]);

        var result = provider.GetDownloadableAddons(AddonTypeEnum.TC);

        var item = Assert.Single(result);
        Assert.True(item.IsInstalled);
        Assert.True(item.IsUpdateAvailable);
    }

    [Fact]
    public void GetDownloadableAddons_DeathWishNotTC_DoesNotTriggerHack()
    {
        var provider = CreateProvider();
        // Put death-wish in Mod cache, not TC — hack should not trigger
        PopulateCache(provider, AddonTypeEnum.Mod, [("death-wish", "1.6.7", "Death Wish")]);
        MockInstalledAddons(provider, AddonTypeEnum.Mod, [CreateMod("death-wish", "1.0")]);

        var result = provider.GetDownloadableAddons(AddonTypeEnum.Mod);

        var item = Assert.Single(result);
        Assert.True(item.IsInstalled);
        Assert.True(item.IsUpdateAvailable, "Non-TC death-wish should use normal version comparison");
    }

    // ---- Infrastructure ----

    private static DownloadableAddonsProvider CreateProvider()
    {
        var game = new DukeGame
        {
            Duke64RomPath = null,
            DukeZHRomPath = null,
            DukeWTInstallPath = null,
            GameInstallFolder = null,
        };

        Mock<IApiInterface> api = new();
        var config = new Mock<IConfigProvider>();
        config.Setup(x => x.DisabledAutoloadMods).Returns([]);
        config.Setup(x => x.FavoriteAddons).Returns([]);

        var filesProvider = new LocalFilesProvider(
            new Mock<Games.Providers.InstalledGamesProvider>().Object,
            new Mock<Core.Client.Cache.ICacheAdder<Stream>>().Object,
            new Mock<Core.All.IChannelPublisher<Core.Client.Helpers.DiHelper.LocalFileEvent>>().Object,
            new Mock<ILogger<LocalFilesProvider>>().Object);

        var metadataProvider = new MetadataProvider(
            filesProvider,
            api.Object,
            new Mock<ILogger<MetadataProvider>>().Object);

        var originalCampaignsProvider = new OriginalCampaignsProvider(config.Object);

        var factory = new InstalledAddonsProviderFactory(
            config.Object,
            originalCampaignsProvider,
            metadataProvider,
            filesProvider,
            new Mock<Core.All.IChannelSubscriber<Core.Client.Helpers.DiHelper.LocalFileEvent>>().Object,
            new Mock<ILoggerFactory>().Object);

        var logger = new Mock<ILogger<DownloadableAddonsProvider>>().Object;

        var provider = new DownloadableAddonsProvider(
            game,
            null!,  // ArchiveTools - not used in GetDownloadableAddons
            null!,  // FilesDownloader - not used in GetDownloadableAddons
            filesProvider,
            api.Object,
            factory,
            logger
        );

        return provider;
    }

    private static void PopulateCache(DownloadableAddonsProvider provider, AddonTypeEnum addonType, List<(string Id, string Version, string Title)> items)
    {
        var cache = new Dictionary<AddonTypeEnum, Dictionary<AddonId, DownloadableAddonJsonModel>>
        {
            [addonType] = items.ToDictionary(
                x => new AddonId(x.Id, x.Version),
                x => new DownloadableAddonJsonModel
                {
                    Id = x.Id,
                    Version = x.Version,
                    Title = x.Title,
                    AddonType = addonType,
                    Game = GameEnum.Duke3D,
                    FileSize = 1000,
                    DownloadUrl = new Uri("https://example.com/file.zip"),
                    IsDisabled = false,
                })
        };

        var field = GetCacheField();
        field.SetValue(provider, cache);
    }

    private static void MockInstalledAddons(DownloadableAddonsProvider provider, AddonTypeEnum addonType, List<BaseAddon> installed)
    {
        var factoryField = typeof(DownloadableAddonsProvider).GetField("_installedAddonsProvider", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(factoryField);
        var installedProvider = factoryField.GetValue(provider);
        Assert.NotNull(installedProvider);

        var cacheFieldName = addonType switch
        {
            AddonTypeEnum.TC => "_campaignsCache",
            AddonTypeEnum.Map => "_mapsCache",
            AddonTypeEnum.Mod => "_modsCache",
            _ => throw new ArgumentOutOfRangeException(nameof(addonType)),
        };

        var cacheField = typeof(InstalledAddonsProvider).GetField(cacheFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(cacheField);
        cacheField.SetValue(installedProvider, installed.ToList());
    }
}
