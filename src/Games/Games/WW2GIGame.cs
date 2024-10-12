using Common.Enums;

namespace Games.Games;

public sealed class WW2GIGame : BaseGame
{
    /// <inheritdoc/>
    public override GameEnum GameEnum => GameEnum.WW2GI;

    /// <inheritdoc/>
    public override string FullName => "World War II GI";

    /// <inheritdoc/>
    public override string ShortName => "WW2GI";

    /// <inheritdoc/>
    public override List<string> RequiredFiles => ["WW2GI.GRP"];

    /// <inheritdoc/>
    private List<string> PlatoonFiles => ["PLATOONL.DAT", "PLATOONL.DEF"];

    public bool IsPlatoonInstalled => IsInstalled(PlatoonFiles);
}
