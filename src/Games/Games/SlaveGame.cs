using Common.All.Enums;

namespace Games.Games;

public sealed class SlaveGame : BaseGame
{
    /// <inheritdoc/>
    public override GameEnum GameEnum => GameEnum.Slave;

    /// <inheritdoc/>
    public override string FullName => "Powerslave";

    /// <inheritdoc/>
    public override string ShortName => "Slave";

    /// <inheritdoc/>
    public override List<string> RequiredFiles => ["STUFF.DAT"];
}
