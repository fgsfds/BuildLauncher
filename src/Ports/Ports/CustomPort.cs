using Core.All.Enums;

namespace Ports.Ports;

/// <summary>
///     Represents a user-defined custom port override.
/// </summary>
public sealed class CustomPort
{
    /// <summary>
    ///     Name of the custom port.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Path to the custom port executable.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    ///     Base port type that this custom port is based on.
    /// </summary>
    public required BasePort BasePort { get; init; }

    /// <summary>
    ///     Port enum of the underlying base port.
    /// </summary>
    public PortEnum PortEnum => BasePort.PortEnum;
}
