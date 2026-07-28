using Core.All.Enums;
using Games.Skills;

namespace Games.Games;

/// <summary>
///     Represents the game Shadow Warrior and its associated metadata.
/// </summary>
public sealed class WangGame : BaseGame
{
    /// <inheritdoc />
    public override GameEnum GameEnum => GameEnum.Wang;

    /// <inheritdoc />
    public override string FullName => "Shadow Warrior";

    /// <inheritdoc />
    public override string ShortName => "Wang";

    /// <inheritdoc />
    protected override IReadOnlyCollection<string> RequiredFiles { get; } = ["SW.GRP"];

    /// <inheritdoc />
    public override Enum Skills => new WangSkillsEnum();
}
