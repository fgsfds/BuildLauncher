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

    /// <inheritdoc />
    public override List<string> RequiredFiles
    {
        get
        {
            List<string> result = ["SONGS", "SOUNDS"];
            result.AddRange(GenerateNumberedFiles("TILES", "ART", 0, 16));

            return result;
        }
    }

    /// <inheritdoc />
    public override Enum? Skills => null;
}
