using System.Collections.Immutable;
using Common.All.Enums;
using Common.Client.Helpers;
using Database.Client;
using Database.Client.DbEntities;
using Microsoft.EntityFrameworkCore;
using Ports.Ports;

namespace Ports.Providers;

/// <summary>
/// Class that provides singleton instances of port types
/// </summary>
public sealed class PortsProvider
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly Dictionary<PortEnum, BasePort> _ports = [];
    private readonly List<CustomPort> _customPorts = [];

    public event EventHandler? CustomPortChangedEvent;

    public PortsProvider(
        IDbContextFactory<DatabaseContext> dbContextFactory,
        IEnumerable<BasePort> ports
        )
    {
        _dbContextFactory = dbContextFactory;

        if (!Directory.Exists(ClientProperties.PortsFolderPath))
        {
            _ = Directory.CreateDirectory(ClientProperties.PortsFolderPath);
        }

        foreach (var port in ports)
        {
            _ports.Add(port.PortEnum, port);
        }

        UpdateCustomPortsList();
    }

    /// <summary>
    /// Get list of ports that support selected game
    /// </summary>
    /// <param name="game">Game enum</param>
    public IReadOnlyList<BasePort> GetPortsThatSupportGame(GameEnum game) => [.. _ports.Values.Where(x => x.SupportedGames.Contains(game))];

    /// <summary>
    /// Get port by enum
    /// </summary>
    /// <param name="portEnum">Port enum</param>
    public BasePort GetPort(PortEnum portEnum) =>
        _ports.TryGetValue(portEnum, out var port) ? port : throw new ArgumentException();

    /// <summary>
    /// Get list of custom ports
    /// </summary>
    public ImmutableList<CustomPort> GetCustomPorts() => [.. _customPorts];

    /// <summary>
    /// Get list of custom ports
    /// </summary>
    public ImmutableList<CustomPort> GetCustomPorts(GameEnum gameEnum) => [.. _customPorts.Where(x => x.BasePort.SupportedGames.Contains(gameEnum))];

    /// <summary>
    /// Add or change custom port
    /// </summary>
    /// <param name="oldName">Old name of the port. Null if new port is being added.</param>
    /// <param name="newName">New name of the port</param>
    /// <param name="newPath">Path to port's exe</param>
    /// <param name="newType">Type of port</param>
    public void AddOrChangeCustomPort(
        string? oldName,
        string newName,
        string newPath,
        PortEnum newType
        )
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var existingPort = dbContext.CustomPorts.Find(oldName);

        if (existingPort is not null)
        {
            _ = dbContext.CustomPorts.Remove(existingPort);
        }

        CustomPortsDbEntity newPortDb = new()
        {
            PortEnum = newType,
            Name = newName,
            PathToExe = newPath
        };

        _ = dbContext.CustomPorts.Add(newPortDb);
        _ = dbContext.SaveChanges();

        UpdateCustomPortsList();

        CustomPortChangedEvent?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Delete custom port
    /// </summary>
    /// <param name="portName">Name of the port</param>
    public void DeleteCustomPort(string portName)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var portToDelete = dbContext.CustomPorts.Find(portName);

        ArgumentNullException.ThrowIfNull(portToDelete);

        _ = dbContext.CustomPorts.Remove(portToDelete);
        _ = dbContext.SaveChanges();

        UpdateCustomPortsList();

        CustomPortChangedEvent?.Invoke(this, EventArgs.Empty);
    }


    /// <summary>
    /// Update list of custom ports
    /// </summary>
    private void UpdateCustomPortsList()
    {
        _customPorts.Clear();

        using var dbContext = _dbContextFactory.CreateDbContext();

        foreach (var port in dbContext.CustomPorts.OrderBy(static x => x.Name))
        {
            _customPorts.Add(new() { Name = port.Name, Path = port.PathToExe, BasePort = _ports.Values.First(x => x.PortEnum == port.PortEnum) });
        }
    }
}
