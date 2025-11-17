using Common.All.Enums;

namespace Ports.Ports;

public sealed class CustomPort
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required BasePort BasePort { get; init; }
    public PortEnum PortEnum => BasePort.PortEnum;
}
