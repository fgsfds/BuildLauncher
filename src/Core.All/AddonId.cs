using System.Diagnostics.CodeAnalysis;
using Core.All.Helpers;

namespace Core.All;

/// <summary>
///     Represents a unique identifier for an addon, consisting of a title and an optional version.
/// </summary>
public sealed class AddonId : IEquatable<AddonId>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonId" /> class.
    /// </summary>
    /// <param name="title">Addon title.</param>
    /// <param name="version">Optional addon version.</param>
    public AddonId(string title, string? version)
    {
        Id = title;
        Version = version;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonId" /> class with no version.
    /// </summary>
    /// <param name="title">Addon title.</param>
    public AddonId(string title)
    {
        Id = title;
        Version = null;
    }

    /// <summary>
    ///     Gets the addon title.
    /// </summary>
    public string Id { get; }

    /// <summary>
    ///     Gets the optional addon version.
    /// </summary>
    public string? Version { get; }

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

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is AddonId other && Equals(other);
    }

    /// <summary>
    ///     Determines whether two <see cref="AddonId" /> instances are equal.
    /// </summary>
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

    /// <summary>
    ///     Determines whether two <see cref="AddonId" /> instances are not equal.
    /// </summary>
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
