using Addons.Addons;
using Addons.Helpers;
using Addons.Providers;
using Avalonia.Desktop.Misc;
using Avalonia.Desktop.ViewModels;
using Core.All;
using Core.All.Enums;
using Core.All.Serializable.Addon;
using Core.Client.Api;
using Core.Client.Cache;
using Core.Client.Helpers;
using Core.Client.Interfaces;
using Core.Client.Providers;
using Games.Games;
using Games.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Ports.Ports;
using Tests.Unit.Helpers;

namespace Tests.Unit.Sequential;

public sealed class CampaignsViewModelTests : IDisposable
{
    private readonly Mock<IAddonDropHelper> _addonInstallerMock;
    private readonly BitmapsCache _bitmapsCache;
    private readonly Mock<IConfigProvider> _configMock;
    private readonly HashSet<string> _disabledMods = [];
    private readonly HashSet<AddonId> _favorites = [];
    private readonly Mock<IFolderOpener> _folderOpenerMock;
    private readonly DukeGame _game;
    private readonly Mock<InstalledGamesProvider> _gamesProviderMock;
    private readonly InstalledAddonsProvider _installedAddonsProvider;
    private readonly InstalledAddonsProviderFactory _installedAddonsProviderFactory;
    private readonly MetadataProvider _metadataProvider;
    private readonly PlaytimeProvider _playtimeProvider;
    private readonly PortStarter _portStarter;
    private readonly RatingProvider _ratingProvider;
    private readonly Mock<IUserNotifier> _userNotifierMock;
    private readonly CampaignsViewModel _viewModel;

    static CampaignsViewModelTests() => HeadlessAvaloniaApp.EnsureInitialized();

    public CampaignsViewModelTests()
    {
        _game = new DukeGame
        {
            Duke64RomPath = null,
            DukeZHRomPath = null,
            DukeWTInstallPath = null,
            GameInstallFolder = null
        };

        _configMock = new Mock<IConfigProvider>();
        _configMock.Setup(x => x.DisabledAutoloadMods).Returns(_disabledMods);
        _configMock.Setup(x => x.FavoriteAddons).Returns(_favorites);
        _configMock.Setup(x => x.Playtimes).Returns(new Dictionary<string, TimeSpan>());
        _configMock.Setup(x => x.Rating).Returns(new Dictionary<string, byte>());

        _gamesProviderMock = new Mock<InstalledGamesProvider>();
        _gamesProviderMock.Setup(x => x.GetGames()).Returns([_game]);

        Mock<ICacheAdder<Stream>> bitmapsCache = new();

        _metadataProvider = new MetadataProvider(
            new OfflineApiInterface(NullLogger<OfflineApiInterface>.Instance),
            NullLogger<MetadataProvider>.Instance
            );

        var originalCampaignsProvider = new OriginalCampaignsProvider(_configMock.Object);

        _installedAddonsProviderFactory = new InstalledAddonsProviderFactory(
            _configMock.Object,
            bitmapsCache.Object,
            originalCampaignsProvider,
            _metadataProvider,
            NullLoggerFactory.Instance
            );

        var apiMock = new Mock<IApiInterface>();
        apiMock.Setup(x => x.GetRatingsAsync()).ReturnsAsync(new Dictionary<string, decimal>());
        apiMock.Setup(x => x.GetMetadataAsync()).ReturnsAsync(new List<AddonManifestJsonModel>());

        _playtimeProvider = new PlaytimeProvider(_configMock.Object);
        _ratingProvider = new RatingProvider(apiMock.Object, _configMock.Object);
        _bitmapsCache = new BitmapsCache([], []);

        _portStarter = new PortStarter(
            _playtimeProvider,
            _installedAddonsProviderFactory,
            new FakeProcessRunner(),
            NullLogger<PortStarter>.Instance
            );

        _addonInstallerMock = new Mock<IAddonDropHelper>();
        _folderOpenerMock = new Mock<IFolderOpener>();
        _userNotifierMock = new Mock<IUserNotifier>();

        _viewModel = new CampaignsViewModel(
            _game,
            _gamesProviderMock.Object,
            _configMock.Object,
            _playtimeProvider,
            _ratingProvider,
            _metadataProvider,
            _installedAddonsProviderFactory,
            _portStarter,
            _bitmapsCache,
            _addonInstallerMock.Object,
            _folderOpenerMock.Object,
            _userNotifierMock.Object,
            NullLogger<CampaignsViewModel>.Instance
            );

        _installedAddonsProvider = _installedAddonsProviderFactory.Get(_game);
    }

    public void Dispose()
    {
        _installedAddonsProvider.Dispose();
    }

    [Fact]
    public async Task InitializeAsync_Completes_AndListAvailable()
    {
        await _viewModel.InitializeAsync();

        Assert.NotNull(_viewModel.AddonsList);
    }

    [Fact]
    public void AddonsList_Empty_ReturnsEmpty()
    {
        Assert.Empty(_viewModel.AddonsList);
    }

    [Fact]
    public void AddonsList_WithCampaigns_ReturnsCampaigns()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("test-camp", "Test", "1.0", AddonTypeEnum.TC);
        _installedAddonsProvider.AddAddon(parsed);

        var list = _viewModel.AddonsList;

        Assert.Contains(list, a => a.AddonId.Id == "test-camp");
    }

    [Fact]
    public void AddonsList_WithSearch_FiltersResults()
    {
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedAddonFile("camp-a", "Alpha Camp", "1.0", AddonTypeEnum.TC));
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedAddonFile("camp-b", "Beta Camp", "1.0", AddonTypeEnum.TC));

        _viewModel.SearchBoxText = "Alpha";

        var list = _viewModel.AddonsList;
        Assert.Contains(list, a => a.AddonId.Id == "camp-a");
        Assert.DoesNotContain(list, a => a.AddonId.Id == "camp-b");
    }

    [Fact]
    public void AddonsList_WithSearchNoMatch_ReturnsEmpty()
    {
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedAddonFile("camp-a", "Alpha Camp", "1.0", AddonTypeEnum.TC));

        _viewModel.SearchBoxText = "Zebra";

        Assert.Empty(_viewModel.AddonsList);
    }

    [Fact]
    public void AddonsList_FavoritesFirst_WithSeparator()
    {
        var favParsed = ParsedAddonFileHelper.CreateParsedAddonFile("camp-fav", "Fav Camp", "1.0", AddonTypeEnum.TC);
        _favorites.Add(new AddonId("camp-fav", "1.0"));
        _installedAddonsProvider.AddAddon(favParsed);
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedAddonFile("camp-other", "Other Camp", "1.0", AddonTypeEnum.TC));

        var list = _viewModel.AddonsList;
        var baseAddons = list.Where(a => a.AddonId?.Id is not null).ToList();
        var favIndex = baseAddons.IndexOf(baseAddons.First(a => a.AddonId.Id == "camp-fav"));
        var otherIndex = baseAddons.IndexOf(baseAddons.First(a => a.AddonId.Id == "camp-other"));

        Assert.True(favIndex >= 0 && otherIndex >= 0);
        Assert.True(favIndex < otherIndex, "Favorites should appear before non-favorites");
    }

    [Fact]
    public void SelectedAddon_Setter_FiresPropertyChanges()
    {
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedAddonFile("test", "Test", "1.0", AddonTypeEnum.TC));
        var campaign = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC).First(a => a.AddonId.Id == "test");

        var changedProperties = new List<string>();
        _viewModel.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        _viewModel.SelectedAddon = campaign;

        Assert.Contains(nameof(CampaignsViewModel.SelectedAddonDescription), changedProperties);
        Assert.Contains(nameof(CampaignsViewModel.SelectedAddonPreview), changedProperties);
        Assert.Contains(nameof(CampaignsViewModel.SelectedAddonRating), changedProperties);
        Assert.Contains(nameof(CampaignsViewModel.IsMetadataUpdateAvailable), changedProperties);
        Assert.Contains(nameof(CampaignsViewModel.SelectedAddonPlaytime), changedProperties);
        Assert.Contains(nameof(CampaignsViewModel.IsPreviewVisible), changedProperties);
    }

    [Fact]
    public void DeleteCampaign_NullSelectedAddon_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _viewModel.DeleteAddonCommand.Execute(null));
    }

    [Fact]
    public void DeleteCampaign_ValidCampaign_RemovesFromList()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("del-test", "Delete Test", "1.0", AddonTypeEnum.TC);
        _installedAddonsProvider.AddAddon(parsed);
        var campaign = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC).First(a => a.AddonId.Id == "del-test");
        _viewModel.SelectedAddon = campaign;

        Directory.CreateDirectory(campaign.FileInfo!.PathToFolder);

        _viewModel.DeleteAddonCommand.Execute(null);

        Assert.DoesNotContain(_viewModel.AddonsList, a => a.AddonId.Id == "del-test");
    }

    [Fact]
    public void AddToFavorite_ValidAddon_UpdatesConfig()
    {
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedAddonFile("fav-test", "Fav Test", "1.0", AddonTypeEnum.TC));
        var campaign = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC).First(a => a.AddonId.Id == "fav-test");

        _viewModel.AddToFavoriteCommand.Execute(campaign);

        _configMock.Verify(x => x.ChangeFavoriteState(new AddonId("fav-test", "1.0"), true), Times.Once);
    }

    [Fact]
    public void RemoveFromFavorite_ValidAddon_UpdatesConfig()
    {
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedAddonFile("unfav-test", "Unfav Test", "1.0", AddonTypeEnum.TC));
        var campaign = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC).First(a => a.AddonId.Id == "unfav-test");

        _viewModel.RemoveFromFavoriteCommand.Execute(campaign);

        _configMock.Verify(x => x.ChangeFavoriteState(new AddonId("unfav-test", "1.0"), false), Times.Once);
    }

    [Fact]
    public void AddToFavorite_InvalidType_Throws()
    {
        Assert.Throws<ArgumentException>(() => _viewModel.AddToFavoriteCommand.Execute("not an addon"));
    }

    [Fact]
    public void ClearSearchBox_CanExecute_HasText()
    {
        _viewModel.SearchBoxText = "something";

        Assert.True(_viewModel.ClearSearchBoxCommand.CanExecute(null));
    }

    [Fact]
    public void ClearSearchBox_CannotExecute_WhenEmpty()
    {
        _viewModel.SearchBoxText = string.Empty;

        Assert.False(_viewModel.ClearSearchBoxCommand.CanExecute(null));
    }

    [Fact]
    public void ClearSearchBox_Executed_ClearsText()
    {
        _viewModel.SearchBoxText = "something";

        _viewModel.ClearSearchBoxCommand.Execute(null);

        Assert.Equal(string.Empty, _viewModel.SearchBoxText);
    }

    [Fact]
    public void ProcessDroppedFiles_DelegatesToInstaller()
    {
        var files = new List<string>
        {
            "test.zip"
        };

        _viewModel.ProcessDroppedFilesCommand.Execute(files);

        _addonInstallerMock.Verify(x => x.AddAddonsAsync(files, _game), Times.Once);
    }

    [Fact]
    public async Task UpdateMetadata_WithAddonValue_Completes()
    {
        var fileInfo = FileCreationHelper.CreateAddonManifestInTempFolder("meta-test", "TC", "Duke3D", "Meta Test", "1.0");

        try
        {
            _installedAddonsProvider.AddAddon(
                new ParsedAddonFile
                {
                    FileInfo = fileInfo,
                    SupportedGame = GameEnum.Duke3D,
                    Manifest = new AddonManifestJsonModel
                    {
                        Id = "meta-test",
                        Title = "Meta Test",
                        Version = "1.0",
                        AddonType = AddonTypeEnum.TC,
                        SupportedGame = new SupportedGameJsonModel
                        {
                            Game = GameEnum.Duke3D
                        }
                    },
                    GridHash = null,
                    PreviewHash = null
                }
                );

            var campaign = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC)
                                                   .First(a => a.AddonId.Id == "meta-test");

            var ex = await Record.ExceptionAsync(() => _viewModel.UpdateMetadataAsync(campaign));

            Assert.Null(ex);
        }
        finally
        {
            var dir = Path.GetDirectoryName(fileInfo.PathToFile);

            if (dir is not null && Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }
    }

    [Fact]
    public async Task UpdateMetadata_WithNullValue_FallsBackToSelectedAddon()
    {
        var fileInfo = FileCreationHelper.CreateAddonManifestInTempFolder("meta-selected", "TC", "Duke3D", "Meta Selected", "1.0");

        try
        {
            _installedAddonsProvider.AddAddon(
                new ParsedAddonFile
                {
                    FileInfo = fileInfo,
                    SupportedGame = GameEnum.Duke3D,
                    Manifest = new AddonManifestJsonModel
                    {
                        Id = "meta-selected",
                        Title = "Meta Selected",
                        Version = "1.0",
                        AddonType = AddonTypeEnum.TC,
                        SupportedGame = new SupportedGameJsonModel
                        {
                            Game = GameEnum.Duke3D
                        }
                    },
                    GridHash = null,
                    PreviewHash = null
                }
                );

            _viewModel.SelectedAddon = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC)
                                                               .First(a => a.AddonId.Id == "meta-selected");

            var ex = await Record.ExceptionAsync(() => _viewModel.UpdateMetadataAsync(null));

            Assert.Null(ex);
        }
        finally
        {
            var dir = Path.GetDirectoryName(fileInfo.PathToFile);

            if (dir is not null && Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }
    }

    [Fact]
    public async Task UpdateMetadata_AddonWithNullFileInfo_Throws()
    {
        var addon = new DukeCampaign
        {
            AddonId = new("no-file", "1.0"),
            Type = AddonTypeEnum.TC,
            Title = "No File",
            SupportedGame = new GameInfo(GameEnum.Duke3D),
            FileInfo = null,
            GridImageHash = null,
            PreviewImageHash = null,
            Author = null,
            ReleaseDate = null,
            Description = null,
            RequiredFeatures = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            Executables = null,
            Options = null,
            MainCon = null,
            AdditionalCons = null,
            RTS = null
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _viewModel.UpdateMetadataAsync(addon));
    }

    [Fact]
    public async Task UpdateMetadata_NullValueAndNullSelected_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _viewModel.UpdateMetadataAsync(null));
    }

    [Fact]
    public void StartCampaign_NullSelectedAddon_CaughtByViewModel()
    {
        var ex = Record.Exception(() => _viewModel.StartAddonCommand.Execute(new object()));
        Assert.Null(ex);
    }

    [Fact]
    public void StartCampaign_UnknownCommandType_CaughtByViewModel()
    {
        _installedAddonsProvider.AddAddon(ParsedAddonFileHelper.CreateParsedAddonFile("test", "Test", "1.0", AddonTypeEnum.TC));

        _viewModel.SelectedAddon = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC)
                                                           .First(a => a.AddonId.Id == "test");

        var ex = Record.Exception(() => _viewModel.StartAddonCommand.Execute("not a port"));
        Assert.Null(ex);
    }

    [Fact]
    public void StartCampaign_BasePort_Completes()
    {
        var parsed = ParsedAddonFileHelper.CreateParsedAddonFile("start-test", "Start Test", "1.0", AddonTypeEnum.TC);
        _installedAddonsProvider.AddAddon(parsed);

        _viewModel.SelectedAddon = _installedAddonsProvider.GetInstalledAddonsByType(AddonTypeEnum.TC)
                                                           .First(a => a.AddonId.Id == "start-test");

        var stubPort = new StubPort();

        var ex = Record.Exception(() => _viewModel.StartAddonCommand.Execute(stubPort));

        Assert.Null(ex);
    }

}
