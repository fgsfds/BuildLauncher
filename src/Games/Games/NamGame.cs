using Common.All.Enums;
using Games.Skills;

namespace Games.Games;

public sealed class NamGame : BaseGame
{
    /// <inheritdoc/>
    public override GameEnum GameEnum => GameEnum.NAM;

    /// <inheritdoc/>
    public override string FullName => "NAM";

    /// <inheritdoc/>
    public override string ShortName => "NAM";

    /// <inheritdoc/>
    public override List<string> RequiredFiles => ["NAM.GRP"];

    /// <inheritdoc/>
    public override Enum Skills => new NamSkillsEnum();
}
