using Core.All.Enums;

namespace Games.Games;

/// <summary>
///     Represents the game TekWar and its associated metadata.
/// </summary>
public sealed class TekWarGame : BaseGame
{
    /// <inheritdoc />
    public override GameEnum GameEnum => GameEnum.TekWar;

    /// <inheritdoc />
    public override string FullName => "TekWar";

    /// <inheritdoc />
    public override string ShortName => FullName;

    private static readonly List<string> _requiredFiles =
    [
        "SONGS",
        "SOUNDS",
        .. GenerateNumberedFiles("TILES", "ART", 0, 16, 3)
    ];

    /// <inheritdoc />
    protected override IReadOnlyList<string> RequiredFiles => _requiredFiles;

    /// <inheritdoc />
    public override Enum? Skills => null;
}
