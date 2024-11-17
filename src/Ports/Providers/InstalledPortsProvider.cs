﻿using Common.Client.Helpers;
using Common.Client.Interfaces;
using Common.Enums;
using CommunityToolkit.Diagnostics;
using Database.Client;
using Database.Client.DbEntities;
using Ports.Ports;
using Ports.Ports.EDuke32;
using System.Collections.Immutable;

namespace Ports.Providers;

/// <summary>
/// Class that provides singleton instances of port types
/// </summary>
public sealed class InstalledPortsProvider
{
    private readonly DatabaseContextFactory _databaseContextFactory;

    private readonly List<BasePort> _builtInPorts;
    private readonly List<CustomPort> _customPorts = [];

    public event EventHandler? CustomPortChangedEvent;

    public BuildGDX BuildGDX { get; init; }
    public EDuke32 EDuke32 { get; init; }
    public NBlood NBlood { get; init; }
    public NotBlood NotBlood { get; init; }
    public PCExhumed PCExhumed { get; init; }
    public Raze Raze { get; init; }
    public RedNukem RedNukem { get; init; }
    public VoidSW VoidSW { get; init; }
    public Fury Fury { get; init; }


    public InstalledPortsProvider(
        IConfigProvider config,
        DatabaseContextFactory databaseContextFactory
        )
    {
        _databaseContextFactory = databaseContextFactory;

        if (!Directory.Exists(ClientProperties.PortsFolderPath))
        {
            _ = Directory.CreateDirectory(ClientProperties.PortsFolderPath);
        }

        BuildGDX = new();
        EDuke32 = new();
        NBlood = new();
        NotBlood = new();
        PCExhumed = new();
        Raze = new();
        RedNukem = new();
        VoidSW = new();
        Fury = new(config);

        _builtInPorts = [BuildGDX, EDuke32, NBlood, NotBlood, PCExhumed, Raze, RedNukem, VoidSW, Fury];

        UpdateCustomPortsList();
    }


    /// <summary>
    /// Get list of ports that support selected game
    /// </summary>
    /// <param name="game">Game enum</param>
    public IEnumerable<BasePort> GetPortsThatSupportGame(GameEnum game) => _builtInPorts.Where(x => x.SupportedGames.Contains(game));

    /// <summary>
    /// Get port by enum
    /// </summary>
    /// <param name="portEnum">Port enum</param>
    public BasePort GetPort(PortEnum portEnum)
    {
        return portEnum switch
        {
            PortEnum.BuildGDX => BuildGDX,
            PortEnum.Raze => Raze,
            PortEnum.EDuke32 => EDuke32,
            PortEnum.RedNukem => RedNukem,
            PortEnum.NBlood => NBlood,
            PortEnum.NotBlood => NotBlood,
            PortEnum.VoidSW => VoidSW,
            PortEnum.PCExhumed => PCExhumed,
            PortEnum.Fury => Fury,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<BasePort>()
        };
    }

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
        using var dbContext = _databaseContextFactory.Get();

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
        using var dbContext = _databaseContextFactory.Get();

        var portToDelete = dbContext.CustomPorts.Find(portName);

        Guard.IsNotNull(portToDelete);

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

        var dbContent = _databaseContextFactory.Get();

        foreach (var port in dbContent.CustomPorts.OrderBy(static x => x.Name))
        {
            _customPorts.Add(new() { Name = port.Name, Path = port.PathToExe, BasePort = _builtInPorts.First(x => x.PortEnum == port.PortEnum) });
        }
    }
}

public readonly struct CustomPort
{
    public required readonly string Name { get; init; }
    public required readonly string Path { get; init; }
    public required readonly BasePort BasePort { get; init; }
    public readonly PortEnum PortEnum => BasePort.PortEnum;
}