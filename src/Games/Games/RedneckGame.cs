using Common.All.Enums;
using Games.Skills;

namespace Games.Games;

public sealed class RedneckGame : BaseGame
{
    /// <inheritdoc/>
    public override GameEnum GameEnum => GameEnum.Redneck;

    /// <inheritdoc/>
    public override string FullName => "Redneck Rampage";

    /// <inheritdoc/>
    public override string ShortName => "Redneck";

    /// <summary>
    /// Path to Rides Again folder
    /// </summary>
    public required string? AgainInstallPath { get; set; }

    /// <inheritdoc/>
    public override List<string> RequiredFiles => ["REDNECK.GRP"];

    /// <summary>
    /// Is Route 66 installed
    /// </summary>
    public bool IsRoute66Installed => IsInstalled(["TILESA66.ART", "TILESB66.ART", "TURD66.ANM", "TURD66.VOC", "END66.ANM", "END66.VOC", "BUBBA66.CON", "DEFS66.CON", "GATOR66.CON", "GAME66.CON", "PIG66.CON"]);

    /// <summary>
    /// Is Rides Again installed
    /// </summary>
    public bool IsAgainInstalled => IsInstalled(["REDNECK.GRP", "BIKER.CON"], AgainInstallPath);

    /// <inheritdoc/>
    public override Enum Skills => new RedneckSkillsEnum();
}
