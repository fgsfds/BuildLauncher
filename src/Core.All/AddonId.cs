using System.Diagnostics.CodeAnalysis;
using Core.All.Helpers;

namespace Core.All;

public sealed class AddonId : IEquatable<AddonId>
{
    public string Id { get; }
    public string? Version { get; }

    public AddonId(string title, string? version)
    {
        Id = title;
        Version = version;
    }

    public AddonId(string title)
    {
        Id = title;
        Version = null;
    }

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is AddonId other && Equals(other);
    }

    /// <inheritdoc />
    public bool Equals(AddonId? other)
    {
        if (other is null)
        {
            return false;
        }

        return Id.Equals(other.Id, StringComparison.OrdinalIgnoreCase) &&
               VersionComparer.Compare(Version, other.Version, ComparisonOperatorEnum.Equals);
    }

    public static bool operator ==(AddonId? left, AddonId? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(AddonId? left, AddonId? right)
    {
        return !(left == right);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(
            Id.GetHashCode(StringComparison.OrdinalIgnoreCase),
            Version
            );
    }
}
