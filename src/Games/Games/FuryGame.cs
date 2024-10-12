using Common.Enums;

namespace Games.Games;

public sealed class FuryGame : BaseGame
{
    /// <inheritdoc/>
    public override GameEnum GameEnum => GameEnum.Fury;

    /// <inheritdoc/>
    public override string FullName => "Ion Fury";

    /// <inheritdoc/>
    public override string ShortName => "Fury";

    /// <inheritdoc/>
    public override List<string> RequiredFiles => ["fury.grp"];
}
