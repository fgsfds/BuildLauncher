using System.Collections.Immutable;
using Core.All.Enums;
using Database.Client;
using Database.Client.DbEntities;
using Microsoft.EntityFrameworkCore;
using Ports.Ports;

namespace Ports.Providers;

/// <summary>
///     Provides singleton instances of port types and manages custom ports.
/// </summary>
public sealed class PortsProvider : IPortsProvider
{
    /// <summary>
    ///     List of custom ports loaded from the database.
    /// </summary>
    private readonly List<CustomPort> _customPorts = [];

    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    /// <summary>
    ///     Dictionary of registered port instances by port enum.
    /// </summary>
    private readonly Dictionary<PortEnum, BasePort> _ports = [];

    /// <summary>
    ///     Initializes a new instance of the <see cref="PortsProvider" /> class.
    /// </summary>
    /// <param name="dbContextFactory">Database context factory.</param>
    /// <param name="ports">Collection of port instances.</param>
    public PortsProvider(
        IDbContextFactory<DatabaseContext> dbContextFactory,
        IEnumerable<BasePort> ports
        )
    {
        _dbContextFactory = dbContextFactory;

        foreach (var port in ports)
        {
            _ports.Add(port.PortEnum, port);
        }

        UpdateCustomPortsList();
    }

    /// <summary>
    ///     Raised when a custom port is added, changed, or deleted.
    /// </summary>
    public event EventHandler? CustomPortChangedEvent;

    /// <summary>
    ///     Gets the list of ports that support the specified game.
    /// </summary>
    /// <param name="game">Game enum.</param>
    public IReadOnlyList<BasePort> GetPortsThatSupportGame(GameEnum game) => [.. _ports.Values.Where(x => x.SupportedGames.Contains(game))];

    /// <summary>
    ///     Gets a port by its enum value.
    /// </summary>
    /// <param name="portEnum">Port enum.</param>
    public BasePort GetPort(PortEnum portEnum) => _ports.TryGetValue(portEnum, out var port) ? port : throw new KeyNotFoundException($"Port '{portEnum}' is not registered.");

    /// <summary>
    ///     Gets the list of all custom ports.
    /// </summary>
    public ImmutableList<CustomPort> GetCustomPorts() => [.. _customPorts];

    /// <summary>
    ///     Gets the list of custom ports that support the specified game.
    /// </summary>
    public ImmutableList<CustomPort> GetCustomPorts(GameEnum gameEnum) => [.. _customPorts.Where(x => x.BasePort.SupportedGames.Contains(gameEnum))];

    /// <summary>
    ///     Adds or changes a custom port.
    /// </summary>
    /// <param name="oldName">Old name of the port. <c>null</c> if a new port is being added.</param>
    /// <param name="newName">New name of the port.</param>
    /// <param name="newPath">Path to the port executable.</param>
    /// <param name="newType">Type of port.</param>
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
    ///     Deletes a custom port.
    /// </summary>
    /// <param name="portName">Name of the port.</param>
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
    ///     Reloads the custom ports list from the database.
    /// </summary>
    private void UpdateCustomPortsList()
    {
        _customPorts.Clear();

        using var dbContext = _dbContextFactory.CreateDbContext();

        foreach (var port in dbContext.CustomPorts.OrderBy(static x => x.Name))
        {
            var basePort = _ports.Values.FirstOrDefault(x => x.PortEnum == port.PortEnum);

            if (basePort is null)
            {
                continue;
            }

            _customPorts.Add(
                new()
                {
                    Name = port.Name,
                    Path = port.PathToExe,
                    BasePort = basePort
                }
                );
        }
    }
}
