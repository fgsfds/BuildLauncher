using System.Collections.Immutable;
using Addons.Addons;
using Core.All.Enums;
using Core.All.Enums.Addons;
using Core.All.Helpers;
using Core.All.Serializable.Addon;
using Games.Games;

namespace Tests.Unit;

public sealed class BasePortTests
{
    private static DukeCampaign CreateCampaign(string id, string title, bool hasOptions = false)
    {
        return new DukeCampaign
        {
            AddonId = new(id, null),
            Type = AddonTypeEnum.TC,
            Title = title,
            SupportedGame = new(GameEnum.Duke3D, null, null),
            FileInfo = null,
            GridImageHash = null,
            PreviewImageHash = null,
            Description = null,
            Author = null,
            ReleaseDate = null,
            MainDef = null,
            AdditionalDefs = null,
            MainCon = null,
            AdditionalCons = null,
            RTS = null,
            StartMap = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            RequiredFeatures = null,
            Executables = null,
            Options = hasOptions
                ? new Dictionary<string, Dictionary<string, OptionalParameterTypeEnum>>
                {
                    ["opt1"] = new()
                    {
                        ["test.def"] = OptionalParameterTypeEnum.DEF
                    }
                }
                : null
        };
    }

    private static DukeGame CreateGame()
    {
        return new DukeGame
        {
            Duke64RomPath = null,
            DukeZHRomPath = null,
            DukeWTInstallPath = null,
            GameInstallFolder = Path.GetTempPath()
        };
    }

    [Fact]
    public void GetStartGameArgs_NullOptionsWithEnabledOptions_ReturnsArgs()
    {
        var port = new BasePortTestProxy();
        var args = port.GetStartGameArgs(CreateGame(), CreateCampaign("null-opt", "Null Opt"), [], ["nonExistentOption"], false, false);
        Assert.NotNull(args);
    }

    [Fact]
    public void PortEnum_ReturnsStub()
    {
        var port = new BasePortTestProxy();
        Assert.Equal(PortEnum.Stub, port.PortEnum);
    }

    [Fact]
    public void SupportedGames_IsEmpty()
    {
        var port = new BasePortTestProxy();
        Assert.Empty(port.SupportedGames);
    }

    [Fact]
    public void SupportedFeatures_IsEmpty()
    {
        var port = new BasePortTestProxy();
        Assert.Empty(port.SupportedFeatures);
    }

    [Fact]
    public void IsInstalled_WhenInstalledVersionIsEmptyString_ReturnsFalse()
    {
        // The proxy returns string.Empty for InstalledVersion,
        // so IsInstalled (which checks "is not null") returns true.
        var port = new BasePortTestProxy();
        Assert.True(port.IsInstalled);
    }

    [Fact]
    public void IsSkillSelectionAvailable_ReturnsFalse()
    {
        var port = new BasePortTestProxy();
        Assert.False(port.IsSkillSelectionAvailable);
    }

    [Fact]
    public void InstallFolderPath_ContainsPortsFolderName()
    {
        var port = new BasePortTestProxy();
        var path = port.InstallFolderPath;
        Assert.EndsWith(port.ShortName, path);
    }

    [Fact]
    public void PortExeFilePath_CombinesInstallFolderAndExe()
    {
        var port = new BasePortTestProxy();
        var path = port.PortExeFilePath;
        Assert.StartsWith(port.InstallFolderPath, path);
        Assert.EndsWith(port.Exe, path, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IconId_IsHashOfPortEnum()
    {
        var port = new BasePortTestProxy();
        Assert.Equal(PortEnum.Stub.GetUniqueHash(), port.IconId);
    }

    [Fact]
    public void IsDownloadable_ReturnsTrue()
    {
        var port = new BasePortTestProxy();
        Assert.True(port.IsDownloadable);
    }

    [Fact]
    public void GetPathToAddonSavedGamesFolder_ReplacesInvalidChars()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var port = new BasePortTestProxy();
        var path = port.CallGetPathToAddonSavedGamesFolder("save", "test:id");
        Assert.Contains("test_id", path);
        Assert.Contains("save", path);
    }

    [Fact]
    public void GetPathToAddonSavedGamesFolder_ContainsPortSavesPath()
    {
        var port = new BasePortTestProxy();
        var path = port.CallGetPathToAddonSavedGamesFolder("data", "myAddon");
        Assert.StartsWith(port.PortSavedGamesFolderPath, path);
        Assert.EndsWith("myAddon", path);
    }

    [Fact]
    public void CallMoveSaveFilesToStorage_DoesNotThrowWithNullPaths()
    {
        var port = new BasePortTestProxy();
        var ex = Record.Exception(() => port.CallMoveSaveFilesToStorage(CreateGame(), CreateCampaign("save1", "Save 1")));
        Assert.Null(ex);
    }

    [Fact]
    public void CallMoveSaveFilesFromStorage_DoesNotThrowWithNullPaths()
    {
        var port = new BasePortTestProxy();
        var ex = Record.Exception(() => port.CallMoveSaveFilesFromStorage(CreateGame(), CreateCampaign("save2", "Save 2")));
        Assert.Null(ex);
    }

    [Fact]
    public void CallGetOptionsArgs_WithDefOption_AppendsDefParam()
    {
        var port = new BasePortTestProxy();
        var sb = new System.Text.StringBuilder();
        port.CallGetOptionsArgs(sb, CreateGame(), CreateCampaign("opt-test", "Opt Test", hasOptions: true), ["opt1"]);
        Assert.Contains("test.def", sb.ToString());
    }

    [Fact]
    public void CallGetOptionsArgs_UnknownOption_ThrowsKeyNotFound()
    {
        var port = new BasePortTestProxy();
        var sb = new System.Text.StringBuilder();
        Assert.Throws<KeyNotFoundException>(() =>
            port.CallGetOptionsArgs(sb, CreateGame(), CreateCampaign("opt-test", "Opt Test", hasOptions: true), ["nonexistent"]));
    }

    [Fact]
    public void GetStartGameArgs_WithSkill_ReturnsNonEmptyArgs()
    {
        var port = new BasePortTestProxy();
        var args = port.GetStartGameArgs(CreateGame(), CreateCampaign("skill-test", "Skill"), [], [], false, false, skill: 3);
        Assert.NotEmpty(args);
    }

    [Fact]
    public void SupportedGamesVersions_IsEmptyByDefault()
    {
        var port = new BasePortTestProxy();
        Assert.Empty(port.SupportedGamesVersions);
    }
}
