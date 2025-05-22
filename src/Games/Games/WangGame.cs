using Common.Enums;

namespace Games.Games;

public sealed class WangGame : BaseGame
{
    /// <inheritdoc/>
    public override GameEnum GameEnum => GameEnum.Wang;

    /// <inheritdoc/>
    public override string FullName => "Shadow Warrior";

    /// <inheritdoc/>
    public override string ShortName => "Wang";

    /// <inheritdoc/>
    public override List<string> RequiredFiles => ["SW.GRP"];
}
