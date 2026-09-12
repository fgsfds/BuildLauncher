using System.Collections.Immutable;
using Core.All.Enums;
using Ports.Ports;

namespace Ports.Providers;

/// <summary>
///     Provides access to the registered and user-defined custom ports.
/// </summary>
public interface IPortsProvider
{
    /// <summary>
    ///     Gets the user-defined custom ports.
    /// </summary>
    /// <returns>
    ///     The custom ports.
    /// </returns>
    ImmutableList<CustomPort> GetCustomPorts();

    /// <summary>
    ///     Deletes the custom port with the specified name.
    /// </summary>
    /// <param name="portName">
    ///     The name of the custom port.
    /// </param>
    void DeleteCustomPort(string portName);

    /// <summary>
    ///     Adds a new custom port or updates an existing one.
    /// </summary>
    /// <param name="oldName">
    ///     The previous name of the port, or
    ///     <c>
    ///         null
    ///     </c>
    ///     when adding.
    /// </param>
    /// <param name="newName">
    ///     The new name of the port.
    /// </param>
    /// <param name="newPath">
    ///     The path to the port executable.
    /// </param>
    /// <param name="newType">
    ///     The port type.
    /// </param>
    void AddOrChangeCustomPort(string? oldName, string newName, string newPath, PortEnum newType);
}
