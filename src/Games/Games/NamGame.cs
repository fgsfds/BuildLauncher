using Core.All.Enums;
using Games.Skills;

namespace Games.Games;

/// <summary>
///     Represents the game NAM and its associated metadata.
/// </summary>
public sealed class NamGame : BaseGame
{
    /// <inheritdoc />
    public override GameEnum GameEnum => GameEnum.NAM;

    /// <inheritdoc />
    public override string FullName => "NAM";

    /// <inheritdoc />
    public override string ShortName => "NAM";

    /// <inheritdoc />
    protected override IReadOnlyList<string> RequiredFiles => ["NAM.GRP"];

    /// <inheritdoc />
    public override Enum Skills => new NamSkillsEnum();
}
