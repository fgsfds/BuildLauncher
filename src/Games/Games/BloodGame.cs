using Common.All.Enums;
using Common.Client.Helpers;
using Games.Skills;

namespace Games.Games;

public sealed class BloodGame : BaseGame
{
    /// <inheritdoc/>
    public override GameEnum GameEnum => GameEnum.Blood;

    /// <inheritdoc/>
    public override string FullName => "Blood";

    /// <inheritdoc/>
    public override string ShortName => FullName;

    /// <inheritdoc/>
    public override List<string> RequiredFiles => [ClientConsts.BloodIni, ClientConsts.BloodRff, ClientConsts.BloodSnd, "GUI.RFF", "SURFACE.DAT", "TILES000.ART", "VOXEL.DAT"];

    /// <summary>
    /// List of files required for Cryptic Passage
    /// </summary>
    private readonly List<string> RequiredCPFiles = [ClientConsts.CrypticIni, "CP01.MAP", "CPART07.AR_", "CPART15.AR_", "CRYPTIC.SMK", "CRYPTIC.WAV"];

    /// <summary>
    /// Is Cryptic Passage instaleld
    /// </summary>
    public bool IsCrypticPassageInstalled => IsInstalled(RequiredCPFiles);

    /// <inheritdoc/>
    public override Enum Skills => new BloodSkillsEnum();
}
