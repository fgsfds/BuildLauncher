using Core.All.Enums;

namespace Games.Games;

/// <summary>
///     Represents the game Powerslave and its associated metadata.
/// </summary>
public sealed class SlaveGame : BaseGame
{
    /// <inheritdoc />
    public override GameEnum GameEnum => GameEnum.Slave;

    /// <inheritdoc />
    public override string FullName => "Powerslave";

    /// <inheritdoc />
    public override string ShortName => "Slave";

    /// <inheritdoc />
    public override List<string> RequiredFiles => ["STUFF.DAT"];

    /// <inheritdoc />
    public override Enum? Skills => null;
}
