using Common.All.Enums;

namespace Games.Games;

public sealed class TekWarGame : BaseGame
{
    /// <inheritdoc/>
    public override GameEnum GameEnum => GameEnum.TekWar;

    /// <inheritdoc/>
    public override string FullName => "TekWar";

    /// <inheritdoc/>
    public override string ShortName => FullName;

    /// <inheritdoc/>
    public override List<string> RequiredFiles
    {
        get
        {
            List<string> result = ["SONGS", "SOUNDS"];

            for (var i = 0; i < 16; i++)
            {
                result.Add($"TILES{i:000}.ART");
            }

            return result;
        }
    }

    /// <inheritdoc/>
    public override Enum? Skills => null;
}
