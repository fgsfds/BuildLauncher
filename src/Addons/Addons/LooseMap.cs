namespace Addons.Addons;

/// <summary>
///     Represents a loose .map file.
/// </summary>
public sealed class LooseMap : BaseAddon
{
    /// <summary>
    ///     Associated Blood .ini file, if any.
    /// </summary>
    public required string? BloodIni { get; init; }
}
