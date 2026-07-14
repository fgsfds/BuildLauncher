using Core.All.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Ports.Ports.EDuke32;

namespace Tests.Unit;

public sealed class RedNukemTests
{
    [Fact]
    public void PortEnum_ReturnsRedNukem()
    {
        var port = new RedNukem();
        Assert.Equal(PortEnum.RedNukem, port.PortEnum);
    }

    [Fact]
    public void Name_ReturnsRedNukem()
    {
        var port = new RedNukem();
        Assert.Equal("RedNukem", port.Name);
    }

    [Fact]
    public void SupportedGames_ContainsDukeAndRedneck()
    {
        var port = new RedNukem();
        Assert.Contains(GameEnum.Duke3D, port.SupportedGames);
        Assert.Contains(GameEnum.Redneck, port.SupportedGames);
        Assert.Contains(GameEnum.NAM, port.SupportedGames);
        Assert.Contains(GameEnum.WW2GI, port.SupportedGames);
        Assert.Contains(GameEnum.Duke64, port.SupportedGames);
    }

    [Fact]
    public void SupportedFeatures_ContainsHightile()
    {
        var port = new RedNukem();
        Assert.Contains(FeatureEnum.Hightile, port.SupportedFeatures);
        Assert.Contains(FeatureEnum.Models, port.SupportedFeatures);
    }

    [Fact]
    public void SupportedGamesVersions_ContainsAtomic()
    {
        var port = new RedNukem();
        Assert.Contains("Duke3D_Atomic", port.SupportedGamesVersions);
    }

    [Fact]
    public void ConfigFile_ReturnsRednukemCfg()
    {
        var port = new RedNukem();
        Assert.NotNull(port);
    }
}
