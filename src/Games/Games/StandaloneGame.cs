using Core.All.Enums;

namespace Games.Games;

/// <summary>
///     Represents a standalone game configuration with no required files.
/// </summary>
public sealed class StandaloneGame : BaseGame
{
    /// <inheritdoc />
    public override GameEnum GameEnum => GameEnum.Standalone;

    /// <inheritdoc />
    public override string FullName => "Standalone";

    /// <inheritdoc />
    public override string ShortName => FullName;

    /// <inheritdoc />
    protected override IReadOnlyList<string> RequiredFiles => [];

    /// <inheritdoc />
    public override Enum? Skills => null;
}
