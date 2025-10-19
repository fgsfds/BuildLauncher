using System.Diagnostics.CodeAnalysis;
using Common.All.Helpers;
using CommunityToolkit.Diagnostics;

namespace Common.All;

public sealed class AddonId
{
    public string Id { get; }
    public string? Version { get; }

    [SetsRequiredMembers]
    public AddonId(
        string title,
        string? version
        )
    {
        Id = title;
        Version = version;
    }

    [SetsRequiredMembers]
    public AddonId(
        string title
        )
    {
        Id = title;
        Version = null;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not AddonId addon)
        {
            ThrowHelper.ThrowArgumentNullException(nameof(obj));
            return false;
        }

        if (!Id.Equals(addon.Id, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return VersionComparer.Compare(Version, addon.Version, "==");
    }

    public static bool operator ==(AddonId left, AddonId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AddonId left, AddonId right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        var str = Id + Version;
        return str.GetHashCode();
    }
}
