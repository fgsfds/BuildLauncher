using Core.All.Enums;
using Ports.Ports;
using Ports.Ports.EDuke32;

namespace Tests.Unit;

public sealed class CustomPortTests
{
    [Fact]
    public void Constructor_SetsRequiredProperties()
    {
        var basePort = new StubPort();
        var port = new CustomPort
        {
            Name = "MyPort",
            Path = "C:\\myport.exe",
            BasePort = basePort
        };

        Assert.Equal("MyPort", port.Name);
        Assert.Equal("C:\\myport.exe", port.Path);
        Assert.Same(basePort, port.BasePort);
    }

    [Fact]
    public void PortEnum_DelegatesToBasePort()
    {
        var basePort = new StubPort();
        var port = new CustomPort
        {
            Name = "Test",
            Path = "test.exe",
            BasePort = basePort
        };

        Assert.Equal(basePort.PortEnum, port.PortEnum);
    }

    [Fact]
    public void PortEnum_WithDifferentBasePort_ReturnsDifferentPortEnum()
    {
        var eduke32 = new EDuke32();
        var port = new CustomPort
        {
            Name = "CustomEDuke",
            Path = "eduke32.exe",
            BasePort = eduke32
        };

        Assert.NotEqual(PortEnum.Stub, port.PortEnum);
    }
}
