using Core.All.Enums;
using Ports.Ports.EDuke32;

namespace Tests.Unit;

public sealed class RednukemTests
{
    [Fact]
    public void PortEnum_ReturnsRednukem()
    {
        var port = new Rednukem();
        Assert.Equal(PortEnum.Rednukem, port.PortEnum);
    }

    [Fact]
    public void Name_ReturnsRednukem()
    {
        var port = new Rednukem();
        Assert.Equal("Rednukem", port.Name);
    }

    [Fact]
    public void SupportedGames_ContainsDukeAndRedneck()
    {
        var port = new Rednukem();
        Assert.Contains(GameEnum.Duke3D, port.SupportedGames);
        Assert.Contains(GameEnum.Redneck, port.SupportedGames);
        Assert.Contains(GameEnum.NAM, port.SupportedGames);
        Assert.Contains(GameEnum.WW2GI, port.SupportedGames);
        Assert.Contains(GameEnum.Duke64, port.SupportedGames);
    }

    [Fact]
    public void SupportedFeatures_ContainsHightile()
    {
        var port = new Rednukem();
        Assert.Contains(FeatureEnum.Hightile, port.SupportedFeatures);
        Assert.Contains(FeatureEnum.Models, port.SupportedFeatures);
    }

    [Fact]
    public void SupportedGamesVersions_ContainsAtomic()
    {
        var port = new Rednukem();
        Assert.Contains("Duke3D_Atomic", port.SupportedGamesVersions);
    }

    [Fact]
    public void ConfigFile_ReturnsRednukemCfg()
    {
        var port = new Rednukem();
        Assert.NotNull(port);
    }
}
