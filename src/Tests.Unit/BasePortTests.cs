using Addons.Addons;
using Core.All.Enums;
using Games.Games;

namespace Tests.Unit;

/// <summary>
///     Tests for the <see cref="BasePort" /> class.
/// </summary>
public sealed class BasePortTests
{
    /// <summary>
    ///     Tests that GetStartGameArgs returns non-null when enabledOptions is not empty
    ///     but the addon has no options defined.
    /// </summary>
    [Fact]
    public void GetStartGameArgs_NullOptionsWithEnabledOptions_ReturnsArgs()
    {
        var port = new BasePortTestProxy();

        var game = new DukeGame
        {
            Duke64RomPath = null,
            DukeZHRomPath = null,
            DukeWTInstallPath = null,
            GameInstallFolder = Path.GetTempPath()
        };

        var addon = new DukeCampaign
        {
            AddonId = new("null-options-test", null),
            Type = AddonTypeEnum.TC,
            Title = "Null Options Test",
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
            Options = null
        };

        var args = port.GetStartGameArgs(game, addon, [], ["nonExistentOption"], false, false);
        Assert.NotNull(args);
    }
}
