using System.Collections.Immutable;
using Core.All.Enums;
using Microsoft.EntityFrameworkCore;
using Ports.Ports;
using Ports.Providers;

namespace Tests.Unit;

public sealed class PortsProviderTests : IDisposable
{
    private readonly InMemoryDbContextFactory _dbFactory;
    private readonly PortsProvider _provider;
    private readonly StubPort _stubPort;
    private readonly DosBox _dosBox;

    public PortsProviderTests()
    {
        _dbFactory = new InMemoryDbContextFactory();
        _stubPort = new StubPort();
        _dosBox = new DosBox();
        _provider = new PortsProvider(_dbFactory, [_stubPort, _dosBox]);
    }

    public void Dispose() => _dbFactory.Dispose();

    [Fact]
    public void GetPort_RegisteredPort_ReturnsInstance()
    {
        var port = _provider.GetPort(PortEnum.Stub);
        Assert.Same(_stubPort, port);
    }

    [Fact]
    public void GetPort_ReturnsDosBox()
    {
        var port = _provider.GetPort(PortEnum.DosBox);
        Assert.Same(_dosBox, port);
    }

    [Fact]
    public void GetPort_UnregisteredPort_ThrowsKeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(() => _provider.GetPort(PortEnum.EDuke32));
    }

    [Fact]
    public void GetPortsThatSupportGame_NoMatch_ReturnsEmpty()
    {
        var ports = _provider.GetPortsThatSupportGame(GameEnum.Fury);
        Assert.Empty(ports);
    }

    [Fact]
    public void GetPortsThatSupportGame_DosBoxSupportsDuke3D_ReturnsDosBox()
    {
        var ports = _provider.GetPortsThatSupportGame(GameEnum.Duke3D);
        Assert.NotEmpty(ports);
        Assert.Contains(ports, p => p.PortEnum == PortEnum.DosBox);
    }

    [Fact]
    public void GetCustomPorts_InitiallyEmpty()
    {
        var custom = _provider.GetCustomPorts();
        Assert.Empty(custom);
    }

    [Fact]
    public void AddOrChangeCustomPort_NewPort_CanRetrieveByName()
    {
        _provider.AddOrChangeCustomPort(null, "MyCustom", "C:\\test.exe", PortEnum.DosBox);

        var custom = _provider.GetCustomPorts();
        Assert.Single(custom);
        Assert.Equal("MyCustom", custom[0].Name);
        Assert.Equal("C:\\test.exe", custom[0].Path);
        Assert.Equal(PortEnum.DosBox, custom[0].PortEnum);
    }

    [Fact]
    public void AddOrChangeCustomPort_EditExisting_UpdatesValues()
    {
        _provider.AddOrChangeCustomPort(null, "EditMe", "C:\\old.exe", PortEnum.DosBox);

        _provider.AddOrChangeCustomPort("EditMe", "EditMe", "C:\\new.exe", PortEnum.DosBox);

        var custom = _provider.GetCustomPorts();
        Assert.Single(custom);
        Assert.Equal("C:\\new.exe", custom[0].Path);
    }

    [Fact]
    public void AddOrChangeCustomPort_FiresCustomPortChangedEvent()
    {
        var fired = false;
        ((PortsProvider)_provider).CustomPortChangedEvent += (_, _) => fired = true;

        _provider.AddOrChangeCustomPort(null, "EventPort", "C:\\event.exe", PortEnum.DosBox);

        Assert.True(fired);
    }

    [Fact]
    public void DeleteCustomPort_ExistingPort_RemovesIt()
    {
        _provider.AddOrChangeCustomPort(null, "DeleteMe", "C:\\delete.exe", PortEnum.DosBox);
        Assert.Single(_provider.GetCustomPorts());

        _provider.DeleteCustomPort("DeleteMe");

        Assert.Empty(_provider.GetCustomPorts());
    }

    [Fact]
    public void DeleteCustomPort_NonexistentPort_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _provider.DeleteCustomPort("Nonexistent"));
    }

    [Fact]
    public void DeleteCustomPort_FiresCustomPortChangedEvent()
    {
        var fired = false;
        ((PortsProvider)_provider).CustomPortChangedEvent += (_, _) => fired = true;
        _provider.AddOrChangeCustomPort(null, "FireDelete", "C:\\fire.exe", PortEnum.DosBox);

        _provider.DeleteCustomPort("FireDelete");

        Assert.True(fired);
    }

    [Fact]
    public void GetCustomPorts_ReturnsOrderedByName()
    {
        _provider.AddOrChangeCustomPort(null, "BPort", "b.exe", PortEnum.DosBox);
        _provider.AddOrChangeCustomPort(null, "APort", "a.exe", PortEnum.DosBox);

        var custom = _provider.GetCustomPorts();
        Assert.Equal(2, custom.Count);
        Assert.Equal("APort", custom[0].Name);
        Assert.Equal("BPort", custom[1].Name);
    }

    [Fact]
    public void GetCustomPorts_WithGameFilter_ReturnsOnlyMatching()
    {
        _provider.AddOrChangeCustomPort(null, "Filtered", "f.exe", PortEnum.DosBox);

        var allCustom = _provider.GetCustomPorts();
        Assert.Single(allCustom);
    }

    [Fact]
    public void CustomPort_PortEnum_MatchesBasePortPortEnum()
    {
        _provider.AddOrChangeCustomPort(null, "CheckEnum", "e.exe", PortEnum.DosBox);

        var custom = _provider.GetCustomPorts();
        Assert.Single(custom);
        Assert.Equal(PortEnum.DosBox, custom[0].PortEnum);
    }
}
