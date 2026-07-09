using Core.All;
using Core.Client.Config;
using Core.Client.Enums;
using Database.Client;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests.Unit;

internal sealed class InMemoryDbContextFactory : IDbContextFactory<DatabaseContext>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<DatabaseContext> _options;

    public InMemoryDbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<DatabaseContext>()
                  .UseSqlite(_connection)
                  .Options;

        using var context = CreateDbContext();
        context.Database.EnsureCreated();
    }

    public DatabaseContext CreateDbContext()
    {
        return new DatabaseContext(_options);
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}


public sealed class ConfigProviderTests : IDisposable
{
    private readonly InMemoryDbContextFactory _dbContextFactory;
    private readonly ConfigProvider _provider;

    public ConfigProviderTests()
    {
        _dbContextFactory = new InMemoryDbContextFactory();
        _provider = new ConfigProvider(_dbContextFactory);
    }

    public void Dispose()
    {
        _dbContextFactory.Dispose();
    }

    [Fact]
    public void Theme_Default_ReturnsSystem()
    {
        Assert.Equal(ThemeEnum.System, _provider.Theme);
    }

    [Fact]
    public void Theme_SetSystem_ReturnsSystem()
    {
        _provider.Theme = ThemeEnum.Dark;
        _provider.Theme = ThemeEnum.System;
        Assert.Equal(ThemeEnum.System, _provider.Theme);
    }

    [Fact]
    public void SkipIntro_Default_ReturnsFalse()
    {
        Assert.False(_provider.SkipIntro);
    }

    [Fact]
    public void SkipStartup_Default_ReturnsFalse()
    {
        Assert.False(_provider.SkipStartup);
    }

    [Fact]
    public void UseLocalApi_Default_ReturnsFalse()
    {
        Assert.False(_provider.UseLocalApi);
    }

    [Fact]
    public void ApiPassword_Default_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, _provider.ApiPassword);
    }

    [Fact]
    public void IsConsented_Default_ReturnsFalse()
    {
        Assert.False(_provider.IsConsented);
    }

    [Fact]
    public void PathDuke3D_Default_ReturnsNull()
    {
        Assert.Null(_provider.PathDuke3D);
    }

    [Fact]
    public void PathDuke3D_TrailingSeparator_IsTrimmed()
    {
        var basePath = Path.Combine("C:\\", "Duke3D");
        _provider.PathDuke3D = basePath + Path.DirectorySeparatorChar;
        Assert.Equal(basePath, _provider.PathDuke3D);
    }

    [Fact]
    public void AllGamePaths_CanStoreAndRetrieve()
    {
        _provider.PathDuke3D = "a";
        _provider.PathDukeWT = "b";
        _provider.PathDuke64 = "c";
        _provider.PathDukeZH = "d";
        _provider.PathWang = "e";
        _provider.PathBlood = "f";
        _provider.PathRedneck = "g";
        _provider.PathRidesAgain = "h";
        _provider.PathSlave = "i";
        _provider.PathFury = "j";
        _provider.PathNam = "k";
        _provider.PathWW2GI = "l";
        _provider.PathWitchaven = "m";
        _provider.PathWitchaven2 = "n";
        _provider.PathTekWar = "o";

        Assert.Equal("a", _provider.PathDuke3D);
        Assert.Equal("b", _provider.PathDukeWT);
        Assert.Equal("c", _provider.PathDuke64);
        Assert.Equal("d", _provider.PathDukeZH);
        Assert.Equal("e", _provider.PathWang);
        Assert.Equal("f", _provider.PathBlood);
        Assert.Equal("g", _provider.PathRedneck);
        Assert.Equal("h", _provider.PathRidesAgain);
        Assert.Equal("i", _provider.PathSlave);
        Assert.Equal("j", _provider.PathFury);
        Assert.Equal("k", _provider.PathNam);
        Assert.Equal("l", _provider.PathWW2GI);
        Assert.Equal("m", _provider.PathWitchaven);
        Assert.Equal("n", _provider.PathWitchaven2);
        Assert.Equal("o", _provider.PathTekWar);
    }

    [Fact]
    public void Rating_Default_ReturnsEmpty()
    {
        Assert.Empty(_provider.Rating);
    }

    [Fact]
    public void AddScore_NewEntry_AddsToRating()
    {
        _provider.AddScore("addon1", 5);
        Assert.Equal((byte)5, _provider.Rating["addon1"]);
    }

    [Fact]
    public void AddScore_ExistingEntry_UpdatesRating()
    {
        _provider.AddScore("addon1", 3);
        _provider.AddScore("addon1", 5);
        Assert.Equal((byte)5, _provider.Rating["addon1"]);
    }

    [Fact]
    public void AddScore_MultipleAddons_StoresAll()
    {
        _provider.AddScore("a", 1);
        _provider.AddScore("b", 2);
        Assert.Equal(2, _provider.Rating.Count);
    }

    [Fact]
    public void AddScore_MaxByteValue_StoresCorrectly()
    {
        _provider.AddScore("addon1", 255);
        Assert.Equal((byte)255, _provider.Rating["addon1"]);
    }

    [Fact]
    public void Playtimes_Default_ReturnsEmpty()
    {
        Assert.Empty(_provider.Playtimes);
    }

    [Fact]
    public void AddPlaytime_NewEntry_AddsToPlaytimes()
    {
        _provider.AddPlaytime("addon1", TimeSpan.FromHours(1));
        Assert.Equal(TimeSpan.FromHours(1), _provider.Playtimes["addon1"]);
    }

    [Fact]
    public void AddPlaytime_ExistingEntry_Accumulates()
    {
        _provider.AddPlaytime("addon1", TimeSpan.FromMinutes(30));
        _provider.AddPlaytime("addon1", TimeSpan.FromMinutes(45));
        Assert.Equal(TimeSpan.FromMinutes(75), _provider.Playtimes["addon1"]);
    }

    [Fact]
    public void AddPlaytime_MultipleAddons_StoresSeparately()
    {
        _provider.AddPlaytime("a", TimeSpan.FromMinutes(10));
        _provider.AddPlaytime("b", TimeSpan.FromMinutes(20));
        Assert.Equal(2, _provider.Playtimes.Count);
        Assert.Equal(TimeSpan.FromMinutes(10), _provider.Playtimes["a"]);
        Assert.Equal(TimeSpan.FromMinutes(20), _provider.Playtimes["b"]);
    }

    [Fact]
    public void AddPlaytime_ZeroTimeSpan_StoresCorrectly()
    {
        _provider.AddPlaytime("addon1", TimeSpan.Zero);
        Assert.Equal(TimeSpan.Zero, _provider.Playtimes["addon1"]);
    }

    [Fact]
    public void DisabledAutoloadMods_Default_ReturnsEmpty()
    {
        Assert.Empty(_provider.DisabledAutoloadMods);
    }

    [Fact]
    public void ChangeModState_DisableMod_AddsToDisabled()
    {
        _provider.ChangeModState(new AddonId("test-mod"), false);
        Assert.Contains("test-mod", _provider.DisabledAutoloadMods);
    }

    [Fact]
    public void ChangeModState_EnableMod_RemovesFromDisabled()
    {
        var addonId = new AddonId("test-mod");
        _provider.ChangeModState(addonId, false);
        _provider.ChangeModState(addonId, true);
        Assert.DoesNotContain("test-mod", _provider.DisabledAutoloadMods);
    }

    [Fact]
    public void ChangeModState_EnableNotDisabled_DoesNothing()
    {
        _provider.ChangeModState(new AddonId("test-mod"), true);
        Assert.Empty(_provider.DisabledAutoloadMods);
    }

    [Fact]
    public void ChangeModState_DisableAlreadyDisabled_DoesNothing()
    {
        var addonId = new AddonId("test-mod");
        _provider.ChangeModState(addonId, false);
        _provider.ChangeModState(addonId, false);
        Assert.Equal(1, _provider.DisabledAutoloadMods.Count);
    }

    [Fact]
    public void ChangeModState_MultipleMods_TracksIndependently()
    {
        _provider.ChangeModState(new AddonId("mod-a"), false);
        _provider.ChangeModState(new AddonId("mod-b"), false);
        Assert.Equal(2, _provider.DisabledAutoloadMods.Count);
        _provider.ChangeModState(new AddonId("mod-a"), true);
        Assert.Equal(1, _provider.DisabledAutoloadMods.Count);
        Assert.DoesNotContain("mod-a", _provider.DisabledAutoloadMods);
        Assert.Contains("mod-b", _provider.DisabledAutoloadMods);
    }

    [Fact]
    public void FavoriteAddons_Default_ReturnsEmpty()
    {
        Assert.Empty(_provider.FavoriteAddons);
    }

    [Fact]
    public void ChangeFavoriteState_Enable_AddsToFavorites()
    {
        var addonId = new AddonId("fav-addon", "1.0");
        _provider.ChangeFavoriteState(addonId, true);
        Assert.Contains(addonId, _provider.FavoriteAddons);
    }

    [Fact]
    public void ChangeFavoriteState_Disable_RemovesFromFavorites()
    {
        var addonId = new AddonId("fav-addon", "1.0");
        _provider.ChangeFavoriteState(addonId, true);
        _provider.ChangeFavoriteState(addonId, false);
        Assert.DoesNotContain(addonId, _provider.FavoriteAddons);
    }

    [Fact]
    public void ChangeFavoriteState_DisableNonexistent_DoesNothing()
    {
        _provider.ChangeFavoriteState(new AddonId("nonexistent"), false);
        Assert.Empty(_provider.FavoriteAddons);
    }

    [Fact]
    public void ChangeFavoriteState_Versionless_Works()
    {
        var addonId = new AddonId("fav-addon");
        _provider.ChangeFavoriteState(addonId, true);
        Assert.Contains(addonId, _provider.FavoriteAddons);
    }

    [Fact]
    public void ChangeFavoriteState_EnableTwice_DoesntDuplicate()
    {
        var addonId = new AddonId("fav-addon");
        _provider.ChangeFavoriteState(addonId, true);
        _provider.ChangeFavoriteState(addonId, true);
        Assert.Contains(addonId, _provider.FavoriteAddons);
    }

    [Fact]
    public void GetEnabledOptions_NoOptions_ReturnsEmpty()
    {
        Assert.Empty(_provider.GetEnabledOptions("addon1"));
    }

    [Fact]
    public void ChangeAddonOptionState_EnableNewOption_AddsOption()
    {
        _provider.ChangeAddonOptionState("addon1", "option1", true);
        Assert.Contains("option1", _provider.GetEnabledOptions("addon1"));
    }

    [Fact]
    public void ChangeAddonOptionState_EnableMultipleOptions_AddsAll()
    {
        _provider.ChangeAddonOptionState("addon1", "opt1", true);
        _provider.ChangeAddonOptionState("addon1", "opt2", true);
        var options = _provider.GetEnabledOptions("addon1");
        Assert.Contains("opt1", options);
        Assert.Contains("opt2", options);
    }

    [Fact]
    public void ChangeAddonOptionState_EnableDuplicateOptions_DoesntDuplicate()
    {
        _provider.ChangeAddonOptionState("addon1", "opt1", true);
        _provider.ChangeAddonOptionState("addon1", "opt1", true);
        Assert.Single(_provider.GetEnabledOptions("addon1"));
    }

    [Fact]
    public void ChangeAddonOptionState_DisableOption_RemovesOption()
    {
        _provider.ChangeAddonOptionState("addon1", "opt1", true);
        _provider.ChangeAddonOptionState("addon1", "opt2", true);
        _provider.ChangeAddonOptionState("addon1", "opt1", false);
        var options = _provider.GetEnabledOptions("addon1");
        Assert.DoesNotContain("opt1", options);
        Assert.Contains("opt2", options);
    }

    [Fact]
    public void ChangeAddonOptionState_DisableNonExistingOption_DoesNothing()
    {
        _provider.ChangeAddonOptionState("nonexistent", "opt", false);
        Assert.Empty(_provider.GetEnabledOptions("nonexistent"));
    }

    [Fact]
    public void ChangeAddonOptionState_DisableLastOption_RemovesOption()
    {
        _provider.ChangeAddonOptionState("addon1", "opt1", true);
        _provider.ChangeAddonOptionState("addon1", "opt1", false);
        Assert.DoesNotContain("opt1", _provider.GetEnabledOptions("addon1"));
    }

    [Fact]
    public void ChangeAddonOptionState_DifferentAddons_Independent()
    {
        _provider.ChangeAddonOptionState("a", "opt", true);
        _provider.ChangeAddonOptionState("b", "other", true);
        Assert.Contains("opt", _provider.GetEnabledOptions("a"));
        Assert.Contains("other", _provider.GetEnabledOptions("b"));
    }

    [Fact]
    public void SettingChange_FiresEvent()
    {
        string? firedParam = null;
        _provider.ParameterChangedEvent += param => firedParam = param;
        _provider.Theme = ThemeEnum.Dark;
        Assert.Equal(nameof(ConfigProvider.Theme), firedParam);
    }

    [Fact]
    public void GamePathChange_FiresEvent()
    {
        string? firedParam = null;
        _provider.ParameterChangedEvent += param => firedParam = param;
        _provider.PathDuke3D = Path.Combine("C:\\", "Duke3D");
        Assert.Equal(nameof(ConfigProvider.PathDuke3D), firedParam);
    }

    [Fact]
    public void AddScore_FiresEvent()
    {
        string? firedParam = null;
        _provider.ParameterChangedEvent += param => firedParam = param;
        _provider.AddScore("addon1", 5);
        Assert.Equal(nameof(ConfigProvider.Rating), firedParam);
    }

    [Fact]
    public void AddPlaytime_FiresEvent()
    {
        string? firedParam = null;
        _provider.ParameterChangedEvent += param => firedParam = param;
        _provider.AddPlaytime("addon1", TimeSpan.FromHours(1));
        Assert.Equal(nameof(ConfigProvider.Playtimes), firedParam);
    }

    [Fact]
    public void ChangeModState_FiresEvent()
    {
        string? firedParam = null;
        _provider.ParameterChangedEvent += param => firedParam = param;
        _provider.ChangeModState(new AddonId("test"), false);
        Assert.Equal(nameof(ConfigProvider.DisabledAutoloadMods), firedParam);
    }

    [Fact]
    public void ChangeFavoriteState_FiresEvent()
    {
        string? firedParam = null;
        _provider.ParameterChangedEvent += param => firedParam = param;
        _provider.ChangeFavoriteState(new AddonId("test"), true);
        Assert.Equal(nameof(ConfigProvider.FavoriteAddons), firedParam);
    }

    [Fact]
    public void MultipleEvents_EachFiresSeparately()
    {
        var firedParams = new List<string?>();
        _provider.ParameterChangedEvent += param => firedParams.Add(param);
        _provider.Theme = ThemeEnum.Dark;
        _provider.PathDuke3D = Path.Combine("C:\\", "Duke");
        _provider.AddScore("addon1", 5);
        Assert.Equal(3, firedParams.Count);
        Assert.Equal(nameof(ConfigProvider.Theme), firedParams[0]);
        Assert.Equal(nameof(ConfigProvider.PathDuke3D), firedParams[1]);
        Assert.Equal(nameof(ConfigProvider.Rating), firedParams[2]);
    }

    [Fact]
    public void SettingPersistsAcrossNewProvider()
    {
        _provider.Theme = ThemeEnum.Dark;
        _provider.PathDuke3D = Path.Combine("C:\\", "Duke3D");
        _provider.AddScore("addon1", 5);

        var freshProvider = new ConfigProvider(_dbContextFactory);
        Assert.Equal(ThemeEnum.Dark, freshProvider.Theme);
        Assert.Equal(Path.Combine("C:\\", "Duke3D"), freshProvider.PathDuke3D);
        Assert.Equal((byte)5, freshProvider.Rating["addon1"]);
        freshProvider = null;
    }
}
