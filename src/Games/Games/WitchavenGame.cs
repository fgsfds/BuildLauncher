using Core.All.Enums;

namespace Games.Games;

/// <summary>
///     Represents the game Witchaven and its associated addon detection.
/// </summary>
public sealed class WitchavenGame : BaseGame
{
    /// <inheritdoc />
    public override GameEnum GameEnum => GameEnum.Witchaven;

    /// <inheritdoc />
    public override string FullName => "Witchaven";

    /// <inheritdoc />
    public override string ShortName => FullName;

    private static readonly List<string> _requiredFiles =
    [
        "JOESND",
        "SONGS",
        .. GenerateNumberedFiles("TILES", "ART", 0, 11, 3),
        .. GenerateNumberedFiles("LEVEL", "MAP", 1, 26, 0)
    ];

    /// <inheritdoc />
    protected override IReadOnlyList<string> RequiredFiles => _requiredFiles;

    private static readonly List<string> _witchaven2RequiredFiles =
    [
        "JOESND",
        "W_SONGS",
        .. GenerateNumberedFiles("TILES", "ART", 0, 16, 3),
        .. GenerateNumberedFiles("LEVEL", "MAP", 1, 16, 0)
    ];

    /// <summary>
    ///     Files required for Witchaven 2.
    /// </summary>
    private IReadOnlyList<string> Witchaven2RequiredFiles => _witchaven2RequiredFiles;

    /// <summary>
    ///     Path to Witchaven 2 install folder.
    /// </summary>
    public string? Witchaven2InstallPath { get; set; }

    /// <summary>
    ///     Is Witchaven 2 installed.
    /// </summary>
    public bool IsWitchaven2Installed => IsInstalled(Witchaven2RequiredFiles, Witchaven2InstallPath);

    /// <inheritdoc />
    public override Enum? Skills => null;
}
