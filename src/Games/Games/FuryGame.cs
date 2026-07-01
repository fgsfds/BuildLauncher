using Core.All.Enums;
using Games.Skills;

namespace Games.Games;

/// <summary>
///     Represents the game Ion Fury and its associated metadata.
/// </summary>
public sealed class FuryGame : BaseGame
{
    /// <inheritdoc />
    public override GameEnum GameEnum => GameEnum.Fury;

    /// <inheritdoc />
    public override string FullName => "Ion Fury";

    /// <inheritdoc />
    public override string ShortName => "Fury";

    /// <inheritdoc />
    public override List<string> RequiredFiles => ["fury.grp"];

    /// <inheritdoc />
    public override Enum Skills => new FurySkillsEnum();
}
