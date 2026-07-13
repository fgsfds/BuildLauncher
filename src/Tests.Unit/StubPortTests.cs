using Core.All.Enums;
using Core.All.Helpers;
using Ports.Ports;

namespace Tests.Unit;

public sealed class StubPortTests
{
    private readonly StubPort _port = new();

    [Fact]
    public void PortEnum_ReturnsStub()
    {
        Assert.Equal(PortEnum.Stub, _port.PortEnum);
    }

    [Fact]
    public void Name_ReturnsStub()
    {
        Assert.Equal("Stub", (string?)_port.Name);
    }

    [Fact]
    public void SupportedGames_IsEmpty()
    {
        Assert.NotNull(_port.SupportedGames);
    }

    [Fact]
    public void SupportedFeatures_IsEmpty()
    {
        Assert.NotNull(_port.SupportedFeatures);
    }

    [Fact]
    public void IsDownloadable_ReturnsTrue()
    {
        Assert.True((bool)_port.IsDownloadable);
    }

    [Fact]
    public void IsSkillSelectionAvailable_ReturnsFalse()
    {
        Assert.False((bool)_port.IsSkillSelectionAvailable);
    }

    [Fact]
    public void InstalledVersion_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, (string?)_port.InstalledVersion);
    }

    [Fact]
    public void IsInstalled_WhenVersionIsEmpty_ReturnsTrue()
    {
        Assert.True((bool)_port.IsInstalled);
    }

    [Fact]
    public void Exe_ReturnsPlatformSpecificExe()
    {
        var expected = CommonProperties.OSEnum is OSEnum.Windows ? "stub.exe" : string.Empty;
        Assert.Equal(expected, (string?)_port.Exe);
    }

    [Fact]
    public void PortExeFilePath_ContainsInstallFolder()
    {
        var path = _port.PortExeFilePath;
        Assert.StartsWith(_port.InstallFolderPath, (string?)path);
    }
}
