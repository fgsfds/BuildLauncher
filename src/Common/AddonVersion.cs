using System.Diagnostics.CodeAnalysis;

namespace Common;

public readonly struct AddonVersion
{
    public required readonly string Id { get; init; }
    public required readonly string? Version{ get; init; }

    [SetsRequiredMembers]
    public AddonVersion(
        string title, 
        string? version
        )
    {
        Id = title;
        Version = version;
    }

    [SetsRequiredMembers]
    public AddonVersion(
        string title
        )
    {
        Id = title;
        Version = null;
    }
}
