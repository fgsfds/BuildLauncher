using Core.All.Enums;
using Core.Client.Helpers;
using Games.Skills;

namespace Games.Games;

/// <summary>
///     Represents the game Blood and its associated metadata.
/// </summary>
public sealed class BloodGame : BaseGame
{
    /// <summary>
    ///     Files required for Cryptic Passage addon.
    /// </summary>
    private readonly List<string> RequiredCPFiles = [ClientConsts.CrypticIni, "CP01.MAP", "CPART07.AR_", "CPART15.AR_", "CRYPTIC.SMK", "CRYPTIC.WAV"];
    /// <inheritdoc />
    public override GameEnum GameEnum => GameEnum.Blood;

    /// <inheritdoc />
    public override string FullName => "Blood";

    /// <inheritdoc />
    public override string ShortName => FullName;

    /// <inheritdoc />
    public override List<string> RequiredFiles => [ClientConsts.BloodIni, ClientConsts.BloodRff, ClientConsts.BloodSnd, "GUI.RFF", "SURFACE.DAT", "TILES000.ART", "VOXEL.DAT"];

    /// <summary>
    ///     Is Cryptic Passage installed.
    /// </summary>
    public bool IsCrypticPassageInstalled => IsInstalled(RequiredCPFiles);

    /// <inheritdoc />
    public override Enum Skills => new BloodSkillsEnum();
}
