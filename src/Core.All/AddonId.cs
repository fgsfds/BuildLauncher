using Core.All.Helpers;

namespace Core.All;

/// <summary>
///     Represents a unique identifier for an addon, consisting of a title and an optional version.
/// </summary>
public readonly record struct AddonId
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonId" /> struct.
    /// </summary>
    /// <param name="title">Addon title.</param>
    /// <param name="version">Optional addon version.</param>
    public AddonId(string title, string? version)
    {
        Id = title;
        Version = version;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonId" /> struct with no version.
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
    public bool Equals(AddonId other) =>
        Id.Equals(other.Id, StringComparison.OrdinalIgnoreCase) &&
        VersionComparer.Compare(Version, other.Version, ComparisonOperatorEnum.Equals);

    /// <inheritdoc />
    public override int GetHashCode() =>
        HashCode.Combine(
            Id.GetHashCode(StringComparison.OrdinalIgnoreCase),
            VersionComparer.GetNormalizedHashCode(Version)
        );
}
