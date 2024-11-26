using System.Diagnostics.CodeAnalysis;

namespace Common;

public readonly struct AddonVersion
{
    public required readonly string Id { get; init; }
    public required readonly string? Version { get; init; }

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

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not AddonVersion addon)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        if ((Version is null || addon.Version is null) &&
            Id.Equals(addon.Id))
        {
            return true;
        }

        return base.Equals(obj);
    }

    public static bool operator ==(AddonVersion left, AddonVersion right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AddonVersion left, AddonVersion right)
    {
        return !(left == right);
    }
}
