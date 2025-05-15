using Common.Helpers;
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

        if (!Id.Equals(addon.Id, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return VersionComparer.Compare(Version, addon.Version, "==");
    }

    public static bool operator ==(AddonVersion left, AddonVersion right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AddonVersion left, AddonVersion right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        var str = Id + Version;
        return str.GetHashCode();
    }
}
