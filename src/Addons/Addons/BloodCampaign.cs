namespace Addons.Addons;

/// <summary>
/// Blood campaign
/// </summary>
public sealed class BloodCampaign : BaseAddon
{
    /// <summary>
    /// Startup .ini file
    /// </summary>
    public required string? INI { get; init; }

    /// <summary>
    /// Main .rff file
    /// </summary>
    public required string? RFF { get; init; }

    /// <summary>
    /// Main .snd file
    /// </summary>
    public required string? SND { get; init; }
}
