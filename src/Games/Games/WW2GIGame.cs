using Core.All.Enums;
using Games.Skills;

namespace Games.Games;

/// <summary>
///     Represents the game World War II GI and its associated addon detection.
/// </summary>
public sealed class WW2GIGame : BaseGame
{
    /// <inheritdoc />
    public override GameEnum GameEnum => GameEnum.WW2GI;

    /// <inheritdoc />
    public override string FullName => "World War II GI";

    /// <inheritdoc />
    public override string ShortName => "WW2GI";

    /// <inheritdoc />
    public override List<string> RequiredFiles => ["WW2GI.GRP"];

    /// <summary>
    ///     Files required for Platoon addon.
    /// </summary>
    private List<string> PlatoonFiles => ["PLATOONL.DAT", "PLATOONL.DEF"];

    /// <summary>
    ///     Is Platoon addon installed.
    /// </summary>
    public bool IsPlatoonInstalled => IsInstalled(PlatoonFiles);

    /// <inheritdoc />
    public override Enum Skills => new WWIISkillsEnum();
}
