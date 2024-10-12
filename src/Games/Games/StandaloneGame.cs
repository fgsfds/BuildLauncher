using Common.Enums;

namespace Games.Games;

public sealed class StandaloneGame : BaseGame
{
    /// <inheritdoc/>
    public override GameEnum GameEnum => GameEnum.Standalone;

    /// <inheritdoc/>
    public override string FullName => "Standalone";

    /// <inheritdoc/>
    public override string ShortName => FullName;

    /// <inheritdoc/>
    public override List<string> RequiredFiles => [];
}
