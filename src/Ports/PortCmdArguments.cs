namespace Ports;

/// <summary>
///     Command-line arguments for building arguments.
/// </summary>
public record PortCmdArguments
{
    /// <summary>
    ///     Command-line parameter for setting game directory.
    /// </summary>
    public required string? AddGameDir { get; init; }

    /// <summary>
    ///     Command-line parameter to add folder to search path.
    /// </summary>
    public required string? AddDirectory { get; init; }

    /// <summary>
    ///     Command-line parameter to load additional file.
    /// </summary>
    public required string? AddFile { get; init; }

    /// <summary>
    ///     Command-line parameter to load main GRP file.
    /// </summary>
    public required string? MainGrp { get; init; }

    /// <summary>
    ///     Command-line parameter to load additional GRP file.
    /// </summary>
    public required string? AddGrp { get; init; }

    /// <summary>
    ///     Command-line parameter to load main Def file.
    /// </summary>
    public required string? MainDef { get; init; }

    /// <summary>
    ///     Command-line parameter to load additional Def file.
    /// </summary>
    public required string? AddDef { get; init; }

    /// <summary>
    ///     Command-line parameter to load main Con file.
    /// </summary>
    public required string? MainCon { get; init; }

    /// <summary>
    ///     Command-line parameter to load additional Con file.
    /// </summary>
    public required string? AddCon { get; init; }

    /// <summary>
    ///     Command-line parameter for skill selection.
    /// </summary>
    public required string? SkillLevel { get; init; }

    /// <summary>
    ///     Command-line parameter for adding main RFF file.
    /// </summary>
    public required string? AddRff { get; init; }

    /// <summary>
    ///     Command-line parameter for adding sound RFF file.
    /// </summary>
    public required string? AddSnd { get; init; }

    /// <summary>
    ///     Command-line parameter to skip the intro.
    /// </summary>
    public required string? SkipIntro { get; init; }

    /// <summary>
    ///     Command-line parameter to skip the startup window.
    /// </summary>
    public required string? SkipStartup { get; init; }

    /// <summary>
    ///     Command-line parameter to skip the Steam/GOG install search.
    /// </summary>
    public required string? SkipSteam { get; init; }
}
