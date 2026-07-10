using System.Collections.Immutable;
using Core.All.Enums;
using Ports.Ports;

namespace Ports.Providers;

public interface IPortsProvider
{
    ImmutableList<CustomPort> GetCustomPorts();

    void DeleteCustomPort(string portName);

    void AddOrChangeCustomPort(string? oldName, string newName, string newPath, PortEnum newType);
}
